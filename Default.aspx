<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Anonymous._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron jumbotron2">
        <br /><br /><br /><br /><br /><br /><br /><br />
    </div>

    <div class="row">
        <div class="col-md-4">
            <h2>Trending Posts</h2>
            <p>
                Some of the top and newest posts
            </p>
            <p>
                <a class="btn btn-default" href="Nation">Trending Posts &raquo;</a>
            </p>
        </div>
        <div class="col-md-4">
            <h2>Contact</h2>
            <p>
                How to reach the person or team behind this project.
            </p>
            <p>
                <a class="btn btn-default" href="Contact">Learn more &raquo;</a>
            </p>
        </div>
        <div class="col-md-4">
            <h2>About</h2>
            <p>
                Find out more about Anonymous.NationsRUs.com from open source location, privacy information, and more.
            </p>
            <p>
                <a class="btn btn-default" href="About">Learn more &raquo;</a>
            </p>
        </div>
    </div>
    <div style="display:none;">
        <div class="splashBannerText">Anonymous Forums About Nations</div><br />
        <div class="splashBannerText">Open sourced forums for issues relating to nations, governments foreign and local affairs.</div><br />
        <div class="splashBannerText"><a href="Nation">Nation's Trending Posts &raquo;</a></div>

    </div>
</asp:Content>
