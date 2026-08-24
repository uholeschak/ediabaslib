using PsdzClient.Contracts;
using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BMW.Authoring.API.Implementation.Sfa.Models
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface ISfaTokenPackageResponse : IBoolResultObject
    {
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        SfaOverAllStatus Status { get; set; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        List<SfaToken> TokenPackage { get; set; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string TokenPackageReference { get; set; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string VIN17 { get; set; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        int MessageFormatVersion { get; set; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        DateTime Date { get; set; }
    }
}
