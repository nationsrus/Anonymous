<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ChangePassword.aspx.cs" Inherits="Anonymous.ChangePassword" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h1><span onclick="hideShow('divChangePassword');" class="hideShowTitle">Change Password</span></h1>
    <div id="divChangePassword">
         <table>
             <tr>
                 <td class="defaultTableHeader">Password:</td>
                 <td class="defaultTableCell"><asp:TextBox CssClass="textbox" TextMode="Password" ID="tbxPassword" runat="server"></asp:TextBox></td>
                 <td></td>
             </tr>
             <tr>
                 <td class="defaultTableHeader">New Password:</td>
                 <td class="defaultTableCell"><asp:TextBox CssClass="textbox" TextMode="Password" ID="tbxNewPassword" runat="server"></asp:TextBox></td>
                 <td>
                     <asp:RegularExpressionValidator ID="rev1" runat="server" 
                            ControlToValidate="tbxNewPassword"
                            ErrorMessage="Password must contain: Minimum 8 characters atleast 1 UpperCase Alphabet, 1 LowerCase Alphabet, 1 Number and 1 Special Character which include $@$!%*?&#^_-{} but do not include the following special characters ()=+:;[]"
                            ValidationExpression="^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[$@$!%*?&#^_-{}])[A-Za-z\d$@$!%*?&#^_-{}]{8,}" 
                            ForeColor="Red"
                            />
                 </td>
             </tr>
             <tr>
                 <td class="defaultTableHeader">Confirm New Password:</td>
                 <td class="defaultTableCell"><asp:TextBox CssClass="textbox" TextMode="Password" ID="tbxConfirmNewPassword" runat="server"></asp:TextBox></td>
                 <td><asp:CompareValidator ID="CompareValidator1" runat="server" 
                            ControlToValidate="tbxNewPassword"
                            CssClass="ValidationError"
                            ControlToCompare="tbxConfirmNewPassword"
                            ErrorMessage="No Match" 
                            ForeColor="Red"
                            ToolTip="Password must be the same" /></td>
             </tr>
            <tr>
                <td colspan="2" align="center">
                    <asp:Button CssClass="button" ID="btnChangePassword" runat="server" Text="Change Password" OnClick="btnChangePassword_Click" />
                    <asp:Label ID="lblPasswordChangeSuccess" Font-Bold="true" ForeColor="Green" runat="server" Visible="false"></asp:Label>
                    <asp:Label ID="lblPasswordChangeError" Font-Bold="true" ForeColor="Red" runat="server" Visible="false"></asp:Label>
                </td>
            </tr>
         </table>
    </div>

</asp:Content>
