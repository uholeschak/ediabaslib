using log4net;
using PsdzClient;
using PsdzClient.Programming;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebPsdzClient.App_Data;

namespace WebPsdzClient
{
    public partial class _Default : BasePage
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(_Default));

        protected void Page_Init(object sender, EventArgs e)
        {
            //log.InfoFormat("_Default Page_Init");
        }

        protected override void Page_Load(object sender, EventArgs e)
        {
            //log.InfoFormat("_Default Page_Load");
            base.Page_Load(sender, e);
            SessionContainer sessionContainer = GetSessionContainer();
            if (sessionContainer == null)
            {
                return;
            }

            if (!IsPostBack)
            {
                UpdateStatus();
                UpdateCurrentOptions();
                UpdateTimerPanel();
            }
            else
            {
                Control postbackControl = GetPostBackControl();
                if (postbackControl == UpdatePanelStatus)
                {
                    UpdateStatus(true);
                }

                if (sessionContainer.RefreshOptions)
                {
                    sessionContainer.RefreshOptions = false;
                    UpdateOptions();
                }
            }
        }

        protected void Page_Unload(object sender, EventArgs e)
        {
            //log.InfoFormat("_Default Page_Unload");
        }

        protected void ButtonStopHost_Click(object sender, EventArgs e)
        {
            SessionContainer sessionContainer = GetSessionContainer();
            if (sessionContainer == null)
            {
                return;
            }

            if (sessionContainer.TaskActive)
            {
                return;
            }

            sessionContainer.StopProgrammingService(Global.IstaFolder);
        }

        protected void ButtonConnect_OnClick(object sender, EventArgs e)
        {
            SessionContainer sessionContainer = GetSessionContainer();
            if (sessionContainer == null)
            {
                return;
            }

            if (sessionContainer.TaskActive)
            {
                return;
            }

            sessionContainer.ConnectVehicle(Global.IstaFolder);
        }

        protected void ButtonDisconnect_OnClick(object sender, EventArgs e)
        {
            SessionContainer sessionContainer = GetSessionContainer();
            if (sessionContainer == null)
            {
                return;
            }

            if (sessionContainer.TaskActive)
            {
                return;
            }

            sessionContainer.DisconnectVehicle();
        }

        protected void ButtonCreateOptions_OnClick(object sender, EventArgs e)
        {
            SessionContainer sessionContainer = GetSessionContainer();
            if (sessionContainer == null)
            {
                return;
            }

            if (sessionContainer.TaskActive)
            {
                return;
            }

#if USE_RPC_CLIENT
            sessionContainer.VehicleFunctions(PsdzRpcServer.Shared.PsdzOperationType.CreateOptions);
#else
            sessionContainer.VehicleFunctions(ProgrammingJobs.OperationType.CreateOptions);
#endif
        }

        protected void ButtonModifyFa_OnClick(object sender, EventArgs e)
        {
            SessionContainer sessionContainer = GetSessionContainer();
            if (sessionContainer == null)
            {
                return;
            }

            if (sessionContainer.TaskActive)
            {
                return;
            }

            sessionContainer.UpdateTargetFa();
#if USE_RPC_CLIENT
            sessionContainer.VehicleFunctions(PsdzRpcServer.Shared.PsdzOperationType.BuildTalModFa);
#else
            sessionContainer.VehicleFunctions(ProgrammingJobs.OperationType.BuildTalModFa);
#endif
        }

        protected void ButtonExecuteTal_OnClick(object sender, EventArgs e)
        {
            SessionContainer sessionContainer = GetSessionContainer();
            if (sessionContainer == null)
            {
                return;
            }

            if (sessionContainer.TaskActive)
            {
                return;
            }

            if (sessionContainer.TaskActive)
            {
                return;
            }

#if USE_RPC_CLIENT
            sessionContainer.VehicleFunctions(PsdzRpcServer.Shared.PsdzOperationType.ExecuteTal);
#else
            sessionContainer.VehicleFunctions(ProgrammingJobs.OperationType.ExecuteTal);
#endif
        }

        protected void ButtonAbort_OnClick(object sender, EventArgs e)
        {
            SessionContainer sessionContainer = GetSessionContainer();
            if (sessionContainer == null)
            {
                return;
            }

            if (!sessionContainer.TaskActive)
            {
                return;
            }

            sessionContainer.Cancel();
        }

        protected void ButtonMsgOk_OnClick(object sender, EventArgs e)
        {
            log.InfoFormat("_Default ButtonMsgOk_OnClick");

            ModalPopupExtenderMsg.Hide();

            SessionContainer sessionContainer = GetSessionContainer();
            if (sessionContainer == null)
            {
                return;
            }

            sessionContainer.ShowMessageModal = null;
            sessionContainer.ShowMessageModalResult = true;
            sessionContainer.MessageWaitEvent.Set();
        }

        protected void ButtonMsgYes_OnClick(object sender, EventArgs e)
        {
            log.InfoFormat("_Default ButtonMsgYes_OnClick");

            ModalPopupExtenderMsg.Hide();

            SessionContainer sessionContainer = GetSessionContainer();
            if (sessionContainer == null)
            {
                return;
            }

            sessionContainer.ShowMessageModal = null;
            sessionContainer.ShowMessageModalResult = true;
            sessionContainer.MessageWaitEvent.Set();
        }

        protected void ButtonMsgNo_OnClick(object sender, EventArgs e)
        {
            log.InfoFormat("_Default ButtonMsgNo_OnClick");

            ModalPopupExtenderMsg.Hide();

            SessionContainer sessionContainer = GetSessionContainer();
            if (sessionContainer == null)
            {
                return;
            }

            sessionContainer.ShowMessageModal = null;
            sessionContainer.ShowMessageModalResult = false;
            sessionContainer.MessageWaitEvent.Set();
        }

        protected void DropDownListOptionType_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            SessionContainer sessionContainer = GetSessionContainer();
            if (sessionContainer == null)
            {
                return;
            }

            if (sessionContainer.TaskActive)
            {
                return;
            }

