using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using BmwFileReader;

namespace MergeEcuFunctions
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Unicode;
            TextWriter outTextWriter = args.Length >= 0 ? Console.Out : null;

            if (args.Length < 1)
            {
                outTextWriter?.WriteLine("No input directory specified");
                return 1;
            }
            if (args.Length < 2)
            {
                outTextWriter?.WriteLine("No merge directory specified");
                return 1;
            }
            if (args.Length < 3)
            {
                outTextWriter?.WriteLine("No output directory specified");
                return 1;
            }

            string inDir = args[0];
            if (string.IsNullOrEmpty(inDir))
            {
                outTextWriter?.WriteLine("Input directory empty");
                return 1;
            }

            string mergeDir = args[1];
            if (string.IsNullOrEmpty(mergeDir))
            {
                outTextWriter?.WriteLine("Merge directory empty");
                return 1;
            }

            string outDir = args[2];
            if (string.IsNullOrEmpty(outDir))
            {
                outTextWriter?.WriteLine("Output directory empty");
                return 1;
            }

            try
            {
                if (!Directory.Exists(inDir))
                {
                    outTextWriter?.WriteLine("Input directory not existing");
                    return 1;
                }

                if (!Directory.Exists(mergeDir))
                {
                    outTextWriter?.WriteLine("Output directory not existing");
                    return 1;
                }

                string outDirSub = Path.Combine(outDir, "EcuFunctionsMerged");
                try
                {
                    if (Directory.Exists(outDirSub))
                    {
                        Directory.Delete(outDirSub, true);
                        Thread.Sleep(1000);
                    }
                    Directory.CreateDirectory(outDirSub);
                }
                catch (Exception)
                {
                    // ignored
                }


                string[] files = Directory.GetFiles(inDir, "*.xml");
                foreach (string inFile in files)
                {
                    try
                    {
                        string fileName = Path.GetFileName(inFile);
                        string mergeFile = Path.Combine(mergeDir, fileName);
                        if (!fileName.StartsWith("faultdata_", StringComparison.OrdinalIgnoreCase) && File.Exists(mergeFile))
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(EcuFunctionStructs.EcuVariant));
                            EcuFunctionStructs.EcuVariant ecuVariantIn = null;
                            EcuFunctionStructs.EcuVariant ecuVariantMerge = null;
                            using (FileStream fs = new FileStream(inFile, FileMode.Open))
                            {
                                if (serializer.Deserialize(fs) is EcuFunctionStructs.EcuVariant ecuVariant)
                                {
                                    ecuVariantIn = ecuVariant;
                                }
                            }

                            using (FileStream fs = new FileStream(mergeFile, FileMode.Open))
                            {
                                if (serializer.Deserialize(fs) is EcuFunctionStructs.EcuVariant ecuVariant)
                                {
                                    ecuVariantMerge = ecuVariant;
                                }
                            }

                            if (ecuVariantIn != null && ecuVariantMerge != null)
                            {
                                MergeEcuVariant(outTextWriter, fileName, ecuVariantIn, ecuVariantMerge);

                                string xmlFile = Path.Combine(outDirSub, fileName);
                                XmlWriterSettings settings = new XmlWriterSettings
                                {
                                    Indent = true,
                                    IndentChars = "\t"
                                };
                                using (XmlWriter writer = XmlWriter.Create(xmlFile, settings))
                                {
                                    serializer.Serialize(writer, ecuVariantIn);
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
            catch (Exception e)
            {
                outTextWriter?.WriteLine(e);
            }
            return 0;
        }

        static bool MergeEcuVariant(TextWriter outTextWriter, string fileName, EcuFunctionStructs.EcuVariant ecuVariantIn, EcuFunctionStructs.EcuVariant ecuVariantMerge)
        {
            List<EcuFunctionStructs.EcuFixedFuncStruct> fixedFuncStructListIn = GetFixedFuncStructListRead(ecuVariantIn);
            List<EcuFunctionStructs.EcuFixedFuncStruct> fixedFuncStructListMerge = GetFixedFuncStructListRead(ecuVariantMerge);

            bool matched = false;
            foreach (EcuFunctionStructs.EcuFixedFuncStruct ecuFixedFuncStruct in fixedFuncStructListMerge)
            {
                if (ecuFixedFuncStruct.EcuJobList != null)
                {
                    foreach (EcuFunctionStructs.EcuJob ecuJob in ecuFixedFuncStruct.EcuJobList)
                    {
                        if (MergeEcuJob(outTextWriter, fileName, fixedFuncStructListIn, ecuJob) > 0)
                        {
                            matched = true;
                        }
                    }
                }
            }
            return matched;
        }

        static List<EcuFunctionStructs.EcuFixedFuncStruct> GetFixedFuncStructListRead(EcuFunctionStructs.EcuVariant ecuVariant)
        {
            List<EcuFunctionStructs.EcuFixedFuncStruct> fixedFuncStructList = GetFixedFuncStructList(ecuVariant);
            List<EcuFunctionStructs.EcuFixedFuncStruct> fixedFuncStructListRead = new List<EcuFunctionStructs.EcuFixedFuncStruct>();
            foreach (EcuFunctionStructs.EcuFixedFuncStruct ecuFixedFunc in fixedFuncStructList)
            {
                if (ecuFixedFunc.GetNodeClassType() != EcuFunctionStructs.EcuFixedFuncStruct.NodeClassType.ControlActuator)
                {
                    fixedFuncStructListRead.Add(ecuFixedFunc);
                }
            }

            return fixedFuncStructListRead;
        }

        static List<EcuFunctionStructs.EcuFixedFuncStruct> GetFixedFuncStructList(EcuFunctionStructs.EcuVariant ecuVariant)
        {
            List<EcuFunctionStructs.EcuFixedFuncStruct> fixedFuncStructList = new List<EcuFunctionStructs.EcuFixedFuncStruct>();

            if (ecuVariant.RefEcuVariantList != null)
            {
                foreach (EcuFunctionStructs.RefEcuVariant refEcuVariant in ecuVariant.RefEcuVariantList)
                {
                    if (refEcuVariant.FixedFuncStructList != null)
                    {
                        fixedFuncStructList.AddRange(refEcuVariant.FixedFuncStructList);
                    }
                }
            }

            if (ecuVariant.EcuFuncStructList != null)
            {
                foreach (EcuFunctionStructs.EcuFuncStruct ecuFuncStruct in ecuVariant.EcuFuncStructList)
                {
                    if (ecuFuncStruct.FixedFuncStructList != null)
                    {
                        fixedFuncStructList.AddRange(ecuFuncStruct.FixedFuncStructList);
                    }
                }
            }

            return fixedFuncStructList;
        }

        static int MergeEcuJob(TextWriter outTextWriter, string fileName, List<EcuFunctionStructs.EcuFixedFuncStruct> fixedFuncStructList, EcuFunctionStructs.EcuJob ecuJobMerge)
        {
            if (ecuJobMerge == null || string.IsNullOrEmpty(ecuJobMerge.Name))
            {
                return 0;
            }

            int matches = 0;
            foreach (EcuFunctionStructs.EcuFixedFuncStruct ecuFixedFuncStruct in fixedFuncStructList)
            {
                if (ecuFixedFuncStruct.EcuJobList != null)
                {
                    foreach (EcuFunctionStructs.EcuJob ecuJob in ecuFixedFuncStruct.EcuJobList)
                    {
                        if (!string.IsNullOrEmpty(ecuJob.Name))
                        {
                            if ((string.Compare(ecuJob.Name, ecuJobMerge.Name, StringComparison.OrdinalIgnoreCase) == 0) &&
                                string.Compare(ecuJob.FuncNameJob, ecuJobMerge.FuncNameJob, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                if (EcuJobsArgsIdentical(ecuJob, ecuJobMerge))
                                {
                                    int results = MergeEcuJobResults(outTextWriter, fileName, ecuJob, ecuJobMerge);
                                    if (results > 0)
                                    {
                                        if (string.Compare(ecuJob.Id, ecuJobMerge.Id, StringComparison.OrdinalIgnoreCase) != 0)
                                        {
                                            List<string> compatIdListList = ecuJob.CompatIdListList ?? new List<string>();
                                            if (!compatIdListList.Contains(ecuJob.Id))
                                            {
                                                compatIdListList.Add(ecuJob.Id);
                                            }

                                            ecuJob.CompatIdListList = compatIdListList;
                                            if (compatIdListList.Count > 1)
                                            {
                                                outTextWriter?.WriteLine("Merge Jobs multi IDs: File='{0}', Job='{1}({2})', Args='{3}', Res='{4}', Count={5}",
                                                    fileName, ecuJob.Name, ecuJob.FuncNameJob, JobsArgsToString(ecuJob), JobsResultsToString(ecuJob), compatIdListList.Count);
                                            }
                                        }
                                        matches += results;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return matches;
        }

        static int MergeEcuJobResults(TextWriter outTextWriter, string fileName, EcuFunctionStructs.EcuJob ecuJobIn, EcuFunctionStructs.EcuJob ecuJobMerge)
        {
            int resultCount = 0;
            if (ecuJobMerge.EcuJobResultList != null)
            {
                foreach (EcuFunctionStructs.EcuJobResult ecuJobResult in ecuJobMerge.EcuJobResultList)
                {
                    if (!string.IsNullOrEmpty(ecuJobResult.Name))
                    {
                        List<EcuFunctionStructs.EcuJobResult> jobResultList = GetMatchingEcuJobResults(ecuJobIn, ecuJobResult);
                        if (jobResultList != null)
                        {
                            foreach (EcuFunctionStructs.EcuJobResult ecuJobResultMatch in jobResultList)
                            {
                                resultCount++;
                                if (string.Compare(ecuJobResultMatch.Id, ecuJobResult.Id, StringComparison.OrdinalIgnoreCase) != 0)
                                {
                                    List<string> compatIdListList = ecuJobResultMatch.CompatIdListList ?? new List<string>();
                                    if (!compatIdListList.Contains(ecuJobResult.Id))
                                    {
                                        compatIdListList.Add(ecuJobResult.Id);
                                    }

                                    ecuJobResultMatch.CompatIdListList = compatIdListList;
                                    if (compatIdListList.Count > 1)
                                    {
                                        outTextWriter?.WriteLine("Merge Results multi IDs: File='{0}', Job='{1}', Args='{2}', Res='{3}', Count={4}",
                                            fileName, ecuJobIn.Name, JobsArgsToString(ecuJobIn), JobsResultsToString(ecuJobIn), compatIdListList.Count);
                                    }
                                }
                            }
                        }
                        else
                        {
                            outTextWriter?.WriteLine("Merge Results no match: File='{0}', Job='{1}', Args='{2}', Res='{3}', JobM='{4}', ArgsM='{5}', ResM='{6}'",
                                fileName, ecuJobIn.Name, JobsArgsToString(ecuJobIn), JobsResultsToString(ecuJobIn),
                                ecuJobMerge.Name, JobsArgsToString(ecuJobMerge), JobsResultsToString(ecuJobMerge));
                        }
                    }
                }
            }

            return resultCount;
        }

        static bool EcuJobsArgsIdentical(EcuFunctionStructs.EcuJob ecuJob1, EcuFunctionStructs.EcuJob ecuJob2)
        {
            List<EcuFunctionStructs.EcuJobParameter> parList1 = new List<EcuFunctionStructs.EcuJobParameter>();
            if (ecuJob1.EcuJobParList != null)
            {
                parList1 = ecuJob1.EcuJobParList.OrderBy(x => x.Name).ToList();
            }

            List<EcuFunctionStructs.EcuJobParameter> parList2 = new List<EcuFunctionStructs.EcuJobParameter>();
            if (ecuJob2.EcuJobParList != null)
            {
                parList2 = ecuJob2.EcuJobParList.OrderBy(x => x.Name).ToList();
            }

            if (parList1.Count != parList2.Count)
            {
                return false;
            }

            for (int i = 0; i < parList1.Count; i++)
            {
                if (string.Compare(parList1[i].Value, parList2[i].Value, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    return false;
                }
            }

            return true;
        }

        static string JobsArgsToString(EcuFunctionStructs.EcuJob ecuJob)
        {
            if (ecuJob.EcuJobParList == null)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            foreach (EcuFunctionStructs.EcuJobParameter ecuJobPar in ecuJob.EcuJobParList)
            {
                if (sb.Length > 0)
                {
                    sb.Append(", ");
                }
                sb.Append(ecuJobPar.Value);
            }

            return sb.ToString();
        }

        static string JobsResultsToString(EcuFunctionStructs.EcuJob ecuJob)
        {
            if (ecuJob.EcuJobResultList == null)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            foreach (EcuFunctionStructs.EcuJobResult ecuJobResult in ecuJob.EcuJobResultList)
            {
                if (sb.Length > 0)
                {
                    sb.Append(", ");
                }
                sb.Append(ecuJobResult.Name);
            }

            return sb.ToString();
        }

        static List<EcuFunctionStructs.EcuJobResult> GetMatchingEcuJobResults(EcuFunctionStructs.EcuJob ecuJob, EcuFunctionStructs.EcuJobResult ecuJobResultComp)
        {
            if (ecuJobResultComp == null || string.IsNullOrEmpty(ecuJobResultComp.Name))
            {
                return null;
            }

            int resultCount = 0;
            List<EcuFunctionStructs.EcuJobResult> jobResultList = new List<EcuFunctionStructs.EcuJobResult>();
            if (ecuJob.EcuJobResultList != null)
            {
                foreach (EcuFunctionStructs.EcuJobResult ecuJobResult in ecuJob.EcuJobResultList)
                {
                    if (!string.IsNullOrEmpty(ecuJobResult.Name))
                    {
                        resultCount++;
                        if ((string.Compare(ecuJobResult.Name, ecuJobResultComp.Name, StringComparison.OrdinalIgnoreCase) == 0) &&
                            (string.Compare(ecuJobResult.FuncNameResult, ecuJobResultComp.FuncNameResult, StringComparison.OrdinalIgnoreCase) == 0))
                        {
                            jobResultList.Add(ecuJobResult);
                        }
                    }
                }
            }

            if (resultCount == 0)
            {
                return null;
            }

            return jobResultList;
        }
    }
}
