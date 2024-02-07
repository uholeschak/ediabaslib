<%@ Page Title="<%$Resources:Global,AccessDenied%>" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AccessDenied.aspx.cs" Inherits="WebPsdzClient.AccessDenied" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="jumbotron">
        <h3><asp:Literal ID="LiteralAccessDenied" runat="server" Text="<%$Resources:Global,AccessDenied%>"></asp:Literal></h3>
    </div>
</asp:Content>
