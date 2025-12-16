using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using PsdzClient.Utility;

namespace PsdzClient.Core.Container
{
    internal class EDIABASAdapterDeviceResult : IDiagnosticDeviceResult
    {
        private IEcuJob job;
        private ParameterContainer inParameters;
        public IEcuJob ECUJob
        {
            get
            {
                if (job != null)
                {
                    return job;
                }

                return new ECUJob();
            }
        }

        public All ResultTree => new All();

        IAdapterError IDiagnosticDeviceResult.Error => new AdapterError(this);

        public EDIABASAdapterDeviceResult()
        {
            job = new ECUJob();
        }

        public EDIABASAdapterDeviceResult(IEcuJob job)
        {
            this.job = job;
        }

        public EDIABASAdapterDeviceResult(IEcuJob job, ParameterContainer inParameters)
        {
            this.job = job;
            this.inParameters = inParameters;
        }

        public object getISTAResult(string resultName)
        {
            return job.getISTAResult(resultName);
        }

        public T getISTAResultAs<T>(string resultName)
        {
            return job.getISTAResultAs<T>(resultName);
        }

        public object getISTAResultAsType(string resultName, Type targetType)
        {
            try
            {
                if (!string.IsNullOrEmpty(resultName) && resultName.EndsWith("_ZUSTAND", StringComparison.Ordinal))
                {
                    Match match = Regex.Match(resultName, "\\[\\d+\\]");
                    if (match.Success)
                    {
                        int num = Convert.ToInt32(match.Value.Replace("[", string.Empty).Replace("]", string.Empty));
                        string text = string.Format(CultureInfo.InvariantCulture, "/WurzelIn/StateLists/Result[{0}]/", num);
                        string resultName2 = inParameters.getParameter(text + "Path") as string;
                        object iSTAResult = job.getISTAResult(resultName2);
                        if (iSTAResult != null && true.Equals(inParameters.getParameter(text + "ReplaceResultWithState")))
                        {
                            foreach (KeyValuePair<string, object> item in inParameters.Parameter)
                            {
                                if (!string.IsNullOrEmpty(item.Key) && item.Value != null && item.Key.StartsWith(text + "States/State[", StringComparison.Ordinal) && item.Key.EndsWith("]/Value", StringComparison.Ordinal))
                                {
                                    string text2 = item.Value.ToString();
                                    string value = ((iSTAResult is char) ? FormatConverter.CompareChar((char)iSTAResult, text2) : ((!(iSTAResult is double)) ? iSTAResult.ToString() : FormatConverter.CompareDouble((double)iSTAResult, text2)));
                                    if (text2.Equals(value) && inParameters.getParameter(item.Key.Replace("/Value", "/Text"))is ITextLocator textLocator)
                                    {
                                        return textLocator.Text;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("EDIABASAdapterDeviceResult.getISTAResultAsType()", exception);
            }

            return job.getISTAResultAsType(resultName, targetType);
        }

        public T getResultAs<T>(string resultName)
        {
            return job.getResultAs<T>(resultName);
        }

        public T getResultAs<T>(ushort set, string resultName)
        {
            return job.getResultAs<T>(set, resultName);
        }
    }
}