using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Tal;
using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace BMW.Rheingold.Psdz
{
    internal static class TaMapper
    {
        private static readonly SwtActionTypeMapper _swtActionTypeMapper = new SwtActionTypeMapper();

        private static readonly ProtocolMapper protocolMapper = new ProtocolMapper();

        private static readonly IDictionary<string, Func<TaModel, IPsdzTa>> _funcMapTaModelToPsdzTa = CreateFuncMapTaModelToPsdzTa();

        private static readonly IDictionary<string, Func<IPsdzTa, TaModel>> _funcMapPsdzTaToTaModel = CreateFuncMapPsdzTaToTaModel();

        public static IPsdzTa Map(TaModel taModel)
        {
            if (taModel == null)
            {
                return null;
            }
            string name = taModel.GetType().Name;
            if (_funcMapTaModelToPsdzTa.ContainsKey(name))
            {
                return _funcMapTaModelToPsdzTa[name](taModel);
            }
            Log.Warning(Log.CurrentMethod(), string.Format(CultureInfo.InvariantCulture, "No serializable class found for class type '{0}'! Base class 'PsdzTa' will be used instead.", name));
            return BuildPsdzTa<PsdzTa>(taModel);
        }

        public static TaModel Map(IPsdzTa taModel)
        {
            if (taModel == null)
            {
                return null;
            }
            string name = taModel.GetType().Name;
            if (_funcMapPsdzTaToTaModel.ContainsKey(name))
            {
                return _funcMapPsdzTaToTaModel[name](taModel);
            }
            Log.Warning(Log.CurrentMethod(), string.Format(CultureInfo.InvariantCulture, "No serializable class found for class type '{0}'! Base class 'TaModel' will be used instead.", name));
            return BuildTaModel<TaModel>(taModel);
        }

        private static TTarget BuildPsdzTa<TTarget>(TaModel ta) where TTarget : PsdzTa, new()
        {
            TTarget val = TalElementMapper.Map<TTarget>(ta.TalElement);
            val.SgbmId = SgbmIdMapper.Map(ta.SgbmId);
            return val;
        }

        private static TTarget BuildTaModel<TTarget>(IPsdzTa psdzTa) where TTarget : TaModel, new()
        {
            TalElementModel talElement = TalElementMapper.Map(psdzTa);
            return new TTarget
            {
                SgbmId = SgbmIdMapper.Map(psdzTa.SgbmId),
                TalElement = talElement
            };
        }

        private static IDictionary<string, Func<TaModel, IPsdzTa>> CreateFuncMapTaModelToPsdzTa()
        {
            return new Dictionary<string, Func<TaModel, IPsdzTa>>
        {
            { "IdRestoreTaModel", BuildPsdzIdRestoreTa },
            { "IdLightTaModel", BuildPsdzldBasisLightTa },
            { "FscDeployTaModel", BuildPsdzFscDeployTa },
            { "SFADeleteTAModel", BuildPsdzSFADeleteTa },
            { "SFAVerifyTAModel", BuildPsdzSFAVerifyTa },
            { "SFAWriteTAModel", BuildPsdzSFAWriteTa },
            { "HddUpdateTaModel", BuildPsdzHddUpdateTa },
            { "BlFlashTaModel", BuildPsdzBlFlashTa },
            { "SwDeployTaModel", BuildPsdzSwDeployTa },
            { "IbaDeployTaModel", BuildIbaDeployTa },
            { "SmacSwDeployOnMasterTaModel", BuildSmacMasterTaModel },
            { "SmacTransferStartTaModel", BuildSmacTransferStartTaModel },
            { "SmacTransferStatusTaModel", BuildSmacTransferStatusTaModel },
            { "EcuMirrorDeployTaModel", BuildPsdzEcuMirrorDeployTa },
            { "SmacEcuMirrorDeployOnMasterTaModel", BuildPsdzSmacEcuMirrorDeployOnMasterTa },
            { "EcuActivateTaModel", BuildPsdzEcuActivateTa },
            { "EcuPollTaModel", BuildPsdzEcuPollTa },
            { "TaModel", BuildPsdzTa<PsdzTa> }
        };
        }

        private static IDictionary<string, Func<IPsdzTa, TaModel>> CreateFuncMapPsdzTaToTaModel()
        {
            return new Dictionary<string, Func<IPsdzTa, TaModel>>
        {
            { "PsdzIdRestoreTa", BuildIdRestoreTaModel },
            { "PsdzIdRestoreLightTa", BuildIdBasisLightTaModel },
            { "PsdzIdBackupLightTa", BuildIdBasisLightTaModel },
            { "PsdzFscDeployTa", BuildFscDeployTaModel },
            { "PsdzSFADeleteTA", BuildSFADeleteTaModel },
            { "PsdzSFAVerifyTA", BuildSFAVerifyTaModel },
            { "PsdzSFAWriteTA", BuildSFAWriteTaModel },
            { "PsdzHddUpdateTA", BuildHddUpdateTaModel },
            { "PsdzBlFlashTa", BuildBlFlashTaModel },
            { "PsdzSwDeployTa", BuildSwDeployTaModel },
            { "PsdzIbaDeployTa", BuildIbaDeployTaModel },
            { "PsdzSmacSwDeployOnMasterTA", BuildSmacMasterTaModel },
            { "PsdzSmacTransferStartTA", BuildSmacTransferStartTaModel },
            { "PsdzSmacTransferStatusTA", BuildSmacTransferStatusTaModel },
            { "PsdzEcuMirrorDeployTa", BuildEcuMirrorDeployTaModel },
            { "PsdzSmacEcuMirrorDeployOnMasterTA", BuildSmacEcuMirrorDeployTaModel },
            { "PsdzEcuActivateTa", BuildEcuActivateTaModel },
            { "PsdzEcuPollTa", BuildEcuPollTaModel },
            { "PsdzTa", BuildTaModel<TaModel> }
        };
        }

        private static PsdzIbaDeployTa BuildIbaDeployTa(TaModel model)
        {
            IbaDeployTaModel ibaDeployTaModel = (IbaDeployTaModel)model;
            PsdzIbaDeployTa psdzIbaDeployTa = BuildPsdzTa<PsdzIbaDeployTa>(ibaDeployTaModel);
            psdzIbaDeployTa.ActualProtocol = protocolMapper.GetValue(ibaDeployTaModel.ActualProtocol);
            psdzIbaDeployTa.PreferredProtocol = protocolMapper.GetValue(ibaDeployTaModel.PreferredProtocol);
            psdzIbaDeployTa.SgbmId = SgbmIdMapper.Map(ibaDeployTaModel.SgbmId);
            return psdzIbaDeployTa;
        }

        private static PsdzHddUpdateTA BuildPsdzHddUpdateTa(TaModel taModel)
        {
            HddUpdateTaModel hddUpdateTaModel = (HddUpdateTaModel)taModel;
            PsdzHddUpdateTA psdzHddUpdateTA = BuildPsdzTa<PsdzHddUpdateTA>(hddUpdateTaModel);
            psdzHddUpdateTA.SecondsToCompletion = hddUpdateTaModel.SecondsToCompletion;
            return psdzHddUpdateTA;
        }

        private static PsdzFscDeployTa BuildPsdzFscDeployTa(TaModel taModel)
        {
            FscDeployTaModel fscDeployTaModel = (FscDeployTaModel)taModel;
            PsdzFscDeployTa psdzFscDeployTa = BuildPsdzTa<PsdzFscDeployTa>(fscDeployTaModel);
            psdzFscDeployTa.Action = _swtActionTypeMapper.GetValue(fscDeployTaModel.Action);
            psdzFscDeployTa.ApplicationId = SwtApplicationIdMapper.Map(fscDeployTaModel.ApplicationId);
            psdzFscDeployTa.Fsc = fscDeployTaModel.Fsc;
            psdzFscDeployTa.FscCert = fscDeployTaModel.FscCert;
            return psdzFscDeployTa;
        }

        private static PsdzIdRestoreTa BuildPsdzIdRestoreTa(TaModel taModel)
        {
            IdRestoreTaModel idRestoreTaModel = (IdRestoreTaModel)taModel;
            PsdzIdRestoreTa psdzIdRestoreTa = BuildPsdzTa<PsdzIdRestoreTa>(idRestoreTaModel);
            psdzIdRestoreTa.BackupFile = idRestoreTaModel.BackupFile;
            return psdzIdRestoreTa;
        }

        private static PsdzIdLightBasisTa BuildPsdzldBasisLightTa(TaModel taModel)
        {
            if (taModel == null)
            {
                return null;
            }
            PsdzIdLightBasisTa psdzIdLightBasisTa = null;
            IdLightTaModel idLightTaModel = (IdLightTaModel)taModel;
            switch (idLightTaModel.IdLightTaType)
            {
                case IdLightTaTypeModel.IdBackup:
                    psdzIdLightBasisTa = BuildPsdzTa<PsdzIdBackupLightTa>(taModel);
                    break;
                case IdLightTaTypeModel.IdRestore:
                    psdzIdLightBasisTa = BuildPsdzTa<PsdzIdRestoreLightTa>(taModel);
                    break;
            }
            if (psdzIdLightBasisTa != null)
            {
                psdzIdLightBasisTa.Ids = idLightTaModel.Ids;
            }
            return psdzIdLightBasisTa;
        }

        private static PsdzSFADeleteTA BuildPsdzSFADeleteTa(TaModel taModel)
        {
            SFADeleteTAModel sFADeleteTAModel = (SFADeleteTAModel)taModel;
            PsdzSFADeleteTA psdzSFADeleteTA = BuildPsdzTa<PsdzSFADeleteTA>(sFADeleteTAModel);
            psdzSFADeleteTA.EstimatedExecutionTime = sFADeleteTAModel.EstimatedExecutionTime;
            psdzSFADeleteTA.FeatureId = sFADeleteTAModel.FeatureId;
            return psdzSFADeleteTA;
        }

        private static PsdzSFAVerifyTA BuildPsdzSFAVerifyTa(TaModel ta)
        {
            return BuildPsdzTa<PsdzSFAVerifyTA>((SFAVerifyTAModel)ta);
        }

        private static PsdzSFAWriteTA BuildPsdzSFAWriteTa(TaModel ta)
        {
            SFAWriteTAModel sFAWriteTAModel = (SFAWriteTAModel)ta;
            PsdzSFAWriteTA psdzSFAWriteTA = BuildPsdzTa<PsdzSFAWriteTA>(sFAWriteTAModel);
            psdzSFAWriteTA.EstimatedExecutionTime = sFAWriteTAModel.EstimatedExecutionTime;
            psdzSFAWriteTA.FeatureId = sFAWriteTAModel.FeatureId;
            psdzSFAWriteTA.SecureToken = SecureTokenForTalMapper.Map(sFAWriteTAModel.SecureToken);
            return psdzSFAWriteTA;
        }

        private static PsdzBlFlashTa BuildPsdzBlFlashTa(TaModel ta)
        {
            BlFlashTaModel blFlashTaModel = (BlFlashTaModel)ta;
            PsdzBlFlashTa psdzBlFlashTa = BuildPsdzTa<PsdzBlFlashTa>(blFlashTaModel);
            psdzBlFlashTa.ActualProtocol = protocolMapper.GetValue(blFlashTaModel.ActualProtocol);
            psdzBlFlashTa.PreferredProtocol = protocolMapper.GetValue(blFlashTaModel.PreferredProtocol);
            return psdzBlFlashTa;
        }

        private static PsdzSwDeployTa BuildPsdzSwDeployTa(TaModel ta)
        {
            SwDeployTaModel swDeployTaModel = (SwDeployTaModel)ta;
            PsdzSwDeployTa psdzSwDeployTa = BuildPsdzTa<PsdzSwDeployTa>(swDeployTaModel);
            psdzSwDeployTa.ActualProtocol = protocolMapper.GetValue(swDeployTaModel.ActualProtocol);
            psdzSwDeployTa.PreferredProtocol = protocolMapper.GetValue(swDeployTaModel.PreferredProtocol);
            return psdzSwDeployTa;
        }

        private static PsdzSmacSwDeployOnMasterTA BuildSmacMasterTaModel(TaModel ta)
        {
            SmacSwDeployOnMasterTaModel smacSwDeployOnMasterTaModel = (SmacSwDeployOnMasterTaModel)ta;
            PsdzSmacSwDeployOnMasterTA psdzSmacSwDeployOnMasterTA = BuildPsdzTa<PsdzSmacSwDeployOnMasterTA>(smacSwDeployOnMasterTaModel);
            psdzSmacSwDeployOnMasterTA.ActualProtocol = protocolMapper.GetValue(smacSwDeployOnMasterTaModel.ActualProtocol);
            psdzSmacSwDeployOnMasterTA.PreferredProtocol = protocolMapper.GetValue(smacSwDeployOnMasterTaModel.PreferredProtocol);
            psdzSmacSwDeployOnMasterTA.SmacIds = smacSwDeployOnMasterTaModel.SmacIds.ToList();
            return psdzSmacSwDeployOnMasterTA;
        }

        private static PsdzSmacTransferStartTA BuildSmacTransferStartTaModel(TaModel ta)
        {
            SmacTransferStartTaModel smacTransferStartTaModel = (SmacTransferStartTaModel)ta;
            PsdzSmacTransferStartTA psdzSmacTransferStartTA = BuildPsdzTa<PsdzSmacTransferStartTA>(smacTransferStartTaModel);
            psdzSmacTransferStartTA.SmartActuatorData = ((IEnumerable<KeyValuePair<string, ICollection<SgbmIdModel>>>)smacTransferStartTaModel.SmartActuatorData).ToDictionary((Func<KeyValuePair<string, ICollection<SgbmIdModel>>, string>)((KeyValuePair<string, ICollection<SgbmIdModel>> x) => x.Key), (Func<KeyValuePair<string, ICollection<SgbmIdModel>>, IList<IPsdzSgbmId>>)((KeyValuePair<string, ICollection<SgbmIdModel>> y) => y.Value.Select((SgbmIdModel x) => SgbmIdMapper.Map(x)).ToList()));
            return psdzSmacTransferStartTA;
        }

        private static PsdzSmacTransferStatusTA BuildSmacTransferStatusTaModel(TaModel ta)
        {
            SmacTransferStatusTaModel smacTransferStatusTaModel = (SmacTransferStatusTaModel)ta;
            PsdzSmacTransferStatusTA psdzSmacTransferStatusTA = BuildPsdzTa<PsdzSmacTransferStatusTA>(smacTransferStatusTaModel);
            psdzSmacTransferStatusTA.SmartActuatorIDs = smacTransferStatusTaModel.SmartActuatorIDs.ToList();
            return psdzSmacTransferStatusTA;
        }

        private static PsdzEcuMirrorDeployTa BuildPsdzEcuMirrorDeployTa(TaModel ta)
        {
            EcuMirrorDeployTaModel ecuMirrorDeployTaModel = (EcuMirrorDeployTaModel)ta;
            PsdzEcuMirrorDeployTa psdzEcuMirrorDeployTa = BuildPsdzTa<PsdzEcuMirrorDeployTa>(ecuMirrorDeployTaModel);
            psdzEcuMirrorDeployTa.ActualProtocol = protocolMapper.GetValue(ecuMirrorDeployTaModel.ActualProtocol);
            psdzEcuMirrorDeployTa.PreferredProtocol = protocolMapper.GetValue(ecuMirrorDeployTaModel.PreferredProtocol);
            psdzEcuMirrorDeployTa.EstimatedExecutionTime = ecuMirrorDeployTaModel.EstimatedExecutionTime;
            psdzEcuMirrorDeployTa.FlashFileSize = ecuMirrorDeployTaModel.FlashFileSize;
            psdzEcuMirrorDeployTa.ProtocolVersion = MirrorProtocolVersionCtoMapper.map(ecuMirrorDeployTaModel.ProtocolVersion);
            psdzEcuMirrorDeployTa.ProgrammingToken = ecuMirrorDeployTaModel.ProgrammingToken;
            psdzEcuMirrorDeployTa.UseDeltaSwe = ecuMirrorDeployTaModel.UseDeltaSwe;
            psdzEcuMirrorDeployTa.SweFlashFile = ecuMirrorDeployTaModel.SweFlashFile;
            return psdzEcuMirrorDeployTa;
        }

        private static PsdzSmacEcuMirrorDeployOnMasterTA BuildPsdzSmacEcuMirrorDeployOnMasterTa(TaModel ta)
        {
            SmacEcuMirrorDeployOnMasterTaModel smacEcuMirrorDeployOnMasterTaModel = (SmacEcuMirrorDeployOnMasterTaModel)ta;
            PsdzSmacEcuMirrorDeployOnMasterTA psdzSmacEcuMirrorDeployOnMasterTA = BuildPsdzTa<PsdzSmacEcuMirrorDeployOnMasterTA>(smacEcuMirrorDeployOnMasterTaModel);
            psdzSmacEcuMirrorDeployOnMasterTA.ActualProtocol = protocolMapper.GetValue(smacEcuMirrorDeployOnMasterTaModel.ActualProtocol);
            psdzSmacEcuMirrorDeployOnMasterTA.PreferredProtocol = protocolMapper.GetValue(smacEcuMirrorDeployOnMasterTaModel.PreferredProtocol);
            psdzSmacEcuMirrorDeployOnMasterTA.EstimatedExecutionTime = smacEcuMirrorDeployOnMasterTaModel.EstimatedExecutionTime;
            psdzSmacEcuMirrorDeployOnMasterTA.FlashFileSize = smacEcuMirrorDeployOnMasterTaModel.FlashFileSize;
            psdzSmacEcuMirrorDeployOnMasterTA.ProtocolVersion = MirrorProtocolVersionCtoMapper.map(smacEcuMirrorDeployOnMasterTaModel.ProtocolVersion);
            psdzSmacEcuMirrorDeployOnMasterTA.ProgrammingToken = smacEcuMirrorDeployOnMasterTaModel.ProgrammingToken;
            psdzSmacEcuMirrorDeployOnMasterTA.UseDeltaSwe = smacEcuMirrorDeployOnMasterTaModel.UseDeltaSwe;
            psdzSmacEcuMirrorDeployOnMasterTA.SweFlashFile = smacEcuMirrorDeployOnMasterTaModel.SweFlashFile;
            psdzSmacEcuMirrorDeployOnMasterTA.SmacIds = smacEcuMirrorDeployOnMasterTaModel.SmacIds.ToList();
            return psdzSmacEcuMirrorDeployOnMasterTA;
        }

        private static PsdzEcuActivateTa BuildPsdzEcuActivateTa(TaModel ta)
        {
            EcuActivateTaModel ecuActivateTaModel = (EcuActivateTaModel)ta;
            PsdzEcuActivateTa psdzEcuActivateTa = BuildPsdzTa<PsdzEcuActivateTa>(ecuActivateTaModel);
            psdzEcuActivateTa.EstimatedTime = ecuActivateTaModel.EstimatedTime;
            psdzEcuActivateTa.ProtocolVersion = MirrorProtocolVersionCtoMapper.map(ecuActivateTaModel.ProtocolVersion);
            psdzEcuActivateTa.ProgrammingToken = ecuActivateTaModel.ProgrammingToken;
            return psdzEcuActivateTa;
        }

        private static PsdzEcuPollTa BuildPsdzEcuPollTa(TaModel ta)
        {
            EcuPollTaModel ecuPollTaModel = (EcuPollTaModel)ta;
            PsdzEcuPollTa psdzEcuPollTa = BuildPsdzTa<PsdzEcuPollTa>(ecuPollTaModel);
            psdzEcuPollTa.EstimatedExecutionTime = ecuPollTaModel.EstimatedExecutionTime;
            return psdzEcuPollTa;
        }

        private static IbaDeployTaModel BuildIbaDeployTaModel(IPsdzTa ta)
        {
            PsdzIbaDeployTa psdzIbaDeployTa = (PsdzIbaDeployTa)ta;
            IbaDeployTaModel ibaDeployTaModel = BuildTaModel<IbaDeployTaModel>(psdzIbaDeployTa);
            ibaDeployTaModel.ActualProtocol = protocolMapper.GetValue(psdzIbaDeployTa.ActualProtocol);
            ibaDeployTaModel.PreferredProtocol = protocolMapper.GetValue(psdzIbaDeployTa.PreferredProtocol);
            ibaDeployTaModel.SgbmId = SgbmIdMapper.Map(psdzIbaDeployTa.SgbmId);
            return ibaDeployTaModel;
        }

        private static FscDeployTaModel BuildFscDeployTaModel(IPsdzTa ta)
        {
            PsdzFscDeployTa psdzFscDeployTa = (PsdzFscDeployTa)ta;
            FscDeployTaModel fscDeployTaModel = BuildTaModel<FscDeployTaModel>(psdzFscDeployTa);
            fscDeployTaModel.Action = _swtActionTypeMapper.GetValue(psdzFscDeployTa.Action);
            fscDeployTaModel.ApplicationId = SwtApplicationIdMapper.Map(psdzFscDeployTa.ApplicationId);
            fscDeployTaModel.Fsc = psdzFscDeployTa.Fsc;
            fscDeployTaModel.FscCert = psdzFscDeployTa.FscCert;
            return fscDeployTaModel;
        }

        private static IdRestoreTaModel BuildIdRestoreTaModel(IPsdzTa psdzTa)
        {
            PsdzIdRestoreTa psdzIdRestoreTa = (PsdzIdRestoreTa)psdzTa;
            IdRestoreTaModel idRestoreTaModel = BuildTaModel<IdRestoreTaModel>(psdzIdRestoreTa);
            idRestoreTaModel.BackupFile = psdzIdRestoreTa.BackupFile;
            return idRestoreTaModel;
        }

        private static IdLightTaModel BuildIdBasisLightTaModel(IPsdzTa psdzTa)
        {
            if (psdzTa == null)
            {
                return null;
            }
            PsdzIdLightBasisTa psdzIdLightBasisTa = (PsdzIdLightBasisTa)psdzTa;
            IdLightTaModel idLightTaModel = BuildTaModel<IdLightTaModel>(psdzTa);
            if (psdzIdLightBasisTa is PsdzIdBackupLightTa)
            {
                idLightTaModel.IdLightTaType = IdLightTaTypeModel.IdBackup;
            }
            else if (psdzIdLightBasisTa is PsdzIdRestoreLightTa)
            {
                idLightTaModel.IdLightTaType = IdLightTaTypeModel.IdRestore;
            }
            idLightTaModel.Ids = psdzIdLightBasisTa.Ids.ToList();
            return idLightTaModel;
        }

        private static SFADeleteTAModel BuildSFADeleteTaModel(IPsdzTa ta)
        {
            PsdzSFADeleteTA psdzSFADeleteTA = (PsdzSFADeleteTA)ta;
            SFADeleteTAModel sFADeleteTAModel = BuildTaModel<SFADeleteTAModel>(psdzSFADeleteTA);
            sFADeleteTAModel.EstimatedExecutionTime = psdzSFADeleteTA.EstimatedExecutionTime;
            sFADeleteTAModel.FeatureId = psdzSFADeleteTA.FeatureId;
            return sFADeleteTAModel;
        }

        private static SFAVerifyTAModel BuildSFAVerifyTaModel(IPsdzTa ta)
        {
            return BuildTaModel<SFAVerifyTAModel>((PsdzSFAVerifyTA)ta);
        }

        private static SFAWriteTAModel BuildSFAWriteTaModel(IPsdzTa ta)
        {
            PsdzSFAWriteTA psdzSFAWriteTA = (PsdzSFAWriteTA)ta;
            SFAWriteTAModel sFAWriteTAModel = BuildTaModel<SFAWriteTAModel>(psdzSFAWriteTA);
            sFAWriteTAModel.EstimatedExecutionTime = psdzSFAWriteTA.EstimatedExecutionTime;
            sFAWriteTAModel.FeatureId = psdzSFAWriteTA.FeatureId;
            sFAWriteTAModel.SecureToken = SecureTokenForTalMapper.Map(psdzSFAWriteTA.SecureToken);
            return sFAWriteTAModel;
        }

        private static HddUpdateTaModel BuildHddUpdateTaModel(IPsdzTa ta)
        {
            PsdzHddUpdateTA psdzHddUpdateTA = (PsdzHddUpdateTA)ta;
            HddUpdateTaModel hddUpdateTaModel = BuildTaModel<HddUpdateTaModel>(psdzHddUpdateTA);
            hddUpdateTaModel.SecondsToCompletion = psdzHddUpdateTA.SecondsToCompletion;
            return hddUpdateTaModel;
        }

        private static BlFlashTaModel BuildBlFlashTaModel(IPsdzTa ta)
        {
            PsdzBlFlashTa psdzBlFlashTa = (PsdzBlFlashTa)ta;
            BlFlashTaModel blFlashTaModel = BuildTaModel<BlFlashTaModel>(psdzBlFlashTa);
            blFlashTaModel.ActualProtocol = protocolMapper.GetValue(psdzBlFlashTa.ActualProtocol);
            blFlashTaModel.PreferredProtocol = protocolMapper.GetValue(psdzBlFlashTa.PreferredProtocol);
            return blFlashTaModel;
        }

        private static TaModel BuildSwDeployTaModel(IPsdzTa ta)
        {
            PsdzSwDeployTa psdzSwDeployTa = (PsdzSwDeployTa)ta;
            SwDeployTaModel swDeployTaModel = BuildTaModel<SwDeployTaModel>(psdzSwDeployTa);
            swDeployTaModel.ActualProtocol = protocolMapper.GetValue(psdzSwDeployTa.ActualProtocol);
            swDeployTaModel.PreferredProtocol = protocolMapper.GetValue(psdzSwDeployTa.PreferredProtocol);
            return swDeployTaModel;
        }

        private static SmacSwDeployOnMasterTaModel BuildSmacMasterTaModel(IPsdzTa ta)
        {
            PsdzSmacSwDeployOnMasterTA psdzSmacSwDeployOnMasterTA = (PsdzSmacSwDeployOnMasterTA)ta;
            SmacSwDeployOnMasterTaModel smacSwDeployOnMasterTaModel = BuildTaModel<SmacSwDeployOnMasterTaModel>(psdzSmacSwDeployOnMasterTA);
            smacSwDeployOnMasterTaModel.ActualProtocol = protocolMapper.GetValue(psdzSmacSwDeployOnMasterTA.ActualProtocol);
            smacSwDeployOnMasterTaModel.PreferredProtocol = protocolMapper.GetValue(psdzSmacSwDeployOnMasterTA.PreferredProtocol);
            smacSwDeployOnMasterTaModel.SmacIds = psdzSmacSwDeployOnMasterTA.SmacIds;
            return smacSwDeployOnMasterTaModel;
        }

        private static SmacTransferStartTaModel BuildSmacTransferStartTaModel(IPsdzTa ta)
        {
            PsdzSmacTransferStartTA psdzSmacTransferStartTA = (PsdzSmacTransferStartTA)ta;
            SmacTransferStartTaModel smacTransferStartTaModel = BuildTaModel<SmacTransferStartTaModel>(psdzSmacTransferStartTA);
            smacTransferStartTaModel.SmartActuatorData = ((IEnumerable<KeyValuePair<string, IList<IPsdzSgbmId>>>)psdzSmacTransferStartTA.SmartActuatorData).ToDictionary((Func<KeyValuePair<string, IList<IPsdzSgbmId>>, string>)((KeyValuePair<string, IList<IPsdzSgbmId>> x) => x.Key), (Func<KeyValuePair<string, IList<IPsdzSgbmId>>, ICollection<SgbmIdModel>>)((KeyValuePair<string, IList<IPsdzSgbmId>> y) => y.Value.Select((IPsdzSgbmId z) => SgbmIdMapper.Map(z)).ToList()));
            return smacTransferStartTaModel;
        }

        private static SmacTransferStatusTaModel BuildSmacTransferStatusTaModel(IPsdzTa ta)
        {
            PsdzSmacTransferStatusTA psdzSmacTransferStatusTA = (PsdzSmacTransferStatusTA)ta;
            SmacTransferStatusTaModel smacTransferStatusTaModel = BuildTaModel<SmacTransferStatusTaModel>(psdzSmacTransferStatusTA);
            smacTransferStatusTaModel.SmartActuatorIDs = psdzSmacTransferStatusTA.SmartActuatorIDs;
            return smacTransferStatusTaModel;
        }

        private static EcuMirrorDeployTaModel BuildEcuMirrorDeployTaModel(IPsdzTa ta)
        {
            PsdzEcuMirrorDeployTa psdzEcuMirrorDeployTa = (PsdzEcuMirrorDeployTa)ta;
            EcuMirrorDeployTaModel ecuMirrorDeployTaModel = BuildTaModel<EcuMirrorDeployTaModel>(psdzEcuMirrorDeployTa);
            ecuMirrorDeployTaModel.ActualProtocol = protocolMapper.GetValue(psdzEcuMirrorDeployTa.ActualProtocol);
            ecuMirrorDeployTaModel.PreferredProtocol = protocolMapper.GetValue(psdzEcuMirrorDeployTa.PreferredProtocol);
            ecuMirrorDeployTaModel.EstimatedExecutionTime = psdzEcuMirrorDeployTa.EstimatedExecutionTime;
            ecuMirrorDeployTaModel.FlashFileSize = psdzEcuMirrorDeployTa.FlashFileSize;
            ecuMirrorDeployTaModel.ProtocolVersion = MirrorProtocolVersionCtoMapper.map(psdzEcuMirrorDeployTa.ProtocolVersion);
            ecuMirrorDeployTaModel.ProgrammingToken = psdzEcuMirrorDeployTa.ProgrammingToken;
            ecuMirrorDeployTaModel.UseDeltaSwe = psdzEcuMirrorDeployTa.UseDeltaSwe;
            ecuMirrorDeployTaModel.SweFlashFile = psdzEcuMirrorDeployTa.SweFlashFile;
            return ecuMirrorDeployTaModel;
        }

        private static SmacEcuMirrorDeployOnMasterTaModel BuildSmacEcuMirrorDeployTaModel(IPsdzTa ta)
        {
            PsdzSmacEcuMirrorDeployOnMasterTA psdzSmacEcuMirrorDeployOnMasterTA = (PsdzSmacEcuMirrorDeployOnMasterTA)ta;
            SmacEcuMirrorDeployOnMasterTaModel smacEcuMirrorDeployOnMasterTaModel = BuildTaModel<SmacEcuMirrorDeployOnMasterTaModel>(psdzSmacEcuMirrorDeployOnMasterTA);
            smacEcuMirrorDeployOnMasterTaModel.ActualProtocol = protocolMapper.GetValue(psdzSmacEcuMirrorDeployOnMasterTA.ActualProtocol);
            smacEcuMirrorDeployOnMasterTaModel.PreferredProtocol = protocolMapper.GetValue(psdzSmacEcuMirrorDeployOnMasterTA.PreferredProtocol);
            smacEcuMirrorDeployOnMasterTaModel.EstimatedExecutionTime = psdzSmacEcuMirrorDeployOnMasterTA.EstimatedExecutionTime;
            smacEcuMirrorDeployOnMasterTaModel.FlashFileSize = psdzSmacEcuMirrorDeployOnMasterTA.FlashFileSize;
            smacEcuMirrorDeployOnMasterTaModel.ProtocolVersion = MirrorProtocolVersionCtoMapper.map(psdzSmacEcuMirrorDeployOnMasterTA.ProtocolVersion);
            smacEcuMirrorDeployOnMasterTaModel.ProgrammingToken = psdzSmacEcuMirrorDeployOnMasterTA.ProgrammingToken;
            smacEcuMirrorDeployOnMasterTaModel.UseDeltaSwe = psdzSmacEcuMirrorDeployOnMasterTA.UseDeltaSwe;
            smacEcuMirrorDeployOnMasterTaModel.SweFlashFile = psdzSmacEcuMirrorDeployOnMasterTA.SweFlashFile;
            smacEcuMirrorDeployOnMasterTaModel.SmacIds = psdzSmacEcuMirrorDeployOnMasterTA.SmacIds;
            return smacEcuMirrorDeployOnMasterTaModel;
        }

        private static EcuActivateTaModel BuildEcuActivateTaModel(IPsdzTa ta)
        {
            PsdzEcuActivateTa psdzEcuActivateTa = (PsdzEcuActivateTa)ta;
            EcuActivateTaModel ecuActivateTaModel = BuildTaModel<EcuActivateTaModel>(psdzEcuActivateTa);
            ecuActivateTaModel.EstimatedTime = psdzEcuActivateTa.EstimatedTime;
            ecuActivateTaModel.ProtocolVersion = MirrorProtocolVersionCtoMapper.map(psdzEcuActivateTa.ProtocolVersion);
            ecuActivateTaModel.ProgrammingToken = psdzEcuActivateTa.ProgrammingToken;
            return ecuActivateTaModel;
        }

        private static EcuPollTaModel BuildEcuPollTaModel(IPsdzTa ta)
        {
            PsdzEcuPollTa psdzEcuPollTa = (PsdzEcuPollTa)ta;
            EcuPollTaModel ecuPollTaModel = BuildTaModel<EcuPollTaModel>(psdzEcuPollTa);
            ecuPollTaModel.EstimatedExecutionTime = psdzEcuPollTa.EstimatedExecutionTime;
            return ecuPollTaModel;
        }
    }
}