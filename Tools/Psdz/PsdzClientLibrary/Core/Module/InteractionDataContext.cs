using BMW.Rheingold.CoreFramework.Interaction.Models;
using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace BMW.Rheingold.CoreFramework.Interaction
{
    [DataContract]
    public class InteractionDataContext : IInteractionDataContext, INotifyPropertyChanged
    {
        [DataMember]
        private readonly List<IInteractionProgressModel> backgroundInteractionCollection;
        [DataMember]
        private readonly ObservableCollection<IInteractionModel> modelCollection;
        [DataMember]
        private IInteractionProgressModel backgroundInteractionModel;
        public IInteractionProgressModel BackgroundInteractionModel
        {
            get
            {
                return backgroundInteractionModel;
            }

            private set
            {
                backgroundInteractionModel = value;
                OnPropertyChanged("BackgroundInteractionModel");
            }
        }

        public ObservableCollection<IInteractionModel> ModelCollection => modelCollection;

        public event PropertyChangedEventHandler PropertyChanged;
        public InteractionDataContext()
        {
            modelCollection = new ObservableCollection<IInteractionModel>();
            backgroundInteractionCollection = new List<IInteractionProgressModel>();
        }

        public void AddBackgroundInteraction(IInteractionProgressModel model)
        {
            if (model != null)
            {
                if (!backgroundInteractionCollection.Contains(model))
                {
                    backgroundInteractionCollection.Add(model);
                }
                else
                {
                    backgroundInteractionCollection.Remove(model);
                    backgroundInteractionCollection.Add(model);
                }

                BackgroundInteractionModel = backgroundInteractionCollection.LastOrDefault();
                return;
            }

            throw new ArgumentNullException("model cannot be null.");
        }

        public bool IsBackgroundInteractionAvailable(IInteractionProgressModel model)
        {
            if (model == null)
            {
                return false;
            }

            return backgroundInteractionCollection.Contains(model);
        }

        public void RemoveBackgroundInteraction(IInteractionProgressModel model)
        {
            if (model != null)
            {
                if (backgroundInteractionCollection.Remove(model))
                {
                    BackgroundInteractionModel = backgroundInteractionCollection.LastOrDefault();
                }
                else
                {
                    Log.Info(Log.CurrentMethod(), "The given model is not an active background interaction model. Nothing will be removed.");
                }

                return;
            }

            throw new ArgumentNullException("model cannot be null");
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}