<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Messages.aspx.cs" Inherits="Anonymous.Messages" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h3 style="color:red;">Private Message Disclaimer</h3>
    Due to the RSA type encryption on private messaging, upon sending a message your friend will retrain a independent copy of the message. Deleting your account cannot have the option to delete the copy of those messages in the friends' accounts. You can only delete your copy.<br />

    <h1><span onclick="hideShow('divMessages');" class="hideShowTitle">Private Messages</span></h1>

    <div id="divMessages">
        <asp:Label ID="lblNoMessages" runat="server" Visible="false">No Messages</asp:Label>
        <asp:Repeater ID="rptrMessages" OnItemDataBound="rptrMessages_ItemDataBound" runat="server">
            <HeaderTemplate>
                <table>
                    <tr>
                        <td class="defaultTableHeader">Anon</td>
                        <td class="defaultTableHeader">New</td>
                        <td class="defaultTableHeader">Total</td>
                        <td class="defaultTableHeader">&nbsp;</td>
                    </tr>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td class="defaultTableCell">
                        <asp:Literal ID="ltlAnonFriend" runat="server"></asp:Literal>
                        <asp:HiddenField ID="hfDecryptedFriendAnonId" runat="server" />
                    </td>
                    <td class="defaultTableCell" align="center">
                        <!--CountOfNewMessages-->
                        <asp:Label ID="lblCountOfNewMessages" runat="server"></asp:Label>
                    </td>
                    <td class="defaultTableCell" align="center">
                        <%# DataBinder.Eval(Container.DataItem, "CountOfMessages") %>
                    </td>
                    <td class="defaultTableCell">
                        <asp:Button ID="btnMessageView" CssClass="button" runat="server" Text="View" OnClick="btnMessageView_Click" />
                    </td>
                </tr>
            </ItemTemplate>
            <FooterTemplate>
                </table>
            </FooterTemplate>
        </asp:Repeater>

    </div>

    <h1><span onclick="hideShow('divMessageNew');" class="hideShowTitle">Send A Private Message</span></h1>

    <div id="divMessageNew">
        <table>
            <tr>
                <td class="defaultTableHeader">To:</td>
                <td class="defaultTableCell">
                    <asp:DropDownList ID="ddlFriends" runat="server"></asp:DropDownList>
                </td>
            </tr>
            <tr>
                <td class="defaultTableHeader">Message</td>
                <td class="defaultTableCell">
                    <asp:TextBox ID="tbxMsg" CssClass="textbox" TextMode="MultiLine" Rows="10" Width="500px" runat="server"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td colspan="2" class="defaultTableCell" align="center">
                    <asp:Button ID="btnMessageSend" CssClass="button" OnClick="btnMessageSendOnClick" runat="server" Text="Send" />
                </td>
            </tr>
        </table>
    </div>
</asp:Content>
