using BMW.Rheingold.Psdz.Model.Tal;

namespace BMW.Rheingold.Psdz
{
    internal static class TalLineMapper
    {
        public static TaCategoryTypeMapper _taCategoryTypeMapper = new TaCategoryTypeMapper();
        public static IPsdzTalLine Map(TalLineModel talLineModel)
        {
            if (talLineModel == null)
            {
                return null;
            }

            PsdzTalLine psdzTalLine = TalElementMapper.Map<PsdzTalLine>(talLineModel.TalElement);
            psdzTalLine.EcuIdentifier = EcuIdentifierMapper.Map(talLineModel.EcuIdentifier);
            psdzTalLine.FscBackup = TaCategoryMapper.Map<PsdzFscBackup>(talLineModel.FscBackup);
            psdzTalLine.FscDeploy = TaCategoryMapper.Map<PsdzFscDeploy>(talLineModel.FscDeploy);
            psdzTalLine.BlFlash = TaCategoryMapper.Map<PsdzBlFlash>(talLineModel.BlFlash);
            psdzTalLine.IbaDeploy = TaCategoryMapper.Map<PsdzIbaDeploy>(talLineModel.IbaDeploy);
            psdzTalLine.SwDeploy = TaCategoryMapper.Map<PsdzSwDeploy>(talLineModel.SwDeploy);
            psdzTalLine.IdBackup = TaCategoryMapper.Map<PsdzIdBackup>(talLineModel.IdBackup);
            psdzTalLine.IdRestore = TaCategoryMapper.Map<PsdzIdRestore>(talLineModel.IdRestore);
            psdzTalLine.SFADeploy = TaCategoryMapper.Map<PsdzSFADeploy>(talLineModel.SfaDeploy);
            psdzTalLine.HddUpdate = TaCategoryMapper.Map<PsdzHddUpdate>(talLineModel.HddUpdate);
            psdzTalLine.TaCategories = _taCategoryTypeMapper.GetValue(talLineModel.TaCategories);
            psdzTalLine.TaCategory = TaCategoryMapper.Map<PsdzTaCategory>(talLineModel.TaCategory);
            psdzTalLine.SmacTransferStart = TaCategoryMapper.Map<PsdzSmacTransferStart>(talLineModel.SmacTransferStart);
            psdzTalLine.SmacTransferStatus = TaCategoryMapper.Map<PsdzSmacTransferStatus>(talLineModel.SmacTransferStatus);
            psdzTalLine.EcuMirrorDeploy = TaCategoryMapper.Map<PsdzEcuMirrorDeploy>(talLineModel.EcuMirrorDeploy);
            psdzTalLine.EcuActivate = TaCategoryMapper.Map<PsdzEcuActivate>(talLineModel.EcuActivate);
            psdzTalLine.EcuPoll = TaCategoryMapper.Map<PsdzEcuPoll>(talLineModel.EcuPoll);
            return psdzTalLine;
        }

        public static TalLineModel Map(IPsdzTalLine psdzTalLine)
        {
            if (psdzTalLine == null)
            {
                return null;
            }

            TalElementModel talElement = TalElementMapper.Map(psdzTalLine);
            return new TalLineModel
            {
                TalElement = talElement,
                EcuIdentifier = EcuIdentifierMapper.Map(psdzTalLine.EcuIdentifier),
                FscBackup = TaCategoryMapper.Map<FscBackupModel>(psdzTalLine.FscBackup),
                FscDeploy = TaCategoryMapper.Map<FscDeployModel>(psdzTalLine.FscDeploy),
                BlFlash = TaCategoryMapper.Map<BlFlashModel>(psdzTalLine.BlFlash),
                IbaDeploy = TaCategoryMapper.Map<IbaDeployModel>(psdzTalLine.IbaDeploy),
                SwDeploy = TaCategoryMapper.Map<SwDeployModel>(psdzTalLine.SwDeploy),
                IdBackup = TaCategoryMapper.Map<IdBackupModel>(psdzTalLine.IdBackup),
                IdRestore = TaCategoryMapper.Map<IdRestoreModel>(psdzTalLine.IdRestore),
                SfaDeploy = TaCategoryMapper.Map<SFADeployModel>(psdzTalLine.SFADeploy),
                HddUpdate = TaCategoryMapper.Map<HddUpdateModel>(psdzTalLine.HddUpdate),
                TaCategories = _taCategoryTypeMapper.GetValue(psdzTalLine.TaCategories),
                TaCategory = TaCategoryMapper.Map<TaCategoryModel>(psdzTalLine.TaCategory),
                SmacTransferStart = TaCategoryMapper.Map<SmacTransferStartModel>(psdzTalLine.SmacTransferStart),
                SmacTransferStatus = TaCategoryMapper.Map<SmacTransferStatusModel>(psdzTalLine.SmacTransferStatus),
                EcuMirrorDeploy = TaCategoryMapper.Map<EcuMirrorDeployModel>(psdzTalLine.EcuMirrorDeploy),
                EcuActivate = TaCategoryMapper.Map<EcuActivateModel>(psdzTalLine.EcuActivate),
                EcuPoll = TaCategoryMapper.Map<EcuPollModel>(psdzTalLine.EcuPoll)
            };
        }
    }
}