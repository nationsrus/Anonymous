<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Admin.aspx.cs" Inherits="Anonymous.Admin" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h1><span onclick="hideShow('divAdmin');" class="hideShowTitle">Admin</span></h1>

    <div id="divAdmin">
        <asp:Button ID="btnDissambleDll" runat="server" OnClick="btnDissambleDll_Click" Text="Dissamble DLL" />
    </div>
</asp:Content>
