<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="anon.aspx.cs" Inherits="Anonymous.anon" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h1><span onclick="hideShow('divPosts');" class="hideShowTitle">Posts</span></h1>

    <div id="divPosts">
        <asp:Repeater ID="rptrPosts" runat="server">
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
                    <td class="defaultTableCell" style="font-size:10px;"><%# DataBinder.Eval(Container.DataItem, "activityTimeStamp") %></td>
                </tr>
            </ItemTemplate>
            <FooterTemplate>
                </table>
            </FooterTemplate>
        </asp:Repeater>
    </div>

    <h1><span onclick="hideShow('divVotes');" class="hideShowTitle">Votes</span></h1>

    <div id="divVotes">
        <asp:Repeater ID="rptrVotes" runat="server">
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
                    <td class="defaultTableCell" style="font-size:10px;"><%# DataBinder.Eval(Container.DataItem, "activityTimeStamp") %></td>
                </tr>
            </ItemTemplate>
            <FooterTemplate>
                </table>
            </FooterTemplate>
        </asp:Repeater>
    </div>

</asp:Content>
