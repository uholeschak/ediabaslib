<%@ Page Title="BMW Coding" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebPsdzClient._Default" culture="auto" meta:resourcekey="PageResource" uiculture="auto" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <style>.text-left { width: 100%; max-width: 100%; resize: none; overflow: auto; }</style>
    <style>.table { border-width: 0; border-color: transparent; }</style>
    <style>.table th { text-align: center }</style>
    <style>.dropdown { width: 100%; max-width: 100%; }</style>
    <style>.checkbox label { text-indent: 30px }</style>
    <div class="jumbotron">
        <asp:UpdatePanel ID="UpdatePanelStatus" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="False">
            <ContentTemplate>
                <asp:Panel ID="PanelButtons" runat="server" CssClass="panel-body" HorizontalAlign="Center" meta:resourcekey="PanelButtonsResource">
                    <asp:Button ID="ButtonStopHost" runat="server" CssClass="btn" Text="Stop Host" OnClick="ButtonStopHost_Click" meta:resourcekey="ButtonStopHostResource" />
                    <asp:Button ID="ButtonConnect" runat="server" CssClass="btn" Text="Connect" OnClick="ButtonConnect_OnClick" meta:resourcekey="ButtonConnectResource" />
                    <asp:Button ID="ButtonDisconnect" runat="server" CssClass="btn" Text="Disconnect" OnClick="ButtonDisconnect_OnClick" meta:resourcekey="ButtonDisconnectResource" />
                    <asp:Button ID="ButtonCreateOptions" runat="server" CssClass="btn" Text="Create Options" OnClick="ButtonCreateOptions_OnClick" meta:resourcekey="ButtonCreateOptionsResource" />
                    <asp:Button ID="ButtonModifyFa" runat="server" CssClass="btn" Text="Modify FA" OnClick="ButtonModifyFa_OnClick" meta:resourcekey="ButtonModifyFaResource" />
                    <asp:Button ID="ButtonExecuteTal" runat="server" CssClass="btn" Text="Execute TAL" OnClick="ButtonExecuteTal_OnClick" meta:resourcekey="ButtonExecuteTalResource" />
                    <asp:Button ID="ButtonAbort" runat="server" CssClass="btn" Text="Abort" OnClick="ButtonAbort_OnClick" meta:resourcekey="ButtonAbortResource" />
                </asp:Panel>
                <asp:Panel ID="PanelOptions" runat="server" CssClass="panel-body" HorizontalAlign="Left" meta:resourcekey="PanelOptionsResource" >
                    <asp:DropDownList ID="DropDownListOptionType" CssClass="dropdown" runat="server" OnSelectedIndexChanged="DropDownListOptionType_OnSelectedIndexChanged" AutoPostBack="True" meta:resourcekey="DropDownListOptionTypeResource">
                    </asp:DropDownList>
                    <asp:CheckBoxList ID="CheckBoxListOptions" runat="server" CssClass="checkbox" CellPadding="5" CellSpacing="5" RepeatColumns="1" RepeatDirection="Horizontal" OnSelectedIndexChanged="CheckBoxListOptions_OnSelectedIndexChanged" AutoPostBack="True" meta:resourcekey="CheckBoxListOptionsResource">
                    </asp:CheckBoxList>
                </asp:Panel>
                <asp:Panel ID="PanelStatus" runat="server" CssClass="panel-body" HorizontalAlign="Center" meta:resourcekey="PanelStatusResource" >
                    <asp:TextBox ID="TextBoxStatus" runat="server" CssClass="text-left" ReadOnly="True" TextMode="MultiLine" Rows="10" meta:resourcekey="TextBoxStatusResource"></asp:TextBox>
                    <asp:TextBox ID="TextBoxProgress" runat="server" CssClass="text-left" ReadOnly="True" meta:resourcekey="TextBoxProgressResource"></asp:TextBox>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
        <asp:UpdatePanel ID="UpdatePanelTimer" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="False">
            <ContentTemplate>
                <asp:Panel ID="PanelHeader" runat="server" CssClass="panel-collapse" HorizontalAlign="Left" meta:resourcekey="PanelHeaderResource">
                    <asp:Label ID="LabelLastUpdate" runat="server" CssClass="label" meta:resourcekey="LabelLastUpdateResource"></asp:Label>
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

        function scrollStatusPanel()
        {
            console.log("Scroll status panel");
            var textarea = document.getElementById('<%=TextBoxStatus.ClientID %>');
            if (textarea)
            {
                textarea.scrollTop = textarea.scrollHeight;
            }
        }

        var postBackId = null;
        function beginRequestHandler(sender, args)
        {
            var postBackElem = args.get_postBackElement();
            postBackId = null;
            if (postBackElem)
            {
                postBackId = postBackElem.id;
            }
            console.log("beginRequestHandler Id=" + postBackId);
        }

        function endRequestHandler(sender, args)
        {
            console.log("endRequestHandler Id=" + postBackId);
            if (postBackId)
            {
                if (postBackId.indexOf("UpdatePanelStatus") !== -1)
                {
                    scrollStatusPanel();
                }
            }
        }

        var requestManager = Sys.WebForms.PageRequestManager.getInstance();
        if (requestManager)
        {
            requestManager.add_beginRequest(beginRequestHandler);
            requestManager.add_endRequest(endRequestHandler);
        }

        window.addEventListener('load', function ()
        {
            scrollStatusPanel();
        });

        window.addEventListener('resize', function (event)
        {
            scrollStatusPanel();
        }, true);
    </script>
</asp:Content>
