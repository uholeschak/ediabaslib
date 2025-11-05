using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace PsdzClient.Core
{
    public class typeBasicFeatures : INotifyPropertyChanged
    {
        private string baureiheField;

        private string ereiheField;

        private string karosserieField;

        private string verkaufsBezeichnungField;

        private string motorField;

        private string getriebeField;

        private string countryOfAssemblyField;

        private string baseVersionField;

        private string landField;

        private string lenkungField;

        private string modelljahrField;

        private string modellmonatField;

        private string markeField;

        private string typeCodeField;

        private string prodartField;

        private string eMotBaureiheField;

        private string aEKurzbezeichnungField;

        private static XmlSerializer serializer;

        public string Baureihe
        {
            get
            {
                return baureiheField;
            }
            set
            {
                if (baureiheField != null)
                {
                    if (!baureiheField.Equals(value))
                    {
                        baureiheField = value;
                        OnPropertyChanged("Baureihe");
                    }
                }
                else
                {
                    baureiheField = value;
                    OnPropertyChanged("Baureihe");
                }
            }
        }

        public string Ereihe
        {
            get
            {
                return ereiheField;
            }
            set
            {
                if (ereiheField != null)
                {
                    if (!ereiheField.Equals(value))
                    {
                        ereiheField = value;
                        OnPropertyChanged("Ereihe");
                    }
                }
                else
                {
                    ereiheField = value;
                    OnPropertyChanged("Ereihe");
                }
            }
        }

        public string Karosserie
        {
            get
            {
                return karosserieField;
            }
            set
            {
                if (karosserieField != null)
                {
                    if (!karosserieField.Equals(value))
                    {
                        karosserieField = value;
                        OnPropertyChanged("Karosserie");
                    }
                }
                else
                {
                    karosserieField = value;
                    OnPropertyChanged("Karosserie");
                }
            }
        }

        public string VerkaufsBezeichnung
        {
            get
            {
                return verkaufsBezeichnungField;
            }
            set
            {
                if (verkaufsBezeichnungField != null)
                {
                    if (!verkaufsBezeichnungField.Equals(value))
                    {
                        verkaufsBezeichnungField = value;
                        OnPropertyChanged("VerkaufsBezeichnung");
                    }
                }
                else
                {
                    verkaufsBezeichnungField = value;
                    OnPropertyChanged("VerkaufsBezeichnung");
                }
            }
        }

        public string Motor
        {
            get
            {
                return motorField;
            }
            set
            {
                if (motorField != null)
                {
                    if (!motorField.Equals(value))
                    {
                        motorField = value;
                        OnPropertyChanged("Motor");
                    }
                }
                else
                {
                    motorField = value;
                    OnPropertyChanged("Motor");
                }
            }
        }

        public string MotorLabel
        {
            get
            {
                if (!(motorField == string.Empty) && !(motorField == "-"))
                {
                    return motorField;
                }
                return EMotBaureihe;
            }
        }

        public string Getriebe
        {
            get
            {
                return getriebeField;
            }
            set
            {
                if (getriebeField != null)
                {
                    if (!getriebeField.Equals(value))
                    {
                        getriebeField = value;
                        OnPropertyChanged("Getriebe");
                    }
                }
                else
                {
                    getriebeField = value;
                    OnPropertyChanged("Getriebe");
                }
            }
        }

        public string CountryOfAssembly
        {
            get
            {
                return countryOfAssemblyField;
            }
            set
            {
                if (countryOfAssemblyField != null)
                {
                    if (!countryOfAssemblyField.Equals(value))
                    {
                        countryOfAssemblyField = value;
                        OnPropertyChanged("CountryOfAssembly");
                    }
                }
                else
                {
                    countryOfAssemblyField = value;
                    OnPropertyChanged("CountryOfAssembly");
                }
            }
        }

        public string BaseVersion
        {
            get
            {
                return baseVersionField;
            }
            set
            {
                if (baseVersionField != null)
                {
                    if (!baseVersionField.Equals(value))
                    {
                        baseVersionField = value;
                        OnPropertyChanged("BaseVersion");
                    }
                }
                else
                {
                    baseVersionField = value;
                    OnPropertyChanged("BaseVersion");
                }
            }
        }

        public string Land
        {
            get
            {
                return landField;
            }
            set
            {
                if (landField != null)
                {
                    if (!landField.Equals(value))
                    {
                        landField = value;
                        OnPropertyChanged("Land");
                    }
                }
                else
                {
                    landField = value;
                    OnPropertyChanged("Land");
                }
            }
        }

        public string Lenkung
        {
            get
            {
                return lenkungField;
            }
            set
            {
                if (lenkungField != null)
                {
                    if (!lenkungField.Equals(value))
                    {
                        lenkungField = value;
                        OnPropertyChanged("Lenkung");
                    }
                }
                else
                {
                    lenkungField = value;
                    OnPropertyChanged("Lenkung");
                }
            }
        }

        public string Modelljahr
        {
            get
            {
                return modelljahrField;
            }
            set
            {
                if (modelljahrField != null)
                {
                    if (!modelljahrField.Equals(value))
                    {
                        modelljahrField = value;
                        OnPropertyChanged("Modelljahr");
                    }
                }
                else
                {
                    modelljahrField = value;
                    OnPropertyChanged("Modelljahr");
                }
            }
        }

        public string Modellmonat
        {
            get
            {
                return modellmonatField;
            }
            set
            {
                if (modellmonatField != null)
                {
                    if (!modellmonatField.Equals(value))
                    {
                        modellmonatField = value;
                        OnPropertyChanged("Modellmonat");
                    }
                }
                else
                {
                    modellmonatField = value;
                    OnPropertyChanged("Modellmonat");
                }
            }
        }

        public string Marke
        {
            get
            {
                return markeField;
            }
            set
            {
                if (markeField != null)
                {
                    if (!markeField.Equals(value))
                    {
                        markeField = value;
                        OnPropertyChanged("Marke");
                    }
                }
                else
                {
                    markeField = value;
                    OnPropertyChanged("Marke");
                }
            }
        }

        public string TypeCode
        {
            get
            {
                return typeCodeField;
            }
            set
            {
                if (typeCodeField != null)
                {
                    if (!typeCodeField.Equals(value))
                    {
                        typeCodeField = value;
                        OnPropertyChanged("TypeCode");
                    }
                }
                else
                {
                    typeCodeField = value;
                    OnPropertyChanged("TypeCode");
                }
            }
        }

        public string Prodart
        {
            get
            {
                return prodartField;
            }
            set
            {
                if (prodartField != null)
                {
                    if (!prodartField.Equals(value))
                    {
                        prodartField = value;
                        OnPropertyChanged("Prodart");
                    }
                }
                else
                {
                    prodartField = value;
                    OnPropertyChanged("Prodart");
                }
            }
        }

        public string EMotBaureihe
        {
            get
            {
                return eMotBaureiheField;
            }
            set
            {
                if (eMotBaureiheField != null)
                {
                    if (!eMotBaureiheField.Equals(value))
                    {
                        eMotBaureiheField = value;
                        OnPropertyChanged("EMotBaureihe");
                    }
                }
                else
                {
                    eMotBaureiheField = value;
                    OnPropertyChanged("EMotBaureihe");
                }
            }
        }

        public string AEKurzbezeichnung
        {
            get
            {
                return aEKurzbezeichnungField;
            }
            set
            {
                if (aEKurzbezeichnungField != null)
                {
                    if (!aEKurzbezeichnungField.Equals(value))
                    {
                        aEKurzbezeichnungField = value;
                        OnPropertyChanged("AEKurzbezeichnung");
                    }
                }
                else
                {
                    aEKurzbezeichnungField = value;
                    OnPropertyChanged("AEKurzbezeichnung");
                }
            }
        }

        private static XmlSerializer Serializer
        {
            get
            {
                if (serializer == null)
                {
                    serializer = new XmlSerializer(typeof(typeBasicFeatures));
                }
                return serializer;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public typeBasicFeatures()
        {
            lenkungField = "LL";
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual string Serialize()
        {
            StreamReader streamReader = null;
            MemoryStream memoryStream = null;
            try
            {
                memoryStream = new MemoryStream();
                Serializer.Serialize(memoryStream, this);
                memoryStream.Seek(0L, SeekOrigin.Begin);
                streamReader = new StreamReader(memoryStream);
                return streamReader.ReadToEnd();
            }
            finally
            {
                streamReader?.Dispose();
                memoryStream?.Dispose();
            }
        }

        public static bool Deserialize(string xml, out typeBasicFeatures obj, out Exception exception)
        {
            exception = null;
            obj = null;
            try
            {
                obj = Deserialize(xml);
                return true;
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }
        }

        public static bool Deserialize(string xml, out typeBasicFeatures obj)
        {
            Exception exception = null;
            return Deserialize(xml, out obj, out exception);
        }

        public static typeBasicFeatures Deserialize(string xml)
        {
            StringReader stringReader = null;
            try
            {
                stringReader = new StringReader(xml);
                return (typeBasicFeatures)Serializer.Deserialize(XmlReader.Create(stringReader));
            }
            finally
            {
                stringReader?.Dispose();
            }
        }

        public virtual bool SaveToFile(string fileName, out Exception exception)
        {
            exception = null;
            try
            {
                SaveToFile(fileName);
                return true;
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }
        }

        public virtual void SaveToFile(string fileName)
        {
            StreamWriter streamWriter = null;
            try
            {
                string value = Serialize();
                streamWriter = new FileInfo(fileName).CreateText();
                streamWriter.WriteLine(value);
                streamWriter.Close();
            }
            finally
            {
                streamWriter?.Dispose();
            }
        }

        public static bool LoadFromFile(string fileName, out typeBasicFeatures obj, out Exception exception)
        {
            exception = null;
            obj = null;
            try
            {
                obj = LoadFromFile(fileName);
                return true;
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }
        }

        public static bool LoadFromFile(string fileName, out typeBasicFeatures obj)
        {
            Exception exception = null;
            return LoadFromFile(fileName, out obj, out exception);
        }

        public static typeBasicFeatures LoadFromFile(string fileName)
        {
            FileStream fileStream = null;
            StreamReader streamReader = null;
            try
            {
                fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                streamReader = new StreamReader(fileStream);
                string xml = streamReader.ReadToEnd();
                streamReader.Close();
                fileStream.Close();
                return Deserialize(xml);
            }
            finally
            {
                fileStream?.Dispose();
                streamReader?.Dispose();
            }
        }
    }
}
