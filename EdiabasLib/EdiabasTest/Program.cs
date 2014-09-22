using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NDesk.Options;
using EdiabasLib;

namespace EdiabasTest
{
    class Program
    {
        static int Main(string[] args)
        {
            string sgdbFile = null;
            string comPort = null;
            string logFile = null;
            List<string> jobNames = new List<string>();
            bool show_help = false;

            var p = new OptionSet()
            {
                { "s|sgdb=", "sgdb file.",
                  v => sgdbFile = v },
                { "p|port=", "COM port.",
                  v => comPort = v },
                { "l|log=", "log file name.",
                  v => logFile = v },
                { "j|job=", "<job name>#<job parameters semicolon separated>#<request results semicolon separated>.",
                  v => jobNames.Add(v) },
                { "h|help",  "show this message and exit", 
                  v => show_help = v != null },
            };

            List<string> extra;
            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException e)
            {
                string thisName = Path.GetFileNameWithoutExtension(System.AppDomain.CurrentDomain.FriendlyName);
                Console.Write(thisName + ": ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `" + thisName + " --help' for more information.");
                return 1;
            }

            if (show_help)
            {
                ShowHelp(p);
                return 0;
            }

            if (sgdbFile == null)
            {
                Console.WriteLine("No sgdb file specified");
                return 1;
            }

            if (comPort == null)
            {
                Console.WriteLine("No COM port specified");
                return 1;
            }

            if (jobNames.Count < 1)
            {
                Console.WriteLine("No jobs specified");
                return 1;
            }

            try
            {
                using (Ediabas ediabas = new Ediabas())
                {
                    EdCommBmwFast edCommBwmFast = new EdCommBmwFast(ediabas);
                    edCommBwmFast.ComPort = comPort;
                    ediabas.EdCommClass = edCommBwmFast;

                    ediabas.FileSearchDir = Path.GetDirectoryName(sgdbFile);
                    if (logFile != null)
                    {
                        ediabas.SwLog = new StreamWriter(logFile);
                    }

                    // entries must be uppercase!
                    ediabas.ConfigDict.Add("SIMULATION", "0");
                    try
                    {
                        ediabas.ResolveSgbdFile(Path.GetFileName(sgdbFile));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("ResolveSgdbFile failed: " + Ediabas.GetExceptionText(ex));
                        return 1;
                    }

                    foreach (string jobString in jobNames)
                    {
                        if (jobString.Length == 0)
                        {
                            Console.WriteLine("Empty job string");
                            return 1;
                        }

                        ediabas.ArgString = string.Empty;
                        ediabas.ResultsRequests = string.Empty;
                        string[] parts = jobString.Split('#');
                        if ((parts.Length < 1) || (parts[0].Length == 0))
                        {
                            Console.WriteLine("Empty job name");
                            return 1;
                        }
                        string jobName = parts[0];
                        if (parts.Length >= 2)
                        {
                            ediabas.ArgString = parts[1];
                        }
                        if (parts.Length >= 3)
                        {
                            ediabas.ResultsRequests = parts[2];
                        }

                        Console.WriteLine("JOB: " + jobName);
                        try
                        {
                            ediabas.ExecuteJob(jobName);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Job execution failed: " + Ediabas.GetExceptionText(ex));
                            return 1;
                        }

                        int dataSet = 1;
                        List<Dictionary<string, Ediabas.ResultData>> resultSets = ediabas.ResultSets;
                        foreach (Dictionary<string, Ediabas.ResultData> resultDict in resultSets)
                        {
                            Console.WriteLine(string.Format("DATASET: {0}", dataSet));
                            if (ediabas.SwLog != null)
                            {
                                ediabas.SwLog.WriteLine(string.Format("DATASET: {0}", dataSet));
                            }
                            foreach (string key in resultDict.Keys.OrderBy(x => x))
                            {
                                Ediabas.ResultData resultData = resultDict[key];
                                string resultText = string.Empty;
                                if (resultData.opData.GetType() == typeof(string))
                                {
                                    resultText = (string)resultData.opData;
                                }
                                else if (resultData.opData.GetType() == typeof(Double))
                                {
                                    resultText = ((Double)resultData.opData).ToString();
                                }
                                else if (resultData.opData.GetType() == typeof(Int64))
                                {
                                    resultText = string.Format("{0} 0x{0:X08}", ((Int64)resultData.opData), ((Int64)resultData.opData));
                                }
                                else if (resultData.opData.GetType() == typeof(byte[]))
                                {
                                    byte[] data = (byte[])resultData.opData;
                                    foreach (byte value in data)
                                    {
                                        resultText += string.Format("{0:X02} ", value);
                                    }
                                }
                                Console.WriteLine(resultData.name + ": " + resultText);
                                if (ediabas.SwLog != null)
                                {
                                    ediabas.SwLog.WriteLine(resultData.name + ": " + resultText);
                                }
                            }
                            dataSet++;
                        }
                        Console.WriteLine();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }

            return 0;
        }

        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: " + Path.GetFileNameWithoutExtension(System.AppDomain.CurrentDomain.FriendlyName) + " [OPTIONS]");
            Console.WriteLine("EDIABAS simulator");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }
    }
}
