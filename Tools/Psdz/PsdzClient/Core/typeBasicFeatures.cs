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
		public typeBasicFeatures()
		{
			this.lenkungField = "LL";
		}

		public string Baureihe
		{
			get
			{
				return this.baureiheField;
			}
			set
			{
				if (this.baureiheField != null)
				{
					if (!this.baureiheField.Equals(value))
					{
						this.baureiheField = value;
						this.OnPropertyChanged("Baureihe");
						return;
					}
				}
				else
				{
					this.baureiheField = value;
					this.OnPropertyChanged("Baureihe");
				}
			}
		}

		public string Ereihe
		{
			get
			{
				return this.ereiheField;
			}
			set
			{
				if (this.ereiheField != null)
				{
					if (!this.ereiheField.Equals(value))
					{
						this.ereiheField = value;
						this.OnPropertyChanged("Ereihe");
						return;
					}
				}
				else
				{
					this.ereiheField = value;
					this.OnPropertyChanged("Ereihe");
				}
			}
		}

		public string Karosserie
		{
			get
			{
				return this.karosserieField;
			}
			set
			{
				if (this.karosserieField != null)
				{
					if (!this.karosserieField.Equals(value))
					{
						this.karosserieField = value;
						this.OnPropertyChanged("Karosserie");
						return;
					}
				}
				else
				{
					this.karosserieField = value;
					this.OnPropertyChanged("Karosserie");
				}
			}
		}

		public string VerkaufsBezeichnung
		{
			get
			{
				return this.verkaufsBezeichnungField;
			}
			set
			{
				if (this.verkaufsBezeichnungField != null)
				{
					if (!this.verkaufsBezeichnungField.Equals(value))
					{
						this.verkaufsBezeichnungField = value;
						this.OnPropertyChanged("VerkaufsBezeichnung");
						return;
					}
				}
				else
				{
					this.verkaufsBezeichnungField = value;
					this.OnPropertyChanged("VerkaufsBezeichnung");
				}
			}
		}

		public string Motor
		{
			get
			{
				return this.motorField;
			}
			set
			{
				if (this.motorField != null)
				{
					if (!this.motorField.Equals(value))
					{
						this.motorField = value;
						this.OnPropertyChanged("Motor");
						return;
					}
				}
				else
				{
					this.motorField = value;
					this.OnPropertyChanged("Motor");
				}
			}
		}

		public string MotorLabel
		{
			get
			{
				if (!(this.motorField == string.Empty) && !(this.motorField == "-"))
				{
					return this.motorField;
				}
				return this.EMotBaureihe;
			}
		}

		public string Getriebe
		{
			get
			{
				return this.getriebeField;
			}
			set
			{
				if (this.getriebeField != null)
				{
					if (!this.getriebeField.Equals(value))
					{
						this.getriebeField = value;
						this.OnPropertyChanged("Getriebe");
						return;
					}
				}
				else
				{
					this.getriebeField = value;
					this.OnPropertyChanged("Getriebe");
				}
			}
		}

		public string CountryOfAssembly
		{
			get
			{
				return this.countryOfAssemblyField;
			}
			set
			{
				if (this.countryOfAssemblyField != null)
				{
					if (!this.countryOfAssemblyField.Equals(value))
					{
						this.countryOfAssemblyField = value;
						this.OnPropertyChanged("CountryOfAssembly");
						return;
					}
				}
				else
				{
					this.countryOfAssemblyField = value;
					this.OnPropertyChanged("CountryOfAssembly");
				}
			}
		}

		public string BaseVersion
		{
			get
			{
				return this.baseVersionField;
			}
			set
			{
				if (this.baseVersionField != null)
				{
					if (!this.baseVersionField.Equals(value))
					{
						this.baseVersionField = value;
						this.OnPropertyChanged("BaseVersion");
						return;
					}
				}
				else
				{
					this.baseVersionField = value;
					this.OnPropertyChanged("BaseVersion");
				}
			}
		}

		public string Land
		{
			get
			{
				return this.landField;
			}
			set
			{
				if (this.landField != null)
				{
					if (!this.landField.Equals(value))
					{
						this.landField = value;
						this.OnPropertyChanged("Land");
						return;
					}
				}
				else
				{
					this.landField = value;
					this.OnPropertyChanged("Land");
				}
			}
		}

		public string Lenkung
		{
			get
			{
				return this.lenkungField;
			}
			set
			{
				if (this.lenkungField != null)
				{
					if (!this.lenkungField.Equals(value))
					{
						this.lenkungField = value;
						this.OnPropertyChanged("Lenkung");
						return;
					}
				}
				else
				{
					this.lenkungField = value;
					this.OnPropertyChanged("Lenkung");
				}
			}
		}

		public string Modelljahr
		{
			get
			{
				return this.modelljahrField;
			}
			set
			{
				if (this.modelljahrField != null)
				{
					if (!this.modelljahrField.Equals(value))
					{
						this.modelljahrField = value;
						this.OnPropertyChanged("Modelljahr");
						return;
					}
				}
				else
				{
					this.modelljahrField = value;
					this.OnPropertyChanged("Modelljahr");
				}
			}
		}

		public string Modellmonat
		{
			get
			{
				return this.modellmonatField;
			}
			set
			{
				if (this.modellmonatField != null)
				{
					if (!this.modellmonatField.Equals(value))
					{
						this.modellmonatField = value;
						this.OnPropertyChanged("Modellmonat");
						return;
					}
				}
				else
				{
					this.modellmonatField = value;
					this.OnPropertyChanged("Modellmonat");
				}
			}
		}

		public string Marke
		{
			get
			{
				return this.markeField;
			}
			set
			{
				if (this.markeField != null)
				{
					if (!this.markeField.Equals(value))
					{
						this.markeField = value;
						this.OnPropertyChanged("Marke");
						return;
					}
				}
				else
				{
					this.markeField = value;
					this.OnPropertyChanged("Marke");
				}
			}
		}

		public string TypeCode
		{
			get
			{
				return this.typeCodeField;
			}
			set
			{
				if (this.typeCodeField != null)
				{
					if (!this.typeCodeField.Equals(value))
					{
						this.typeCodeField = value;
						this.OnPropertyChanged("TypeCode");
						return;
					}
				}
				else
				{
					this.typeCodeField = value;
					this.OnPropertyChanged("TypeCode");
				}
			}
		}

		public string Prodart
		{
			get
			{
				return this.prodartField;
			}
			set
			{
				if (this.prodartField != null)
				{
					if (!this.prodartField.Equals(value))
					{
						this.prodartField = value;
						this.OnPropertyChanged("Prodart");
						return;
					}
				}
				else
				{
					this.prodartField = value;
					this.OnPropertyChanged("Prodart");
				}
			}
		}

		public string EMotBaureihe
		{
			get
			{
				return this.eMotBaureiheField;
			}
			set
			{
				if (this.eMotBaureiheField != null)
				{
					if (!this.eMotBaureiheField.Equals(value))
					{
						this.eMotBaureiheField = value;
						this.OnPropertyChanged("EMotBaureihe");
						return;
					}
				}
				else
				{
					this.eMotBaureiheField = value;
					this.OnPropertyChanged("EMotBaureihe");
				}
			}
		}

		public string AEKurzbezeichnung
		{
			get
			{
				return this.aEKurzbezeichnungField;
			}
			set
			{
				if (this.aEKurzbezeichnungField != null)
				{
					if (!this.aEKurzbezeichnungField.Equals(value))
					{
						this.aEKurzbezeichnungField = value;
						this.OnPropertyChanged("AEKurzbezeichnung");
						return;
					}
				}
				else
				{
					this.aEKurzbezeichnungField = value;
					this.OnPropertyChanged("AEKurzbezeichnung");
				}
			}
		}

		private static XmlSerializer Serializer
		{
			get
			{
				if (typeBasicFeatures.serializer == null)
				{
					typeBasicFeatures.serializer = new XmlSerializer(typeof(typeBasicFeatures));
				}
				return typeBasicFeatures.serializer;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
			if (propertyChanged != null)
			{
				propertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public virtual string Serialize()
		{
			StreamReader streamReader = null;
			MemoryStream memoryStream = null;
			string result;
			try
			{
				memoryStream = new MemoryStream();
				typeBasicFeatures.Serializer.Serialize(memoryStream, this);
				memoryStream.Seek(0L, SeekOrigin.Begin);
				streamReader = new StreamReader(memoryStream);
				result = streamReader.ReadToEnd();
			}
			finally
			{
				if (streamReader != null)
				{
					streamReader.Dispose();
				}
				if (memoryStream != null)
				{
					memoryStream.Dispose();
				}
			}
			return result;
		}

		public static bool Deserialize(string xml, out typeBasicFeatures obj, out Exception exception)
		{
			exception = null;
			obj = null;
			bool result;
			try
			{
				obj = typeBasicFeatures.Deserialize(xml);
				result = true;
			}
			catch (Exception ex)
			{
				exception = ex;
				result = false;
			}
			return result;
		}

		public static bool Deserialize(string xml, out typeBasicFeatures obj)
		{
			Exception ex = null;
			return typeBasicFeatures.Deserialize(xml, out obj, out ex);
		}

		public static typeBasicFeatures Deserialize(string xml)
		{
			StringReader stringReader = null;
			typeBasicFeatures result;
			try
			{
				stringReader = new StringReader(xml);
				result = (typeBasicFeatures)typeBasicFeatures.Serializer.Deserialize(XmlReader.Create(stringReader));
			}
			finally
			{
				if (stringReader != null)
				{
					stringReader.Dispose();
				}
			}
			return result;
		}

		public virtual bool SaveToFile(string fileName, out Exception exception)
		{
			exception = null;
			bool result;
			try
			{
				this.SaveToFile(fileName);
				result = true;
			}
			catch (Exception ex)
			{
				exception = ex;
				result = false;
			}
			return result;
		}

		public virtual void SaveToFile(string fileName)
		{
			StreamWriter streamWriter = null;
			try
			{
				string value = this.Serialize();
				streamWriter = new FileInfo(fileName).CreateText();
				streamWriter.WriteLine(value);
				streamWriter.Close();
			}
			finally
			{
				if (streamWriter != null)
				{
					streamWriter.Dispose();
				}
			}
		}

		public static bool LoadFromFile(string fileName, out typeBasicFeatures obj, out Exception exception)
		{
			exception = null;
			obj = null;
			bool result;
			try
			{
				obj = typeBasicFeatures.LoadFromFile(fileName);
				result = true;
			}
			catch (Exception ex)
			{
				exception = ex;
				result = false;
			}
			return result;
		}

		public static bool LoadFromFile(string fileName, out typeBasicFeatures obj)
		{
			Exception ex = null;
			return typeBasicFeatures.LoadFromFile(fileName, out obj, out ex);
		}

		public static typeBasicFeatures LoadFromFile(string fileName)
		{
			FileStream fileStream = null;
			StreamReader streamReader = null;
			typeBasicFeatures result;
			try
			{
				fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
				streamReader = new StreamReader(fileStream);
				string xml = streamReader.ReadToEnd();
				streamReader.Close();
				fileStream.Close();
				result = typeBasicFeatures.Deserialize(xml);
			}
			finally
			{
				if (fileStream != null)
				{
					fileStream.Dispose();
				}
				if (streamReader != null)
				{
					streamReader.Dispose();
				}
			}
			return result;
		}

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
	}
}
