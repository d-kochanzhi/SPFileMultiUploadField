using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.SharePoint.Utilities;
using System.Configuration;
using System.IO;
using FileMultiUploadField.Core;

namespace FileMultiUploadField
{
    class FieldMultiUploadFieldControl : Microsoft.SharePoint.WebControls.BaseFieldControl
    {
        protected FileUpload UploadFileControl;
        protected Button UploadButton;
        protected Label StatusLabel;
        protected HiddenField hdnFileName;
        protected Panel pnlUpload;
        protected GridView grdFiles;

       

        public override void Focus()
        {
            if (Field == null || this.ControlMode == SPControlMode.Display)
            { return; }
            EnsureChildControls();
            UploadFileControl.Focus();
        }

        public override object Value
        {
            get
            {
                EnsureChildControls();
                string _value = string.Empty;
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    using (SPSite site = new SPSite(SPContext.Current.Web.Site.ID))
                    {
                        using (SPWeb web = site.OpenWeb(SPContext.Current.Web.ID))
                        {
                            web.AllowUnsafeUpdates = true;
                            _value = GetDestFolder(web).ServerRelativeUrl;
                        }
                    }
                });
                return _value;
               
            }
            set
            {
                EnsureChildControls();
                hdnFileName.Value = (string)this.ItemFieldValue;
                StatusLabel.Text = "Папка: <a href='" + (string)this.ItemFieldValue + "' target='_blank'>Просмотр (" + (string)this.ItemFieldValue + ")</a>";
            }
        }

        public override void Validate()
        {
            if (ControlMode == SPControlMode.Display || !IsValid)
            {
                return;
            }

            base.Validate();

            if (Field.Required &&
                (Value == null || Value.ToString().Length == 0))
            {
                this.ErrorMessage = "Необходимо задать значение для этого обязательного поля";
                IsValid = false;
                return;
            }
        }

        protected override void RenderFieldForDisplay(HtmlTextWriter output)
        {
            SPFileCollection files = GetFolderFiles();
            if (files != null && files.Count > 0)
            {
                files.Cast<SPFile>().Select(x => new SPFileFeilds(x)).OrderBy(x=>x.Name).ToList().ForEach((file)=> {
                    HyperLink link = new HyperLink();
                    link.Text = file.Name;
                    link.Target = "_blank";
                    link.NavigateUrl = SPContext.Current.Web.Site.Url + file.Url;
                    link.RenderControl(output);

                    output.WriteLine("<br />");
                });
            }

           
        }

        protected override void CreateChildControls()
        {
            if (Field == null || this.ControlMode == SPControlMode.Display)
            { return; }

            base.CreateChildControls();

            try
            {
                UploadFileControl = (FileUpload)TemplateContainer.FindControl("UploadFileControl");
                UploadButton = (Button)TemplateContainer.FindControl("UploadButton");
                StatusLabel = (Label)TemplateContainer.FindControl("StatusLabel");
                hdnFileName = (HiddenField)TemplateContainer.FindControl("hdnFileName");
                pnlUpload = (Panel)TemplateContainer.FindControl("pnlUpload");

                LoadFilesToGrid();


                UploadButton.Click += new EventHandler(UploadButton_Click);
                grdFiles.RowCommand += grdFiles_RowCommand;
                if (hdnFileName.Value != string.Empty)                  
                    StatusLabel.Text = "Папка: <a href='" + hdnFileName.ToString() + "' target='_blank'>Просмотр (" + hdnFileName.ToString() + ")</a>";
             

                FileMultiUploadField _field = (FileMultiUploadField)this.Field;
                if (_field.UseIDasFolder && this.ControlMode == SPControlMode.New)
                {
                    pnlUpload.Visible = false;
                    grdFiles.Visible = false;
                    StatusLabel.Text = "Для загрузки файлов сохраните элемент.";
                } else if (this.ControlMode == SPControlMode.Display) {
                    pnlUpload.Visible = false;
                }

                /*
                 Стилизация контролов
                 первый класс по умолчанию, следующий класс экземпляра (для кастомизации)
                 */
                pnlUpload.CssClass = string.Join(" ", "UploadContainer", String.Concat( this.Field.TypeAsString, "_", this.Field.InternalName));
                grdFiles.CssClass = string.Join(" ", "FilesContainer", String.Concat(this.Field.TypeAsString, "_", this.Field.InternalName));
                StatusLabel.CssClass = string.Join(" ", "StatusContainer", String.Concat(this.Field.TypeAsString, "_", this.Field.InternalName));

               

            }
            catch (Exception ex)
            {
                Controls.Add(new Label() { Text = ex.ToString() });
            }
        }


        private void LoadFilesToGrid()
        {
            grdFiles = (GridView)TemplateContainer.FindControl("grdFiles");
            if (grdFiles!= null)
            {
                SPFileCollection files = GetFolderFiles();
                grdFiles.DataSource = (files != null && files.Count > 0) ? GetFolderFiles().Cast<SPFile>().Select(x => new SPFileFeilds(x)).ToList() : null;
                grdFiles.DataBind();
            }
        }

        void grdFiles_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "deletefile")
            {
                FileMultiUploadField _field = (FileMultiUploadField)this.Field;
                SPSite sourceSite = SPControl.GetContextSite(Context);
                SPWeb sourceWeb = SPControl.GetContextWeb(Context);
                try
                {

                    if (_field.UseElevatedPrivileges)
                    {
                        SPSecurity.RunWithElevatedPrivileges(delegate()
                        {
                            using (SPSite site = new SPSite(sourceSite.ID))
                            {
                                using (SPWeb web = site.OpenWeb(sourceWeb.ID))
                                {
                                    web.AllowUnsafeUpdates = true;
                                    DeleteJob(web, Helper.NullToStr(e.CommandArgument));
                                }
                            }
                        });

                    }
                    else
                    {
                        using (SPWeb web = sourceSite.OpenWeb(sourceWeb.ID))
                        {
                            web.AllowUnsafeUpdates = true;
                            DeleteJob(web, Helper.NullToStr(e.CommandArgument));
                        }
                    }

                }
                catch (Exception ex)
                {
                    StatusLabel.Text = "Удаление файла :: Ошибка " + ex.Message;
                }

                LoadFilesToGrid();
            }

        }

        private void DeleteJob(SPWeb web, string filename)
        {
            SPFolder destFolder = GetDestFolder(web);
            SPFile file = destFolder.Files.Cast<SPFile>().Where(f => f.Name == filename).FirstOrDefault();
            if (file != null) { file.Delete(); destFolder.Update(); }

        }

        protected void UploadButton_Click(object sender, EventArgs e)
        {
            if (UploadFileControl.PostedFile == null) return;

            FileMultiUploadField _field = (FileMultiUploadField)this.Field;
            SPSite sourceSite = SPControl.GetContextSite(Context);
            SPWeb sourceWeb = SPControl.GetContextWeb(Context);
            try
            {

                if (_field.UseElevatedPrivileges)
                {
                    SPSecurity.RunWithElevatedPrivileges(delegate()
                    {
                        using (SPSite site = new SPSite(sourceSite.ID))
                        {
                            using (SPWeb web = site.OpenWeb(sourceWeb.ID))
                            {
                                web.AllowUnsafeUpdates = true;
                                UploadFileJob(web);
                            }
                        }
                    });

                }
                else
                {
                    using (SPWeb web = sourceSite.OpenWeb(sourceWeb.ID))
                    {
                        web.AllowUnsafeUpdates = true;
                        UploadFileJob(web);
                    }
                }

            }
            catch (Exception ex)
            {
                StatusLabel.Text = "Загрузка файла :: Ошибка " + ex.Message;
            }

            LoadFilesToGrid();
        }

        private void UploadFileJob(SPWeb web)
        {
            SPFolder destFolder = GetDestFolder(web);

            string strFileName = UploadFileControl.PostedFile.FileName.Substring(UploadFileControl.PostedFile.FileName.LastIndexOf("\\") + 1);
            if (!string.IsNullOrEmpty(strFileName))
            {
                Stream fStream = UploadFileControl.PostedFile.InputStream;
                SPFile objFile = destFolder.Files.Add(strFileName, fStream, true);
                objFile.Item.UpdateOverwriteVersion();
                StatusLabel.Text = "Загрузка файла :: Успешно ";
                hdnFileName.Value = objFile.ParentFolder.ServerRelativeUrl;
            }
        }

        private SPFileCollection GetFolderFiles()
        {

            SPFileCollection returnvalue = null;
            FileMultiUploadField _field = (FileMultiUploadField)this.Field;
            SPSite sourceSite = SPControl.GetContextSite(Context);
            SPWeb sourceWeb = SPControl.GetContextWeb(Context);
            try
            {
                if (_field.UseElevatedPrivileges)
                {
                    SPSecurity.RunWithElevatedPrivileges(delegate()
                    {
                        using (SPSite site = new SPSite(sourceSite.ID))
                        {
                            using (SPWeb web = site.OpenWeb(sourceWeb.ID))
                            {
                                web.AllowUnsafeUpdates = true;
                                returnvalue = GetFolderFilesJob(web);
                            }
                        }
                    });

                }
                else
                {
                    using (SPWeb web = sourceSite.OpenWeb(sourceWeb.ID))
                    {
                        web.AllowUnsafeUpdates = true;
                        returnvalue = GetFolderFilesJob(web);
                    }
                }

            }
            catch (Exception ex)
            {
                StatusLabel.Text = "Чтение файлов :: Ошибка " + ex.Message;
            }

            return returnvalue;
        }

        private SPFileCollection GetFolderFilesJob(SPWeb web)
        {

            SPFolder destFolder = GetDestFolder(web);
            return destFolder.Files;
        }

        private SPFolder GetDestFolder(SPWeb web)
        {                      

            FileMultiUploadField _field = (FileMultiUploadField)this.Field;
            SPList objList = web.Lists.GetList(Guid.Parse(_field.UploadDocumentLibrary), false);

            SPFolder destFolder = objList.RootFolder;
            if (_field.UseIDasFolder) destFolder = destFolder.SubFolders.Add(this.ListItem.ID.ToString());        

            return destFolder;
        }

        protected override string DefaultTemplateName
        {
            get
            {
                return "FileMultiUploadControlTemplate";
            }
        }
    }

    public class SPFileFeilds
    {
        public string Name { get; set; }
        public string Url { get; set; }

        public SPFileFeilds(SPFile file)
        {
            if (file != null)
            {
                this.Name = file.Name;
                this.Url = file.ServerRelativeUrl;
            }
        }
    }
}
