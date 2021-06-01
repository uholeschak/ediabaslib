using System;
using System.Collections.Generic;
using System.IO;
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
            List<EcuFunctionStructs.EcuFixedFuncStruct> fixedFuncStructListIn = GetFixedFuncStructList(ecuVariantIn);
            List<EcuFunctionStructs.EcuFixedFuncStruct> fixedFuncStructListMerge = GetFixedFuncStructList(ecuVariantMerge);

            bool matched = false;
            foreach (EcuFunctionStructs.EcuFixedFuncStruct ecuFixedFuncStruct in fixedFuncStructListMerge)
            {
                if (ecuFixedFuncStruct.EcuJobList != null)
                {
                    foreach (EcuFunctionStructs.EcuJob ecuJob in ecuFixedFuncStruct.EcuJobList)
                    {
                        List<EcuFunctionStructs.EcuJob> jobList = GetMatchingEcuJobs(fixedFuncStructListIn, ecuJob);
                        if (jobList != null)
                        {
                            if (jobList.Count > 0)
                            {
                                matched = true;
                                EcuFunctionStructs.EcuJob ecuJobMatched = null;
                                if (jobList.Count > 1)
                                {
                                    foreach (EcuFunctionStructs.EcuJob ecuJobCheck in jobList)
                                    {
                                        if (MergeEcuJobResult(outTextWriter, fileName, ecuJobCheck, ecuJob, true))
                                        {
                                            ecuJobMatched = ecuJobCheck;
                                            break;
                                        }
                                    }

                                    if (ecuJobMatched == null)
                                    {
                                        outTextWriter?.WriteLine("File='{0}', Job='{1}', Args='{2}': No result match, using first", fileName, ecuJob.Name, JobsArgsToString(ecuJob));
                                        ecuJobMatched = jobList[0];
                                    }
                                }
                                else
                                {
                                    ecuJobMatched = jobList[0];
                                }

                                ecuJobMatched.IgnoreMatch = true;
                                if (string.Compare(ecuJobMatched.Id, ecuJob.Id, StringComparison.OrdinalIgnoreCase) != 0)
                                {
                                    if (ecuJobMatched.CompatIdListList == null)
                                    {
                                        ecuJobMatched.CompatIdListList = new List<string>();
                                    }

                                    if (!ecuJobMatched.CompatIdListList.Contains(ecuJob.Id))
                                    {
                                        ecuJobMatched.CompatIdListList.Add(ecuJob.Id);
                                    }
                                }

                                MergeEcuJobResult(outTextWriter, fileName, ecuJobMatched, ecuJob);
                            }
                            else
                            {
                                outTextWriter?.WriteLine("File='{0}', Job='{1}', Args='{2}: No match", fileName, ecuJob.Name, JobsArgsToString(ecuJob));
                            }
                        }
                    }
                }
            }
            return matched;
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

        static List<EcuFunctionStructs.EcuJob> GetMatchingEcuJobs(List<EcuFunctionStructs.EcuFixedFuncStruct> fixedFuncStructList, EcuFunctionStructs.EcuJob ecuJobComp)
        {
            if (ecuJobComp == null || string.IsNullOrEmpty(ecuJobComp.Name))
            {
                return null;
            }

            List <EcuFunctionStructs.EcuJob> jobList = new List<EcuFunctionStructs.EcuJob>();
            foreach (EcuFunctionStructs.EcuFixedFuncStruct ecuFixedFuncStruct in fixedFuncStructList)
            {
                if (ecuFixedFuncStruct.EcuJobList != null)
                {
                    foreach (EcuFunctionStructs.EcuJob ecuJob in ecuFixedFuncStruct.EcuJobList)
                    {
                        if (!string.IsNullOrEmpty(ecuJob.Name))
                        {
                            if (string.Compare(ecuJob.Name, ecuJobComp.Name, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                if (EcuJobsArgsIdentical(ecuJob, ecuJobComp) && !ecuJob.IgnoreMatch)
                                {
                                    jobList.Add(ecuJob);
                                }
                            }
                        }
                    }
                }
            }

            return jobList;
        }

        static bool EcuJobsArgsIdentical(EcuFunctionStructs.EcuJob ecuJob1, EcuFunctionStructs.EcuJob ecuJob2)
        {
            int count1 = ecuJob1.EcuJobParList?.Count ?? 0;
            int count2 = ecuJob2.EcuJobParList?.Count ?? 0;
            if (count1 != count2)
            {
                return false;
            }

            for (int i = 0; i < count1; i++)
            {
                if (ecuJob1.EcuJobParList != null && ecuJob2.EcuJobParList != null)
                {
                    if (string.Compare(ecuJob1.EcuJobParList[i].Value, ecuJob2.EcuJobParList[i].Value, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        return false;
                    }
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

        static bool MergeEcuJobResult(TextWriter outTextWriter, string fileName, EcuFunctionStructs.EcuJob ecuJobIn, EcuFunctionStructs.EcuJob ecuJobMerge, bool checkOnly = false)
        {
            bool matched = false;
            if (ecuJobMerge.EcuJobResultList != null)
            {
                foreach (EcuFunctionStructs.EcuJobResult ecuJobResult in ecuJobMerge.EcuJobResultList)
                {
                    if (!string.IsNullOrEmpty(ecuJobResult.Name))
                    {
                        List<EcuFunctionStructs.EcuJobResult> jobResultList = GetMatchingEcuJobResults(ecuJobIn, ecuJobResult);
                        if (jobResultList != null)
                        {
                            if (jobResultList.Count > 0)
                            {
                                matched = true;

                                if (!checkOnly)
                                {
                                    if (jobResultList.Count == 1)
                                    {
                                        EcuFunctionStructs.EcuJobResult ecuJobMatch = jobResultList[0];
                                        if (string.Compare(ecuJobMatch.Id, ecuJobResult.Id, StringComparison.OrdinalIgnoreCase) != 0)
                                        {
                                            if (ecuJobMatch.CompatIdListList == null)
                                            {
                                                ecuJobMatch.CompatIdListList = new List<string>();
                                            }

                                            if (!ecuJobMatch.CompatIdListList.Contains(ecuJobResult.Id))
                                            {
                                                ecuJobMatch.CompatIdListList.Add(ecuJobResult.Id);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        outTextWriter?.WriteLine("File='{0}', Job='{1}', Result='{2}': Match count={3}", fileName, ecuJobMerge.Name, ecuJobResult.Name, jobResultList.Count);
                                    }
                                }
                            }
                            else
                            {
                                if (!checkOnly)
                                {
                                    outTextWriter?.WriteLine("File='{0}', Job='{1}', Result='{2}': No Match", fileName, ecuJobMerge.Name, ecuJobResult.Name);
                                }
                            }
                        }
                    }
                }
            }

            return matched;
        }

        static List<EcuFunctionStructs.EcuJobResult> GetMatchingEcuJobResults(EcuFunctionStructs.EcuJob ecuJob, EcuFunctionStructs.EcuJobResult ecuJobResultComp)
        {
            if (ecuJobResultComp == null || string.IsNullOrEmpty(ecuJobResultComp.Name))
            {
                return null;
            }

            List<EcuFunctionStructs.EcuJobResult> jobResultList = new List<EcuFunctionStructs.EcuJobResult>();
            if (ecuJob.EcuJobResultList != null)
            {
                foreach (EcuFunctionStructs.EcuJobResult ecuJobResult in ecuJob.EcuJobResultList)
                {
                    if (!string.IsNullOrEmpty(ecuJobResult.Name))
                    {
                        if (string.Compare(ecuJobResult.Name, ecuJobResultComp.Name, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            jobResultList.Add(ecuJobResult);
                        }
                    }
                }
            }

            return jobResultList;
        }
    }
}
