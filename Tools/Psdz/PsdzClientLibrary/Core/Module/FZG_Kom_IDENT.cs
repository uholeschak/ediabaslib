using BMW.Rheingold.CoreFramework.Contracts;
using BMW.Rheingold.Module.ISTA;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System;
using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Module;

namespace BMW.Rheingold.Module.ISTA
{
    internal class FZG_Kom_IDENT : ISTAModule
    {
        public int[] mig_doku_anweisung_slots;
        public int x;
        public bool EcuErrorMessage;
        public ITextLocator[] p_Steuergeraet;
        public string[] p_SG_gruppe;
        public string[] p_Verdacht_Versorgung;
        public string[] p_Status_Ident;
        public string[] p_Sgbd;
        public string p_Status_Ident_ges;
        public int JA;
        public int NEIN;
        public bool QUIT;
        public int RESULT;
        public int SELEKT;
        public string ENTER;
        public int Anzahl_SG;
        public IServiceDialog MessageServiceDlg1;
        public FZG_Kom_IDENT(ParameterContainer InParameter)
        {
            if (InParameter != null)
            {
                _globalModuleInParameter = InParameter;
            }

            __handleInParameter();
            mig_doku_anweisung_slots = new int[4];
            x = 0;
            EcuErrorMessage = false;
            p_Steuergeraet = new ITextLocator[20];
            p_SG_gruppe = new string[20];
            p_Verdacht_Versorgung = new string[20];
            p_Status_Ident = new string[20];
            p_Sgbd = new string[20];
            JA = 1;
            NEIN = 2;
            QUIT = false;
            RESULT = 0;
            SELEKT = 0;
            ENTER = "";
            Anzahl_SG = 0;
            ParameterContainer parameterContainer = new ParameterContainer();
            new ParameterContainer();
            ParameterContainer inoutParameters = new ParameterContainer();
            parameterContainer.setParameter("txtParam", new TextLocator());
            parameterContainer.setParameter("Quittierung", true);
            parameterContainer.setParameter("Display", true);
            MessageServiceDlg1 = Factory.CreateServiceDialog(this, "global", "51915403", _globalTabModuleISTA, 3145, parameterContainer, inoutParameters);
        }

        public virtual void Prepare()
        {
        }

        public virtual void Reset()
        {
        }

