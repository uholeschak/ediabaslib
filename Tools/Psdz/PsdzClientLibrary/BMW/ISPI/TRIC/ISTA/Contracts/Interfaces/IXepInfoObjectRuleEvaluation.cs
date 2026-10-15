using System;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Interfaces
{
    public interface IXepInfoObjectRuleEvaluation : IMultilanguageTitle
    {
        bool IsOriginatedFromParentNode { get; set; }

        bool IsNews { get; }

        bool IsRgNews { get; }

        string Title { get; }

        decimal Id { get; }

        decimal? ControlId { get; }

        string DocumentType { get; }

        string Identifikator { get; }

        string InfoType { get; }

        decimal? Nodeclass { get; }

        decimal? Assembly { get; }

        decimal? DebugInfo { get; }

        string UsedDeviceAdapters { get; }

        decimal? VersionNumber { get; }

        string ProgramType { get; }

        DateTime? ValidFrom { get; }

        DateTime? ValidTo { get; }

        decimal? SicherheitsRelevant { get; }

        decimal? TitleId { get; }

        decimal? Generell { get; }

        decimal? TeleserviceKennung { get; }

        decimal? FahrzeugKommunikation { get; }

        decimal? Messtechnik { get; }

        decimal? Versteckt { get; }

        decimal? HinweisId { get; }

        string Hinweis_dede { get; }

        string Hinweis_engb { get; }

        string Hinweis_enus { get; }

        string Hinweis_fr { get; }

        string Hinweis_th { get; }

        string Hinweis_sv { get; }

        string Hinweis_it { get; }

        string Hinweis_es { get; }

        string Hinweis_id { get; }

        string Hinweis_ko { get; }

        string Hinweis_el { get; }

        string Hinweis_tr { get; }

        string Hinweis_zhcn { get; }

        string Hinweis_ru { get; }

        string Hinweis_nl { get; }

        string Hinweis_pt { get; }

        string Hinweis_zhtw { get; }

        string Hinweis_ja { get; }

        string Hinweis_plpl { get; }

        string Hinweis_cscz { get; }

        string Name { get; }

        string InformationsTyp { get; }

        DateTime? CreateDate { get; }

        DateTime? ExpiryDate { get; }

        DateTime? ChangeDate { get; }

        DateTime? LaunchDate { get; }

        decimal? Abgasrelevant { get; }

        string Dringlichkeit { get; }

        string Informationsformat { get; }

        string Grobzeichen { get; }

        string AwNummer { get; }

        string SwzNummer { get; }

        string SiNummer { get; }

        string ZielIStufe { get; }

        DateTime? ModificationTime { get; }

        string InfoFormat { get; }

        string DocNumber { get; }

        decimal? Priority { get; }

        string Identifier { get; }

        decimal? SafetyRelatedInfo { get; }

        bool IsSuspicious { get; }

        bool FastNavigation { get; }

        string GetLocalizedInfoObjectTitle(string language);
    }
}
