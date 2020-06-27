<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="FriendInvite.aspx.cs" Inherits="Anonymous.FriendInvite" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <script>
        function onSubmit(token) {
            document.getElementById("ctl01").submit();
            document.getElementById("<%= btnSendInvite.ClientID %>").click();
        }
    </script>

    <h1><span onclick="hideShow('divInviteToAnon');" class="hideShowTitle">Invite Friend To Anonymous.</span></h1>

    <div id="divInviteToAnon">
        Disclaimer: Due to security and privacy of everyone's email address, no success or failure message will display upon successful and failure attempts. When this friend uses the invite to register, your anon-# will be shared with them. Invites will automatically expire after 60 days.
        <table>
            <tr>
                <td class="defaultTableHeader">
                    Friend's email address:
                </td>
                <td class="defaultTableCell">
                    <asp:TextBox ID="tbxFriendEmail" CssClass="textbox" runat="server"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td class="defaultTableHeader">
                    Your Name/Nickname:
                </td>
                <td class="defaultTableCell">
                    <asp:TextBox ID="tbxNickname" CssClass="textbox" runat="server"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td class="defaultTableHeader">
                    Your email address (optional):
                </td>
                <td class="defaultTableCell">
                    <asp:TextBox ID="tbxInviteShareEmailAddress" CssClass="textbox" runat="server"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td class="defaultTableHeader">
                    Custom message:
                </td>
                <td class="defaultTableCell">
                    <asp:TextBox ID="tbxCustomMessage" CssClass="textbox" TextMode="MultiLine" Rows="4" Width="400px" runat="server"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td colspan="2" class="defaultTableFooterSubmit">
                    <button class="g-recaptcha button" 
                        data-sitekey="6Ldu6qkZAAAAAIPQ-aaOL62X4m_not9wmbSM_cUI" 
                        data-callback='onSubmit' 
                        data-action='submit' runat="server" id="btnRecaptchaSubmit" >Submit</button>
                    <asp:Button ID="btnSendInvite" OnClick="btnSendInvite_Click" CssClass="hidden" Text="Send" runat="server" />
                </td>
            </tr>
        </table>
        <asp:Label ID="lblInviteError" runat="server" Visible="false"></asp:Label>
    </div>

</asp:Content>
