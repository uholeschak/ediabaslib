using System;
using BMW.Authoring;
using BMW.Authoring.API;
using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Interaction.Models;
using PsdzClient;
using PsdzClient.Core;

namespace BMW.Authoring.API
{
    public class QDMModeAPI : IQDMModeAPI, IHideObjectMembers
    {
        [PreserveSource(Hint = "IIstaOperationLogic", Placeholder = true)]
        private PlaceholderType operationLogic;

        public IAuthoringModule IstaModule { get; set; }

        public QDMModeAPI(IAuthoringModule module)
        {
            //[-] operationLogic = module.IstaOperationLogic as IIstaOperationLogic;
            IstaModule = module;
        }

        public bool ActivateQDMMode(ITextLocator popupTitle = null, ITextLocator popupMessage = null, DialogSize dialogSize = DialogSize.S)
        {
            if (IsQDMModeActivated())
            {
                Log.Info("ActivateQDMMode", "was called but QDM mode is already active.");
                return true;
            }
            Log.Info("ActivateQDMMode", "called with param popupTitle= {0} and popupMessage= {1}", popupTitle, popupMessage);
            InteractionQuestionModel interactionQuestionModel = new InteractionQuestionModel();
            if (popupTitle == null)
            {
                interactionQuestionModel.Title = new FormatedData("#QDM.ErrorPatterns").Localize();
            }
            else
            {
                interactionQuestionModel.Title = popupTitle.ToString();
            }
            if (popupMessage == null)
            {
                interactionQuestionModel.QuestionText = "";
            }
            else
            {
                try
                {
                    //[-] interactionQuestionModel.QuestionTextHtml = new TextContent(popupMessage.TextContent.FormattedText).GetTextForUI(operationLogic.Lang)[0].TextItem;
                }
                catch (Exception arg)
                {
                    Log.Warning(Log.CurrentMethod(), $"Could not transform SPE text to FlowDocument, using plain text instead. Exception: {arg}");
                    interactionQuestionModel.QuestionText = popupMessage.ToString();
                }
            }
            interactionQuestionModel.CmdNoLabel = new FormatedData("#No").Localize();
            interactionQuestionModel.CmdYesLabel = new FormatedData("#Yes").Localize();
            //[-] operationLogic.Services.InteractionService.Register(interactionQuestionModel);
            InteractionButtonResponse response = interactionQuestionModel.Response;
            if (response != null && response.Action == InteractionButton.No)
            {
                Log.Info("ActivateQDMMode", "Deactivating QDM Mode from the dialog");
            }
            else
            {
                InteractionButtonResponse response2 = interactionQuestionModel.Response;
                if (response2 != null && response2.Action == InteractionButton.Yes)
                {
                    Log.Info("ActivateQDMMode", "Activating QDM Mode");
                    //[-] operationLogic.ActivateQDMMode();
                    //[-] operationLogic.Services.NavigationService.NavigateTo(TabName.ServicePlan_HitList);
                }
            }
            return IsQDMModeActivated();
        }

        public bool FillTrefferListe(string identifikator, int? priority = null)
        {
            //[-] if (!operationLogic.CheckQDMQModeActivated())
            {
                Log.Error("QDMModeAPI.FillTreffeListe", "QDM Mode is not activated yet.Activate it before filling into trefferliste.");
                return false;
            }
            //[-] if (operationLogic.AddToIdentificatorListForQDM(identifikator, priority))
            //[-] {
            //[-] Log.Info("QDMModeAPI.FillTreffeListe", $"Added document to HitList with identificator: {identifikator} and priority: {priority}");
            //[-] }
            //[-] else
            {
                Log.Warning("QDMModeAPI.FillTreffeListe", $"Document with identificator: {identifikator} and priority: {priority} could not have been added to HitList");
            }
            return true;
        }

        public bool IsQDMModeActivated()
        {
            //[-] return operationLogic.CheckQDMQModeActivated();
            //[+] return true;
            return true;
        }

        public bool DeactivateQDMMode()
        {
            Log.Info("DeactivateQDMMode", "Deactivating QDM Mode.");
            //[-] return operationLogic.DeactivateQDMMode();
            //[+] return true;
            return true;
        }

        Type IHideObjectMembers.GetType()
        {
            return GetType();
        }
    }
}
