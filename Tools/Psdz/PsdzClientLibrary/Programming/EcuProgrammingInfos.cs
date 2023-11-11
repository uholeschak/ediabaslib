using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.Programming;
using BMW.Rheingold.Programming.API;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Tal;
using PsdzClient.Core;
using PsdzClientLibrary.Core;

namespace PsdzClient.Programming
{
	public class EcuProgrammingInfos : IEnumerable<IEcuProgrammingInfo>, IEcuProgrammingInfos, IEnumerable
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public IEcuProgrammingInfosData DataContext
		{
			get
			{
				return this.dataContext;
			}
			set
			{
				this.dataContext = value;
				this.OnPropertyChanged("DataContext");
			}
		}

        public ProgrammingObjectBuilder ProgrammingObjectBuilder
		{
			get
			{
				return this.programmingObjectBuilder;
			}
			set
			{
				this.programmingObjectBuilder = value;
			}
		}

        public EcuProgrammingInfos(IVehicle vehicle, IFFMDynamicResolver ffmResolver, bool standard = true)
		{
			//this.db = db;
			this.vehicle = vehicle;
			this.ffmResolver = ffmResolver;
			this.ecuProgrammingInfos = new List<EcuProgrammingInfo>();
			if (standard)
			{
				this.dataContext = new EcuProgrammingInfosData();
				this.ResetProgrammingInfos(false, true);
			}
		}

		~EcuProgrammingInfos()
		{
			this.UnregisterEventHandler();
		}

		public virtual void EstablishSelection()
		{
		}

		public IEnumerator<IEcuProgrammingInfo> GetEnumerator()
		{
			return this.ecuProgrammingInfos.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.ecuProgrammingInfos.GetEnumerator();
		}

		IEcuProgrammingInfo IEcuProgrammingInfos.GetItem(IEcu ecu)
		{
			EcuProgrammingInfo item = this.GetItem(ecu);
			if (item == null)
			{
				return null;
			}
			return item.Data;
		}

		public virtual IEcuProgrammingInfo GetItem(IEcu ecu, string category)
		{
			return this.GetItem(ecu);
		}

		public void SelectCodingForIndustrialCustomer(IEcu ecu, string category, bool value)
		{
			IEcuProgrammingInfo item = this.GetItem(ecu, category);
			if (item == null)
			{
				throw new ArgumentNullException("ecu");
			}
			IProgrammingAction programmingAction = item.GetProgrammingAction(ProgrammingActionType.Coding);
			if (programmingAction != null)
			{
				programmingAction.Select(value);
			}
			if (programmingAction != null)
			{
				item.IsCodingScheduled = value;
			}
		}

		public void SelectProgrammingForIndustrialCustomer(IEcu ecu, string category, bool value)
		{
			IEcuProgrammingInfo item = this.GetItem(ecu, category);
			if (item == null)
			{
				throw new ArgumentNullException("ecu");
			}
			foreach (IProgrammingAction programmingAction in item.GetProgrammingActions(null))
			{
				if (programmingAction != null)
				{
					programmingAction.Select(value);
				}
				if (programmingAction != null && programmingAction.Type == ProgrammingActionType.Coding)
				{
					item.IsCodingScheduled = value;
				}
			}
			item.IsProgrammingScheduled = value;
		}

		public void SelectReplacementForIndustrialCustomer(IEcu ecu, string category, bool value)
		{
			IEcuProgrammingInfo item = this.GetItem(ecu, category);
			if (item == null)
			{
				throw new ArgumentNullException("ecu");
			}
			IProgrammingAction programmingAction = item.GetProgrammingAction(ProgrammingActionType.Replacement);
			if (programmingAction == null)
			{
				return;
			}
			programmingAction.Select(value);
		}

