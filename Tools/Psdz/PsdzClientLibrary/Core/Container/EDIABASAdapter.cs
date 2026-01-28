using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PsdzClient.Utility;
using PsdzClient;

namespace PsdzClient.Core.Container
{
    internal class EDIABASAdapter : BaseAdapter
    {
        private bool binModeReq;
        private byte[] ecuData;
        private string ecuGroup = string.Empty;
        private string ecuJob = string.Empty;
        private IEcuKom ecuKom;
        private string ecuParam = string.Empty;
        private string ecuResultFilter = string.Empty;
        private bool parameterizationDone;
        public EDIABASAdapter(bool StandardErrorHandling, IEcuKom ecuKom, ConfigurationContainer configContainer) : base(StandardErrorHandling, configContainer)
        {
            this.ecuKom = ecuKom;
        }

        public void DoParameterization()
        {
            long num = 0L;
            long num2 = 0L;
            try
            {
                ParameterContainer parameterContainer = new ParameterContainer();
                ABranch run = configContainer.Body.Configuration.Run;
                if (configContainer.Header != null && configContainer.Header.Version != null)
                {
                    num = configContainer.Header.Version.Major;
                    num2 = configContainer.Header.Version.Minor;
                }

                if (run.Children != null)
                {
                    foreach (ANode child in run.Children)
                    {
                        diveConfigNodes(parameterContainer, child, "/Run", binModeReq);
                    }
                }

                if (ISTACoreFramwork.DebugLevel > 0 && configContainer.ParametrizationOverrides != null && configContainer.ParametrizationOverrides.Parameter != null)
                {
                    foreach (KeyValuePair<string, object> item in configContainer.ParametrizationOverrides.Parameter)
                    {
                        Log.Info("EDIABASAdapter.Execute()", "found parameterization key: {0}", item.Key);
                    }
                }

                if (ISTACoreFramwork.DebugLevel > 0 && configContainer.RunOverrides != null && configContainer.RunOverrides.Parameter != null)
                {
                    foreach (KeyValuePair<string, object> item2 in configContainer.RunOverrides.Parameter)
                    {
                        Log.Debug(ISTACoreFramwork.DebugLevel, "EDIABASAdapter.Execute()", "found run override key: {0}", item2.Key);
                    }
                }

                if (ISTACoreFramwork.DebugLevel > 0)
                {
                    foreach (KeyValuePair<string, object> item3 in parameterContainer.Parameter)
                    {
                        Log.Debug(ISTACoreFramwork.DebugLevel, "EDIABASAdapter.Execute()", "found var Path entry key: {0}", item3.Key);
                    }
                }

                if (parameterContainer.Count <= 0)
                {
                    return;
                }

                string text = null;
                KeyValuePair<string, object>? keyValuePairEndsWith = parameterContainer.getKeyValuePairEndsWith("ECUGroupOrVariant");
                if (keyValuePairEndsWith.HasValue && keyValuePairEndsWith.HasValue && !string.IsNullOrEmpty(keyValuePairEndsWith.Value.Key))
                {
                    text = keyValuePairEndsWith.Value.Key;
                    if (keyValuePairEndsWith.Value.Value != null)
                    {
                        ecuGroup = keyValuePairEndsWith.Value.Value.ToString();
                    }
                }

                keyValuePairEndsWith = configContainer.ParametrizationOverrides.getKeyValuePairEndsWith("ECUGroupOrVariant");
                if (keyValuePairEndsWith.HasValue && keyValuePairEndsWith.HasValue && !string.IsNullOrEmpty(keyValuePairEndsWith.Value.Key) && keyValuePairEndsWith.Value.Value != null)
                {
                    ecuGroup = keyValuePairEndsWith.Value.Value.ToString();
                }

                keyValuePairEndsWith = configContainer.RunOverrides.getKeyValuePairEndsWith("ECUGroupOrVariant");
                if (keyValuePairEndsWith.HasValue && keyValuePairEndsWith.HasValue && !string.IsNullOrEmpty(keyValuePairEndsWith.Value.Key) && keyValuePairEndsWith.Value.Value != null)
                {
                    ecuGroup = keyValuePairEndsWith.Value.Value.ToString();
                }

                if (string.IsNullOrEmpty(text))
                {
                    keyValuePairEndsWith = parameterContainer.getKeyValuePairContains("UnknownGroup");
                    if (keyValuePairEndsWith.HasValue && keyValuePairEndsWith.HasValue && !string.IsNullOrEmpty(keyValuePairEndsWith.Value.Key) && keyValuePairEndsWith.Value.Value != null)
                    {
                        text = keyValuePairEndsWith.Value.Key;
                        string[] array = text.Split('/');
                        ecuGroup = array[5];
                        ecuJob = array[7];
                        if (text.EndsWith("ARG1", StringComparison.Ordinal) && (num != 2 || num2 != 0L))
                        {
                            ecuParam = keyValuePairEndsWith.Value.Value?.ToString() + ";";
                        }
                    }

                    if (string.IsNullOrEmpty(text))
                    {
                        if (configContainer.Header == null || configContainer.Header.Version == null || configContainer.Header.Version.Major != 2 || configContainer.Header.Version.Minor != 0L)
                        {
                            Log.Warning("EDIABASAdapter.DoParamterization()", "unable to identify sgbd from default values od config overrides");
                            return;
                        }

                        keyValuePairEndsWith = parameterContainer.getKeyValuePairContains("/Variant/");
                        if (keyValuePairEndsWith.HasValue && keyValuePairEndsWith.HasValue && !string.IsNullOrEmpty(keyValuePairEndsWith.Value.Key) && keyValuePairEndsWith.Value.Value != null)
                        {
                            text = keyValuePairEndsWith.Value.Key;
                            string[] array2 = text.Split('/');
                            ecuGroup = array2[5];
                            ecuJob = array2[7];
                        }
                        else
                        {
                            keyValuePairEndsWith = parameterContainer.getKeyValuePairContains("/Group/");
                            if (!keyValuePairEndsWith.HasValue || !keyValuePairEndsWith.HasValue || string.IsNullOrEmpty(keyValuePairEndsWith.Value.Key) || keyValuePairEndsWith.Value.Value == null)
                            {
                                Log.Warning("EDIABASAdapter.DoParamterization()", "unable to identify sgbd from default values od config overrides");
                                return;
                            }

                            text = keyValuePairEndsWith.Value.Key;
                            string[] array3 = text.Split('/');
                            ecuGroup = array3[3];
                            ecuJob = array3[7];
                        }
                    }
                }
                else
                {
                    string[] array4 = text.Split('/');
                    ecuJob = array4[5];
                    if (string.IsNullOrEmpty(ecuGroup))
                    {
                        ecuGroup = array4[3];
                    }
                }

                foreach (KeyValuePair<string, object> item4 in parameterContainer.Parameter)
                {
                    if (!(item4.Key != text) && (num != 2 || num2 != 0L))
                    {
                        continue;
                    }

                    keyValuePairEndsWith = configContainer.RunOverrides.getKeyValuePair(item4.Key);
                    if (keyValuePairEndsWith.HasValue && keyValuePairEndsWith.HasValue && !string.IsNullOrEmpty(keyValuePairEndsWith.Value.Key) && keyValuePairEndsWith.Value.Value != null)
                    {
                        if (keyValuePairEndsWith.Value.Value is byte[])
                        {
                            ecuData = (byte[])keyValuePairEndsWith.Value.Value;
                            ecuParam = FormatConverter.ConvertECUParamToString(keyValuePairEndsWith.Value.Value);
                            binModeReq = true;
                        }
                        else
                        {
                            ecuParam = ecuParam + FormatConverter.ConvertECUParamToString(keyValuePairEndsWith.Value.Value) + ";";
                        }

                        continue;
                    }

                    keyValuePairEndsWith = configContainer.ParametrizationOverrides.getKeyValuePair(item4.Key);
                    if (keyValuePairEndsWith.HasValue && keyValuePairEndsWith.HasValue && !string.IsNullOrEmpty(keyValuePairEndsWith.Value.Key) && keyValuePairEndsWith.Value.Value != null)
                    {
                        if (keyValuePairEndsWith.Value.Value is byte[])
                        {
                            ecuData = (byte[])keyValuePairEndsWith.Value.Value;
                            ecuParam = FormatConverter.ConvertECUParamToString(keyValuePairEndsWith.Value.Value);
                            binModeReq = true;
                        }
                        else
                        {
                            ecuParam = ecuParam + FormatConverter.ConvertECUParamToString(keyValuePairEndsWith.Value.Value) + ";";
                        }
                    }
                    else if (item4.Value != null)
                    {
                        if (item4.Value is byte[])
                        {
                            ecuData = (byte[])item4.Value;
                            ecuParam = FormatConverter.ConvertECUParamToString(item4.Value);
                            binModeReq = true;
                        }
                        else
                        {
                            ecuParam = ecuParam + FormatConverter.ConvertECUParamToString(item4.Value) + ";";
                        }
                    }
                    else
                    {
                        Log.Warning("EDIABASAdapter.DoParameterization()", "got empty parameter value from path: {0}", item4.Key);
                        ecuParam += ";";
                    }
                }

                if (!string.IsNullOrEmpty(ecuParam))
                {
                    ecuParam = ecuParam.TrimEnd(';');
                }

                parameterizationDone = true;
            }
            catch (Exception exception)
            {
                Log.WarningException("EDIABASAdapter.DoParameterization()", exception);
            }
        }