        public virtual void SG_20(string[] SG_gruppe, ITextLocator[] Steuergeraet, string[] Verdacht_Versorgung, ref string[] Status_Ident, ref string[] Sgbd, ref string Status_Ident_ges)
        {
            int num = 0;
            Logger.WriteInformation("SG_20called");
            p_Steuergeraet = Steuergeraet;
            p_SG_gruppe = SG_gruppe;
            p_Verdacht_Versorgung = Verdacht_Versorgung;
            bool flag = false;
            switch (Verdacht_Versorgung[0])
            {
                case "-":
                    flag = true;
                    break;
                case "-Versorgung":
                    flag = true;
                    break;
                case "-Weckleitung":
                    flag = true;
                    break;
                case "-KL30_geschaltet":
                    flag = true;
                    break;
                case "-Weckleitung_KL30_geschaltet":
                    flag = true;
                    break;
                case "-KL15N_geschaltet":
                    flag = true;
                    break;
                default:
                    flag = false;
                    break;
            }

            if (flag)
            {
                Versorgungspruefung();
            }
            else
            {
                _DoLoopHandling = true;
                for (int i = 0; i < 20 && SG_gruppe[i] != null; i++)
                {
                    Anzahl_SG++;
                }

                _DoLoopHandling = false;
                Identifikationspruefung();
            }

            Status_Ident = p_Status_Ident;
            Sgbd = p_Sgbd;
            Status_Ident_ges = p_Status_Ident_ges;
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void Identifikationspruefung()
        {
            int num = 0;
            Logger.WriteInformation("Identifikationspruefungcalled");
            int num2 = 0;
            string text = "";
            string text2 = "";
            string text3 = "";
            int num3 = 0;
            string[] array = new string[20];
            _DoLoopHandling = true;
            for (int i = 0; i < Anzahl_SG; i++)
            {
                ParameterContainer parameterContainer = new ParameterContainer();
                ParameterContainer outParam = new ParameterContainer();
                ParameterContainer parameterContainer2 = new ParameterContainer();
                parameterContainer.setParameter("txtParam", __Text("71495173387", new __TextParameter[1] { new __TextParameter("parameterSG", p_Steuergeraet[i]) }));
                parameterContainer.setParameter("Quittierung", false);
                parameterContainer.setParameter("TIMEOUT", 700);
                parameterContainer.setParameter("Protocol", true);
                parameterContainer.setParameter("Display", true);
                Factory.CreateServiceDialog(this, "Identifikationspruefung", "51915403", _globalTabModuleISTA, 1318, parameterContainer, parameterContainer2).Invoke("InitializeDialog", parameterContainer, outParam, parameterContainer2);
                text = " ";
                ConfigurationContainer configurationContainer = null;
                configurationContainer = ConfigurationContainer.Deserialize("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<ConfigurationContainer xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" Name=\"Parametrization tree for EDIABAS\" Compression=\"Zip\" MajorVersion=\"1\" MinorVersion=\"0\">\r\n  <Header>\r\n    <Version Major=\"1\" Minor=\"2\" />\r\n    <Adapter Name=\"BMW-EDIABAS-Adapter\">\r\n      <ClassReference FullClassName=\"Siemens.SidisEnterprise.BaseSystem.DiagnosticDevices.Vehicle.Ediabas.Adapter.BMW.EdiabasAdapter\" Location=\"Siemens.SEP.Ediabas.Adapter.BMW\" />\r\n      <SubDeviceCollection />\r\n    </Adapter>\r\n  </Header>\r\n  <Body>\r\n    <Configuration Name=\"EDIABAS_SpExtract\">\r\n      <Run xsi:type=\"SingleChoice\" Name=\"Run\">\r\n        <Children>\r\n          <Node xsi:type=\"SingleChoice\" Name=\"Group\">\r\n            <Children>\r\n              <Node xsi:type=\"SingleChoice\" Name=\"D_MOTOR\" Comment=\"\">\r\n                <Children>\r\n                  <Node xsi:type=\"SingleChoice\" Name=\"VirtualVariantJob\">\r\n                    <Children>\r\n                      <Node xsi:type=\"Executable\" Name=\"IDENT\" Comment=\"¦|ME9K_NG4|=&gt;identdaten fuer dme auslesen¦|N73_R0|N73TU_R0|N73H_R0|N62_TUE2|N62_TUE|MV1722|MS_S65_3|MS_S65_2|MS_S65|MSV80|MSV70|MSS70|MSS60|MSD85Y|MSD80N43|MSD80|MS450DS0|MEVD176K|MEV9N46L|MEV9N46|MEV17_2N|MEV17_2|MED17_2N|MED17_2|ME9N62_2|ME9N62|ME9N45|ME9E65_6|ME17N45|DDE50K47|D73N57D0|D73N57C0|D73N57B0|D73N47A0|D72N47B0|D71N47D0|D71N47C0|D71N47B0|D71N47A0|D70N47B0|D70N47A0|D63MM670|D62M57B0|D62M57A0|D60TMCA|D60PSA0|D60M57D0|D60M57A0|D60M47A0|D51MM670|D50M57E1|D50M57E0|D50M57D0|D50M57C0|D50M57B1|D50M57A1|D50M57A0|D50M47B1|D50M47A|MEV17N46|MEVD17KW|MSD85|MVD1722|MEVD174K|EDME82|=&gt;identdaten&#xD;&#xA;kwp2000: $1a readecuidentification&#xD;&#xA;modus : default¦\">\r\n                        <Children>\r\n                          <Node xsi:type=\"All\" Name=\"Argument\">\r\n                            <Children>\r\n                              <Node xsi:type=\"Value\" Name=\"ECUGroupOrVariant\" Comment=\"\">\r\n                                <Literal>\r\n                                  <Text TranslationMode=\"All\" />\r\n                                </Literal>\r\n                              </Node>\r\n                            </Children>\r\n                          </Node>\r\n                        </Children>\r\n                        <Result xsi:type=\"All\" Name=\"Result\">\r\n                          <Children>\r\n                            <Node xsi:type=\"MultipleChoice\" Name=\"Status\">\r\n                              <Children>\r\n                                <Node xsi:type=\"Value\" Name=\"VARIANTE\" Comment=\"\">\r\n                                  <Literal>\r\n                                    <Text TranslationMode=\"All\" />\r\n                                  </Literal>\r\n                                </Node>\r\n                                <Node xsi:type=\"Value\" Name=\"JOB_STATUS\" Comment=\"okay, wenn fehlerfrei  table jobresult status_text\">\r\n                                  <Literal>\r\n                                    <Text TranslationMode=\"All\" />\r\n                                  </Literal>\r\n                                </Node>\r\n                              </Children>\r\n                            </Node>\r\n                          </Children>\r\n                        </Result>\r\n                      </Node>\r\n                    </Children>\r\n                  </Node>\r\n                </Children>\r\n              </Node>\r\n            </Children>\r\n          </Node>\r\n        </Children>\r\n      </Run>\r\n    </Configuration>\r\n  </Body>\r\n</ConfigurationContainer>");
                configurationContainer.AddRunOverride("/Run/Group/D_MOTOR/VirtualVariantJob/IDENT/Argument/ECUGroupOrVariant", p_SG_gruppe[i]);
                ParameterContainer parameterContainer3 = new ParameterContainer();
                ParameterContainer parameterContainer4 = new ParameterContainer();
                ParameterContainer parameterContainer5 = new ParameterContainer();
                parameterContainer3.setParameter("DSCConfig", null);
                parameterContainer3.setParameter("Display", false);
                parameterContainer3.setParameter("FehlerMeldung", true);
                parameterContainer3.setParameter("IO_FrageText", null);
                parameterContainer3.setParameter("/WurzelIn/FehlerMeldung", EcuErrorMessage);
                parameterContainer3.setParameter("/WurzelIn/DSCConfig", configurationContainer);
                parameterContainer3.setParameter("/WurzelIn/StateLists/Result[0]/Path", "/Result/Status/VARIANTE");
                parameterContainer3.setParameter("/WurzelIn/StateLists/Result[0]/Unit", "");
                parameterContainer3.setParameter("/WurzelIn/StateLists/Result[1]/Path", "/Result/Status/JOB_STATUS");
                parameterContainer3.setParameter("/WurzelIn/StateLists/Result[1]/Unit", "");
                Factory.CreateServiceDialog(this, "Identifikationspruefung", "51939083", _globalTabModuleISTA, 57734, parameterContainer3, parameterContainer5).Invoke("InitializeDialog", parameterContainer3, parameterContainer4, parameterContainer5);
                IDiagnosticDeviceResult diagnosticDeviceResult = (IDiagnosticDeviceResult)parameterContainer4.getParameter("/WurzelOut/DSCResult");
                int num4 = 0;
                object iSTAResultAsType = diagnosticDeviceResult.getISTAResultAsType("/Result/Rows/$Count", typeof(int));
                if (iSTAResultAsType != null)
                {
                    num4 = (int)iSTAResultAsType;
                }

                object iSTAResultAsType2 = diagnosticDeviceResult.getISTAResultAsType("/Result/Status/VARIANTE", typeof(string));
                if (iSTAResultAsType2 != null)
                {
                    text2 = (string)iSTAResultAsType2;
                }

                object iSTAResultAsType3 = diagnosticDeviceResult.getISTAResultAsType("/Result/Status/JOB_STATUS", typeof(string));
                if (iSTAResultAsType3 != null)
                {
                    text = (string)iSTAResultAsType3;
                }

                _ = 0;
                p_Status_Ident[i] = text;
                if (text == "OKAY")
                {
                    p_Sgbd[i] = text2;
                    continue;
                }

                string text4 = (string.IsNullOrEmpty(text3) ? "" : ", ");
                text3 = text3 + text4 + Convert.ToString(p_Steuergeraet[i].TextContent.PlainText);
                array[num2] = p_Verdacht_Versorgung[i];
                num3++;
                num2++;
            }

            _DoLoopHandling = false;
            if (num3 == 0)
            {
                p_Status_Ident_ges = "OKAY";
            }
            else
            {
                p_Status_Ident_ges = "NOK";
                if (num3 == 1)
                {
                    __SetSuspiciousItem(__DiagnosticObject(array[0]));
                    __handleOutParameter();
                    ParameterContainer parameterContainer6 = new ParameterContainer();
                    ParameterContainer outParam2 = new ParameterContainer();
                    ParameterContainer parameterContainer7 = new ParameterContainer();
                    parameterContainer6.setParameter("txtParam", __Text("71495363851", new __TextParameter[1] { new __TextParameter("Meld_SG_v[0]", text3) }));
                    parameterContainer6.setParameter("WertFeld", null);
                    parameterContainer6.setParameter("Quittierung", true);
                    parameterContainer6.setParameter("TIMEOUT", 0);
                    parameterContainer6.setParameter("Protocol", true);
                    parameterContainer6.setParameter("Display", true);
                    Factory.CreateServiceDialog(this, "Identifikationspruefung", "51915403", _globalTabModuleISTA, 313, parameterContainer6, parameterContainer7).Invoke("InitializeDialog", parameterContainer6, outParam2, parameterContainer7);
                }
                else
                {
                    _DoLoopHandling = true;
                    for (int j = 0; j < num3; j++)
                    {
                        __SetSuspiciousItem(__DiagnosticObject(array[j]));
                        __handleOutParameter();
                    }

                    _DoLoopHandling = false;
                    ParameterContainer parameterContainer8 = new ParameterContainer();
                    ParameterContainer outParam3 = new ParameterContainer();
                    ParameterContainer parameterContainer9 = new ParameterContainer();
                    parameterContainer8.setParameter("txtParam", __Text("71495371531", new __TextParameter[1] { new __TextParameter("Meld_SG_sum_v", text3) }));
                    parameterContainer8.setParameter("WertFeld", null);
                    parameterContainer8.setParameter("Quittierung", true);
                    parameterContainer8.setParameter("TIMEOUT", 0);
                    parameterContainer8.setParameter("Protocol", true);
                    parameterContainer8.setParameter("Display", true);
                    Factory.CreateServiceDialog(this, "Identifikationspruefung", "51915403", _globalTabModuleISTA, 423, parameterContainer8, parameterContainer9).Invoke("InitializeDialog", parameterContainer8, outParam3, parameterContainer9);
                }
            }

            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void Versorgungspruefung()
        {
            int num = 0;
            Logger.WriteInformation("Versorgungspruefungcalled");
            string text = "";
            string text2 = "";
            string text3 = "-";
            ITextLocator value = null;
            ITextLocator textLocator = null;
            int num2 = 0;
            text = " ";
            ParameterContainer parameterContainer = new ParameterContainer();
            ParameterContainer outParam = new ParameterContainer();
            ParameterContainer parameterContainer2 = new ParameterContainer();
            parameterContainer.setParameter("txtParam", __Text("71495173387", new __TextParameter[1] { new __TextParameter("parameterSG", p_Steuergeraet[0]) }));
            parameterContainer.setParameter("Quittierung", false);
            parameterContainer.setParameter("TIMEOUT", 700);
            parameterContainer.setParameter("Protocol", true);
            parameterContainer.setParameter("Display", true);
            Factory.CreateServiceDialog(this, "Versorgungspruefung", "51915403", _globalTabModuleISTA, 57894, parameterContainer, parameterContainer2).Invoke("InitializeDialog", parameterContainer, outParam, parameterContainer2);
            ConfigurationContainer configurationContainer = null;
            configurationContainer = ConfigurationContainer.Deserialize("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<ConfigurationContainer xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" Name=\"Parametrization tree for EDIABAS\" Compression=\"Zip\" MajorVersion=\"1\" MinorVersion=\"0\">\r\n  <Header>\r\n    <Version Major=\"1\" Minor=\"2\" />\r\n    <Adapter Name=\"BMW-EDIABAS-Adapter\">\r\n      <ClassReference FullClassName=\"Siemens.SidisEnterprise.BaseSystem.DiagnosticDevices.Vehicle.Ediabas.Adapter.BMW.EdiabasAdapter\" Location=\"Siemens.SEP.Ediabas.Adapter.BMW\" />\r\n      <SubDeviceCollection />\r\n    </Adapter>\r\n  </Header>\r\n  <Body>\r\n    <Configuration Name=\"EDIABAS_SpExtract\">\r\n      <Run xsi:type=\"SingleChoice\" Name=\"Run\">\r\n        <Children>\r\n          <Node xsi:type=\"SingleChoice\" Name=\"Group\">\r\n            <Children>\r\n              <Node xsi:type=\"SingleChoice\" Name=\"D_MOTOR\" Comment=\"\">\r\n                <Children>\r\n                  <Node xsi:type=\"SingleChoice\" Name=\"VirtualVariantJob\">\r\n                    <Children>\r\n                      <Node xsi:type=\"Executable\" Name=\"IDENT\" Comment=\"¦|ME9K_NG4|=&gt;identdaten fuer dme auslesen¦|N73_R0|N73TU_R0|N73H_R0|N62_TUE2|N62_TUE|MV1722|MS_S65_3|MS_S65_2|MS_S65|MSV80|MSV70|MSS70|MSS60|MSD85Y|MSD80N43|MSD80|MS450DS0|MEVD176K|MEV9N46L|MEV9N46|MEV17_2N|MEV17_2|MED17_2N|MED17_2|ME9N62_2|ME9N62|ME9N45|ME9E65_6|ME17N45|DDE50K47|D73N57D0|D73N57C0|D73N57B0|D73N47A0|D72N47B0|D71N47D0|D71N47C0|D71N47B0|D71N47A0|D70N47B0|D70N47A0|D63MM670|D62M57B0|D62M57A0|D60TMCA|D60PSA0|D60M57D0|D60M57A0|D60M47A0|D51MM670|D50M57E1|D50M57E0|D50M57D0|D50M57C0|D50M57B1|D50M57A1|D50M57A0|D50M47B1|D50M47A|MEV17N46|MEVD17KW|MSD85|MVD1722|MEVD174K|EDME82|=&gt;identdaten&#xD;&#xA;kwp2000: $1a readecuidentification&#xD;&#xA;modus : default¦\">\r\n                        <Children>\r\n                          <Node xsi:type=\"All\" Name=\"Argument\">\r\n                            <Children>\r\n                              <Node xsi:type=\"Value\" Name=\"ECUGroupOrVariant\" Comment=\"\">\r\n                                <Literal>\r\n                                  <Text TranslationMode=\"All\" />\r\n                                </Literal>\r\n                              </Node>\r\n                            </Children>\r\n                          </Node>\r\n                        </Children>\r\n                        <Result xsi:type=\"All\" Name=\"Result\">\r\n                          <Children>\r\n                            <Node xsi:type=\"MultipleChoice\" Name=\"Status\">\r\n                              <Children>\r\n                                <Node xsi:type=\"Value\" Name=\"VARIANTE\" Comment=\"\">\r\n                                  <Literal>\r\n                                    <Text TranslationMode=\"All\" />\r\n                                  </Literal>\r\n                                </Node>\r\n                                <Node xsi:type=\"Value\" Name=\"JOB_STATUS\" Comment=\"okay, wenn fehlerfrei  table jobresult status_text\">\r\n                                  <Literal>\r\n                                    <Text TranslationMode=\"All\" />\r\n                                  </Literal>\r\n                                </Node>\r\n                              </Children>\r\n                            </Node>\r\n                          </Children>\r\n                        </Result>\r\n                      </Node>\r\n                    </Children>\r\n                  </Node>\r\n                </Children>\r\n              </Node>\r\n            </Children>\r\n          </Node>\r\n        </Children>\r\n      </Run>\r\n    </Configuration>\r\n  </Body>\r\n</ConfigurationContainer>");
            configurationContainer.AddRunOverride("/Run/Group/D_MOTOR/VirtualVariantJob/IDENT/Argument/ECUGroupOrVariant", p_SG_gruppe[0]);
            ParameterContainer parameterContainer3 = new ParameterContainer();
            ParameterContainer parameterContainer4 = new ParameterContainer();
            ParameterContainer parameterContainer5 = new ParameterContainer();
            parameterContainer3.setParameter("DSCConfig", null);
            parameterContainer3.setParameter("Display", false);
            parameterContainer3.setParameter("FehlerMeldung", true);
            parameterContainer3.setParameter("IO_FrageText", null);
            parameterContainer3.setParameter("/WurzelIn/FehlerMeldung", EcuErrorMessage);
            parameterContainer3.setParameter("/WurzelIn/DSCConfig", configurationContainer);
            parameterContainer3.setParameter("/WurzelIn/StateLists/Result[0]/Path", "/Result/Status/VARIANTE");
            parameterContainer3.setParameter("/WurzelIn/StateLists/Result[0]/Unit", "");
            parameterContainer3.setParameter("/WurzelIn/StateLists/Result[1]/Path", "/Result/Status/JOB_STATUS");
            parameterContainer3.setParameter("/WurzelIn/StateLists/Result[1]/Unit", "");
            Factory.CreateServiceDialog(this, "Versorgungspruefung", "51939083", _globalTabModuleISTA, 57930, parameterContainer3, parameterContainer5).Invoke("InitializeDialog", parameterContainer3, parameterContainer4, parameterContainer5);
            IDiagnosticDeviceResult diagnosticDeviceResult = (IDiagnosticDeviceResult)parameterContainer4.getParameter("/WurzelOut/DSCResult");
            int num3 = 0;
            object iSTAResultAsType = diagnosticDeviceResult.getISTAResultAsType("/Result/Rows/$Count", typeof(int));
            if (iSTAResultAsType != null)
            {
                num3 = (int)iSTAResultAsType;
            }

            object iSTAResultAsType2 = diagnosticDeviceResult.getISTAResultAsType("/Result/Status/VARIANTE", typeof(string));
            if (iSTAResultAsType2 != null)
            {
                text2 = (string)iSTAResultAsType2;
            }

            object iSTAResultAsType3 = diagnosticDeviceResult.getISTAResultAsType("/Result/Status/JOB_STATUS", typeof(string));
            if (iSTAResultAsType3 != null)
            {
                text = (string)iSTAResultAsType3;
            }

            _ = 0;
            if (text == "OKAY")
            {
                p_Status_Ident[0] = text;
                p_Sgbd[0] = text2;
                ParameterContainer parameterContainer6 = new ParameterContainer();
                ParameterContainer outParam2 = new ParameterContainer();
                ParameterContainer parameterContainer7 = new ParameterContainer();
                parameterContainer6.setParameter("txtParam", __Text("71495181067"));
                parameterContainer6.setParameter("WertFeld", null);
                parameterContainer6.setParameter("Quittierung", true);
                parameterContainer6.setParameter("TIMEOUT", 0);
                parameterContainer6.setParameter("Protocol", true);
                parameterContainer6.setParameter("Display", true);
                Factory.CreateServiceDialog(this, "Versorgungspruefung", "51915403", _globalTabModuleISTA, 116, parameterContainer6, parameterContainer7).Invoke("InitializeDialog", parameterContainer6, outParam2, parameterContainer7);
                p_Status_Ident_ges = "OKAY";
            }
            else
            {
                p_Status_Ident[0] = text;
                switch (p_Verdacht_Versorgung[0])
                {
                    case "-":
                    {
                        ParameterContainer parameterContainer8 = new ParameterContainer();
                        ParameterContainer outParam3 = new ParameterContainer();
                        ParameterContainer parameterContainer9 = new ParameterContainer();
                        parameterContainer8.setParameter("txtParam", __Text("71495219083", new __TextParameter[1] { new __TextParameter("parameter1", p_Steuergeraet[0]) }));
                        parameterContainer8.setParameter("WertFeld", null);
                        parameterContainer8.setParameter("Quittierung", true);
                        parameterContainer8.setParameter("TIMEOUT", 0);
                        parameterContainer8.setParameter("Protocol", true);
                        parameterContainer8.setParameter("Display", true);
                        Factory.CreateServiceDialog(this, "Versorgungspruefung", "51915403", _globalTabModuleISTA, 56295, parameterContainer8, parameterContainer9).Invoke("InitializeDialog", parameterContainer8, outParam3, parameterContainer9);
                        break;
                    }

                    case "-Versorgung":
                        DocumentHandler(DocumentStatementAction.Add, __Document("48056132235"), 3);
                        value = __Text("71495226763");
                        break;
                    case "-Weckleitung":
                        DocumentHandler(DocumentStatementAction.Add, __Document("48056132235"), 3);
                        value = __Text("71495234315");
                        break;
                    case "-KL30_geschaltet":
                        DocumentHandler(DocumentStatementAction.Add, __Document("48056132235"), 3);
                        value = __Text("71495241867");
                        break;
                    case "-KL15N_geschaltet":
                        DocumentHandler(DocumentStatementAction.Add, __Document("48056132235"), 3);
                        value = __Text("71495249419");
                        break;
                    case "-Weckleitung_KL30_geschaltet":
                        DocumentHandler(DocumentStatementAction.Add, __Document("48056132235"), 3);
                        value = __Text("71495256971");
                        break;
                }

                if (p_Verdacht_Versorgung[0] == "-")
                {
                    num2 = 0;
                }
                else
                {
                    ParameterContainer parameterContainer10 = new ParameterContainer();
                    ParameterContainer parameterContainer11 = new ParameterContainer();
                    ParameterContainer parameterContainer12 = new ParameterContainer();
                    parameterContainer10.setParameter("priorText", __Text("71495264523", new __TextParameter[2] { new __TextParameter("parameter1", p_Steuergeraet[0]), new __TextParameter("parameter2", value) }));
                    parameterContainer10.setParameter("pastText", null);
                    parameterContainer10.setParameter("ButtonCount", 2);
                    parameterContainer10.setParameter("ButtonLabel1", __Text("71495196427"));
                    parameterContainer10.setParameter("ButtonLabel2", __Text("71495196427"));
                    parameterContainer10.setParameter("ButtonText1", __Text("71495203979"));
                    parameterContainer10.setParameter("ButtonText2", __Text("71495211531"));
                    parameterContainer10.setParameter("Display", true);
                    Factory.CreateServiceDialog(this, "Versorgungspruefung", "51911691", _globalTabModuleISTA, 53824, parameterContainer10, parameterContainer12).Invoke("InitializeDialog2", parameterContainer10, parameterContainer11, parameterContainer12);
                    if (parameterContainer11.getParameter("Result") != null)
                    {
                        RESULT = (int)parameterContainer11.getParameter("Result");
                    }

                    num2 = ((RESULT == 1) ? 1 : 0);
                }

                while (num2 == 1)
                {
                    _DoLoopHandling = true;
                    QUIT = false;
                    Sleep(100);
                    while (!QUIT)
                    {
                        _DoLoopHandling = true;
                        text = "NOK";
                        ConfigurationContainer configurationContainer2 = null;
                        configurationContainer2 = ConfigurationContainer.Deserialize("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<ConfigurationContainer xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" Name=\"Parametrization tree for EDIABAS\" Compression=\"Zip\" MajorVersion=\"1\" MinorVersion=\"0\">\r\n  <Header>\r\n    <Version Major=\"1\" Minor=\"2\" />\r\n    <Adapter Name=\"BMW-EDIABAS-Adapter\">\r\n      <ClassReference FullClassName=\"Siemens.SidisEnterprise.BaseSystem.DiagnosticDevices.Vehicle.Ediabas.Adapter.BMW.EdiabasAdapter\" Location=\"Siemens.SEP.Ediabas.Adapter.BMW\" />\r\n      <SubDeviceCollection />\r\n    </Adapter>\r\n  </Header>\r\n  <Body>\r\n    <Configuration Name=\"EDIABAS_SpExtract\">\r\n      <Run xsi:type=\"SingleChoice\" Name=\"Run\">\r\n        <Children>\r\n          <Node xsi:type=\"SingleChoice\" Name=\"Group\">\r\n            <Children>\r\n              <Node xsi:type=\"SingleChoice\" Name=\"D_MOTOR\" Comment=\"\">\r\n                <Children>\r\n                  <Node xsi:type=\"SingleChoice\" Name=\"VirtualVariantJob\">\r\n                    <Children>\r\n                      <Node xsi:type=\"Executable\" Name=\"IDENT\" Comment=\"¦|ME9K_NG4|=&gt;identdaten fuer dme auslesen¦|N73_R0|N73TU_R0|N73H_R0|N62_TUE2|N62_TUE|MV1722|MS_S65_3|MS_S65_2|MS_S65|MSV80|MSV70|MSS70|MSS60|MSD85Y|MSD80N43|MSD80|MS450DS0|MEVD176K|MEV9N46L|MEV9N46|MEV17_2N|MEV17_2|MED17_2N|MED17_2|ME9N62_2|ME9N62|ME9N45|ME9E65_6|ME17N45|DDE50K47|D73N57D0|D73N57C0|D73N57B0|D73N47A0|D72N47B0|D71N47D0|D71N47C0|D71N47B0|D71N47A0|D70N47B0|D70N47A0|D63MM670|D62M57B0|D62M57A0|D60TMCA|D60PSA0|D60M57D0|D60M57A0|D60M47A0|D51MM670|D50M57E1|D50M57E0|D50M57D0|D50M57C0|D50M57B1|D50M57A1|D50M57A0|D50M47B1|D50M47A|MEV17N46|MEVD17KW|MSD85|MVD1722|MEVD174K|EDME82|=&gt;identdaten&#xD;&#xA;kwp2000: $1a readecuidentification&#xD;&#xA;modus : default¦\">\r\n                        <Children>\r\n                          <Node xsi:type=\"All\" Name=\"Argument\">\r\n                            <Children>\r\n                              <Node xsi:type=\"Value\" Name=\"ECUGroupOrVariant\" Comment=\"\">\r\n                                <Literal>\r\n                                  <Text TranslationMode=\"All\" />\r\n                                </Literal>\r\n                              </Node>\r\n                            </Children>\r\n                          </Node>\r\n                        </Children>\r\n                        <Result xsi:type=\"All\" Name=\"Result\">\r\n                          <Children>\r\n                            <Node xsi:type=\"MultipleChoice\" Name=\"Status\">\r\n                              <Children>\r\n                                <Node xsi:type=\"Value\" Name=\"VARIANTE\" Comment=\"\">\r\n                                  <Literal>\r\n                                    <Text TranslationMode=\"All\" />\r\n                                  </Literal>\r\n                                </Node>\r\n                                <Node xsi:type=\"Value\" Name=\"JOB_STATUS\" Comment=\"okay, wenn fehlerfrei  table jobresult status_text\">\r\n                                  <Literal>\r\n                                    <Text TranslationMode=\"All\" />\r\n                                  </Literal>\r\n                                </Node>\r\n                              </Children>\r\n                            </Node>\r\n                          </Children>\r\n                        </Result>\r\n                      </Node>\r\n                    </Children>\r\n                  </Node>\r\n                </Children>\r\n              </Node>\r\n            </Children>\r\n          </Node>\r\n        </Children>\r\n      </Run>\r\n    </Configuration>\r\n  </Body>\r\n</ConfigurationContainer>");
                        configurationContainer2.AddRunOverride("/Run/Group/D_MOTOR/VirtualVariantJob/IDENT/Argument/ECUGroupOrVariant", p_SG_gruppe[0]);
                        ParameterContainer parameterContainer13 = new ParameterContainer();
                        ParameterContainer parameterContainer14 = new ParameterContainer();
                        ParameterContainer parameterContainer15 = new ParameterContainer();
                        parameterContainer13.setParameter("DSCConfig", null);
                        parameterContainer13.setParameter("Display", false);
                        parameterContainer13.setParameter("FehlerMeldung", true);
                        parameterContainer13.setParameter("IO_FrageText", null);
                        parameterContainer13.setParameter("/WurzelIn/FehlerMeldung", EcuErrorMessage);
                        parameterContainer13.setParameter("/WurzelIn/DSCConfig", configurationContainer2);
                        parameterContainer13.setParameter("/WurzelIn/StateLists/Result[0]/Path", "/Result/Status/VARIANTE");
                        parameterContainer13.setParameter("/WurzelIn/StateLists/Result[0]/Unit", "");
                        parameterContainer13.setParameter("/WurzelIn/StateLists/Result[1]/Path", "/Result/Status/JOB_STATUS");
                        parameterContainer13.setParameter("/WurzelIn/StateLists/Result[1]/Unit", "");
                        Factory.CreateServiceDialog(this, "Versorgungspruefung", "51939083", _globalTabModuleISTA, 58064, parameterContainer13, parameterContainer15).Invoke("InitializeDialog", parameterContainer13, parameterContainer14, parameterContainer15);
                        IDiagnosticDeviceResult diagnosticDeviceResult2 = (IDiagnosticDeviceResult)parameterContainer14.getParameter("/WurzelOut/DSCResult");
                        int num4 = 0;
                        object iSTAResultAsType4 = diagnosticDeviceResult2.getISTAResultAsType("/Result/Rows/$Count", typeof(int));
                        if (iSTAResultAsType4 != null)
                        {
                            num4 = (int)iSTAResultAsType4;
                        }

                        object iSTAResultAsType5 = diagnosticDeviceResult2.getISTAResultAsType("/Result/Status/VARIANTE", typeof(string));
                        if (iSTAResultAsType5 != null)
                        {
                            text2 = (string)iSTAResultAsType5;
                        }

                        object iSTAResultAsType6 = diagnosticDeviceResult2.getISTAResultAsType("/Result/Status/JOB_STATUS", typeof(string));
                        if (iSTAResultAsType6 != null)
                        {
                            text = (string)iSTAResultAsType6;
                        }

                        _ = 0;
                        switch (text3)
                        {
                            case "-":
                                text3 = "\\";
                                break;
                            case "\\":
                                text3 = "|";
                                break;
                            case "|":
                                text3 = "/";
                                break;
                            default:
                                text3 = "-";
                                break;
                        }

                        p_Status_Ident[0] = text;
                        if (text == "OKAY")
                        {
                            textLocator = __Text("71495287563");
                        }
                        else
                        {
                            textLocator = __Text("71493852683");
                            p_Status_Ident_ges = "OKAY";
                        }

                        Sleep(100);
                        ParameterContainer parameterContainer16 = new ParameterContainer();
                        ParameterContainer parameterContainer17 = new ParameterContainer();
                        ParameterContainer inoutParam = new ParameterContainer();
                        parameterContainer16.setParameter("txtParam", __Text("71495280011", new __TextParameter[3] { new __TextParameter("parameter2", p_Steuergeraet[0]), new __TextParameter("parameter1", textLocator), new __TextParameter("parameter3", text3) }));
                        parameterContainer16.setParameter("WertFeld", null);
                        parameterContainer16.setParameter("Quittierung", false);
                        parameterContainer16.setParameter("TIMEOUT", 0);
                        parameterContainer16.setParameter("Protocol", true);
                        parameterContainer16.setParameter("Display", true);
                        MessageServiceDlg1.Invoke("InitializeDialog", parameterContainer16, parameterContainer17, inoutParam);
                        if (parameterContainer17.getParameter("Quit") != null)
                        {
                            QUIT = (bool)parameterContainer17.getParameter("Quit");
                        }

                        _DoLoopHandling = false;
                    }

                    MessageServiceDlg1.Invoke("HideDialog", null, null, null);
                    if (text == "OKAY")
                    {
                        if (p_Verdacht_Versorgung[0] == "-")
                        {
                            ParameterContainer parameterContainer18 = new ParameterContainer();
                            ParameterContainer parameterContainer19 = new ParameterContainer();
                            ParameterContainer parameterContainer20 = new ParameterContainer();
                            parameterContainer18.setParameter("priorText", __Text("71495302667", new __TextParameter[1] { new __TextParameter("parameter1", p_Steuergeraet[0]) }));
                            parameterContainer18.setParameter("pastText", null);
                            parameterContainer18.setParameter("ButtonCount", 2);
                            parameterContainer18.setParameter("ButtonLabel1", __Text("71495196427"));
                            parameterContainer18.setParameter("ButtonLabel2", __Text("71495196427"));
                            parameterContainer18.setParameter("ButtonText1", __Text("71495203979"));
                            parameterContainer18.setParameter("ButtonText2", __Text("71495211531"));
                            parameterContainer18.setParameter("Display", true);
                            Factory.CreateServiceDialog(this, "Versorgungspruefung", "51911691", _globalTabModuleISTA, 53907, parameterContainer18, parameterContainer20).Invoke("InitializeDialog2", parameterContainer18, parameterContainer19, parameterContainer20);
                            if (parameterContainer19.getParameter("Result") != null)
                            {
                                RESULT = (int)parameterContainer19.getParameter("Result");
                            }
                        }
                        else
                        {
                            ParameterContainer parameterContainer21 = new ParameterContainer();
                            ParameterContainer parameterContainer22 = new ParameterContainer();
                            ParameterContainer parameterContainer23 = new ParameterContainer();
                            parameterContainer21.setParameter("priorText", __Text("71495302667", new __TextParameter[1] { new __TextParameter("parameter1", p_Steuergeraet[0]) }));
                            parameterContainer21.setParameter("pastText", null);
                            parameterContainer21.setParameter("ButtonCount", 2);
                            parameterContainer21.setParameter("ButtonLabel1", __Text("71495196427"));
                            parameterContainer21.setParameter("ButtonLabel2", __Text("71495196427"));
                            parameterContainer21.setParameter("ButtonText1", __Text("71495203979"));
                            parameterContainer21.setParameter("ButtonText2", __Text("71495211531"));
                            parameterContainer21.setParameter("Display", true);
                            Factory.CreateServiceDialog(this, "Versorgungspruefung", "51911691", _globalTabModuleISTA, 53942, parameterContainer21, parameterContainer23).Invoke("InitializeDialog2", parameterContainer21, parameterContainer22, parameterContainer23);
                            if (parameterContainer22.getParameter("Result") != null)
                            {
                                RESULT = (int)parameterContainer22.getParameter("Result");
                            }
                        }
                    }
                    else if (p_Verdacht_Versorgung[0] == "-")
                    {
                        ParameterContainer parameterContainer24 = new ParameterContainer();
                        ParameterContainer parameterContainer25 = new ParameterContainer();
                        ParameterContainer parameterContainer26 = new ParameterContainer();
                        parameterContainer24.setParameter("priorText", __Text("71495188619", new __TextParameter[1] { new __TextParameter("parameter1", p_Steuergeraet[0]) }));
                        parameterContainer24.setParameter("pastText", null);
                        parameterContainer24.setParameter("ButtonCount", 2);
                        parameterContainer24.setParameter("ButtonLabel1", __Text("71495196427"));
                        parameterContainer24.setParameter("ButtonLabel2", __Text("71495196427"));
                        parameterContainer24.setParameter("ButtonText1", __Text("71495203979"));
                        parameterContainer24.setParameter("ButtonText2", __Text("71495211531"));
                        parameterContainer24.setParameter("Display", true);
                        Factory.CreateServiceDialog(this, "Versorgungspruefung", "51911691", _globalTabModuleISTA, 53978, parameterContainer24, parameterContainer26).Invoke("InitializeDialog2", parameterContainer24, parameterContainer25, parameterContainer26);
                        if (parameterContainer25.getParameter("Result") != null)
                        {
                            RESULT = (int)parameterContainer25.getParameter("Result");
                        }
                    }
                    else
                    {
                        ParameterContainer parameterContainer27 = new ParameterContainer();
                        ParameterContainer parameterContainer28 = new ParameterContainer();
                        ParameterContainer parameterContainer29 = new ParameterContainer();
                        parameterContainer27.setParameter("priorText", __Text("71495264523", new __TextParameter[2] { new __TextParameter("parameter1", p_Steuergeraet[0]), new __TextParameter("parameter2", value) }));
                        parameterContainer27.setParameter("pastText", null);
                        parameterContainer27.setParameter("ButtonCount", 2);
                        parameterContainer27.setParameter("ButtonLabel1", __Text("71495196427"));
                        parameterContainer27.setParameter("ButtonLabel2", __Text("71495196427"));
                        parameterContainer27.setParameter("ButtonText1", __Text("71495203979"));
                        parameterContainer27.setParameter("ButtonText2", __Text("71495211531"));
                        parameterContainer27.setParameter("Display", true);
                        Factory.CreateServiceDialog(this, "Versorgungspruefung", "51911691", _globalTabModuleISTA, 54013, parameterContainer27, parameterContainer29).Invoke("InitializeDialog2", parameterContainer27, parameterContainer28, parameterContainer29);
                        if (parameterContainer28.getParameter("Result") != null)
                        {
                            RESULT = (int)parameterContainer28.getParameter("Result");
                        }
                    }

                    num2 = ((RESULT == 1) ? 1 : 0);
                    _DoLoopHandling = false;
                }

                if (text == "OKAY")
                {
                    p_Status_Ident_ges = "REPAIRED";
                }
                else
                {
                    p_Status_Ident_ges = "NOK";
                }
            }

            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }
    }
}