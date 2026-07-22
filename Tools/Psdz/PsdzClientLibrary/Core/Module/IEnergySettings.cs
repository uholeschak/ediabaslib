using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework.EnergySettings
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    public interface IEnergySettings
    {
        void PreventSleepMode();

        void RestoreEnergySettingsInfo();
    }
}
