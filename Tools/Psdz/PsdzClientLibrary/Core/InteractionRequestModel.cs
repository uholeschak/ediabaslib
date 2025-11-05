using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Threading;

namespace PsdzClient.Core
{
    [DataContract]
    public abstract class InteractionRequestModel<TResponse> : InteractionModel, IInteractionRequestModel<TResponse>, IInteractionModel, INotifyPropertyChanged where TResponse : InteractionResponse
    {
        private TResponse response;

        private readonly ManualResetEvent resumeEvent;

        private bool showPending;

        [DataMember]
        public TResponse Response
        {
            get
            {
                return response;
            }
            set
            {
                response = value;
                OnPropertyChanged("Response");
            }
        }

        [DataMember]
        public bool ShowPending
        {
            get
            {
                return showPending;
            }
            internal set
            {
                showPending = value;
                OnPropertyChanged("ShowPending");
            }
        }

        protected InteractionRequestModel()
        {
            resumeEvent = new ManualResetEvent(initialState: false);
            showPending = true;
        }

        internal override void OnRegistered()
        {
            base.OnRegistered();
            resumeEvent.Reset();
        }

        public TResponse WaitOnResponse(bool resetFirst = false)
        {
            if (resetFirst)
            {
                resumeEvent.Reset();
            }
            resumeEvent.WaitOne();
            return response;
        }

        public void OnResponseRecived(InteractionResponse response)
        {
            try
            {
                Response = (TResponse)response;
                if (ConfigIAPHelper.IsLogInterupters())
                {
                    LogResponseMessage(Response);
                }
                OnResponseRecivedAndLog(Response);
            }
            catch (InvalidCastException)
            {
                Log.Error("InteractionRequestModel.OnResponseRecived()", $"Invalid response cast from type:'{response?.GetType()}' to type:'{typeof(TResponse)}'.");
            }
        }

        protected void NotifyAboutResponseReceived()
        {
            resumeEvent.Set();
        }

        private void OnResponseRecivedAndLog(TResponse response)
        {
            Log.Info("InteractionRequestModel.OnResponseRecived()", "InteractionResponse was set to the model.");
            OnResponseReceived(response);
        }

        public abstract void OnResponseReceived(TResponse response);

        public override void Dispose()
        {
            Log.Debug("InteractionRequestModel.Dispose", string.Format("Start: {0}: {1} ", "IsDisposing", base.IsDisposing));
            if (!base.IsDisposing)
            {
                base.Dispose();
                NotifyAboutResponseReceived();
            }
            Log.Debug("InteractionRequestModel.Dispose", "End");
        }
    }
}