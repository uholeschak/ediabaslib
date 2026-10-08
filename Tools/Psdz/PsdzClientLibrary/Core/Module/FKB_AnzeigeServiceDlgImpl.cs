using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BMW.Rheingold.CoreFramework.Contracts;

namespace BMW.Rheingold.Module.ISTA
{
    internal class FKB_AnzeigeServiceDlgImpl : DtcAnzeigeDynImpl
    {
        public ITextLocator DK_SG_901;
        public int Ersatzwert_Zahl;
        public string Ersatzwert_String;
        public bool QUIT;
        public int SELEKT;
        public int RESULT;
        public string ENTER;
        public List<ITextLocator> Anzeige_Text_Anfang;
        public List<ITextLocator> Anzeige_Text_Ende;
        public string p_F_SELEKT_ORT_NR_HEX;
        public int p_F_SELEKT_ORT_NR_DEZ;
        public string p_F_SELEKT_ORT_TEXT;
        public int p_F_SELEKT_VORHANDEN_NR;
        public string p_F_SELEKT_VORHANDEN_TEXT;
        public int p_F_SELEKT_HFK;
        public string p_F_SELEKT_SGBD;
        public int p_Ausgang_Nr;
        public List<string> akt_FS_Verdachte;
        public List<string> akt_virtFS_Verdachte;
        public List<string> akt_samFS_Verdachte;
        public List<string> akt_Verdachte;
        public IDictionary<string, string> FC_id_code;
        public IDictionary<string, string> FC_id_text;
        public IDictionary<string, string> FC_id_sgbd;
        public IDocumentLocator m_aktFkbDescriptionDocLocator;
        public IDocumentLocator m_aktFkbDetailsDocLocator;
        public IDocumentLocator m_aktFkbSysContextDocLocator;
        public bool m_bDisplayErrorDetailsCalled;
        public bool p_WeiterButtonEnabledStack;
        public int lastSelected;
        public bool whiteText;
        public FKB_AnzeigeServiceDlgImpl(ParameterContainer inParameters) : base(inParameters)
        {
            if (inParameters != null)
            {
                _globalModuleInParameter = inParameters;
            }

            __handleInParameter();
            DK_SG_901 = new TextLocator();
            Ersatzwert_Zahl = -1;
            Ersatzwert_String = "NV";
            QUIT = false;
            SELEKT = 0;
            RESULT = 0;
            ENTER = "";
            Anzeige_Text_Anfang = new List<ITextLocator>();
            Anzeige_Text_Ende = new List<ITextLocator>();
            p_F_SELEKT_ORT_NR_HEX = "";
            p_F_SELEKT_ORT_NR_DEZ = -1;
            p_F_SELEKT_ORT_TEXT = "";
            p_F_SELEKT_VORHANDEN_NR = 0;
            p_F_SELEKT_VORHANDEN_TEXT = "";
            p_F_SELEKT_HFK = 0;
            p_F_SELEKT_SGBD = "";
            akt_FS_Verdachte = new List<string>();
            akt_virtFS_Verdachte = new List<string>();
            akt_samFS_Verdachte = new List<string>();
            akt_Verdachte = new List<string>();
            FC_id_code = new Dictionary<string, string>();
            FC_id_text = new Dictionary<string, string>();
            FC_id_sgbd = new Dictionary<string, string>();
            m_aktFkbDescriptionDocLocator = null;
            m_aktFkbDetailsDocLocator = null;
            m_aktFkbSysContextDocLocator = null;
            m_bDisplayErrorDetailsCalled = true;
            p_WeiterButtonEnabledStack = false;
            lastSelected = -1;
            whiteText = true;
        }

        public virtual void Prepare()
        {
        }

        public virtual void Reset()
        {
        }

        private void Onm_EnterpriseEnumCtrlSelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayErrorDetails();
        }

        public override void Invoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            if ("FKB_Anzeige".Equals(method))
            {
                int ablaufsteuerung = Convert.ToInt32(inParam.getParameter("Ablaufsteuerung", 300421));
                ITextLocator diagnosekode_ = inParam.getParameter("Diagnosekode_901", __Text()) as ITextLocator;
                string F_SELEKT_ORT_NR_HEX = null;
                int F_SELEKT_ORT_NR_DEZ = 0;
                string F_SELEKT_ORT_TEXT = null;
                int F_SELEKT_VORHANDEN_NR = 0;
                string F_SELEKT_VORHANDEN_TEXT = null;
                int F_SELEKT_HFK = 0;
                string F_SELEKT_SGBD = null;
                int Ausgang_Nr = 0;
                string FC_id = null;
                FKB_Anzeige(filterVirtualFaults: true, ablaufsteuerung, diagnosekode_, ref F_SELEKT_ORT_NR_HEX, ref F_SELEKT_ORT_NR_DEZ, ref F_SELEKT_ORT_TEXT, ref F_SELEKT_VORHANDEN_NR, ref F_SELEKT_VORHANDEN_TEXT, ref F_SELEKT_HFK, ref F_SELEKT_SGBD, ref Ausgang_Nr, ref FC_id);
                outParam.setParameter("F_SELEKT_ORT_NR_HEX", F_SELEKT_ORT_NR_HEX);
                outParam.setParameter("F_SELEKT_ORT_NR_DEZ", F_SELEKT_ORT_NR_DEZ);
                outParam.setParameter("F_SELEKT_ORT_TEXT", F_SELEKT_ORT_TEXT);
                outParam.setParameter("F_SELEKT_VORHANDEN_NR", F_SELEKT_VORHANDEN_NR);
                outParam.setParameter("F_SELEKT_VORHANDEN_TEXT", F_SELEKT_VORHANDEN_TEXT);
                outParam.setParameter("F_SELEKT_HFK", F_SELEKT_HFK);
                outParam.setParameter("F_SELEKT_SGBD", F_SELEKT_SGBD);
                outParam.setParameter("Ausgang_Nr", Ausgang_Nr);
                return;
            }

