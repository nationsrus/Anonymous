<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AccountDestruction.aspx.cs" Inherits="Anonymous.AccountDestruction" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <script type = "text/javascript">
        function ValidateCheckBox(sender, args) {
            if (document.getElementById("<%=cbxUnderstand.ClientID %>").checked == true) {
                args.IsValid = true;
            } else {
                args.IsValid = false;
            }
        }
        function ValidateCheckBox2(sender, args) {
            if (document.getElementById("<%=cbxUnderstandPrivateMessages.ClientID %>").checked == true) {
                        args.IsValid = true;
                    } else {
                        args.IsValid = false;
                    }
                }
    </script>

    <h1><span onclick="hideShow('divDestroyAccount');" class="hideShowTitle">Destroy My Account</span></h1>
    <div id="divDestroyAccount">

        <h3>Reasons you might want to destroy your account</h3>
        <ul>
            <li>To delete all data which includes private messages and forum posts</li>
            <li>To change email address</li>
            <li>You know there is no going back</li>
            <li>You know that once data is deleted, it can't be retried</li>
        </ul>

        <h3>Account Destruction Options</h3>
        <table>
            <tr>
                <td></td>
                <td>
                    <asp:CheckBox ID="cbxInformFriends" Text="&nbsp;Inform friends" ToolTip="A system message to friends will be sent to inform them that this account was deleted." runat="server" />
                </td>
            </tr>
            <tr>
                <td></td>
                <td>
                    <asp:CheckBox ID="cbxDeletePublicMessages" Text="&nbsp;Delete public messages" ToolTip="All your public messages will be replaced with 'This account has be deleted'" runat="server" />
                </td>
            </tr>
            <tr style="display:none;">
                <td></td>
                <td>
                    <asp:CheckBox ID="cbxKeepSkeletonAccount" Text="&nbsp;Keep account email and password active, but delete all other messages or posts." runat="server" />
                </td>
            </tr>
            <tr>
                <td></td>
                <td>
                    <asp:CheckBox ID="cbxTotalAnnihilation" Text="&nbsp;Totally annihilate this account. Also deletes account itself." ToolTip="You will be logged out and no account on this website will be attached to the email address and password. Logging in with the current email address and password will no longer work." runat="server" />
                </td>
            </tr>
            <tr>
                <td><asp:CustomValidator ID="CustomValidator2" runat="server" ForeColor="Red" ErrorMessage="Required*" ClientValidationFunction="ValidateCheckBox2"></asp:CustomValidator></td>
                <td><asp:CheckBox ID="cbxUnderstandPrivateMessages" Text="&nbsp;I understand I will be unable to retrieve private messages" ToolTip="Friends will retain a copy of the private messages due to the encryption. Deleting their copy is impossible." runat="server" /></td>
            </tr>
            <tr>
                <td><asp:CustomValidator ID="CustomValidator1" runat="server" ForeColor="Red" ErrorMessage="Required*" ClientValidationFunction="ValidateCheckBox"></asp:CustomValidator></td>
                <td><asp:CheckBox ID="cbxUnderstand" Text="&nbsp;I understand all data is permanently altered" runat="server" /></td>
            </tr>
            <tr>
                <td colspan="2" align="center">
                    <asp:Button CssClass="button" ID="btnDestroy" OnClick="btnDestroy_Click" OnClientClick="confirm('Are you sure? I confirm and understand there is no going back after this.');" Text="Submit" runat="server" />
                </td>
            </tr>
        </table>
        
        
    </div>


    <asp:Label ID="lblSystemMsg" runat="server"></asp:Label>

</asp:Content>
