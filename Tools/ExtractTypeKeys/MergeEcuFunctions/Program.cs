using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
            TextWriter logTextWriter = args.Length >= 3 ? Console.Out : null;

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
            if (ecuVariantMerge.RefEcuVariantList != null)
            {
                foreach (EcuFunctionStructs.RefEcuVariant refEcuVariant in ecuVariantMerge.RefEcuVariantList)
                {
                    if (refEcuVariant.FixedFuncStructList != null)
                    {
                        foreach (EcuFunctionStructs.EcuFixedFuncStruct ecuFixedFuncStruct in refEcuVariant.FixedFuncStructList)
                        {
                            if (ecuFixedFuncStruct.EcuJobList != null)
                            {
                                foreach (EcuFunctionStructs.EcuJob ecuJob in ecuFixedFuncStruct.EcuJobList)
                                {
                                    List<EcuFunctionStructs.EcuJob> jobList = GetMatchingEcuJobs(ecuVariantIn, ecuJob);
                                    if (jobList != null)
                                    {
                                        if (jobList.Count > 0)
                                        {
                                            jobList[0].IgnoreMatch = true;
                                        }
                                        else
                                        {
                                            outTextWriter?.WriteLine("File='{0}', Job='{1}': No Match", fileName, ecuJob.Name);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }

        static List<EcuFunctionStructs.EcuJob> GetMatchingEcuJobs(EcuFunctionStructs.EcuVariant ecuVariant, EcuFunctionStructs.EcuJob ecuJobComp)
        {
            if (ecuJobComp == null || string.IsNullOrEmpty(ecuJobComp.Name))
            {
                return null;
            }

            List <EcuFunctionStructs.EcuJob> jobList = new List<EcuFunctionStructs.EcuJob>();
            if (ecuVariant.RefEcuVariantList != null)
            {
                foreach (EcuFunctionStructs.RefEcuVariant refEcuVariant in ecuVariant.RefEcuVariantList)
                {
                    if (refEcuVariant.FixedFuncStructList != null)
                    {
                        foreach (EcuFunctionStructs.EcuFixedFuncStruct ecuFixedFuncStruct in refEcuVariant.FixedFuncStructList)
                        {
                            if (ecuFixedFuncStruct.EcuJobList != null)
                            {
                                foreach (EcuFunctionStructs.EcuJob ecuJob in ecuFixedFuncStruct.EcuJobList)
                                {
                                    if (!string.IsNullOrEmpty(ecuJob.Name))
                                    {
                                        if (string.Compare(ecuJob.Name, ecuJobComp.Name, StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            if (!ecuJob.IgnoreMatch)
                                            {
                                                jobList.Add(ecuJob);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return jobList;
        }
    }
}
