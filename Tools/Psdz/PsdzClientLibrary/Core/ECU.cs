using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Programming;
using PsdzClientLibrary.Core;

namespace PsdzClient.Core
{
	public class ECU : INotifyPropertyChanged, IEcu, ICloneable
	{
		[XmlIgnore]
		IEnumerable<IAif> IEcu.AIF
		{
			get
			{
				return this.AIF;
			}
		}

		[XmlIgnore]
		public string EcuUid { get; set; }
#if false
		[XmlIgnore]
		IEnumerable<IDtc> IEcu.FEHLER
		{
			get
			{
				return this.FEHLER;
			}
		}

		[XmlIgnore]
		IEnumerable<IDtc> IEcu.INFO
		{
			get
			{
				return this.INFO;
			}
		}

		[XmlIgnore]
		IEnumerable<IJob> IEcu.JOBS
		{
			get
			{
				return this.JOBS;
			}
		}
#endif
		[XmlIgnore]
		public string ProgrammingVariantName
		{
			get
			{
				return this.bntn;
			}
			set
			{
				if ((value == null && this.bntn != null) || (value != null && !value.Equals(this.bntn)))
				{
					this.bntn = value;
					this.OnPropertyChanged("ProgrammingVariantName");
				}
			}
		}

		[XmlIgnore]
		IEcuStatusInfo IEcu.StatusInfo
		{
			get
			{
				return this.StatusInfo;
			}
			set
			{
				this.StatusInfo = value;
			}
		}

		[XmlIgnore]
		public int StillProgrammable
		{
			get
			{
				return this.stillProgrammable;
			}
			set
			{
				this.PropertyChanged.NotifyPropertyChanged(this, Expression.Lambda<Func<object>>(Expression.Convert(Expression.Property(Expression.Constant(this, typeof(ECU)), "StillProgrammable"), typeof(object)), Array.Empty<ParameterExpression>()), ref this.stillProgrammable, value);
			}
		}

		[XmlIgnore]
		IEnumerable<BusType> IEcu.SubBUS
		{
			get
			{
				return this.SubBUS;
			}
		}

		[XmlIgnore]
		ISvk IEcu.SVK
		{
			get
			{
				return this.SVK;
			}
		}

		[XmlIgnore]
		IEnumerable<ISwtStatus> IEcu.SWTStatus
		{
			get
			{
				return this.SWTStatus;
			}
		}

		[XmlIgnore]
		IEnumerable<IEcuTransaction> IEcu.TAL
		{
			get
			{
				return this.TAL;
			}
		}
#if false
		[XmlIgnore]
		public XEP_ECUCLIQUES XepEcuClique
		{
			get
			{
				return this.xepEcuClique;
			}
			set
			{
				if (this.xepEcuClique != value)
				{
					this.xepEcuClique = value;
					this.OnPropertyChanged("XepEcuClique");
				}
			}
		}
#endif
		public string ECUTitle
		{
			get
			{
				return this.eCUTitle;
			}
			set
			{
				if (this.eCUTitle != value)
				{
					this.eCUTitle = value;
					this.OnPropertyChanged("ECUTitle");
				}
			}
		}

		//[XmlIgnore]
		//public XEP_ECUVARIANTS XepEcuVariant { get; set; }

		public object Clone()
		{
			return base.MemberwiseClone();
		}

		public override bool Equals(object obj)
		{
			ECU ecu = obj as ECU;
			if (ecu == null)
			{
				return false;
			}
			if (this.ID_SG_ADR == ecu.ID_SG_ADR)
			{
				long? id_LIN_SLAVE_ADR = this.ID_LIN_SLAVE_ADR;
				long? id_LIN_SLAVE_ADR2 = ecu.ID_LIN_SLAVE_ADR;
				if (id_LIN_SLAVE_ADR.GetValueOrDefault() == id_LIN_SLAVE_ADR2.GetValueOrDefault() & id_LIN_SLAVE_ADR != null == (id_LIN_SLAVE_ADR2 != null))
				{
					return true;
				}
			}
			return false;
		}

		public void FillEcuTitleTree(string ecuShortName)
		{
			if (string.IsNullOrEmpty(this.TITLE_ECUTREE) || !this.IsTitleEcuTreeFilled())
			{
				string text = string.Format("{0}_0x{1:X2}", this.ECU_GROBNAME, this.ID_SG_ADR);
				this.TITLE_ECUTREE = (string.IsNullOrEmpty(ecuShortName) ? text : ecuShortName);
			}
		}
#if false
		public DTC getDTCbyF_ORT(int F_ORT)
		{
			try
			{
				if (this.FEHLER != null)
				{
					foreach (DTC dtc in this.FEHLER)
					{
						long? f_ORT = dtc.F_ORT;
						long num = (long)F_ORT;
						if (f_ORT.GetValueOrDefault() == num & f_ORT != null)
						{
							return dtc;
						}
					}
				}
			}
			catch (Exception exception)
			{
				Log.WarningException("ECU.getDTCbyF_ORT()", exception);
			}
			return null;
		}
#endif
		IDtc IEcu.getDTCbyF_ORT(int F_ORT)
		{
			//return this.getDTCbyF_ORT(F_ORT);
            return null;
        }

#if false
        public DTC GetDTCById(decimal id)
		{
            if (FEHLER != null)
            {
                foreach (DTC item in FEHLER)
                {
                    if (id.Equals(item.Id))
                    {
                        return item;
                    }
                }
            }
            if (INFO != null)
            {
                foreach (DTC item2 in INFO)
                {
                    if (id.Equals(item2.Id))
                    {
                        return item2;
                    }
                }
            }
            return null;
		}
#endif
		IDtc IEcu.GetDTCById(decimal id)
		{
			//return this.GetDTCById(id);
            return null;
        }

		public override int GetHashCode()
		{
			if (this.ID_LIN_SLAVE_ADR != null)
			{
				return (this.ID_SG_ADR + this.ID_LIN_SLAVE_ADR.Value).GetHashCode();
			}
			return this.ID_SG_ADR.GetHashCode();
		}

