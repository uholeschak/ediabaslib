using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using log4net;
using PsdzClient;
using PsdzClient.Programing;
using WebPsdzClient.App_Data;

namespace WebPsdzClient
{
    public partial class _Default : Page
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(_Default));

        protected void Page_Init(object sender, EventArgs e)
        {
            log.InfoFormat("_Default Page_Init");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            log.InfoFormat("_Default Page_Load");
            SessionContainer sessionContainer = GetSessionContainer();
            if (sessionContainer == null)
            {
                return;
            }

            sessionContainer.UpdateDisplayFunc = UpdateDisplay;
            sessionContainer.UpdateOptionsFunc = UpdateOptions;

            if (!IsPostBack)
            {
                UpdateDisplay();
            }

            if (!IsPostBack || sessionContainer.RefreshOptions)
            {
                UpdateCurrentOptions();
            }

            sessionContainer.RefreshOptions = false;
        }

        protected void Page_Unload(object sender, EventArgs e)
        {
            log.InfoFormat("_Default Page_Unload");
        }

        protected void ButtonStartHost_Click(object sender, EventArgs e)
        {
            SessionContainer sessionContainer = GetSessionContainer();
            if (sessionContainer == null)
            {
                return;
            }

            sessionContainer.StartProgrammingService(Global.IstaFolder);
        }

        protected void ButtonStopHost_Click(object sender, EventArgs e)
        {
            SessionContainer sessionContainer = GetSessionContainer();
            if (sessionContainer == null)
            {
                return;
            }

            sessionContainer.StopProgrammingService();
        }

        protected void ButtonConnect_OnClick(object sender, EventArgs e)
        {
            SessionContainer sessionContainer = GetSessionContainer();
            if (sessionContainer == null)
            {
                return;
            }

            sessionContainer.ConnectVehicle(Global.IstaFolder, Global.VehicleIp, false);
        }

        protected void ButtonDisconnect_OnClick(object sender, EventArgs e)
        {
            SessionContainer sessionContainer = GetSessionContainer();
            if (sessionContainer == null)
            {
                return;
            }

            sessionContainer.DisconnectVehicle(UpdateDisplay);
        }

        protected void ButtonCreateOptions_OnClick(object sender, EventArgs e)
        {
            SessionContainer sessionContainer = GetSessionContainer();
            if (sessionContainer == null)
            {
                return;
            }

            sessionContainer.VehicleFunctions(UpdateDisplay, ProgrammingJobs.OperationType.CreateOptions);
        }

        protected void ButtonModifyFa_OnClick(object sender, EventArgs e)
        {
            SessionContainer sessionContainer = GetSessionContainer();
            if (sessionContainer == null)
            {
                return;
            }

            sessionContainer.ProgrammingJobs.UpdateTargetFa();
            sessionContainer.VehicleFunctions(UpdateDisplay, ProgrammingJobs.OperationType.BuildTalModFa);
        }

        protected void ButtonExecuteTal_OnClick(object sender, EventArgs e)
        {
            SessionContainer sessionContainer = GetSessionContainer();
            if (sessionContainer == null)
            {
                return;
            }

            sessionContainer.VehicleFunctions(UpdateDisplay, ProgrammingJobs.OperationType.ExecuteTal);
        }

        protected void DropDownListOptionType_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            SessionContainer sessionContainer = GetSessionContainer();
            if (sessionContainer == null)
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
                sessionContainer.ProgrammingJobs.UpdateTargetFa();
                UpdateOptions();
            }
        }

        protected void TimerUpdate_Tick(object sender, EventArgs e)
        {
            UpdateStatus(true);
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

        private void UpdateDisplay()
        {
            UpdateStatus();
        }

        private void UpdateStatus(bool fromTimer = false)
        {
            log.InfoFormat("UpdateStatus FromTimer: {0}", fromTimer);

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
                    hostRunning = sessionContainer.ProgrammingJobs.ProgrammingService != null && sessionContainer.ProgrammingJobs.ProgrammingService.IsPsdzPsdzServiceHostInitialized();
                }

                if (sessionContainer.ProgrammingJobs.PsdzContext?.Connection != null)
                {
                    vehicleConnected = true;
                    talPresent = sessionContainer.ProgrammingJobs.PsdzContext?.Tal != null;
                }

                bool modifyTal = !active && hostRunning && vehicleConnected && sessionContainer.OptionsDict != null;
                ButtonStartHost.Enabled = !active && !hostRunning;
                ButtonStopHost.Enabled = !active && hostRunning;
                ButtonConnect.Enabled = !active && hostRunning && !vehicleConnected;
                ButtonDisconnect.Enabled = !active && hostRunning && vehicleConnected;
                ButtonCreateOptions.Enabled = !active && hostRunning && vehicleConnected && sessionContainer.OptionsDict == null;
                ButtonModifyFa.Enabled = modifyTal;
                ButtonExecuteTal.Enabled = modifyTal && talPresent;

                TextBoxStatus.Text = sessionContainer.StatusText;
            }
            catch (Exception e)
            {
                log.ErrorFormat("UpdateStatus Exception: {0}", e.Message);
            }
        }

        private void UpdateOptions()
        {
            SessionContainer sessionContainer = GetSessionContainer();
            if (sessionContainer == null)
            {
                return;
            }

            sessionContainer.RefreshOptions = true;
            Response.Redirect(Request.RawUrl, false);
        }

        private void UpdateCurrentOptions()
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

                if (!UpdatePanelStatus.IsInPartialRendering)
                {
                    UpdatePanelStatus.Update();
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("SelectOptions Exception: {0}", e.Message);
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

                if (!UpdatePanelStatus.IsInPartialRendering)
                {
                    UpdatePanelStatus.Update();
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("SelectOptions Exception: {0}", e.Message);
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
    }
}