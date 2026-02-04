<%@ Page Title="BMW Coding" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebPsdzClient._Default" culture="auto" meta:resourcekey="PageResource" uiculture="auto" %>

<%@ Register assembly="AjaxControlToolkit" namespace="AjaxControlToolkit" tagprefix="ajaxToolkit" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <style>.text-left { width: 100%; max-width: 100%; resize: none; overflow: auto; }</style>
    <style>.text-left:focus { outline: none }</style>
    <style>#MainContent_DropDownListOptionType { width: 100%; max-width: 100%; }</style>
    <style>#MainContent_CheckBoxListOptions input { margin-right: 5px; vertical-align: middle; }</style>
    <style>#MainContent_CheckBoxListOptions label { margin: 0; display: contents; vertical-align: middle; }</style>
    <div class="jumbotron">
        <asp:UpdatePanel ID="UpdatePanelStatus" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="False">
            <ContentTemplate>
                <asp:Panel ID="PanelButtons" runat="server" CssClass="panel panel-body" HorizontalAlign="Center" meta:resourcekey="PanelButtonsResource">
                    <asp:Button ID="ButtonStopHost" runat="server" CssClass="btn btn-secondary mb-1" Text="Stop Host" OnClick="ButtonStopHost_Click" meta:resourcekey="ButtonStopHostResource" />
                    <asp:Button ID="ButtonConnect" runat="server" CssClass="btn btn-secondary mb-1" Text="Connect" OnClick="ButtonConnect_OnClick" meta:resourcekey="ButtonConnectResource" />
                    <asp:Button ID="ButtonDisconnect" runat="server" CssClass="btn btn-secondary mb-1" Text="Disconnect" OnClick="ButtonDisconnect_OnClick" meta:resourcekey="ButtonDisconnectResource" />
                    <asp:Button ID="ButtonCreateOptions" runat="server" CssClass="btn btn-secondary mb-1" Text="Create Options" OnClick="ButtonCreateOptions_OnClick" meta:resourcekey="ButtonCreateOptionsResource" />
                    <asp:Button ID="ButtonModifyFa" runat="server" CssClass="btn btn-secondary mb-1" Text="Modify FA" OnClick="ButtonModifyFa_OnClick" meta:resourcekey="ButtonModifyFaResource" />
                    <asp:Button ID="ButtonExecuteTal" runat="server" CssClass="btn btn-secondary mb-1" Text="Execute TAL" OnClick="ButtonExecuteTal_OnClick" meta:resourcekey="ButtonExecuteTalResource" />
                    <asp:Button ID="ButtonAbort" runat="server" CssClass="btn btn-secondary mb-1" Text="Abort" OnClick="ButtonAbort_OnClick" meta:resourcekey="ButtonAbortResource" />
                </asp:Panel>
                <asp:Panel ID="PanelOptions" runat="server" CssClass="panel panel-body" HorizontalAlign="Left" meta:resourcekey="PanelOptionsResource" >
                    <asp:DropDownList ID="DropDownListOptionType" CssClass="dropdown mb-1" runat="server" OnSelectedIndexChanged="DropDownListOptionType_OnSelectedIndexChanged" AutoPostBack="True" meta:resourcekey="DropDownListOptionTypeResource">
                    </asp:DropDownList>
                    <asp:CheckBoxList ID="CheckBoxListOptions" runat="server" CssClass="checkbox mb-1" CellPadding="0" CellSpacing="0" RepeatColumns="1" RepeatDirection="Horizontal" OnSelectedIndexChanged="CheckBoxListOptions_OnSelectedIndexChanged" AutoPostBack="True" meta:resourcekey="CheckBoxListOptionsResource">
                    </asp:CheckBoxList>
                </asp:Panel>
                <asp:Panel ID="PanelStatus" runat="server" CssClass="panel panel-body" HorizontalAlign="Center" meta:resourcekey="PanelStatusResource" >
                    <asp:TextBox ID="TextBoxStatus" runat="server" CssClass="text-left mb-1" ReadOnly="True" TextMode="MultiLine" Rows="10" meta:resourcekey="TextBoxStatusResource"></asp:TextBox>
                    <asp:TextBox ID="TextBoxProgress" runat="server" CssClass="text-left mb-1" ReadOnly="True" meta:resourcekey="TextBoxProgressResource"></asp:TextBox>
                </asp:Panel>

                <asp:LinkButton ID="LinkButtonMsgModal" runat="server"></asp:LinkButton>
                <asp:Panel ID="PanelMsgModal" runat="server" CssClass="modalPopup" style="display:none;">
                    <div class="jumbotron">
                        <asp:Panel ID="PanelMsgModalText" runat="server" CssClass="panel panel-body mb-1" HorizontalAlign="Center">
                            <asp:Literal ID="LiteralMsgModal" runat="server"></asp:Literal>
                        </asp:Panel>
                        <asp:Panel ID="PanelMsgModalButtons" runat="server" CssClass="panel panel-body" HorizontalAlign="Center">
                            <asp:Button ID="ButtonMsgOk" runat="server" CssClass="btn btn-secondary" Text="Yes" meta:resourcekey="ButtonMsgOk" OnClick="ButtonMsgOk_OnClick" />
                            <asp:Button ID="ButtonMsgYes" runat="server" CssClass="btn btn-secondary" Text="Yes" meta:resourcekey="ButtonMsgYes" OnClick="ButtonMsgYes_OnClick" />
                            <asp:Button ID="ButtonMsgNo" runat="server" CssClass="btn btn-secondary" Text="No" meta:resourcekey="ButtonMsgNo" OnClick="ButtonMsgNo_OnClick" />
                        </asp:Panel>
                        <asp:HiddenField ID="HiddenFieldMsgModal" runat="server"></asp:HiddenField>
                    </div>
                </asp:Panel>
                <ajaxToolkit:ModalPopupExtender ID="ModalPopupExtenderMsg" BehaviorID="ModalPopupExtenderMsgBehaviour" DropShadow="true" runat="server" TargetControlID="LinkButtonMsgModal" PopupControlID="PanelMsgModal">
                </ajaxToolkit:ModalPopupExtender>
            </ContentTemplate>
        </asp:UpdatePanel>
        <asp:UpdatePanel ID="UpdatePanelTimer" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="False">
            <ContentTemplate>
                <asp:Panel ID="PanelHeader" runat="server" CssClass="panel panel-collapse" HorizontalAlign="Left" meta:resourcekey="PanelHeaderResource">
                    <asp:Label ID="LabelLastUpdate" runat="server" CssClass="text-left" meta:resourcekey="LabelLastUpdateResource"></asp:Label>
                </asp:Panel>
                <asp:Timer ID="TimerUpdate" runat="server" Interval="2000" OnTick="TimerUpdate_Tick">
                </asp:Timer>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <script type="text/javascript">
        function isPostBack()
        {
            var postBackState = '<%= Page.IsPostBack.ToString()%>';
            console.log("isPostBack State=" + postBackState);
            return postBackState == 'True';
        }

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

        function showPopupMsgModal(show)
        {
            console.log("Show modal message popup: Show=" + show);
            var modalPopup = $find('ModalPopupExtenderMsgBehaviour');
            if (modalPopup)
            {
                if (show)
                {
                    console.log("Show modal popup");
                    modalPopup.show();
                }
                else
                {
                    console.log("Hide modal popup");
                    modalPopup.hide();
                }
            }
            else
            {
                console.log("ModalPopupExtenderMsg not found");
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
