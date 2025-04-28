using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Exceptions;
using BMW.Rheingold.Psdz.Model.Swt;
using BMW.Rheingold.Psdz.Model.Tal;
using BMW.Rheingold.Psdz.Model.Tal.TalFilter;

namespace BMW.Rheingold.Psdz
{
    public enum PsdzTalFilterAction
    {
        AllowedToBeTreated,
        Empty,
        MustBeTreated,
        MustNotBeTreated,
        OnlyToBeTreatedAndBlockCategoryInAllEcu
    }

    [ServiceContract(SessionMode = SessionMode.Required)]
    [ServiceKnownType(typeof(PsdzTalFilter))]
    [ServiceKnownType(typeof(PsdzTal))]
    [ServiceKnownType(typeof(PsdzFa))]
    [ServiceKnownType(typeof(PsdzSwtAction))]
    [ServiceKnownType(typeof(PsdzEcuIdentifier))]
    [ServiceKnownType(typeof(PsdzSweTalFilterOptions))]
    public interface IObjectBuilderService
    {
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzTalFilter BuildEmptyTalFilter();

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzFa BuildFa(IPsdzFa faInput);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzFa BuildFaFromXml(string xml);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzSwtAction BuildSwtActionFromXml(string xml);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzTal BuildTalFromXml(string xml);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzTal BuildEmptyTal();

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzTalFilter DefineFilterForAllEcus(PsdzTaCategories[] psdzTaCategories, PsdzTalFilterAction talFilterAction, IPsdzTalFilter filter);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzTalFilter DefineFilterForSelectedEcus(PsdzTaCategories[] psdzTaCategories, int[] diagAddress, PsdzTalFilterAction talFilterAction, IPsdzTalFilter filter, IDictionary<string, PsdzTalFilterAction> smacFilter = null);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzTalFilter DefineFilterForSwes(int diagAddress, PsdzTalFilterAction talFilterAction, PsdzTaCategories category, IList<IPsdzSweTalFilterOptions> sweTaFilters, IPsdzTalFilter filter);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzTal UpdateTalEcus(IPsdzTal psdzTal, IEnumerable<IPsdzEcuIdentifier> installedEcuListIst, IEnumerable<IPsdzEcuIdentifier> installedEcuListSoll);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzTal SetPreferredFlashProtocol(IPsdzTal tal, IPsdzEcuIdentifier ecu, PsdzProtocol psdzProtocol);
    }
}
