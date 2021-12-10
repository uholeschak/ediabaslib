<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebPsdzClient._Default" %>
<%@ OutputCache Location="None" VaryByParam="None" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron">
        <h1>ASP.NET</h1>
        <p class="lead">ASP.NET is a free web framework for building great Web sites and Web applications using HTML, CSS, and JavaScript.</p>
        <p><a href="http://www.asp.net" class="btn btn-primary btn-lg">Learn more &raquo;</a></p>
    </div>
    <div class="jumbotron">
        <h1>Toolbar</h1>
        <p class="lead">
        <asp:Button ID="ButtonStartHost" runat="server" Text="Start Host" OnClick="ButtonStartHost_Click" />
        <asp:Button ID="ButtonStopHost" runat="server" Text="Stop Host" OnClick="ButtonStopHost_Click" />
        </p>
    </div>
</asp:Content>
