using BMW.Rheingold.Psdz.Model.Swt;
using PsdzClient.Programming;

namespace BMW.Rheingold.Psdz
{
    internal class SwtApplicationMapper
    {
        private static FscStateEnumMapper _fscStateEnumMapper = new FscStateEnumMapper();

        private static SoftwareSigStateEnumMapper _softwareSigStateEnumMapper = new SoftwareSigStateEnumMapper();

        private static SwtActionTypeMapper _swtActionTypeEnumMapper = new SwtActionTypeMapper();

        private static SwtTypeEnumMapper _swtTypeEnumMapper = new SwtTypeEnumMapper();

        private static FscCertStateEnumMapper _fscCertStateEnumMapper = new FscCertStateEnumMapper();

        public static SwtApplicationModel Map(IPsdzSwtApplication swtApplication)
        {
            if (swtApplication == null)
            {
                return null;
            }
            return new SwtApplicationModel
            {
                BackupPossible = swtApplication.IsBackupPossible,
                Fsc = swtApplication.Fsc,
                FscCert = swtApplication.FscCert,
                FscCertState = _fscCertStateEnumMapper.GetValue(swtApplication.FscCertState),
                FscState = _fscStateEnumMapper.GetValue(swtApplication.FscState),
                Position = swtApplication.Position,
                SwtApplicationId = SwtApplicationIdMapper.Map(swtApplication.SwtApplicationId),
                SwtActionType = _swtActionTypeEnumMapper.GetValue(swtApplication.SwtActionType),
                SwtType = _swtTypeEnumMapper.GetValue(swtApplication.SwtType),
                SoftwareSigState = _softwareSigStateEnumMapper.GetValue(swtApplication.SoftwareSigState)
            };
        }

        public static IPsdzSwtApplication Map(SwtApplicationModel model)
        {
            if (model == null)
            {
                return null;
            }
            return new PsdzSwtApplication
            {
                IsBackupPossible = model.BackupPossible,
                Fsc = model.Fsc,
                FscCert = model.FscCert,
                FscCertState = _fscCertStateEnumMapper.GetValue(model.FscCertState),
                FscState = _fscStateEnumMapper.GetValue(model.FscState),
                Position = model.Position,
                SwtApplicationId = SwtApplicationIdMapper.Map(model.SwtApplicationId),
                SwtActionType = _swtActionTypeEnumMapper.GetValue(model.SwtActionType),
                SwtType = _swtTypeEnumMapper.GetValue(model.SwtType),
                SoftwareSigState = _softwareSigStateEnumMapper.GetValue(model.SoftwareSigState)
            };
        }
    }
}