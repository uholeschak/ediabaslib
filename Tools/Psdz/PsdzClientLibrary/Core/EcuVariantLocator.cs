using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
	public class EcuVariantLocator : ISPELocator, IEcuVariantLocator
	{
		public EcuVariantLocator(PdszDatabase.EcuVar ecuVariant)
		{
			this.ecuVariant = ecuVariant;
			//this.children = new ISPELocator[0];
		}

		public static IEcuVariantLocator CreateEcuVariantLocator(string ecuVariant, Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
		{
			PdszDatabase.EcuVar ecuVariantByName = ClientContext.GetClientContext(vecInfo)?.Database?.GetEcuVariantByName(ecuVariant);
			if (ecuVariantByName != null)
			{
				return new EcuVariantLocator(ecuVariantByName, vecInfo, ffmResolver);
			}
			return null;
		}

		public EcuVariantLocator(decimal id, Vehicle vec, IFFMDynamicResolver ffmResolver)
		{
            this.vecInfo = vec;
			this.ecuVariant = ClientContext.GetClientContext(this.vecInfo)?.Database?.GetEcuVariantById(id.ToString(CultureInfo.InvariantCulture));
			this.ffmResolver = ffmResolver;
		}

		public EcuVariantLocator(PdszDatabase.EcuVar ecuVariant, Vehicle vec, IFFMDynamicResolver ffmResolver)
		{
            this.vecInfo = vec;
			this.ecuVariant = ecuVariant;
			//this.children = new ISPELocator[0];
			this.ffmResolver = ffmResolver;
		}
#if false
		public ISPELocator[] Children
		{
			get
			{
				if (this.children != null && this.children.Length != 0)
				{
					return this.children;
				}
				new List<ISPELocator>();
				ICollection<XEP_FAULTCODE> xepFaultCodeByEcuVariantId = ClientContext.Database?.GetXepFaultCodeByEcuVariantId(this.ecuVariant.Id, this.vecInfo, this.ffmResolver);
				if (xepFaultCodeByEcuVariantId != null)
				{
					foreach (XEP_FAULTCODE xep_FAULTCODE in xepFaultCodeByEcuVariantId)
					{
						new FaultCode();
					}
				}
				return this.children;
			}
		}
#endif
		public string Id
		{
			get
			{
				return this.ecuVariant.Id.ToString(CultureInfo.InvariantCulture);
			}
		}

		public ISPELocator[] Parents
		{
			get
			{
				if (this.parents != null && this.parents.Length != 0)
				{
					return this.parents;
				}
				List<ISPELocator> list = new List<ISPELocator>();
				if (string.IsNullOrEmpty(this.ecuVariant.GroupId))
				{
					PdszDatabase.EcuGroup ecuGroupById = ClientContext.GetClientContext(this.vecInfo)?.Database?.GetEcuGroupById(this.ecuVariant.GroupId);
					if (ecuGroupById != null)
					{
						list.Add(new EcuGroupLocator(ecuGroupById, this.vecInfo, this.ffmResolver));
						this.parents = list.ToArray();
					}
				}
				return this.parents;
			}
		}

		public string DataClassName
		{
			get
			{
				return "ECUVariant";
			}
		}

		public string[] OutgoingLinkNames
		{
			get
			{
				return new string[0];
			}
		}

		public string[] IncomingLinkNames
		{
			get
			{
				return new string[0];
			}
		}

		public string[] DataValueNames
		{
			get
			{
				return new string[]
				{
					"ID",
					"FAULTMEMORYDELETEWAITINGTIME",
					"NAME",
					"TITLEID",
					"TITLE_DEDE",
					"TITLE_ENGB",
					"TITLE_ENUS",
					"TITLE_FR",
					"TITLE_TH",
					"TITLE_SV",
					"TITLE_IT",
					"TITLE_ES",
					"TITLE_ID",
					"TITLE_KO",
					"TITLE_EL",
					"TITLE_TR",
					"TITLE_ZHCN",
					"TITLE_RU",
					"TITLE_NL",
					"TITLE_PT",
					"TITLE_ZHTW",
					"TITLE_JA",
					"TITLE_CSCZ",
					"TITLE_PLPL",
					"VALIDFROM",
					"VALIDTO",
					"SICHERHEITSRELEVANT",
					"ECUGROUPID",
					"SORT"
				};
			}
		}

		public string SignedId
		{
			get
			{
				if (this.ecuVariant == null)
				{
					return string.Empty;
				}
				return this.ecuVariant.Id;
			}
		}

		public Exception Exception
		{
			get
			{
				return null;
			}
		}

		public bool HasException
		{
			get
			{
				return false;
			}
		}
#if false
		public string GetDataValue(string name)
		{
			if (this.ecuVariant != null && !string.IsNullOrEmpty(name))
			{
				string text = name.ToUpperInvariant();
				if (text != null)
				{
					uint num = < PrivateImplementationDetails >.ComputeStringHash(text);
					if (num <= 869559705U)
					{
						if (num <= 526602168U)
						{
							if (num <= 72914907U)
							{
								if (num != 16830413U)
								{
									if (num != 25111569U)
									{
										if (num == 72914907U)
										{
											if (text == "TITLE_JA")
											{
												return this.ecuVariant.Title_ja;
											}
										}
									}
									else if (text == "SORT")
									{
										if (this.ecuVariant.Sort == null)
										{
											return "0";
										}
										return this.ecuVariant.Sort.ToString();
									}
								}
								else if (text == "TITLE_ENUS")
								{
									return this.ecuVariant.Title_enus;
								}
							}
							else if (num <= 410075207U)
							{
								if (num != 405672864U)
								{
									if (num == 410075207U)
									{
										if (text == "NODECLASS")
										{
											return "5719042";
										}
									}
								}
								else if (text == "TITLE_ENGB")
								{
									return this.ecuVariant.Title_engb;
								}
							}
							else if (num != 419325489U)
							{
								if (num == 526602168U)
								{
									if (text == "TITLE_NL")
									{
										return this.ecuVariant.Title_nl;
									}
								}
							}
							else if (text == "TITLE_ZHTW")
							{
								return this.ecuVariant.Title_zhtw;
							}
						}
						else if (num <= 576390572U)
						{
							if (num <= 567617109U)
							{
								if (num != 528176286U)
								{
									if (num == 567617109U)
									{
										if (text == "TITLE_ZHCN")
										{
											return this.ecuVariant.Title_zhcn;
										}
									}
								}
								else if (text == "TITLE_TH")
								{
									return this.ecuVariant.Title_th;
								}
							}
							else if (num != 572932585U)
							{
								if (num == 576390572U)
								{
									if (text == "TITLE_KO")
									{
										return this.ecuVariant.Title_ko;
									}
								}
							}
							else if (text == "TITLE")
							{
								return this.ecuVariant.Title;
							}
						}
						else if (num <= 678189119U)
						{
							if (num != 628842000U)
							{
								if (num == 678189119U)
								{
									if (text == "TITLE_RU")
									{
										return this.ecuVariant.Title_ru;
									}
								}
							}
							else if (text == "TITLE_TR")
							{
								return this.ecuVariant.Title_tr;
							}
						}
						else if (num != 727562098U)
						{
							if (num == 869559705U)
							{
								if (text == "TITLE_CSCZ")
								{
									return this.ecuVariant.Title_cscz;
								}
							}
						}
						else if (text == "TITLE_DEDE")
						{
							return this.ecuVariant.Title_dede;
						}
					}
					else if (num <= 2819690657U)
					{
						if (num <= 1458105184U)
						{
							if (num != 1035818431U)
							{
								if (num != 1387956774U)
								{
									if (num == 1458105184U)
									{
										if (text == "ID")
										{
											return this.ecuVariant.Id.ToString(CultureInfo.InvariantCulture);
										}
									}
								}
								else if (text == "NAME")
								{
									return this.ecuVariant.Name;
								}
							}
							else if (text == "FAULTMEMORYDELETEWAITINGTIME")
							{
								if (this.ecuVariant.FaultMemoryDeleteWaitingTime == null)
								{
									return string.Empty;
								}
								return this.ecuVariant.FaultMemoryDeleteWaitingTime.ToString();
							}
						}
						else if (num <= 2700653218U)
						{
							if (num != 1944305449U)
							{
								if (num == 2700653218U)
								{
									if (text == "TITLE_PLPL")
									{
										return this.ecuVariant.Title_plpl;
									}
								}
							}
							else if (text == "VALIDFROM")
							{
								if (this.ecuVariant.ValidFrom == null)
								{
									return string.Empty;
								}
								return this.ecuVariant.ValidFrom.ToString();
							}
						}
						else if (num != 2726887280U)
						{
							if (num == 2819690657U)
							{
								if (text == "SICHERHEITSRELEVANT")
								{
									if (this.ecuVariant.Sicherheitsrelevant == null)
									{
										return "0";
									}
									return this.ecuVariant.Sicherheitsrelevant.ToString();
								}
							}
						}
						else if (text == "VALIDTO")
						{
							if (this.ecuVariant.ValidTo == null)
							{
								return string.Empty;
							}
							return this.ecuVariant.ValidTo.ToString();
						}
					}
					else if (num <= 3949971919U)
					{
						if (num <= 3780518443U)
						{
							if (num != 2915361786U)
							{
								if (num == 3780518443U)
								{
									if (text == "TITLE_EL")
									{
										return this.ecuVariant.Title_el;
									}
								}
							}
							else if (text == "ECUGROUPID")
							{
								if (this.ecuVariant.EcuGroupId == null)
								{
									return "0";
								}
								return this.ecuVariant.EcuGroupId.ToString();
							}
						}
						else if (num != 3915430943U)
						{
							if (num == 3949971919U)
							{
								if (text == "TITLE_SV")
								{
									return this.ecuVariant.Title_sv;
								}
							}
						}
						else if (text == "TITLE_IT")
						{
							return this.ecuVariant.Title_it;
						}
					}
					else if (num <= 3998627490U)
					{
						if (num != 3950119014U)
						{
							if (num == 3998627490U)
							{
								if (text == "TITLE_ES")
								{
									return this.ecuVariant.Title_es;
								}
							}
						}
						else if (text == "TITLE_PT")
						{
							return this.ecuVariant.Title_pt;
						}
					}
					else if (num != 4183872847U)
					{
						if (num == 4249850490U)
						{
							if (text == "TITLE_FR")
							{
								return this.ecuVariant.Title_fr;
							}
						}
					}
					else if (text == "TITLE_ID")
					{
						return this.ecuVariant.Title_id;
					}
				}
				return string.Empty;
			}
			return null;
		}
#endif
		public ISPELocator[] GetIncomingLinks()
		{
			return new ISPELocator[0];
		}

		public ISPELocator[] GetIncomingLinks(string incomingLinkName)
		{
			return this.parents;
		}
#if false
		public ISPELocator[] GetOutgoingLinks()
		{
			return this.children;
		}

		public ISPELocator[] GetOutgoingLinks(string outgoingLinkName)
		{
			return this.children;
		}

		public T GetDataValue<T>(string name)
		{
			try
			{
				if (!string.IsNullOrEmpty(name) && this.ecuVariant != null)
				{
					object obj = null;
					string text = name.ToUpperInvariant();
					if (text != null)
					{
						uint num = < PrivateImplementationDetails >.ComputeStringHash(text);
						if (num <= 869559705U)
						{
							if (num <= 526602168U)
							{
								if (num <= 72914907U)
								{
									if (num != 16830413U)
									{
										if (num != 25111569U)
										{
											if (num == 72914907U)
											{
												if (text == "TITLE_JA")
												{
													obj = this.ecuVariant.Title_ja;
												}
											}
										}
										else if (text == "SORT")
										{
											obj = this.ecuVariant.Sort;
										}
									}
									else if (text == "TITLE_ENUS")
									{
										obj = this.ecuVariant.Title_enus;
									}
								}
								else if (num <= 410075207U)
								{
									if (num != 405672864U)
									{
										if (num == 410075207U)
										{
											if (text == "NODECLASS")
											{
												obj = "5719042";
											}
										}
									}
									else if (text == "TITLE_ENGB")
									{
										obj = this.ecuVariant.Title_engb;
									}
								}
								else if (num != 419325489U)
								{
									if (num == 526602168U)
									{
										if (text == "TITLE_NL")
										{
											obj = this.ecuVariant.Title_nl;
										}
									}
								}
								else if (text == "TITLE_ZHTW")
								{
									obj = this.ecuVariant.Title_zhtw;
								}
							}
							else if (num <= 576390572U)
							{
								if (num <= 567617109U)
								{
									if (num != 528176286U)
									{
										if (num == 567617109U)
										{
											if (text == "TITLE_ZHCN")
											{
												obj = this.ecuVariant.Title_zhcn;
											}
										}
									}
									else if (text == "TITLE_TH")
									{
										obj = this.ecuVariant.Title_th;
									}
								}
								else if (num != 572932585U)
								{
									if (num == 576390572U)
									{
										if (text == "TITLE_KO")
										{
											obj = this.ecuVariant.Title_ko;
										}
									}
								}
								else if (text == "TITLE")
								{
									obj = this.ecuVariant.Title;
								}
							}
							else if (num <= 678189119U)
							{
								if (num != 628842000U)
								{
									if (num == 678189119U)
									{
										if (text == "TITLE_RU")
										{
											obj = this.ecuVariant.Title_ru;
										}
									}
								}
								else if (text == "TITLE_TR")
								{
									obj = this.ecuVariant.Title_tr;
								}
							}
							else if (num != 727562098U)
							{
								if (num == 869559705U)
								{
									if (text == "TITLE_CSCZ")
									{
										obj = this.ecuVariant.Title_cscz;
									}
								}
							}
							else if (text == "TITLE_DEDE")
							{
								obj = this.ecuVariant.Title_dede;
							}
						}
						else if (num <= 2819690657U)
						{
							if (num <= 1458105184U)
							{
								if (num != 1035818431U)
								{
									if (num != 1387956774U)
									{
										if (num == 1458105184U)
										{
											if (text == "ID")
											{
												obj = this.ecuVariant.Id;
											}
										}
									}
									else if (text == "NAME")
									{
										obj = this.ecuVariant.Name;
									}
								}
								else if (text == "FAULTMEMORYDELETEWAITINGTIME")
								{
									obj = this.ecuVariant.FaultMemoryDeleteWaitingTime;
								}
							}
							else if (num <= 2700653218U)
							{
								if (num != 1944305449U)
								{
									if (num == 2700653218U)
									{
										if (text == "TITLE_PLPL")
										{
											obj = this.ecuVariant.Title_plpl;
										}
									}
								}
								else if (text == "VALIDFROM")
								{
									obj = (this.ecuVariant.ValidFrom != null);
								}
							}
							else if (num != 2726887280U)
							{
								if (num == 2819690657U)
								{
									if (text == "SICHERHEITSRELEVANT")
									{
										obj = this.ecuVariant.Sicherheitsrelevant;
									}
								}
							}
							else if (text == "VALIDTO")
							{
								obj = (this.ecuVariant.ValidTo != null);
							}
						}
						else if (num <= 3949971919U)
						{
							if (num <= 3780518443U)
							{
								if (num != 2915361786U)
								{
									if (num == 3780518443U)
									{
										if (text == "TITLE_EL")
										{
											obj = this.ecuVariant.Title_el;
										}
									}
								}
								else if (text == "ECUGROUPID")
								{
									obj = this.ecuVariant.EcuGroupId;
								}
							}
							else if (num != 3915430943U)
							{
								if (num == 3949971919U)
								{
									if (text == "TITLE_SV")
									{
										obj = this.ecuVariant.Title_sv;
									}
								}
							}
							else if (text == "TITLE_IT")
							{
								obj = this.ecuVariant.Title_it;
							}
						}
						else if (num <= 3998627490U)
						{
							if (num != 3950119014U)
							{
								if (num == 3998627490U)
								{
									if (text == "TITLE_ES")
									{
										obj = this.ecuVariant.Title_es;
									}
								}
							}
							else if (text == "TITLE_PT")
							{
								obj = this.ecuVariant.Title_pt;
							}
						}
						else if (num != 4183872847U)
						{
							if (num == 4249850490U)
							{
								if (text == "TITLE_FR")
								{
									obj = this.ecuVariant.Title_fr;
								}
							}
						}
						else if (text == "TITLE_ID")
						{
							obj = this.ecuVariant.Title_id;
						}
					}
					if (obj != null)
					{
						return (T)((object)Convert.ChangeType(obj, typeof(T)));
					}
				}
			}
			catch (Exception exception)
			{
				Log.WarningException("EcuVariantLocator.GetDataValue<T>()", exception);
			}
			return default(T);
		}
#endif
		private readonly PdszDatabase.EcuVar ecuVariant;

		//private readonly ISPELocator[] children;

		private ISPELocator[] parents;

		private readonly Vehicle vecInfo;

		private readonly IFFMDynamicResolver ffmResolver;
	}
}
