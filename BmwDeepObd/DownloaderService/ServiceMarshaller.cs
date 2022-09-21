// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServiceMarshaller.cs" company="Matthew Leibowitz">
//   Copyright (c) Matthew Leibowitz
//   This code is licensed under the Apache 2.0 License
//   http://www.apache.org/licenses/LICENSE-2.0.html
// </copyright>
// <summary>
//   Used by the client activity to proxy requests to the DownloaderService.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Google.Android.Vending.Expansion.Downloader;

namespace ExpansionDownloader.Service
{
    using Android.Content;
    using Android.OS;

    /// <summary>
    /// Used by the client activity to proxy requests to the DownloaderService.
    /// </summary>
    /// <remarks>
    /// Most importantly, you must call <see cref="CreateProxy"/> during the 
    /// <see cref="IDownloaderClient.OnServiceConnected"/> callback in your 
    /// activity in order to instantiate an <see cref="IDownloaderService"/>
    /// object that you can then use to issue commands to the
    /// <see cref="DownloaderService"/> (such as to pause and resume downloads).
    /// </remarks>
    public static class ServiceMarshaller
    {
        #region Public Methods and Operators

        /// <summary>
        /// Returns a proxy that will marshall calls to IDownloaderService methods
        /// </summary>
        /// <param name="messenger">
        /// The messenger.
        /// </param>
        /// <returns>
        /// A proxy that will marshall calls to IDownloaderService methods
        /// </returns>
        public static IDownloaderService CreateProxy(Messenger messenger)
        {
            return new Proxy(messenger);
        }

        /// <summary>
        /// Returns a stub object that, when connected, will listen for 
        /// marshalled IDownloaderService methods and translate them into calls 
        /// to the supplied interface.
        /// </summary>
        /// <param name="itf">
        /// An implementation of IDownloaderService that will be called when
        /// remote method calls are unmarshalled.
        /// </param>
        /// <returns>
        /// A stub that will listen for marshalled IDownloaderService methods.
        /// </returns>
        public static IDownloaderServiceConnection CreateStub(IDownloaderService itf)
        {
            return new DownloaderServiceConnection(itf);
        }

        #endregion

        /// <summary>
        /// The downloader service connection.
        /// </summary>
        private class DownloaderServiceConnection : IDownloaderServiceConnection
        {
            #region Fields

            /// <summary>
            /// The downloader service.
            /// </summary>
            private readonly IDownloaderService downloaderService;

            /// <summary>
            /// The messenger.
            /// </summary>
            private readonly Messenger messenger;

            #endregion

            #region Constructors and Destructors

            /// <summary>
            /// Initializes a new instance of the <see cref="DownloaderServiceConnection"/> class.
            /// </summary>
            /// <param name="downloaderService">
            /// The downloader service.
            /// </param>
            public DownloaderServiceConnection(IDownloaderService downloaderService)
            {
                var handler = new Handler(this.SendMessage);
                this.messenger = new Messenger(handler);
                this.downloaderService = downloaderService;
            }

            #endregion

            #region Public Methods and Operators

            /// <summary>
            /// The connect.
            /// </summary>
            /// <param name="context">
            /// The context.
            /// </param>
            public void Connect(Context context)
            {
            }

            /// <summary>
            /// The disconnect.
            /// </summary>
            /// <param name="context">
            /// The context.
            /// </param>
            public void Disconnect(Context context)
            {
            }

            /// <summary>
            /// Returns a messenger.
            /// </summary>
            /// <returns>
            /// The messenger
            /// </returns>
            public Messenger GetMessenger()
            {
                return this.messenger;
            }

            #endregion

            #region Methods

            /// <summary>
            /// The send message.
            /// </summary>
            /// <param name="message">
            /// The message.
            /// </param>
            private void SendMessage(Message message)
            {
                switch ((ServiceMessages)message.What)
                {
                    case ServiceMessages.RequestAbortDownload:
                        this.downloaderService.RequestAbortDownload();
                        break;
                    case ServiceMessages.RequestContinueDownload:
                        this.downloaderService.RequestContinueDownload();
                        break;
                    case ServiceMessages.RequestPauseDownload:
                        this.downloaderService.RequestPauseDownload();
                        break;
                    case ServiceMessages.SetDownloadFlags:
                        var flags = (ServiceFlags)message.Data.GetInt(ServiceParameters.Flags);
                        this.downloaderService.SetDownloadFlags(flags);
                        break;
                    case ServiceMessages.RequestDownloadState:
                        this.downloaderService.RequestDownloadStatus();
                        break;
                    case ServiceMessages.RequestClientUpdate:
                        var m = (Messenger)message.Data.GetParcelable(ServiceParameters.Messenger);
                        this.downloaderService.OnClientUpdated(m);
                        break;
                }
            }

            #endregion
        }

        /// <summary>
        /// The proxy.
        /// </summary>
        private class Proxy : IDownloaderService
        {
            #region Fields

            /// <summary>
            /// The messenger.
            /// </summary>
            private readonly Messenger messenger;

            #endregion

            #region Constructors and Destructors

            /// <summary>
            /// Initializes a new instance of the <see cref="Proxy"/> class.
            /// </summary>
            /// <param name="msg">
            /// The msg.
            /// </param>
            public Proxy(Messenger msg)
            {
                this.messenger = msg;
            }

            #endregion

            #region Public Methods and Operators

            /// <summary>
            /// The on client updated.
            /// </summary>
            /// <param name="clientMessenger">
            /// The client messenger.
            /// </param>
            public void OnClientUpdated(Messenger clientMessenger)
            {
                using (var bundle = new Bundle(1))
                {
                    bundle.PutParcelable(ServiceParameters.Messenger, clientMessenger);
                    this.Send(ServiceMessages.RequestClientUpdate, bundle);
                }
            }

            /// <summary>
            /// The request abort download.
            /// </summary>
            public void RequestAbortDownload()
            {
                using (var bundle = new Bundle())
                {
                    this.Send(ServiceMessages.RequestAbortDownload, bundle);
                }
            }

            /// <summary>
            /// The request continue download.
            /// </summary>
            public void RequestContinueDownload()
            {
                using (var bundle = new Bundle())
                {
                    this.Send(ServiceMessages.RequestContinueDownload, bundle);
                }
            }

            /// <summary>
            /// The request download status.
            /// </summary>
            public void RequestDownloadStatus()
            {
                using (var bundle = new Bundle())
                {
                    this.Send(ServiceMessages.RequestDownloadState, bundle);
                }
            }

            /// <summary>
            /// The request pause download.
            /// </summary>
            public void RequestPauseDownload()
            {
                using (var bundle = new Bundle())
                {
                    this.Send(ServiceMessages.RequestPauseDownload, bundle);
                }
            }

            /// <summary>
            /// The set download flags.
            /// </summary>
            /// <param name="flags">
            /// The flags.
            /// </param>
            public void SetDownloadFlags(ServiceFlags flags)
            {
                using (var p = new Bundle())
                {
                    p.PutInt(ServiceParameters.Flags, (int)flags);
                    this.Send(ServiceMessages.SetDownloadFlags, p);
                }
            }

            #endregion

            #region Methods

            /// <summary>
            /// The send.
            /// </summary>
            /// <param name="method">
            /// The method.
            /// </param>
            /// <param name="p">
            /// The p.
            /// </param>
            private void Send(ServiceMessages method, Bundle p)
            {
                Message m = Message.Obtain(null, (int)method);
                m.Data = p;
                try
                {
                    this.messenger.Send(m);
                }
                catch (RemoteException e)
                {
                    e.PrintStackTrace();
                }
            }

            #endregion
        }
    }
}