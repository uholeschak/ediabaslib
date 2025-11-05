using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace PsdzClient.Core
{
    public class ObservableCollectionEx<T> : ObservableCollection<T>
    {
        public override event NotifyCollectionChangedEventHandler CollectionChanged;
        public ObservableCollectionEx()
        {
        }

        public ObservableCollectionEx(IEnumerable<T> collection) : base(collection)
        {
        }

        public ObservableCollectionEx(List<T> list) : base(list)
        {
        }

        public void AddAsSingleObject(T item)
        {
            if (!Contains(item))
            {
                Add(item);
            }
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            try
            {
                using (BlockReentrancy())
                {
                    NotifyCollectionChangedEventHandler notifyCollectionChangedEventHandler = CollectionChanged;
                    if (notifyCollectionChangedEventHandler == null)
                    {
                        return;
                    }

                    Delegate[] invocationList = notifyCollectionChangedEventHandler.GetInvocationList();
                    for (int i = 0; i < invocationList.Length; i++)
                    {
                        NotifyCollectionChangedEventHandler notifyCollectionChangedEventHandler2 = (NotifyCollectionChangedEventHandler)invocationList[i];
                        if (notifyCollectionChangedEventHandler2.Target is DispatcherObject dispatcherObject && !dispatcherObject.CheckAccess())
                        {
                            dispatcherObject.Dispatcher.Invoke(DispatcherPriority.DataBind, notifyCollectionChangedEventHandler2, this, e);
                        }
                        else
                        {
                            notifyCollectionChangedEventHandler2(this, e);
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