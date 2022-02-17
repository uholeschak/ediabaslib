using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using BMW.Rheingold.Psdz.Client;
using log4net;
using PsdzClient;
using PsdzClient.Programing;
using WebPsdzClient.App_Data;

namespace WebPsdzClient
{
    public partial class _Default : Page
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(_Default));

        protected override void InitializeCulture()
        {
            SessionContainer.SetLogInfo(Session.SessionID);
            SessionContainer sessionContainer = GetSessionContainer();
            if (sessionContainer == null)
            {
                return;
            }

            string language = sessionContainer.GetLanguage();
            if (!string.IsNullOrEmpty(language))
            {
                try
                {
                    CultureInfo culture = CultureInfo.CreateSpecificCulture(language.ToLowerInvariant());
                    Thread.CurrentThread.CurrentCulture = culture;
                    Thread.CurrentThread.CurrentUICulture = culture;
                    Culture = culture.TwoLetterISOLanguageName;
                    UICulture = culture.TwoLetterISOLanguageName;
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("InitializeCulture Exception: {0}", ex.Message);
                }
            }
            base.InitializeCulture();
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            //log.InfoFormat("_Default Page_Init");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //log.InfoFormat("_Default Page_Load");
            SessionContainer sessionContainer = GetSessionContainer();
            if (sessionContainer == null)
            {
                return;
            }

            if (!sessionContainer.RefreshOptions)
            {
                string messageText = sessionContainer.ShowMessageModal;
                if (!string.IsNullOrEmpty(messageText))
                {
                    try
                    {
                        messageText = messageText.Replace("\r\n", "<br>");
                        bool messageWait = sessionContainer.ShowMessageModalWait;
                        log.InfoFormat("_Default Page_Load Show Message='{0}'", messageText);

                        LiteralMsgModal.Text = messageText;
                        ButtonMsgOk.Visible = !messageWait;
                        ButtonMsgYes.Visible = messageWait;
                        ButtonMsgNo.Visible = messageWait;
                        ModalPopupExtenderMsgOk.Show();

                        sessionContainer.ShowMessageModal = null;
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Show Message Exception: {0}", ex.Message);
                    }
                }
            }

            if (!IsPostBack)
            {
                UpdateStatus();
                UpdateCurrentOptions();
                UpdateTimerPanel();
            }
            else
            {
                Control postbackControl = GetPostBackControl(this);
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

            sessionContainer.VehicleFunctions(ProgrammingJobs.OperationType.CreateOptions);
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

            sessionContainer.ProgrammingJobs.UpdateTargetFa();
            sessionContainer.VehicleFunctions(ProgrammingJobs.OperationType.BuildTalModFa);
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

            sessionContainer.VehicleFunctions(ProgrammingJobs.OperationType.ExecuteTal);
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

            ModalPopupExtenderMsgOk.Hide();

            SessionContainer sessionContainer = GetSessionContainer();
            if (sessionContainer == null)
            {
                return;
            }

            sessionContainer.ShowMessageModalResult = true;
            sessionContainer.MessageWaitEvent.Set();
        }

        protected void ButtonMsgYes_OnClick(object sender, EventArgs e)
        {
            log.InfoFormat("_Default ButtonMsgYes_OnClick");

            ModalPopupExtenderMsgOk.Hide();

            SessionContainer sessionContainer = GetSessionContainer();
            if (sessionContainer == null)
            {
                return;
            }

            sessionContainer.ShowMessageModalResult = true;
            sessionContainer.MessageWaitEvent.Set();
        }

        protected void ButtonMsgNo_OnClick(object sender, EventArgs e)
        {
            log.InfoFormat("_Default ButtonMsgNo_OnClick");

            ModalPopupExtenderMsgOk.Hide();

            SessionContainer sessionContainer = GetSessionContainer();
            if (sessionContainer == null)
            {
                return;
            }

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

            PdszDatabase.SwiRegisterEnum? selectedSwiRegister = null;
            ListItem listItemSelect = DropDownListOptionType.SelectedItem;
            if (listItemSelect != null)
            {
                if (Enum.TryParse(listItemSelect.Value, true, out PdszDatabase.SwiRegisterEnum swiRegister))
                {
                    selectedSwiRegister = swiRegister;
                }
            }

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
                    return;
                }

                if (selectedIndex < 0 || selectedIndex >= CheckBoxListOptions.Items.Count)
                {
                    return;
                }

                ListItem listItem = CheckBoxListOptions.Items[selectedIndex];
                if (!listItem.Enabled)
                {
                    return;
                }

                string swiActionId = listItem.Value;
                if (string.IsNullOrEmpty(swiActionId))
                {
                    return;
                }

                ProgrammingJobs programmingJobs = sessionContainer.ProgrammingJobs;
                bool modified = false;
                Dictionary<PdszDatabase.SwiRegisterEnum, List<ProgrammingJobs.OptionsItem>> optionsDict = sessionContainer.OptionsDict;
                if (optionsDict != null && sessionContainer.SelectedSwiRegister.HasValue)
                {
                    if (optionsDict.TryGetValue(sessionContainer.SelectedSwiRegister.Value, out List<ProgrammingJobs.OptionsItem> optionsItems))
                    {
                        foreach (ProgrammingJobs.OptionsItem optionsItem in optionsItems)
                        {
                            if (string.Compare(optionsItem.SwiAction.Id, swiActionId, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                if (programmingJobs.SelectedOptions != null)
                                {
                                    if (listItem.Selected)
                                    {
                                        if (!programmingJobs.SelectedOptions.Contains(optionsItem))
                                        {
                                            programmingJobs.SelectedOptions.Add(optionsItem);
                                        }
                                    }
                                    else
                                    {
                                        programmingJobs.SelectedOptions.Remove(optionsItem);
                                    }
                                }

                                modified = true;
                                break;
                            }
                        }
                    }
                }

                if (modified)
                {
                    if (sessionContainer.ProgrammingJobs.PsdzContext != null)
                    {
                        sessionContainer.ProgrammingJobs.PsdzContext.Tal = null;
                    }

                    sessionContainer.ProgrammingJobs.UpdateTargetFa();
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

        private SessionContainer GetSessionContainer()
        {
            if (Session.Contents[Global.SessionContainerName] is SessionContainer sessionContainer)
            {
                return sessionContainer;
            }

            log.ErrorFormat("GetSessionContainer No SessionContainer");
            return null;
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
                bool abortPossible = sessionContainer.Cts != null;
                bool hostRunning = false;
                bool vehicleConnected = false;
                bool talPresent = false;
                if (!active)
                {
                    hostRunning = PsdzServiceStarter.IsServerInstanceRunning();
                }

                if (sessionContainer.ProgrammingJobs.PsdzContext?.Connection != null)
                {
                    vehicleConnected = true;
                    talPresent = sessionContainer.ProgrammingJobs.PsdzContext?.Tal != null;
                }

                bool modifyTal = !active && hostRunning && vehicleConnected && sessionContainer.OptionsDict != null;
                ButtonStopHost.Enabled = !active && hostRunning;
                ButtonStopHost.Visible = sessionContainer.DeepObdVersion <= 0;
                ButtonConnect.Enabled = !active && !vehicleConnected;
                ButtonDisconnect.Enabled = !active && hostRunning && vehicleConnected;
                ButtonCreateOptions.Enabled = !active && hostRunning && vehicleConnected && sessionContainer.OptionsDict == null;
                ButtonModifyFa.Enabled = modifyTal;
                ButtonExecuteTal.Enabled = modifyTal && talPresent;
                ButtonAbort.Enabled = active && abortPossible;
                DropDownListOptionType.Enabled = !active && hostRunning && vehicleConnected;
                CheckBoxListOptions.Enabled = !active && hostRunning && vehicleConnected;

                TextBoxStatus.Text = sessionContainer.StatusText;
                TextBoxProgress.Text = sessionContainer.ProgressText;

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
                DateTime localTime = DateTime.Now;
                DateTime utcTime = localTime.ToUniversalTime();
                string localString = localTime.ToString("HH:mm:ss");
                string utcString = utcTime.ToString("HH:mm:ss");

                string timeFormat = GetGlobalResourceObject("Global", "TimeDisplay") as string ?? string.Empty;
                LabelLastUpdate.Text = string.Format(CultureInfo.InvariantCulture, timeFormat, localString, utcString);
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

            if (sessionContainer.DeepObdVersion > 0)
            {
                sessionContainer.ReloadPage();
            }
            else
            {
                try
                {
                    Request.ValidateInput();
                    string url = Request.RawUrl;
                    log.InfoFormat("UpdateOptions Reload Url: {0}", url);
                    if (!string.IsNullOrEmpty(url))
                    {
                        Response.Redirect(url, false);
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("UpdateOptions Exception: {0}", ex.Message);
                }
            }
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

                if (sessionContainer.OptionsDict != null)
                {
                    ProgrammingJobs programmingJobs = sessionContainer.ProgrammingJobs;
                    if (sessionContainer.SelectedSwiRegister == null)
                    {
                        sessionContainer.SelectedSwiRegister = programmingJobs.OptionTypes[0].SwiRegisterEnum;
                    }
                    foreach (ProgrammingJobs.OptionType optionTypeUpdate in programmingJobs.OptionTypes)
                    {
                        ListItem listItem = new ListItem(optionTypeUpdate.ToString(), optionTypeUpdate.SwiRegisterEnum.ToString());
                        if (sessionContainer.SelectedSwiRegister == optionTypeUpdate.SwiRegisterEnum)
                        {
                            listItem.Selected = true;
                        }
                        DropDownListOptionType.Items.Add(listItem);
                    }
                }
                else
                {
                    sessionContainer.SelectedSwiRegister = null;
                }

                SelectOptions(sessionContainer.SelectedSwiRegister);
                PanelOptions.Visible = sessionContainer.OptionsDict != null;

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

        private void SelectOptions(PdszDatabase.SwiRegisterEnum? swiRegisterEnum)
        {
            try
            {
                SessionContainer sessionContainer = GetSessionContainer();
                if (sessionContainer == null)
                {
                    return;
                }

                CheckBoxListOptions.Items.Clear();

                ProgrammingJobs programmingJobs = sessionContainer.ProgrammingJobs;
                if (programmingJobs.ProgrammingService == null || programmingJobs.PsdzContext == null)
                {
                    return;
                }

                Dictionary<PdszDatabase.SwiRegisterEnum, List<ProgrammingJobs.OptionsItem>> optionsDict = sessionContainer.OptionsDict;
                List<PdszDatabase.SwiAction> selectedSwiActions = GetSelectedSwiActions(programmingJobs);
                List<PdszDatabase.SwiAction> linkedSwiActions = programmingJobs.ProgrammingService.PdszDatabase.ReadLinkedSwiActions(selectedSwiActions, programmingJobs.PsdzContext.Vehicle, null);

                if (optionsDict != null && programmingJobs.SelectedOptions != null && swiRegisterEnum.HasValue)
                {
                    if (optionsDict.TryGetValue(swiRegisterEnum.Value, out List<ProgrammingJobs.OptionsItem> optionsItems))
                    {
                        foreach (ProgrammingJobs.OptionsItem optionsItem in optionsItems)
                        {
                            bool itemSelected = false;
                            bool itemEnabled = true;
                            bool addItem = true;
                            int selectIndex = programmingJobs.SelectedOptions.IndexOf(optionsItem);
                            if (selectIndex >= 0)
                            {
                                if (selectIndex == programmingJobs.SelectedOptions.Count - 1)
                                {
                                    itemSelected = true;
                                }
                                else
                                {
                                    itemSelected = true;
                                    itemEnabled = false;
                                }
                            }
                            else
                            {
                                if (linkedSwiActions != null &&
                                    linkedSwiActions.Any(x => string.Compare(x.Id, optionsItem.SwiAction.Id, StringComparison.OrdinalIgnoreCase) == 0))
                                {
                                    addItem = false;
                                }
                                else
                                {
                                    if (!programmingJobs.ProgrammingService.PdszDatabase.EvaluateXepRulesById(optionsItem.SwiAction.Id, programmingJobs.PsdzContext.Vehicle, null))
                                    {
                                        addItem = false;
                                    }
                                }
                            }

                            if (addItem)
                            {
                                ListItem listItem = new ListItem(optionsItem.ToString(), optionsItem.SwiAction.Id);
                                listItem.Selected = itemSelected;
                                listItem.Enabled = itemEnabled;
                                CheckBoxListOptions.Items.Add(listItem);
                            }
                        }
                    }
                }

                UpdatePanels();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("SelectOptions Exception: {0}", ex.Message);
            }
        }

        private List<PdszDatabase.SwiAction> GetSelectedSwiActions(ProgrammingJobs programmingJobs)
        {
            if (programmingJobs.PsdzContext == null || programmingJobs.SelectedOptions == null)
            {
                return null;
            }

            List<PdszDatabase.SwiAction> selectedSwiActions = new List<PdszDatabase.SwiAction>();
            foreach (ProgrammingJobs.OptionsItem optionsItem in programmingJobs.SelectedOptions)
            {
                if (optionsItem.SwiAction != null)
                {
                    log.InfoFormat("GetSelectedSwiActions Selected: {0}", optionsItem.SwiAction);
                    selectedSwiActions.Add(optionsItem.SwiAction);
                }
            }

            log.InfoFormat("GetSelectedSwiActions Count: {0}", selectedSwiActions.Count);

            return selectedSwiActions;
        }

        public static Control GetPostBackControl(Page page)
        {
            Control control = null;
            string ctrlname = page.Request.Params.Get("__EVENTTARGET");
            if (!string.IsNullOrEmpty(ctrlname))
            {
                control = page.FindControl(ctrlname);
            }
            else
            {
                foreach (string ctl in page.Request.Form)
                {
                    Control c = page.FindControl(ctl);
                    if (c is System.Web.UI.WebControls.Button)
                    {
                        control = c;
                        break;
                    }
                }

            }

            return control;
        }
    }
}