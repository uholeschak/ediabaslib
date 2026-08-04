using PsdzClient.Core;
using System;
using System.ComponentModel;

namespace BMW.Authoring.API
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface ICertificateDocumentCreator : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        string CreateChargingCablePass(ITextLocator CustomerMode2, ITextLocator Customer_Flex, ITextLocator Customer_Mode3, ITextLocator Workshop_Mode2, ITextLocator Workshop_Flex, ITextLocator Workshop_Mode3);

        [EditorBrowsable(EditorBrowsableState.Always)]
        string CreateBatteryHealthPass(string caseCondition, int batteryCapacity, string caseSerialnumber);

        [EditorBrowsable(EditorBrowsableState.Always)]
        string CreateBatteryQuickHealthCheck(int batteryCapacity, string caseSerialnumber);

        [EditorBrowsable(EditorBrowsableState.Always)]
        [Obsolete("CreateEdrDataPwDocument is obsolete.", false)]
        string CreateEdrDataPwDocument(string password);

        [EditorBrowsable(EditorBrowsableState.Always)]
        string CreateHvComponentCertificate(string serialNumber, bool result, string testType);
    }

}