            if ("Anzeige_aller_Fehler".Equals(method))
            {
                int ablaufsteuerung2 = Convert.ToInt32(inParam.getParameter("Ablaufsteuerung", 300421));
                ITextLocator diagnosekode_2 = inParam.getParameter("Diagnosekode_901", __Text()) as ITextLocator;
                string F_SELEKT_ORT_NR_HEX2 = null;
                int F_SELEKT_ORT_NR_DEZ2 = 0;
                string F_SELEKT_ORT_TEXT2 = null;
                int F_SELEKT_VORHANDEN_NR2 = 0;
                string F_SELEKT_VORHANDEN_TEXT2 = null;
                int F_SELEKT_HFK2 = 0;
                string F_SELEKT_SGBD2 = null;
                int Ausgang_Nr2 = 0;
                string FC_id2 = null;
                FKB_Anzeige(filterVirtualFaults: false, ablaufsteuerung2, diagnosekode_2, ref F_SELEKT_ORT_NR_HEX2, ref F_SELEKT_ORT_NR_DEZ2, ref F_SELEKT_ORT_TEXT2, ref F_SELEKT_VORHANDEN_NR2, ref F_SELEKT_VORHANDEN_TEXT2, ref F_SELEKT_HFK2, ref F_SELEKT_SGBD2, ref Ausgang_Nr2, ref FC_id2);
                outParam.setParameter("F_SELEKT_ORT_NR_DEZ", F_SELEKT_ORT_NR_DEZ2);
                outParam.setParameter("F_SELEKT_NR_TEXT", F_SELEKT_ORT_TEXT2);
                outParam.setParameter("F_SELEKT_VORHANDEN_NR", F_SELEKT_VORHANDEN_NR2);
                outParam.setParameter("F_SELEKT_VORHANDEN_TEXT", F_SELEKT_VORHANDEN_TEXT2);
                outParam.setParameter("F_SELEKT_HFK", F_SELEKT_HFK2);
                outParam.setParameter("F_SELEKT_SGBD", F_SELEKT_SGBD2);
                outParam.setParameter("Ausgang_Nr", Ausgang_Nr2);
                if (!string.IsNullOrEmpty(FC_id2))
                {
                    IFaultCodeLocator faultCodeLocator = GetFaultCode(FC_id2);
                    if (faultCodeLocator == null)
                    {
                        faultCodeLocator = GetVirtualFaultCode(FC_id2);
                        if (faultCodeLocator == null)
                        {
                            faultCodeLocator = GetCombinedFaultCode(FC_id2);
                        }
                    }

                    if (faultCodeLocator != null)
                    {
                        outParam.setParameter("F_SELEKT_NR_TEXT", (faultCodeLocator.TextContent != null) ? faultCodeLocator.TextContent.PlainText : "NV");
                        outParam.setParameter("F_SELEKT_CODE", faultCodeLocator.GetDataValue("F_SELEKT_CODE"));
                    }
                    else
                    {
                        outParam.setParameter("F_SELEKT_CODE", "NV");
                    }
                }
                else
                {
                    outParam.setParameter("F_SELEKT_CODE", "NV");
                }

                return;
            }