		public string GetNewestZusbauNoFromAif()
		{
			string result = null;
			if (this.AIF != null)
			{
				int? num = null;
				foreach (AIF aif in this.AIF)
				{
					if (num != null)
					{
						int? aif_ANZAHL_PROG = aif.AIF_ANZAHL_PROG;
						int? num2 = num;
						if (!(aif_ANZAHL_PROG.GetValueOrDefault() > num2.GetValueOrDefault() & (aif_ANZAHL_PROG != null & num2 != null)))
						{
							continue;
						}
					}
					num = aif.AIF_ANZAHL_PROG;
					result = aif.AIF_ZB_NR;
				}
			}
			return result;
		}

		public bool IsRoot()
		{
			BusType bus = this.BUS;
			if (bus != BusType.ROOT)
			{
				if (bus != BusType.VIRTUALROOT)
				{
					return false;
				}
			}
			return true;
		}

		public bool IsSet(long fOrt)
		{
#if false
			try
			{
				if (this.FEHLER != null)
				{
					foreach (DTC dtc in this.FEHLER)
					{
						long? f_ORT = dtc.F_ORT;
						if (f_ORT.GetValueOrDefault() == fOrt & f_ORT != null)
						{
							return true;
						}
					}
				}
			}
			catch (Exception)
			{
				Log.WarningException("ECU.IsSet(long fOrt)", exception);
			}
#endif
			return false;
		}

		public bool IsTitleEcuTreeFilled()
		{
			return !string.IsNullOrEmpty(this.TITLE_ECUTREE) && !this.TITLE_ECUTREE.EndsWith(string.Format("0x{0:X2}", this.ID_SG_ADR), StringComparison.Ordinal);
		}

		public bool IsVirtual()
		{
			BusType bus = this.BUS;
			if (bus != BusType.VIRTUAL)
			{
				if (bus != BusType.VIRTUALROOT)
				{
					return false;
				}
			}
			return true;
		}

		public bool IsVirtualOrVirtualBusCheck()
		{
			BusType bus = this.BUS;
			return bus - BusType.VIRTUAL <= 2;
		}

		public bool IsVirtualRootOrVirtualBusCheck()
		{
			BusType bus = this.BUS;
			return bus - BusType.VIRTUALBUSCHECK <= 1;
		}

		public string LogEcu()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("TITLE_ECUTREE: \"").Append(this.TITLE_ECUTREE).Append("\"");
			stringBuilder.Append("; ID_SG_ADR: \"").Append(string.Format(CultureInfo.InvariantCulture, "0x{0:X2}", this.ID_SG_ADR)).Append("\"");
			stringBuilder.Append("; ECU_GROBNAME: \"").Append(this.ECU_GROBNAME).Append("\"");
			stringBuilder.Append("; ECU_GRUPPE: \"").Append(this.ECU_GRUPPE).Append("\"");
			stringBuilder.Append("; ECU_SGBD: \"").Append(this.ECU_SGBD).Append("\"");
			stringBuilder.Append("; VARIANTE: \"").Append(this.VARIANTE).Append("\"");
			stringBuilder.Append("; ProgrammingVariantName: \"").Append(this.ProgrammingVariantName).Append("\"");
			stringBuilder.Append("; SERIENNUMMER: \"").Append(this.SERIENNUMMER).Append("\"");
			stringBuilder.Append("; IDENT_SUCCESSFULLY: \"").Append(this.IDENT_SUCCESSFULLY).Append("\"");
			stringBuilder.Append("; IS_SUCCESSFULLY: \"").Append(this.IS_SUCCESSFULLY).Append("\"");
			stringBuilder.Append("; FS_SUCCESSFULLY: \"").Append(this.FS_SUCCESSFULLY).Append("\"");
			stringBuilder.Append("; SERIAL_SUCCESSFULLY: \"").Append(this.SERIAL_SUCCESSFULLY).Append("\"");
			stringBuilder.Append("; AIF_SUCCESSFULLY: \"").Append(this.AIF_SUCCESSFULLY).Append("\"");
			stringBuilder.Append("; COMMUNICATION_SUCCESSFULLY: \"").Append(this.COMMUNICATION_SUCCESSFULLY).Append("\"");
			stringBuilder.Append("; DATEN_REFERENZ_SUCCESSFULLY: \"").Append(this.DATEN_REFERENZ_SUCCESSFULLY).Append("\"");
			stringBuilder.Append("; HWREF_SUCCESSFULLY: \"").Append(this.HWREF_SUCCESSFULLY).Append("\"");
			stringBuilder.Append("; PHYSHW_SUCCESSFULLY: \"").Append(this.PHYSHW_SUCCESSFULLY).Append("\"");
			stringBuilder.Append("; SVK_SUCCESSFULLY: \"").Append(this.SVK_SUCCESSFULLY).Append("\"");
			return stringBuilder.ToString();
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			try
			{
				stringBuilder.AppendFormat("ID_SG_ADR: 0x{0:X2} \n", this.ID_SG_ADR);
				stringBuilder.AppendFormat("ECU_HAS_CONFIG_OVERRIDE: {0} \n", this.ECU_HAS_CONFIG_OVERRIDE);
				stringBuilder.AppendFormat("VARIANTE: {0} \n", this.VARIANTE);
				stringBuilder.AppendFormat("ECU_GRUPPE: {0} \n", this.ECU_GRUPPE);
				stringBuilder.AppendFormat("ECU_SGBD: {0} \n", this.ECU_SGBD);
				stringBuilder.AppendFormat("DiagProtocoll: {0} \n", this.DiagProtocoll);
				stringBuilder.AppendFormat("BUS: {0}\n", this.BUS);
				stringBuilder.AppendFormat("COMMUNICATION_SUCCESSFULLY: {0}\n", this.COMMUNICATION_SUCCESSFULLY);
				stringBuilder.AppendFormat("IDENT_SUCCESSFULLY: {0} \n", this.IDENT_SUCCESSFULLY);
				stringBuilder.AppendFormat("FS_SUCCESSFULLY: {0} \n", this.FS_SUCCESSFULLY);
				stringBuilder.AppendFormat("SVK_SUCCESSFULLY: {0} \n", this.SVK_SUCCESSFULLY);
				stringBuilder.AppendFormat("SERIAL_SUCCESSFULLY: {0} \n", this.SERIAL_SUCCESSFULLY);
				stringBuilder.AppendFormat("PHYSHW_SUCCESSFULLY: {0} \n", this.PHYSHW_SUCCESSFULLY);
			}
			catch (Exception exception)
			{
				Log.WarningException("ECU.ToString()", exception);
			}
			return stringBuilder.ToString();
		}

