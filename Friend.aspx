<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Friend.aspx.cs" Inherits="Anonymous.Friend" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Anon-<asp:Literal ID="ltlFriendAnonId" runat="server"></asp:Literal></h1>

    <asp:Label ID="lblSystemMsg" ForeColor="Red" Visible="false" runat="server"></asp:Label>

    <table>
        <tr>
            <td class="defaultTableHeader">Name/Nickname:</td>
            <td><asp:TextBox CssClass="textbox" ID="tbxNickname" runat="server"></asp:TextBox></td>
        </tr>
        <tr>
            <td class="defaultTableHeader">Email:</td>
            <td><asp:TextBox CssClass="textbox" ID="tbxEmail" runat="server"></asp:TextBox></td>
        </tr>
        <tr>
            <td colspan="2" align="center">
                <asp:Button CssClass="button" ID="btnUpdate" Text="Update" runat="server" OnClick="btnUpdate_Click" />
            </td>
        </tr>
    </table>
</asp:Content>
