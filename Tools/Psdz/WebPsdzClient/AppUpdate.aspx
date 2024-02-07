<%@ Page Title="<%$Resources:Global,AppUpdateRequired%>" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AppUpdate.aspx.cs" Inherits="WebPsdzClient.AppUpdate" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="jumbotron">
        <h3><asp:Literal ID="LiteralAppUpdate" runat="server" Text="<%$Resources:Global,AppUpdateRequired%>"></asp:Literal></h3>
    </div>
</asp:Content>