		public ECU()
		{
			this.subBUSField = new ObservableCollection<BusType>();
			//this.sWTStatusField = new ObservableCollection<typeSWTStatus>();
			//this.selectedINFOField = new DTC();
			//this.selectedDTCField = new DTC();
			this.tALField = new ObservableCollection<typeECU_Transaction>();
			this.sVKField = new SVK();
			//this.iNFOField = new ObservableCollection<DTC>();
			//this.FEHLER = new ObservableCollection<DTC>();
			this.aIFField = new ObservableCollection<AIF>();
			//this.jOBSField = new ObservableCollection<JOB>();
			this.bUSField = BusType.UNKNOWN;
			this.diagProtocollField = typeDiagProtocoll.UNKNOWN;
			this.eCUTreeColumnField = -1;
			this.eCUTreeRowField = -1;
			this.eCUTreeColorField = "#323232";
			this.iD_SG_ADRField = -1L;
			this.f_ANZField = 0;
			this.i_ANZField = 0;
			this.cOMMUNICATION_SUCCESSFULLYField = false;
			this.iDENT_SUCCESSFULLYField = false;
			this.aIF_SUCCESSFULLYField = false;
			this.fS_SUCCESSFULLYField = false;
			this.iS_SUCCESSFULLYField = false;
			this.sERIAL_SUCCESSFULLYField = false;
			this.sVK_SUCCESSFULLYField = false;
			this.pHYSHW_SUCCESSFULLYField = false;
			this.hWREF_SUCCESSFULLYField = false;
			this.eCU_HAS_CONFIG_OVERRIDEField = false;
			this.bUSIDField = 0U;
			this.dATEN_REFERENZ_SUCCESSFULLYField = false;
			this.fLASH_STATEField = -1;
			this.eCU_ASSEMBLY_CONFIRMEDField = true;
		}
#if false
		private void FEHLER_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Add)
			{
				using (IEnumerator enumerator = e.NewItems.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						DTC newItem = (DTC)enumerator.Current;
						DTC dtc = this.FEHLER.LastOrDefault((DTC cf) => cf.UniqueId == newItem.UniqueId && cf != newItem);
						if (dtc != null)
						{
							this.FEHLER.Remove(dtc);
						}
					}
				}
			}
		}
#endif
		[XmlIgnore]
		public IEcuStatusInfo StatusInfo
		{
			get
			{
				return this.statusInfo;
			}
			set
			{
				if (this.statusInfo != value)
				{
					this.statusInfo = value;
					this.OnPropertyChanged("StatusInfo");
				}
			}
		}

		public string VARIANTE
		{
			get
			{
				return this.vARIANTEField;
			}
			set
			{
				if (this.vARIANTEField != null)
				{
					if (!this.vARIANTEField.Equals(value))
					{
						this.vARIANTEField = value;
						this.OnPropertyChanged("VARIANTE");
						return;
					}
				}
				else
				{
					this.vARIANTEField = value;
					this.OnPropertyChanged("VARIANTE");
				}
			}
		}
#if false
		public ObservableCollection<JOB> JOBS
		{
			get
			{
				return this.jOBSField;
			}
			set
			{
				if (this.jOBSField != null)
				{
					if (!this.jOBSField.Equals(value))
					{
						this.jOBSField = value;
						this.OnPropertyChanged("JOBS");
						return;
					}
				}
				else
				{
					this.jOBSField = value;
					this.OnPropertyChanged("JOBS");
				}
			}
		}
