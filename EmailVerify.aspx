<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="EmailVerify.aspx.cs" Inherits="Anonymous.EmailVerify" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <asp:Label ID="lblSystemMsg" ForeColor="Red" runat="server"></asp:Label>
    <br />

    <asp:Panel ID="pnlVerify" runat="server">
        <h1><span onclick="hideShow('divEmailVerify');" class="hideShowTitle">Email Verify</span></h1>

        <div id="divEmailVerify">
            <table>
                <tr>
                    <td class="defaultTableHeader">
                        Verification Code:
                    </td>
                    <td class="defaultTableCell">
                        <asp:TextBox CssClass="textbox" ID="tbxCode" runat="server"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td colspan="2" align="center">
                        <asp:Button CssClass="button" ID="btnSubmit" runat="server" OnClick="btnSubmit_Click" Text="Submit" />
                    </td>
                </tr>
            </table>
        </div>
    </asp:Panel>

    <asp:Panel ID="pnlVerified" Visible="false" runat="server">
        <h1><span onclick="hideShow('divEmailVerified');" class="hideShowTitle">Email Verification Success</span></h1>

        <div id="divEmailVerified">
            <h2>Email verification success.</h2>
        </div>
    </asp:Panel>

</asp:Content>
