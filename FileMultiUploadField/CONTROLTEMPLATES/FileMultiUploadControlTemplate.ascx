<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Register TagPrefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" %>




<SharePoint:RenderingTemplate ID="FileMultiUploadControlTemplate" runat="server">
    <Template>
    <SharePoint:CssRegistration Name="/_layouts/15/FileMultiUploadField/style.css" runat="server" />

        <asp:Panel ID="pnlUpload" runat="server">
            <asp:FileUpload ID="UploadFileControl" runat="server" CssClass="ms-ButtonHeightWidth" Width="250px" />
            <asp:Button ID="UploadButton" runat="server" CssClass="ms-ButtonHeightWidth" CausesValidation="false" Text="Загрузить" Width="70px" />
        </asp:Panel>
        <asp:Label ID="StatusLabel" runat="server" Width="100%" />
        <asp:HiddenField ID="hdnFileName" runat="server" />

        <asp:GridView ID="grdFiles" Width="100%" CssClass="filemultiuploadcontrol" 
            PagerStyle-CssClass="pager" HeaderStyle-CssClass="header" RowStyle-CssClass="rows"
            runat="server" 
            AutoGenerateColumns="False" AllowPaging="false" AllowSorting="false" ShowHeader="false" ShowFooter="false">
            <Columns>
                <asp:TemplateField HeaderText="Файл" ItemStyle-CssClass="ms-formlabel">
                    <ItemTemplate>
                        <asp:HyperLink ID="HyperLink1" runat="server" Target="_blank" 
                            Text='<%# DataBinder.Eval (Container.DataItem, "Name") %>'
                            NavigateUrl='<%# DataBinder.Eval (Container.DataItem, "Url") %>' ></asp:HyperLink>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="" ItemStyle-CssClass="ms-formbody">
                    <ItemTemplate>
                        <asp:LinkButton runat="server" ID="lnkDelete" CommandName="deletefile" CommandArgument='<%# DataBinder.Eval (Container.DataItem, "Name") %>'  CssClass="delete_icon" ></asp:LinkButton>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <EmptyDataTemplate>
                <b class="ms-formlabel">Нет файлов</b>
            </EmptyDataTemplate>
        </asp:GridView>


    </Template>
</SharePoint:RenderingTemplate>


