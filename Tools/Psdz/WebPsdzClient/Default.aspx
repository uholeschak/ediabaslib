<%@ Page Title="Psdz" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebPsdzClient._Default" culture="auto" meta:resourcekey="PageResourceDefault" uiculture="auto" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <style>.text-left { width: 100%; max-width: 100%; resize: none; overflow: auto; }</style>
    <style>.table { border-width: 0; border-color: transparent; }</style>
    <style>.table th { text-align: center }</style>
    <style>.dropdown { width: 100%; max-width: 100%; }</style>
    <style>.checkbox label { text-indent: 30px }</style>
    <div class="jumbotron">
        <asp:UpdatePanel ID="UpdatePanelStatus" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="False">
            <ContentTemplate>
                <asp:Panel ID="PanelButtons" runat="server" CssClass="panel-body" HorizontalAlign="Center" meta:resourcekey="PanelButtonsResource1">
                    <asp:Button ID="ButtonStopHost" runat="server" CssClass="btn" Text="Stop Host" OnClick="ButtonStopHost_Click" meta:resourcekey="ButtonStopHostDefault" />
                    <asp:Button ID="ButtonConnect" runat="server" CssClass="btn" Text="Connect" OnClick="ButtonConnect_OnClick" meta:resourcekey="ButtonConnectDefault" />
                    <asp:Button ID="ButtonDisconnect" runat="server" CssClass="btn" Text="Disconnect" OnClick="ButtonDisconnect_OnClick" meta:resourcekey="ButtonDisconnectDefault" />
                    <asp:Button ID="ButtonCreateOptions" runat="server" CssClass="btn" Text="Create Options" OnClick="ButtonCreateOptions_OnClick" meta:resourcekey="ButtonCreateOptionsDefault" />
                    <asp:Button ID="ButtonModifyFa" runat="server" CssClass="btn" Text="Modify FA" OnClick="ButtonModifyFa_OnClick" meta:resourcekey="ButtonModifyFaDefault" />
                    <asp:Button ID="ButtonExecuteTal" runat="server" CssClass="btn" Text="Execute TAL" OnClick="ButtonExecuteTal_OnClick" meta:resourcekey="ButtonExecuteTalDefault" />
                    <asp:Button ID="ButtonAbort" runat="server" CssClass="btn" Text="Abort" OnClick="ButtonAbort_OnClick" meta:resourcekey="ButtonAbortDefault" />
                </asp:Panel>
                <asp:Panel ID="PanelOptions" runat="server" CssClass="panel-body" HorizontalAlign="Left" meta:resourcekey="PanelOptionsResource1" >
                    <asp:DropDownList ID="DropDownListOptionType" CssClass="dropdown" runat="server" OnSelectedIndexChanged="DropDownListOptionType_OnSelectedIndexChanged" AutoPostBack="True" meta:resourcekey="DropDownListOptionTypeResource1">
                    </asp:DropDownList>
                    <asp:CheckBoxList ID="CheckBoxListOptions" runat="server" CssClass="checkbox" CellPadding="5" CellSpacing="5" RepeatColumns="1" RepeatDirection="Horizontal" OnSelectedIndexChanged="CheckBoxListOptions_OnSelectedIndexChanged" AutoPostBack="True" meta:resourcekey="CheckBoxListOptionsResource1">
                    </asp:CheckBoxList>
                </asp:Panel>
                <asp:Panel ID="PanelStatus" runat="server" CssClass="panel-body" HorizontalAlign="Center" meta:resourcekey="PanelStatusResource1" >
                    <asp:TextBox ID="TextBoxStatus" runat="server" CssClass="text-left" ReadOnly="True" TextMode="MultiLine" Rows="10" meta:resourcekey="TextBoxStatusResource1"></asp:TextBox>
                    <asp:TextBox ID="TextBoxProgress" runat="server" CssClass="text-left" ReadOnly="True" meta:resourcekey="TextBoxProgressResource1"></asp:TextBox>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
        <asp:UpdatePanel ID="UpdatePanelTimer" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="False">
            <ContentTemplate>
                <asp:Panel ID="PanelHeader" runat="server" CssClass="panel-collapse" HorizontalAlign="Left" meta:resourcekey="PanelHeaderResource1">
                    <asp:Label ID="LabelLastUpdate" runat="server" CssClass="label" meta:resourcekey="LabelLastUpdateResource1"></asp:Label>
                </asp:Panel>
                <asp:Timer ID="TimerUpdate" runat="server" Interval="2000" OnTick="TimerUpdate_Tick">
                </asp:Timer>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <script type="text/javascript">
        function updatePanelStatus()
        {
            console.log("updatePanelStatus called");
            __doPostBack("<%=UpdatePanelStatus.UniqueID %>", "");
        }

        function scrollTextBox(status)
        {
            console.log("scrollTextBox called, Status=" + status);
            if (status)
            {
                var textarea = document.getElementById('<%=TextBoxStatus.ClientID %>');
                textarea.scrollTop = textarea.scrollHeight;
            }
        }
    </script>
</asp:Content>
