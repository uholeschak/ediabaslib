using System.Collections.Generic;
using Android.Content;
using Android.OS;
using Android.Util;
using System.Linq;
using System.Text;
using Android.Net;

namespace BmwDeepObd.BroadcastManager;

public class InternalBroadcastManager
{
    private class ReceiverRecord
    {
        public IntentFilter filter;
        public BroadcastReceiver receiver;
        public bool broadcasting;
        public bool dead;

        public ReceiverRecord(IntentFilter _filter, BroadcastReceiver _receiver)
        {
            filter = _filter;
            receiver = _receiver;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("Receiver{");
            builder.Append(receiver);
            builder.Append(" filter=");
            builder.Append(filter);
            if (dead)
            {
                builder.Append(" DEAD");
            }
            builder.Append("}");
            return builder.ToString();
        }
    }

    private class BroadcastRecord
    {
        public Intent intent;
        public List<ReceiverRecord> receivers;

        public BroadcastRecord(Intent _intent, List<ReceiverRecord> _receivers)
        {
            intent = _intent;
            receivers = _receivers;
        }
    }

    private static string TAG = "LocalBroadcastManager";
    private static bool DEBUG = false;

    private Context mAppContext;

    private Dictionary<BroadcastReceiver, List<ReceiverRecord>> mReceivers
            = new Dictionary<BroadcastReceiver, List<ReceiverRecord>>();
    private Dictionary<string, List<ReceiverRecord>> mActions = new Dictionary<string, List<ReceiverRecord>>();

    private List<BroadcastRecord> mPendingBroadcasts = new List<BroadcastRecord>();

    public const int MSG_EXEC_PENDING_BROADCASTS = 1;

    private Handler mHandler;

    private static object mLock = new object();
    private static InternalBroadcastManager mInstance;

    public static InternalBroadcastManager GetInstance(Context context)
    {
        lock (mLock)
        {
            if (mInstance == null)
            {
                mInstance = new InternalBroadcastManager(context.ApplicationContext);
            }
            return mInstance;
        }
    }

    private class BroadcastHandler : Handler
    {
        public BroadcastHandler(Looper looper) : base(looper)
        {
        }

        public override void HandleMessage(Message msg)
        {
            switch (msg.What)
            {
                case MSG_EXEC_PENDING_BROADCASTS:
                    // ToDo: Access ExecutePendingBroadcasts
                    //ExecutePendingBroadcasts();
                    break;

                default:
                    base.HandleMessage(msg);
                    break;
            }
        }
    }

    private InternalBroadcastManager(Context context)
    {
        mAppContext = context;
        mHandler = new BroadcastHandler(context.MainLooper);
    }

    /**
     * Register a receive for any local broadcasts that match the given IntentFilter.
     *
     * @param receiver The BroadcastReceiver to handle the broadcast.
     * @param filter Selects the Intent broadcasts to be received.
     *
     * @see #unregisterReceiver
     */
    public void RegisterReceiver(BroadcastReceiver receiver, IntentFilter filter)
    {
        lock(mReceivers)
        {
            ReceiverRecord entry = new ReceiverRecord(filter, receiver);
            mReceivers.TryGetValue(receiver, out List<ReceiverRecord> filters);
            if (filters == null)
            {
                filters = new List<ReceiverRecord>();
                mReceivers.Add(receiver, filters);
            }
            filters.Add(entry);
            for (int i = 0; i < filter.CountActions(); i++)
            {
                string action = filter.GetAction(i);
                mActions.TryGetValue(action, out List<ReceiverRecord> entries);
                if (entries == null)
                {
                    entries = new List<ReceiverRecord>();
                    mActions.Add(action, entries);
                }
                entries.Add(entry);
            }
        }
    }

    /**
     * Unregister a previously registered BroadcastReceiver.  <em>All</em>
     * filters that have been registered for this BroadcastReceiver will be
     * removed.
     *
     * @param receiver The BroadcastReceiver to unregister.
     *
     * @see #registerReceiver
     */
    public void UnregisterReceiver(BroadcastReceiver receiver)
    {
        lock(mReceivers)
        {
            mReceivers.TryGetValue(receiver, out List<ReceiverRecord> filters);
            if (filters == null)
            {
                return;
            }
            mReceivers.Remove(receiver);

            for (int i = filters.Count() - 1; i >= 0; i--)
            {
                ReceiverRecord filter = filters[i];
                filter.dead = true;
                for (int j = 0; j < filter.filter.CountActions(); j++)
                {
                    string action = filter.filter.GetAction(j);
                    mActions.TryGetValue(action, out List<ReceiverRecord> receivers);
                    if (receivers != null)
                    {
                        for (int k = receivers.Count - 1; k >= 0; k--)
                        {
                            ReceiverRecord rec = receivers[k];
                            if (rec.receiver == receiver)
                            {
                                rec.dead = true;
                                receivers.RemoveAt(k);
                            }
                        }
                        if (receivers.Count <= 0)
                        {
                            mActions.Remove(action);
                        }
                    }
                }
            }
        }
    }