        private bool CheckForBinModeRequired()
        {
            if (binModeReq && ("FA".Equals(ecuGroup, StringComparison.OrdinalIgnoreCase) || "00SWTKWP".Equals(ecuGroup, StringComparison.OrdinalIgnoreCase) || "G_ZGW".Equals(ecuGroup, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            return false;
        }

        public IDiagnosticDeviceResult Execute()
        {
            return Execute(null);
        }

        private void MarkAsFastaRelevant(IEcuJob job, IEnumerable<string> jobNames)
        {
            jobNames.ForEach(delegate (string fastaRelevantJobName)
            {
                job.maskResultFASTARelevant(0, -1, fastaRelevantJobName);
            });
        }

        private IEnumerable<string> RetrieveFastaRelevantJobNames(ParameterContainer inParam)
        {
            List<string> list = new List<string>();
            int num = 0;
            string text;
            do
            {
                string name = $"/WurzelIn/StateLists/Result[{num}]/Path";
                text = inParam.getParameter(name, null) as string;
                if (text != null)
                {
                    list.Add(text.Split('/').Last());
                    num++;
                }
            }
            while (!string.IsNullOrEmpty(text));
            return list;
        }

        public IDiagnosticDeviceResult Execute(ParameterContainer inParameters)
        {
            try
            {
                if (ecuKom == null)
                {
                    Log.Warning("EDIABASAdapter.Execute()", "ecuKom was null; no communication possible");
                    return new EDIABASAdapterDeviceResult(new ECUJob(), inParameters);
                }

                if (parameterizationDone && !string.IsNullOrEmpty(ecuGroup) && !string.IsNullOrEmpty(this.ecuJob))
                {
                    if (!binModeReq || !CheckForBinModeRequired())
                    {
                        IEcuJob ecuJob = ecuKom.ApiJob(ecuGroup, this.ecuJob, ecuParam, ecuResultFilter);
                        ecuJob.maskResultFASTARelevant(1, -1, "JOB_STATUS");
                        MarkAsFastaRelevant(ecuJob, RetrieveFastaRelevantJobNames(inParameters));
                        Log.Error("EidiabusAdapter", string.Join("\n", ecuJob.JobResult.Select((IEcuResult x) => $"name: {x.Name}, value: {x.Value}, relevant: {x.FASTARelevant}, set: {x.Set}")));
                        Log.Info("EDIABASAdapter.Execute()", "apiJob('{0}', '{1}', '{2}') - JobStatus: {3}:{4}", ecuGroup, this.ecuJob, ecuParam, ecuJob?.JobErrorCode ?? (-1), (ecuJob != null) ? ecuJob.JobErrorText : "null");
                        return new EDIABASAdapterDeviceResult(ecuJob, inParameters);
                    }

                    if (ecuData != null)
                    {
                        IEcuJob ecuJob2 = ecuKom.apiJobData(ecuGroup, this.ecuJob, ecuData, ecuData.Length, ecuResultFilter, string.Empty);
                        ecuJob2.maskResultFASTARelevant(1, -1, "JOB_STATUS");
                        MarkAsFastaRelevant(ecuJob2, RetrieveFastaRelevantJobNames(inParameters));
                        Log.Info("EDIABASAdapter.Execute()", "apiJobData('{0}', '{1}', Data Len: {2}) - JobStatus: {3}:{4}", ecuGroup, this.ecuJob, (ecuData != null) ? ecuData.Length.ToString(CultureInfo.InvariantCulture) : "null", ecuJob2?.JobErrorCode ?? (-1), (ecuJob2 != null) ? ecuJob2.JobErrorText : "null");
                        return new EDIABASAdapterDeviceResult(ecuJob2, inParameters);
                    }

                    Log.Warning("EDIABASAdapter.Execute()", "binModeReq was true but no valid ecuData");
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("EDIABASAdapter.Execute()", exception);
            }

            return new EDIABASAdapterDeviceResult(new ECUJob(), inParameters);
        }

        [PreserveSource(Added = true)]
        public bool BinModeReq => binModeReq;

        [PreserveSource(Added = true)]
        public byte[] EcuData => ecuData;

        [PreserveSource(Added = true)]
        public string EcuGroup => ecuGroup;

        [PreserveSource(Added = true)]
        public string EcuJob => ecuJob;

        [PreserveSource(Added = true)]
        public string EcuParam => ecuParam;

        [PreserveSource(Added = true)]
        public string EcuResultFilter => ecuResultFilter;

        [PreserveSource(Added = true)]
        public bool ParameterizationDone => parameterizationDone;

        [PreserveSource(Added = true)]
        public bool IsBinModeRequired => CheckForBinModeRequired();
    }
}