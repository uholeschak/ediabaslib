using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Localization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa.LocalizableMessageTo
{
    public interface ILocalizableMessageTo : ILocalizableMessage
    {
        string Description { get; }
    }
}
