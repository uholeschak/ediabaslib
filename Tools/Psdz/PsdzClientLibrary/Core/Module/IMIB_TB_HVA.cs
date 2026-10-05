using BMW.Rheingold.Module.ISTA;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts;
using BMW.Rheingold.Measurement.Common;
using BMW.Rheingold.Measurement.Common.Contract;

#pragma warning disable CS0649
namespace BMW.Rheingold.Module.ISTA
{
    internal class IMIB_TB_HVA : ISTAModule
    {
        public ITextLocator txtMeldung;

        public int i;

        private IDeviceGeneric tracebuffer;

        public IMIB_TB_HVA(ParameterContainer InParameter)
        {
            if (InParameter != null)
            {
                _globalModuleInParameter = InParameter;
            }
            __handleInParameter();
            i = 0;
        }

        public virtual void Prepare()
        {
        }

        public virtual void Reset()
        {
        }

        private IDeviceGeneric GetDeviceGeneric()
        {
            if (tracebuffer == null)
            {
                IDeviceImib deviceImib = base.MeasurementLauncher.ReserveMeasurementDevice();
                if (deviceImib != null)
                {
                    tracebuffer = deviceImib.VirtualDevice;
                }
                if (tracebuffer == null)
                {
                    throw new ArgumentNullException("The generic device is not available.");
                }
            }
            return tracebuffer;
        }

        public virtual void a_Laden(ref int Ergebnis)
        {
            Logger.WriteInformation("a_Ladencalled");
            string text = "IMIB nicht verbunden!";
            ParameterContainer parameterContainer = new ParameterContainer();
            ParameterContainer outParam = new ParameterContainer();
            ParameterContainer parameterContainer2 = new ParameterContainer();
            base.Factory.CreateServiceDialog(this, "a_Laden", "51695499", _globalTabModuleISTA, 25877, parameterContainer, parameterContainer2).Invoke("ReserveIMIBAdapter", parameterContainer, outParam, parameterContainer2);
            e_Entladen();
            IDeviceGeneric deviceGeneric = GetDeviceGeneric();
            text = (deviceGeneric.LoadPlugin("HVANoise", currentDomain: false).Status ? "True" : "False");
            if ("False".Equals(text))
            {
                text = (deviceGeneric.LoadPlugin("hvanoisemeas", currentDomain: false).Status ? "True" : "False");
            }
            switch (text)
            {
                case "IMIB nicht verbunden!":
                    Ergebnis = -1;
                    text = __Text("69301773323").TextContent.PlainText;
                    break;
                case "true":
                case "True":
                case "TRUE":
                    Ergebnis = 1;
                    break;
                default:
                    Ergebnis = -2;
                    text = __Text("69302222091").TextContent.PlainText;
                    break;
            }
            Trace.TraceInformation("Test_Message", "", DateTime.UtcNow, "IMIB_SysInfo", "TraceBuffer.LoadPlugIn(\\\"hvanoisemeas\\\",false)", Convert.ToString(text));
        }

        public virtual void b_Start()
        {
            Logger.WriteInformation("b_Startcalled");
            IDeviceGeneric deviceGeneric = GetDeviceGeneric();
            if (deviceGeneric != null)
            {
                try
                {
                    _ = deviceGeneric.SetProperty("init", "").AnswerStatus;
                }
                catch (Exception exception)
                {
                    Log.ErrorException("IMIB_TB_HVA.b_Start", exception);
                }
            }
            else
            {
                Log.Error("IMIB_TB_HVA.b_Start()", "No connection to IMIB");
            }
            if (deviceGeneric != null)
            {
                try
                {
                    deviceGeneric.Start();
                }
                catch (Exception exception2)
                {
                    Log.ErrorException("IMIB_TB_HVA.b_Start", exception2);
                }
            }
            else
            {
                Log.Error("IMIB_TB_HVA.b_Start()", "No connection to IMIB");
            }
            if (deviceGeneric != null)
            {
                try
                {
                    _ = deviceGeneric.SetProperty("read", "").AnswerStatus;
                    return;
                }
                catch (Exception exception3)
                {
                    Log.ErrorException("IMIB_TB_HVA.b_Start", exception3);
                    return;
                }
            }
            Log.Error("IMIB_TB_HVA.b_Start()", "No connection to IMIB");
        }

