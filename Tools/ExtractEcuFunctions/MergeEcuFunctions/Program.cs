using BmwFileReader;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

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

            string inFileName = args[0];
            if (string.IsNullOrEmpty(inFileName))
            {
                outTextWriter?.WriteLine("Input file empty");
                return 1;
            }

            string mergeFileName = args[1];
            if (string.IsNullOrEmpty(mergeFileName))
            {
                outTextWriter?.WriteLine("Merge file empty");
                return 1;
            }

            string outFile = args[2];
            if (string.IsNullOrEmpty(outFile))
            {
                outTextWriter?.WriteLine("Output file empty");
                return 1;
            }

            try
            {
                if (!File.Exists(inFileName))
                {
                    outTextWriter?.WriteLine("Input file not existing");
                    return 1;
                }

                if (!File.Exists(mergeFileName))
                {
                    outTextWriter?.WriteLine("Merge file not existing");
                    return 1;
                }

                List<string> entryList = GetZipEntryList(inFileName);
                if (entryList == null || entryList.Count == 0)
                {
                    outTextWriter?.WriteLine("No entries in zip found");
                    return 1;
                }

                FileStream fsOut = null;
                ZipOutputStream zipStream = null;
                try
                {
                    fsOut = File.Create(outFile);
                    zipStream = new ZipOutputStream(fsOut);

                    zipStream.SetLevel(9); //0-9, 9 being the highest level of compression

                    foreach (string entryName in entryList)
                    {
                        try
                        {
                            bool stored = false;
                            bool entryValid = true;

                            string entryExtension = Path.GetExtension(entryName);
                            if (string.Compare(entryExtension, ".xml", StringComparison.OrdinalIgnoreCase) != 0)
                            {
                                entryValid = false;
                            }

                            if (entryName.StartsWith("faultdata_", StringComparison.OrdinalIgnoreCase))
                            {
                                entryValid = false;
                            }

                            if (entryValid)
                            {
                                if (GetEcuDataObject(inFileName, entryName, typeof(EcuFunctionStructs.EcuVariant)) is EcuFunctionStructs.EcuVariant ecuVariantIn)
                                {
                                    if (GetEcuDataObject(mergeFileName, entryName, typeof(EcuFunctionStructs.EcuVariant)) is EcuFunctionStructs.EcuVariant ecuVariantMerge)
                                    {
                                        MergeEcuVariant(outTextWriter, entryName, ecuVariantIn, ecuVariantMerge);

                                        XmlWriterSettings settings = new XmlWriterSettings
                                        {
                                            Indent = true,
                                            IndentChars = "\t"
                                        };
                                        XmlSerializer serializer = new XmlSerializer(typeof(EcuFunctionStructs.EcuVariant));
                                        using (MemoryStream memStream = new MemoryStream())
                                        {
                                            using (XmlWriter writer = XmlWriter.Create(memStream, settings))
                                            {
                                                serializer.Serialize(writer, ecuVariantIn);
                                            }

                                            AddZipEntry(zipStream, entryName, memStream);
                                        }

                                        stored = true;
                                    }
                                }
                            }

                            if (!stored)
                            {
                                using (MemoryStream memStream = new MemoryStream())
                                {
                                    if (GetZipDataStream(inFileName, entryName, memStream))
                                    {
                                        AddZipEntry(zipStream, entryName, memStream);
                                    }
                                    else
                                    {
                                        throw new Exception(string.Format("Copy zip entry {0} failed", entryName));
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            outTextWriter?.WriteLine(e);
                        }
                    }

                }
                finally
                {
                    if (zipStream != null)
                    {
                        zipStream.IsStreamOwner = true; // Makes the Close also Close the underlying stream
                        zipStream.Close();
                    }
                    fsOut?.Close();
                }
            }
            catch (Exception e)
            {
                outTextWriter?.WriteLine(e);
            }
            return 0;
        }

        private static void AddZipEntry(ZipOutputStream zipStream, string entryName, Stream entryStream)
        {
            entryName = ZipEntry.CleanName(entryName); // Removes drive from name and fixes slash direction
            ZipEntry newEntry = new ZipEntry(entryName);
            newEntry.DateTime = DateTime.Now;           // Note the zip format stores 2 second granularity

            // Specifying the AESKeySize triggers AES encryption. Allowable values are 0 (off), 128 or 256.
            // A password on the ZipOutputStream is required if using AES.
            //   newEntry.AESKeySize = 256;

            // To permit the zip to be unpacked by built-in extractor in WinXP and Server2003, WinZip 8, Java, and other older code,
            // you need to do one of the following: Specify UseZip64.Off, or set the Size.
            // If the file may be bigger than 4GB, or you do not need WinXP built-in compatibility, you do not need either,
            // but the zip will be in Zip64 format which not all utilities can understand.
            //   zipStream.UseZip64 = UseZip64.Off;
            entryStream.Seek(0, SeekOrigin.Begin);
            newEntry.Size = entryStream.Length;

            zipStream.PutNextEntry(newEntry);

            // Zip the file in buffered chunks
            byte[] buffer = new byte[4096];
            StreamUtils.Copy(entryStream, zipStream, buffer);
            zipStream.CloseEntry();
        }

        static List<string> GetZipEntryList(string fileName)
        {
            try
            {
                ZipFile zf = null;
                try
                {
                    List<string> entryList = new List<string>();
                    using (FileStream fs = File.OpenRead(fileName))
                    {
                        zf = new ZipFile(fs);
                        foreach (ZipEntry zipEntry in zf)
                        {
                            if (!zipEntry.IsFile)
                            {
                                continue; // Ignore directories
                            }

                            entryList.Add(zipEntry.Name);
                        }
                    }

                    return entryList;
                }
                finally
                {
                    if (zf != null)
                    {
                        zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                        zf.Close(); // Ensure we release resources
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        static bool GetZipDataStream(string fileName, string entryName, Stream outStream)
        {
            try
            {
                if (string.IsNullOrEmpty(entryName))
                {
                    return false;
                }

                ZipFile zf = null;
                try
                {
                    using (FileStream fs = File.OpenRead(fileName))
                    {
                        zf = new ZipFile(fs);
                        foreach (ZipEntry zipEntry in zf)
                        {
                            if (!zipEntry.IsFile)
                            {
                                continue; // Ignore directories
                            }
                            if (string.Compare(zipEntry.Name, entryName, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                using (Stream zipStream = zf.GetInputStream(zipEntry))
                                {
                                    byte[] buffer = new byte[4096];
                                    StreamUtils.Copy(zipStream, outStream, buffer);
                                }
                                return true;
                            }
                        }
                    }

                    return false;
                }
                finally
                {
                    if (zf != null)
                    {
                        zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                        zf.Close(); // Ensure we release resources
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        static object GetEcuDataObject(string fileName, string entryName, Type type)
        {
            try
            {
                if (string.IsNullOrEmpty(entryName))
                {
                    return null;
                }

                object ecuObject = null;
                ZipFile zf = null;
                try
                {
                    using (FileStream fs = File.OpenRead(fileName))
                    {
                        zf = new ZipFile(fs);
                        foreach (ZipEntry zipEntry in zf)
                        {
                            if (!zipEntry.IsFile)
                            {
                                continue; // Ignore directories
                            }
                            if (string.Compare(zipEntry.Name, entryName, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                Stream zipStream = zf.GetInputStream(zipEntry);
                                using (TextReader reader = new StreamReader(zipStream))
                                {
                                    XmlSerializer serializer = new XmlSerializer(type);
                                    ecuObject = serializer.Deserialize(reader);
                                }
                                break;
                            }
                        }
                    }

                    return ecuObject;
                }
                finally
                {
                    if (zf != null)
                    {
                        zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                        zf.Close(); // Ensure we release resources
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
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
                        if (MergeEcuJob(outTextWriter, fileName, fixedFuncStructListIn, ecuJob, ecuFixedFuncStruct) > 0)
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

        static int MergeEcuJob(TextWriter outTextWriter, string fileName, List<EcuFunctionStructs.EcuFixedFuncStruct> fixedFuncStructList,
            EcuFunctionStructs.EcuJob ecuJobMerge, EcuFunctionStructs.EcuFixedFuncStruct ecuFixedFuncStructMerge)
        {
            if (ecuJobMerge == null || string.IsNullOrEmpty(ecuJobMerge.Name) || ecuJobMerge.EcuJobResultList?.Count == 0)
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
                            if ((string.Compare(ecuJob.Name.Trim(), ecuJobMerge.Name.Trim(), StringComparison.OrdinalIgnoreCase) == 0) &&
                                string.Compare(ecuJob.FuncNameJob.Trim(), ecuJobMerge.FuncNameJob.Trim(), StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                if (EcuJobsArgsIdentical(ecuJob, ecuJobMerge))
                                {
                                    int results = MergeEcuJobResults(null, fileName, ecuJob, ecuJobMerge);
                                    if (results > 0)
                                    {
                                        if (ecuFixedFuncStructMerge.CompatIdListList != null)
                                        {
                                            List<string> compatIdListList = ecuFixedFuncStruct.CompatIdListList ?? new List<string>();
                                            foreach (string compatId in ecuFixedFuncStructMerge.CompatIdListList)
                                            {
                                                if (!ecuFixedFuncStruct.IdPresent(compatId))
                                                {
                                                    compatIdListList.Add(compatId);
                                                    outTextWriter?.WriteLine("Merge Fixed old ID: File='{0}', Job='{1}({2})', OldId='{3}'",
                                                        fileName, ecuJob.Name, ecuJob.FuncNameJob, compatId);
                                                }
                                            }

                                            ecuFixedFuncStruct.CompatIdListList = compatIdListList;
                                        }

                                        if (!ecuFixedFuncStruct.IdPresent(ecuFixedFuncStructMerge.Id))
                                        {
                                            List<string> compatIdListList = ecuFixedFuncStruct.CompatIdListList ?? new List<string>();
                                            compatIdListList.Add(ecuFixedFuncStructMerge.Id);
                                            ecuFixedFuncStruct.CompatIdListList = compatIdListList;
                                        }

                                        if (ecuJobMerge.CompatIdListList != null)
                                        {
                                            List<string> compatIdListList = ecuJob.CompatIdListList ?? new List<string>();
                                            foreach (string compatId in ecuJobMerge.CompatIdListList)
                                            {
                                                if (!ecuJob.IdPresent(compatId))
                                                {
                                                    compatIdListList.Add(compatId);
                                                    outTextWriter?.WriteLine("Merge Job old ID: File='{0}', Job='{1}({2})', OldId='{3}'",
                                                        fileName, ecuJob.Name, ecuJob.FuncNameJob, compatId);
                                                }
                                            }

                                            ecuJob.CompatIdListList = compatIdListList;
                                        }

                                        if (!ecuJob.IdPresent(ecuJobMerge.Id))
                                        {
                                            List<string> compatIdListList = ecuJob.CompatIdListList ?? new List<string>();
                                            compatIdListList.Add(ecuJobMerge.Id.Trim());
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

            if (matches == 0)
            {
                outTextWriter?.WriteLine("Merge Jobs no match: File='{0}', Job='{1}({2})', Args='{3}', Res='{4}'",
                    fileName, ecuJobMerge.Name, ecuJobMerge.FuncNameJob, JobsArgsToString(ecuJobMerge), JobsResultsToString(ecuJobMerge));
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
                                if (ecuJobResult.CompatIdListList != null)
                                {
                                    List<string> compatIdListList = ecuJobResultMatch.CompatIdListList ?? new List<string>();
                                    foreach (string compatId in ecuJobResult.CompatIdListList)
                                    {
                                        if (!ecuJobResult.IdPresent(compatId))
                                        {
                                            compatIdListList.Add(compatId);
                                            outTextWriter?.WriteLine("Merge Result old ID: File='{0}', Job='{1}', OldId='{2}'",
                                                fileName, ecuJobIn.Name, compatId);
                                        }
                                    }
                                    ecuJobResultMatch.CompatIdListList = compatIdListList;
                                }

                                if (!ecuJobResultMatch.IdPresent(ecuJobResult.Id))
                                {
                                    List<string> compatIdListList = ecuJobResultMatch.CompatIdListList ?? new List<string>();
                                    compatIdListList.Add(ecuJobResult.Id);
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
                if ((string.Compare(ecuJob1.Name.Trim(), "SVK_LESEN", StringComparison.OrdinalIgnoreCase) == 0) &&
                    (string.Compare(ecuJob2.Name.Trim(), "SVK_LESEN", StringComparison.OrdinalIgnoreCase) == 0))
                {
                    if (parList1.Count == 1 && parList2.Count == 0 &&
                        string.Compare(parList1[0].Value.Trim(), "SVK_AKTUELL", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }

                    if (parList1.Count == 0 && parList2.Count == 1 &&
                        string.Compare(parList2[0].Value.Trim(), "SVK_AKTUELL", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }
                }
                return false;
            }

            for (int i = 0; i < parList1.Count; i++)
            {
                if (string.Compare(parList1[i].Value.Trim(), parList2[i].Value.Trim(), StringComparison.OrdinalIgnoreCase) != 0)
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
                        if ((string.Compare(ecuJobResult.Name.Trim(), ecuJobResultComp.Name.Trim(), StringComparison.OrdinalIgnoreCase) == 0) &&
                            (string.Compare(ecuJobResult.FuncNameResult.Trim(), ecuJobResultComp.FuncNameResult.Trim(), StringComparison.OrdinalIgnoreCase) == 0))
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