#endif
		public BusType BUS
		{
			get
			{
				return this.bUSField;
			}
			set
			{
				if (!this.bUSField.Equals(value))
				{
					this.bUSField = value;
					this.OnPropertyChanged("BUS");
				}
			}
		}

		public short? ID_BUS_INDEX
		{
			get
			{
				return this.iD_BUS_INDEXField;
			}
			set
			{
				if (this.iD_BUS_INDEXField != null)
				{
					if (!this.iD_BUS_INDEXField.Equals(value))
					{
						this.iD_BUS_INDEXField = value;
						this.OnPropertyChanged("ID_BUS_INDEX");
						return;
					}
				}
				else
				{
					this.iD_BUS_INDEXField = value;
					this.OnPropertyChanged("ID_BUS_INDEX");
				}
			}
		}

		public typeDiagProtocoll DiagProtocoll
		{
			get
			{
				return this.diagProtocollField;
			}
			set
			{
				if (!this.diagProtocollField.Equals(value))
				{
					this.diagProtocollField = value;
					this.OnPropertyChanged("DiagProtocoll");
				}
			}
		}

		public int ECUTreeColumn
		{
			get
			{
				return this.eCUTreeColumnField;
			}
			set
			{
				if (!this.eCUTreeColumnField.Equals(value))
				{
					this.eCUTreeColumnField = value;
					this.OnPropertyChanged("ECUTreeColumn");
				}
			}
		}

		public int ECUTreeRow
		{
			get
			{
				return this.eCUTreeRowField;
			}
			set
			{
				if (!this.eCUTreeRowField.Equals(value))
				{
					this.eCUTreeRowField = value;
					this.OnPropertyChanged("ECUTreeRow");
				}
			}
		}

		public string TITLE_ECUTREE
		{
			get
			{
				return this.tITLE_ECUTREEField;
			}
			set
			{
				if (this.tITLE_ECUTREEField != null)
				{
					if (!this.tITLE_ECUTREEField.Equals(value))
					{
						this.tITLE_ECUTREEField = value;
						this.OnPropertyChanged("TITLE_ECUTREE");
						return;
					}
				}
				else
				{
					this.tITLE_ECUTREEField = value;
					this.OnPropertyChanged("TITLE_ECUTREE");
				}
			}
		}

		public string ECUTreeColor
		{
			get
			{
				return this.eCUTreeColorField;
			}
			set
			{
				if (this.eCUTreeColorField != null)
				{
					if (!this.eCUTreeColorField.Equals(value))
					{
						this.eCUTreeColorField = value;
						this.OnPropertyChanged("ECUTreeColor");
						return;
					}
				}
				else
				{
					this.eCUTreeColorField = value;
					this.OnPropertyChanged("ECUTreeColor");
				}
			}
		}

		public string ECU_ADR
		{
			get
			{
				return this.eCU_ADRField;
			}
			set
			{
				if (this.eCU_ADRField != null)
				{
					if (!this.eCU_ADRField.Equals(value))
					{
						this.eCU_ADRField = value;
						this.OnPropertyChanged("ECU_ADR");
						return;
					}
				}
				else
				{
					this.eCU_ADRField = value;
					this.OnPropertyChanged("ECU_ADR");
				}
			}
		}

		public ObservableCollection<AIF> AIF
		{
			get
			{
				return this.aIFField;
			}
			set
			{
				if (this.aIFField != null)
				{
					if (!this.aIFField.Equals(value))
					{
						this.aIFField = value;
						this.OnPropertyChanged("AIF");
						return;
					}
				}
				else
				{
					this.aIFField = value;
					this.OnPropertyChanged("AIF");
				}
			}
		}

		public short? ID_LIEF_NR
		{
			get
			{
				return this.iD_LIEF_NRField;
			}
			set
			{
				if (this.iD_LIEF_NRField != null)
				{
					if (!this.iD_LIEF_NRField.Equals(value))
					{
						this.iD_LIEF_NRField = value;
						this.OnPropertyChanged("ID_LIEF_NR");
						return;
					}
				}
				else
				{
					this.iD_LIEF_NRField = value;
					this.OnPropertyChanged("ID_LIEF_NR");
				}
			}
		}

		public string ID_LIEF_TEXT
		{
			get
			{
				return this.iD_LIEF_TEXTField;
			}
			set
			{
				if (this.iD_LIEF_TEXTField != null)
				{
					if (!this.iD_LIEF_TEXTField.Equals(value))
					{
						this.iD_LIEF_TEXTField = value;
						this.OnPropertyChanged("ID_LIEF_TEXT");
						return;
					}
				}
				else
				{
					this.iD_LIEF_TEXTField = value;
					this.OnPropertyChanged("ID_LIEF_TEXT");
				}
			}
		}

		public short? ID_SW_NR
		{
			get
			{
				return this.iD_SW_NRField;
			}
			set
			{
				if (this.iD_SW_NRField != null)
				{
					if (!this.iD_SW_NRField.Equals(value))
					{
						this.iD_SW_NRField = value;
						this.OnPropertyChanged("ID_SW_NR");
						return;
					}
				}
				else
				{
					this.iD_SW_NRField = value;
					this.OnPropertyChanged("ID_SW_NR");
				}
			}
		}

		public string ID_SW_NR_MCV
		{
			get
			{
				return this.iD_SW_NR_MCVField;
			}
			set
			{
				if (this.iD_SW_NR_MCVField != null)
				{
					if (!this.iD_SW_NR_MCVField.Equals(value))
					{
						this.iD_SW_NR_MCVField = value;
						this.OnPropertyChanged("ID_SW_NR_MCV");
						return;
					}
				}
				else
				{
					this.iD_SW_NR_MCVField = value;
					this.OnPropertyChanged("ID_SW_NR_MCV");
				}
			}
		}

		public string ID_SW_NR_FSV
		{
			get
			{
				return this.iD_SW_NR_FSVField;
			}
			set
			{
				if (this.iD_SW_NR_FSVField != null)
				{
					if (!this.iD_SW_NR_FSVField.Equals(value))
					{
						this.iD_SW_NR_FSVField = value;
						this.OnPropertyChanged("ID_SW_NR_FSV");
						return;
					}
				}
				else
				{
					this.iD_SW_NR_FSVField = value;
					this.OnPropertyChanged("ID_SW_NR_FSV");
				}
			}
		}

		public string ID_SW_NR_OSV
		{
			get
			{
				return this.iD_SW_NR_OSVField;
			}
			set
			{
				if (this.iD_SW_NR_OSVField != null)
				{
					if (!this.iD_SW_NR_OSVField.Equals(value))
					{
						this.iD_SW_NR_OSVField = value;
						this.OnPropertyChanged("ID_SW_NR_OSV");
						return;
					}
				}
				else
				{
					this.iD_SW_NR_OSVField = value;
					this.OnPropertyChanged("ID_SW_NR_OSV");
				}
			}
		}

		public string ID_SW_NR_RES
		{
			get
			{
				return this.iD_SW_NR_RESField;
			}
			set
			{
				if (this.iD_SW_NR_RESField != null)
				{
					if (!this.iD_SW_NR_RESField.Equals(value))
					{
						this.iD_SW_NR_RESField = value;
						this.OnPropertyChanged("ID_SW_NR_RES");
						return;
					}
				}
				else
				{
					this.iD_SW_NR_RESField = value;
					this.OnPropertyChanged("ID_SW_NR_RES");
				}
			}
		}

		public short? ID_EWS_SS
		{
			get
			{
				return this.iD_EWS_SSField;
			}
			set
			{
				if (this.iD_EWS_SSField != null)
				{
					if (!this.iD_EWS_SSField.Equals(value))
					{
						this.iD_EWS_SSField = value;
						this.OnPropertyChanged("ID_EWS_SS");
						return;
					}
				}
				else
				{
					this.iD_EWS_SSField = value;
					this.OnPropertyChanged("ID_EWS_SS");
				}
			}
		}

		public string SERIENNUMMER
		{
			get
			{
				return this.sERIENNUMMERField;
			}
			set
			{
				if (this.sERIENNUMMERField != null)
				{
					if (!this.sERIENNUMMERField.Equals(value))
					{
						this.sERIENNUMMERField = value;
						this.OnPropertyChanged("SERIENNUMMER");
						return;
					}
				}
				else
				{
					this.sERIENNUMMERField = value;
					this.OnPropertyChanged("SERIENNUMMER");
				}
			}
		}

		public string ID_BMW_NR
		{
			get
			{
				return this.iD_BMW_NRField;
			}
			set
			{
				if (this.iD_BMW_NRField != null)
				{
					if (!this.iD_BMW_NRField.Equals(value))
					{
						this.iD_BMW_NRField = value;
						this.OnPropertyChanged("ID_BMW_NR");
						return;
					}
				}
				else
				{
					this.iD_BMW_NRField = value;
					this.OnPropertyChanged("ID_BMW_NR");
				}
			}
		}

		public string ID_HW_NR
		{
			get
			{
				return this.iD_HW_NRField;
			}
			set
			{
				if (this.iD_HW_NRField != null)
				{
					if (!this.iD_HW_NRField.Equals(value))
					{
						this.iD_HW_NRField = value;
						this.OnPropertyChanged("ID_HW_NR");
						return;
					}
				}
				else
				{
					this.iD_HW_NRField = value;
					this.OnPropertyChanged("ID_HW_NR");
				}
			}
		}

		public short? ID_COD_INDEX
		{
			get
			{
				return this.iD_COD_INDEXField;
			}
			set
			{
				if (this.iD_COD_INDEXField != null)
				{
					if (!this.iD_COD_INDEXField.Equals(value))
					{
						this.iD_COD_INDEXField = value;
						this.OnPropertyChanged("ID_COD_INDEX");
						return;
					}
				}
				else
				{
					this.iD_COD_INDEXField = value;
					this.OnPropertyChanged("ID_COD_INDEX");
				}
			}
		}

		public int? ID_DIAG_INDEX
		{
			get
			{
				return this.iD_DIAG_INDEXField;
			}
			set
			{
				if (this.iD_DIAG_INDEXField != null)
				{
					if (!this.iD_DIAG_INDEXField.Equals(value))
					{
						this.iD_DIAG_INDEXField = value;
						this.OnPropertyChanged("ID_DIAG_INDEX");
						return;
					}
				}
				else
				{
					this.iD_DIAG_INDEXField = value;
					this.OnPropertyChanged("ID_DIAG_INDEX");
				}
			}
		}

		public int? ID_VAR_INDEX
		{
			get
			{
				return this.iD_VAR_INDEXField;
			}
			set
			{
				if (this.iD_VAR_INDEXField != null)
				{
					if (!this.iD_VAR_INDEXField.Equals(value))
					{
						this.iD_VAR_INDEXField = value;
						this.OnPropertyChanged("ID_VAR_INDEX");
						return;
					}
				}
				else
				{
					this.iD_VAR_INDEXField = value;
					this.OnPropertyChanged("ID_VAR_INDEX");
				}
			}
		}

		public int? ID_DATUM_JAHR
		{
			get
			{
				return this.iD_DATUM_JAHRField;
			}
			set
			{
				if (this.iD_DATUM_JAHRField != null)
				{
					if (!this.iD_DATUM_JAHRField.Equals(value))
					{
						this.iD_DATUM_JAHRField = value;
						this.OnPropertyChanged("ID_DATUM_JAHR");
						return;
					}
				}
				else
				{
					this.iD_DATUM_JAHRField = value;
					this.OnPropertyChanged("ID_DATUM_JAHR");
				}
			}
		}

		public int? ID_DATUM_MONAT
		{
			get
			{
				return this.iD_DATUM_MONATField;
			}
			set
			{
				if (this.iD_DATUM_MONATField != null)
				{
					if (!this.iD_DATUM_MONATField.Equals(value))
					{
						this.iD_DATUM_MONATField = value;
						this.OnPropertyChanged("ID_DATUM_MONAT");
						return;
					}
				}
				else
				{
					this.iD_DATUM_MONATField = value;
					this.OnPropertyChanged("ID_DATUM_MONAT");
				}
			}
		}

		public int? ID_DATUM_TAG
		{
			get
			{
				return this.iD_DATUM_TAGField;
			}
			set
			{
				if (this.iD_DATUM_TAGField != null)
				{
					if (!this.iD_DATUM_TAGField.Equals(value))
					{
						this.iD_DATUM_TAGField = value;
						this.OnPropertyChanged("ID_DATUM_TAG");
						return;
					}
				}
				else
				{
					this.iD_DATUM_TAGField = value;
					this.OnPropertyChanged("ID_DATUM_TAG");
				}
			}
		}

		public string ID_DATUM
		{
			get
			{
				return this.iD_DATUMField;
			}
			set
			{
				if (this.iD_DATUMField != null)
				{
					if (!this.iD_DATUMField.Equals(value))
					{
						this.iD_DATUMField = value;
						this.OnPropertyChanged("ID_DATUM");
						return;
					}
				}
				else
				{
					this.iD_DATUMField = value;
					this.OnPropertyChanged("ID_DATUM");
				}
			}
		}

		public short? ID_DATUM_KW
		{
			get
			{
				return this.iD_DATUM_KWField;
			}
			set
			{
				if (this.iD_DATUM_KWField != null)
				{
					if (!this.iD_DATUM_KWField.Equals(value))
					{
						this.iD_DATUM_KWField = value;
						this.OnPropertyChanged("ID_DATUM_KW");
						return;
					}
				}
				else
				{
					this.iD_DATUM_KWField = value;
					this.OnPropertyChanged("ID_DATUM_KW");
				}
			}
		}

		public long? ID_SGBD_INDEX
		{
			get
			{
				return this.iD_SGBD_INDEXField;
			}
			set
			{
				if (this.iD_SGBD_INDEXField != null)
				{
					if (!this.iD_SGBD_INDEXField.Equals(value))
					{
						this.iD_SGBD_INDEXField = value;
						this.OnPropertyChanged("ID_SGBD_INDEX");
						return;
					}
				}
				else
				{
					this.iD_SGBD_INDEXField = value;
					this.OnPropertyChanged("ID_SGBD_INDEX");
				}
			}
		}

		public long ID_SG_ADR
		{
			get
			{
				return this.iD_SG_ADRField;
			}
			set
			{
				if (!this.iD_SG_ADRField.Equals(value))
				{
					this.iD_SG_ADRField = value;
					this.OnPropertyChanged("ID_SG_ADR");
				}
			}
		}

		public long? ID_LIN_SLAVE_ADR
		{
			get
			{
				return this.iD_LIN_SLAVE_ADRField;
			}
			set
			{
				if (this.iD_LIN_SLAVE_ADRField != null)
				{
					if (!this.iD_LIN_SLAVE_ADRField.Equals(value))
					{
						this.iD_LIN_SLAVE_ADRField = value;
						this.OnPropertyChanged("ID_LIN_SLAVE_ADR");
						return;
					}
				}
				else
				{
					this.iD_LIN_SLAVE_ADRField = value;
					this.OnPropertyChanged("ID_LIN_SLAVE_ADR");
				}
			}
		}

		public int F_ANZ
		{
			get
			{
				return this.f_ANZField;
			}
			set
			{
				if (!this.f_ANZField.Equals(value))
				{
					this.f_ANZField = value;
					this.OnPropertyChanged("F_ANZ");
				}
			}
		}
