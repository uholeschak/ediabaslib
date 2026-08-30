using BMW.Rheingold.CoreFramework.Contracts;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.Module.ISTA;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System;

namespace BMW.Rheingold.Module.ISTA
{
    internal class Dialog_Zuendungstatus : ISTAServiceDialog
    {
        public IServiceDialog __MessageServiceDlg;

        public bool EcuErrorMessage;

        public short ZwischenstandZuendung;

        public short Klemme15Spg;

        public ITextLocator ZuendungText;

        public Dialog_Zuendungstatus(ParameterContainer InParameter)
        {
            if (InParameter != null)
            {
                _globalModuleInParameter = InParameter;
            }
            __handleInParameter();
            EcuErrorMessage = false;
            ZwischenstandZuendung = 0;
            Klemme15Spg = 0;
            __MessageServiceDlg = base.Factory.CreateServiceDialog(this, "global", "51915403", _globalTabModuleISTA, 2642, InParameter, new ParameterContainer());
        }

        public virtual void Prepare()
        {
        }

        public virtual void Reset()
        {
            DocumentHandler(DocumentStatementAction.Remove, __Document("61055504139"), 3);
            DocumentHandler(DocumentStatementAction.Remove, __Document("61057296779"), 3);
        }

        public virtual void ZuendungEin(bool i_automatic, bool i_PopUp, ITextLocator i_ZuendungEinText, string i_hilfsvariable, ref short i_KL15spg)
        {
            int num = 0;
            Logger.WriteInformation("ZuendungEincalled");
            ZuendungText = i_ZuendungEinText;
            Klemme15Spg = 0;
            ConfigurationContainer configurationContainer = null;
            configurationContainer = ConfigurationContainer.Deserialize("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<ConfigurationContainer xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" Name=\"Parametrization tree for EDIABAS\" Compression=\"Zip\" MajorVersion=\"1\" MinorVersion=\"0\">\r\n  <Header>\r\n    <Version Major=\"1\" Minor=\"2\" />\r\n    <Adapter Name=\"BMW-EDIABAS-Adapter\">\r\n      <ClassReference FullClassName=\"Siemens.SidisEnterprise.BaseSystem.DiagnosticDevices.Vehicle.Ediabas.Adapter.BMW.EdiabasAdapter\" Location=\"Siemens.SEP.Ediabas.Adapter.BMW\" />\r\n      <SubDeviceCollection />\r\n    </Adapter>\r\n  </Header>\r\n  <Body>\r\n    <Configuration Name=\"EDIABAS_SpExtract\">\r\n      <Run xsi:type=\"SingleChoice\" Name=\"Run\">\r\n        <Children>\r\n          <Node xsi:type=\"SingleChoice\" Name=\"Group\">\r\n            <Children>\r\n              <Node xsi:type=\"SingleChoice\" Name=\"UnknownGroup\" Comment=\"\">\r\n                <Children>\r\n                  <Node xsi:type=\"SingleChoice\" Name=\"VirtualVariantJob\">\r\n                    <Children>\r\n                      <Node xsi:type=\"Executable\" Name=\"GET_VOLTAGE\" Comment=\"KL30 oder KL 15 analog einlesen\">\r\n                        <Children>\r\n                          <Node xsi:type=\"All\" Name=\"Argument\">\r\n                            <Children>\r\n                              <Node xsi:type=\"Value\" Name=\"ECUGroupOrVariant\" Comment=\"\">\r\n                                <Literal>\r\n                                  <Text TranslationMode=\"All\">DIA_DOSE</Text>\r\n                                </Literal>\r\n                              </Node>\r\n                              <Node xsi:type=\"Value\" Name=\"ARG1\" Comment=\"\">\r\n                                <Literal>\r\n                                  <Text TranslationMode=\"All\">KL15</Text>\r\n                                </Literal>\r\n                              </Node>\r\n                            </Children>\r\n                          </Node>\r\n                        </Children>\r\n                        <Result xsi:type=\"All\" Name=\"Result\">\r\n                          <Children>\r\n                            <Node xsi:type=\"Sequence\" Name=\"Rows\">\r\n                              <Children>\r\n                                <Node xsi:type=\"MultipleChoice\" Name=\"Row\">\r\n                                  <Children>\r\n                                    <Node xsi:type=\"Value\" Name=\"SPANNUNG_V\" Comment=\"Spannung in Millivolt\">\r\n                                      <Literal>\r\n                                        <Short>0</Short>\r\n                                      </Literal>\r\n                                    </Node>\r\n                                  </Children>\r\n                                </Node>\r\n                              </Children>\r\n                            </Node>\r\n                          </Children>\r\n                        </Result>\r\n                      </Node>\r\n                    </Children>\r\n                  </Node>\r\n                </Children>\r\n              </Node>\r\n            </Children>\r\n          </Node>\r\n        </Children>\r\n      </Run>\r\n    </Configuration>\r\n  </Body>\r\n</ConfigurationContainer>");
            IDiagnosticDeviceResult diagnosticDeviceResult = null;
            ParameterContainer parameterContainer = new ParameterContainer();
            ParameterContainer parameterContainer2 = new ParameterContainer();
            ParameterContainer parameterContainer3 = new ParameterContainer();
            parameterContainer.setParameter("DSCConfig", null);
            parameterContainer.setParameter("Display", false);
            parameterContainer.setParameter("FehlerMeldung", true);
            parameterContainer.setParameter("IO_FrageText", null);
            parameterContainer.setParameter("/WurzelIn/FehlerMeldung", EcuErrorMessage);
            parameterContainer.setParameter("/WurzelIn/DSCConfig", configurationContainer);
            parameterContainer.setParameter("/WurzelIn/StateLists/Result[0]/Path", "/Result/Rows/Row[0]/SPANNUNG_V");
            parameterContainer.setParameter("/WurzelIn/StateLists/Result[0]/Unit", "");
            base.Factory.CreateServiceDialog(this, "ZuendungEin", "51939083", _globalTabModuleISTA, 70, parameterContainer, parameterContainer3).Invoke("InitializeDialog", parameterContainer, parameterContainer2, parameterContainer3);
            diagnosticDeviceResult = (IDiagnosticDeviceResult)parameterContainer2.getParameter("/WurzelOut/DSCResult");
            int num2 = 0;
            object iSTAResultAsType = diagnosticDeviceResult.getISTAResultAsType("/Result/Rows/$Count", typeof(int));
            if (iSTAResultAsType != null)
            {
                num2 = (int)iSTAResultAsType;
            }
            if (num2 > 0)
            {
                object iSTAResultAsType2 = diagnosticDeviceResult.getISTAResultAsType("/Result/Rows/Row[0]/SPANNUNG_V", typeof(short));
                if (iSTAResultAsType2 != null)
                {
                    Klemme15Spg = (short)iSTAResultAsType2;
                }
            }
            ZwischenstandZuendung = Klemme15Spg;
            int num3 = 0;
            int num4 = 0;
            if (i_PopUp)
            {
                if (i_automatic)
                {
                    while (Klemme15Spg < 8000)
                    {
                        base._DoLoopHandling = true;
                        __MessagePopup(i_ZuendungEinText.TextContent);
                        Sleep(500);
                        ConfigurationContainer configurationContainer2 = null;
                        configurationContainer2 = ConfigurationContainer.Deserialize("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<ConfigurationContainer xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" Name=\"Parametrization tree for EDIABAS\" Compression=\"Zip\" MajorVersion=\"1\" MinorVersion=\"0\">\r\n  <Header>\r\n    <Version Major=\"1\" Minor=\"2\" />\r\n    <Adapter Name=\"BMW-EDIABAS-Adapter\">\r\n      <ClassReference FullClassName=\"Siemens.SidisEnterprise.BaseSystem.DiagnosticDevices.Vehicle.Ediabas.Adapter.BMW.EdiabasAdapter\" Location=\"Siemens.SEP.Ediabas.Adapter.BMW\" />\r\n      <SubDeviceCollection />\r\n    </Adapter>\r\n  </Header>\r\n  <Body>\r\n    <Configuration Name=\"EDIABAS_SpExtract\">\r\n      <Run xsi:type=\"SingleChoice\" Name=\"Run\">\r\n        <Children>\r\n          <Node xsi:type=\"SingleChoice\" Name=\"Group\">\r\n            <Children>\r\n              <Node xsi:type=\"SingleChoice\" Name=\"UnknownGroup\" Comment=\"\">\r\n                <Children>\r\n                  <Node xsi:type=\"SingleChoice\" Name=\"VirtualVariantJob\">\r\n                    <Children>\r\n                      <Node xsi:type=\"Executable\" Name=\"GET_VOLTAGE\" Comment=\"KL30 oder KL 15 analog einlesen\">\r\n                        <Children>\r\n                          <Node xsi:type=\"All\" Name=\"Argument\">\r\n                            <Children>\r\n                              <Node xsi:type=\"Value\" Name=\"ECUGroupOrVariant\" Comment=\"\">\r\n                                <Literal>\r\n                                  <Text TranslationMode=\"All\">DIA_DOSE</Text>\r\n                                </Literal>\r\n                              </Node>\r\n                              <Node xsi:type=\"Value\" Name=\"ARG1\" Comment=\"\">\r\n                                <Literal>\r\n                                  <Text TranslationMode=\"All\">KL15</Text>\r\n                                </Literal>\r\n                              </Node>\r\n                            </Children>\r\n                          </Node>\r\n                        </Children>\r\n                        <Result xsi:type=\"All\" Name=\"Result\">\r\n                          <Children>\r\n                            <Node xsi:type=\"Sequence\" Name=\"Rows\">\r\n                              <Children>\r\n                                <Node xsi:type=\"MultipleChoice\" Name=\"Row\">\r\n                                  <Children>\r\n                                    <Node xsi:type=\"Value\" Name=\"SPANNUNG_V\" Comment=\"Spannung in Millivolt\">\r\n                                      <Literal>\r\n                                        <Short>0</Short>\r\n                                      </Literal>\r\n                                    </Node>\r\n                                  </Children>\r\n                                </Node>\r\n                              </Children>\r\n                            </Node>\r\n                          </Children>\r\n                        </Result>\r\n                      </Node>\r\n                    </Children>\r\n                  </Node>\r\n                </Children>\r\n              </Node>\r\n            </Children>\r\n          </Node>\r\n        </Children>\r\n      </Run>\r\n    </Configuration>\r\n  </Body>\r\n</ConfigurationContainer>");
                        IDiagnosticDeviceResult diagnosticDeviceResult2 = null;
                        ParameterContainer parameterContainer4 = new ParameterContainer();
                        ParameterContainer parameterContainer5 = new ParameterContainer();
                        ParameterContainer parameterContainer6 = new ParameterContainer();
                        parameterContainer4.setParameter("DSCConfig", null);
                        parameterContainer4.setParameter("Display", false);
                        parameterContainer4.setParameter("FehlerMeldung", true);
                        parameterContainer4.setParameter("IO_FrageText", null);
                        parameterContainer4.setParameter("/WurzelIn/FehlerMeldung", EcuErrorMessage);
                        parameterContainer4.setParameter("/WurzelIn/DSCConfig", configurationContainer2);
                        parameterContainer4.setParameter("/WurzelIn/StateLists/Result[0]/Path", "/Result/Rows/Row[0]/SPANNUNG_V");
                        parameterContainer4.setParameter("/WurzelIn/StateLists/Result[0]/Unit", "");
                        base.Factory.CreateServiceDialog(this, "ZuendungEin", "51939083", _globalTabModuleISTA, 43412, parameterContainer4, parameterContainer6).Invoke("InitializeDialog", parameterContainer4, parameterContainer5, parameterContainer6);
                        diagnosticDeviceResult2 = (IDiagnosticDeviceResult)parameterContainer5.getParameter("/WurzelOut/DSCResult");
                        int num5 = 0;
                        object iSTAResultAsType3 = diagnosticDeviceResult2.getISTAResultAsType("/Result/Rows/$Count", typeof(int));
                        if (iSTAResultAsType3 != null)
                        {
                            num5 = (int)iSTAResultAsType3;
                        }
                        if (num5 > 0)
                        {
                            object iSTAResultAsType4 = diagnosticDeviceResult2.getISTAResultAsType("/Result/Rows/Row[0]/SPANNUNG_V", typeof(short));
                            if (iSTAResultAsType4 != null)
                            {
                                Klemme15Spg = (short)iSTAResultAsType4;
                            }
                        }
                        if (Vehicle.VCI.VCIType == VCIDeviceType.SIM)
                        {
                            Klemme15Spg = 12000;
                        }
                        base._DoLoopHandling = false;
                    }
                }
                else
                {
                    __MessagePopup(i_ZuendungEinText.TextContent);
                }
            }
            else if (i_automatic)
            {
                do
                {
                    base._DoLoopHandling = true;
                    num3++;
                    if (num3 < 2)
                    {
                        DocumentHandler(DocumentStatementAction.Add, __Document("61055504139"), 3);
                    }
                    if (num4 == 0)
                    {
                        ConfigurationContainer configurationContainer3 = null;
                        configurationContainer3 = ConfigurationContainer.Deserialize("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<ConfigurationContainer xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" Name=\"Parametrization tree for EDIABAS\" Compression=\"Zip\" MajorVersion=\"1\" MinorVersion=\"0\">\r\n  <Header>\r\n    <Version Major=\"1\" Minor=\"2\" />\r\n    <Adapter Name=\"BMW-EDIABAS-Adapter\">\r\n      <ClassReference FullClassName=\"Siemens.SidisEnterprise.BaseSystem.DiagnosticDevices.Vehicle.Ediabas.Adapter.BMW.EdiabasAdapter\" Location=\"Siemens.SEP.Ediabas.Adapter.BMW\" />\r\n      <SubDeviceCollection />\r\n    </Adapter>\r\n  </Header>\r\n  <Body>\r\n    <Configuration Name=\"EDIABAS_SpExtract\">\r\n      <Run xsi:type=\"SingleChoice\" Name=\"Run\">\r\n        <Children>\r\n          <Node xsi:type=\"SingleChoice\" Name=\"Group\">\r\n            <Children>\r\n              <Node xsi:type=\"SingleChoice\" Name=\"UnknownGroup\" Comment=\"\">\r\n                <Children>\r\n                  <Node xsi:type=\"SingleChoice\" Name=\"VirtualVariantJob\">\r\n                    <Children>\r\n                      <Node xsi:type=\"Executable\" Name=\"FA_LESEN\" Comment=\"Liest den Fahrzeugauftrag aus der BMSX&#xD;&#xA;UDS: $22 ReadDataByIdentifier&#xD;&#xA;UDS: $3F06 DataIdentifier FA_lesen\">\r\n                        <Children>\r\n                          <Node xsi:type=\"All\" Name=\"Argument\">\r\n                            <Children>\r\n                              <Node xsi:type=\"Value\" Name=\"ECUGroupOrVariant\" Comment=\"\">\r\n                                <Literal>\r\n                                  <Text TranslationMode=\"All\">X_K001</Text>\r\n                                </Literal>\r\n                              </Node>\r\n                            </Children>\r\n                          </Node>\r\n                        </Children>\r\n                        <Result xsi:type=\"All\" Name=\"Result\">\r\n                          <Children>\r\n                            <Node xsi:type=\"MultipleChoice\" Name=\"Status\">\r\n                              <Children>\r\n                                <Node xsi:type=\"Value\" Name=\"JOB_STATUS\" Comment=\"OKAY, wenn fehlerfrei  table JobResult STATUS_TEXT\">\r\n                                  <Literal>\r\n                                    <Text TranslationMode=\"All\" />\r\n                                  </Literal>\r\n                                </Node>\r\n                              </Children>\r\n                            </Node>\r\n                            <Node xsi:type=\"Sequence\" Name=\"Rows\">\r\n                              <Children>\r\n                                <Node xsi:type=\"MultipleChoice\" Name=\"Row\">\r\n                                  <Children>\r\n                                    <Node xsi:type=\"Value\" Name=\"FAHRZEUGAUFTRAG\" Comment=\"Daten des Fahrzeugauftrages\">\r\n                                      <Literal>\r\n                                        <Text TranslationMode=\"All\" />\r\n                                      </Literal>\r\n                                    </Node>\r\n                                  </Children>\r\n                                </Node>\r\n                              </Children>\r\n                            </Node>\r\n                          </Children>\r\n                        </Result>\r\n                      </Node>\r\n                    </Children>\r\n                  </Node>\r\n                </Children>\r\n              </Node>\r\n            </Children>\r\n          </Node>\r\n        </Children>\r\n      </Run>\r\n    </Configuration>\r\n  </Body>\r\n</ConfigurationContainer>");
                        IDiagnosticDeviceResult diagnosticDeviceResult3 = null;
                        ParameterContainer parameterContainer7 = new ParameterContainer();
                        ParameterContainer parameterContainer8 = new ParameterContainer();
                        ParameterContainer parameterContainer9 = new ParameterContainer();
                        parameterContainer7.setParameter("DSCConfig", null);
                        parameterContainer7.setParameter("Display", false);
                        parameterContainer7.setParameter("FehlerMeldung", true);
                        parameterContainer7.setParameter("IO_FrageText", null);
                        parameterContainer7.setParameter("/WurzelIn/FehlerMeldung", EcuErrorMessage);
                        parameterContainer7.setParameter("/WurzelIn/DSCConfig", configurationContainer3);
                        parameterContainer7.setParameter("/WurzelIn/StateLists/Result[0]/Path", "/Result/Status/JOB_STATUS");
                        parameterContainer7.setParameter("/WurzelIn/StateLists/Result[0]/Unit", "");
                        parameterContainer7.setParameter("/WurzelIn/StateLists/Result[1]/Path", "/Result/Rows/Row[0]/FAHRZEUGAUFTRAG");
                        parameterContainer7.setParameter("/WurzelIn/StateLists/Result[1]/Unit", "");
                        base.Factory.CreateServiceDialog(this, "ZuendungEin", "51939083", _globalTabModuleISTA, 43679, parameterContainer7, parameterContainer9).Invoke("InitializeDialog", parameterContainer7, parameterContainer8, parameterContainer9);
                        diagnosticDeviceResult3 = (IDiagnosticDeviceResult)parameterContainer8.getParameter("/WurzelOut/DSCResult");
                        int num6 = 0;
                        object iSTAResultAsType5 = diagnosticDeviceResult3.getISTAResultAsType("/Result/Rows/$Count", typeof(int));
                        if (iSTAResultAsType5 != null)
                        {
                            num6 = (int)iSTAResultAsType5;
                        }
                        object iSTAResultAsType6 = diagnosticDeviceResult3.getISTAResultAsType("/Result/Status/JOB_STATUS", typeof(string));
                        if (iSTAResultAsType6 != null)
                        {
                            _ = (string)iSTAResultAsType6;
                        }
                        if (num6 > 0)
                        {
                            object iSTAResultAsType7 = diagnosticDeviceResult3.getISTAResultAsType("/Result/Rows/Row[0]/FAHRZEUGAUFTRAG", typeof(string));
                            if (iSTAResultAsType7 != null)
                            {
                                _ = (string)iSTAResultAsType7;
                            }
                        }
                        num4 = 1;
                    }
                    else
                    {
                        ConfigurationContainer configurationContainer4 = null;
                        configurationContainer4 = ConfigurationContainer.Deserialize("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<ConfigurationContainer xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" Name=\"Parametrization tree for EDIABAS\" Compression=\"Zip\" MajorVersion=\"1\" MinorVersion=\"0\">\r\n  <Header>\r\n    <Version Major=\"1\" Minor=\"2\" />\r\n    <Adapter Name=\"BMW-EDIABAS-Adapter\">\r\n      <ClassReference FullClassName=\"Siemens.SidisEnterprise.BaseSystem.DiagnosticDevices.Vehicle.Ediabas.Adapter.BMW.EdiabasAdapter\" Location=\"Siemens.SEP.Ediabas.Adapter.BMW\" />\r\n      <SubDeviceCollection />\r\n    </Adapter>\r\n  </Header>\r\n  <Body>\r\n    <Configuration Name=\"EDIABAS_SpExtract\">\r\n      <Run xsi:type=\"SingleChoice\" Name=\"Run\">\r\n        <Children>\r\n          <Node xsi:type=\"SingleChoice\" Name=\"Group\">\r\n            <Children>\r\n              <Node xsi:type=\"SingleChoice\" Name=\"UnknownGroup\" Comment=\"\">\r\n                <Children>\r\n                  <Node xsi:type=\"SingleChoice\" Name=\"VirtualVariantJob\">\r\n                    <Children>\r\n                      <Node xsi:type=\"Executable\" Name=\"STATUS_FAHRGESTELLNUMMER\" Comment=\"17 ASCII Byte Fahrgestell-Nummer aus BMSK&#xD;&#xA;KWP 2000: $21 ReadDataByLocalIdentifier&#xD;&#xA;LocalIdentifier $30&#xD;&#xA;Falls keine Antwort von BMSKP (weil BMSKP im Bootblock),&#xD;&#xA;wird auf die FGNR aus dem FA-Bereich ($22, $10, $10) zurueckgegriffen&#xD;&#xA;Modus : Default\">\r\n                        <Children>\r\n                          <Node xsi:type=\"All\" Name=\"Argument\">\r\n                            <Children>\r\n                              <Node xsi:type=\"Value\" Name=\"ECUGroupOrVariant\" Comment=\"\">\r\n                                <Literal>\r\n                                  <Text TranslationMode=\"All\">MRK24</Text>\r\n                                </Literal>\r\n                              </Node>\r\n                            </Children>\r\n                          </Node>\r\n                        </Children>\r\n                        <Result xsi:type=\"All\" Name=\"Result\">\r\n                          <Children>\r\n                            <Node xsi:type=\"MultipleChoice\" Name=\"Status\">\r\n                              <Children>\r\n                                <Node xsi:type=\"Value\" Name=\"JOB_STATUS\" Comment=\"OKAY, wenn fehlerfrei  table JobResult STATUS_TEXT\">\r\n                                  <Literal>\r\n                                    <Text TranslationMode=\"All\" />\r\n                                  </Literal>\r\n                                </Node>\r\n                              </Children>\r\n                            </Node>\r\n                            <Node xsi:type=\"Sequence\" Name=\"Rows\">\r\n                              <Children>\r\n                                <Node xsi:type=\"MultipleChoice\" Name=\"Row\">\r\n                                  <Children>\r\n                                    <Node xsi:type=\"Value\" Name=\"FGNUMMER\" Comment=\"ausgelesene Fahrgestellnummer\">\r\n                                      <Literal>\r\n                                        <Text TranslationMode=\"All\" />\r\n                                      </Literal>\r\n                                    </Node>\r\n                                  </Children>\r\n                                </Node>\r\n                              </Children>\r\n                            </Node>\r\n                          </Children>\r\n                        </Result>\r\n                      </Node>\r\n                    </Children>\r\n                  </Node>\r\n                </Children>\r\n              </Node>\r\n            </Children>\r\n          </Node>\r\n        </Children>\r\n      </Run>\r\n    </Configuration>\r\n  </Body>\r\n</ConfigurationContainer>");
                        IDiagnosticDeviceResult diagnosticDeviceResult4 = null;
                        ParameterContainer parameterContainer10 = new ParameterContainer();
                        ParameterContainer parameterContainer11 = new ParameterContainer();
                        ParameterContainer parameterContainer12 = new ParameterContainer();
                        parameterContainer10.setParameter("DSCConfig", null);
                        parameterContainer10.setParameter("Display", false);
                        parameterContainer10.setParameter("FehlerMeldung", true);
                        parameterContainer10.setParameter("IO_FrageText", null);
                        parameterContainer10.setParameter("/WurzelIn/FehlerMeldung", EcuErrorMessage);
                        parameterContainer10.setParameter("/WurzelIn/DSCConfig", configurationContainer4);
                        parameterContainer10.setParameter("/WurzelIn/StateLists/Result[0]/Path", "/Result/Status/JOB_STATUS");
                        parameterContainer10.setParameter("/WurzelIn/StateLists/Result[0]/Unit", "");
                        parameterContainer10.setParameter("/WurzelIn/StateLists/Result[1]/Path", "/Result/Rows/Row[0]/FGNUMMER");
                        parameterContainer10.setParameter("/WurzelIn/StateLists/Result[1]/Unit", "");
                        base.Factory.CreateServiceDialog(this, "ZuendungEin", "51939083", _globalTabModuleISTA, 43693, parameterContainer10, parameterContainer12).Invoke("InitializeDialog", parameterContainer10, parameterContainer11, parameterContainer12);
                        diagnosticDeviceResult4 = (IDiagnosticDeviceResult)parameterContainer11.getParameter("/WurzelOut/DSCResult");
                        int num7 = 0;
                        object iSTAResultAsType8 = diagnosticDeviceResult4.getISTAResultAsType("/Result/Rows/$Count", typeof(int));
                        if (iSTAResultAsType8 != null)
                        {
                            num7 = (int)iSTAResultAsType8;
                        }
                        object iSTAResultAsType9 = diagnosticDeviceResult4.getISTAResultAsType("/Result/Status/JOB_STATUS", typeof(string));
                        if (iSTAResultAsType9 != null)
                        {
                            _ = (string)iSTAResultAsType9;
                        }
                        if (num7 > 0)
                        {
                            object iSTAResultAsType10 = diagnosticDeviceResult4.getISTAResultAsType("/Result/Rows/Row[0]/FGNUMMER", typeof(string));
                            if (iSTAResultAsType10 != null)
                            {
                                _ = (string)iSTAResultAsType10;
                            }
                        }
                        num4 = 0;
                    }
                    ParameterContainer parameterContainer13 = new ParameterContainer();
                    ParameterContainer outParam = new ParameterContainer();
                    ParameterContainer inoutParam = new ParameterContainer();
                    parameterContainer13.setParameter("txtParam", i_ZuendungEinText);
                    parameterContainer13.setParameter("Quittierung", false);
                    parameterContainer13.setParameter("Display", true);
                    parameterContainer13.setParameter("Timeout", 1);
                    parameterContainer13.setParameter("Protocol", false);
                    __MessageServiceDlg.Invoke("InitializeDialog", parameterContainer13, outParam, inoutParam);
                    Sleep(500);
                    ConfigurationContainer configurationContainer5 = null;
                    configurationContainer5 = ConfigurationContainer.Deserialize("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<ConfigurationContainer xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" Name=\"Parametrization tree for EDIABAS\" Compression=\"Zip\" MajorVersion=\"1\" MinorVersion=\"0\">\r\n  <Header>\r\n    <Version Major=\"1\" Minor=\"2\" />\r\n    <Adapter Name=\"BMW-EDIABAS-Adapter\">\r\n      <ClassReference FullClassName=\"Siemens.SidisEnterprise.BaseSystem.DiagnosticDevices.Vehicle.Ediabas.Adapter.BMW.EdiabasAdapter\" Location=\"Siemens.SEP.Ediabas.Adapter.BMW\" />\r\n      <SubDeviceCollection />\r\n    </Adapter>\r\n  </Header>\r\n  <Body>\r\n    <Configuration Name=\"EDIABAS_SpExtract\">\r\n      <Run xsi:type=\"SingleChoice\" Name=\"Run\">\r\n        <Children>\r\n          <Node xsi:type=\"SingleChoice\" Name=\"Group\">\r\n            <Children>\r\n              <Node xsi:type=\"SingleChoice\" Name=\"UnknownGroup\" Comment=\"\">\r\n                <Children>\r\n                  <Node xsi:type=\"SingleChoice\" Name=\"VirtualVariantJob\">\r\n                    <Children>\r\n                      <Node xsi:type=\"Executable\" Name=\"GET_VOLTAGE\" Comment=\"KL30 oder KL 15 analog einlesen\">\r\n                        <Children>\r\n                          <Node xsi:type=\"All\" Name=\"Argument\">\r\n                            <Children>\r\n                              <Node xsi:type=\"Value\" Name=\"ECUGroupOrVariant\" Comment=\"\">\r\n                                <Literal>\r\n                                  <Text TranslationMode=\"All\">DIA_DOSE</Text>\r\n                                </Literal>\r\n                              </Node>\r\n                              <Node xsi:type=\"Value\" Name=\"ARG1\" Comment=\"\">\r\n                                <Literal>\r\n                                  <Text TranslationMode=\"All\">KL15</Text>\r\n                                </Literal>\r\n                              </Node>\r\n                            </Children>\r\n                          </Node>\r\n                        </Children>\r\n                        <Result xsi:type=\"All\" Name=\"Result\">\r\n                          <Children>\r\n                            <Node xsi:type=\"Sequence\" Name=\"Rows\">\r\n                              <Children>\r\n                                <Node xsi:type=\"MultipleChoice\" Name=\"Row\">\r\n                                  <Children>\r\n                                    <Node xsi:type=\"Value\" Name=\"SPANNUNG_V\" Comment=\"Spannung in Millivolt\">\r\n                                      <Literal>\r\n                                        <Short>0</Short>\r\n                                      </Literal>\r\n                                    </Node>\r\n                                  </Children>\r\n                                </Node>\r\n                              </Children>\r\n                            </Node>\r\n                          </Children>\r\n                        </Result>\r\n                      </Node>\r\n                    </Children>\r\n                  </Node>\r\n                </Children>\r\n              </Node>\r\n            </Children>\r\n          </Node>\r\n        </Children>\r\n      </Run>\r\n    </Configuration>\r\n  </Body>\r\n</ConfigurationContainer>");
                    IDiagnosticDeviceResult diagnosticDeviceResult5 = null;
                    ParameterContainer parameterContainer14 = new ParameterContainer();
                    ParameterContainer parameterContainer15 = new ParameterContainer();
                    ParameterContainer parameterContainer16 = new ParameterContainer();
                    parameterContainer14.setParameter("DSCConfig", null);
                    parameterContainer14.setParameter("Display", false);
                    parameterContainer14.setParameter("FehlerMeldung", true);
                    parameterContainer14.setParameter("IO_FrageText", null);
                    parameterContainer14.setParameter("/WurzelIn/FehlerMeldung", EcuErrorMessage);
                    parameterContainer14.setParameter("/WurzelIn/DSCConfig", configurationContainer5);
                    parameterContainer14.setParameter("/WurzelIn/StateLists/Result[0]/Path", "/Result/Rows/Row[0]/SPANNUNG_V");
                    parameterContainer14.setParameter("/WurzelIn/StateLists/Result[0]/Unit", "");
                    base.Factory.CreateServiceDialog(this, "ZuendungEin", "51939083", _globalTabModuleISTA, 43380, parameterContainer14, parameterContainer16).Invoke("InitializeDialog", parameterContainer14, parameterContainer15, parameterContainer16);
                    diagnosticDeviceResult5 = (IDiagnosticDeviceResult)parameterContainer15.getParameter("/WurzelOut/DSCResult");
                    int num8 = 0;
                    object iSTAResultAsType11 = diagnosticDeviceResult5.getISTAResultAsType("/Result/Rows/$Count", typeof(int));
                    if (iSTAResultAsType11 != null)
                    {
                        num8 = (int)iSTAResultAsType11;
                    }
                    if (num8 > 0)
                    {
                        object iSTAResultAsType12 = diagnosticDeviceResult5.getISTAResultAsType("/Result/Rows/Row[0]/SPANNUNG_V", typeof(short));
                        if (iSTAResultAsType12 != null)
                        {
                            Klemme15Spg = (short)iSTAResultAsType12;
                        }
                    }
                    if (Vehicle.VCI.VCIType == VCIDeviceType.SIM)
                    {
                        Klemme15Spg = 12000;
                    }
                    base._DoLoopHandling = false;
                }
                while (Klemme15Spg < 8000);
            }
            else
            {
                DocumentHandler(DocumentStatementAction.Add, __Document("61055504139"), 3);
                ParameterContainer parameterContainer17 = new ParameterContainer();
                ParameterContainer outParam2 = new ParameterContainer();
                ParameterContainer inoutParam2 = new ParameterContainer();
                parameterContainer17.setParameter("txtParam", i_ZuendungEinText);
                parameterContainer17.setParameter("Quittierung", true);
                parameterContainer17.setParameter("Display", true);
                parameterContainer17.setParameter("Protocol", true);
                __MessageServiceDlg.Invoke("InitializeDialog", parameterContainer17, outParam2, inoutParam2);
            }
            i_KL15spg = Klemme15Spg;
            Logger.WriteInformation("_ExitIndex is: {0}", num);
            Reset();
        }

