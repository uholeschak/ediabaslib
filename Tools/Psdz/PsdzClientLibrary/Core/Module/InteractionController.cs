using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Interaction;
using BMW.Rheingold.CoreFramework.Interaction.Models;
using BMW.Rheingold.Psdz;
using PsdzClient.Core;
using PsdzClient.Utility;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace BMW.Rheingold.CoreFramework.Interaction
{
    public class InteractionController : IInteractionController, IInteractionButtonNotificationService
    {
        private readonly InteractionDataContext interactionDataContext;

        public IInteractionDataContext InteractionDataContext => interactionDataContext;

        public InteractionController()
        {
            interactionDataContext = new InteractionDataContext();
            interactionDataContext.ModelCollection.CollectionChanged += ModelCollection_CollectionChanged;
        }

        private void ModelCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        InteractionModel model2 = e.NewItems[0] as InteractionModel;
                        TriggerInteractionMetrics(model2, shown: true);
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        InteractionModel model = e.OldItems[0] as InteractionModel;
                        TriggerInteractionMetrics(model, shown: false);
                        break;
                    }
            }
        }

        private void TriggerInteractionMetrics(InteractionModel model, bool shown)
        {
            if (!TimeMetricsUtility.HasMetrics)
            {
                return;
            }
            string text = null;
            if (model is InteractionProgressModel || model is InteractionDoIpCheckModel)
            {
                return;
            }
            if (!(model is InteractionQuestionPopupModel interactionQuestionPopupModel))
            {
                if (!(model is InteractionQuestionModel interactionQuestionModel))
                {
                    if (model is InteractionMessageModel interactionMessageModel)
                    {
                        text = interactionMessageModel.MessageText;
                        text = text.Substring(0, Math.Min(50, text.Length));
                    }
                }
                else
                {
                    text = interactionQuestionModel.QuestionText;
                    text = text.Substring(0, Math.Min(50, text.Length));
                }
            }
            else
            {
                text = interactionQuestionPopupModel.Question;
                text = text.Substring(0, Math.Min(50, text.Length));
            }
            Type type = model.GetType();
            string text2 = (model.IsTriggeredSynchronously ? "sync " : "async");
            string text3 = "[" + text2 + "] " + type.Name + " - " + model.Title;
            if (!string.IsNullOrEmpty(text))
            {
                text3 = text3 + " - " + text + "...";
            }
            if (shown)
            {
                TimeMetricsUtility.Instance.PopupShown(text3);
            }
            else
            {
                TimeMetricsUtility.Instance.PopupClosed(text3);
            }
        }

        public void ChangeMode(InteractionProgressModel model, TaskMode mode)
        {
            if (mode == TaskMode.RunInBackround)
            {
                interactionDataContext.AddBackgroundInteraction(model);
                lock (InteractionDataContext.ModelCollection)
                {
                    InteractionDataContext.ModelCollection.Remove(model);
                }
            }
            if (mode != TaskMode.RunInForeground)
            {
                return;
            }
            interactionDataContext.RemoveBackgroundInteraction(model);
            if (!InteractionDataContext.ModelCollection.Contains(model))
            {
                lock (InteractionDataContext.ModelCollection)
                {
                    InteractionDataContext.ModelCollection.Insert(0, model);
                }
            }
        }

        public void DeregisterInteraction(InteractionModel model)
        {
            try
            {
                if (model == null)
                {
                    throw new ArgumentNullException("model");
                }
                model.Disposed -= InteractionModelDisposed;
                model.OnDeregistered();
                if (interactionDataContext.IsBackgroundInteractionAvailable(model as IInteractionProgressModel))
                {
                    interactionDataContext.RemoveBackgroundInteraction(model as IInteractionProgressModel);
                }
                else
                {
                    if (!InteractionDataContext.ModelCollection.Contains(model))
                    {
                        Log.Error("InteractionController.DeregisterInteraction", "The interaction model '{0}' is not registered.", model.GetType().Name);
                        return;
                    }
                    if (!InteractionDataContext.ModelCollection.Last().Equals(model))
                    {
                        Log.Warning("InteractionController.DeregisterInteraction", "The interaction model '{0}' was deregistered in a wrong order.", model.GetType().Name);
                    }
                    lock (InteractionDataContext.ModelCollection)
                    {
                        InteractionDataContext.ModelCollection.Remove(model);
                    }
                }
                Log.Info("InteractionController.DeregisterInteraction()", "A '{0}' was deregistered.", model.GetType().Name);
            }
            catch (Exception exception)
            {
                Log.ErrorException("InteractionController.DeregisterInteraction", exception);
            }
        }

        public void DeregisterInteractionBackground(IInteractionProgressModel model)
        {
            interactionDataContext.RemoveBackgroundInteraction(model);
        }

        public void NotifyClosing(Guid modelId)
        {
            if (GetOperationModelById(modelId) is InteractionModel interactionModel)
            {
                interactionModel.OnClosing();
                InteractionResponse responseCloseButton = interactionModel.ResponseCloseButton;
                if (responseCloseButton != null)
                {
                    try
                    {
                        NotifyResponse(interactionModel.Guid, responseCloseButton);
                    }
                    catch (Exception exception)
                    {
                        Log.WarningException("InteractionController.NotifyClosing()", exception);
                    }
                }
            }
            else
            {
                Log.Error("InteractionController.NotifyClosing()", "Model not found.");
            }
        }

        public bool NotifyResponse(Guid modelId, InteractionResponse response)
        {
            if (GetOperationModelById(modelId) is IInteractionRequestModel<InteractionResponse> interactionRequestModel)
            {
                try
                {
                    interactionRequestModel.OnResponseRecived(response);
                    return true;
                }
                catch (Exception exception)
                {
                    Log.Error("InteractionController.NotifyResponse()", "Failed to set response of type '{0}' to the model.", response.GetType().Name);
                    Log.ErrorException("InteractionController.NotifyResponse()", exception);
                    return false;
                }
            }
            Log.Error("InteractionController.NotifyResponse()", "Model not found.");
            return false;
        }

        public string GetInteractionModelType(Guid modelId)
        {
            return (GetOperationModelById(modelId) as IInteractionRequestModel<InteractionResponse>).GetType().FullName;
        }

        public virtual void RegisterInteraction(InteractionModel model)
        {
            try
            {
                //[-] model.IsTriggeredSynchronously = true;
                //[-] RegisterInteractionModel(model);
                //[-] (model as IInteractionRequestModel<InteractionResponse>)?.WaitOnResponse();
            }
            catch (Exception exception)
            {
                Log.ErrorException("InteractionController.RegisterInteraction", exception);
            }
        }

        public virtual Task<TResponse> RegisterInteractionAsync<TResponse>(InteractionRequestModel<TResponse> model) where TResponse : InteractionResponse
        {
            try
            {
                //[-] RegisterInteractionModel(model);
                //[-] return Task.Run(() => model.WaitOnResponse());
                //[+] return Task.FromResult<TResponse>(null);
                return Task.FromResult<TResponse>(null);
            }
            catch (Exception exception)
            {
                Log.ErrorException("InteractionController.RegisterInteraction", exception);
                return Task.FromResult<TResponse>(null);
            }
        }

        public virtual TResponse RegisterInteractionSync<TResponse>(InteractionRequestModel<TResponse> model) where TResponse : InteractionResponse
        {
            try
            {
                //[-] model.IsTriggeredSynchronously = true;
                //[-] RegisterInteractionModel(model);
                //[-] return model.WaitOnResponse();
                //[+] return null;
                return null;
            }
            catch (Exception exception)
            {
                Log.ErrorException("InteractionController.RegisterInteraction", exception);
                return null;
            }
        }

        public void RegisterInteractionBackground(IInteractionProgressModel model)
        {
            interactionDataContext.AddBackgroundInteraction(model);
        }

        private IInteractionModel GetOperationModelById(Guid modelId)
        {
            if (InteractionDataContext.ModelCollection.All((IInteractionModel m) => m.Guid != modelId))
            {
                Log.Error("InteractionController.GetOperationModelById()", "The interaction model with id '{0}' is not registered.", modelId);
                return null;
            }
            IInteractionModel interactionModel = InteractionDataContext.ModelCollection.First((IInteractionModel x) => x.Guid.Equals(modelId));
            if (!interactionModel.Equals(InteractionDataContext.ModelCollection.Last()))
            {
                Log.Warning("InteractionController.GetOperationModelById()", "The received response don't match with the current model.");
            }
            return interactionModel;
        }

        private void InteractionModelDisposed(object sender, EventArgs e)
        {
            if (sender is InteractionModel model)
            {
                DeregisterInteraction(model);
            }
        }

        private void RegisterInteractionModel(InteractionModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }
            if (InteractionDataContext.ModelCollection.Contains(model))
            {
                throw new ArgumentException("Model is already registered.");
            }
            model.Disposed += InteractionModelDisposed;
            lock (InteractionDataContext.ModelCollection)
            {
                InteractionDataContext.ModelCollection.Add(model);
            }
            LogInteractionMessageModel(model);
            model.OnRegistered();
        }

        private static void LogInteractionMessageModel(InteractionModel model)
        {
            if (model == null)
            {
                return;
            }
            if (!(model is InteractionMessageModel interactionMessageModel))
            {
                Log.Info("InteractionController.RegisterInteractionModel()", "A '{0}' was registered. Interaction Title: '{1}'", model.GetType().Name, model.Title);
                return;
            }
            Translator translator = GetTranslator();
            if (ShouldLogErrorBasedOnTitle(interactionMessageModel.Title, translator))
            {
                Log.Info("InteractionController.RegisterInteractionModel()", "[Error InteractionMessageModel] was registered. With Title: `{0}`, message: `{1}`, detail: `{2}`", TryTranslateStringToEnglish(interactionMessageModel.Title, translator), TryTranslateStringToEnglish(interactionMessageModel.MessageText, translator), TryTranslateStringToEnglish(interactionMessageModel.DetailText, translator));
            }
        }

        private static Translator GetTranslator()
        {
            return new Translator
            {
                ResourceName = "BMW.Rheingold.CoreFramework.Localization.Localization.xml"
            };
        }

        private static bool ShouldLogErrorBasedOnTitle(string title, Translator translator)
        {
            string module = "ISTAGui";
            return new string[2] { "#Error", "#Warning" }.Any((string x) => x.Equals(translator.GetId(title, module)));
        }

        private static string TryTranslateStringToEnglish(string toTranslation, Translator translator)
        {
            string text = "en-US";
            if (ConfigSettings.CurrentUICulture.Equals(text) || ConfigSettings.CurrentUICulture.Equals("en-GB"))
            {
                return toTranslation;
            }
            string module = "ISTAGui";
            string id = translator.GetId(toTranslation, module);
            string name = translator.GetName(id, module, text);
            if (!string.IsNullOrEmpty(name) && !name.Equals(id))
            {
                return name;
            }
            return toTranslation;
        }
    }
}
