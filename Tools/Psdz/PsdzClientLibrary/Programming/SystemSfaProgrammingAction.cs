using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Core;
using System;
using System.ComponentModel;

namespace PsdzClient.Programming
{
    internal class SystemSfaProgrammingAction : ProgrammingAction, ISystemSfaProgrammingAction, IProgrammingAction, INotifyPropertyChanged, IComparable<IProgrammingAction>, ITherapyPlanAction2, ITherapyPlanAction
    {
        internal SystemSfaProgrammingAction(IEcu parentEcu, ProgrammingActionType type, bool isEditable, int order)
            : base(parentEcu, type, isEditable, order)
        {
            switch (type)
            {
                case ProgrammingActionType.SFAWrite:
                    titleTextId = "#ProgrammingActionType.SFAWriteSystemFunctions";
                    break;
                case ProgrammingActionType.SFADelete:
                    titleTextId = "#ProgrammingActionType.SFADeleteSystemFunctions";
                    break;
                default:
                    throw new NotSupportedException("Creating a SystemSfaProgrammingAction with a ProgrammingActionType other than SFAWrite or SFADelete is not supported.");
            }
            base.Title = ProgrammingAction.BuildTitle(base.Type, base.ParentEcu, ConfigSettings.CurrentUICulture, titleTextId);
        }
    }
}