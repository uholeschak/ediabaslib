using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UdsFileReader
{
    public static class UdsReader
    {
        private static readonly Encoding Encoding = Encoding.GetEncoding(1252);

        public static List<string[]> ExtractFileSegment(List<string> fileList, string segmentName)
        {
            string segmentStart = "[" + segmentName + "]";
            string segmentEnd = "[/" + segmentName + "]";

            List<string[]> lineList = new List<string[]>();
            foreach (string fileName in fileList)
            {
                try
                {
                    using (StreamReader sr = new StreamReader(fileName, Encoding))
                    {
                        bool inSegment = false;
                        for (;;)
                        {
                            string line = sr.ReadLine();
                            if (line == null)
                            {
                                break;
                            }

                            if (line.StartsWith("["))
                            {
                                if (string.Compare(line, segmentStart, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    inSegment = true;
                                }
                                else if (string.Compare(line, segmentEnd, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    inSegment = false;
                                }
                                continue;
                            }

                            if (!inSegment)
                            {
                                continue;
                            }
                            string[] lineArray = line.Split(',');
                            if (lineArray.Length > 0)
                            {
                                lineList.Add(lineArray);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return lineList;
        }

        public static bool GetIncludeFiles(string fileName, List<string> includeFiles)
        {
            try
            {
                string dir = Path.GetDirectoryName(fileName);
                if (dir == null)
                {
                    return false;
                }

                List<string[]> lineList = ExtractFileSegment(new List<string> { fileName }, "INC");
                if (lineList == null)
                {
                    return false;
                }

                foreach (string[] line in lineList)
                {
                    if (line.Length >= 2)
                    {
                        string file = line[1];
                        if (!string.IsNullOrWhiteSpace(file))
                        {
                            string fileNameInc = Path.Combine(dir, file);
                            if (!includeFiles.Contains(fileNameInc))
                            {
                                includeFiles.Add(fileNameInc);
                                if (!GetIncludeFiles(fileNameInc, includeFiles))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