        public virtual void c_Lesen(ref List<string> ErgebnisListe, ref double Status, ref double Wert1, ref double Wert2, ref double Wert3, ref double Wert4, ref double Wert5, ref double Wert6, ref double Drehzahl)
        {
            Logger.WriteInformation("c_Lesencalled");
            ErgebnisListe = new List<string> { "-1", "-1", "-1", "-1", "-1", "-1", "-1", "-1" };
            Status = -1.0;
            Wert1 = -1.0;
            Wert2 = -1.0;
            Wert3 = -1.0;
            Wert4 = -1.0;
            Wert5 = -1.0;
            Wert6 = -1.0;
            Drehzahl = -1.0;
            IDeviceGeneric deviceGeneric = GetDeviceGeneric();
            Sleep(5000);
            if (deviceGeneric != null)
            {
                try
                {
                    GenericDeviceReadResult genericDeviceReadResult = deviceGeneric.Read();
                    ErgebnisListe = new List<string>(genericDeviceReadResult.Values.Select((float x) => x.ToString(CultureInfo.InvariantCulture)));
                    if (genericDeviceReadResult.Values.Count() > 7)
                    {
                        Status = Convert.ToDouble(Convert.ToDecimal(genericDeviceReadResult.Values[0], CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
                        Wert1 = Convert.ToDouble(Convert.ToDecimal(genericDeviceReadResult.Values[1], CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
                        Wert2 = Convert.ToDouble(Convert.ToDecimal(genericDeviceReadResult.Values[2], CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
                        Wert3 = Convert.ToDouble(Convert.ToDecimal(genericDeviceReadResult.Values[3], CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
                        Wert4 = Convert.ToDouble(Convert.ToDecimal(genericDeviceReadResult.Values[4], CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
                        Wert5 = Convert.ToDouble(Convert.ToDecimal(genericDeviceReadResult.Values[5], CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
                        Wert6 = Convert.ToDouble(Convert.ToDecimal(genericDeviceReadResult.Values[6], CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
                        Drehzahl = Convert.ToDouble(Convert.ToDecimal(genericDeviceReadResult.Values[7], CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
                    }
                }
                catch (Exception exception)
                {
                    Log.ErrorException("IMIB_TB_HVA.c_Lesen", exception);
                }
            }
            else
            {
                Log.Error("IMIB_TB_HVA.c_Lesen()", "No connection to IMIB");
            }
            Trace.TraceInformation("Test_Message", "\"\"", "DateTime.UtcNow", "\"IMIB_SysInfo\"", "\"Ergebnis Read('HVA Klackern')\"", "Read_Result");
        }

        public virtual void d_Stop()
        {
            Logger.WriteInformation("d_Stopcalled");
            IDeviceGeneric deviceGeneric = GetDeviceGeneric();
            if (deviceGeneric != null)
            {
                _ = deviceGeneric.LoadPlugin(null, currentDomain: false).Status;
            }
            else
            {
                Log.Error("IMIB_TB_HVA.c_Lesen()", "No connection to IMIB");
            }
        }

        public virtual void e_Entladen()
        {
            Logger.WriteInformation("e_Entladencalled");
            IDeviceGeneric deviceGeneric = GetDeviceGeneric();
            if (deviceGeneric != null)
            {
                _ = deviceGeneric.LoadPlugin(null, currentDomain: false).Status;
            }
            else
            {
                Log.Error("IMIB_TB_HVA.e_Entladen()", "No connection to IMIB");
            }
        }
    }
}
