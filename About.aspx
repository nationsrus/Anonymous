<%@ Page Title="About" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="Anonymous.About" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Title %>.</h2>
    <h3>A secure freedom of speech forum to discuss local issues, political issues, and foreign affairs.</h3>
    <p>Anonymous.NationsRUs.com does NOT ever support illegal and/or violent actions. That includes physical, virtual, vocal, communication, and any form of action that can harm or hurt anyone. Knowledge is power, let's spread knowledge.</p>
    <p>The purpose of Anonymous.NationsRUs.com is to provide a safe and secure area for people to discuss misunderstandings and educate each others.</p>
    <p>Privacy is the utmost concern here. No detailed user information is ever stored.</p>
    

    <h2>Software Technology Summary</h2>
    Anonymous.NationsRUs.com uses the following software technology below to ensure robust security and privacy:
    <ul>
        <li>Encryptions used: AES, HMAC, RSA, SHA</li>
        <li>Private messages are encrypted with RSA</li>
        <li>Passwords are encrypted with AES, HMAC</li>
        <li>Posts are stored as plain text in the database</li>
        <li>Friends information is stored against AES encryption</li>
        <li>Account email is stored in the database using SHA512 plus application salt</li>
    </ul>

    <h2>GitHub</h2>
    <p>This is an open sourced project and details can be found at <a href="https://github.com/nationsrus/Anonymous" target="_blank">https://github.com/nationsrus/Anonymous</a></p>

</asp:Content>
