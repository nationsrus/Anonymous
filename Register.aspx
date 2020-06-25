<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="Anonymous.Register" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnlRegistration" runat="server">
    <h1><span onclick="hideShow('divRegister');" class="hideShowTitle">Register</span></h1>

    <div id="divRegister">
        <table>
            <tr>
                <td class="defaultTableHeader">Email:</td>
                <td class="defaultTableCell"><asp:TextBox ID="tbxEmail" CssClass="textbox" runat="server"></asp:TextBox></td>
                <td class="defaultTableCell"></td>
            </tr>
            <tr>
                <td class="defaultTableHeader">Password:</td>
                <td class="defaultTableCell"><asp:TextBox ID="tbxPassword1" CssClass="textbox" runat="server" TextMode="Password"></asp:TextBox></td>
                <td class="defaultTableCell">
                    <asp:RegularExpressionValidator ID="rev1" runat="server" 
                            ControlToValidate="tbxPassword1"
                            ErrorMessage="Password must contain: Minimum 8 characters atleast 1 UpperCase Alphabet, 1 LowerCase Alphabet, 1 Number and 1 Special Character which include $@$!%*?&#^_-{} but do not include the following special characters ()=+:;[]"
                            ValidationExpression="^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[$@$!%*?&#^_-{}])[A-Za-z\d$@$!%*?&#^_-{}]{8,}" 
                            ForeColor="Red"
                            />
                </td>
            </tr>
            <tr>
                <td class="defaultTableHeader">Confirm Password:</td>
                <td class="defaultTableCell"><asp:TextBox ID="tbxPassword2" CssClass="textbox" runat="server" TextMode="Password"></asp:TextBox></td>
                <td class="defaultTableCell">
                    <asp:CompareValidator ID="CompareValidator1" runat="server" 
                            ControlToValidate="tbxPassword2"
                            CssClass="ValidationError"
                            ControlToCompare="tbxPassword1"
                            ErrorMessage="No Match" 
                            ForeColor="Red"
                            ToolTip="Password must be the same" />
                </td>
            </tr>
            <tr>
                <td class="defaultTableHeader">Invite Code: (optional)</td>
                <td class="defaultTableCell"><asp:TextBox ID="tbxCode" CssClass="textbox" ToolTip="This only helps to automatically add your friend in your friends list after reigstration." runat="server"></asp:TextBox></td>
            </tr>
            <tr id="InvitedEmailAddressRow" class="defaultTableHeader" visible="false" runat="server">
                <td>
                    Invited Email Address:
                </td>
                <td class="defaultTableCell">
                    <asp:TextBox ID="tbxInvitedEmailAddress" CssClass="textbox" runat="server" ToolTip="This will add your friend as a friend to your account if correct"></asp:TextBox>
                </td>
            </tr>
            <tr id="SkipInvitedEmailRow" class="defaultTableHeader" visible="false" runat="server">
                <td class="defaultTableCell">
                    <asp:CheckBox ID="cbxSkipInvitedEmailAddress" Text="&nbsp;Skip Email invited email address verification" ToolTip="This will not automatically add your friend to your friends list after registration. You'll have to manually add them afterwards." runat="server" />
                </td>
            </tr>
            <tr>
                <td align="center" colspan="2">
                    <asp:Button ID="btnRegister" OnClick="btnRegisterOnClick" runat="server" Text="Register" />
                    <asp:Label ID="lblRegistrationError" runat="server" Visible="false">Email already exists in our system. Go to <a href='login'>Login</a> by <a href='login'>clicking here.</a></asp:Label>
                </td>
            </tr>
        </table>

        <br />

        <ul>
            <li>Do NOT lose your password. Reseting your password afterwards will destory ALL saved messsages as they are encrypted with a hashed key that uses your password as part of its source.</li>
            <li>Your password and email are NOT stored in plain text in the database</li>
            <li>Your email is not stored in plain text in the database for your security</li>
            <li>Recommended: use a password manager to ensure long length unique passwords</li>
        </ul>

    </div>
     </asp:Panel>
    <br />

    <asp:Panel ID="pnlAccountCreated" Visible="false" runat="server">
        <h1><span onclick="hideShow('divRegistrationComplete');" class="hideShowTitle">Registration Complete</span></h1>

        <div id="divRegistrationComplete">

            Account created. Go to <a href='login'>Login</a> by <a href='login'>clicking here.</a><br /><br />
            An email with email verification code has been sent. Certain functions will be disabled until email verification is complete.
        </div>
    </asp:Panel>


    <br /><br />
</asp:Content>
