<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="FriendMessages.aspx.cs" Inherits="Anonymous.FriendMessages" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h1>Private messages with <asp:Literal ID="ltlAnonFriend" runat="server"></asp:Literal></h1>

    <asp:Repeater ID="rptrMessages" OnItemDataBound="rptrMessages_ItemDataBound" runat="server">
        <HeaderTemplate>
            <table>
                <tr>
                    <td class="defaultTableHeader">
                        
                    </td>
                    <td class="defaultTableHeader">
                        Msg
                    </td>
                    <td class="defaultTableHeader">TimeStamp</td>
                </tr>
        </HeaderTemplate>
        <ItemTemplate>
            <tr style="border-top:solid gray 1px;">
                <td class="defaultTableCell" width="115px" style="text-align:right;">
                    <asp:Label ID="lblWho" runat="server"></asp:Label>
                </td>
                <td class="defaultTableCell">
                    <div id="divMsg" runat="server" onclick="hideMinimalShow(this.id);" style="cursor:n-resize; overflow: hidden;">
                        <%# DataBinder.Eval(Container.DataItem, "decryptedMsg") %>
                    </div>
                </td>
                <td class="defaultTableCell" width="175px">
                    <%# DataBinder.Eval(Container.DataItem, "decryptedInsertTimeStamp") %>
                </td>
            </tr>
        </ItemTemplate>
        <FooterTemplate>
            </table>
        </FooterTemplate>
    </asp:Repeater>

    <h1>Send A Private Message</h1>

    <div id="divMessageNew">
        <table>
            <tr>
                <td class="defaultTableHeader">Message</td>
                <td class="defaultTableCell">
                    <asp:TextBox CssClass="textbox" ID="tbxMsg" TextMode="MultiLine" Rows="10" Width="500px" runat="server"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td colspan="2" class="defaultTableCell" align="center">
                    <asp:Button CssClass="button" ID="btnMessageSend" OnClick="btnMessageSend_Click" runat="server" Text="Send" />
                </td>
            </tr>
        </table>
    </div>
</asp:Content>
