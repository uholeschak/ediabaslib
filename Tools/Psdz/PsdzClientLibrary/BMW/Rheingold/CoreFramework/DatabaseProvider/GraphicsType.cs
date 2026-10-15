using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class GraphicsType : INotifyPropertyChanged
    {
        private static List<string> supportedFormat = new List<string>
        {
            "png",
            "jpg",
            "mp3",
            "wmv",
            "mp4",
            "svg"
        };
        private const string DefaultFormat = "png";
        private string nameField;
        private string typeField;
        private string linkIdField;
        private byte[] graphicField;
        private bool ctordoneField;
        public string Name
        {
            get
            {
                return nameField;
            }

            set
            {
                if (nameField != null)
                {
                    if (!nameField.Equals(value))
                    {
                        nameField = value;
                        OnPropertyChanged("Name");
                    }
                }
                else
                {
                    nameField = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public string Type
        {
            get
            {
                return typeField;
            }

            set
            {
                if (typeField != null)
                {
                    if (!typeField.Equals(value))
                    {
                        typeField = value;
                        OnPropertyChanged("Type");
                    }
                }
                else
                {
                    typeField = value;
                    OnPropertyChanged("Type");
                }
            }
        }

        public string LinkId
        {
            get
            {
                return linkIdField;
            }

            set
            {
                if (linkIdField != null)
                {
                    if (!linkIdField.Equals(value))
                    {
                        linkIdField = value;
                        OnPropertyChanged("LinkId");
                    }
                }
                else
                {
                    linkIdField = value;
                    OnPropertyChanged("LinkId");
                }
            }
        }

        public byte[] Graphic
        {
            get
            {
                return graphicField;
            }

            set
            {
                if (graphicField != null)
                {
                    if (!graphicField.Equals(value))
                    {
                        graphicField = value;
                        OnPropertyChanged("Graphic");
                    }
                }
                else
                {
                    graphicField = value;
                    OnPropertyChanged("Graphic");
                }
            }
        }

        [DefaultValue(true)]
        public bool ctordone
        {
            get
            {
                return ctordoneField;
            }

            set
            {
                if (!ctordoneField.Equals(value))
                {
                    ctordoneField = value;
                    OnPropertyChanged("ctordone");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public static string GetSuffixByFormat(string fileformat)
        {
            if (string.IsNullOrEmpty(fileformat))
            {
                return "png";
            }

            string text = supportedFormat.FirstOrDefault((string f) => fileformat.ToLower().Contains(f));
            if (text != null)
            {
                return text;
            }

            return "png";
        }

        public GraphicsType()
        {
            typeField = "UNK";
            ctordoneField = true;
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}