            throw new ServiceDialogMethodUnsupportedException(method);
        }

        public virtual void FKB_Anzeige(bool filterVirtualFaults, int Ablaufsteuerung, ITextLocator Diagnosekode_901, ref string F_SELEKT_ORT_NR_HEX, ref int F_SELEKT_ORT_NR_DEZ, ref string F_SELEKT_ORT_TEXT, ref int F_SELEKT_VORHANDEN_NR, ref string F_SELEKT_VORHANDEN_TEXT, ref int F_SELEKT_HFK, ref string F_SELEKT_SGBD, ref int Ausgang_Nr, ref string FC_id)
        {
            int num = 0;
            int num2 = 0;
            bool flag = false;
            Logger.WriteInformation("FKB_Anzeige called");
            string text = "";
            string text2 = "";
            string text3 = "";
            string text4 = "0";
            string text5 = "0";
            string text6 = "0";
            string text7 = "0";
            string text8 = "0";
            string text9 = "0";
            DK_SG_901 = Diagnosekode_901;
            text = Convert.ToString(Ablaufsteuerung);
            if (text.Length > 3)
            {
                text2 = text.Substring(0, 3);
                text = text.Substring(3, 3);
                num2 = Convert.ToInt32(text2);
                Ablaufsteuerung = Convert.ToInt32(text);
            }
            else if (flag)
            {
                num2 = Ablaufsteuerung;
            }

            if ((Ablaufsteuerung == 300 || Ablaufsteuerung == 411 || Ablaufsteuerung == 412 || Ablaufsteuerung == 421 || Ablaufsteuerung == 422 || Ablaufsteuerung == 431 || Ablaufsteuerung == 432) && !flag)
            {
                Log.Info("FKB_Anzeige()", "valid input for ISTA: {0}", Ablaufsteuerung);
            }
            else if (!flag)
            {
                Ablaufsteuerung = 300;
                Log.Info("FKB_Anzeige()", "default: {0}", Ablaufsteuerung);
            }

            if ((num2 == 300 || num2 == 411 || num2 == 412 || num2 == 421 || num2 == 422 || num2 == 431 || num2 == 432) && flag)
            {
                Log.Info("FKB_Anzeige()", "valid input for Teleservice: {0}", num2);
            }
            else if (flag)
            {
                num2 = 300;
                Log.Info("FKB_Anzeige()", "default: {0}", Ablaufsteuerung);
            }

            if (!flag)
            {
                if (Ablaufsteuerung > 0)
                {
                    text3 = Convert.ToString(Ablaufsteuerung);
                    if (Ablaufsteuerung >= 100)
                    {
                        text4 = text3.Substring(0, 1);
                        text5 = text3.Substring(1, 1);
                        text6 = text3.Substring(2, 1);
                    }
                    else if (Ablaufsteuerung >= 10)
                    {
                        text4 = text3.Substring(0, 1);
                        text5 = text3.Substring(1, 1);
                        text6 = "0";
                    }
                    else
                    {
                        text4 = text3.Substring(0, 1);
                        text5 = "0";
                        text6 = "0";
                    }
                }
                else
                {
                    text3 = "000";
                    text4 = text3.Substring(0, 1);
                    text5 = text3.Substring(1, 1);
                    text6 = text3.Substring(2, 1);
                }
            }

            if (flag)
            {
                if (num2 >= 0)
                {
                    text2 = Convert.ToString(num2);
                    if (num2 >= 100)
                    {
                        text7 = text2.Substring(0, 1);
                        text8 = text2.Substring(1, 1);
                        text9 = text2.Substring(2, 1);
                    }
                    else if (num2 >= 10)
                    {
                        text7 = text2.Substring(0, 1);
                        text8 = text2.Substring(1, 1);
                        text9 = "0";
                    }
                    else
                    {
                        text7 = text2.Substring(0, 1);
                        text8 = "0";
                        text9 = "0";
                    }
                }
                else
                {
                    text2 = "000";
                    text4 = text2.Substring(0, 1);
                    text5 = text2.Substring(1, 1);
                    text6 = text2.Substring(2, 1);
                }
            }

            Anzeige_Text_Anfang = new List<ITextLocator>(new ITextLocator[1] { __Text("68025239179") });
            if (!flag)
            {
                if (!(text4 == "3"))
                {
                    if (text4 == "4")
                    {
                        if (text6 == "1")
                        {
                            Anzeige_Text_Ende = new List<ITextLocator>(new ITextLocator[1] { __Text("68025561739") });
                        }
                        else
                        {
                            Anzeige_Text_Ende = new List<ITextLocator>(new ITextLocator[1] { __Text("68915705739") });
                        }
                    }
                    else
                    {
                        Anzeige_Text_Anfang = new List<ITextLocator>(new ITextLocator[1] { __Text("68025239179") });
                        Anzeige_Text_Ende = new List<ITextLocator>(new ITextLocator[1] { __Text("68025561739") });
                    }
                }
                else
                {
                    Anzeige_Text_Ende = new List<ITextLocator>(new ITextLocator[1] { __Text("68025554059") });
                }
            }

            if (flag)
            {
                if (!(text7 == "3"))
                {
                    if (text7 == "4")
                    {
                        if (text9 == "1")
                        {
                            Anzeige_Text_Ende = new List<ITextLocator>(new ITextLocator[1] { __Text("68025561739") });
                        }
                        else
                        {
                            Anzeige_Text_Ende = new List<ITextLocator>(new ITextLocator[1] { __Text("68915705739") });
                        }
                    }
                    else
                    {
                        Anzeige_Text_Anfang = new List<ITextLocator>(new ITextLocator[1] { __Text("68025239179") });
                        Anzeige_Text_Ende = new List<ITextLocator>(new ITextLocator[1] { __Text("68025561739") });
                    }
                }
                else
                {
                    Anzeige_Text_Ende = new List<ITextLocator>(new ITextLocator[1] { __Text("20000378124251") });
                }
            }

            ParameterContainer parameterContainer = new ParameterContainer();
            ParameterContainer outParam = new ParameterContainer();
            ParameterContainer parameterContainer2 = new ParameterContainer();
            parameterContainer.setParameter("txtParam", __Text("71535854475"));
            parameterContainer.setParameter("Quittierung", false);
            parameterContainer.setParameter("TIMEOUT", 0);
            parameterContainer.setParameter("Protocol", false);
            parameterContainer.setParameter("Display", true);
            Factory.CreateServiceDialog(this, "FKB_Anzeige", "51915403", _globalTabModuleISTA, 38732, parameterContainer, parameterContainer2).Invoke("InitializeDialog", parameterContainer, outParam, parameterContainer2);
            Panel.Forward.Enabled = false;
            DTC_ANZEIGE_DYN_Start(Anzeige_Text_Anfang, Anzeige_Text_Ende, Ausblenden_realerFehlerspeicher: false, filterVirtualFaults, filterVirtualFaults, ref p_F_SELEKT_ORT_NR_HEX, ref p_F_SELEKT_ORT_NR_DEZ, ref p_F_SELEKT_SGBD, ref p_F_SELEKT_ORT_TEXT, ref FC_id);
            switch (p_F_SELEKT_ORT_NR_DEZ)
            {
                case -1:
                    p_F_SELEKT_ORT_NR_HEX = Ersatzwert_String;
                    p_F_SELEKT_ORT_NR_DEZ = Ersatzwert_Zahl;
                    p_F_SELEKT_ORT_TEXT = Ersatzwert_String;
                    p_F_SELEKT_VORHANDEN_NR = Ersatzwert_Zahl;
                    p_F_SELEKT_VORHANDEN_TEXT = Ersatzwert_String;
                    p_F_SELEKT_HFK = Ersatzwert_Zahl;
                    p_F_SELEKT_SGBD = Ersatzwert_String;
                    break;
                case 0:
                    p_F_SELEKT_VORHANDEN_NR = Ersatzwert_Zahl;
                    p_F_SELEKT_VORHANDEN_TEXT = Ersatzwert_String;
                    p_F_SELEKT_HFK = Ersatzwert_Zahl;
                    if (p_F_SELEKT_SGBD == "")
                    {
                        p_F_SELEKT_SGBD = Ersatzwert_String;
                    }

                    break;
                default:
                {
                    ParameterContainer parameterContainer3 = new ParameterContainer();
                    ParameterContainer parameterContainer4 = new ParameterContainer();
                    ParameterContainer parameterContainer5 = new ParameterContainer();
                    parameterContainer3.setParameter("F_ORT_NR_HEX", p_F_SELEKT_ORT_NR_HEX);
                    Factory.CreateServiceDialog(this, "FKB_Anzeige", "67207569803", _globalTabModuleISTA, 37136, parameterContainer3, parameterContainer5).Invoke("DTC_Details_kurz", parameterContainer3, parameterContainer4, parameterContainer5);
                    if (parameterContainer4.getParameter("F_finden_ORT_NR_DEZ") != null)
                    {
                        p_F_SELEKT_ORT_NR_DEZ = (int)parameterContainer4.getParameter("F_finden_ORT_NR_DEZ");
                    }

                    if (parameterContainer4.getParameter("F_finden_ORT_TEXT") != null)
                    {
                        p_F_SELEKT_ORT_TEXT = (string)parameterContainer4.getParameter("F_finden_ORT_TEXT");
                    }

                    if (parameterContainer4.getParameter("F_finden_VORHANDEN_NR") != null)
                    {
                        p_F_SELEKT_VORHANDEN_NR = (int)parameterContainer4.getParameter("F_finden_VORHANDEN_NR");
                    }

                    if (parameterContainer4.getParameter("F_finden_VORHANDEN_TEXT") != null)
                    {
                        p_F_SELEKT_VORHANDEN_TEXT = (string)parameterContainer4.getParameter("F_finden_VORHANDEN_TEXT");
                    }

                    if (parameterContainer4.getParameter("F_finden_HFK") != null)
                    {
                        p_F_SELEKT_HFK = (int)parameterContainer4.getParameter("F_finden_HFK");
                    }

                    break;
                }
            }

            if (!flag)
            {
                if (p_F_SELEKT_ORT_NR_DEZ > -1)
                {
                    if (text4 == "3")
                    {
                        ParameterContainer parameterContainer6 = new ParameterContainer();
                        ParameterContainer parameterContainer7 = new ParameterContainer();
                        ParameterContainer parameterContainer8 = new ParameterContainer();
                        parameterContainer6.setParameter("priorText", __Text("68025570827"));
                        parameterContainer6.setParameter("ButtonCount", 2);
                        parameterContainer6.setParameter("ButtonLabel1", __Text("68025578507"));
                        parameterContainer6.setParameter("ButtonLabel2", __Text("68025578507"));
                        parameterContainer6.setParameter("ButtonText1", __Text("68026123659"));
                        parameterContainer6.setParameter("ButtonText2", __Text("68026131339"));
                        parameterContainer6.setParameter("Display", true);
                        Factory.CreateServiceDialog(this, "FKB_Anzeige", "13628358027", _globalTabModuleISTA, 36817, parameterContainer6, parameterContainer8).Invoke("InitializeDialog2", parameterContainer6, parameterContainer7, parameterContainer8);
                        if (parameterContainer7.getParameter("Result") != null)
                        {
                            SELEKT = (int)parameterContainer7.getParameter("Result");
                        }

                        if (SELEKT == 1)
                        {
                            p_Ausgang_Nr = 2;
                        }
                        else
                        {
                            p_Ausgang_Nr = 1;
                        }
                    }
                    else if (!(text6 == "1"))
                    {
                        if (text6 == "2")
                        {
                            p_Ausgang_Nr = 5;
                        }
                        else
                        {
                            p_Ausgang_Nr = 4;
                        }
                    }
                    else
                    {
                        p_Ausgang_Nr = 4;
                    }
                }
                else if (text4 == "3")
                {
                    ParameterContainer parameterContainer9 = new ParameterContainer();
                    ParameterContainer outParam2 = new ParameterContainer();
                    ParameterContainer parameterContainer10 = new ParameterContainer();
                    parameterContainer9.setParameter("Display", true);
                    parameterContainer9.setParameter("__Anfang", __Text("68026139019"));
                    parameterContainer9.setParameter("_1er_Button", __Text("68026146699"));
                    parameterContainer9.setParameter("_1er_Diagnosekode", DK_SG_901);
                    Factory.CreateServiceDialog(this, "FKB_Anzeige", "51937067403", _globalTabModuleISTA, 37288, parameterContainer9, parameterContainer10).Invoke("Maximal_6_Diagnosekodes", parameterContainer9, outParam2, parameterContainer10);
                    p_Ausgang_Nr = 0;
                }
                else
                {
                    if (text5 == "1")
                    {
                        ParameterContainer parameterContainer11 = new ParameterContainer();
                        ParameterContainer parameterContainer12 = new ParameterContainer();
                        ParameterContainer parameterContainer13 = new ParameterContainer();
                        parameterContainer11.setParameter("priorText", __Text("68026154763"));
                        parameterContainer11.setParameter("pastText", null);
                        parameterContainer11.setParameter("ButtonCount", 2);
                        parameterContainer11.setParameter("ButtonLabel1", __Text("68025578507"));
                        parameterContainer11.setParameter("ButtonLabel2", __Text("68025578507"));
                        parameterContainer11.setParameter("ButtonLabel3", null);
                        parameterContainer11.setParameter("ButtonLabel4", null);
                        parameterContainer11.setParameter("ButtonLabel5", null);
                        parameterContainer11.setParameter("ButtonLabel6", null);
                        parameterContainer11.setParameter("ButtonText1", __Text("68026213771"));
                        parameterContainer11.setParameter("ButtonText2", __Text("68026221451"));
                        parameterContainer11.setParameter("ButtonText3", null);
                        parameterContainer11.setParameter("ButtonText4", null);
                        parameterContainer11.setParameter("ButtonText5", null);
                        parameterContainer11.setParameter("ButtonText6", null);
                        parameterContainer11.setParameter("Display", true);
                        Factory.CreateServiceDialog(this, "FKB_Anzeige", "51911691", _globalTabModuleISTA, 36965, parameterContainer11, parameterContainer13).Invoke("InitializeDialog2", parameterContainer11, parameterContainer12, parameterContainer13);
                        if (parameterContainer12.getParameter("Result") != null)
                        {
                            SELEKT = (int)parameterContainer12.getParameter("Result");
                        }

                        if (SELEKT == 1)
                        {
                            p_Ausgang_Nr = 3;
                        }
                        else
                        {
                            p_Ausgang_Nr = 0;
                        }
                    }

                    if (text5 == "3")
                    {
                        ParameterContainer parameterContainer14 = new ParameterContainer();
                        ParameterContainer outParam3 = new ParameterContainer();
                        ParameterContainer parameterContainer15 = new ParameterContainer();
                        parameterContainer14.setParameter("Display", true);
                        parameterContainer14.setParameter("__Anfang", __Text("72286061707"));
                        parameterContainer14.setParameter("_1er_Button", __Text("68026146699"));
                        parameterContainer14.setParameter("_1er_Diagnosekode", DK_SG_901);
                        Factory.CreateServiceDialog(this, "FKB_Anzeige", "51937067403", _globalTabModuleISTA, 36919, parameterContainer14, parameterContainer15).Invoke("Maximal_6_Diagnosekodes", parameterContainer14, outParam3, parameterContainer15);
                        p_Ausgang_Nr = 0;
                    }

                    if (text5 != "1" && text5 != "3")
                    {
                        ParameterContainer parameterContainer16 = new ParameterContainer();
                        ParameterContainer outParam4 = new ParameterContainer();
                        ParameterContainer parameterContainer17 = new ParameterContainer();
                        parameterContainer16.setParameter("Display", true);
                        parameterContainer16.setParameter("__Anfang", __Text("68026139019"));
                        parameterContainer16.setParameter("_1er_Button", __Text("68026146699"));
                        parameterContainer16.setParameter("_1er_Diagnosekode", DK_SG_901);
                        Factory.CreateServiceDialog(this, "FKB_Anzeige", "51937067403", _globalTabModuleISTA, 36919, parameterContainer16, parameterContainer17).Invoke("Maximal_6_Diagnosekodes", parameterContainer16, outParam4, parameterContainer17);
                        p_Ausgang_Nr = 0;
                    }
                }
            }

            if (flag)
            {
                if (p_F_SELEKT_ORT_NR_DEZ > -1)
                {
                    if (text7 == "3")
                    {
                        ParameterContainer parameterContainer18 = new ParameterContainer();
                        ParameterContainer parameterContainer19 = new ParameterContainer();
                        ParameterContainer parameterContainer20 = new ParameterContainer();
                        parameterContainer18.setParameter("priorText", __Text("68025570827"));
                        parameterContainer18.setParameter("ButtonCount", 2);
                        parameterContainer18.setParameter("ButtonLabel1", __Text("68025578507"));
                        parameterContainer18.setParameter("ButtonLabel2", __Text("68025578507"));
                        parameterContainer18.setParameter("ButtonText1", __Text("68026123659"));
                        parameterContainer18.setParameter("ButtonText2", __Text("68026131339"));
                        parameterContainer18.setParameter("Display", true);
                        Factory.CreateServiceDialog(this, "FKB_Anzeige", "13628358027", _globalTabModuleISTA, 36817, parameterContainer18, parameterContainer20).Invoke("InitializeDialog2", parameterContainer18, parameterContainer19, parameterContainer20);
                        if (parameterContainer19.getParameter("Result") != null)
                        {
                            SELEKT = (int)parameterContainer19.getParameter("Result");
                        }

                        if (SELEKT == 1)
                        {
                            p_Ausgang_Nr = 6;
                        }
                        else
                        {
                            p_Ausgang_Nr = 1;
                        }
                    }
                    else if (!(text9 == "1"))
                    {
                        if (text9 == "2")
                        {
                            p_Ausgang_Nr = 5;
                        }
                        else
                        {
                            p_Ausgang_Nr = 4;
                        }
                    }
                    else
                    {
                        p_Ausgang_Nr = 4;
                    }
                }
                else if (text7 == "3")
                {
                    ParameterContainer parameterContainer21 = new ParameterContainer();
                    ParameterContainer outParam5 = new ParameterContainer();
                    ParameterContainer parameterContainer22 = new ParameterContainer();
                    parameterContainer21.setParameter("Display", true);
                    parameterContainer21.setParameter("__Anfang", __Text("20000378124252"));
                    parameterContainer21.setParameter("_1er_Button", __Text("68026146699"));
                    parameterContainer21.setParameter("_1er_Diagnosekode", DK_SG_901);
                    Factory.CreateServiceDialog(this, "FKB_Anzeige", "51937067403", _globalTabModuleISTA, 37288, parameterContainer21, parameterContainer22).Invoke("Maximal_6_Diagnosekodes", parameterContainer21, outParam5, parameterContainer22);
                    p_Ausgang_Nr = 7;
                }
                else
                {
                    if (text8 == "1")
                    {
                        ParameterContainer parameterContainer23 = new ParameterContainer();
                        ParameterContainer parameterContainer24 = new ParameterContainer();
                        ParameterContainer parameterContainer25 = new ParameterContainer();
                        parameterContainer23.setParameter("priorText", __Text("68026154763"));
                        parameterContainer23.setParameter("pastText", null);
                        parameterContainer23.setParameter("ButtonCount", 2);
                        parameterContainer23.setParameter("ButtonLabel1", __Text("68025578507"));
                        parameterContainer23.setParameter("ButtonLabel2", __Text("68025578507"));
                        parameterContainer23.setParameter("ButtonLabel3", null);
                        parameterContainer23.setParameter("ButtonLabel4", null);
                        parameterContainer23.setParameter("ButtonLabel5", null);
                        parameterContainer23.setParameter("ButtonLabel6", null);
                        parameterContainer23.setParameter("ButtonText1", __Text("68026213771"));
                        parameterContainer23.setParameter("ButtonText2", __Text("68026221451"));
                        parameterContainer23.setParameter("ButtonText3", null);
                        parameterContainer23.setParameter("ButtonText4", null);
                        parameterContainer23.setParameter("ButtonText5", null);
                        parameterContainer23.setParameter("ButtonText6", null);
                        parameterContainer23.setParameter("Display", true);
                        Factory.CreateServiceDialog(this, "FKB_Anzeige", "51911691", _globalTabModuleISTA, 36965, parameterContainer23, parameterContainer25).Invoke("InitializeDialog2", parameterContainer23, parameterContainer24, parameterContainer25);
                        if (parameterContainer24.getParameter("Result") != null)
                        {
                            SELEKT = (int)parameterContainer24.getParameter("Result");
                        }

                        if (SELEKT == 1)
                        {
                            p_Ausgang_Nr = 3;
                        }
                        else
                        {
                            p_Ausgang_Nr = 0;
                        }
                    }

                    if (text8 == "3")
                    {
                        ParameterContainer parameterContainer26 = new ParameterContainer();
                        ParameterContainer outParam6 = new ParameterContainer();
                        ParameterContainer parameterContainer27 = new ParameterContainer();
                        parameterContainer26.setParameter("Display", true);
                        parameterContainer26.setParameter("__Anfang", __Text("72286061707"));
                        parameterContainer26.setParameter("_1er_Button", __Text("68026146699"));
                        parameterContainer26.setParameter("_1er_Diagnosekode", DK_SG_901);
                        Factory.CreateServiceDialog(this, "FKB_Anzeige", "51937067403", _globalTabModuleISTA, 36919, parameterContainer26, parameterContainer27).Invoke("Maximal_6_Diagnosekodes", parameterContainer26, outParam6, parameterContainer27);
                        p_Ausgang_Nr = 0;
                    }

                    if (text8 != "1" && text8 != "3")
                    {
                        ParameterContainer parameterContainer28 = new ParameterContainer();
                        ParameterContainer outParam7 = new ParameterContainer();
                        ParameterContainer parameterContainer29 = new ParameterContainer();
                        parameterContainer28.setParameter("Display", true);
                        parameterContainer28.setParameter("__Anfang", __Text("68026139019"));
                        parameterContainer28.setParameter("_1er_Button", __Text("68026146699"));
                        parameterContainer28.setParameter("_1er_Diagnosekode", DK_SG_901);
                        Factory.CreateServiceDialog(this, "FKB_Anzeige", "51937067403", _globalTabModuleISTA, 36919, parameterContainer28, parameterContainer29).Invoke("Maximal_6_Diagnosekodes", parameterContainer28, outParam7, parameterContainer29);
                        p_Ausgang_Nr = 0;
                    }
                }
            }

            F_SELEKT_ORT_NR_HEX = p_F_SELEKT_ORT_NR_HEX;
            F_SELEKT_ORT_NR_DEZ = p_F_SELEKT_ORT_NR_DEZ;
            F_SELEKT_ORT_TEXT = p_F_SELEKT_ORT_TEXT;
            F_SELEKT_VORHANDEN_NR = p_F_SELEKT_VORHANDEN_NR;
            F_SELEKT_VORHANDEN_TEXT = p_F_SELEKT_VORHANDEN_TEXT;
            F_SELEKT_HFK = p_F_SELEKT_HFK;
            F_SELEKT_SGBD = p_F_SELEKT_SGBD;
            Ausgang_Nr = p_Ausgang_Nr;
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        private void DTC_ANZEIGE_DYN_Start(List<ITextLocator> Anzeige_Text_Anfang, List<ITextLocator> Anzeige_Text_Ende, bool Ausblenden_realerFehlerspeicher, bool Ausblenden_Sammelfehler, bool Ausblenden_VirtuelleFehler, ref string Status_Fehlerkode_selektiert_hex, ref int Status_Fehlerkode_selektiert_dez, ref string SGBD_Fehlerkode_selektiert, ref string Status_fehlerkode_selektiert_Text, ref string Status_Fehlerkode_selektiert_ID)
        {
            int num = 0;
            Logger.WriteInformation("DTC_ANZEIGE_DYN_Start called");
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            int num5 = -1;
            ParameterContainer parameterContainer = new ParameterContainer();
            ParameterContainer parameterContainer2 = new ParameterContainer();
            ParameterContainer parameterContainer3 = new ParameterContainer();
            Factory.CreateServiceDialog(this, "DTC_ANZEIGE_DYN_Start", "52683531", _globalTabModuleISTA, 2650, parameterContainer, parameterContainer3).Invoke("AnzahlFehlerspeicher", parameterContainer, parameterContainer2, parameterContainer3);
            if (parameterContainer2.getParameter("anzahlFehlerkodes") != null)
            {
                num2 = (int)parameterContainer2.getParameter("anzahlFehlerkodes");
            }

            if (parameterContainer2.getParameter("anzahlVirtuelleFehlerkodes") != null)
            {
                num3 = (int)parameterContainer2.getParameter("anzahlVirtuelleFehlerkodes");
            }

            if (parameterContainer2.getParameter("anzahlSammelfehlerkodes") != null)
            {
                num4 = (int)parameterContainer2.getParameter("anzahlSammelfehlerkodes");
            }

            StringBuilder stringBuilder = new StringBuilder();
            ITextLocator textLocator = null;
            DateTime now = DateTime.Now;
            if (num2 + num3 + num4 > 0)
            {
                List<string> list = null;
                if (num2 > 0 && !Ausblenden_realerFehlerspeicher)
                {
                    List<string> list2 = null;
                    List<string> list3 = null;
                    List<string> list4 = null;
                    ParameterContainer parameterContainer4 = new ParameterContainer();
                    ParameterContainer parameterContainer5 = new ParameterContainer();
                    ParameterContainer parameterContainer6 = new ParameterContainer();
                    Factory.CreateServiceDialog(this, "DTC_ANZEIGE_DYN_Start", "52683531", _globalTabModuleISTA, 2670, parameterContainer4, parameterContainer6).Invoke("Fehlerkodes", parameterContainer4, parameterContainer5, parameterContainer6);
                    if (parameterContainer5.getParameter("fehlerkode_NodeID") != null)
                    {
                        list = (List<string>)parameterContainer5.getParameter("fehlerkode_NodeID");
                    }

                    if (parameterContainer5.getParameter("fehlerkode_Text") != null)
                    {
                        list2 = (List<string>)parameterContainer5.getParameter("fehlerkode_Text");
                    }

                    if (parameterContainer5.getParameter("fehlerkode_hex") != null)
                    {
                        list3 = (List<string>)parameterContainer5.getParameter("fehlerkode_hex");
                    }

                    if (parameterContainer5.getParameter("fehlerkode_SGBD") != null)
                    {
                        list4 = (List<string>)parameterContainer5.getParameter("fehlerkode_SGBD");
                    }

                    _DoLoopHandling = true;
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (!FC_id_code.ContainsKey(list[i]))
                        {
                            FC_id_code.Add(list[i], list3[i]);
                            FC_id_text.Add(list[i], list2[i]);
                            FC_id_sgbd.Add(list[i], list4[i]);
                        }
                    }

                    _DoLoopHandling = false;
                }

                List<string> list5 = null;
                if (num3 > 0 && !Ausblenden_VirtuelleFehler)
                {
                    List<string> list6 = null;
                    List<string> list7 = null;
                    List<string> list8 = null;
                    ParameterContainer parameterContainer7 = new ParameterContainer();
                    ParameterContainer parameterContainer8 = new ParameterContainer();
                    ParameterContainer parameterContainer9 = new ParameterContainer();
                    Factory.CreateServiceDialog(this, "DTC_ANZEIGE_DYN_Start", "52683531", _globalTabModuleISTA, 2697, parameterContainer7, parameterContainer9).Invoke("VirtuelleFehlerkodes", parameterContainer7, parameterContainer8, parameterContainer9);
                    if (parameterContainer8.getParameter("fehlerkode_NodeID") != null)
                    {
                        list5 = (List<string>)parameterContainer8.getParameter("fehlerkode_NodeID");
                    }

                    if (parameterContainer8.getParameter("fehlerkode_Text") != null)
                    {
                        list6 = (List<string>)parameterContainer8.getParameter("fehlerkode_Text");
                    }

                    if (parameterContainer8.getParameter("fehlerkode_hex") != null)
                    {
                        list7 = (List<string>)parameterContainer8.getParameter("fehlerkode_hex");
                    }

                    if (parameterContainer8.getParameter("fehlerkode_SGBD") != null)
                    {
                        list8 = (List<string>)parameterContainer8.getParameter("fehlerkode_SGBD");
                    }

                    _DoLoopHandling = true;
                    for (int j = 0; j < list5.Count; j++)
                    {
                        if (!FC_id_code.ContainsKey(list5[j]))
                        {
                            FC_id_code.Add(list5[j], list7[j]);
                            FC_id_text.Add(list5[j], list6[j]);
                            FC_id_sgbd.Add(list5[j], list8[j]);
                        }
                    }

                    _DoLoopHandling = false;
                }

                List<string> list9 = null;
                if (num4 > 0 && !Ausblenden_Sammelfehler)
                {
                    List<string> list10 = null;
                    List<string> list11 = null;
                    List<string> list12 = null;
                    ParameterContainer parameterContainer10 = new ParameterContainer();
                    ParameterContainer parameterContainer11 = new ParameterContainer();
                    ParameterContainer parameterContainer12 = new ParameterContainer();
                    Factory.CreateServiceDialog(this, "DTC_ANZEIGE_DYN_Start", "52683531", _globalTabModuleISTA, 2722, parameterContainer10, parameterContainer12).Invoke("Sammelfehlerkodes", parameterContainer10, parameterContainer11, parameterContainer12);
                    if (parameterContainer11.getParameter("fehlerkode_NodeID") != null)
                    {
                        list9 = (List<string>)parameterContainer11.getParameter("fehlerkode_NodeID");
                    }

                    if (parameterContainer11.getParameter("fehlerkode_Text") != null)
                    {
                        list10 = (List<string>)parameterContainer11.getParameter("fehlerkode_Text");
                    }

                    if (parameterContainer11.getParameter("fehlerkode_hex") != null)
                    {
                        list11 = (List<string>)parameterContainer11.getParameter("fehlerkode_hex");
                    }

                    if (parameterContainer11.getParameter("fehlerkode_SGBD") != null)
                    {
                        list12 = (List<string>)parameterContainer11.getParameter("fehlerkode_SGBD");
                    }

                    _DoLoopHandling = true;
                    for (int k = 0; k < list9.Count; k++)
                    {
                        if (!FC_id_code.ContainsKey(list9[k]))
                        {
                            FC_id_code.Add(list9[k], list11[k]);
                            FC_id_text.Add(list9[k], list10[k]);
                            FC_id_sgbd.Add(list9[k], list12[k]);
                        }
                    }

                    _DoLoopHandling = false;
                }

                ISPELocator iSPELocator = CalledFrom();
                ISPELocator[] array = null;
                if (iSPELocator != null)
                {
                    array = iSPELocator.GetIncomingLinks("SuspicionLink");
                }

                if (array != null && array.Length != 0)
                {
                    int num6 = array.Length;
                    _DoLoopHandling = true;
                    for (int l = 0; l < num6; l++)
                    {
                        string item = ((array[l] != null) ? array[l].Id : "null");
                        if (list != null && list.Contains(item))
                        {
                            akt_FS_Verdachte.AddIfNotContains(item);
                        }
                        else if (list5 != null && list5.Contains(item))
                        {
                            akt_virtFS_Verdachte.AddIfNotContains(item);
                        }
                        else if (list9 != null && list9.Contains(item))
                        {
                            akt_samFS_Verdachte.AddIfNotContains(item);
                        }
                    }

                    _DoLoopHandling = false;
                    akt_FS_Verdachte.Sort((string x, string y) => FC_id_code[x].CompareTo(FC_id_code[y]));
                    akt_virtFS_Verdachte.Sort((string x, string y) => FC_id_code[x].CompareTo(FC_id_code[y]));
                    akt_samFS_Verdachte.Sort((string x, string y) => FC_id_code[x].CompareTo(FC_id_code[y]));
                    _ = 0 + akt_FS_Verdachte.Count + akt_virtFS_Verdachte.Count;
                    _ = akt_samFS_Verdachte.Count;
                    List<ITextContent> list13 = new List<ITextContent>();
                    new List<ITextContent>();
                    int num7 = 0;
                    _DoLoopHandling = true;
                    foreach (string item2 in akt_virtFS_Verdachte)
                    {
                        akt_Verdachte.AddIfNotContains(item2);
                        list13.Add(__Text().TextContent);
                        list13[num7].Concat(FC_id_code[item2] + " " + FC_id_text[item2]);
                        num7++;
                    }

                    _DoLoopHandling = false;
                    _DoLoopHandling = true;
                    foreach (string item3 in akt_samFS_Verdachte)
                    {
                        akt_Verdachte.AddIfNotContains(item3);
                        list13.Add(__Text().TextContent);
                        list13[num7].Concat(FC_id_code[item3] + " " + FC_id_text[item3]);
                        num7++;
                    }

                    _DoLoopHandling = false;
                    _DoLoopHandling = true;
                    foreach (string item4 in akt_FS_Verdachte)
                    {
                        akt_Verdachte.AddIfNotContains(item4);
                        list13.Add(__Text().TextContent);
                        list13[num7].Concat(FC_id_code[item4] + " " + FC_id_text[item4]);
                        num7++;
                    }

                    _DoLoopHandling = false;
                    ITextLocator textLocator2 = ((Anzeige_Text_Anfang != null) ? __Text().Concat(Anzeige_Text_Anfang) : __Text());
                    ITextLocator textLocator3 = ((Anzeige_Text_Ende != null) ? __Text().Concat(Anzeige_Text_Ende) : __Text());
                    Model.PriorText = GetContent(textLocator2.TextContent);
                    UpdateFaultList(akt_Verdachte);
                    Model.PastText = GetContent(textLocator3.TextContent);
                    Model.SelectedIndex = -1;
                    if (akt_Verdachte != null && akt_Verdachte.Count > 0)
                    {
                        NavigateTo(ServiceDialogUI);
                        Panel.Forward.Enabled = true;
                        UserInterface.DisplayWaitCursor(bWaitCursor: false);
                        TrySelectFirstFault();
                        textLocator = __Text();
                        textLocator.Concat(Anzeige_Text_Anfang);
                        foreach (ITextContent item5 in list13)
                        {
                            textLocator.TextContent.Concat(item5);
                        }

                        textLocator.Concat(textLocator3);
                        WaitOnUserInteraction();
                    }
                    else
                    {
                        ResetScreenMode();
                    }

                    if (!m_bDisplayErrorDetailsCalled)
                    {
                        DisplayErrorDetails();
                    }

                    if (textLocator2 != null)
                    {
                        stringBuilder.Append(textLocator2.TextContent.PlainText);
                    }

                    foreach (string item6 in akt_virtFS_Verdachte)
                    {
                        stringBuilder.Append("<br/>");
                        stringBuilder.Append("- " + FC_id_sgbd[item6] + ":" + FC_id_code[item6] + " " + FC_id_text[item6]);
                    }

                    foreach (string item7 in akt_samFS_Verdachte)
                    {
                        stringBuilder.Append("<br/>");
                        stringBuilder.Append("- " + FC_id_code[item7] + " " + FC_id_text[item7]);
                    }

                    foreach (string item8 in akt_FS_Verdachte)
                    {
                        stringBuilder.Append("<br/>");
                        stringBuilder.Append("- " + FC_id_sgbd[item8] + ":" + FC_id_code[item8] + " " + FC_id_text[item8]);
                    }

                    stringBuilder.Append("<br/>");
                    if (textLocator3 != null)
                    {
                        stringBuilder.Append(textLocator3.TextContent.PlainText);
                    }

                    LogStatement("Test_Message", "Time", DateTime.UtcNow, "Dialog", "DTC_ANZEIGE_DYN", "Fehlerdaten", stringBuilder.ToString());
                    num5 = Model.SelectedIndex;
                }
                else
                {
                    ResetScreenMode();
                }
            }
            else
            {
                ResetScreenMode();
            }

            ITextLocator textLocator4 = __Text();
            if (num5 > -1)
            {
                string text = akt_Verdachte[num5];
                if (akt_samFS_Verdachte.Contains(text))
                {
                    textLocator4.TextContent.Concat(FC_id_code[text]);
                }
                else
                {
                    textLocator4.TextContent.Concat(FC_id_sgbd[text] + ":" + FC_id_code[text]);
                }

                LogStatement("Test_Message", "Time", DateTime.UtcNow, "Dialog", "DTC_ANZEIGE_DYN", "Fehlerdaten", stringBuilder.ToString(), "Benutzereingabe", textLocator4);
            }
            else
            {
                textLocator4.TextContent.Concat("-1");
                LogStatement("Test_Message", "Time", DateTime.UtcNow, "Dialog", "DTC_ANZEIGE_DYN", "Fehlerdaten", __Text("71523394571"), "Benutzereingabe", textLocator4.ToString());
            }

            SetupFASTAContainer(textLocator, textLocator4, "FKB_Anzeige", now, DateTime.Now);
            if (num5 > -1)
            {
                string text2 = akt_Verdachte[num5];
                Status_Fehlerkode_selektiert_hex = FC_id_code[text2];
                SGBD_Fehlerkode_selektiert = FC_id_sgbd[text2];
                if (akt_FS_Verdachte.Contains(text2))
                {
                    Status_Fehlerkode_selektiert_dez = Convert.ToInt32(Status_Fehlerkode_selektiert_hex, 16);
                    Status_Fehlerkode_selektiert_ID = text2;
                }
                else if (akt_samFS_Verdachte.Contains(text2))
                {
                    Status_Fehlerkode_selektiert_dez = 0;
                    Status_Fehlerkode_selektiert_ID = text2;
                }
                else if (akt_virtFS_Verdachte.Contains(text2))
                {
                    Status_Fehlerkode_selektiert_dez = 0;
                    Status_Fehlerkode_selektiert_ID = text2;
                }
                else
                {
                    Status_Fehlerkode_selektiert_dez = 0;
                    Status_Fehlerkode_selektiert_ID = null;
                }
            }
            else
            {
                Status_Fehlerkode_selektiert_hex = "";
                SGBD_Fehlerkode_selektiert = "";
                Status_Fehlerkode_selektiert_dez = -1;
                Status_Fehlerkode_selektiert_ID = null;
            }

            RemoveActualDocument();
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        private void SetupFASTAContainer(ITextLocator protokollFaultList, ITextLocator protokollAnswer, string dialogType, DateTime startTime, DateTime? endTime)
        {
            if (protokollFaultList == null || protokollFaultList.TextContent == null)
            {
                return;
            }

            if (FastaProtocoler != null)
            {
                IAction<IUiDialog> action = FastaProtocoler.CreateAndAddUiDialogFromServiceProgram(dialogType, LastCallingMethod);
                action.StartTime = startTime;
                if (endTime.HasValue)
                {
                    action.EndTime = endTime.Value;
                }

                IList<LocalizedText> textForUI = protokollFaultList.TextContent.GetTextForUI(logic.Lang);
                action.SpecialAction.CreateAndAddMessageText(textForUI);
                if (protokollAnswer != null)
                {
                    List<LocalizedText> list = new List<LocalizedText>();
                    list.AddRange(logic.Lang.Select((string x) => new LocalizedText(protokollAnswer.TextContent.FormattedText, x)));
                    action.SpecialAction.AddAnswer(list, null);
                }
            }
            else
            {
                Log.Warning("FKB_AnzeigeServiceDlg.SetupFASTAContainer()", "No FASTA available.");
            }
        }

        private void ShowDocument(string FC_Id)
        {
            int num = 0;
            Logger.WriteInformation("ShowDocument called");
            IDocumentLocator documentLocator = null;
            IDocumentLocator documentLocator2 = null;
            IDocumentLocator documentLocator3 = null;
            if (akt_FS_Verdachte.Contains(FC_Id))
            {
                IFaultCodeLocator faultCodeLocator = FaultCodeNode(FC_Id);
                ISPELocator obj = faultCodeLocator.Parents[0];
                ISPELocator iSPELocator = obj.Parents[0];
                string dataValue = obj.GetDataValue("Name");
                string dataValue2 = iSPELocator.GetDataValue("Name");
                string dataValue3 = faultCodeLocator.GetDataValue("Code");
                IFaultCodeLocator faultCodeLocator2 = FaultCodeNode(dataValue2, dataValue, dataValue3);
                string text = "Fehlerkodebeschreibung";
                text = "SYSTEM_CONTEXT";
                documentLocator3 = faultCodeLocator2.GetDocument(text);
                if (documentLocator3 != null)
                {
                    DocumentHandler(DocumentStatementAction.Add, documentLocator3, 2);
                }

                text = "FAULT_CODE_DETAILS";
                documentLocator2 = faultCodeLocator2.GetDocument(text);
                if (documentLocator2 != null)
                {
                    DocumentHandler(DocumentStatementAction.Add, documentLocator2, 1);
                }

                text = "Fehlerkodebeschreibung";
                documentLocator = faultCodeLocator2.GetDocument(text);
                if (documentLocator != null)
                {
                    DocumentHandler(DocumentStatementAction.Add, documentLocator, 0);
                }
            }

            if (akt_virtFS_Verdachte.Contains(FC_Id))
            {
                IVirtualFaultCodeLocator virtualFaultCodeLocator = VirtualFaultCodeNode(FC_Id);
                string docType = "Fehlerkodebeschreibung";
                documentLocator = virtualFaultCodeLocator.GetDocument(docType);
                if (documentLocator != null)
                {
                    DocumentHandler(DocumentStatementAction.Add, documentLocator, 0);
                }
            }

            if (akt_samFS_Verdachte.Contains(FC_Id))
            {
                ICombinedFaultLocator combinedFaultLocator = CombinedFaultNode(FC_Id);
                string docType2 = "Fehlerkodebeschreibung";
                documentLocator = combinedFaultLocator.GetDocument(docType2);
                if (documentLocator != null)
                {
                    DocumentHandler(DocumentStatementAction.Add, documentLocator, 0);
                }
            }

            RemoveActualDocument();
            if (documentLocator != null)
            {
                m_aktFkbDescriptionDocLocator = documentLocator;
            }

            if (documentLocator2 != null)
            {
                m_aktFkbDetailsDocLocator = documentLocator2;
            }

            if (documentLocator3 != null)
            {
                m_aktFkbSysContextDocLocator = documentLocator3;
            }

            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        private void RemoveActualDocument()
        {
            int num = 0;
            Logger.WriteInformation("RemoveActualDocument called");
            if (m_aktFkbSysContextDocLocator != null)
            {
                DocumentHandler(DocumentStatementAction.Remove, m_aktFkbSysContextDocLocator, 2);
                m_aktFkbSysContextDocLocator = null;
            }

            if (m_aktFkbDetailsDocLocator != null)
            {
                DocumentHandler(DocumentStatementAction.Remove, m_aktFkbDetailsDocLocator, 1);
                m_aktFkbDetailsDocLocator = null;
            }

            if (m_aktFkbDescriptionDocLocator != null)
            {
                DocumentHandler(DocumentStatementAction.Remove, m_aktFkbDescriptionDocLocator, 0);
                m_aktFkbDescriptionDocLocator = null;
            }

            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        private void DisplayErrorDetails()
        {
            int num = 0;
            Logger.WriteInformation("DisplayErrorDetails called");
            int selectedIndex = Model.SelectedIndex;
            if (selectedIndex < 0)
            {
                num = 0;
            }
            else
            {
                Panel.Forward.Enabled = false;
                ShowDocument(akt_Verdachte[selectedIndex]);
                Panel.Forward.Enabled = true;
            }

            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        private new string GetContent(ITextContent content)
        {
            string result = string.Empty;
            if (content is TextContent textContent)
            {
                result = textContent.GetTextForUI(Lang)[0].TextItem;
            }
            else
            {
                Log.Error("DtcAnzeigeDynImpl.GetContent()", "Couldn't retrieve text from textconent.");
            }

            return result;
        }

        protected override void SetMarkedInformation(FaultModelDtcDyn selectedFault)
        {
            decimal? num = null;
            if (!selectedFault.DTC.Id.HasValue)
            {
                num = selectedFault.DTC.F_ORT.GetValueOrDefault();
                if (num.HasValue)
                {
                    MarkedFaultCodes.AddIfNotContains(num.Value);
                }
            }
            else
            {
                base.SetMarkedInformation(selectedFault);
            }
        }
    }
}
