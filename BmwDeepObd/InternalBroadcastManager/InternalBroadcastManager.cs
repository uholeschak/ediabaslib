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
        public IntentFilter Filter { get; private set; }
        public BroadcastReceiver Receiver { get; private set; }
        public bool Broadcasting { get; set; }
        public bool Dead { get; set; }

        public ReceiverRecord(IntentFilter _filter, BroadcastReceiver _receiver)
        {
            Filter = _filter;
            Receiver = _receiver;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("Receiver{");
            builder.Append(Receiver);
            builder.Append(" filter=");
            builder.Append(Filter);
            if (Dead)
            {
                builder.Append(" DEAD");
            }
            builder.Append("}");
            return builder.ToString();
        }
    }

    private class BroadcastRecord
    {
        public Intent Intent { get; private set; }
        public List<ReceiverRecord> Receivers { get; private set; }

        public BroadcastRecord(Intent _intent, List<ReceiverRecord> _receivers)
        {
            Intent = _intent;
            Receivers = _receivers;
        }
    }

    public const int MsgExecPendingBroadcasts = 1;

    private static string Tag = typeof(InternalBroadcastManager).FullName;
    private static bool DebugMode = false;
    private static object lockObject = new object();
    private static InternalBroadcastManager instance;

    private Context appContext;
    private Dictionary<BroadcastReceiver, List<ReceiverRecord>> receiversDict = new Dictionary<BroadcastReceiver, List<ReceiverRecord>>();
    private Dictionary<string, List<ReceiverRecord>> actionsDict = new Dictionary<string, List<ReceiverRecord>>();
    private List<BroadcastRecord> pendingBroadcastList = new List<BroadcastRecord>();
    private Handler handler;

    public static InternalBroadcastManager GetInstance(Context context)
    {
        lock (lockObject)
        {
            if (instance == null)
            {
                instance = new InternalBroadcastManager(context.ApplicationContext);
            }
            return instance;
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
                case MsgExecPendingBroadcasts:
                    instance.ExecutePendingBroadcasts();
                    break;

                default:
                    base.HandleMessage(msg);
                    break;
            }
        }
    }

    private InternalBroadcastManager(Context context)
    {
        appContext = context;
        handler = new BroadcastHandler(context.MainLooper);
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
        lock(receiversDict)
        {
            ReceiverRecord entry = new ReceiverRecord(filter, receiver);
            receiversDict.TryGetValue(receiver, out List<ReceiverRecord> filters);
            if (filters == null)
            {
                filters = new List<ReceiverRecord>();
                receiversDict.Add(receiver, filters);
            }

            filters.Add(entry);
            for (int i = 0; i < filter.CountActions(); i++)
            {
                string action = filter.GetAction(i);
                if (action != null)
                {
                    actionsDict.TryGetValue(action, out List<ReceiverRecord> entries);
                    if (entries == null)
                    {
                        entries = new List<ReceiverRecord>();
                        actionsDict.Add(action, entries);
                    }
                    entries.Add(entry);
                }
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
        lock(receiversDict)
        {
            receiversDict.TryGetValue(receiver, out List<ReceiverRecord> filters);
            if (filters == null)
            {
                return;
            }

            receiversDict.Remove(receiver);
            for (int i = filters.Count - 1; i >= 0; i--)
            {
                ReceiverRecord filter = filters[i];
                filter.Dead = true;
                for (int j = 0; j < filter.Filter.CountActions(); j++)
                {
                    string action = filter.Filter.GetAction(j);
                    if (action != null)
                    {
                        actionsDict.TryGetValue(action, out List<ReceiverRecord> receivers);
                        if (receivers != null)
                        {
                            for (int k = receivers.Count - 1; k >= 0; k--)
                            {
                                ReceiverRecord rec = receivers[k];
                                if (rec.Receiver == receiver)
                                {
                                    rec.Dead = true;
                                    receivers.RemoveAt(k);
                                }
                            }
                            if (receivers.Count <= 0)
                            {
                                actionsDict.Remove(action);
                            }
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
        lock (receiversDict)
        {
            string action = intent.Action;
            string type = intent.ResolveTypeIfNeeded(appContext.ContentResolver);
            Uri data = intent.Data;
            string scheme = intent.Scheme;
            ICollection<string> categories = intent.Categories;
            bool debug = DebugMode || ((intent.Flags & ActivityFlags.DebugLogResolution) != 0);
            if (debug)
            {
                Log.Verbose(
                    Tag, "Resolving type " + type + " scheme " + scheme
                         + " of intent " + intent);
            }

            actionsDict.TryGetValue(intent.Action, out List<ReceiverRecord> entries);
            if (entries != null)
            {
                if (debug)
                {
                    Log.Verbose(Tag, "Action list: " + entries);
                }

                List<ReceiverRecord> receivers = null;
                foreach (ReceiverRecord receiver in entries)
                {
                    if (debug)
                    {
                        Log.Verbose(Tag, "Matching against filter " + receiver.Filter);
                    }

                    if (receiver.Broadcasting)
                    {
                        if (debug)
                        {
                            Log.Verbose(Tag, "  Filter's target already added");
                        }
                        continue;
                    }

                    MatchResults match = receiver.Filter.Match(action, type, scheme, data, categories, Tag);
                    if (match >= 0)
                    {
                        if (debug)
                        {
                            Log.Verbose(Tag, string.Format("  Filter matched!  match={0}", match));
                        }

                        if (receivers == null)
                        {
                            receivers = new List<ReceiverRecord>();
                        }

                        receivers.Add(receiver);
                        receiver.Broadcasting = true;
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

                            Log.Verbose(Tag, "  Filter did not match: " + reason);
                        }
                    }
                }

                if (receivers != null)
                {
                    foreach (ReceiverRecord receiver in receivers)
                    {
                        receiver.Broadcasting = false;
                    }

                    pendingBroadcastList.Add(new BroadcastRecord(intent, receivers));
                    if (!handler.HasMessages(MsgExecPendingBroadcasts))
                    {
                        handler.SendEmptyMessage(MsgExecPendingBroadcasts);
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
            lock(receiversDict)
            {
                int N = pendingBroadcastList.Count;
                if (N <= 0)
                {
                    return;
                }

                brs = new BroadcastRecord[N];
                pendingBroadcastList.CopyTo(brs);
                pendingBroadcastList.Clear();
            }

            foreach (BroadcastRecord br in brs)
            {
                foreach (ReceiverRecord rec in br.Receivers)
                {
                    if (!rec.Dead)
                    {
                        rec.Receiver.OnReceive(appContext, br.Intent);
                    }
                }
            }
        }
    }
}
