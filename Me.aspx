<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Me.aspx.cs" Inherits="Anonymous.Me" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">


    <h1>Me and my settings</h1>
    Me: Anon-<asp:Literal ID="ltlAnonId" runat="server"></asp:Literal><br />
    Account Email: <asp:Label ID="lblEmail" runat="server"></asp:Label> <asp:Label ID="lblNotVerified" ForeColor="Red" Visible="false" runat="server"><a href="EmailVerify">[Not Verified]</a></asp:Label><asp:Label ID="lblVerified" ForeColor="Green" Visible="false" runat="server">[Verified]</asp:Label>

    <br /><br />
    <table>
        <tr>
            <td colspan="2"><h3>Public Information (Optional)</h3></td>
        </tr>
        <tr>
            <td>Name/NickName:</td>
            <td><asp:TextBox CssClass="textbox" ID="tbxPublicName" runat="server" MaxLength="256"></asp:TextBox></td>
        </tr>
        <tr>
            <td>City:</td>
            <td><asp:TextBox CssClass="textbox" ID="tbxPublicCity" runat="server" MaxLength="256"></asp:TextBox></td>
        </tr>
        <tr>
            <td>Nation:</td>
            <td><asp:TextBox CssClass="textbox" ID="tbxPublicNation" runat="server" MaxLength="256"></asp:TextBox></td>
        </tr>
        <tr>
            <td colspan="2"><asp:CheckBox ID="cbxFriendRequestsRequireEmailAddress" Text="&nbsp;Only allow friend requests from those that know my email address too" runat="server" /></td>
        </tr>
        <tr>
            <td colspan="2" align="center">
                <asp:Button ID="btnSave" CssClass="button" runat="server" Text="Save" OnClick="btnSaveOnClick" />
            </td>
        </tr>
    </table>
    <asp:Label ID="lblPublicInfoSave" runat="server"></asp:Label>

    <h1><span onclick="hideShow('divMyActivity');" class="hideShowTitle">My Activity</span></h1>

    <div id="divMyActivity">
        <asp:Repeater ID="rptrMyActivity" runat="server">
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
    </div>
    <br />

    <h1><span onclick="hideShow('divUsageSuggestionsGuide');" class="hideShowTitle">Usage/Suggestions Guide</span></h1>

    <div id="divUsageSuggestionsGuide">
        <ul>
            <li>Set a Public Name/Nickname, city and/or nation to help friends identify you.</li>
            <li>If registered with invite code, private message your friend to let them know who you are. Due to system security and privacy your friend should have you listed as a friend, but with no name, nickname and/or email address.</li>
            <li>When posting publicly do not provide personal information.</li>
            <li>Use a password manager to ensure complex unique passwords.</li>
        </ul>
    </div>
    <br />

    <h1><span onclick="hideShow('divChangePassword');" class="hideShowTitle">Change Password</span></h1>
    <div id="divChangePassword" style="display:none;">
         <a href="ChangePassword">Change Password...</a>
    </div>

    <br />

    <h1><span onclick="hideShow('divDestroyAccount');" class="hideShowTitle">Destroy My Account</span></h1>
    <div id="divDestroyAccount" style="display:none;">

        <a href="AccountDestruction">Account Destruction...</a>
        
    </div>
</asp:Content>
