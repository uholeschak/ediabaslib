using BMW.Authoring;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Core;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public interface IFA : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        bool KWort_IsSet(ProductType produktart, string KWort, params string[] KWort_);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool KWort_IsSet(ProductType produktart, string[] KWort);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool EWort_IsSet(ProductType produktart, string EWort, params string[] EWort_);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool EWort_IsSet(ProductType produktart, string[] EWort);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool SA_IsSet(ProductType produktart, string SA, params string[] SA_);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool SA_IsSet(ProductType produktart, string[] SA);

        [EditorBrowsable(EditorBrowsableState.Always)]
        ITextContent SA_GetTitel(ProductType produktart, string SA);

        [EditorBrowsable(EditorBrowsableState.Always)]
        ITextContent EWort_GetTitel(ProductType produktart, string EWort);

        [EditorBrowsable(EditorBrowsableState.Always)]
        ITextContent KWort_GetTitel(ProductType produktart, string kWort);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool SX_IsSet(ProductType produktart, string SX, params string[] SX_);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool SX_IsSet(ProductType produktart, string[] SX);

        [EditorBrowsable(EditorBrowsableState.Always)]
        ICollection<LocalizedSAItem> SaLocalizedItems_GetList();

        [EditorBrowsable(EditorBrowsableState.Always)]
        ITextContent STANDARD_FA_Get();

        [EditorBrowsable(EditorBrowsableState.Always)]
        ICollection SA_GetList();

        [EditorBrowsable(EditorBrowsableState.Always)]
        ICollection EWort_GetList();

        [EditorBrowsable(EditorBrowsableState.Always)]
        ICollection KWort_GetList();
    }
}