    /**
     * Broadcast the given intent to all interested BroadcastReceivers.  This
     * call is asynchronous; it returns immediately, and you will continue
     * executing while the receivers are run.
     *
     * @param intent The Intent to broadcast; all receivers matching this
     *     Intent will receive the broadcast.
     *
     * @see #registerReceiver
     *
     * @return Returns true if the intent has been scheduled for delivery to one or more
     * broadcast receivers.  (Note that delivery may not ultimately take place if one of those
     * receivers is unregistered before it is dispatched.)
     */
    public bool SendBroadcast(Intent intent)
    {
        lock (mReceivers)
        {
            string action = intent.Action;
            string type = intent.ResolveTypeIfNeeded(mAppContext.ContentResolver);
            Uri data = intent.Data;
            string scheme = intent.Scheme;
            ICollection<string> categories = intent.Categories;
            bool debug = DEBUG || ((intent.Flags & ActivityFlags.DebugLogResolution) != 0);
            if (debug)
            {
                Log.Verbose(
                    TAG, "Resolving type " + type + " scheme " + scheme
                         + " of intent " + intent);
            }

            mActions.TryGetValue(intent.Action, out List<ReceiverRecord> entries);
            if (entries != null)
            {
                if (debug)
                {
                    Log.Verbose(TAG, "Action list: " + entries);
                }

                List<ReceiverRecord> receivers = null;
                for (int i = 0; i < entries.Count; i++)
                {
                    ReceiverRecord receiver = entries[i];
                    if (debug)
                    {
                        Log.Verbose(TAG, "Matching against filter " + receiver.filter);
                    }

                    if (receiver.broadcasting)
                    {
                        if (debug)
                        {
                            Log.Verbose(TAG, "  Filter's target already added");
                        }
                        continue;
                    }

                    MatchResults match = receiver.filter.Match(action, type, scheme, data, categories, "LocalBroadcastManager");
                    if (match >= 0)
                    {
                        if (debug)
                        {
                            Log.Verbose(TAG, string.Format("  Filter matched!  match={0}", match));
                        }

                        if (receivers == null)
                        {
                            receivers = new List<ReceiverRecord>();
                        }
                        receivers.Add(receiver);
                        receiver.broadcasting = true;
                    }
                    else
                    {
                        if (debug)
                        {
                            string reason;
                            switch (match)
                            {
                                case MatchResults.NoMatchAction: reason = "action"; break;
                                case MatchResults.NoMatchCategory: reason = "category"; break;
                                case MatchResults.NoMatchData: reason = "data"; break;
                                case MatchResults.NoMatchType: reason = "type"; break;
                                default: reason = "unknown reason"; break;
                            }
                            Log.Verbose(TAG, "  Filter did not match: " + reason);
                        }
                    }
                }

                if (receivers != null)
                {
                    for (int i = 0; i < receivers.Count(); i++)
                    {
                        receivers[i].broadcasting = false;
                    }
                    mPendingBroadcasts.Add(new BroadcastRecord(intent, receivers));
                    if (!mHandler.HasMessages(MSG_EXEC_PENDING_BROADCASTS))
                    {
                        mHandler.SendEmptyMessage(MSG_EXEC_PENDING_BROADCASTS);
                    }
                    return true;
                }
            }
        }
        return false;
    }

    /**
     * Like {@link #sendBroadcast(Intent)}, but if there are any receivers for
     * the Intent this function will block and immediately dispatch them before
     * returning.
     */
    public void SendBroadcastSync(Intent intent)
    {
        if (SendBroadcast(intent))
        {
            ExecutePendingBroadcasts();
        }
    }

    public void ExecutePendingBroadcasts()
    {
        while (true)
        {
            BroadcastRecord[] brs;
            lock(mReceivers)
            {
                int N = mPendingBroadcasts.Count;
                if (N <= 0)
                {
                    return;
                }
                brs = new BroadcastRecord[N];
                mPendingBroadcasts.CopyTo(brs);
                mPendingBroadcasts.Clear();
            }

            for (int i = 0; i < brs.Length; i++)
            {
                BroadcastRecord br = brs[i];
                int nbr = br.receivers.Count;
                for (int j = 0; j < nbr; j++)
                {
                    ReceiverRecord rec = br.receivers[j];
                    if (!rec.dead)
                    {
                        rec.receiver.OnReceive(mAppContext, br.intent);
                    }
                }
            }
        }
    }
}
