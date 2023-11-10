using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using PsdzClientLibrary.Core;

namespace PsdzClient.Core
{
    public class ObservableCollectionEx<T> : ObservableCollection<T>
    {
        public ObservableCollectionEx()
        {
        }

        public ObservableCollectionEx(IEnumerable<T> collection) : base(collection)
        {
        }

        public ObservableCollectionEx(List<T> list) : base(list)
        {
        }

        public override event NotifyCollectionChangedEventHandler CollectionChanged;

        public void AddAsSingleObject(T item)
        {
            if (base.Contains(item))
            {
                return;
            }
            base.Add(item);
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            try
            {
                using (base.BlockReentrancy())
                {
                    NotifyCollectionChangedEventHandler collectionChanged = this.CollectionChanged;
                    if (collectionChanged != null)
                    {
                        foreach (NotifyCollectionChangedEventHandler notifyCollectionChangedEventHandler in collectionChanged.GetInvocationList())
                        {
                            DispatcherObject dispatcherObject = notifyCollectionChangedEventHandler.Target as DispatcherObject;
                            if (dispatcherObject != null && !dispatcherObject.CheckAccess())
                            {
                                dispatcherObject.Dispatcher.Invoke(DispatcherPriority.DataBind, notifyCollectionChangedEventHandler, this, new object[]
                                {
                                    e
                                });
                            }
                            else
                            {
                                notifyCollectionChangedEventHandler(this, e);
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("ObservableCollectionEx.OnCollectionChanged()", exception);
            }
        }
    }
}
