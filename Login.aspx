<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Anonymous.Login" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <ul>
        <li>This login is seperate application from https://nationsrus.com but made by the same developer</li> 
        <li>There is no plan currently to provide single signon from nationsrus.com to <asp:Literal id="ltlWebsiteName" runat="server"></asp:Literal></li>
        <li>There is NO "forgot password" feature as due to the encryption. Keep your password in a safe place such as a password manager.</li>
    </ul>
    
    <asp:Panel ID="pnlTest" runat="server" Visible="false">
        <a href="GoogleLogin">
            <div class="col s12 m6 offset-m3 center-align">
                <img src="image/GoogleSignUpDark.jpg" width="15%" height="15%" />
            </div>
        </a>


        <br />

        Create account (input Google User Id):
        <asp:TextBox ID="tbxUserId" runat="server"></asp:TextBox>
        <asp:Button ID="btnSubmit" runat="server" Text="Create" OnClick="btnSubmitOnClick" />


        <br /><br />
        Account login (input Google User Id):
        <asp:TextBox ID="tbxLoginAccount" runat="server"></asp:TextBox>
        <asp:Button ID="btnLogin" runat="server" Text="Login" OnClick="btnLoginOnClick" />

        <br /><br />
        Account RSA Encryption Login:
        email address: <asp:TextBox ID="tbxUsername" runat="server"></asp:TextBox><br />
        password: <asp:TextBox ID="tbxPassword" runat="server"></asp:TextBox><br />
        <asp:Button ID="btnRsaLogin" OnClick="btnRsaLoginOnClick" runat="server" Text="Login" /><br />
        Msg: <asp:TextBox ID="tbxMsg" runat="server"></asp:TextBox>
    </asp:Panel>

    <table align="center">
        <tr>
            <td class="defaultTableHeader">Email Address: </td>
            <td class="defaultTableCell"><asp:TextBox CssClass="textbox" ID="tbxEmail" runat="server"></asp:TextBox></td>
        </tr>
        <tr>
            <td class="defaultTableHeader">Password: </td>
            <td class="defaultTableCell"><asp:TextBox CssClass="textbox" ID="tbxPswd" runat="server" TextMode="Password"></asp:TextBox></td>
        </tr>
        <tr>
            <td colspan="2" align="center"><asp:Button ID="btnSecureLogin" CssClass="button" OnClick="btnSecureLoginOnClick" runat="server" Text="Login" /></td>
        </tr>
        <tr>
            <td colspan="2" align="center">
                <br />
                <a href="Register">Register Here</a>
            </td>
        </tr>
    </table>
    <asp:Label runat="server" ID="lblError" ForeColor="Red"></asp:Label>


    <br /><br />
    
</asp:Content>