		internal EcuProgrammingInfo AddEcuProgrammingInfo(IEcu ecu)
		{
			if (this.GetItemFromProgrammingInfos(ecu.ID_SG_ADR) != null)
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Diagnosis address 0x{0:X2} is already added.", ecu.ID_SG_ADR));
			}
			EcuProgrammingInfo ecuProgrammingInfo = new EcuProgrammingInfo(ecu, this.programmingObjectBuilder, true);
			this.RegisterEventHandler(ecuProgrammingInfo);
			this.ecuProgrammingInfos.Add(ecuProgrammingInfo);
			this.DataContext.List.Add(ecuProgrammingInfo.Data);
			this.AddProgrammingInfoBeforeReplace(ecuProgrammingInfo.Data);
			this.ecuProgrammingInfosMap.Add(ecuProgrammingInfo.Ecu, ecuProgrammingInfo);
			return ecuProgrammingInfo;
		}

		internal void AddProgrammingInfoBeforeReplace(IEcuProgrammingInfoData ecuProgrammingInfoData)
		{
			if (this.ProgrammingInfoCanBeAdded(ecuProgrammingInfoData))
			{
				this.DataContext.ECUsWithIndividualData.Add(ecuProgrammingInfoData);
			}
		}

		internal EcuProgrammingInfo GetItem(IEcu ecu)
		{
			if (this.ecuProgrammingInfosMap.ContainsKey(ecu))
			{
				return this.ecuProgrammingInfosMap[ecu];
			}
			return null;
		}

		internal EcuProgrammingInfo GetItemFromProgrammingInfos(long diagAddress)
		{
			using (IEnumerator<EcuProgrammingInfo> enumerator = this.ecuProgrammingInfos.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					EcuProgrammingInfo ecuProgrammingInfo = enumerator.Current;
					if (ecuProgrammingInfo.Ecu.ID_SG_ADR == diagAddress)
					{
						return ecuProgrammingInfo;
					}
				}
			}
			return null;
		}

		internal IEnumerable<IProgrammingAction> GetProgrammingActions(ProgrammingActionType[] programmingActionTypeFilter, typeDiagProtocoll[] diagProtocolFilter)
		{
			IList<IProgrammingAction> list = new List<IProgrammingAction>();
			foreach (IEcuProgrammingInfo ecuProgrammingInfo in this.ecuProgrammingInfos)
			{
				if (diagProtocolFilter == null || diagProtocolFilter.Contains(ecuProgrammingInfo.Ecu.DiagProtocoll))
				{
					list.AddRange(ecuProgrammingInfo.GetProgrammingActions(programmingActionTypeFilter));
				}
			}
			return list;
		}

		internal void RefreshProgrammingInfoBeforeReplace(IEnumerable<IEcuProgrammingInfoData> ecuProgrammingInfoData)
		{
			this.DataContext.ECUsWithIndividualData.Clear();
			foreach (IEcuProgrammingInfoData ecuProgrammingInfoData2 in ecuProgrammingInfoData)
			{
				this.AddProgrammingInfoBeforeReplace(ecuProgrammingInfoData2);
			}
		}

		internal void ResetProgrammingInfos(bool unregister = true, bool resetAll = true)
		{
			if (resetAll)
			{
				if (unregister)
				{
					this.UnregisterEventHandler();
				}
				this.programmingObjectBuilder = new ProgrammingObjectBuilder(this.vehicle as Vehicle, this.ffmResolver);
				this.CreateEcuProgrammingInfos(this.vehicle.ECU);
				this.ecuProgrammingInfosMap = new Dictionary<IEcu, EcuProgrammingInfo>();
				this.ecuProgrammingInfos.ForEach(delegate (EcuProgrammingInfo info)
				{
					this.ecuProgrammingInfosMap.Add(info.Ecu, info);
				});
				return;
			}

			foreach (IEcu key in ((IEnumerable<IEcu>)new List<IEcu>(from ecu in this.ecuProgrammingInfosMap.Keys
																	where !this.vehicle.ECU.Contains(ecu)
																	select ecu)))
			{
				EcuProgrammingInfo ecuProgrammingInfo = this.ecuProgrammingInfosMap[key];
				this.ecuProgrammingInfos.Remove(ecuProgrammingInfo);
				this.DataContext.List.Remove(ecuProgrammingInfo.Data);
				this.DataContext.ECUsWithIndividualData.Remove(ecuProgrammingInfo.Data);
				this.ecuProgrammingInfosMap.Remove(key);
			}
        }

		internal void SetSvkCurrentForEachEcu(ISvt svt)
		{
			if (svt == null)
			{
				this.SetSvkCurrentToNull();
				return;
			}
			this.UpdateProgrammingInfo(svt);
			this.RefreshProgrammingInfoBeforeReplace(this.DataContext.List.ToList<IEcuProgrammingInfoData>());
		}

		internal void SetSvkTargetForEachEcu(ISvt svt)
		{
			if (svt == null)
			{
				foreach (EcuProgrammingInfo ecuProgrammingInfo in this.ecuProgrammingInfos)
				{
					ecuProgrammingInfo.SvkTarget = null;
				}
				return;
			}
			foreach (IEcuObj ecuObj in svt.Ecus)
			{
				EcuProgrammingInfo ecuProgrammingInfo2 = this.GetItemFromProgrammingInfos((long)ecuObj.EcuIdentifier.DiagAddrAsInt);
				if (ecuProgrammingInfo2 != null)
				{
					ecuProgrammingInfo2.SvkTarget = ecuObj.StandardSvk;
				}
				else
				{
					ECU ecu = this.programmingObjectBuilder.Build(ecuObj);
					if (ecu != null)
					{
						ecuProgrammingInfo2 = this.GetEcuProgrammingInfo(ecu);
                        ecuProgrammingInfo2.SvkTarget = ecuObj.StandardSvk;
					}
				}
			}
		}

		internal void UpdateProgrammingActions(IPsdzTal tal, int escalationStep)
		{
			foreach (IPsdzEcuIdentifier psdzEcuIdentifier in tal.AffectedEcus)
			{
				EcuProgrammingInfo itemFromProgrammingInfos = this.GetItemFromProgrammingInfos((long)psdzEcuIdentifier.DiagAddrAsInt);
				if (itemFromProgrammingInfos != null)
				{
					IPsdzEcuIdentifier id = psdzEcuIdentifier;
					IEnumerable<IPsdzTalLine> talLines = from talLine in tal.TalLines
														 where talLine.EcuIdentifier.Equals(id)
														 select talLine;
					itemFromProgrammingInfos.UpdateProgrammingActions(talLines, true, escalationStep);
				}
				else
				{
					Log.Warning("EcuProgrammingInfos.UpdateProgrammingActions", "Could not find ecu programming object for 0x{0:X2}", new object[]
					{
						psdzEcuIdentifier.DiagAddrAsInt
					});
				}
			}
		}

		protected virtual void OnActionPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "IsSelected")
			{
				IProgrammingAction programmingAction = sender as IProgrammingAction;
				if (programmingAction == null)
				{
					return;
				}
				object obj = this.threadLock;
				lock (obj)
				{
					if (!programmingAction.IsSelected && this.dataContext.SelectedActionData.Contains(programmingAction.DataContext))
					{
						this.dataContext.SelectedActionData.Remove(programmingAction.DataContext);
					}
					else if (programmingAction.IsSelected && !this.dataContext.SelectedActionData.Contains(programmingAction.DataContext))
					{
						this.AddItem(programmingAction);
					}
				}
			}
		}

		protected virtual bool OnlyAddECUsWithIndividualData()
		{
			return true;
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
			if (propertyChanged == null)
			{
				return;
			}
			propertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		protected void RegisterEventHandler(IEcuProgrammingInfo ecuProgrammingInfo)
		{
			INotifyCollectionChanged notifyCollectionChanged = ecuProgrammingInfo.ProgrammingActions as INotifyCollectionChanged;
			if (notifyCollectionChanged != null)
			{
				notifyCollectionChanged.CollectionChanged += this.OnEcuProgrammingActionsChanged;
			}
		}

		protected void UnregisterEventHandler()
		{
			foreach (EcuProgrammingInfo ecuProgrammingInfo in this.ecuProgrammingInfos)
			{
				INotifyCollectionChanged notifyCollectionChanged = ((IEcuProgrammingInfo)ecuProgrammingInfo).ProgrammingActions as INotifyCollectionChanged;
				if (notifyCollectionChanged != null)
				{
					notifyCollectionChanged.CollectionChanged -= this.OnEcuProgrammingActionsChanged;
				}
				foreach (IProgrammingAction programmingAction in ((IEcuProgrammingInfo)ecuProgrammingInfo).ProgrammingActions)
				{
					programmingAction.PropertyChanged -= this.OnActionPropertyChanged;
				}
			}
		}

		private void AddItem(IProgrammingAction itemToAdd)
		{
			for (int i = 0; i < this.dataContext.SelectedActionData.Count; i++)
			{
				IProgrammingActionData programmingActionData = this.dataContext.SelectedActionData[i];
				if (itemToAdd.DataContext.Order < programmingActionData.Order)
				{
					this.dataContext.SelectedActionData.Insert(i, itemToAdd.DataContext);
					this.OnPropertyChanged("SelectedActions");
					return;
				}
			}
			this.dataContext.SelectedActionData.Add(itemToAdd.DataContext);
			this.OnPropertyChanged("SelectedActions");
		}

		private void CreateEcuProgrammingInfos(IEnumerable<IEcu> ecus)
		{
			if (ecus == null)
			{
				throw new ArgumentNullException();
			}
			this.ecuProgrammingInfos.Clear();
			IList<EcuProgrammingInfoData> list = new List<EcuProgrammingInfoData>();
			foreach (IEcu ecu in ecus)
			{
				EcuProgrammingInfo ecuProgrammingInfo = new EcuProgrammingInfo(ecu, this.programmingObjectBuilder, true);
				this.RegisterEventHandler(ecuProgrammingInfo);
				this.ecuProgrammingInfos.Add(ecuProgrammingInfo);
				list.Add(ecuProgrammingInfo.Data);
			}
			this.RefreshProgrammingInfoBeforeReplace(list);
			this.RefreshProgrammingInfo(list);
		}

		private EcuProgrammingInfo GetEcuProgrammingInfo(ECU ecuFromTargetSvt)
		{
			EcuProgrammingInfo ecuProgrammingInfo = new EcuProgrammingInfo(ecuFromTargetSvt, this.programmingObjectBuilder, true);
			this.ecuProgrammingInfos.Add(ecuProgrammingInfo);
			this.DataContext.List.Add(ecuProgrammingInfo.Data);
			this.AddProgrammingInfoBeforeReplace(ecuProgrammingInfo.Data);
			this.ecuProgrammingInfosMap.Add(ecuFromTargetSvt, ecuProgrammingInfo);
			return ecuProgrammingInfo;
		}

		private void OnEcuProgrammingActionsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			NotifyCollectionChangedAction action = e.Action;
			if (action <= NotifyCollectionChangedAction.Remove)
			{
				foreach (object obj in e.NewItems)
				{
					IProgrammingAction programmingAction = obj as IProgrammingAction;
					if (programmingAction != null)
					{
						object obj2 = this.threadLock;
						lock (obj2)
						{
							if (e.Action == NotifyCollectionChangedAction.Add)
							{
								programmingAction.PropertyChanged += this.OnActionPropertyChanged;
								if (programmingAction.IsSelected && !this.dataContext.SelectedActionData.Contains(programmingAction.DataContext))
								{
									this.AddItem(programmingAction);
								}
							}
							else if (e.Action == NotifyCollectionChangedAction.Remove)
							{
								programmingAction.PropertyChanged -= this.OnActionPropertyChanged;
								if (this.dataContext.SelectedActionData.Contains(programmingAction.DataContext))
								{
									this.dataContext.SelectedActionData.Remove(programmingAction.DataContext);
								}
							}
						}
					}
				}
			}
		}

		private bool ProgrammingInfoCanBeAdded(IEcuProgrammingInfoData ecuProgrammingInfoData)
		{
			return !this.OnlyAddECUsWithIndividualData() || this.vehicle.IsMotorcycle() || (ecuProgrammingInfoData.Ecu != null && ecuProgrammingInfoData.Ecu.StatusInfo != null && ecuProgrammingInfoData.Ecu.StatusInfo.HasIndividualData);
		}

		private void RefreshProgrammingInfo(IList<EcuProgrammingInfoData> ecuProgrammingInfoDataList)
		{
			this.DataContext.List.Clear();
			this.DataContext.List.AddRange(ecuProgrammingInfoDataList);
		}

		private void SetSvkCurrentToNull()
		{
			foreach (EcuProgrammingInfo ecuProgrammingInfo in this.ecuProgrammingInfos)
			{
				ecuProgrammingInfo.SvkCurrent = null;
			}
		}

		private void UpdateProgrammingInfo(ISvt svt)
		{
			foreach (IEcuObj ecuObj in svt.Ecus)
			{
				EcuProgrammingInfo itemFromProgrammingInfos = this.GetItemFromProgrammingInfos((long)ecuObj.EcuIdentifier.DiagAddrAsInt);
				if (itemFromProgrammingInfos != null)
				{
					if (ecuObj.BnTnName == null && itemFromProgrammingInfos.Ecu.ProgrammingVariantName != null)
					{
#if false
						Log.Info("EcuProgrammingInfos.SetSvkCurrentForEachEcu()", "Do not replace \"{0}\" with null for ECU: {1}.", new object[]
						{
							itemFromProgrammingInfos.Ecu.ProgrammingVariantName,
							itemFromProgrammingInfos.Ecu.LogEcu()
						});
#endif
					}
					else
					{
						itemFromProgrammingInfos.Ecu.ProgrammingVariantName = ecuObj.BnTnName;
					}
					itemFromProgrammingInfos.Ecu.StatusInfo = ecuObj.EcuStatusInfo;
					itemFromProgrammingInfos.SvkCurrent = ecuObj.StandardSvk;
				}
			}
		}

		protected readonly IList<EcuProgrammingInfo> ecuProgrammingInfos;

		protected IDictionary<IEcu, EcuProgrammingInfo> ecuProgrammingInfosMap;

		protected IVehicle vehicle;

		//private readonly IDatabaseProvider db;

		private readonly IFFMDynamicResolver ffmResolver;

		private readonly object threadLock = new object();

		private IEcuProgrammingInfosData dataContext;

		private ProgrammingObjectBuilder programmingObjectBuilder;
	}
}
