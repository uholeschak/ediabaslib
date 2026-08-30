using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using PsdzClient.Programming;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using PsdzClient;

namespace BMW.Rheingold.CoreFramework
{
    [DataContract]
    public class ModuleData : IModule, INotifyPropertyChanged
    {
        [DataMember]
        private ModuleExecutionOrigin executedFrom;
        [DataMember]
        private InfoObject infoObj;
        [DataMember]
        private string infoType;
        [DataMember]
        private bool isActive;
        [DataMember]
        private bool isMinimizable;
        [DataMember]
        private ModuleExecutionStateType moduleState;
        [DataMember]
        private string name;
        [DataMember]
        private typeDiagObjectState stateField;
        [DataMember]
        private string title;
        [DataMember]
        private string visibleName;
        public bool IsActive
        {
            get
            {
                return isActive;
            }

            set
            {
                if (!object.Equals(isActive, value))
                {
                    isActive = value;
                    OnPropertyChanged("IsActive");
                }
            }
        }

        public bool IsExecutionCompleted
        {
            get
            {
                if (ModuleState != ModuleExecutionStateType.finished && ModuleState != ModuleExecutionStateType.error)
                {
                    return ModuleState == ModuleExecutionStateType.aborted;
                }

                return true;
            }
        }

        public ModuleExecutionStateType ModuleState
        {
            get
            {
                return moduleState;
            }

            set
            {
                if (!object.Equals(moduleState, value))
                {
                    moduleState = value;
                    OnPropertyChanged("ModuleState");
                }
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
                if (!object.Equals(title, value))
                {
                    title = value;
                    OnPropertyChanged("Title");
                }
            }
        }

        public InfoObject InfoObj
        {
            get
            {
                return infoObj;
            }

            private set
            {
                if (!object.Equals(infoObj, value))
                {
                    infoObj = value;
                    OnPropertyChanged("InfoObj");
                }
            }
        }

        public ModuleExecutionOrigin ExecutedFrom
        {
            get
            {
                return executedFrom;
            }

            set
            {
                if (!object.Equals(executedFrom, value))
                {
                    executedFrom = value;
                    OnPropertyChanged("ExecutedFrom");
                }
            }
        }

        public bool IsIdesModule
        {
            get
            {
                if (InfoObj != null)
                {
                //[-] decimal? nodeclass = InfoObj.XepInfoObject.Nodeclass;
                //[-] decimal num = 41153666;
                //[-] return (nodeclass.GetValueOrDefault() == num) & nodeclass.HasValue;
                }

                return false;
            }
        }

        public bool IsModuleExecutionRunning
        {
            get
            {
                if (ModuleState != ModuleExecutionStateType.idle)
                {
                    return ModuleState == ModuleExecutionStateType.running;
                }

                return true;
            }
        }

        public string VisibleName
        {
            get
            {
                return visibleName;
            }

            set
            {
                if (!object.Equals(visibleName, value))
                {
                    visibleName = value;
                    OnPropertyChanged("VisibleName");
                }
            }
        }

        public typeDiagObjectState Status
        {
            get
            {
                return stateField;
            }

            set
            {
                if (!stateField.Equals(value))
                {
                    stateField = value;
                    InfoObject infoObject = infoObj;
                    if (infoObject != null)
                    {
                        infoObject.State = value;
                    }

                    OnPropertyChanged("Status");
                }
            }
        }

        public string Name
        {
            get
            {
                return name;
            }

            private set
            {
                if (!object.Equals(name, value))
                {
                    name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public bool IsMinimizable
        {
            get
            {
                return isMinimizable;
            }

            set
            {
                if (!object.Equals(isMinimizable, value))
                {
                    isMinimizable = value;
                    OnPropertyChanged("IsMinimizable");
                }
            }
        }

        public bool IsModuleExecutionMinimized
        {
            get
            {
                InfoObject infoObject = infoObj;
                if (infoObject != null)
                {
                    return infoObject.State == typeDiagObjectState.Minimized;
                }

                return false;
            }
        }

        public string InfoType
        {
            get
            {
                return infoType;
            }

            set
            {
                if (!object.Equals(infoType, value))
                {
                    infoType = value;
                    OnPropertyChanged("InfoType");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        [PreserveSource(Cleaned = true)]
        public static InfoObject CreateHelperInfoObject(string moduleName)
        {
            return new InfoObject();
        }

        public static ModuleData CreateModuleDataFromModuleName(string moduleName)
        {
            return new ModuleData(CreateHelperInfoObject(moduleName));
        }

        [PreserveSource(Cleaned = true)]
        public ModuleData(IXepInfoObject xepInfoObject) : this(new InfoObject())
        {
        }

        public ModuleData(InfoObject infoObject)
        {
            if (infoObject == null)
            {
                throw new ArgumentException("infoObject");
            }

            //[-] if (infoObject.XepInfoObject == null)
            //[-] {
            //[-] throw new ArgumentException("XepInfoObject");
            //[-] }
            InfoObj = infoObject;
            //[-] Name = infoObj.Title ?? infoObj.XepInfoObject.Identifikator;
            //[+] Name = infoObj.Title ?? string.Empty;
            Name = infoObj.Title ?? string.Empty;
            InfoType = "ABL";
            IsMinimizable = true;
            VisibleName = Name;
            Status = typeDiagObjectState.NotCalled;
            ModuleState = ModuleExecutionStateType.created;
            IsActive = true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}