<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebPsdzClient._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron">
        <h1>Toolbar</h1>
        <asp:Table ID="TableButtons" runat="server" HorizontalAlign="Center" CellSpacing="5">
            <asp:TableRow>
                <asp:TableCell>
                    <asp:Button ID="ButtonStartHost" runat="server" Text="Start Host" OnClick="ButtonStartHost_Click" />
                </asp:TableCell>
                <asp:TableCell>
                    <asp:Button ID="ButtonStopHost" runat="server" Text="Stop Host" OnClick="ButtonStopHost_Click" />
                </asp:TableCell>
            </asp:TableRow>
        </asp:Table>
        <asp:UpdatePanel ID="UpdatePanelStatus" runat="server">
            <ContentTemplate>
                <asp:Table ID="TableStatus" runat="server" HorizontalAlign="Center" Width="100%">
                    <asp:TableRow>
                        <asp:TableCell>
                            <asp:TextBox ID="TextBoxStatus" runat="server" ReadOnly="True" TextMode="MultiLine" Width="100%" Rows="10"></asp:TextBox>
                        </asp:TableCell>
                    </asp:TableRow>
                </asp:Table>
                <asp:Timer ID="TimerUpdate" runat="server" Interval="5000" OnTick="TimerUpdate_Tick">
                </asp:Timer>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</asp:Content>
