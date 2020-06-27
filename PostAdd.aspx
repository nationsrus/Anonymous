<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="PostAdd.aspx.cs" Inherits="Anonymous.PostAdd" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <script>
        function onSubmit(token) {
            document.getElementById("ctl01").submit();
            document.getElementById("<%= btnPost.ClientID %>").click();
        }
    </script>

    <br />
    Nation: <asp:DropDownList ID="ddlNation" runat="server"></asp:DropDownList>

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
                    <button class="g-recaptcha button" 
                        data-sitekey="6Ldu6qkZAAAAAIPQ-aaOL62X4m_not9wmbSM_cUI" 
                        data-callback='onSubmit' 
                        data-action='submit' OnClick="btnRegisterOnClick" runat="server" id="btnRecaptchaSubmit" >Submit</button>
                    <asp:Button ID="btnPost" CssClass="hidden" OnClick="btnPostOnClick" Text="Post" runat="server" />
                </td>
            </tr>
        </table>
        
    </div>

</asp:Content>
