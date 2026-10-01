using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Localization;

namespace BMW.Rheingold.Psdz.Model.Sfa.LocalizableMessageTo
{
    public interface ILocalizableMessageTo : ILocalizableMessage
    {
        string Description { get; }
    }
}
