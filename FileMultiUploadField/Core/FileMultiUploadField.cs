using FileMultiUploadField.Core;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FileMultiUploadField
{
    public class FileMultiUploadField : SPFieldText
    {

        public FileMultiUploadField(SPFieldCollection fields, string fieldName) : base(fields, fieldName) { Init(); }

        public FileMultiUploadField(SPFieldCollection fields, string typeName, string displayName) : base(fields, typeName, displayName) { Init(); }


        #region "PROPERTIES"

        private string _UploadDocumentLibrary = string.Empty;
        public string UploadDocumentLibrary
        {
            get
            {
                return _UploadDocumentLibrary;
            }
            set
            {
                this.SetCustomProperty("UploadDocumentLibrary", value);
                _UploadDocumentLibrary = value;
            }
        }

        private bool _UseIDasFolder = false;
        public bool UseIDasFolder
        {
            get
            {
                return _UseIDasFolder;
            }
            set
            {
                this.SetCustomProperty("UseIDasFolder", value);
                _UseIDasFolder = value;
            }
        }

        private bool _UseElevatedPrivileges = false;
        public bool UseElevatedPrivileges
        {
            get
            {
                return _UseElevatedPrivileges;
            }
            set
            {
                this.SetCustomProperty("UseElevatedPrivileges", value);
                _UseElevatedPrivileges = value;
            }
        }




        #endregion

        private void Init()
        {
            this.UploadDocumentLibrary = Helper.NullToStr(this.GetCustomProperty("UploadDocumentLibrary"));
            this.UseIDasFolder = Helper.NullToBool(this.GetCustomProperty("UseIDasFolder"));
            this.UseElevatedPrivileges = Helper.NullToBool(this.GetCustomProperty("UseElevatedPrivileges"));

        }
        public override void Update()
        {
            this.SetCustomProperty("UploadDocumentLibrary", this.UploadDocumentLibrary);
            this.SetCustomProperty("UseIDasFolder", this.UseIDasFolder);
            this.SetCustomProperty("UseElevatedPrivileges", this.UseElevatedPrivileges);
            base.Update();
        }

        public override BaseFieldControl FieldRenderingControl
        {
            get
            {
                BaseFieldControl fieldControl = new FieldMultiUploadFieldControl();
                fieldControl.FieldName = InternalName;
                return fieldControl;
            }
        }

       
    }
}
