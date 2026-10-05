using BMW.Rheingold.CoreFramework.Contracts;
using BMW.Rheingold.ISTA.CoreFramework.SOCAccessor;
using BMW.Rheingold.Module.ISTA;
using PsdzClient.Core.Container;
using System;
using System.Collections.Generic;
using System.Globalization;
using BMW.Rheingold.CoreFramework;

namespace BMW.Rheingold.Module.ISTA
{
    internal class TYPMERKMAL_ISTA : ISTAModule
    {
        public bool EcuErrorMessage;

        public string strDlgInfo;

        public bool bWriteLog;

        public int m_maxAnzahlSonderausstattungen;

        public bool p_WeiterButtonEnabledStack;

        public TYPMERKMAL_ISTA(ParameterContainer InParameter)
        {
            if (InParameter != null)
            {
                _globalModuleInParameter = InParameter;
            }
            __handleInParameter();
            EcuErrorMessage = false;
            strDlgInfo = "22.04.2009-TYPMERKMAL_ISTA";
            bWriteLog = true;
            m_maxAnzahlSonderausstattungen = 100;
            p_WeiterButtonEnabledStack = false;
        }

        public virtual void Prepare()
        {
        }

        public virtual void Reset()
        {
        }

        public virtual void InitializeDialog(ref string[] Typmerkmale, ref string[] Sonderausstattungen)
        {
            int num = 0;
            Logger.WriteInformation("InitializeDialogcalled");
            if (bWriteLog)
            {
                Logger.WriteInformation(strDlgInfo);
            }
            Typmerkmale = new string[30];
            base._DoLoopHandling = true;
            for (int i = 0; i < Typmerkmale.Length; i++)
            {
                Typmerkmale[i] = "";
            }
            base._DoLoopHandling = false;
            Sonderausstattungen = new string[m_maxAnzahlSonderausstattungen];
            base._DoLoopHandling = true;
            for (int j = 0; j < Sonderausstattungen.Length; j++)
            {
                Sonderausstattungen[j] = "";
            }
            base._DoLoopHandling = false;
            string text = "";
            Typmerkmale[0] = __convertToString(SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/VIN7"));
            string[] array = new string[18]
            {
            "Marke", "Baureihe", "Verkaufsbezeichnung", "EBezeichnung", "Karosserie", "Tueren", "Antrieb", "Laenderausfuehrung", "Lenkung", "Baujahr",
            "Baumonat", "Motor", "Kraftstoffart", "Getriebe", "Hubraum", "IStufeHO", "IStufeWerk", "Sicherheitsrelevant"
            };
            base._DoLoopHandling = true;
            for (int k = 0; k < array.Length; k++)
            {
                text = "/ExternalData/VehicleIdentification/BaseCharacteristics/" + array[k] + "/Title";
                Typmerkmale[k + 1] = __convertToString(SOCAccessor.OrderContext.System.GetProperty(text));
            }
            base._DoLoopHandling = false;
            Typmerkmale[16] = __convertToString(SOCAccessor.OrderContext.ServiceProgram.GetPersistantProperty("/ExtendedVehicleInformation/SP/IStufeHO"));
            Typmerkmale[17] = __convertToString(SOCAccessor.OrderContext.ServiceProgram.GetPersistantProperty("/ExtendedVehicleInformation/SP/IStufeWerk"));
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            int num5 = 0;
            text = "/ExternalData/ServiceProgram/PublicData/ExtendedVehicleInformation/SP/Equipments";
            if (SOCAccessor.OrderContext.System.GetProperty(text) is List<string> list)
            {
                base._DoLoopHandling = true;
                for (int l = 0; l < list.Count; l++)
                {
                    if (num2 < m_maxAnzahlSonderausstattungen)
                    {
                        Sonderausstattungen[num2] = list[l];
                    }
                    num2++;
                    num3++;
                }
                base._DoLoopHandling = false;
            }
            else
            {
                text = "/ExternalData/VehicleShortTest/AdditionalData/SaLaPas";
                if (SOCAccessor.OrderContext.System.GetProperty(text) is List<string> list2)
                {
                    base._DoLoopHandling = true;
                    for (int m = 0; m < list2.Count; m++)
                    {
                        if (num2 < m_maxAnzahlSonderausstattungen)
                        {
                            Sonderausstattungen[num2] = list2[m];
                        }
                        num2++;
                        num3++;
                    }
                    base._DoLoopHandling = false;
                }
            }
            text = "/ExternalData/VehicleIdentification/AdditionalData/E_Worte";
            if (SOCAccessor.OrderContext.System.GetProperty(text) is List<string> list3)
            {
                base._DoLoopHandling = true;
                for (int n = 0; n < list3.Count; n++)
                {
                    if (num2 < m_maxAnzahlSonderausstattungen)
                    {
                        Sonderausstattungen[num2] = list3[n];
                    }
                    num2++;
                    num4++;
                }
                base._DoLoopHandling = false;
            }
            text = "/ExternalData/VehicleIdentification/AdditionalData/K_Worte";
            if (SOCAccessor.OrderContext.System.GetProperty(text) is List<string> list4)
            {
                base._DoLoopHandling = true;
                for (int num6 = 0; num6 < list4.Count; num6++)
                {
                    if (num2 < m_maxAnzahlSonderausstattungen)
                    {
                        Sonderausstattungen[num2] = list4[num6];
                    }
                    num2++;
                    num5++;
                }
                base._DoLoopHandling = false;
            }
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void Fahrgestellnummer(ref string PFahrgestellnummer)
        {
            int num = 0;
            Logger.WriteInformation("Fahrgestellnummercalled");
            PFahrgestellnummer = __convertToString(SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/VehicleIdentificationNumber"));
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void Marke(ref string PMarke)
        {
            int num = 0;
            Logger.WriteInformation("Markecalled");
            PMarke = __convertToString(SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/Marke/Title"));
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void Baureihe(ref string PBaureihe)
        {
            int num = 0;
            Logger.WriteInformation("Baureihecalled");
            PBaureihe = __convertToString(SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/Baureihe/Title"));
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void EBezeichnung(ref string PEBezeichnung)
        {
            int num = 0;
            Logger.WriteInformation("EBezeichnungcalled");
            PEBezeichnung = __convertToString(SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/EBezeichnung/Title"));
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void Verkaufsbezeichnung(ref string PVerkaufsbezeichnung)
        {
            int num = 0;
            Logger.WriteInformation("Verkaufsbezeichnungcalled");
            PVerkaufsbezeichnung = __convertToString(SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/Verkaufsbezeichnung/Title"));
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void Karosserieform(ref string PKarosserieform)
        {
            int num = 0;
            Logger.WriteInformation("Karosserieformcalled");
            PKarosserieform = __convertToString(SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/Karosserie/Title"));
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void Tueren(ref string PTueren)
        {
            int num = 0;
            Logger.WriteInformation("Tuerencalled");
            PTueren = __convertToString(SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/Tueren/Title"));
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void Antrieb(ref string PAntrieb)
        {
            int num = 0;
            Logger.WriteInformation("Antriebcalled");
            PAntrieb = __convertToString(SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/Antrieb/Title"));
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void Laenderausfuehrung(ref string PLaenderausfuehrung)
        {
            int num = 0;
            Logger.WriteInformation("Laenderausfuehrungcalled");
            PLaenderausfuehrung = __convertToString(SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/Laenderausfuehrung/Title"));
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void Lenkung(ref string PLenkung)
        {
            int num = 0;
            Logger.WriteInformation("Lenkungcalled");
            PLenkung = __convertToString(SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/Lenkung/Title"));
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void Baujahr(ref string PBaujahr)
        {
            int num = 0;
            Logger.WriteInformation("Baujahrcalled");
            PBaujahr = __convertToString(SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/Baujahr/Title"));
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void Baumonat(ref string PBaumonat)
        {
            int num = 0;
            Logger.WriteInformation("Baumonatcalled");
            PBaumonat = __convertToString(SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/Baumonat/Title"));
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void Motor(ref string PMotor)
        {
            int num = 0;
            Logger.WriteInformation("Motorcalled");
            PMotor = __convertToString(SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/Motor/Title"));
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void Kraftstoff(ref string PKraftstoff)
        {
            int num = 0;
            Logger.WriteInformation("Kraftstoffcalled");
            PKraftstoff = __convertToString(SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/Kraftstoffart/Title"));
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void Getriebe(ref string PGetriebe)
        {
            int num = 0;
            Logger.WriteInformation("Getriebecalled");
            PGetriebe = __convertToString(SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/Getriebe/Title"));
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void Hubraum(ref string PHubraum)
        {
            int num = 0;
            Logger.WriteInformation("Hubraumcalled");
            PHubraum = __convertToString(SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/Hubraum/Title"));
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void IStufeHO(ref string PIStufeHO)
        {
            int num = 0;
            Logger.WriteInformation("IStufeHOcalled");
            PIStufeHO = __convertToString(SOCAccessor.OrderContext.ServiceProgram.GetPersistantProperty("/ExtendedVehicleInformation/SP/IStufeHO"));
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void IStufeWerk(ref string PIStufeWerk)
        {
            int num = 0;
            Logger.WriteInformation("IStufeWerkcalled");
            PIStufeWerk = __convertToString(SOCAccessor.OrderContext.ServiceProgram.GetPersistantProperty("/ExtendedVehicleInformation/SP/IStufeWerk"));
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void Sicherheitsrelevant(ref string PSicherheitsrelevant)
        {
            int num = 0;
            Logger.WriteInformation("Sicherheitsrelevantcalled");
            PSicherheitsrelevant = __convertToString(SOCAccessor.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/BaseCharacteristics/Sicherheitsrelevant/Title"));
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void Produktionsdatum(ref string PProduktionsdatum)
        {
            int num = 0;
            Logger.WriteInformation("Produktionsdatum called");
            string text = "";
            string value = "";
            string text2 = "00000000";
            Logger.WriteInformation("vor Exceptionhandling");
            try
            {
                ConfigurationContainer configurationContainer = null;
                configurationContainer = ConfigurationContainer.Deserialize("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<ConfigurationContainer xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" Name=\"Parametrization tree for EDIABAS\" Compression=\"Zip\" MajorVersion=\"1\" MinorVersion=\"0\">\r\n  <Header>\r\n    <Version Major=\"1\" Minor=\"2\" />\r\n    <Adapter Name=\"BMW-EDIABAS-Adapter\">\r\n      <ClassReference FullClassName=\"Siemens.SidisEnterprise.BaseSystem.DiagnosticDevices.Vehicle.Ediabas.Adapter.BMW.EdiabasAdapter\" Location=\"Siemens.SEP.Ediabas.Adapter.BMW\" />\r\n      <SubDeviceCollection />\r\n    </Adapter>\r\n  </Header>\r\n  <Body>\r\n    <Configuration Name=\"EDIABAS_SpExtract\">\r\n      <Run xsi:type=\"SingleChoice\" Name=\"Run\">\r\n        <Children>\r\n          <Node xsi:type=\"SingleChoice\" Name=\"Group\">\r\n            <Children>\r\n              <Node xsi:type=\"SingleChoice\" Name=\"D_CAS\" Comment=\"\">\r\n                <Children>\r\n                  <Node xsi:type=\"SingleChoice\" Name=\"VirtualVariantJob\">\r\n                    <Children>\r\n                      <Node xsi:type=\"Executable\" Name=\"IDENT\" Comment=\"¦|CAS_RR|CAS|=&gt;identdaten&#xD;&#xA;kwp2000: $1a readecuidentification&#xD;&#xA;modus : default¦\">\r\n                        <Children>\r\n                          <Node xsi:type=\"All\" Name=\"Argument\">\r\n                            <Children>\r\n                              <Node xsi:type=\"Value\" Name=\"ECUGroupOrVariant\" Comment=\"\">\r\n                                <Literal>\r\n                                  <Text TranslationMode=\"All\">D_CAS</Text>\r\n                                </Literal>\r\n                              </Node>\r\n                            </Children>\r\n                          </Node>\r\n                        </Children>\r\n                        <Result xsi:type=\"All\" Name=\"Result\">\r\n                          <Children>\r\n                            <Node xsi:type=\"MultipleChoice\" Name=\"Status\">\r\n                              <Children>\r\n                                <Node xsi:type=\"Value\" Name=\"VARIANTE\" Comment=\"\">\r\n                                  <Literal>\r\n                                    <Text TranslationMode=\"All\" />\r\n                                  </Literal>\r\n                                </Node>\r\n                                <Node xsi:type=\"Value\" Name=\"JOB_STATUS\" Comment=\"okay, wenn fehlerfrei  table jobresult status_text\">\r\n                                  <Literal>\r\n                                    <Text TranslationMode=\"All\" />\r\n                                  </Literal>\r\n                                </Node>\r\n                              </Children>\r\n                            </Node>\r\n                          </Children>\r\n                        </Result>\r\n                      </Node>\r\n                    </Children>\r\n                  </Node>\r\n                </Children>\r\n              </Node>\r\n            </Children>\r\n          </Node>\r\n        </Children>\r\n      </Run>\r\n    </Configuration>\r\n  </Body>\r\n</ConfigurationContainer>");
                ParameterContainer parameterContainer = new ParameterContainer();
                ParameterContainer parameterContainer2 = new ParameterContainer();
                ParameterContainer parameterContainer3 = new ParameterContainer();
                parameterContainer.setParameter("DSCConfig", null);
                parameterContainer.setParameter("Display", false);
                parameterContainer.setParameter("FehlerMeldung", true);
                parameterContainer.setParameter("IO_FrageText", null);
                parameterContainer.setParameter("/WurzelIn/FehlerMeldung", EcuErrorMessage);
                parameterContainer.setParameter("/WurzelIn/DSCConfig", configurationContainer);
                parameterContainer.setParameter("/WurzelIn/StateLists/Result[0]/Path", "/Result/Status/SAETZE");
                parameterContainer.setParameter("/WurzelIn/StateLists/Result[0]/Unit", "");
                parameterContainer.setParameter("/WurzelIn/StateLists/Result[1]/Path", "/Result/Status/VARIANTE");
                parameterContainer.setParameter("/WurzelIn/StateLists/Result[1]/Unit", "");
                parameterContainer.setParameter("/WurzelIn/StateLists/Result[2]/Path", "/Result/Status/JOB_STATUS");
                parameterContainer.setParameter("/WurzelIn/StateLists/Result[2]/Unit", "");
                base.Factory.CreateServiceDialog(this, "Produktionsdatum", "51939083", _globalTabModuleISTA, 2051, parameterContainer, parameterContainer3).Invoke("InitializeDialog", parameterContainer, parameterContainer2, parameterContainer3);
                IDiagnosticDeviceResult obj = (IDiagnosticDeviceResult)parameterContainer2.getParameter("/WurzelOut/DSCResult");
                int num2 = 0;
                object iSTAResultAsType = obj.getISTAResultAsType("/Result/Rows/$Count", typeof(int));
                if (iSTAResultAsType != null)
                {
                    num2 = (int)iSTAResultAsType;
                }
                object iSTAResultAsType2 = obj.getISTAResultAsType("/Result/Status/VARIANTE", typeof(string));
                if (iSTAResultAsType2 != null)
                {
                    value = (string)iSTAResultAsType2;
                }
                object iSTAResultAsType3 = obj.getISTAResultAsType("/Result/Status/JOB_STATUS", typeof(string));
                if (iSTAResultAsType3 != null)
                {
                    text = (string)iSTAResultAsType3;
                }
                _ = 0;
            }
            catch (Exception)
            {
                text = "NotOk";
                Logger.WriteInformation("kein 61/3");
            }
            Logger.WriteInformation("nach Exceptionhandling");
            if (text == "OKAY")
            {
                ushort num3 = 0;
                byte b = 0;
                byte b2 = 0;
                text = "";
                ConfigurationContainer configurationContainer2 = null;
                configurationContainer2 = ConfigurationContainer.Deserialize("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<ConfigurationContainer xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" Name=\"Parametrization tree for EDIABAS\" Compression=\"Zip\" MajorVersion=\"1\" MinorVersion=\"0\">\r\n  <Header>\r\n    <Version Major=\"1\" Minor=\"2\" />\r\n    <Adapter Name=\"BMW-EDIABAS-Adapter\">\r\n      <ClassReference FullClassName=\"Siemens.SidisEnterprise.BaseSystem.DiagnosticDevices.Vehicle.Ediabas.Adapter.BMW.EdiabasAdapter\" Location=\"Siemens.SEP.Ediabas.Adapter.BMW\" />\r\n      <SubDeviceCollection />\r\n    </Adapter>\r\n  </Header>\r\n  <Body>\r\n    <Configuration Name=\"EDIABAS_SpExtract\">\r\n      <Run xsi:type=\"SingleChoice\" Name=\"Run\">\r\n        <Children>\r\n          <Node xsi:type=\"SingleChoice\" Name=\"Group\">\r\n            <Children>\r\n              <Node xsi:type=\"SingleChoice\" Name=\"D_CAS\" Comment=\"\">\r\n                <Children>\r\n                  <Node xsi:type=\"SingleChoice\" Name=\"VirtualVariantJob\">\r\n                    <Children>\r\n                      <Node xsi:type=\"Executable\" Name=\"STATUS_PROG_LOCATION_DATUM\" Comment=\"Jobbeschreibung: Dieser Job schreibt Ort und Datum der ECU-Programmierung&#xD;&#xA;Vorbedingungen:  keine&#xD;&#xA;Diagnose-Service: KWP 2000: $21 ReadDataByLocalIdentifier&#xD;&#xA;LocalIdentifier $04&#xD;&#xA;Modus:    Default\">\r\n                        <Children>\r\n                          <Node xsi:type=\"All\" Name=\"Argument\">\r\n                            <Children>\r\n                              <Node xsi:type=\"Value\" Name=\"ECUGroupOrVariant\" Comment=\"\">\r\n                                <Literal>\r\n                                  <Text TranslationMode=\"All\" />\r\n                                </Literal>\r\n                              </Node>\r\n                            </Children>\r\n                          </Node>\r\n                        </Children>\r\n                        <Result xsi:type=\"All\" Name=\"Result\">\r\n                          <Children>\r\n                            <Node xsi:type=\"MultipleChoice\" Name=\"Status\">\r\n                              <Children>\r\n                                <Node xsi:type=\"Value\" Name=\"JOB_STATUS\" Comment=\"Beschreibung:  Status der Job Verarbeitung  Gültige Werte:  OKAY, wenn fehlerfrei  table JobResult STATUS_TEXT\">\r\n                                  <Literal>\r\n                                    <Text TranslationMode=\"All\" />\r\n                                  </Literal>\r\n                                </Node>\r\n                              </Children>\r\n                            </Node>\r\n                            <Node xsi:type=\"Sequence\" Name=\"Rows\">\r\n                              <Children>\r\n                                <Node xsi:type=\"MultipleChoice\" Name=\"Row\">\r\n                                  <Children>\r\n                                    <Node xsi:type=\"Value\" Name=\"PROG_TIME_DAY\" Comment=\"Beschreibung:  Dieses Result enthält den Tag der Programmierung  Datenlänge:  1 Byte  Gültige Werte:  &quot;Tag&quot;: 1-31  Einheit:  keine\">\r\n                                      <Literal>\r\n                                        <UShort>0</UShort>\r\n                                      </Literal>\r\n                                    </Node>\r\n                                    <Node xsi:type=\"Value\" Name=\"PROG_TIME_MONTH\" Comment=\"Beschreibung:  Dieses Result enthält den Monat der Programmierung  Datenlänge:  Byte0,Bit0-3  Gültige Werte:  &quot;Monat&quot;: 1-12  Einheit:  keine\">\r\n                                      <Literal>\r\n                                        <UByte>0</UByte>\r\n                                      </Literal>\r\n                                    </Node>\r\n                                    <Node xsi:type=\"Value\" Name=\"PROG_TIME_YEAR_2_DIGITS\" Comment=\"Beschreibung:  Dieses Result enthält das Jahr der Programmierung  Datenlänge:  1 Byte  Gültige Werte:  &quot;Jahr&quot;: 0-99  Einheit:  keine\">\r\n                                      <Literal>\r\n                                        <UByte>0</UByte>\r\n                                      </Literal>\r\n                                    </Node>\r\n                                  </Children>\r\n                                </Node>\r\n                              </Children>\r\n                            </Node>\r\n                          </Children>\r\n                        </Result>\r\n                      </Node>\r\n                    </Children>\r\n                  </Node>\r\n                </Children>\r\n              </Node>\r\n            </Children>\r\n          </Node>\r\n        </Children>\r\n      </Run>\r\n    </Configuration>\r\n  </Body>\r\n</ConfigurationContainer>");
                configurationContainer2.AddRunOverride("/Run/Group/D_CAS/VirtualVariantJob/STATUS_PROG_LOCATION_DATUM/Argument/ECUGroupOrVariant", value);
                IDiagnosticDeviceResult diagnosticDeviceResult = null;
                ParameterContainer parameterContainer4 = new ParameterContainer();
                ParameterContainer parameterContainer5 = new ParameterContainer();
                ParameterContainer parameterContainer6 = new ParameterContainer();
                parameterContainer4.setParameter("DSCConfig", null);
                parameterContainer4.setParameter("Display", false);
                parameterContainer4.setParameter("FehlerMeldung", true);
                parameterContainer4.setParameter("IO_FrageText", null);
                parameterContainer4.setParameter("/WurzelIn/FehlerMeldung", EcuErrorMessage);
                parameterContainer4.setParameter("/WurzelIn/DSCConfig", configurationContainer2);
                parameterContainer4.setParameter("/WurzelIn/StateLists/Result[0]/Path", "/Result/Status/SAETZE");
                parameterContainer4.setParameter("/WurzelIn/StateLists/Result[0]/Unit", "");
                parameterContainer4.setParameter("/WurzelIn/StateLists/Result[1]/Path", "/Result/Status/JOB_STATUS");
                parameterContainer4.setParameter("/WurzelIn/StateLists/Result[1]/Unit", "");
                parameterContainer4.setParameter("/WurzelIn/StateLists/Result[2]/Path", "/Result/Rows/Row[0]/PROG_TIME_DAY");
                parameterContainer4.setParameter("/WurzelIn/StateLists/Result[2]/Unit", "");
                parameterContainer4.setParameter("/WurzelIn/StateLists/Result[3]/Path", "/Result/Rows/Row[0]/PROG_TIME_MONTH");
                parameterContainer4.setParameter("/WurzelIn/StateLists/Result[3]/Unit", "");
                parameterContainer4.setParameter("/WurzelIn/StateLists/Result[4]/Path", "/Result/Rows/Row[0]/PROG_TIME_YEAR_2_DIGITS");
                parameterContainer4.setParameter("/WurzelIn/StateLists/Result[4]/Unit", "");
                base.Factory.CreateServiceDialog(this, "Produktionsdatum", "51939083", _globalTabModuleISTA, 2068, parameterContainer4, parameterContainer6).Invoke("InitializeDialog", parameterContainer4, parameterContainer5, parameterContainer6);
                diagnosticDeviceResult = (IDiagnosticDeviceResult)parameterContainer5.getParameter("/WurzelOut/DSCResult");
                int num4 = 0;
                object iSTAResultAsType4 = diagnosticDeviceResult.getISTAResultAsType("/Result/Rows/$Count", typeof(int));
                if (iSTAResultAsType4 != null)
                {
                    num4 = (int)iSTAResultAsType4;
                }
                object iSTAResultAsType5 = diagnosticDeviceResult.getISTAResultAsType("/Result/Status/JOB_STATUS", typeof(string));
                if (iSTAResultAsType5 != null)
                {
                    text = (string)iSTAResultAsType5;
                }
                if (num4 > 0)
                {
                    object iSTAResultAsType6 = diagnosticDeviceResult.getISTAResultAsType("/Result/Rows/Row[0]/PROG_TIME_DAY", typeof(ushort));
                    if (iSTAResultAsType6 != null)
                    {
                        num3 = (ushort)iSTAResultAsType6;
                    }
                    object iSTAResultAsType7 = diagnosticDeviceResult.getISTAResultAsType("/Result/Rows/Row[0]/PROG_TIME_MONTH", typeof(byte));
                    if (iSTAResultAsType7 != null)
                    {
                        b2 = (byte)iSTAResultAsType7;
                    }
                    object iSTAResultAsType8 = diagnosticDeviceResult.getISTAResultAsType("/Result/Rows/Row[0]/PROG_TIME_YEAR_2_DIGITS", typeof(byte));
                    if (iSTAResultAsType8 != null)
                    {
                        b = (byte)iSTAResultAsType8;
                    }
                }
                if (text == "OKAY")
                {
                    text2 = "20" + b.ToString("00", CultureInfo.InvariantCulture) + b2.ToString("00", CultureInfo.InvariantCulture) + num3.ToString("00", CultureInfo.InvariantCulture);
                }
                else
                {
                    text = "";
                    ConfigurationContainer configurationContainer3 = null;
                    configurationContainer3 = ConfigurationContainer.Deserialize("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<ConfigurationContainer xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" Name=\"Parametrization tree for EDIABAS\" Compression=\"Zip\" MajorVersion=\"1\" MinorVersion=\"0\">\r\n  <Header>\r\n    <Version Major=\"1\" Minor=\"2\" />\r\n    <Adapter Name=\"BMW-EDIABAS-Adapter\">\r\n      <ClassReference FullClassName=\"Siemens.SidisEnterprise.BaseSystem.DiagnosticDevices.Vehicle.Ediabas.Adapter.BMW.EdiabasAdapter\" Location=\"Siemens.SEP.Ediabas.Adapter.BMW\" />\r\n      <SubDeviceCollection />\r\n    </Adapter>\r\n  </Header>\r\n  <Body>\r\n    <Configuration Name=\"EDIABAS_SpExtract\">\r\n      <Run xsi:type=\"SingleChoice\" Name=\"Run\">\r\n        <Children>\r\n          <Node xsi:type=\"SingleChoice\" Name=\"Group\">\r\n            <Children>\r\n              <Node xsi:type=\"SingleChoice\" Name=\"D_CAS\" Comment=\"\">\r\n                <Children>\r\n                  <Node xsi:type=\"SingleChoice\" Name=\"VirtualVariantJob\">\r\n                    <Children>\r\n                      <Node xsi:type=\"Executable\" Name=\"STATUS_HO_CODE\" Comment=\"8 Byte &quot;HO-Codierung&quot; lesen&#xD;&#xA;KWP 2000: $22 ReadDataByCommonIdentifier&#xD;&#xA;CommonIdentifier=$1013 &#xD;&#xA;Modus : Default\">\r\n                        <Children>\r\n                          <Node xsi:type=\"All\" Name=\"Argument\">\r\n                            <Children>\r\n                              <Node xsi:type=\"Value\" Name=\"ECUGroupOrVariant\" Comment=\"\">\r\n                                <Literal>\r\n                                  <Text TranslationMode=\"All\" />\r\n                                </Literal>\r\n                              </Node>\r\n                            </Children>\r\n                          </Node>\r\n                        </Children>\r\n                        <Result xsi:type=\"All\" Name=\"Result\">\r\n                          <Children>\r\n                            <Node xsi:type=\"MultipleChoice\" Name=\"Status\">\r\n                              <Children>\r\n                                <Node xsi:type=\"Value\" Name=\"JOB_STATUS\" Comment=\"OKAY, wenn fehlerfrei  table JobResult STATUS_TEXT\">\r\n                                  <Literal>\r\n                                    <Text TranslationMode=\"All\" />\r\n                                  </Literal>\r\n                                </Node>\r\n                              </Children>\r\n                            </Node>\r\n                            <Node xsi:type=\"Sequence\" Name=\"Rows\">\r\n                              <Children>\r\n                                <Node xsi:type=\"MultipleChoice\" Name=\"Row\">\r\n                                  <Children>\r\n                                    <Node xsi:type=\"Value\" Name=\"FIRST_REG_TIME_DAY\" Comment=\"&quot;Tag&quot;  Byte5\">\r\n                                      <Literal>\r\n                                        <UShort>0</UShort>\r\n                                      </Literal>\r\n                                    </Node>\r\n                                    <Node xsi:type=\"Value\" Name=\"FIRST_REG_TIME_MONTH\" Comment=\"&quot;Monat&quot;  Byte6\">\r\n                                      <Literal>\r\n                                        <UByte>0</UByte>\r\n                                      </Literal>\r\n                                    </Node>\r\n                                    <Node xsi:type=\"Value\" Name=\"FIRST_REG_TIME_YEAR_2_DIGITS\" Comment=\"&quot;Jahr&quot;  Byte7\">\r\n                                      <Literal>\r\n                                        <UByte>0</UByte>\r\n                                      </Literal>\r\n                                    </Node>\r\n                                  </Children>\r\n                                </Node>\r\n                              </Children>\r\n                            </Node>\r\n                          </Children>\r\n                        </Result>\r\n                      </Node>\r\n                    </Children>\r\n                  </Node>\r\n                </Children>\r\n              </Node>\r\n            </Children>\r\n          </Node>\r\n        </Children>\r\n      </Run>\r\n    </Configuration>\r\n  </Body>\r\n</ConfigurationContainer>");
                    configurationContainer3.AddRunOverride("/Run/Group/D_CAS/VirtualVariantJob/STATUS_HO_CODE/Argument/ECUGroupOrVariant", value);
                    IDiagnosticDeviceResult diagnosticDeviceResult2 = null;
                    ParameterContainer parameterContainer7 = new ParameterContainer();
                    ParameterContainer parameterContainer8 = new ParameterContainer();
                    ParameterContainer parameterContainer9 = new ParameterContainer();
                    parameterContainer7.setParameter("DSCConfig", null);
                    parameterContainer7.setParameter("Display", false);
                    parameterContainer7.setParameter("FehlerMeldung", true);
                    parameterContainer7.setParameter("IO_FrageText", null);
                    parameterContainer7.setParameter("/WurzelIn/FehlerMeldung", EcuErrorMessage);
                    parameterContainer7.setParameter("/WurzelIn/DSCConfig", configurationContainer3);
                    parameterContainer7.setParameter("/WurzelIn/StateLists/Result[0]/Path", "/Result/Status/SAETZE");
                    parameterContainer7.setParameter("/WurzelIn/StateLists/Result[0]/Unit", "");
                    parameterContainer7.setParameter("/WurzelIn/StateLists/Result[1]/Path", "/Result/Status/JOB_STATUS");
                    parameterContainer7.setParameter("/WurzelIn/StateLists/Result[1]/Unit", "");
                    parameterContainer7.setParameter("/WurzelIn/StateLists/Result[2]/Path", "/Result/Rows/Row[0]/FIRST_REG_TIME_DAY");
                    parameterContainer7.setParameter("/WurzelIn/StateLists/Result[2]/Unit", "");
                    parameterContainer7.setParameter("/WurzelIn/StateLists/Result[3]/Path", "/Result/Rows/Row[0]/FIRST_REG_TIME_MONTH");
                    parameterContainer7.setParameter("/WurzelIn/StateLists/Result[3]/Unit", "");
                    parameterContainer7.setParameter("/WurzelIn/StateLists/Result[4]/Path", "/Result/Rows/Row[0]/FIRST_REG_TIME_YEAR_2_DIGITS");
                    parameterContainer7.setParameter("/WurzelIn/StateLists/Result[4]/Unit", "");
                    base.Factory.CreateServiceDialog(this, "Produktionsdatum", "51939083", _globalTabModuleISTA, 2135, parameterContainer7, parameterContainer9).Invoke("InitializeDialog", parameterContainer7, parameterContainer8, parameterContainer9);
                    diagnosticDeviceResult2 = (IDiagnosticDeviceResult)parameterContainer8.getParameter("/WurzelOut/DSCResult");
                    int num5 = 0;
                    object iSTAResultAsType9 = diagnosticDeviceResult2.getISTAResultAsType("/Result/Rows/$Count", typeof(int));
                    if (iSTAResultAsType9 != null)
                    {
                        num5 = (int)iSTAResultAsType9;
                    }
                    object iSTAResultAsType10 = diagnosticDeviceResult2.getISTAResultAsType("/Result/Status/JOB_STATUS", typeof(string));
                    if (iSTAResultAsType10 != null)
                    {
                        text = (string)iSTAResultAsType10;
                    }
                    if (num5 > 0)
                    {
                        object iSTAResultAsType11 = diagnosticDeviceResult2.getISTAResultAsType("/Result/Rows/Row[0]/FIRST_REG_TIME_DAY", typeof(ushort));
                        if (iSTAResultAsType11 != null)
                        {
                            num3 = (ushort)iSTAResultAsType11;
                        }
                        object iSTAResultAsType12 = diagnosticDeviceResult2.getISTAResultAsType("/Result/Rows/Row[0]/FIRST_REG_TIME_MONTH", typeof(byte));
                        if (iSTAResultAsType12 != null)
                        {
                            b2 = (byte)iSTAResultAsType12;
                        }
                        object iSTAResultAsType13 = diagnosticDeviceResult2.getISTAResultAsType("/Result/Rows/Row[0]/FIRST_REG_TIME_YEAR_2_DIGITS", typeof(byte));
                        if (iSTAResultAsType13 != null)
                        {
                            b = (byte)iSTAResultAsType13;
                        }
                    }
                    text2 = ((!(text == "OKAY")) ? "00000000" : ("20" + b.ToString("00", CultureInfo.InvariantCulture) + b2.ToString("00", CultureInfo.InvariantCulture) + num3.ToString("00", CultureInfo.InvariantCulture)));
                }
            }
            else
            {
                text = "";
                value = "";
                Logger.WriteInformation("vor Exceptionhandling2");
                try
                {
                    ConfigurationContainer configurationContainer4 = null;
                    configurationContainer4 = ConfigurationContainer.Deserialize("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<ConfigurationContainer xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" Name=\"Parametrization tree for EDIABAS\" Compression=\"Zip\" MajorVersion=\"1\" MinorVersion=\"0\">\r\n  <Header>\r\n    <Version Major=\"1\" Minor=\"2\" />\r\n    <Adapter Name=\"BMW-EDIABAS-Adapter\">\r\n      <ClassReference FullClassName=\"Siemens.SidisEnterprise.BaseSystem.DiagnosticDevices.Vehicle.Ediabas.Adapter.BMW.EdiabasAdapter\" Location=\"Siemens.SEP.Ediabas.Adapter.BMW\" />\r\n      <SubDeviceCollection />\r\n    </Adapter>\r\n  </Header>\r\n  <Body>\r\n    <Configuration Name=\"EDIABAS_SpExtract\">\r\n      <Run xsi:type=\"SingleChoice\" Name=\"Run\">\r\n        <Children>\r\n          <Node xsi:type=\"SingleChoice\" Name=\"Group\">\r\n            <Children>\r\n              <Node xsi:type=\"SingleChoice\" Name=\"G_CAS\" Comment=\"\">\r\n                <Children>\r\n                  <Node xsi:type=\"SingleChoice\" Name=\"VirtualVariantJob\">\r\n                    <Children>\r\n                      <Node xsi:type=\"Executable\" Name=\"IDENT\" Comment=\"¦|CAS4|FEM_20|CAS4_2|=&gt;identdaten&#xD;&#xA;uds : $22 readdatabyidentifier&#xD;&#xA;uds : $f150 sub-parameter sgbd-index&#xD;&#xA;modus: default¦\">\r\n                        <Children>\r\n                          <Node xsi:type=\"All\" Name=\"Argument\">\r\n                            <Children>\r\n                              <Node xsi:type=\"Value\" Name=\"ECUGroupOrVariant\" Comment=\"\">\r\n                                <Literal>\r\n                                  <Text TranslationMode=\"All\">G_CAS</Text>\r\n                                </Literal>\r\n                              </Node>\r\n                            </Children>\r\n                          </Node>\r\n                        </Children>\r\n                        <Result xsi:type=\"All\" Name=\"Result\">\r\n                          <Children>\r\n                            <Node xsi:type=\"MultipleChoice\" Name=\"Status\">\r\n                              <Children>\r\n                                <Node xsi:type=\"Value\" Name=\"VARIANTE\" Comment=\"\">\r\n                                  <Literal>\r\n                                    <Text TranslationMode=\"All\" />\r\n                                  </Literal>\r\n                                </Node>\r\n                                <Node xsi:type=\"Value\" Name=\"JOB_STATUS\" Comment=\"okay, wenn fehlerfrei  table jobresult status_text\">\r\n                                  <Literal>\r\n                                    <Text TranslationMode=\"All\" />\r\n                                  </Literal>\r\n                                </Node>\r\n                              </Children>\r\n                            </Node>\r\n                          </Children>\r\n                        </Result>\r\n                      </Node>\r\n                    </Children>\r\n                  </Node>\r\n                </Children>\r\n              </Node>\r\n            </Children>\r\n          </Node>\r\n        </Children>\r\n      </Run>\r\n    </Configuration>\r\n  </Body>\r\n</ConfigurationContainer>");
                    ParameterContainer parameterContainer10 = new ParameterContainer();
                    ParameterContainer parameterContainer11 = new ParameterContainer();
                    ParameterContainer parameterContainer12 = new ParameterContainer();
                    parameterContainer10.setParameter("DSCConfig", null);
                    parameterContainer10.setParameter("Display", false);
                    parameterContainer10.setParameter("FehlerMeldung", true);
                    parameterContainer10.setParameter("IO_FrageText", null);
                    parameterContainer10.setParameter("/WurzelIn/FehlerMeldung", EcuErrorMessage);
                    parameterContainer10.setParameter("/WurzelIn/DSCConfig", configurationContainer4);
                    parameterContainer10.setParameter("/WurzelIn/StateLists/Result[0]/Path", "/Result/Status/SAETZE");
                    parameterContainer10.setParameter("/WurzelIn/StateLists/Result[0]/Unit", "");
                    parameterContainer10.setParameter("/WurzelIn/StateLists/Result[1]/Path", "/Result/Status/VARIANTE");
                    parameterContainer10.setParameter("/WurzelIn/StateLists/Result[1]/Unit", "");
                    parameterContainer10.setParameter("/WurzelIn/StateLists/Result[2]/Path", "/Result/Status/JOB_STATUS");
                    parameterContainer10.setParameter("/WurzelIn/StateLists/Result[2]/Unit", "");
                    base.Factory.CreateServiceDialog(this, "Produktionsdatum", "51939083", _globalTabModuleISTA, 2158, parameterContainer10, parameterContainer12).Invoke("InitializeDialog", parameterContainer10, parameterContainer11, parameterContainer12);
                    IDiagnosticDeviceResult obj2 = (IDiagnosticDeviceResult)parameterContainer11.getParameter("/WurzelOut/DSCResult");
                    int num6 = 0;
                    object iSTAResultAsType14 = obj2.getISTAResultAsType("/Result/Rows/$Count", typeof(int));
                    if (iSTAResultAsType14 != null)
                    {
                        num6 = (int)iSTAResultAsType14;
                    }
                    object iSTAResultAsType15 = obj2.getISTAResultAsType("/Result/Status/VARIANTE", typeof(string));
                    if (iSTAResultAsType15 != null)
                    {
                        value = (string)iSTAResultAsType15;
                    }
                    object iSTAResultAsType16 = obj2.getISTAResultAsType("/Result/Status/JOB_STATUS", typeof(string));
                    if (iSTAResultAsType16 != null)
                    {
                        text = (string)iSTAResultAsType16;
                    }
                    _ = 0;
                }
                catch (Exception)
                {
                    text = "NotOk";
                    Logger.WriteInformation("kein 61/3");
                }
                Logger.WriteInformation("nach Exceptionhandling2");
                if (text == "OKAY")
                {
                    ushort num7 = 0;
                    ushort num8 = 0;
                    ushort num9 = 0;
                    string text3 = "";
                    text = "";
                    ConfigurationContainer configurationContainer5 = null;
                    configurationContainer5 = ConfigurationContainer.Deserialize("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<ConfigurationContainer xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" Name=\"Parametrization tree for EDIABAS\" Compression=\"Zip\" MajorVersion=\"1\" MinorVersion=\"0\">\r\n  <Header>\r\n    <Version Major=\"1\" Minor=\"2\" />\r\n    <Adapter Name=\"BMW-EDIABAS-Adapter\">\r\n      <ClassReference FullClassName=\"Siemens.SidisEnterprise.BaseSystem.DiagnosticDevices.Vehicle.Ediabas.Adapter.BMW.EdiabasAdapter\" Location=\"Siemens.SEP.Ediabas.Adapter.BMW\" />\r\n      <SubDeviceCollection />\r\n    </Adapter>\r\n  </Header>\r\n  <Body>\r\n    <Configuration Name=\"EDIABAS_SpExtract\">\r\n      <Run xsi:type=\"SingleChoice\" Name=\"Run\">\r\n        <Children>\r\n          <Node xsi:type=\"SingleChoice\" Name=\"Group\">\r\n            <Children>\r\n              <Node xsi:type=\"SingleChoice\" Name=\"G_CAS\" Comment=\"\">\r\n                <Children>\r\n                  <Node xsi:type=\"SingleChoice\" Name=\"VirtualVariantJob\">\r\n                    <Children>\r\n                      <Node xsi:type=\"Executable\" Name=\"STATUS_CAS_INIT_LOC_DATE\" Comment=\"Konfiguration des CAS bzgl. Schlüssel-Initialisierung auslesen.&#xD;&#xA;JobHeaderFormat&#xD;&#xA;STATUS_CAS_INIT_LOC_DATE&#xD;&#xA;Diagnose-Service: UDS $22 DID $4203\">\r\n                        <Children>\r\n                          <Node xsi:type=\"All\" Name=\"Argument\">\r\n                            <Children>\r\n                              <Node xsi:type=\"Value\" Name=\"ECUGroupOrVariant\" Comment=\"\">\r\n                                <Literal>\r\n                                  <Text TranslationMode=\"All\" />\r\n                                </Literal>\r\n                              </Node>\r\n                            </Children>\r\n                          </Node>\r\n                        </Children>\r\n                        <Result xsi:type=\"All\" Name=\"Result\">\r\n                          <Children>\r\n                            <Node xsi:type=\"MultipleChoice\" Name=\"Status\">\r\n                              <Children>\r\n                                <Node xsi:type=\"Value\" Name=\"JOB_STATUS\" Comment=\"OKAY, wenn fehlerfrei  table JobResult STATUS_TEXT\">\r\n                                  <Literal>\r\n                                    <Text TranslationMode=\"All\" />\r\n                                  </Literal>\r\n                                </Node>\r\n                              </Children>\r\n                            </Node>\r\n                            <Node xsi:type=\"Sequence\" Name=\"Rows\">\r\n                              <Children>\r\n                                <Node xsi:type=\"MultipleChoice\" Name=\"Row\">\r\n                                  <Children>\r\n                                    <Node xsi:type=\"Value\" Name=\"STAT_INIT_DAY_WERT\" Comment=\"Tag der CAS-/Schlüssel-Initialisierung 1 - 31 Dieser Wert ist nach dem Verriegeln des EWS4_TRSP_SK nicht mehr änderbar.\">\r\n                                      <Literal>\r\n                                        <UShort>0</UShort>\r\n                                      </Literal>\r\n                                    </Node>\r\n                                    <Node xsi:type=\"Value\" Name=\"STAT_INIT_LOCATION_WERT\" Comment=\"Ort der Schlüssel-Initialisierung (4 Zeichen ASCII) 0240 = Werk 2.4, 0220 =Werk 2.2, 0100 =Werk München, ... Dieser Wert ist nach dem Verriegeln des EWS4_TRSP_SK nicht mehr änderbar.\">\r\n                                      <Literal>\r\n                                        <Text TranslationMode=\"All\" />\r\n                                      </Literal>\r\n                                    </Node>\r\n                                    <Node xsi:type=\"Value\" Name=\"STAT_INIT_MONTH_WERT\" Comment=\"Monat der CAS-/Schlüssel-Initialisierung 1 - 12 Dieser Wert ist nach dem Verriegeln des EWS4_TRSP_SK nicht mehr änderbar.\">\r\n                                      <Literal>\r\n                                        <UShort>0</UShort>\r\n                                      </Literal>\r\n                                    </Node>\r\n                                    <Node xsi:type=\"Value\" Name=\"STAT_INIT_YEAR_WERT\" Comment=\"Jahr der CAS-/Schlüssel-Initialisierung 2000 - 2999 Dieser Wert ist nach dem Verriegeln des EWS4_TRSP_SK nicht mehr änderbar.\">\r\n                                      <Literal>\r\n                                        <UShort>0</UShort>\r\n                                      </Literal>\r\n                                    </Node>\r\n                                  </Children>\r\n                                </Node>\r\n                              </Children>\r\n                            </Node>\r\n                          </Children>\r\n                        </Result>\r\n                      </Node>\r\n                    </Children>\r\n                  </Node>\r\n                </Children>\r\n              </Node>\r\n            </Children>\r\n          </Node>\r\n        </Children>\r\n      </Run>\r\n    </Configuration>\r\n  </Body>\r\n</ConfigurationContainer>");
                    configurationContainer5.AddRunOverride("/Run/Group/G_CAS/VirtualVariantJob/STATUS_CAS_INIT_LOC_DATE/Argument/ECUGroupOrVariant", value);
                    IDiagnosticDeviceResult diagnosticDeviceResult3 = null;
                    ParameterContainer parameterContainer13 = new ParameterContainer();
                    ParameterContainer parameterContainer14 = new ParameterContainer();
                    ParameterContainer parameterContainer15 = new ParameterContainer();
                    parameterContainer13.setParameter("DSCConfig", null);
                    parameterContainer13.setParameter("Display", false);
                    parameterContainer13.setParameter("FehlerMeldung", true);
                    parameterContainer13.setParameter("IO_FrageText", null);
                    parameterContainer13.setParameter("/WurzelIn/FehlerMeldung", EcuErrorMessage);
                    parameterContainer13.setParameter("/WurzelIn/DSCConfig", configurationContainer5);
                    parameterContainer13.setParameter("/WurzelIn/StateLists/Result[0]/Path", "/Result/Status/SAETZE");
                    parameterContainer13.setParameter("/WurzelIn/StateLists/Result[0]/Unit", "");
                    parameterContainer13.setParameter("/WurzelIn/StateLists/Result[1]/Path", "/Result/Status/JOB_STATUS");
                    parameterContainer13.setParameter("/WurzelIn/StateLists/Result[1]/Unit", "");
                    parameterContainer13.setParameter("/WurzelIn/StateLists/Result[2]/Path", "/Result/Rows/Row[0]/STAT_INIT_DAY_WERT");
                    parameterContainer13.setParameter("/WurzelIn/StateLists/Result[2]/Unit", "");
                    parameterContainer13.setParameter("/WurzelIn/StateLists/Result[3]/Path", "/Result/Rows/Row[0]/STAT_INIT_LOCATION_WERT");
                    parameterContainer13.setParameter("/WurzelIn/StateLists/Result[3]/Unit", "");
                    parameterContainer13.setParameter("/WurzelIn/StateLists/Result[4]/Path", "/Result/Rows/Row[0]/STAT_INIT_MONTH_WERT");
                    parameterContainer13.setParameter("/WurzelIn/StateLists/Result[4]/Unit", "");
                    parameterContainer13.setParameter("/WurzelIn/StateLists/Result[5]/Path", "/Result/Rows/Row[0]/STAT_INIT_YEAR_WERT");
                    parameterContainer13.setParameter("/WurzelIn/StateLists/Result[5]/Unit", "");
                    base.Factory.CreateServiceDialog(this, "Produktionsdatum", "51939083", _globalTabModuleISTA, 2255, parameterContainer13, parameterContainer15).Invoke("InitializeDialog", parameterContainer13, parameterContainer14, parameterContainer15);
                    diagnosticDeviceResult3 = (IDiagnosticDeviceResult)parameterContainer14.getParameter("/WurzelOut/DSCResult");
                    int num10 = 0;
                    object iSTAResultAsType17 = diagnosticDeviceResult3.getISTAResultAsType("/Result/Rows/$Count", typeof(int));
                    if (iSTAResultAsType17 != null)
                    {
                        num10 = (int)iSTAResultAsType17;
                    }
                    object iSTAResultAsType18 = diagnosticDeviceResult3.getISTAResultAsType("/Result/Status/JOB_STATUS", typeof(string));
                    if (iSTAResultAsType18 != null)
                    {
                        text = (string)iSTAResultAsType18;
                    }
                    if (num10 > 0)
                    {
                        object iSTAResultAsType19 = diagnosticDeviceResult3.getISTAResultAsType("/Result/Rows/Row[0]/STAT_INIT_DAY_WERT", typeof(ushort));
                        if (iSTAResultAsType19 != null)
                        {
                            num7 = (ushort)iSTAResultAsType19;
                        }
                        object iSTAResultAsType20 = diagnosticDeviceResult3.getISTAResultAsType("/Result/Rows/Row[0]/STAT_INIT_LOCATION_WERT", typeof(string));
                        if (iSTAResultAsType20 != null)
                        {
                            text3 = (string)iSTAResultAsType20;
                        }
                        object iSTAResultAsType21 = diagnosticDeviceResult3.getISTAResultAsType("/Result/Rows/Row[0]/STAT_INIT_MONTH_WERT", typeof(ushort));
                        if (iSTAResultAsType21 != null)
                        {
                            num9 = (ushort)iSTAResultAsType21;
                        }
                        object iSTAResultAsType22 = diagnosticDeviceResult3.getISTAResultAsType("/Result/Rows/Row[0]/STAT_INIT_YEAR_WERT", typeof(ushort));
                        if (iSTAResultAsType22 != null)
                        {
                            num8 = (ushort)iSTAResultAsType22;
                        }
                    }
                    text2 = ((!(text == "OKAY") || !(text3 != "0220")) ? "00000000" : (num8.ToString("0000", CultureInfo.InvariantCulture) + num9.ToString("00", CultureInfo.InvariantCulture) + num7.ToString("00", CultureInfo.InvariantCulture)));
                }
            }
            PProduktionsdatum = text2;
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void SteuergeraeteListe(ref string[] SG_Gruppen, ref string[] SG_Varianten, ref int[] SG_Adressen)
        {
            int num = 0;
            Logger.WriteInformation("SteuergeraeteListecalled");
            string text = null;
            text = "/ExternalData/ServiceProgram/PublicData/ExtendedVehicleInformation/SP/Variants";
            List<string> list = SOCAccessor.OrderContext.System.GetProperty(text) as List<string>;
            int[] array = null;
            string[] array2 = null;
            string[] array3 = null;
            int num2 = 0;
            bool flag = false;
            num2 = list?.Count ?? (-1);
            if (num2 < 0)
            {
                flag = true;
            }
            else
            {
                array = new int[num2];
                array2 = new string[num2];
                array3 = new string[num2];
                int num3 = 0;
                while (!flag && num3 < num2)
                {
                    string text2 = list[num3];
                    int num4 = text2.IndexOf(":", StringComparison.OrdinalIgnoreCase);
                    if (num4 > 0 && text2.Length > num4)
                    {
                        int num5 = text2.IndexOf(":", num4 + 1, StringComparison.OrdinalIgnoreCase);
                        if (num5 > 0 && text2.Length > num5)
                        {
                            array2[num3] = text2.Substring(0, num4);
                            array3[num3] = text2.Substring(num4 + 1, num5 - num4 - 1);
                            string text3 = text2.Substring(num5 + 1);
                            string text4 = text3;
                            foreach (char value in text4)
                            {
                                if ("0123456789".IndexOf(value) < 0)
                                {
                                    flag = true;
                                }
                            }
                            if (!flag)
                            {
                                array[num3] = Convert.ToInt32(text3, CultureInfo.InvariantCulture);
                            }
                        }
                        else
                        {
                            flag = true;
                        }
                    }
                    else
                    {
                        flag = true;
                    }
                    num3++;
                }
            }
            if (flag)
            {
                SG_Gruppen = new string[1];
                SG_Gruppen[0] = "FEHLER";
                SG_Varianten = new string[1];
                SG_Varianten[0] = "FEHLER";
                SG_Adressen = new int[1];
                SG_Adressen[0] = -1;
            }
            else
            {
                SG_Gruppen = new string[num2];
                SG_Gruppen = array2;
                SG_Varianten = new string[num2];
                SG_Varianten = array3;
                SG_Adressen = new int[num2];
                SG_Adressen = array;
            }
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void SollverbauGruppenliste(ref string[] Gruppenliste)
        {
            int num = 0;
            Logger.WriteInformation("SollverbauGruppenlistecalled");
            string text = null;
            text = "/ExternalData/ServiceProgram/PublicData/ExtendedVehicleInformation/SP/Groups";
            List<string> list = SOCAccessor.OrderContext.System.GetProperty(text) as List<string>;
            int num2 = list?.Count ?? 0;
            if (num2 > 0)
            {
                Gruppenliste = new string[num2];
                list.CopyTo(Gruppenliste);
            }
            else
            {
                Gruppenliste = new string[1];
                Gruppenliste[0] = "dummy";
            }
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }
    }
}
