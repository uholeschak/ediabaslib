using PsdzClient.Core;
using PsdzClient.Programming;
using System.Globalization;

namespace PsdzClient.Programming
{
    public class EnablingCodeData
    {
        public int DiagAddrAsInt { get; set; }

        public int ApplicationNumber { get; set; }

        public int SoftwareUpdateIndex { get; set; }

        public string Title { get; set; }

        public FscState FscState { get; set; }

        public static string GetEnablingCodeName(int applicationId, int upgradeIndex, Vehicle vehicle, IFFMDynamicResolver dynamicResolver)
        {
#if false
            IDatabaseProvider instance = DatabaseProviderFactory.Instance;
            XEP_SWIACTIVATIONCODE_SWT xEP_SWIACTIVATIONCODE_SWT = null;
            xEP_SWIACTIVATIONCODE_SWT = instance.GetSwiActivationCodeSWTByAppIdUpgIdx(applicationId.ToString(CultureInfo.InvariantCulture), upgradeIndex.ToString(CultureInfo.InvariantCulture), vehicle, dynamicResolver) ?? instance.GetSwiActivationCodeSWTByAppIdUpgIdx(applicationId.ToString(CultureInfo.InvariantCulture), "65535", vehicle, dynamicResolver);
            if (xEP_SWIACTIVATIONCODE_SWT == null)
            {
                Log.Warning(Log.CurrentMethod(), $"Unable to resolve title for enabling code data with applicationId {applicationId} and upgradeIndex {upgradeIndex}.");
                return FormatedData.Localize("#TherapyPlanEntryOrigin.unknown");
            }
            string text = (string.IsNullOrEmpty(xEP_SWIACTIVATIONCODE_SWT.Title) ? xEP_SWIACTIVATIONCODE_SWT.Title_engb : xEP_SWIACTIVATIONCODE_SWT.Title);
#else
            string text = string.Empty;
#endif
            if (string.IsNullOrEmpty(text))
            {
                Log.Warning(Log.CurrentMethod(), $"Unable to resolve title translation for enabling code data with applicationId {applicationId} and upgradeIndex {upgradeIndex}, even after trying the fallback.");
                return FormatedData.Localize("#TherapyPlanEntryOrigin.unknown");
            }
            return text;
        }
    }
}