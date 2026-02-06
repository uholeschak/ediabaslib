<%@ Page Title="<%$Resources:Global,SessionsExceeded%>" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="SessionsExceeded.aspx.cs" Inherits="WebPsdzClient.SessionsExceeded" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="jumbotron">
        <h3><asp:Literal ID="LiteralSessionsExceeded" runat="server" Text="<%$Resources:Global,SessionsExceeded%>"></asp:Literal></h3>
        <h3><asp:Literal ID="LiteralTryAgainLater" runat="server" Text="<%$Resources:Global,TryAgainLater%>"></asp:Literal></h3>
    </div>
</asp:Content>