#if false
		public ObservableCollection<DTC> FEHLER
		{
			get
			{
				return this.fEHLERField;
			}
			set
			{
				if (this.fEHLERField != null)
				{
					if (!this.fEHLERField.Equals(value))
					{
						this.fEHLERField = value;
						this.OnPropertyChanged("FEHLER");
						this.FEHLER.CollectionChanged += this.FEHLER_CollectionChanged;
						return;
					}
				}
				else
				{
					this.fEHLERField = value;
					this.OnPropertyChanged("FEHLER");
					this.FEHLER.CollectionChanged += this.FEHLER_CollectionChanged;
				}
			}
		}
#endif
		public int I_ANZ
		{
			get
			{
				return this.i_ANZField;
			}
			set
			{
				if (!this.i_ANZField.Equals(value))
				{
					this.i_ANZField = value;
					this.OnPropertyChanged("I_ANZ");
				}
			}
		}
#if false
		public ObservableCollection<DTC> INFO
		{
			get
			{
				return this.iNFOField;
			}
			set
			{
				if (this.iNFOField != null)
				{
					if (!this.iNFOField.Equals(value))
					{
						this.iNFOField = value;
						this.OnPropertyChanged("INFO");
						return;
					}
				}
				else
				{
					this.iNFOField = value;
					this.OnPropertyChanged("INFO");
				}
			}
		}
