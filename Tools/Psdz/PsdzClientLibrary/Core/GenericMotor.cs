using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
    public class GenericMotor : INotifyPropertyChanged
    {
        private string engine1Field;
        private string engine2Field;
        private string engineLabel1Field;
        private string engineLabel2Field;
        public string Engine1
        {
            get
            {
                return engine1Field;
            }

            set
            {
                if (engine1Field != value)
                {
                    engine1Field = value;
                    OnPropertyChanged("Engine1");
                }
            }
        }

        public string Engine2
        {
            get
            {
                return engine2Field;
            }

            set
            {
                if (engine2Field != value)
                {
                    engine2Field = value;
                    OnPropertyChanged("Engine2");
                }
            }
        }

        public string EngineLabel1
        {
            get
            {
                return engineLabel1Field;
            }

            set
            {
                if (engineLabel1Field != value)
                {
                    engineLabel1Field = value;
                    OnPropertyChanged("EngineLabel1");
                }
            }
        }

        public string EngineLabel2
        {
            get
            {
                return engineLabel2Field;
            }

            set
            {
                if (engineLabel2Field != value)
                {
                    engineLabel2Field = value;
                    OnPropertyChanged("EngineLabel2");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged([CallerMemberName] string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}