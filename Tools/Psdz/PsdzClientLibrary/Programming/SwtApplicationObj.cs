using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using PsdzClient.Programming;

namespace BMW.Rheingold.Programming.API
{
    [DataContract]
    internal class SwtApplicationObj : ISwtApplication, ISwtApplicationReport, INotifyPropertyChanged
    {
        [DataMember]
        private int diagAddrAsInt;
        [DataMember]
        private byte[] fsc;
        [DataMember]
        private byte[] fscCertificate;
        [DataMember]
        private FscCertificateState fscCertificateState;
        [DataMember]
        private FscState fscState;
        [DataMember]
        private ISwtApplicationId id;
        [DataMember]
        private string title;
        [DataMember]
        private bool isBackupPossible;
        [DataMember]
        private int position;
        [DataMember]
        private SwtActionType? swtActionType;
        [DataMember]
        private SwtType swtType;
        public int DiagAddrAsInt
        {
            get
            {
                return diagAddrAsInt;
            }

            set
            {
                diagAddrAsInt = value;
                OnPropertyChanged("DiagAddrAsInt");
            }
        }

        public byte[] Fsc
        {
            get
            {
                return fsc;
            }

            internal set
            {
                fsc = value;
                OnPropertyChanged("Fsc");
            }
        }

        public byte[] FscCertificate
        {
            get
            {
                return fscCertificate;
            }

            internal set
            {
                fscCertificate = value;
                OnPropertyChanged("FscCertificate");
            }
        }

        public FscCertificateState FscCertificateState
        {
            get
            {
                return fscCertificateState;
            }

            internal set
            {
                fscCertificateState = value;
                OnPropertyChanged("FscCertificateState");
            }
        }

        public FscState FscState
        {
            get
            {
                return fscState;
            }

            internal set
            {
                fscState = value;
                OnPropertyChanged("FscState");
            }
        }

        public ISwtApplicationId Id
        {
            get
            {
                return id;
            }

            private set
            {
                id = value;
                OnPropertyChanged("Id");
            }
        }

        public string Title
        {
            get
            {
                return title;
            }

            set
            {
                title = value;
                OnPropertyChanged("Title");
            }
        }

        public IDictionary<string, string> TitleDictionary { get; set; }

        public bool IsBackupPossible
        {
            get
            {
                return isBackupPossible;
            }

            internal set
            {
                isBackupPossible = value;
                OnPropertyChanged("IsBackupPossible");
            }
        }

        public int Position
        {
            get
            {
                return position;
            }

            internal set
            {
                position = value;
                OnPropertyChanged("Position");
            }
        }

        public SwtActionType? SwtActionType
        {
            get
            {
                return swtActionType;
            }

            internal set
            {
                swtActionType = value;
                OnPropertyChanged("SwtActionType");
            }
        }

        public SwtType SwtType
        {
            get
            {
                return swtType;
            }

            internal set
            {
                swtType = value;
                OnPropertyChanged("SwtType");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        internal SwtApplicationObj(ISwtApplicationId id)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }

            Id = id;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}