        public virtual void ZuendungAus(bool i_automatic, bool i_PopUp, ITextLocator i_ZuendungAusText, string i_hilfsvariable, ref short i_KL15spg)
        {
            int num = 0;
            Logger.WriteInformation("ZuendungAuscalled");
            ZuendungText = i_ZuendungAusText;
            Klemme15Spg = 12345;
            ConfigurationContainer configurationContainer = null;
            configurationContainer = ConfigurationContainer.Deserialize("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<ConfigurationContainer xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" Name=\"Parametrization tree for EDIABAS\" Compression=\"Zip\" MajorVersion=\"1\" MinorVersion=\"0\">\r\n  <Header>\r\n    <Version Major=\"1\" Minor=\"2\" />\r\n    <Adapter Name=\"BMW-EDIABAS-Adapter\">\r\n      <ClassReference FullClassName=\"Siemens.SidisEnterprise.BaseSystem.DiagnosticDevices.Vehicle.Ediabas.Adapter.BMW.EdiabasAdapter\" Location=\"Siemens.SEP.Ediabas.Adapter.BMW\" />\r\n      <SubDeviceCollection />\r\n    </Adapter>\r\n  </Header>\r\n  <Body>\r\n    <Configuration Name=\"EDIABAS_SpExtract\">\r\n      <Run xsi:type=\"SingleChoice\" Name=\"Run\">\r\n        <Children>\r\n          <Node xsi:type=\"SingleChoice\" Name=\"Group\">\r\n            <Children>\r\n              <Node xsi:type=\"SingleChoice\" Name=\"UnknownGroup\" Comment=\"\">\r\n                <Children>\r\n                  <Node xsi:type=\"SingleChoice\" Name=\"VirtualVariantJob\">\r\n                    <Children>\r\n                      <Node xsi:type=\"Executable\" Name=\"GET_VOLTAGE\" Comment=\"KL30 oder KL 15 analog einlesen\">\r\n                        <Children>\r\n                          <Node xsi:type=\"All\" Name=\"Argument\">\r\n                            <Children>\r\n                              <Node xsi:type=\"Value\" Name=\"ECUGroupOrVariant\" Comment=\"\">\r\n                                <Literal>\r\n                                  <Text TranslationMode=\"All\">DIA_DOSE</Text>\r\n                                </Literal>\r\n                              </Node>\r\n                              <Node xsi:type=\"Value\" Name=\"ARG1\" Comment=\"\">\r\n                                <Literal>\r\n                                  <Text TranslationMode=\"All\">KL15</Text>\r\n                                </Literal>\r\n                              </Node>\r\n                            </Children>\r\n                          </Node>\r\n                        </Children>\r\n                        <Result xsi:type=\"All\" Name=\"Result\">\r\n                          <Children>\r\n                            <Node xsi:type=\"Sequence\" Name=\"Rows\">\r\n                              <Children>\r\n                                <Node xsi:type=\"MultipleChoice\" Name=\"Row\">\r\n                                  <Children>\r\n                                    <Node xsi:type=\"Value\" Name=\"SPANNUNG_V\" Comment=\"Spannung in Millivolt\">\r\n                                      <Literal>\r\n                                        <Short>0</Short>\r\n                                      </Literal>\r\n                                    </Node>\r\n                                  </Children>\r\n                                </Node>\r\n                              </Children>\r\n                            </Node>\r\n                          </Children>\r\n                        </Result>\r\n                      </Node>\r\n                    </Children>\r\n                  </Node>\r\n                </Children>\r\n              </Node>\r\n            </Children>\r\n          </Node>\r\n        </Children>\r\n      </Run>\r\n    </Configuration>\r\n  </Body>\r\n</ConfigurationContainer>");
            IDiagnosticDeviceResult diagnosticDeviceResult = null;
            ParameterContainer parameterContainer = new ParameterContainer();
            ParameterContainer parameterContainer2 = new ParameterContainer();
            ParameterContainer parameterContainer3 = new ParameterContainer();
            parameterContainer.setParameter("DSCConfig", null);
            parameterContainer.setParameter("Display", false);
            parameterContainer.setParameter("FehlerMeldung", true);
            parameterContainer.setParameter("IO_FrageText", null);
            parameterContainer.setParameter("/WurzelIn/FehlerMeldung", EcuErrorMessage);
            parameterContainer.setParameter("/WurzelIn/DSCConfig", configurationContainer);
            parameterContainer.setParameter("/WurzelIn/StateLists/Result[0]/Path", "/Result/Rows/Row[0]/SPANNUNG_V");
            parameterContainer.setParameter("/WurzelIn/StateLists/Result[0]/Unit", "");
            base.Factory.CreateServiceDialog(this, "ZuendungAus", "51939083", _globalTabModuleISTA, 43570, parameterContainer, parameterContainer3).Invoke("InitializeDialog", parameterContainer, parameterContainer2, parameterContainer3);
            diagnosticDeviceResult = (IDiagnosticDeviceResult)parameterContainer2.getParameter("/WurzelOut/DSCResult");
            int num2 = 0;
            object iSTAResultAsType = diagnosticDeviceResult.getISTAResultAsType("/Result/Rows/$Count", typeof(int));
            if (iSTAResultAsType != null)
            {
                num2 = (int)iSTAResultAsType;
            }
            if (num2 > 0)
            {
                object iSTAResultAsType2 = diagnosticDeviceResult.getISTAResultAsType("/Result/Rows/Row[0]/SPANNUNG_V", typeof(short));
                if (iSTAResultAsType2 != null)
                {
                    Klemme15Spg = (short)iSTAResultAsType2;
                }
                if (Vehicle.VCI.VCIType == VCIDeviceType.SIM)
                {
                    Klemme15Spg = 0;
                }
            }
            ZwischenstandZuendung = Klemme15Spg;
            int num3 = 0;
            if (i_PopUp)
            {
                if (i_automatic)
                {
                    while (Klemme15Spg >= 8000)
                    {
                        base._DoLoopHandling = true;
                        __MessagePopup(i_ZuendungAusText.TextContent);
                        Sleep(500);
                        ConfigurationContainer configurationContainer2 = null;
                        configurationContainer2 = ConfigurationContainer.Deserialize("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<ConfigurationContainer xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" Name=\"Parametrization tree for EDIABAS\" Compression=\"Zip\" MajorVersion=\"1\" MinorVersion=\"0\">\r\n  <Header>\r\n    <Version Major=\"1\" Minor=\"2\" />\r\n    <Adapter Name=\"BMW-EDIABAS-Adapter\">\r\n      <ClassReference FullClassName=\"Siemens.SidisEnterprise.BaseSystem.DiagnosticDevices.Vehicle.Ediabas.Adapter.BMW.EdiabasAdapter\" Location=\"Siemens.SEP.Ediabas.Adapter.BMW\" />\r\n      <SubDeviceCollection />\r\n    </Adapter>\r\n  </Header>\r\n  <Body>\r\n    <Configuration Name=\"EDIABAS_SpExtract\">\r\n      <Run xsi:type=\"SingleChoice\" Name=\"Run\">\r\n        <Children>\r\n          <Node xsi:type=\"SingleChoice\" Name=\"Group\">\r\n            <Children>\r\n              <Node xsi:type=\"SingleChoice\" Name=\"UnknownGroup\" Comment=\"\">\r\n                <Children>\r\n                  <Node xsi:type=\"SingleChoice\" Name=\"VirtualVariantJob\">\r\n                    <Children>\r\n                      <Node xsi:type=\"Executable\" Name=\"GET_VOLTAGE\" Comment=\"KL30 oder KL 15 analog einlesen\">\r\n                        <Children>\r\n                          <Node xsi:type=\"All\" Name=\"Argument\">\r\n                            <Children>\r\n                              <Node xsi:type=\"Value\" Name=\"ECUGroupOrVariant\" Comment=\"\">\r\n                                <Literal>\r\n                                  <Text TranslationMode=\"All\">DIA_DOSE</Text>\r\n                                </Literal>\r\n                              </Node>\r\n                              <Node xsi:type=\"Value\" Name=\"ARG1\" Comment=\"\">\r\n                                <Literal>\r\n                                  <Text TranslationMode=\"All\">KL15</Text>\r\n                                </Literal>\r\n                              </Node>\r\n                            </Children>\r\n                          </Node>\r\n                        </Children>\r\n                        <Result xsi:type=\"All\" Name=\"Result\">\r\n                          <Children>\r\n                            <Node xsi:type=\"Sequence\" Name=\"Rows\">\r\n                              <Children>\r\n                                <Node xsi:type=\"MultipleChoice\" Name=\"Row\">\r\n                                  <Children>\r\n                                    <Node xsi:type=\"Value\" Name=\"SPANNUNG_V\" Comment=\"Spannung in Millivolt\">\r\n                                      <Literal>\r\n                                        <Short>0</Short>\r\n                                      </Literal>\r\n                                    </Node>\r\n                                  </Children>\r\n                                </Node>\r\n                              </Children>\r\n                            </Node>\r\n                          </Children>\r\n                        </Result>\r\n                      </Node>\r\n                    </Children>\r\n                  </Node>\r\n                </Children>\r\n              </Node>\r\n            </Children>\r\n          </Node>\r\n        </Children>\r\n      </Run>\r\n    </Configuration>\r\n  </Body>\r\n</ConfigurationContainer>");
                        IDiagnosticDeviceResult diagnosticDeviceResult2 = null;
                        ParameterContainer parameterContainer4 = new ParameterContainer();
                        ParameterContainer parameterContainer5 = new ParameterContainer();
                        ParameterContainer parameterContainer6 = new ParameterContainer();
                        parameterContainer4.setParameter("DSCConfig", null);
                        parameterContainer4.setParameter("Display", false);
                        parameterContainer4.setParameter("FehlerMeldung", true);
                        parameterContainer4.setParameter("IO_FrageText", null);
                        parameterContainer4.setParameter("/WurzelIn/FehlerMeldung", EcuErrorMessage);
                        parameterContainer4.setParameter("/WurzelIn/DSCConfig", configurationContainer2);
                        parameterContainer4.setParameter("/WurzelIn/StateLists/Result[0]/Path", "/Result/Rows/Row[0]/SPANNUNG_V");
                        parameterContainer4.setParameter("/WurzelIn/StateLists/Result[0]/Unit", "");
                        base.Factory.CreateServiceDialog(this, "ZuendungAus", "51939083", _globalTabModuleISTA, 43588, parameterContainer4, parameterContainer6).Invoke("InitializeDialog", parameterContainer4, parameterContainer5, parameterContainer6);
                        diagnosticDeviceResult2 = (IDiagnosticDeviceResult)parameterContainer5.getParameter("/WurzelOut/DSCResult");
                        int num4 = 0;
                        object iSTAResultAsType3 = diagnosticDeviceResult2.getISTAResultAsType("/Result/Rows/$Count", typeof(int));
                        if (iSTAResultAsType3 != null)
                        {
                            num4 = (int)iSTAResultAsType3;
                        }
                        if (num4 > 0)
                        {
                            object iSTAResultAsType4 = diagnosticDeviceResult2.getISTAResultAsType("/Result/Rows/Row[0]/SPANNUNG_V", typeof(short));
                            if (iSTAResultAsType4 != null)
                            {
                                Klemme15Spg = (short)iSTAResultAsType4;
                            }
                        }
                        if (Vehicle.VCI.VCIType == VCIDeviceType.SIM)
                        {
                            Klemme15Spg = 0;
                        }
                        base._DoLoopHandling = false;
                    }
                }
                else
                {
                    __MessagePopup(i_ZuendungAusText.TextContent);
                }
            }
            else if (i_automatic)
            {
                do
                {
                    base._DoLoopHandling = true;
                    num3++;
                    if (num3 < 2)
                    {
                        DocumentHandler(DocumentStatementAction.Add, __Document("61057296779"), 3);
                    }
                    ParameterContainer parameterContainer7 = new ParameterContainer();
                    ParameterContainer outParam = new ParameterContainer();
                    ParameterContainer inoutParam = new ParameterContainer();
                    parameterContainer7.setParameter("txtParam", i_ZuendungAusText);
                    parameterContainer7.setParameter("Quittierung", false);
                    parameterContainer7.setParameter("Display", true);
                    parameterContainer7.setParameter("Timeout", 1);
                    parameterContainer7.setParameter("Protocol", false);
                    __MessageServiceDlg.Invoke("InitializeDialog", parameterContainer7, outParam, inoutParam);
                    Sleep(500);
                    ConfigurationContainer configurationContainer3 = null;
                    configurationContainer3 = ConfigurationContainer.Deserialize("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<ConfigurationContainer xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" Name=\"Parametrization tree for EDIABAS\" Compression=\"Zip\" MajorVersion=\"1\" MinorVersion=\"0\">\r\n  <Header>\r\n    <Version Major=\"1\" Minor=\"2\" />\r\n    <Adapter Name=\"BMW-EDIABAS-Adapter\">\r\n      <ClassReference FullClassName=\"Siemens.SidisEnterprise.BaseSystem.DiagnosticDevices.Vehicle.Ediabas.Adapter.BMW.EdiabasAdapter\" Location=\"Siemens.SEP.Ediabas.Adapter.BMW\" />\r\n      <SubDeviceCollection />\r\n    </Adapter>\r\n  </Header>\r\n  <Body>\r\n    <Configuration Name=\"EDIABAS_SpExtract\">\r\n      <Run xsi:type=\"SingleChoice\" Name=\"Run\">\r\n        <Children>\r\n          <Node xsi:type=\"SingleChoice\" Name=\"Group\">\r\n            <Children>\r\n              <Node xsi:type=\"SingleChoice\" Name=\"UnknownGroup\" Comment=\"\">\r\n                <Children>\r\n                  <Node xsi:type=\"SingleChoice\" Name=\"VirtualVariantJob\">\r\n                    <Children>\r\n                      <Node xsi:type=\"Executable\" Name=\"GET_VOLTAGE\" Comment=\"KL30 oder KL 15 analog einlesen\">\r\n                        <Children>\r\n                          <Node xsi:type=\"All\" Name=\"Argument\">\r\n                            <Children>\r\n                              <Node xsi:type=\"Value\" Name=\"ECUGroupOrVariant\" Comment=\"\">\r\n                                <Literal>\r\n                                  <Text TranslationMode=\"All\">DIA_DOSE</Text>\r\n                                </Literal>\r\n                              </Node>\r\n                              <Node xsi:type=\"Value\" Name=\"ARG1\" Comment=\"\">\r\n                                <Literal>\r\n                                  <Text TranslationMode=\"All\">KL15</Text>\r\n                                </Literal>\r\n                              </Node>\r\n                            </Children>\r\n                          </Node>\r\n                        </Children>\r\n                        <Result xsi:type=\"All\" Name=\"Result\">\r\n                          <Children>\r\n                            <Node xsi:type=\"Sequence\" Name=\"Rows\">\r\n                              <Children>\r\n                                <Node xsi:type=\"MultipleChoice\" Name=\"Row\">\r\n                                  <Children>\r\n                                    <Node xsi:type=\"Value\" Name=\"SPANNUNG_V\" Comment=\"Spannung in Millivolt\">\r\n                                      <Literal>\r\n                                        <Short>0</Short>\r\n                                      </Literal>\r\n                                    </Node>\r\n                                  </Children>\r\n                                </Node>\r\n                              </Children>\r\n                            </Node>\r\n                          </Children>\r\n                        </Result>\r\n                      </Node>\r\n                    </Children>\r\n                  </Node>\r\n                </Children>\r\n              </Node>\r\n            </Children>\r\n          </Node>\r\n        </Children>\r\n      </Run>\r\n    </Configuration>\r\n  </Body>\r\n</ConfigurationContainer>");
                    IDiagnosticDeviceResult diagnosticDeviceResult3 = null;
                    ParameterContainer parameterContainer8 = new ParameterContainer();
                    ParameterContainer parameterContainer9 = new ParameterContainer();
                    ParameterContainer parameterContainer10 = new ParameterContainer();
                    parameterContainer8.setParameter("DSCConfig", null);
                    parameterContainer8.setParameter("Display", false);
                    parameterContainer8.setParameter("FehlerMeldung", true);
                    parameterContainer8.setParameter("IO_FrageText", null);
                    parameterContainer8.setParameter("/WurzelIn/FehlerMeldung", EcuErrorMessage);
                    parameterContainer8.setParameter("/WurzelIn/DSCConfig", configurationContainer3);
                    parameterContainer8.setParameter("/WurzelIn/StateLists/Result[0]/Path", "/Result/Rows/Row[0]/SPANNUNG_V");
                    parameterContainer8.setParameter("/WurzelIn/StateLists/Result[0]/Unit", "");
                    base.Factory.CreateServiceDialog(this, "ZuendungAus", "51939083", _globalTabModuleISTA, 43605, parameterContainer8, parameterContainer10).Invoke("InitializeDialog", parameterContainer8, parameterContainer9, parameterContainer10);
                    diagnosticDeviceResult3 = (IDiagnosticDeviceResult)parameterContainer9.getParameter("/WurzelOut/DSCResult");
                    int num5 = 0;
                    object iSTAResultAsType5 = diagnosticDeviceResult3.getISTAResultAsType("/Result/Rows/$Count", typeof(int));
                    if (iSTAResultAsType5 != null)
                    {
                        num5 = (int)iSTAResultAsType5;
                    }
                    if (num5 > 0)
                    {
                        object iSTAResultAsType6 = diagnosticDeviceResult3.getISTAResultAsType("/Result/Rows/Row[0]/SPANNUNG_V", typeof(short));
                        if (iSTAResultAsType6 != null)
                        {
                            Klemme15Spg = (short)iSTAResultAsType6;
                        }
                    }
                    if (Vehicle.VCI.VCIType == VCIDeviceType.SIM)
                    {
                        Klemme15Spg = 0;
                    }
                    base._DoLoopHandling = false;
                }
                while (Klemme15Spg >= 8000);
            }
            else
            {
                DocumentHandler(DocumentStatementAction.Add, __Document("61057296779"), 3);
                ParameterContainer parameterContainer11 = new ParameterContainer();
                ParameterContainer outParam2 = new ParameterContainer();
                ParameterContainer inoutParam2 = new ParameterContainer();
                parameterContainer11.setParameter("txtParam", i_ZuendungAusText);
                parameterContainer11.setParameter("Quittierung", true);
                parameterContainer11.setParameter("Display", true);
                parameterContainer11.setParameter("Protocol", true);
                __MessageServiceDlg.Invoke("InitializeDialog", parameterContainer11, outParam2, inoutParam2);
            }
            i_KL15spg = Klemme15Spg;
            Logger.WriteInformation("_ExitIndex is: {0}", num);
            Reset();
        }

        public override void Invoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            throw new NotImplementedException();
        }
    }
}
