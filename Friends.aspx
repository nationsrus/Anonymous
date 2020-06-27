<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Friends.aspx.cs" Inherits="Anonymous.Friends" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <asp:Label ID="lblSystemMsg" runat="server"></asp:Label>

    <h1><span onclick="hideShow('divRecentFriendActivity');" class="hideShowTitle">Recent Activity</span></h1>

    <div id="divRecentFriendActivity">
        <asp:Repeater ID="rptrRecentFriendActivty" runat="server">
            <HeaderTemplate>
                <table>
                    <tr>
                        <td class="defaultTableHeader">AnonId</td>
                        <td class="defaultTableHeader">Post</td>
                        <td class="defaultTableHeader">Friend Activity</td>
                        <td class="defaultTableHeader">Timestamp</td>
                    </tr>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td class="defaultTableCell"><%# DataBinder.Eval(Container.DataItem, "AnonId") %></td>
                    <td class="defaultTableCell"><a href="Post?postId=<%# DataBinder.Eval(Container.DataItem, "postId") %>"><%# DataBinder.Eval(Container.DataItem, "post150character") %></a></td>
                    <td class="defaultTableCell"><%# DataBinder.Eval(Container.DataItem, "friendActivity") %></td>
                    <td class="defaultTableCell PostTimeStamp" style="font-size:10px;"><%# DataBinder.Eval(Container.DataItem, "activityTimeStamp") %></td>
                </tr>
            </ItemTemplate>
            <FooterTemplate>
                </table>
            </FooterTemplate>
        </asp:Repeater>
        <asp:Label ID="lblNoResults" runat="server">No results</asp:Label>
    </div>

    <h1><span onclick="hideShow('divInviteToAnon');" class="hideShowTitle">Invite Friend To Anonymous.</span></h1>

    <div id="divInviteToAnon">
        <a href="FriendInvite">Invite friend...</a>
    </div>

    <h1><span onclick="hideShow('divFriendshipRequests');" class="hideShowTitle">Friendship Requests</span></h1>

    <div id="divFriendshipRequests">
        <asp:Repeater ID="rptrFriendshipRequests" OnItemDataBound="rptrFriendshipRequestsOnItemDataBound" runat="server">
            <HeaderTemplate>
                <table id="tRptrFriendshipRequests">
                    <tr class="defaultTableHeader">
                        <td class="defaultTableCell">
                            AnonId
                        </td>
                        <td class="defaultTableCell">
                            Message
                        </td>
                        <td class="defaultTableCell">
                            Accept/Reject
                        </td>
                    </tr>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td class="defaultTableCell">
                        <asp:Label ID="lblFriendAnonId" runat="server"></asp:Label>
                    </td>
                    <td class="defaultTableCellAlignLeft" style="text-align:left;">
                        <asp:Label ID="lblMsg" runat="server"></asp:Label>
                    </td>
                    <td class="defaultTableCell">
                        <asp:HiddenField ID="hfFriendRequestId" runat="server" />
                        <asp:HiddenField ID="hfFriendEmail" runat="server" />
                        <asp:HiddenField ID="hfEncryptedRequesterEmail" runat="server" />
                        <asp:HiddenField ID="hfFriendAnonId" runat="server" />
                        <asp:Button ID="btnFriendAccept" runat="server" Text="Accept" OnClick="btnFriendAcceptOnClick" />
                        <asp:Button ID="btnFriendReject" runat="server" Text="Reject" OnClick="btnFriendRejectOnClick" />
                    </td>
                </tr>
            </ItemTemplate>
            <FooterTemplate>
                </table>
            </FooterTemplate>
        </asp:Repeater>
        <asp:Label ID="lblNoPendingFriendshipRequests" runat="server">No pending friendship requests</asp:Label>
    </div>

    <h1><span onclick="hideShow('divFriends');" class="hideShowTitle">Friends List</span></h1>

    <div id="divFriends">
        <asp:Repeater ID="rptrFriends" OnItemDataBound="rptrFriendsOnItemDataBound" runat="server">
            <HeaderTemplate>
                <table>
                    <tr class="defaultTableHeader">
                        <td class="defaultTableHeader">AnonId</td>
                        <td class="defaultTableHeader">NickName</td>
                        <td class="defaultTableHeader">Email</td>
                        <td class="defaultTableHeader">Public Name/NickName</td>
                        <td class="defaultTableHeader">Public City</td>
                        <td class="defaultTableHeader">Public Nation</td>
                        <td class="defaultTableHeader">&nbsp;</td>
                    </tr>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td class="defaultTableCell">
                        <asp:Label ID="lblAnonId" runat="server"></asp:Label>
                    </td>
                    <td class="defaultTableCell">
                        <asp:Label ID="lblNickName" runat="server"></asp:Label>
                    </td>
                    <td class="defaultTableCell">
                        <asp:Label ID="lblEmail" runat="server"></asp:Label>
                    </td>
                    <td class="defaultTableCell">
                        <asp:Label ID="lblPublicNickName" runat="server"></asp:Label>
                    </td>
                    <td class="defaultTableCell">
                        <asp:Label ID="lblPublicCity" runat="server"></asp:Label>
                    </td>
                    <td class="defaultTableCell">
                        <asp:Label ID="lblPublicNation" runat="server"></asp:Label>
                    </td>
                    <td class="defaultTableCell">
                        <asp:HiddenField ID="hfFriendId" runat="server" />
                        <asp:Button ID="btnFriendEdit" CssClass="button" OnClick="btnFriendEdit_Click" Text="Edit" runat="server" />
                        <asp:Button ID="btnFriendMessage" CssClass="button" OnClick="btnFriendMessageOnClick" Text="Message" runat="server" />
                    </td>
                </tr>
            </ItemTemplate>
            <FooterTemplate>
                </table>
            </FooterTemplate>
        </asp:Repeater>
    </div>

    <h1><span onclick="hideShow('divFriendAdd');" class="hideShowTitle">Add Friend</span></h1>
    <div id="divFriendAdd">
        <table>
            <tr>
                <td class="defaultTableHeader">
                    AnonId:
                </td>
                <td>
                    <asp:TextBox ID="tbxAnonId" CssClass="textbox" runat="server"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td class="defaultTableHeader">
                    Email Address: (Only if friend requires this)
                </td>
                <td>
                    <asp:TextBox ID="tbxEmail" CssClass="textbox" runat="server"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td class="defaultTableHeader">
                    Message (optional):
                </td>
                <td>
                    <asp:TextBox CssClass="textbox" TextMode="MultiLine" Rows="5" Width="800px" MaxLength="600" runat="server" ID="tbxMsg"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td colspan="2" class="defaultTableCell" style="text-align:center;"><asp:CheckBox ID="cbxShareEmail" Text="&nbsp;Share my email address with friend" runat="server" /></td>
            </tr>
            <tr>
                <td colspan="2" style="text-align:center;">
                    <asp:Button ID="btnSubmit" CssClass="button" runat="server" Text="Add" OnClick="btnSubmitOnClick" />
                </td>
            </tr>
        </table>
        <asp:Label ID="lblFriendAddError" runat="server" ForeColor="Red" Visible="false"></asp:Label>
        <asp:Label ID="lblFriendRequestSent" runat="server" Visible="false">Friendship request sent. Due to privacy and data encryption, no pending record will displayed here. If your friend accepts this invite, you'll see them on your friends list afterwards.</asp:Label>
     </div>

    <h1><span onclick="hideShow('divFriendSearch');" class="hideShowTitle">Friend Search</span></h1>
    <div id="divFriendSearch">
        <table>
            <tr>
                <td class="defaultTableHeader">Name/Nickname:</td>
                <td><asp:TextBox ID="tbxName" CssClass="textbox" runat="server" MaxLength="256"></asp:TextBox></td>
            </tr>
            <tr>
                <td class="defaultTableHeader">City:</td>
                <td><asp:TextBox ID="tbxCity" CssClass="textbox" runat="server" MaxLength="256"></asp:TextBox></td>
            </tr>
            <tr>
                <td class="defaultTableHeader">Nation:</td>
                <td><asp:TextBox ID="tbxNation" CssClass="textbox" runat="server" MaxLength="256"></asp:TextBox></td>
            </tr>
            <tr>
                <td colspan="2" style="text-align:center;"><asp:Button ID="btnSearch" CssClass="button" OnClick="btnSearchOnClick" runat="server" Text="Search" /></td>
            </tr>
        </table>
        <br />
        <asp:Repeater ID="rptrSearch" runat="server">
            <HeaderTemplate>
                <table>
                    <tr class="defaultTableHeader">
                        <td style="display:none;">Add</td>
                        <td class="defaultTableHeader">AnonId</td>
                        <td class="defaultTableHeader">Name/NickName</td>
                        <td class="defaultTableHeader">City</td>
                        <td class="defaultTableHeader">Nation</td>
                    </tr>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td style="display:none;" class="defaultTableCell">
                        <asp:HiddenField ID="hfFriendAnonId" Value='<%# DataBinder.Eval(Container.DataItem, "AnonId") %>' runat="server" />
                        <asp:Button ID="btnSearchFriendAdd" CssClass="button" OnClick="btnSearchFriendAddOnClick" Text="Request Friendship" runat="server" />
                    </td>
                    <td class="defaultTableCell"><%# DataBinder.Eval(Container.DataItem, "AnonId") %></td>
                    <td class="defaultTableCell"><%# DataBinder.Eval(Container.DataItem, "PublicName") %></td>
                    <td class="defaultTableCell"><%# DataBinder.Eval(Container.DataItem, "PublicCity") %></td>
                    <td class="defaultTableCell"><%# DataBinder.Eval(Container.DataItem, "PublicNation") %></td>
                </tr>
            </ItemTemplate>
            <FooterTemplate>
                </table>
            </FooterTemplate>
        </asp:Repeater>

        <asp:Label ID="lblSearchResults" runat="server"></asp:Label>
    </div>
</asp:Content>
