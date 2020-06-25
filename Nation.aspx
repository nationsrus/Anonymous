<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Nation.aspx.cs" Inherits="Anonymous.Nation" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    Change: <asp:DropDownList ID="ddlNation" runat="server"></asp:DropDownList>
    <asp:Button ID="btnNationChange" OnClick="btnNationChangeOnClick" CssClass="button" runat="server" Text="Go" />


    <br />
    <h1><span onclick="hideShow('divTrendingPosts');" class="hideShowTitle">Trending Posts</span></h1>

    <div id="divTrendingPosts">

        <asp:Repeater ID="rptrPosts" OnItemDataBound="rptrPosts_ItemDataBound" runat="server">
            <HeaderTemplate>
                <table>
                    <tr>
                        <td class="defaultTableHeader">Post</td>
                        <td class="defaultTableHeader">Posted Date</td>
                    </tr>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td class="defaultTableCell">
                        <a href="Post?PostId=<%# Eval("PostId") %>" class="PostMsg"><asp:Literal ID="ltlMsgFirstBit" runat="server"></asp:Literal></a>
                    </td>
                    <td class="defaultTableCell PostTimeStamp" style="width:170px; display:block;">
                        <%# Eval("insertTimeStamp") %>
                    </td>
                </tr>
            </ItemTemplate>
            <FooterTemplate>
                </table>
            </FooterTemplate>
        </asp:Repeater>
    </div>

    <br />
    <h1><span onclick="hideShow('divRecentPosts');" class="hideShowTitle">Most Recent Posts</span></h1>

    <div id="divTrendindivRecentPostsgPosts">

        <asp:Repeater ID="rptrRecentPosts" OnItemDataBound="rptrPosts_ItemDataBound" runat="server">
            <HeaderTemplate>
                <table>
                    <tr>
                        <td class="defaultTableHeader">Post</td>
                        <td class="defaultTableHeader">Posted Date</td>
                    </tr>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td class="defaultTableCell">
                        <a href="Post?PostId=<%# Eval("PostId") %>" class="PostMsg"><asp:Literal ID="ltlMsgFirstBit" runat="server"></asp:Literal></a>
                    </td>
                    <td class="defaultTableCell PostTimeStamp" style="width:170px; display:block;">
                        <%# Eval("insertTimeStamp") %>
                    </td>
                </tr>
            </ItemTemplate>
            <FooterTemplate>
                </table>
            </FooterTemplate>
        </asp:Repeater>
    </div>


    <br />
    <h1><span onclick="hideShow('divPostAdd');" class="hideShowTitle">Add Post</span></h1>

    <div id="divPostAdd">
        <table>
            <tr>
                <td class="defaultTableHeader">
                    Post Text:
                </td>
                <td class="defaultTableCell">
                    <asp:TextBox ID="tbxDescription" CssClass="textbox" TextMode="MultiLine" Rows="8" Width="600px" runat="server"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td colspan="2" class="defaultTableCell" style="text-align:center;">
                    <asp:Button ID="btnPost" CssClass="button" OnClick="btnPostOnClick" Text="Post" runat="server" />
                </td>
            </tr>
        </table>
        
    </div>

</asp:Content>