#if USE_RPC_CLIENT
            PsdzRpcServer.Shared.PsdzRpcSwiRegisterEnum ? selectedSwiRegister = null;
            ListItem listItemSelect = DropDownListOptionType.SelectedItem;
            if (listItemSelect != null)
            {
                if (Enum.TryParse(listItemSelect.Value, true, out PsdzRpcServer.Shared.PsdzRpcSwiRegisterEnum swiRegister))
                {
                    selectedSwiRegister = swiRegister;
                }
            }
#else
            PsdzDatabase.SwiRegisterEnum? selectedSwiRegister = null;
            ListItem listItemSelect = DropDownListOptionType.SelectedItem;
            if (listItemSelect != null)
            {
                if (Enum.TryParse(listItemSelect.Value, true, out PsdzDatabase.SwiRegisterEnum swiRegister))
                {
                    selectedSwiRegister = swiRegister;
                }
            }
#endif
            if (sessionContainer.SelectedSwiRegister != selectedSwiRegister)
            {
                sessionContainer.SelectedSwiRegister = selectedSwiRegister;
                UpdateOptions();
            }
        }

        protected void CheckBoxListOptions_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            SessionContainer sessionContainer = GetSessionContainer();
            if (sessionContainer == null)
            {
                return;
            }

            try
            {
                if (sessionContainer.TaskActive)
                {
                    return;
                }

                Request.ValidateInput();
                string eventArgs = Request.Form["__EVENTTARGET"];
                if (string.IsNullOrEmpty(eventArgs))
                {
                    return;
                }
                string[] checkedBox = eventArgs.Split('$');
                if (checkedBox.Length < 1)
                {
                    return;
                }

                if (!int.TryParse(checkedBox[checkedBox.Length - 1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int selectedIndex))
                {
                    log.ErrorFormat("CheckBoxListOptions_OnSelectedIndexChanged Invalid checkbox: {0}", checkedBox[checkedBox.Length - 1]);
                    return;
                }

                if (selectedIndex < 0 || selectedIndex >= CheckBoxListOptions.Items.Count)
                {
                    log.ErrorFormat("CheckBoxListOptions_OnSelectedIndexChanged Invalid index: {0}", selectedIndex);
                    return;
                }

                ListItem listItem = CheckBoxListOptions.Items[selectedIndex];
                if (!listItem.Enabled)
                {
                    log.ErrorFormat("CheckBoxListOptions_OnSelectedIndexChanged Disabled: {0}", listItem.Text);
                    return;
                }

                log.InfoFormat("CheckBoxListOptions_OnSelectedIndexChanged Selected: {0}", listItem.Text);
                string optionId = listItem.Value;
                bool modified = false;
                if (sessionContainer.SelectOptionId(optionId, listItem.Selected))
                {
                    modified = true;
                }
                else
                {
                    log.ErrorFormat("CheckBoxListOptions_OnSelectedIndexChanged Failed to select option: {0}", listItem.Text);
                }

                if (modified)
                {
                    UpdateOptions();
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CheckBoxListOptions_OnSelectedIndexChanged Exception: {0}", ex.Message);
            }
        }

        protected void TimerUpdate_Tick(object sender, EventArgs e)
        {
            //log.InfoFormat("_Default TimerUpdate_Tick");

            SessionContainer sessionContainer = GetSessionContainer();
            if (sessionContainer == null)
            {
                return;
            }

            sessionContainer.UpdateDisplay(false);
            UpdateTimerPanel();
        }

        private void UpdateStatus(bool updatePanel = false)
        {
            log.InfoFormat("UpdateStatus Update panel: {0}", updatePanel);

            try
            {
                SessionContainer sessionContainer = GetSessionContainer();
                if (sessionContainer == null)
                {
                    return;
                }

                bool active = sessionContainer.TaskActive;
                bool abortPossible = sessionContainer.IsCancelPossible();
                bool hostRunning = false;
                bool vehicleConnected = false;
                bool talPresent = false;

                if (!active)
                {
                    hostRunning = sessionContainer.IsPsdzInitialized();
                }

                if (sessionContainer.IsVehicleConnected())
                {
                    vehicleConnected = true;
                    talPresent = sessionContainer.IsTalPresent();
                }

                bool hasOptionsDict = sessionContainer.HasOptionsDict();
                bool modifyTal = !active && hostRunning && vehicleConnected && hasOptionsDict;
                ButtonStopHost.Enabled = !active && hostRunning;
                ButtonStopHost.Visible = sessionContainer.DeepObdVersion <= 0;
                ButtonConnect.Enabled = !active && !vehicleConnected;
                ButtonDisconnect.Enabled = !active && hostRunning && vehicleConnected;
                ButtonCreateOptions.Enabled = !active && hostRunning && vehicleConnected && !hasOptionsDict;
                ButtonModifyFa.Enabled = modifyTal;
                ButtonExecuteTal.Enabled = modifyTal && talPresent;
                ButtonAbort.Enabled = active && abortPossible;
                DropDownListOptionType.Enabled = !active && hostRunning && vehicleConnected;
                CheckBoxListOptions.Enabled = !active && hostRunning && vehicleConnected;

                TextBoxStatus.Text = sessionContainer.StatusText;
                TextBoxProgress.Text = sessionContainer.ProgressText;

                string messageText = sessionContainer.ShowMessageModal;
                if (!string.IsNullOrEmpty(messageText))
                {
                    string modalCount = sessionContainer.ShowMessageModalCount.ToString(CultureInfo.InvariantCulture);
                    if (string.Compare(HiddenFieldMsgModal.Value, modalCount, StringComparison.Ordinal) != 0)
                    {
                        bool okBtn = sessionContainer.ShowMessageModalOkBtn;
                        bool messageWait = sessionContainer.ShowMessageModalWait;
                        messageText = messageText.Replace("\r\n", "<br/>");

                        log.InfoFormat("_Default Page_Load UpdateStatus Count={0}, OKButton={1}, Wait={2}, Message='{3}'", modalCount, okBtn, messageWait, messageText);

                        LiteralMsgModal.Text = messageText;
                        ButtonMsgOk.Visible = okBtn;
                        ButtonMsgYes.Visible = !okBtn;
                        ButtonMsgNo.Visible = !okBtn;
                        ModalPopupExtenderMsg.Show();
                        HiddenFieldMsgModal.Value = modalCount;
                    }
                }

                if (updatePanel)
                {
                    UpdatePanels();
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("UpdateStatus Exception: {0}", ex.Message);
            }
        }

        private void UpdatePanels()
        {
            try
            {
                if (!UpdatePanelStatus.IsInPartialRendering)
                {
                    UpdatePanelStatus.Update();
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("UpdatePanels Exception: {0}", ex.Message);
            }

            UpdateTimerPanel();
        }

        private void UpdateTimerPanel()
        {
            try
            {
                SessionContainer sessionContainer = GetSessionContainer();
                if (sessionContainer == null)
                {
                    return;
                }

                DateTime localTime = DateTime.Now;
                DateTime utcTime = localTime.ToUniversalTime();
                string localString = localTime.ToString("HH:mm:ss");
                string utcString = utcTime.ToString("HH:mm:ss");

                string timeFormat = GetGlobalResourceObject("Global", "TimeDisplay") as string ?? string.Empty;
                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format(CultureInfo.InvariantCulture, timeFormat, localString, utcString));

                int? connectTimeouts = sessionContainer.ConnectTimeouts;
                if (connectTimeouts != null && connectTimeouts.Value > 0)
                {
                    sb.Append("<br/>");
                    string connectFailFormat = GetGlobalResourceObject("Global", "InternetTimeouts") as string ?? string.Empty;
                    sb.Append(string.Format(CultureInfo.InvariantCulture, connectFailFormat, connectTimeouts.Value));
                }

                LabelLastUpdate.Text = sb.ToString();
                if (!UpdatePanelTimer.IsInPartialRendering)
                {
                    UpdatePanelTimer.Update();
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("UpdateTimerPanel Exception: {0}", ex.Message);
            }
        }

        private void UpdateOptions()
        {
            SessionContainer sessionContainer = GetSessionContainer();
            if (sessionContainer == null)
            {
                return;
            }

            sessionContainer.ReloadPage();
        }

        private void UpdateCurrentOptions(bool updatePanel = false)
        {
            try
            {
                SessionContainer sessionContainer = GetSessionContainer();
                if (sessionContainer == null)
                {
                    return;
                }

                DropDownListOptionType.Items.Clear();

                bool hasOptionsDict = sessionContainer.HasOptionsDict();
                if (hasOptionsDict)
                {
                    if (sessionContainer.SelectedSwiRegister == null)
                    {
                        sessionContainer.SetDefaultSelectedSwiRegister();
                    }

                    List<ListItem> listItems = sessionContainer.GetOptionTypes();
                    if (listItems != null)
                    {
                        DropDownListOptionType.Items.AddRange(listItems.ToArray());
                    }
                }
                else
                {
                    sessionContainer.SelectedSwiRegister = null;
                }

                SelectOptions(sessionContainer.SelectedSwiRegister);
                PanelOptions.Visible = hasOptionsDict;

                if (updatePanel)
                {
                    UpdatePanels();
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("SelectOptions Exception: {0}", ex.Message);
            }
        }

#if USE_RPC_CLIENT
        private void SelectOptions(PsdzRpcServer.Shared.PsdzRpcSwiRegisterEnum? swiRegisterEnum)
#else
        private void SelectOptions(PsdzDatabase.SwiRegisterEnum? swiRegisterEnum)
#endif
        {
            try
            {
                SessionContainer sessionContainer = GetSessionContainer();
                if (sessionContainer == null)
                {
                    return;
                }

                CheckBoxListOptions.Items.Clear();
                List<ListItem> listItems = sessionContainer.GetSelectedOptions(swiRegisterEnum);
                if (listItems != null)
                {
                    CheckBoxListOptions.Items.AddRange(listItems.ToArray());
                }

                UpdatePanels();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("SelectOptions Exception: {0}", ex.Message);
            }
        }
    }
}