#endif
		public SVK SVK
		{
			get
			{
				return this.sVKField;
			}
			set
			{
				if (this.sVKField != null)
				{
					if (!this.sVKField.Equals(value))
					{
						this.sVKField = value;
						this.OnPropertyChanged("SVK");
						return;
					}
				}
				else
				{
					this.sVKField = value;
					this.OnPropertyChanged("SVK");
				}
			}
		}

		public string PHYSIKALISCHE_HW_NR
		{
			get
			{
				return this.pHYSIKALISCHE_HW_NRField;
			}
			set
			{
				if (this.pHYSIKALISCHE_HW_NRField != null)
				{
					if (!this.pHYSIKALISCHE_HW_NRField.Equals(value))
					{
						this.pHYSIKALISCHE_HW_NRField = value;
						this.OnPropertyChanged("PHYSIKALISCHE_HW_NR");
						return;
					}
				}
				else
				{
					this.pHYSIKALISCHE_HW_NRField = value;
					this.OnPropertyChanged("PHYSIKALISCHE_HW_NR");
				}
			}
		}

		public ObservableCollection<typeECU_Transaction> TAL
		{
			get
			{
				return this.tALField;
			}
			set
			{
				if (this.tALField != null)
				{
					if (!this.tALField.Equals(value))
					{
						this.tALField = value;
						this.OnPropertyChanged("TAL");
						return;
					}
				}
				else
				{
					this.tALField = value;
					this.OnPropertyChanged("TAL");
				}
			}
		}

		public string HARDWARE_REFERENZ
		{
			get
			{
				return this.hARDWARE_REFERENZField;
			}
			set
			{
				if (this.hARDWARE_REFERENZField != null)
				{
					if (!this.hARDWARE_REFERENZField.Equals(value))
					{
						this.hARDWARE_REFERENZField = value;
						this.OnPropertyChanged("HARDWARE_REFERENZ");
						return;
					}
				}
				else
				{
					this.hARDWARE_REFERENZField = value;
					this.OnPropertyChanged("HARDWARE_REFERENZ");
				}
			}
		}

		public int? HW_REF_STATUS
		{
			get
			{
				return this.hW_REF_STATUSField;
			}
			set
			{
				if (this.hW_REF_STATUSField != null)
				{
					if (!this.hW_REF_STATUSField.Equals(value))
					{
						this.hW_REF_STATUSField = value;
						this.OnPropertyChanged("HW_REF_STATUS");
						return;
					}
				}
				else
				{
					this.hW_REF_STATUSField = value;
					this.OnPropertyChanged("HW_REF_STATUS");
				}
			}
		}

		public ObservableCollection<typeSWTStatus> SWTStatus
		{
			get
			{
				return this.sWTStatusField;
			}
			set
			{
				if (this.sWTStatusField != null)
				{
					if (!this.sWTStatusField.Equals(value))
					{
						this.sWTStatusField = value;
						this.OnPropertyChanged("SWTStatus");
						return;
					}
				}
				else
				{
					this.sWTStatusField = value;
					this.OnPropertyChanged("SWTStatus");
				}
			}
		}

		public string DATEN_REFERENZ
		{
			get
			{
				return this.dATEN_REFERENZField;
			}
			set
			{
				if (this.dATEN_REFERENZField != null)
				{
					if (!this.dATEN_REFERENZField.Equals(value))
					{
						this.dATEN_REFERENZField = value;
						this.OnPropertyChanged("DATEN_REFERENZ");
						return;
					}
				}
				else
				{
					this.dATEN_REFERENZField = value;
					this.OnPropertyChanged("DATEN_REFERENZ");
				}
			}
		}

		public ObservableCollection<BusType> SubBUS
		{
			get
			{
				return this.subBUSField;
			}
			set
			{
				if (this.subBUSField != null)
				{
					if (!this.subBUSField.Equals(value))
					{
						this.subBUSField = value;
						this.OnPropertyChanged("SubBUS");
						return;
					}
				}
				else
				{
					this.subBUSField = value;
					this.OnPropertyChanged("SubBUS");
				}
			}
		}

		public string ECU_GROBNAME
		{
			get
			{
				return this.eCU_GROBNAMEField;
			}
			set
			{
				if (this.eCU_GROBNAMEField != null)
				{
					if (!this.eCU_GROBNAMEField.Equals(value))
					{
						this.eCU_GROBNAMEField = value;
						this.OnPropertyChanged("ECU_GROBNAME");
						return;
					}
				}
				else
				{
					this.eCU_GROBNAMEField = value;
					this.OnPropertyChanged("ECU_GROBNAME");
				}
			}
		}

		public string ECU_NAME
		{
			get
			{
				return this.eCU_NAMEField;
			}
			set
			{
				if (this.eCU_NAMEField != null)
				{
					if (!this.eCU_NAMEField.Equals(value))
					{
						this.eCU_NAMEField = value;
						this.OnPropertyChanged("ECU_NAME");
						return;
					}
				}
				else
				{
					this.eCU_NAMEField = value;
					this.OnPropertyChanged("ECU_NAME");
				}
			}
		}

		public string ECU_SGBD
		{
			get
			{
				return this.eCU_SGBDField;
			}
			set
			{
				if (this.eCU_SGBDField != null)
				{
					if (!this.eCU_SGBDField.Equals(value))
					{
						this.eCU_SGBDField = value;
						this.OnPropertyChanged("ECU_SGBD");
						return;
					}
				}
				else
				{
					this.eCU_SGBDField = value;
					this.OnPropertyChanged("ECU_SGBD");
				}
			}
		}

		public string ECU_GRUPPE
		{
			get
			{
				return this.eCU_GRUPPEField;
			}
			set
			{
				if (this.eCU_GRUPPEField != null)
				{
					if (!this.eCU_GRUPPEField.Equals(value))
					{
						this.eCU_GRUPPEField = value;
						this.OnPropertyChanged("ECU_GRUPPE");
						return;
					}
				}
				else
				{
					this.eCU_GRUPPEField = value;
					this.OnPropertyChanged("ECU_GRUPPE");
				}
			}
		}

		[DefaultValue(false)]
		public bool COMMUNICATION_SUCCESSFULLY
		{
			get
			{
				return this.cOMMUNICATION_SUCCESSFULLYField;
			}
			set
			{
				if (!this.cOMMUNICATION_SUCCESSFULLYField.Equals(value))
				{
					this.cOMMUNICATION_SUCCESSFULLYField = value;
					this.OnPropertyChanged("COMMUNICATION_SUCCESSFULLY");
				}
			}
		}

		[DefaultValue(false)]
		public bool IDENT_SUCCESSFULLY
		{
			get
			{
				return this.iDENT_SUCCESSFULLYField;
			}
			set
			{
				if (!this.iDENT_SUCCESSFULLYField.Equals(value))
				{
					this.iDENT_SUCCESSFULLYField = value;
					this.OnPropertyChanged("IDENT_SUCCESSFULLY");
				}
			}
		}

		[DefaultValue(false)]
		public bool AIF_SUCCESSFULLY
		{
			get
			{
				return this.aIF_SUCCESSFULLYField;
			}
			set
			{
				if (!this.aIF_SUCCESSFULLYField.Equals(value))
				{
					this.aIF_SUCCESSFULLYField = value;
					this.OnPropertyChanged("AIF_SUCCESSFULLY");
				}
			}
		}

		[DefaultValue(false)]
		public bool FS_SUCCESSFULLY
		{
			get
			{
				return this.fS_SUCCESSFULLYField;
			}
			set
			{
				if (!this.fS_SUCCESSFULLYField.Equals(value))
				{
					this.fS_SUCCESSFULLYField = value;
					this.OnPropertyChanged("FS_SUCCESSFULLY");
				}
			}
		}

		[DefaultValue(false)]
		public bool IS_SUCCESSFULLY
		{
			get
			{
				return this.iS_SUCCESSFULLYField;
			}
			set
			{
				if (!this.iS_SUCCESSFULLYField.Equals(value))
				{
					this.iS_SUCCESSFULLYField = value;
					this.OnPropertyChanged("IS_SUCCESSFULLY");
				}
			}
		}

		[DefaultValue(false)]
		public bool SERIAL_SUCCESSFULLY
		{
			get
			{
				return this.sERIAL_SUCCESSFULLYField;
			}
			set
			{
				if (!this.sERIAL_SUCCESSFULLYField.Equals(value))
				{
					this.sERIAL_SUCCESSFULLYField = value;
					this.OnPropertyChanged("SERIAL_SUCCESSFULLY");
				}
			}
		}

		[DefaultValue(false)]
		public bool SVK_SUCCESSFULLY
		{
			get
			{
				return this.sVK_SUCCESSFULLYField;
			}
			set
			{
				if (!this.sVK_SUCCESSFULLYField.Equals(value))
				{
					this.sVK_SUCCESSFULLYField = value;
					this.OnPropertyChanged("SVK_SUCCESSFULLY");
				}
			}
		}

		[DefaultValue(false)]
		public bool PHYSHW_SUCCESSFULLY
		{
			get
			{
				return this.pHYSHW_SUCCESSFULLYField;
			}
			set
			{
				if (!this.pHYSHW_SUCCESSFULLYField.Equals(value))
				{
					this.pHYSHW_SUCCESSFULLYField = value;
					this.OnPropertyChanged("PHYSHW_SUCCESSFULLY");
				}
			}
		}

		[DefaultValue(false)]
		public bool HWREF_SUCCESSFULLY
		{
			get
			{
				return this.hWREF_SUCCESSFULLYField;
			}
			set
			{
				if (!this.hWREF_SUCCESSFULLYField.Equals(value))
				{
					this.hWREF_SUCCESSFULLYField = value;
					this.OnPropertyChanged("HWREF_SUCCESSFULLY");
				}
			}
		}

		[DefaultValue(false)]
		public bool ECU_HAS_CONFIG_OVERRIDE
		{
			get
			{
				return this.eCU_HAS_CONFIG_OVERRIDEField;
			}
			set
			{
				if (!this.eCU_HAS_CONFIG_OVERRIDEField.Equals(value))
				{
					this.eCU_HAS_CONFIG_OVERRIDEField = value;
					this.OnPropertyChanged("ECU_HAS_CONFIG_OVERRIDE");
				}
			}
		}

		[DefaultValue(typeof(uint), "0")]
		public uint BUSID
		{
			get
			{
				return this.bUSIDField;
			}
			set
			{
				if (!this.bUSIDField.Equals(value))
				{
					this.bUSIDField = value;
					this.OnPropertyChanged("BUSID");
				}
			}
		}

		[DefaultValue(false)]
		public bool DATEN_REFERENZ_SUCCESSFULLY
		{
			get
			{
				return this.dATEN_REFERENZ_SUCCESSFULLYField;
			}
			set
			{
				if (!this.dATEN_REFERENZ_SUCCESSFULLYField.Equals(value))
				{
					this.dATEN_REFERENZ_SUCCESSFULLYField = value;
					this.OnPropertyChanged("DATEN_REFERENZ_SUCCESSFULLY");
				}
			}
		}

		[DefaultValue(-1)]
		public int FLASH_STATE
		{
			get
			{
				return this.fLASH_STATEField;
			}
			set
			{
				if (!this.fLASH_STATEField.Equals(value))
				{
					this.fLASH_STATEField = value;
					this.OnPropertyChanged("FLASH_STATE");
				}
			}
		}

		[DefaultValue(true)]
		public bool ECU_ASSEMBLY_CONFIRMED
		{
			get
			{
				return this.eCU_ASSEMBLY_CONFIRMEDField;
			}
			set
			{
				if (!this.eCU_ASSEMBLY_CONFIRMEDField.Equals(value))
				{
					this.eCU_ASSEMBLY_CONFIRMEDField = value;
					this.OnPropertyChanged("ECU_ASSEMBLY_CONFIRMED");
				}
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

		private string bntn;

		private int stillProgrammable;

		//private XEP_ECUCLIQUES xepEcuClique;

		private string eCUTitle;

		private string vARIANTEField;

		//private ObservableCollection<JOB> jOBSField;

		private BusType bUSField;

		private short? iD_BUS_INDEXField;

		private typeDiagProtocoll diagProtocollField;

		private int eCUTreeColumnField;

		private int eCUTreeRowField;

		private string tITLE_ECUTREEField;

		private string eCUTreeColorField;

		private string eCU_ADRField;

		private ObservableCollection<AIF> aIFField;

		private short? iD_LIEF_NRField;

		private string iD_LIEF_TEXTField;

		private short? iD_SW_NRField;

		private string iD_SW_NR_MCVField;

		private string iD_SW_NR_FSVField;

		private string iD_SW_NR_OSVField;

		private string iD_SW_NR_RESField;

		private short? iD_EWS_SSField;

		private string sERIENNUMMERField;

		private string iD_BMW_NRField;

		private string iD_HW_NRField;

		private short? iD_COD_INDEXField;

		private int? iD_DIAG_INDEXField;

		private int? iD_VAR_INDEXField;

		private int? iD_DATUM_JAHRField;

		private int? iD_DATUM_MONATField;

		private int? iD_DATUM_TAGField;

		private string iD_DATUMField;

		private short? iD_DATUM_KWField;

		private long? iD_SGBD_INDEXField;

		private long iD_SG_ADRField;

		private long? iD_LIN_SLAVE_ADRField;

		private int f_ANZField;

		//private ObservableCollection<DTC> fEHLERField;

		private int i_ANZField;

		//private ObservableCollection<DTC> iNFOField;

		private SVK sVKField;

		private string pHYSIKALISCHE_HW_NRField;

		private ObservableCollection<typeECU_Transaction> tALField;

		//private DTC selectedDTCField;

		//private DTC selectedINFOField;

		private string hARDWARE_REFERENZField;

		private int? hW_REF_STATUSField;

		private ObservableCollection<typeSWTStatus> sWTStatusField;

		private string dATEN_REFERENZField;

		private ObservableCollection<BusType> subBUSField;

		private string eCU_GROBNAMEField;

		private string eCU_NAMEField;

		private string eCU_SGBDField;

		private string eCU_GRUPPEField;

		private bool cOMMUNICATION_SUCCESSFULLYField;

		private bool iDENT_SUCCESSFULLYField;

		private bool aIF_SUCCESSFULLYField;

		private bool fS_SUCCESSFULLYField;

		private bool iS_SUCCESSFULLYField;

		private bool sERIAL_SUCCESSFULLYField;

		private bool sVK_SUCCESSFULLYField;

		private bool pHYSHW_SUCCESSFULLYField;

		private bool hWREF_SUCCESSFULLYField;

		private bool eCU_HAS_CONFIG_OVERRIDEField;

		private uint bUSIDField;

		private bool dATEN_REFERENZ_SUCCESSFULLYField;

		private int fLASH_STATEField;

		private bool eCU_ASSEMBLY_CONFIRMEDField;

		private IEcuStatusInfo statusInfo;
	}
}
