using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Core;
using System.Collections.Generic;
using BMW.Rheingold.CoreFramework.DatabaseProvider;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public static class RheingoldProgammingObjectBuilder
    {
        public static IRxSwinObject CreateRxSwinObject(string hexAddress, List<string> rxSwins, bool isMasterEcu, bool isNegativeJobResult, string negativeJobResult)
        {
            return new RxSwinObject(hexAddress, rxSwins, isMasterEcu, isNegativeJobResult, negativeJobResult);
        }

        public static IPlannedSwiAction CreatePlannedSwiAction(string name, bool isDisabled)
        {
            return new PlannedSwiAction(name, isDisabled);
        }
    }
}
