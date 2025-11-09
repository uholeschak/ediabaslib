using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework.Contracts.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IDtc : INotifyPropertyChanged
    {
        IDtcContext Current { get; }

        IEnumerable<IDtcContext> DTCContext { get; }

        bool FS_LESEN_DETAIL_SUCCESSFULLY { get; }

        bool FS_LESEN_SUCCESSFULLY { get; }

        long? F_ART { get; }

        IEnumerable<IFArtExt> F_ART_EXT { get; }

        string F_CODE { get; }

        int? F_EREIGNIS_DTC { get; }

        int? F_FEHLERKLASSE_NR { get; }

        string F_FEHLERKLASSE_TEXT { get; }

        byte[] F_HEX_CODE { get; }

        long? F_HFK { get; }

        long? F_HLZ { get; }

        long? F_LZ { get; }

        long? F_ORT { get; }

        string F_ORT_TEXT { get; }

        ushort? F_PCODE { get; }

        string F_PCODE_STRING { get; }

        string F_PCODE_TEXT { get; }

        int? F_READY_NR { get; }

        string F_READY_TEXT { get; }

        uint? F_SAE_CODE { get; }

        string F_SAE_CODE_STRING { get; }

        string F_SAE_CODE_TEXT { get; }

        int? F_SYMPTOM_NR { get; }

        string F_SYMPTOM_TEXT { get; }

        long? F_UEBERLAUF { get; }

        IEnumerable<IDtcUmweltDisplay> F_UW_Display { get; }

        long? F_UW_KM { get; }

        double? F_UW_KM_SUPREME { get; }

        long F_UW_KM_Min { get; }

        long F_UW_ZEIT { get; }

        double? F_UW_ZEIT_SUPREME { get; }

        long? F_VERSION { get; }

        int? F_VORHANDEN_NR { get; }

        string F_VORHANDEN_TEXT { get; }

        int? F_WARNUNG_NR { get; }

        string F_WARNUNG_TEXT { get; }

        IDtcContext First { get; }

        IDtcContext Second { get; }

        string FortAsHexString { get; }

        decimal? Id { get; }

        bool IsCombined { get; }

        bool IsVirtual { get; }

        bool? Relevance { get; }

        bool IsFSLesenExpert { get; set; }
    }
}