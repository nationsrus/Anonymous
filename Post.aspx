<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Post.aspx.cs" Inherits="Anonymous.Post" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .Message {
            padding: 1px 5px 1px 5px;
        }
        .Message-UserTimeStamp {
            font-size:x-small;
            color:slategrey;
        }
        .Message-Text {
            color:darkred;
        }
    </style>
    <asp:Label ID="lblTitle" runat="server" Font-Size="Larger"></asp:Label>
    <asp:Label ID="lblMsg" runat="server" Visible="false"></asp:Label>
    
    <a href="Nation"> << Back</a> <asp:HyperLink ID="hlParent" runat="server" Text=" - Parent Post"></asp:HyperLink>

    <br /><br />

    <asp:Repeater ID="rptrMessages" OnItemCommand="rptrMessages_ItemCommand" runat="server">

    </asp:Repeater>


    <br /><br />
    <h1><span onclick="hideShowFocus('divPostAdd','<%= tbxMesssage.ClientID %>');" class="hideShowTitle">Add Post</span></h1>

    <div id="divPostAdd">
        <asp:TextBox ID="tbxMesssage" CssClass="textbox" TextMode="MultiLine" Rows="8" Width="600px" runat="server"></asp:TextBox><br />
        <asp:Button ID="btnMessageAdd" CssClass="button" Text="Submit" OnClick="btnMessageAddOnClick" runat="server" />
        <div id="divBottmFocus">&nbsp;</div>
    </div>

</asp:Content>
