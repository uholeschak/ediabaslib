using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Core;
using PsdzClientLibrary;

namespace PsdzClient.Core
{
    public class VehicleLogistics
    {
        private class E46EcuCharacteristics { }
		private class E36EcuCharacteristics { }
		private class E39EcuCharacteristics { }
        private class E38EcuCharacteristics { }
        private class E52EcuCharacteristics { }
		private class E53EcuCharacteristics { }
        private class F01EcuCharacteristics { }
        private class F25_1404EcuCharacteristics { }
        private class F25EcuCharacteristics { }
        private class R50EcuCharacteristics { }
        private class RR6EcuCharacteristics { }
        private class R55EcuCharacteristics { }
        private class RR2EcuCharacteristics { }
        private class RREcuCharacteristics { }
        private class BNT_G11_G12_G3X_SP2015 { }
        private class MRXEcuCharacteristics { }
        private class MREcuCharacteristics { }
        private class E70EcuCharacteristicsAMPT { }
        private class E70EcuCharacteristicsAMPH { }
        private class E70EcuCharacteristics { }
        private class E60EcuCharacteristics { }
        private class E83EcuCharacteristics { }
        private class E85EcuCharacteristics { }
        private class F15EcuCharacteristics { }
        private class F01_1307EcuCharacteristics { }
        private class BNT_G01_G02_G08_F97_F98_SP2015 { }
        private class E89EcuCharacteristics { }
        private class F56EcuCharacteristics { }
        private class F20EcuCharacteristics { }

        private static uint ComputeStringHash(string s)
        {
            uint num = 0;
            if (s != null)
            {
                num = 2166136261U;
                for (int i = 0; i < s.Length; i++)
                {
                    num = ((uint)s[i] ^ num) * 16777619U;
                }
            }
            return num;
        }

		private static string GetEcuCharacteristics(string storedXmlFileName, Vehicle vecInfo)
        {
            return storedXmlFileName;
        }

        private static string GetEcuCharacteristics<T>(string storedXmlFileName, Vehicle vecInfo)
        {
            return storedXmlFileName;
        }

        private static string GetCharacteristicsName(Vehicle vecInfo)
		{
			int customHashCode = vecInfo.GetCustomHashCode();
			if (!string.IsNullOrEmpty(vecInfo.Ereihe))
			{
				string text = vecInfo.Ereihe.ToUpper();
				if (text != null)
				{
					uint num = ComputeStringHash(text);
					if (num <= 2275499142U)
					{
						if (num <= 1422189093U)
						{
							if (num <= 623919382U)
							{
								if (num <= 211557815U)
								{
									if (num <= 43781625U)
									{
										if (num <= 26856911U)
										{
											if (num != 10079292U)
											{
												if (num != 14853800U)
												{
													if (num != 26856911U)
													{
														goto IL_2379;
													}
													if (!(text == "K14"))
													{
														goto IL_2379;
													}
													goto IL_2239;
												}
												else
												{
													if (!(text == "M13"))
													{
														goto IL_2379;
													}
													goto IL_215E;
												}
											}
											else
											{
												if (!(text == "K15"))
												{
													goto IL_2379;
												}
												goto IL_2239;
											}
										}
										else if (num <= 31631419U)
										{
											if (num != 27004006U)
											{
												if (num != 31631419U)
												{
													goto IL_2379;
												}
												if (!(text == "M12"))
												{
													goto IL_2379;
												}
												goto IL_1EE9;
											}
											else
											{
												if (!(text == "K66"))
												{
													goto IL_2379;
												}
												goto IL_1C23;
											}
										}
										else if (num != 43634530U)
										{
											if (num != 43781625U)
											{
												goto IL_2379;
											}
											if (!(text == "K67"))
											{
												goto IL_2379;
											}
											goto IL_1C23;
										}
										else if (!(text == "K17"))
										{
											goto IL_2379;
										}
									}
									else if (num <= 77189768U)
									{
										if (num != 60412149U)
										{
											if (num != 60559244U)
											{
												if (num != 77189768U)
												{
													goto IL_2379;
												}
												if (!(text == "K19"))
												{
													goto IL_2379;
												}
												goto IL_1307;
											}
											else
											{
												if (!(text == "K60"))
												{
													goto IL_2379;
												}
												goto IL_1C23;
											}
										}
										else
										{
											if (!(text == "K16"))
											{
												goto IL_2379;
											}
											goto IL_2239;
										}
									}
									else if (num <= 93967387U)
									{
										if (num != 77336863U)
										{
											if (num != 93967387U)
											{
												goto IL_2379;
											}
											if (!(text == "K18"))
											{
												goto IL_2379;
											}
											goto IL_1307;
										}
										else
										{
											if (!(text == "K61"))
											{
												goto IL_2379;
											}
											goto IL_1C23;
										}
									}
									else if (num != 110892101U)
									{
										if (num != 211557815U)
										{
											goto IL_2379;
										}
										if (!(text == "K69"))
										{
											goto IL_2379;
										}
										goto IL_1C23;
									}
									else
									{
										if (!(text == "K63"))
										{
											goto IL_2379;
										}
										goto IL_1C23;
									}
								}
								else if (num <= 379039815U)
								{
									if (num <= 278374101U)
									{
										if (num != 261596482U)
										{
											if (num != 268518608U)
											{
												if (num != 278374101U)
												{
													goto IL_2379;
												}
												if (!(text == "K09"))
												{
													goto IL_2379;
												}
												goto IL_1C23;
											}
											else
											{
												if (text == "E39")
												{
													return VehicleLogistics.GetEcuCharacteristics<E39EcuCharacteristics>("BNT-XML-E39.xml", vecInfo);
												}
												goto IL_2379;
											}
										}
										else
										{
											if (!(text == "K08"))
											{
												goto IL_2379;
											}
											goto IL_1C23;
										}
									}
									else if (num <= 311929339U)
									{
										if (num != 285296227U)
										{
											if (num != 311929339U)
											{
												goto IL_2379;
											}
											if (!(text == "K07"))
											{
												goto IL_2379;
											}
										}
										else
										{
											if (text == "E38")
											{
												return VehicleLogistics.GetEcuCharacteristics<E38EcuCharacteristics>("BNT-XML-E38.xml", vecInfo);
											}
											goto IL_2379;
										}
									}
									else if (num != 362262196U)
									{
										if (num != 379039815U)
										{
											goto IL_2379;
										}
										if (!(text == "K03"))
										{
											goto IL_2379;
										}
										goto IL_1C23;
									}
									else
									{
										if (!(text == "K02"))
										{
											goto IL_2379;
										}
										goto IL_1C23;
									}
								}
								else if (num <= 570809940U)
								{
									if (num != 486774750U)
									{
										if (num != 520182893U)
										{
											if (num != 570809940U)
											{
												goto IL_2379;
											}
											if (text == "E53")
											{
												return VehicleLogistics.GetEcuCharacteristics<E53EcuCharacteristics>("BNT-XML-E53.xml", vecInfo);
											}
											goto IL_2379;
										}
										else
										{
											if (text == "E36")
											{
												return VehicleLogistics.GetEcuCharacteristics<E36EcuCharacteristics>("BNT-XML-E36.xml", vecInfo);
											}
											goto IL_2379;
										}
									}
									else
									{
										if (text == "E46")
										{
											return VehicleLogistics.GetEcuCharacteristics<E46EcuCharacteristics>("BNT-XML-E46.xml", vecInfo);
										}
										goto IL_2379;
									}
								}
								else if (num <= 593739736U)
								{
									if (num != 587587559U)
									{
										if (num != 593739736U)
										{
											goto IL_2379;
										}
										if (!(text == "V98"))
										{
											goto IL_2379;
										}
										goto IL_1744;
									}
									else
									{
										if (text == "E52")
										{
											return VehicleLogistics.GetEcuCharacteristics<E52EcuCharacteristics>("BNT-XML-E52.xml", vecInfo);
										}
										goto IL_2379;
									}
								}
								else if (num != 610517355U)
								{
									if (num != 623919382U)
									{
										goto IL_2379;
									}
									if (!(text == "I20"))
									{
										goto IL_2379;
									}
									if (vecInfo.HasMrr30())
									{
										return VehicleLogistics.GetEcuCharacteristics("BNT-XML-I20_FRS.xml", vecInfo);
									}
									if (vecInfo.HasFrr30v())
									{
										return VehicleLogistics.GetEcuCharacteristics("BNT-XML-I20_FRSF.xml", vecInfo);
									}
									return VehicleLogistics.GetEcuCharacteristics("BNT_I20.xml", vecInfo);
								}
								else
								{
									if (!(text == "V99"))
									{
										goto IL_2379;
									}
									goto IL_1C23;
								}
								return VehicleLogistics.GetEcuCharacteristics("MRKE01EcuCharacteristics.xml", vecInfo);
							}
							if (num <= 1338300998U)
							{
								if (num <= 976102286U)
								{
									if (num <= 786117595U)
									{
										if (num != 714685471U)
										{
											if (num != 752709452U)
											{
												if (num != 786117595U)
												{
													goto IL_2379;
												}
												if (!(text == "259"))
												{
													goto IL_2379;
												}
												goto IL_2239;
											}
											else
											{
												if (!(text == "247"))
												{
													goto IL_2379;
												}
												goto IL_2239;
											}
										}
										else
										{
											if (!(text == "K599"))
											{
												goto IL_2379;
											}
											goto IL_2239;
										}
									}
									else if (num <= 887303337U)
									{
										if (num != 870152785U)
										{
											if (num != 887303337U)
											{
												goto IL_2379;
											}
											if (!(text == "MRK24"))
											{
												goto IL_2379;
											}
											goto IL_1744;
										}
										else
										{
											if (!(text == "248"))
											{
												goto IL_2379;
											}
											goto IL_2239;
										}
									}
									else if (num != 969220989U)
									{
										if (num != 976102286U)
										{
											goto IL_2379;
										}
										if (!(text == "I15"))
										{
											goto IL_2379;
										}
									}
									else
									{
										if (text == "F01BN2K")
										{
											return VehicleLogistics.GetEcuCharacteristics<F01EcuCharacteristics>("BNT-XML-F01.xml", vecInfo);
										}
										goto IL_2379;
									}
								}
								else if (num <= 1276256427U)
								{
									if (num != 1026435143U)
									{
										if (num != 1259478808U)
										{
											if (num != 1276256427U)
											{
												goto IL_2379;
											}
											if (!(text == "259R"))
											{
												goto IL_2379;
											}
											goto IL_2239;
										}
										else
										{
											if (!(text == "259S"))
											{
												goto IL_2379;
											}
											goto IL_2239;
										}
									}
									else if (!(text == "I12"))
									{
										goto IL_2379;
									}
								}
								else if (num <= 1320793104U)
								{
									if (num != 1304745760U)
									{
										if (num != 1320793104U)
										{
											goto IL_2379;
										}
										if (!(text == "R50"))
										{
											goto IL_2379;
										}
										goto IL_734;
									}
									else
									{
										if (!(text == "F21"))
										{
											goto IL_2379;
										}
										goto IL_2255;
									}
								}
								else if (num != 1321523379U)
								{
									if (num != 1338300998U)
									{
										goto IL_2379;
									}
									if (!(text == "F23"))
									{
										goto IL_2379;
									}
									goto IL_2255;
								}
								else
								{
									if (!(text == "F20"))
									{
										goto IL_2379;
									}
									goto IL_2255;
								}
								return VehicleLogistics.GetEcuCharacteristics("BNT-XML-I12_I15.xml", vecInfo);
							}
							if (num <= 1372003331U)
							{
								if (num <= 1355225712U)
								{
									if (num != 1354348342U)
									{
										if (num != 1355078617U)
										{
											if (num != 1355225712U)
											{
												goto IL_2379;
											}
											if (!(text == "F54"))
											{
												goto IL_2379;
											}
											goto IL_215E;
										}
										else
										{
											if (!(text == "F22"))
											{
												goto IL_2379;
											}
											goto IL_2255;
										}
									}
									else if (!(text == "R52"))
									{
										goto IL_2379;
									}
								}
								else if (num <= 1371125961U)
								{
									if (num != 1368372283U)
									{
										if (num != 1371125961U)
										{
											goto IL_2379;
										}
										if (!(text == "R53"))
										{
											goto IL_2379;
										}
									}
									else
									{
										if (!(text == "A67"))
										{
											goto IL_2379;
										}
										goto IL_1744;
									}
								}
								else if (num != 1371856236U)
								{
									if (num != 1372003331U)
									{
										goto IL_2379;
									}
									if (!(text == "F55"))
									{
										goto IL_2379;
									}
									goto IL_215E;
								}
								else
								{
									if (!(text == "F25"))
									{
										goto IL_2379;
									}
									if (vecInfo.C_DATETIME != null && !(vecInfo.C_DATETIME < DiagnosticsBusinessData.DTimeF25Lci))
									{
										return VehicleLogistics.GetEcuCharacteristics<F25_1404EcuCharacteristics>("BNT-XML-F25_1404.xml", vecInfo);
									}
									return VehicleLogistics.GetEcuCharacteristics<F25EcuCharacteristics>("BNT-XML-F25.xml", vecInfo);
								}
							}
							else if (num <= 1405558569U)
							{
								if (num != 1388780950U)
								{
									if (num != 1404681199U)
									{
										if (num != 1405558569U)
										{
											goto IL_2379;
										}
										if (!(text == "F57"))
										{
											goto IL_2379;
										}
										goto IL_215E;
									}
									else
									{
										if (!(text == "R55"))
										{
											goto IL_2379;
										}
										goto IL_C77;
									}
								}
								else
								{
									if (!(text == "F56"))
									{
										goto IL_2379;
									}
									goto IL_215E;
								}
							}
							else if (num <= 1421379504U)
							{
								if (num != 1405705664U)
								{
									if (num != 1421379504U)
									{
										goto IL_2379;
									}
									if (!(text == "RR5"))
									{
										goto IL_2379;
									}
									goto IL_A3E;
								}
								else
								{
									if (!(text == "F47"))
									{
										goto IL_2379;
									}
									goto IL_215E;
								}
							}
							else if (num != 1421458818U)
							{
								if (num != 1422189093U)
								{
									goto IL_2379;
								}
								if (text == "F26")
								{
									return VehicleLogistics.GetEcuCharacteristics<F25_1404EcuCharacteristics>("BNT-XML-F25_1404.xml", vecInfo);
								}
								goto IL_2379;
							}
							else
							{
								if (!(text == "R56"))
								{
									goto IL_2379;
								}
								goto IL_C77;
							}
							IL_734:
							return VehicleLogistics.GetEcuCharacteristics<R50EcuCharacteristics>("BNT-XML-R50.xml", vecInfo);
						}
						else if (num <= 1713917564U)
						{
							if (num <= 1522045218U)
							{
								if (num <= 1456038521U)
								{
									if (num <= 1438236437U)
									{
										if (num != 1422483283U)
										{
											if (num != 1438157123U)
											{
												if (num != 1438236437U)
												{
													goto IL_2379;
												}
												if (!(text == "R57"))
												{
													goto IL_2379;
												}
												goto IL_C77;
											}
											else
											{
												if (!(text == "RR4"))
												{
													goto IL_2379;
												}
												goto IL_A3E;
											}
										}
										else
										{
											if (!(text == "F46"))
											{
												goto IL_2379;
											}
											goto IL_215E;
										}
									}
									else if (num <= 1455014056U)
									{
										if (num != 1439260902U)
										{
											if (num != 1455014056U)
											{
												goto IL_2379;
											}
											if (!(text == "R58"))
											{
												goto IL_2379;
											}
											goto IL_C77;
										}
										else
										{
											if (!(text == "F45"))
											{
												goto IL_2379;
											}
											goto IL_215E;
										}
									}
									else if (num != 1455891426U)
									{
										if (num != 1456038521U)
										{
											goto IL_2379;
										}
										if (!(text == "F44"))
										{
											goto IL_2379;
										}
									}
									else
									{
										if (!(text == "F52"))
										{
											goto IL_2379;
										}
										goto IL_215E;
									}
								}
								else if (num <= 1472924508U)
								{
									if (num != 1471712361U)
									{
										if (num != 1471791675U)
										{
											if (num != 1472924508U)
											{
												goto IL_2379;
											}
											if (!(text == "R21"))
											{
												goto IL_2379;
											}
											goto IL_2239;
										}
										else
										{
											if (!(text == "R59"))
											{
												goto IL_2379;
											}
											goto IL_C77;
										}
									}
									else
									{
										if (text == "RR6")
										{
											goto IL_A3E;
										}
										goto IL_2379;
									}
								}
								else if (num <= 1501132678U)
								{
									if (num != 1488489980U)
									{
										if (num != 1501132678U)
										{
											goto IL_2379;
										}
										if (text == "U11")
										{
											return VehicleLogistics.GetEcuCharacteristics("BNT_U06-Fallback.xml", vecInfo);
										}
										goto IL_2379;
									}
									else
									{
										if (!(text == "RR1"))
										{
											goto IL_2379;
										}
										goto IL_BE9;
									}
								}
								else if (num != 1517763202U)
								{
									if (num != 1522045218U)
									{
										goto IL_2379;
									}
									if (!(text == "RR3"))
									{
										goto IL_2379;
									}
									goto IL_BE9;
								}
								else
								{
									if (!(text == "U06"))
									{
										goto IL_2379;
									}
									if (vecInfo.HasMrr30())
									{
										if (!vecInfo.IsPhev())
										{
											return VehicleLogistics.GetEcuCharacteristics("BNT-XML-U06_FRS.xml", vecInfo);
										}
										return VehicleLogistics.GetEcuCharacteristics("BNT-XML-U06_PHEV_FRS.xml", vecInfo);
									}
									else
									{
										if (!vecInfo.HasFrr30v())
										{
											return VehicleLogistics.GetEcuCharacteristics("BNT_U06-Fallback.xml", vecInfo);
										}
										if (!vecInfo.IsPhev())
										{
											return VehicleLogistics.GetEcuCharacteristics("BNT-XML-U06_FRSF.xml", vecInfo);
										}
										return VehicleLogistics.GetEcuCharacteristics("BNT-XML-U06_PHEV_FRSF.xml", vecInfo);
									}
								}
							}
							else if (num <= 1623923079U)
							{
								if (num <= 1527920712U)
								{
									if (num != 1523148997U)
									{
										if (num != 1523257365U)
										{
											if (num != 1527920712U)
											{
												goto IL_2379;
											}
											if (!(text == "259C"))
											{
												goto IL_2379;
											}
											goto IL_2239;
										}
										else
										{
											if (!(text == "R22"))
											{
												goto IL_2379;
											}
											goto IL_2239;
										}
									}
									else if (!(text == "F40"))
									{
										goto IL_2379;
									}
								}
								else if (num <= 1606453912U)
								{
									if (num != 1538822837U)
									{
										if (num != 1606453912U)
										{
											goto IL_2379;
										}
										if (!(text == "R61"))
										{
											goto IL_2379;
										}
										goto IL_C77;
									}
									else
									{
										if (text == "RR2")
										{
											goto IL_BE9;
										}
										goto IL_2379;
									}
								}
								else if (num != 1623231531U)
								{
									if (num != 1623923079U)
									{
										goto IL_2379;
									}
									if (!(text == "R28"))
									{
										goto IL_2379;
									}
									goto IL_2239;
								}
								else
								{
									if (text == "R60")
									{
										goto IL_C77;
									}
									goto IL_2379;
								}
							}
							else if (num <= 1646807088U)
							{
								if (num != 1628586426U)
								{
									if (num != 1640592330U)
									{
										if (num != 1646807088U)
										{
											goto IL_2379;
										}
										if (!(text == "G02"))
										{
											goto IL_2379;
										}
										goto IL_1D6A;
									}
									else
									{
										if (!(text == "F49"))
										{
											goto IL_2379;
										}
										goto IL_215E;
									}
								}
								else
								{
									if (!(text == "259E"))
									{
										goto IL_2379;
									}
									goto IL_2239;
								}
							}
							else if (num <= 1697139945U)
							{
								if (num != 1657369949U)
								{
									if (num != 1697139945U)
									{
										goto IL_2379;
									}
									if (!(text == "G01"))
									{
										goto IL_2379;
									}
									goto IL_1D6A;
								}
								else
								{
									if (!(text == "F48"))
									{
										goto IL_2379;
									}
									goto IL_215E;
								}
							}
							else if (num != 1697287040U)
							{
								if (num != 1713917564U)
								{
									goto IL_2379;
								}
								if (!(text == "G06"))
								{
									goto IL_2379;
								}
								goto IL_1C98;
							}
							else
							{
								if (!(text == "G15"))
								{
									goto IL_2379;
								}
								goto IL_1BF3;
							}
							return VehicleLogistics.GetEcuCharacteristics("BNT-XML-F40_F44.xml", vecInfo);
							IL_BE9:
							if (vecInfo.C_DATETIME != null && !(vecInfo.C_DATETIME < DiagnosticsBusinessData.DTimeRR_S2))
							{
								return VehicleLogistics.GetEcuCharacteristics<RR2EcuCharacteristics>("BNT-XML-RR2.xml", vecInfo);
							}
							return VehicleLogistics.GetEcuCharacteristics<RREcuCharacteristics>("BNT-XML-RR.xml", vecInfo);
						}
						else
						{
							if (num <= 2022128458U)
							{
								if (num <= 1814730373U)
								{
									if (num <= 1747619897U)
									{
										if (num != 1714064659U)
										{
											if (num != 1730695183U)
											{
												if (num != 1747619897U)
												{
													goto IL_2379;
												}
												if (!(text == "G16"))
												{
													goto IL_2379;
												}
												goto IL_1BF3;
											}
											else
											{
												if (!(text == "G07"))
												{
													goto IL_2379;
												}
												goto IL_1C98;
											}
										}
										else
										{
											if (!(text == "G14"))
											{
												goto IL_2379;
											}
											goto IL_1BF3;
										}
									}
									else
									{
										if (num <= 1764397516U)
										{
											if (num != 1764250421U)
											{
												if (num != 1764397516U)
												{
													goto IL_2379;
												}
												if (!(text == "G11"))
												{
													goto IL_2379;
												}
											}
											else
											{
												if (!(text == "G05"))
												{
													goto IL_2379;
												}
												goto IL_1C98;
											}
										}
										else if (num != 1814583278U)
										{
											if (num != 1814730373U)
											{
												goto IL_2379;
											}
											if (!(text == "G12"))
											{
												goto IL_2379;
											}
										}
										else
										{
											if (!(text == "G08"))
											{
												goto IL_2379;
											}
											goto IL_1D6A;
										}
										if (vecInfo.HasHuMgu())
										{
											return VehicleLogistics.GetEcuCharacteristics("BNT-XML-G1X_G3X_SP2018_MGU.xml", vecInfo);
										}
										if (vecInfo.HasEnavevoOrNbtevo())
										{
											return VehicleLogistics.GetEcuCharacteristics<BNT_G11_G12_G3X_SP2015>("BNT-XML-G11_G12_G3X_SP2015.xml", vecInfo);
										}
										return VehicleLogistics.GetEcuCharacteristics("BNT_G1X_G3X_SP2018.xml", vecInfo);
									}
								}
								else if (num <= 1933306539U)
								{
									if (num != 1915396087U)
									{
										if (num != 1916528920U)
										{
											if (num != 1933306539U)
											{
												goto IL_2379;
											}
											if (!(text == "G83"))
											{
												goto IL_2379;
											}
											goto IL_2372;
										}
										else
										{
											if (!(text == "G82"))
											{
												goto IL_2379;
											}
											goto IL_2372;
										}
									}
									else
									{
										if (!(text == "G18"))
										{
											goto IL_2379;
										}
										goto IL_1C98;
									}
								}
								else if (num <= 1982212373U)
								{
									if (num != 1950084158U)
									{
										if (num != 1982212373U)
										{
											goto IL_2379;
										}
										if (!(text == "G38"))
										{
											goto IL_2379;
										}
									}
									else
									{
										if (!(text == "G80"))
										{
											goto IL_2379;
										}
										goto IL_2372;
									}
								}
								else if (num != 2005350839U)
								{
									if (num != 2022128458U)
									{
										goto IL_2379;
									}
									if (!(text == "RR12"))
									{
										goto IL_2379;
									}
									goto IL_2286;
								}
								else
								{
									if (!(text == "RR11"))
									{
										goto IL_2379;
									}
									goto IL_2286;
								}
							}
							else if (num <= 2224180547U)
							{
								if (num <= 2099655706U)
								{
									if (num != 2033411980U)
									{
										if (num != 2082878087U)
										{
											if (num != 2099655706U)
											{
												goto IL_2379;
											}
											if (!(text == "G31"))
											{
												goto IL_2379;
											}
										}
										else if (!(text == "G32"))
										{
											goto IL_2379;
										}
									}
									else
									{
										if (text == "J29")
										{
											return VehicleLogistics.GetEcuCharacteristics("BNT-XML-J29.xml", vecInfo);
										}
										goto IL_2379;
									}
								}
								else if (num <= 2206417190U)
								{
									if (num != 2116433325U)
									{
										if (num != 2206417190U)
										{
											goto IL_2379;
										}
										if (!(text == "K84"))
										{
											goto IL_2379;
										}
										goto IL_1C23;
									}
									else if (!(text == "G30"))
									{
										goto IL_2379;
									}
								}
								else if (num != 2207402928U)
								{
									if (num != 2224180547U)
									{
										goto IL_2379;
									}
									if (!(text == "K29"))
									{
										goto IL_2379;
									}
									goto IL_1744;
								}
								else
								{
									if (!(text == "K28"))
									{
										goto IL_2379;
									}
									goto IL_1744;
								}
							}
							else if (num <= 2256750047U)
							{
								if (num != 2225019190U)
								{
									if (num != 2239972428U)
									{
										if (num != 2256750047U)
										{
											goto IL_2379;
										}
										if (!(text == "K83"))
										{
											goto IL_2379;
										}
										goto IL_1C23;
									}
									else
									{
										if (!(text == "K82"))
										{
											goto IL_2379;
										}
										goto IL_1C23;
									}
								}
								else
								{
									if (!(text == "K75"))
									{
										goto IL_2379;
									}
									goto IL_1744;
								}
							}
							else if (num <= 2273527666U)
							{
								if (num != 2258574428U)
								{
									if (num != 2273527666U)
									{
										goto IL_2379;
									}
									if (!(text == "K80"))
									{
										goto IL_2379;
									}
									goto IL_1C23;
								}
								else
								{
									if (!(text == "K73"))
									{
										goto IL_2379;
									}
									goto IL_1744;
								}
							}
							else if (num != 2275352047U)
							{
								if (num != 2275499142U)
								{
									goto IL_2379;
								}
								if (!(text == "K48"))
								{
									goto IL_2379;
								}
								goto IL_1C23;
							}
							else
							{
								if (!(text == "K72"))
								{
									goto IL_2379;
								}
								goto IL_1744;
							}
							if (vecInfo.HasEnavevoOrNbtevo())
							{
								return VehicleLogistics.GetEcuCharacteristics("BNT-XML-G1X_G3X_SP2018_NOMGU.xml", vecInfo);
							}
							if (vecInfo.HasHuMgu())
							{
								return VehicleLogistics.GetEcuCharacteristics("BNT-XML-G1X_G3X_SP2018_MGU.xml", vecInfo);
							}
							return VehicleLogistics.GetEcuCharacteristics("BNT_G1X_G3X_SP2018.xml", vecInfo);
						}
						IL_A3E:
						return VehicleLogistics.GetEcuCharacteristics<RR6EcuCharacteristics>("BNT-XML-RR6.xml", vecInfo);
						IL_C77:
						return VehicleLogistics.GetEcuCharacteristics<R55EcuCharacteristics>("BNT-XML-R55.xml", vecInfo);
					}
					if (num <= 2845166379U)
					{
						if (num <= 2443275332U)
						{
							if (num > 2392103832U)
							{
								if (num <= 2416642220U)
								{
									if (num <= 2400011696U)
									{
										if (num != 2392942475U)
										{
											if (num != 2399717506U)
											{
												if (num != 2400011696U)
												{
													goto IL_2379;
												}
												if (!(text == "E88"))
												{
													goto IL_2379;
												}
												goto IL_1EE9;
											}
											else
											{
												if (text == "E64")
												{
													goto IL_1403;
												}
												goto IL_2379;
											}
										}
										else
										{
											if (!(text == "K43"))
											{
												goto IL_2379;
											}
											goto IL_1744;
										}
									}
									else if (num <= 2409720094U)
									{
										if (num != 2408881451U)
										{
											if (num != 2409720094U)
											{
												goto IL_2379;
											}
											if (!(text == "K40"))
											{
												goto IL_2379;
											}
											goto IL_1744;
										}
										else
										{
											if (!(text == "K32"))
											{
												goto IL_2379;
											}
											goto IL_1C23;
										}
									}
									else if (num != 2416495125U)
									{
										if (num != 2416642220U)
										{
											goto IL_2379;
										}
										if (!(text == "E71"))
										{
											goto IL_2379;
										}
									}
									else
									{
										if (!(text == "E65"))
										{
											goto IL_2379;
										}
										goto IL_16FE;
									}
								}
								else if (num <= 2426497713U)
								{
									if (num != 2416789315U)
									{
										if (num != 2425511975U)
										{
											if (num != 2426497713U)
											{
												goto IL_2379;
											}
											if (!(text == "K41"))
											{
												goto IL_2379;
											}
											goto IL_2239;
										}
										else
										{
											if (!(text == "K25"))
											{
												goto IL_2379;
											}
											goto IL_1744;
										}
									}
									else
									{
										if (!(text == "E89"))
										{
											goto IL_2379;
										}
										goto IL_1EE9;
									}
								}
								else if (num <= 2442289594U)
								{
									if (num != 2433419839U)
									{
										if (num != 2442289594U)
										{
											goto IL_2379;
										}
										if (!(text == "K26"))
										{
											goto IL_2379;
										}
										goto IL_1744;
									}
									else if (!(text == "E70"))
									{
										goto IL_2379;
									}
								}
								else if (num != 2442436689U)
								{
									if (num != 2443275332U)
									{
										goto IL_2379;
									}
									if (!(text == "K46"))
									{
										goto IL_2379;
									}
									if (VehicleLogistics.getBNType(vecInfo) == BNType.BN2020_MOTORBIKE)
									{
										return VehicleLogistics.GetEcuCharacteristics<MRXEcuCharacteristics>("BNT-XML-BIKE-K001.xml", vecInfo);
									}
									return VehicleLogistics.GetEcuCharacteristics<MREcuCharacteristics>("MREcuCharacteristics.xml", vecInfo);
								}
								else
								{
									if (!(text == "K30"))
									{
										goto IL_2379;
									}
									goto IL_2239;
								}
								ECU ecu = vecInfo.ECU.FirstOrDefault((ECU x) => "AMPT70".Equals(x.ECU_SGBD, StringComparison.OrdinalIgnoreCase) || "AMPT07".Equals(x.ECU_SGBD, StringComparison.OrdinalIgnoreCase) || "AMPH07".Equals(x.ECU_SGBD, StringComparison.OrdinalIgnoreCase) || "AMPH70".Equals(x.ECU_SGBD, StringComparison.OrdinalIgnoreCase));
								if (ecu != null && ecu.ECU_SGBD != null)
								{
									if (ecu.ECU_SGBD.Contains("AMPT70") || ecu.ECU_SGBD.Contains("AMPT07"))
									{
										return VehicleLogistics.GetEcuCharacteristics<E70EcuCharacteristicsAMPT>("BNT-XML-E70-AMPT70-AMPT07.xml", vecInfo);
									}
									if (ecu.ECU_SGBD.Contains("AMPH07") || ecu.ECU_SGBD.Contains("AMPH70"))
									{
										return VehicleLogistics.GetEcuCharacteristics<E70EcuCharacteristicsAMPH>("BNT-XML-E70-AMPH70-AMPH07.xml", vecInfo);
									}
								}
								return VehicleLogistics.GetEcuCharacteristics<E70EcuCharacteristics>("BNT-XML-E70_NOAMPT_NOAMPH.xml", vecInfo);
							}
							if (num <= 2349384649U)
							{
								if (num <= 2292276761U)
								{
									if (num != 2290305285U)
									{
										if (num != 2292129666U)
										{
											if (num != 2292276761U)
											{
												goto IL_2379;
											}
											if (!(text == "K49"))
											{
												goto IL_2379;
											}
											goto IL_1C23;
										}
										else
										{
											if (!(text == "K71"))
											{
												goto IL_2379;
											}
											goto IL_1744;
										}
									}
									else
									{
										if (!(text == "K81"))
										{
											goto IL_2379;
										}
										goto IL_1C23;
									}
								}
								else if (num <= 2315829411U)
								{
									if (num != 2308907285U)
									{
										if (num != 2315829411U)
										{
											goto IL_2379;
										}
										if (!(text == "E63"))
										{
											goto IL_2379;
										}
									}
									else
									{
										if (!(text == "K70"))
										{
											goto IL_2379;
										}
										goto IL_1744;
									}
								}
								else if (num != 2332607030U)
								{
									if (num != 2349384649U)
									{
										goto IL_2379;
									}
									if (!(text == "E61"))
									{
										goto IL_2379;
									}
								}
								else if (!(text == "E60"))
								{
									goto IL_2379;
								}
							}
							else if (num <= 2375179118U)
							{
								if (num != 2358401499U)
								{
									if (num != 2366162268U)
									{
										if (num != 2375179118U)
										{
											goto IL_2379;
										}
										if (!(text == "K22"))
										{
											goto IL_2379;
										}
										goto IL_1C23;
									}
									else
									{
										if (!(text == "E66"))
										{
											goto IL_2379;
										}
										goto IL_16FE;
									}
								}
								else
								{
									if (text == "K21")
									{
										goto IL_1307;
									}
									goto IL_2379;
								}
							}
							else if (num <= 2382939887U)
							{
								if (num != 2376164856U)
								{
									if (num != 2382939887U)
									{
										goto IL_2379;
									}
									if (!(text == "E67"))
									{
										goto IL_2379;
									}
									goto IL_16FE;
								}
								else
								{
									if (!(text == "K42"))
									{
										goto IL_2379;
									}
									goto IL_1744;
								}
							}
							else if (num != 2391956737U)
							{
								if (num != 2392103832U)
								{
									goto IL_2379;
								}
								if (!(text == "K33"))
								{
									goto IL_2379;
								}
								goto IL_1C23;
							}
							else
							{
								if (!(text == "K23"))
								{
									goto IL_2379;
								}
								goto IL_1C23;
							}
							IL_1403:
							return VehicleLogistics.GetEcuCharacteristics<E60EcuCharacteristics>("BNT-XML-E60.xml", vecInfo);
						}
						if (num > 2551010267U)
						{
							if (num <= 2618267838U)
							{
								if (num <= 2584712600U)
								{
									if (num != 2567787886U)
									{
										if (num != 2584565505U)
										{
											if (num != 2584712600U)
											{
												goto IL_2379;
											}
											if (!(text == "E93"))
											{
												goto IL_2379;
											}
											goto IL_1EE9;
										}
										else
										{
											if (text == "E83")
											{
												return VehicleLogistics.GetEcuCharacteristics<E83EcuCharacteristics>("BNT-XML-E83.xml", vecInfo);
											}
											goto IL_2379;
										}
									}
									else
									{
										if (!(text == "E82"))
										{
											goto IL_2379;
										}
										goto IL_1EE9;
									}
								}
								else if (num <= 2601490219U)
								{
									if (num != 2601343124U)
									{
										if (num != 2601490219U)
										{
											goto IL_2379;
										}
										if (!(text == "E92"))
										{
											goto IL_2379;
										}
										goto IL_1EE9;
									}
									else
									{
										if (!(text == "E84"))
										{
											goto IL_2379;
										}
										goto IL_1EE9;
									}
								}
								else if (num != 2618120743U)
								{
									if (num != 2618267838U)
									{
										goto IL_2379;
									}
									if (!(text == "E91"))
									{
										goto IL_2379;
									}
									goto IL_1EE9;
								}
								else if (!(text == "E85"))
								{
									goto IL_2379;
								}
							}
							else if (num <= 2651675981U)
							{
								if (num != 2634898362U)
								{
									if (num != 2635045457U)
									{
										if (num != 2651675981U)
										{
											goto IL_2379;
										}
										if (!(text == "E87"))
										{
											goto IL_2379;
										}
										goto IL_1EE9;
									}
									else
									{
										if (!(text == "E90"))
										{
											goto IL_2379;
										}
										goto IL_1EE9;
									}
								}
								else if (!(text == "E86"))
								{
									goto IL_2379;
								}
							}
							else if (num <= 2729132584U)
							{
								if (num != 2691943669U)
								{
									if (num != 2729132584U)
									{
										goto IL_2379;
									}
									if (!(text == "K569"))
									{
										goto IL_2379;
									}
									goto IL_2239;
								}
								else
								{
									if (!(text == "C01"))
									{
										goto IL_2379;
									}
									goto IL_2239;
								}
							}
							else if (num != 2731831713U)
							{
								if (num != 2845166379U)
								{
									goto IL_2379;
								}
								if (!(text == "247E"))
								{
									goto IL_2379;
								}
								goto IL_2239;
							}
							else
							{
								if (text == "H91")
								{
									goto IL_1A3B;
								}
								goto IL_2379;
							}
							return VehicleLogistics.GetEcuCharacteristics<E85EcuCharacteristics>("BNT-XML-E85.xml", vecInfo);
						}
						if (num <= 2476830570U)
						{
							if (num <= 2460052951U)
							{
								if (num != 2443422427U)
								{
									if (num != 2459067213U)
									{
										if (num != 2460052951U)
										{
											goto IL_2379;
										}
										if (!(text == "K47"))
										{
											goto IL_2379;
										}
										goto IL_1C23;
									}
									else
									{
										if (!(text == "K27"))
										{
											goto IL_2379;
										}
										goto IL_1744;
									}
								}
								else
								{
									if (!(text == "K54"))
									{
										goto IL_2379;
									}
									goto IL_1C23;
								}
							}
							else if (num <= 2466827982U)
							{
								if (num != 2461271238U)
								{
									if (num != 2466827982U)
									{
										goto IL_2379;
									}
									if (text == "E68")
									{
										goto IL_16FE;
									}
									goto IL_2379;
								}
								else if (!(text == "H61"))
								{
									goto IL_2379;
								}
							}
							else if (num != 2466975077U)
							{
								if (num != 2476830570U)
								{
									goto IL_2379;
								}
								if (text == "K44")
								{
									goto IL_1744;
								}
								goto IL_2379;
							}
							else
							{
								if (text == "E72")
								{
									return VehicleLogistics.GetEcuCharacteristics("BNT-XML-E72.xml", vecInfo);
								}
								goto IL_2379;
							}
						}
						else if (num <= 2509547165U)
						{
							if (num != 2492769546U)
							{
								if (num != 2493755284U)
								{
									if (num != 2509547165U)
									{
										goto IL_2379;
									}
									if (!(text == "K34"))
									{
										goto IL_2379;
									}
									goto IL_1C23;
								}
								else
								{
									if (!(text == "K51"))
									{
										goto IL_2379;
									}
									goto IL_1C23;
								}
							}
							else
							{
								if (!(text == "K35"))
								{
									goto IL_2379;
								}
								goto IL_1C23;
							}
						}
						else if (num <= 2527310522U)
						{
							if (num != 2510532903U)
							{
								if (num != 2527310522U)
								{
									goto IL_2379;
								}
								if (!(text == "K53"))
								{
									goto IL_2379;
								}
								goto IL_1C23;
							}
							else
							{
								if (!(text == "K50"))
								{
									goto IL_2379;
								}
								goto IL_1C23;
							}
						}
						else if (num != 2544088141U)
						{
							if (num != 2551010267U)
							{
								goto IL_2379;
							}
							if (!(text == "E81"))
							{
								goto IL_2379;
							}
							goto IL_1EE9;
						}
						else
						{
							if (!(text == "K52"))
							{
								goto IL_2379;
							}
							goto IL_1C23;
						}
						IL_1A3B:
						return VehicleLogistics.GetEcuCharacteristics("H61EcuCharacteristics.xml", vecInfo);
						IL_16FE:
						return VehicleLogistics.GetEcuCharacteristics("BNT-XML-E65.xml", vecInfo);
					}
					if (num <= 3770511300U)
					{
						if (num <= 3534344706U)
						{
							if (num <= 3467234230U)
							{
								if (num <= 3233663528U)
								{
									if (num != 2929478274U)
									{
										if (num != 3123387255U)
										{
											if (num != 3233663528U)
											{
												goto IL_2379;
											}
											if (!(text == "E189"))
											{
												goto IL_2379;
											}
											goto IL_2239;
										}
										else
										{
											if (text == "I01")
											{
												return VehicleLogistics.GetEcuCharacteristics("BNT-XML-I01.xml", vecInfo);
											}
											goto IL_2379;
										}
									}
									else
									{
										if (!(text == "K589"))
										{
											goto IL_2379;
										}
										goto IL_2239;
									}
								}
								else if (num <= 3434009218U)
								{
									if (num != 3433678992U)
									{
										if (num != 3434009218U)
										{
											goto IL_2379;
										}
										if (!(text == "E169"))
										{
											goto IL_2379;
										}
										goto IL_2239;
									}
									else
									{
										if (!(text == "F90"))
										{
											goto IL_2379;
										}
										if (vecInfo.HasHuMgu())
										{
											return VehicleLogistics.GetEcuCharacteristics("BNT-XML-G1X_G3X_SP2018_MGU.xml", vecInfo);
										}
										if (vecInfo.ECU.Any((ECU x) => "NBTEVO".Equals(x.ECU_SGBD, StringComparison.OrdinalIgnoreCase)))
										{
											return VehicleLogistics.GetEcuCharacteristics<BNT_G11_G12_G3X_SP2015>("BNT-XML-G11_G12_G3X_SP2015.xml", vecInfo);
										}
										return VehicleLogistics.GetEcuCharacteristics("BNT_G1X_G3X_SP2018.xml", vecInfo);
									}
								}
								else if (num != 3450456611U)
								{
									if (num != 3467234230U)
									{
										goto IL_2379;
									}
									if (!(text == "F92"))
									{
										goto IL_2379;
									}
									goto IL_1BF3;
								}
								else
								{
									if (!(text == "F91"))
									{
										goto IL_2379;
									}
									goto IL_1BF3;
								}
							}
							else if (num <= 3484158944U)
							{
								if (num != 3471543152U)
								{
									if (num != 3484011849U)
									{
										if (num != 3484158944U)
										{
											goto IL_2379;
										}
										if (!(text == "F83"))
										{
											goto IL_2379;
										}
										goto IL_2255;
									}
									else
									{
										if (text == "F93")
										{
											goto IL_1BF3;
										}
										goto IL_2379;
									}
								}
								else
								{
									if (text == "X_K001")
									{
										goto IL_1C23;
									}
									goto IL_2379;
								}
							}
							else if (num <= 3517567087U)
							{
								if (num != 3500936563U)
								{
									if (num != 3517567087U)
									{
										goto IL_2379;
									}
									if (!(text == "F95"))
									{
										goto IL_2379;
									}
									goto IL_1C98;
								}
								else
								{
									if (!(text == "F82"))
									{
										goto IL_2379;
									}
									goto IL_2255;
								}
							}
							else if (num != 3517714182U)
							{
								if (num != 3534344706U)
								{
									goto IL_2379;
								}
								if (text == "F96")
								{
									goto IL_1C98;
								}
								goto IL_2379;
							}
							else
							{
								if (!(text == "F81"))
								{
									goto IL_2379;
								}
								goto IL_2255;
							}
						}
						else if (num <= 3584824658U)
						{
							if (num <= 3551269420U)
							{
								if (num != 3534491801U)
								{
									if (num != 3551122325U)
									{
										if (num != 3551269420U)
										{
											goto IL_2379;
										}
										if (!(text == "F87"))
										{
											goto IL_2379;
										}
										goto IL_2255;
									}
									else
									{
										if (!(text == "F97"))
										{
											goto IL_2379;
										}
										goto IL_1D6A;
									}
								}
								else
								{
									if (!(text == "F80"))
									{
										goto IL_2379;
									}
									goto IL_2255;
								}
							}
							else if (num <= 3568047039U)
							{
								if (num != 3567899944U)
								{
									if (num != 3568047039U)
									{
										goto IL_2379;
									}
									if (!(text == "F86"))
									{
										goto IL_2379;
									}
								}
								else
								{
									if (text == "F98")
									{
										goto IL_1D6A;
									}
									goto IL_2379;
								}
							}
							else if (num != 3569179872U)
							{
								if (num != 3584824658U)
								{
									goto IL_2379;
								}
								if (!(text == "F85"))
								{
									goto IL_2379;
								}
							}
							else
							{
								if (!(text == "F18"))
								{
									goto IL_2379;
								}
								goto IL_208A;
							}
						}
						else if (num <= 3736956062U)
						{
							if (num != 3703400824U)
							{
								if (num != 3720178443U)
								{
									if (num != 3736956062U)
									{
										goto IL_2379;
									}
									if (!(text == "F12"))
									{
										goto IL_2379;
									}
									goto IL_208A;
								}
								else
								{
									if (!(text == "F11"))
									{
										goto IL_2379;
									}
									goto IL_208A;
								}
							}
							else
							{
								if (!(text == "F10"))
								{
									goto IL_2379;
								}
								goto IL_208A;
							}
						}
						else if (num <= 3753880776U)
						{
							if (num != 3753733681U)
							{
								if (num != 3753880776U)
								{
									goto IL_2379;
								}
								if (!(text == "F03"))
								{
									goto IL_2379;
								}
								goto IL_208A;
							}
							else
							{
								if (!(text == "F13"))
								{
									goto IL_2379;
								}
								goto IL_208A;
							}
						}
						else if (num != 3756453761U)
						{
							if (num != 3770511300U)
							{
								goto IL_2379;
							}
							if (!(text == "F14"))
							{
								goto IL_2379;
							}
						}
						else
						{
							if (text == "E89X")
							{
								goto IL_1EE9;
							}
							goto IL_2379;
						}
					}
					else if (num <= 3872309847U)
					{
						if (num <= 3817627881U)
						{
							if (num <= 3787436014U)
							{
								if (num != 3770658395U)
								{
									if (num != 3787288919U)
									{
										if (num != 3787436014U)
										{
											goto IL_2379;
										}
										if (!(text == "F01"))
										{
											goto IL_2379;
										}
										goto IL_208A;
									}
									else if (!(text == "F15"))
									{
										goto IL_2379;
									}
								}
								else
								{
									if (!(text == "F02"))
									{
										goto IL_2379;
									}
									goto IL_208A;
								}
							}
							else if (num <= 3804066538U)
							{
								if (num != 3787583109U)
								{
									if (num != 3804066538U)
									{
										goto IL_2379;
									}
									if (!(text == "F16"))
									{
										goto IL_2379;
									}
								}
								else
								{
									if (!(text == "F39"))
									{
										goto IL_2379;
									}
									goto IL_215E;
								}
							}
							else if (num != 3804360728U)
							{
								if (num != 3817627881U)
								{
									goto IL_2379;
								}
								if (!(text == "RR31"))
								{
									goto IL_2379;
								}
								goto IL_2286;
							}
							else
							{
								if (!(text == "F36"))
								{
									goto IL_2379;
								}
								goto IL_2255;
							}
						}
						else if (num <= 3837915966U)
						{
							if (num != 3820991252U)
							{
								if (num != 3837768871U)
								{
									if (num != 3837915966U)
									{
										goto IL_2379;
									}
									if (!(text == "F34"))
									{
										goto IL_2379;
									}
									goto IL_2255;
								}
								else
								{
									if (!(text == "F06"))
									{
										goto IL_2379;
									}
									goto IL_208A;
								}
							}
							else
							{
								if (!(text == "F07"))
								{
									goto IL_2379;
								}
								goto IL_208A;
							}
						}
						else if (num <= 3871324109U)
						{
							if (num != 3854693585U)
							{
								if (num != 3871324109U)
								{
									goto IL_2379;
								}
								if (text == "F04")
								{
									goto IL_208A;
								}
								goto IL_2379;
							}
							else
							{
								if (!(text == "F35"))
								{
									goto IL_2379;
								}
								goto IL_2255;
							}
						}
						else if (num != 3871471204U)
						{
							if (num != 3872309847U)
							{
								goto IL_2379;
							}
							if (text == "F60")
							{
								goto IL_215E;
							}
							goto IL_2379;
						}
						else
						{
							if (!(text == "F32"))
							{
								goto IL_2379;
							}
							goto IL_2255;
						}
					}
					else if (num <= 3935218309U)
					{
						if (num <= 3905026442U)
						{
							if (num != 3884885452U)
							{
								if (num != 3888248823U)
								{
									if (num != 3905026442U)
									{
										goto IL_2379;
									}
									if (!(text == "F30"))
									{
										goto IL_2379;
									}
									goto IL_2255;
								}
								else
								{
									if (!(text == "F33"))
									{
										goto IL_2379;
									}
									goto IL_2255;
								}
							}
							else
							{
								if (!(text == "RR21"))
								{
									goto IL_2379;
								}
								goto IL_2286;
							}
						}
						else if (num <= 3921912429U)
						{
							if (num != 3921804061U)
							{
								if (num != 3921912429U)
								{
									goto IL_2379;
								}
								if (text == "R13")
								{
									goto IL_2239;
								}
								goto IL_2379;
							}
							else
							{
								if (text == "F31")
								{
									goto IL_2255;
								}
								goto IL_2379;
							}
						}
						else if (num != 3927871724U)
						{
							if (num != 3935218309U)
							{
								goto IL_2379;
							}
							if (text == "RR22")
							{
								goto IL_2286;
							}
							goto IL_2379;
						}
						else
						{
							if (!(text == "G42"))
							{
								goto IL_2379;
							}
							goto IL_2372;
						}
					}
					else if (num <= 4129497342U)
					{
						if (num != 4028831628U)
						{
							if (num != 4045609247U)
							{
								if (num != 4129497342U)
								{
									goto IL_2379;
								}
								if (!(text == "G26"))
								{
									goto IL_2379;
								}
								goto IL_2372;
							}
							else
							{
								if (text == "G29")
								{
									return VehicleLogistics.GetEcuCharacteristics("BNT-XML-G29.xml", vecInfo);
								}
								goto IL_2379;
							}
						}
						else
						{
							if (!(text == "G28"))
							{
								goto IL_2379;
							}
							goto IL_2372;
						}
					}
					else if (num <= 4179830199U)
					{
						if (num != 4163052580U)
						{
							if (num != 4179830199U)
							{
								goto IL_2379;
							}
							if (!(text == "G21"))
							{
								goto IL_2379;
							}
							goto IL_2372;
						}
						else
						{
							if (!(text == "G20"))
							{
								goto IL_2379;
							}
							goto IL_2372;
						}
					}
					else if (num != 4196607818U)
					{
						if (num != 4213385437U)
						{
							goto IL_2379;
						}
						if (!(text == "G23"))
						{
							goto IL_2379;
						}
						goto IL_2372;
					}
					else
					{
						if (text == "G22")
						{
							goto IL_2372;
						}
						goto IL_2379;
					}
					return VehicleLogistics.GetEcuCharacteristics<F15EcuCharacteristics>("BNT-XML-F15.xml", vecInfo);
					IL_208A:
					if (vecInfo.C_DATETIME != null && !(vecInfo.C_DATETIME < DiagnosticsBusinessData.DTimeF01Lci))
					{
						if (vecInfo.ECU != null)
						{
							ECU ecu2 = vecInfo.getECU(new long?(16L));
							if (ecu2 != null && ecu2.SubBUS != null && ecu2.SubBUS.Contains(BusType.MOST))
							{
								return VehicleLogistics.GetEcuCharacteristics<F01EcuCharacteristics>("BNT-XML-F01.xml", vecInfo);
							}
						}
						return VehicleLogistics.GetEcuCharacteristics<F01_1307EcuCharacteristics>("BNT-XML-F01_1307.xml", vecInfo);
					}
					return VehicleLogistics.GetEcuCharacteristics<F01EcuCharacteristics>("BNT-XML-F01.xml", vecInfo);
					IL_1307:
					if (VehicleLogistics.getBNType(vecInfo) == BNType.BN2000_MOTORBIKE)
					{
						return VehicleLogistics.GetEcuCharacteristics<MREcuCharacteristics>("MREcuCharacteristics.xml", vecInfo);
					}
					return VehicleLogistics.GetEcuCharacteristics<MRXEcuCharacteristics>("BNT-XML-BIKE-K001.xml", vecInfo);
					IL_1744:
					return VehicleLogistics.GetEcuCharacteristics<MREcuCharacteristics>("MREcuCharacteristics.xml", vecInfo);
					IL_1BF3:
					if (vecInfo.HasHuMgu())
					{
						return VehicleLogistics.GetEcuCharacteristics("BNT-XML-G1X_G3X_SP2018_MGU.xml", vecInfo);
					}
					return VehicleLogistics.GetEcuCharacteristics("BNT_G1X_G3X_SP2018.xml", vecInfo);
					IL_1C23:
					return VehicleLogistics.GetEcuCharacteristics<MRXEcuCharacteristics>("BNT-XML-BIKE-K001.xml", vecInfo);
					IL_1C98:
					return VehicleLogistics.GetEcuCharacteristics("BNT-XML-G05_G06_G07.xml", vecInfo);
					IL_1D6A:
					if (vecInfo.Ereihe.Equals("G08", StringComparison.InvariantCultureIgnoreCase) && vecInfo.IsBev())
					{
						return VehicleLogistics.GetEcuCharacteristics("BNT-XML-G08BEV.xml", vecInfo);
					}
					if (vecInfo.HasEnavevoOrNbtevo())
					{
						return VehicleLogistics.GetEcuCharacteristics<BNT_G01_G02_G08_F97_F98_SP2015>("BNT-XML-G01_G02_G08_F97_F98_SP2015.xml", vecInfo);
					}
					if (vecInfo.HasHuMgu())
					{
						return VehicleLogistics.GetEcuCharacteristics("BNT-XML-G01_G02_G08_F97_F98_SP2018.xml", vecInfo);
					}
					return VehicleLogistics.GetEcuCharacteristics<BNT_G01_G02_G08_F97_F98_SP2015>("BNT-XML-G01_G02_G08_F97_F98_SP2015.xml", vecInfo);
					IL_1EE9:
					return VehicleLogistics.GetEcuCharacteristics<E89EcuCharacteristics>("BNT-XML-E89.xml", vecInfo);
					IL_215E:
					if (vecInfo.Ereihe.Equals("F56", StringComparison.InvariantCultureIgnoreCase) && vecInfo.IsBev())
					{
						return VehicleLogistics.GetEcuCharacteristics("BNT-XML-F56BEV.xml", vecInfo);
					}
					return VehicleLogistics.GetEcuCharacteristics<F56EcuCharacteristics>("BNT-XML-F56.xml", vecInfo);
					IL_2239:
					return VehicleLogistics.GetEcuCharacteristics("MRK01XEcuCharacteristics.xml", vecInfo);
					IL_2255:
					return VehicleLogistics.GetEcuCharacteristics<F20EcuCharacteristics>("BNT-XML-F20.xml", vecInfo);
					IL_2286:
					return VehicleLogistics.GetEcuCharacteristics("BNT-XML-RR1X_RR3X_RRNM.xml", vecInfo);
					IL_2372:
					return VehicleLogistics.HandleG20G28EcuCharcteristics(vecInfo);
				}
				IL_2379:
                ;
                //Log.Info("VehicleLogistics.GetCharacteristics()", "cannot retrieve bordnet configuration using ereihe", Array.Empty<object>());
            }
			switch (vecInfo.BNType)
			{
				case BNType.IBUS:
					return VehicleLogistics.GetEcuCharacteristics("iBusEcuCharacteristics.xml", vecInfo);
				case BNType.BN2000_MOTORBIKE:
					return VehicleLogistics.GetEcuCharacteristics<MREcuCharacteristics>("MREcuCharacteristics.xml", vecInfo);
				case BNType.BN2020_MOTORBIKE:
					return VehicleLogistics.GetEcuCharacteristics<MRXEcuCharacteristics>("BNT-XML-BIKE-K001.xml", vecInfo);
				case BNType.BNK01X_MOTORBIKE:
					return VehicleLogistics.GetEcuCharacteristics("MRK01XEcuCharacteristics.xml", vecInfo);
				case BNType.BN2000_WIESMANN:
					return VehicleLogistics.GetEcuCharacteristics("WiesmannEcuCharacteristics.xml", vecInfo);
				case BNType.BN2000_RODING:
					return VehicleLogistics.GetEcuCharacteristics("RodingEcuCharacteristics.xml", vecInfo);
				case BNType.BN2000_PGO:
					return VehicleLogistics.GetEcuCharacteristics("PGOEcuCharacteristics.xml", vecInfo);
				case BNType.BN2000_GIBBS:
					return VehicleLogistics.GetEcuCharacteristics("GibbsEcuCharacteristics.xml", vecInfo);
				case BNType.BN2020_CAMPAGNA:
					return VehicleLogistics.GetEcuCharacteristics("CampagnaEcuCharacteristics.xml", vecInfo);
			}
#if false
			if (VehicleLogistics.readFromDatabase)
			{
				BaseEcuCharacteristics baseEcuCharacteristics = VehicleLogistics.GetEcuCharacteristics(string.Empty, vecInfo);
				if (baseEcuCharacteristics != null)
				{
					return baseEcuCharacteristics;
				}
			}
			Log.Warning("VehicleLogistics.GetCharacteristics()", string.Format("No configuration found for vehicle with ereihe: {0}, bn type: {1}", vecInfo.Ereihe, vecInfo.BNType), Array.Empty<object>());
#endif
            return null;
		}

        private static string HandleG20G28EcuCharcteristics(Vehicle vecInfo)
        {
            bool flag = vecInfo.IsPhev();
            bool flag2 = vecInfo.IsBev();
            if ((vecInfo.Ereihe.Equals("G26") || vecInfo.Ereihe.Equals("G28")) && flag2)
            {
                return VehicleLogistics.GetEcuCharacteristics("BNT-XML-G26_G28_BEV.xml", vecInfo);
            }
            if ((vecInfo.Ereihe.Equals("G20") || vecInfo.Ereihe.Equals("G21")) && flag)
            {
                return VehicleLogistics.GetEcuCharacteristics("BNT-XML-G20_G21_PHEV.xml", vecInfo);
            }
            if (vecInfo.HasHuMgu() && !flag)
            {
                return VehicleLogistics.GetEcuCharacteristics("BNT-XML-G20_G28_MGU.xml", vecInfo);
            }
            if (vecInfo.HasEnavevo() && new string[]
                {
                    "G20",
                    "G21",
                    "G22",
                    "G23",
                    "G42"
                }.Any((string er) => string.Equals(er, vecInfo.Ereihe, StringComparison.InvariantCultureIgnoreCase)))
            {
                return VehicleLogistics.GetEcuCharacteristics("BNT-XML-G20_G28_NOMGU.xml", vecInfo);
            }
            return VehicleLogistics.GetEcuCharacteristics("BNT_G20_G28.xml", vecInfo);
        }

		public static BNType getBNType(Vehicle vecInfo)
		{
			if (vecInfo == null)
			{
				//Log.Warning("VehicleLogistics.getBNType()", "vehicle was null", Array.Empty<object>());
				return BNType.UNKNOWN;
			}
			if (string.IsNullOrEmpty(vecInfo.Ereihe))
			{
				return BNType.UNKNOWN;
			}
			string text = vecInfo.Ereihe.ToUpper();
			if (text != null)
			{
				uint num = ComputeStringHash(text);
				if (num <= 2271572957U)
				{
					if (num <= 1405558569U)
					{
						if (num <= 570809940U)
						{
							if (num <= 110892101U)
							{
								if (num <= 43634530U)
								{
									if (num <= 26856911U)
									{
										if (num != 10079292U)
										{
											if (num != 14853800U)
											{
												if (num != 26856911U)
												{
													goto IL_1F57;
												}
												if (!(text == "K14"))
												{
													goto IL_1F57;
												}
												return BNType.BN2000_MOTORBIKE;
											}
											else
											{
												if (!(text == "M13"))
												{
													goto IL_1F57;
												}
												return BNType.BN2020;
											}
										}
										else
										{
											if (!(text == "K15"))
											{
												goto IL_1F57;
											}
											return BNType.BN2000_MOTORBIKE;
										}
									}
									else if (num != 27004006U)
									{
										if (num != 31631419U)
										{
											if (num != 43634530U)
											{
												goto IL_1F57;
											}
											if (!(text == "K17"))
											{
												goto IL_1F57;
											}
											return BNType.BN2020_MOTORBIKE;
										}
										else
										{
											if (text == "M12")
											{
												return BNType.BEV2010;
											}
											goto IL_1F57;
										}
									}
									else
									{
										if (!(text == "K66"))
										{
											goto IL_1F57;
										}
										return BNType.BN2020_MOTORBIKE;
									}
								}
								else if (num <= 60559244U)
								{
									if (num != 43781625U)
									{
										if (num != 60412149U)
										{
											if (num != 60559244U)
											{
												goto IL_1F57;
											}
											if (!(text == "K60"))
											{
												goto IL_1F57;
											}
											return BNType.BN2020_MOTORBIKE;
										}
										else
										{
											if (!(text == "K16"))
											{
												goto IL_1F57;
											}
											return BNType.BN2000_MOTORBIKE;
										}
									}
									else
									{
										if (!(text == "K67"))
										{
											goto IL_1F57;
										}
										return BNType.BN2020_MOTORBIKE;
									}
								}
								else if (num <= 77336863U)
								{
									if (num != 77189768U)
									{
										if (num != 77336863U)
										{
											goto IL_1F57;
										}
										if (!(text == "K61"))
										{
											goto IL_1F57;
										}
										return BNType.BN2020_MOTORBIKE;
									}
									else
									{
										if (!(text == "K19"))
										{
											goto IL_1F57;
										}
										if (!string.IsNullOrEmpty(vecInfo.VINType) && (vecInfo.VINType.Equals("0C05") || vecInfo.VINType.Equals("0C15")))
										{
											return BNType.BN2020_MOTORBIKE;
										}
										return BNType.BN2000_MOTORBIKE;
									}
								}
								else if (num != 93967387U)
								{
									if (num != 110892101U)
									{
										goto IL_1F57;
									}
									if (!(text == "K63"))
									{
										goto IL_1F57;
									}
									return BNType.BN2020_MOTORBIKE;
								}
								else
								{
									if (!(text == "K18"))
									{
										goto IL_1F57;
									}
									if (!string.IsNullOrEmpty(vecInfo.VINType) && (vecInfo.VINType.Equals("0C04") || vecInfo.VINType.Equals("0C14")))
									{
										return BNType.BN2020_MOTORBIKE;
									}
									return BNType.BN2000_MOTORBIKE;
								}
							}
							else if (num <= 279343610U)
							{
								if (num <= 229010753U)
								{
									if (num != 211557815U)
									{
										if (num != 212086039U)
										{
											if (num != 229010753U)
											{
												goto IL_1F57;
											}
											if (!(text == "MF30"))
											{
												goto IL_1F57;
											}
											return BNType.BN2000_WIESMANN;
										}
										else
										{
											if (!(text == "MF25"))
											{
												goto IL_1F57;
											}
											return BNType.BN2000_WIESMANN;
										}
									}
									else
									{
										if (!(text == "K69"))
										{
											goto IL_1F57;
										}
										return BNType.BN2020_MOTORBIKE;
									}
								}
								else if (num <= 268518608U)
								{
									if (num != 261596482U)
									{
										if (num != 268518608U)
										{
											goto IL_1F57;
										}
										if (!(text == "E39"))
										{
											goto IL_1F57;
										}
										return BNType.IBUS;
									}
									else
									{
										if (!(text == "K08"))
										{
											goto IL_1F57;
										}
										return BNType.BN2020_MOTORBIKE;
									}
								}
								else if (num != 278374101U)
								{
									if (num != 279343610U)
									{
										goto IL_1F57;
									}
									if (!(text == "MF35"))
									{
										goto IL_1F57;
									}
									return BNType.BN2000_WIESMANN;
								}
								else
								{
									if (!(text == "K09"))
									{
										goto IL_1F57;
									}
									return BNType.BN2020_MOTORBIKE;
								}
							}
							else if (num <= 362262196U)
							{
								if (num != 285296227U)
								{
									if (num != 311929339U)
									{
										if (num != 362262196U)
										{
											goto IL_1F57;
										}
										if (!(text == "K02"))
										{
											goto IL_1F57;
										}
										return BNType.BN2020_MOTORBIKE;
									}
									else
									{
										if (!(text == "K07"))
										{
											goto IL_1F57;
										}
										return BNType.BN2020_MOTORBIKE;
									}
								}
								else
								{
									if (!(text == "E38"))
									{
										goto IL_1F57;
									}
									return BNType.IBUS;
								}
							}
							else if (num <= 486774750U)
							{
								if (num != 379039815U)
								{
									if (num != 486774750U)
									{
										goto IL_1F57;
									}
									if (!(text == "E46"))
									{
										goto IL_1F57;
									}
									return BNType.IBUS;
								}
								else
								{
									if (!(text == "K03"))
									{
										goto IL_1F57;
									}
									return BNType.BN2020_MOTORBIKE;
								}
							}
							else if (num != 520182893U)
							{
								if (num != 570809940U)
								{
									goto IL_1F57;
								}
								if (!(text == "E53"))
								{
									goto IL_1F57;
								}
								return BNType.IBUS;
							}
							else
							{
								if (!(text == "E36"))
								{
									goto IL_1F57;
								}
								return BNType.IBUS;
							}
						}
						else if (num <= 1276256427U)
						{
							if (num <= 752709452U)
							{
								if (num <= 610517355U)
								{
									if (num != 587587559U)
									{
										if (num != 593739736U)
										{
											if (num != 610517355U)
											{
												goto IL_1F57;
											}
											if (text == "V99")
											{
												return BNType.BN2020_CAMPAGNA;
											}
											goto IL_1F57;
										}
										else
										{
											if (text == "V98")
											{
												return BNType.BN2000_MOTORBIKE;
											}
											goto IL_1F57;
										}
									}
									else
									{
										if (!(text == "E52"))
										{
											goto IL_1F57;
										}
										return BNType.IBUS;
									}
								}
								else if (num <= 714685471U)
								{
									if (num != 623919382U)
									{
										if (num != 714685471U)
										{
											goto IL_1F57;
										}
										if (!(text == "K599"))
										{
											goto IL_1F57;
										}
										return BNType.BNK01X_MOTORBIKE;
									}
									else
									{
										if (!(text == "I20"))
										{
											goto IL_1F57;
										}
										return BNType.BN2020;
									}
								}
								else if (num != 732329237U)
								{
									if (num != 752709452U)
									{
										goto IL_1F57;
									}
									if (!(text == "247"))
									{
										goto IL_1F57;
									}
									return BNType.BNK01X_MOTORBIKE;
								}
								else
								{
									if (text == "RODING_ROADSTER")
									{
										return BNType.BN2000_RODING;
									}
									goto IL_1F57;
								}
							}
							else if (num <= 976102286U)
							{
								if (num != 786117595U)
								{
									if (num != 870152785U)
									{
										if (num != 976102286U)
										{
											goto IL_1F57;
										}
										if (!(text == "I15"))
										{
											goto IL_1F57;
										}
										return BNType.BN2020;
									}
									else
									{
										if (!(text == "248"))
										{
											goto IL_1F57;
										}
										return BNType.BNK01X_MOTORBIKE;
									}
								}
								else
								{
									if (!(text == "259"))
									{
										goto IL_1F57;
									}
									return BNType.BNK01X_MOTORBIKE;
								}
							}
							else if (num <= 1026435143U)
							{
								if (num != 1016166472U)
								{
									if (num != 1026435143U)
									{
										goto IL_1F57;
									}
									if (!(text == "I12"))
									{
										goto IL_1F57;
									}
									return BNType.BN2020;
								}
								else
								{
									if (text == "N18")
									{
										return BNType.BN2000_PGO;
									}
									goto IL_1F57;
								}
							}
							else if (num != 1259478808U)
							{
								if (num != 1276256427U)
								{
									goto IL_1F57;
								}
								if (!(text == "259R"))
								{
									goto IL_1F57;
								}
								return BNType.BNK01X_MOTORBIKE;
							}
							else
							{
								if (!(text == "259S"))
								{
									goto IL_1F57;
								}
								return BNType.BNK01X_MOTORBIKE;
							}
						}
						else if (num <= 1355225712U)
						{
							if (num <= 1321523379U)
							{
								if (num != 1304745760U)
								{
									if (num != 1320793104U)
									{
										if (num != 1321523379U)
										{
											goto IL_1F57;
										}
										if (!(text == "F20"))
										{
											goto IL_1F57;
										}
										return BNType.BN2020;
									}
									else
									{
										if (!(text == "R50"))
										{
											goto IL_1F57;
										}
										return BNType.IBUS;
									}
								}
								else
								{
									if (!(text == "F21"))
									{
										goto IL_1F57;
									}
									return BNType.BN2020;
								}
							}
							else if (num <= 1354348342U)
							{
								if (num != 1338300998U)
								{
									if (num != 1354348342U)
									{
										goto IL_1F57;
									}
									if (!(text == "R52"))
									{
										goto IL_1F57;
									}
									return BNType.IBUS;
								}
								else
								{
									if (!(text == "F23"))
									{
										goto IL_1F57;
									}
									return BNType.BN2020;
								}
							}
							else if (num != 1355078617U)
							{
								if (num != 1355225712U)
								{
									goto IL_1F57;
								}
								if (!(text == "F54"))
								{
									goto IL_1F57;
								}
								return BNType.BN2020;
							}
							else
							{
								if (!(text == "F22"))
								{
									goto IL_1F57;
								}
								return BNType.BN2020;
							}
						}
						else if (num <= 1371856236U)
						{
							if (num != 1368372283U)
							{
								if (num != 1371125961U)
								{
									if (num != 1371856236U)
									{
										goto IL_1F57;
									}
									if (!(text == "F25"))
									{
										goto IL_1F57;
									}
									return BNType.BN2020;
								}
								else
								{
									if (!(text == "R53"))
									{
										goto IL_1F57;
									}
									return BNType.IBUS;
								}
							}
							else
							{
								if (!(text == "A67"))
								{
									goto IL_1F57;
								}
								return BNType.BN2000_MOTORBIKE;
							}
						}
						else if (num <= 1388780950U)
						{
							if (num != 1372003331U)
							{
								if (num != 1388780950U)
								{
									goto IL_1F57;
								}
								if (!(text == "F56"))
								{
									goto IL_1F57;
								}
								return BNType.BN2020;
							}
							else
							{
								if (!(text == "F55"))
								{
									goto IL_1F57;
								}
								return BNType.BN2020;
							}
						}
						else if (num != 1404681199U)
						{
							if (num != 1405558569U)
							{
								goto IL_1F57;
							}
							if (!(text == "F57"))
							{
								goto IL_1F57;
							}
							return BNType.BN2020;
						}
						else if (!(text == "R55"))
						{
							goto IL_1F57;
						}
					}
					else if (num <= 1646807088U)
					{
						if (num <= 1472924508U)
						{
							if (num <= 1438236437U)
							{
								if (num <= 1422189093U)
								{
									if (num != 1421379504U)
									{
										if (num != 1421458818U)
										{
											if (num != 1422189093U)
											{
												goto IL_1F57;
											}
											if (!(text == "F26"))
											{
												goto IL_1F57;
											}
											return BNType.BN2020;
										}
										else
										{
											if (!(text == "R56"))
											{
												goto IL_1F57;
											}
											return BNType.BN2000;
										}
									}
									else if (!(text == "RR5"))
									{
										goto IL_1F57;
									}
								}
								else if (num != 1422483283U)
								{
									if (num != 1438157123U)
									{
										if (num != 1438236437U)
										{
											goto IL_1F57;
										}
										if (!(text == "R57"))
										{
											goto IL_1F57;
										}
										return BNType.BN2000;
									}
									else if (!(text == "RR4"))
									{
										goto IL_1F57;
									}
								}
								else
								{
									if (!(text == "F46"))
									{
										goto IL_1F57;
									}
									return BNType.BN2020;
								}
							}
							else if (num <= 1455891426U)
							{
								if (num != 1439260902U)
								{
									if (num != 1455014056U)
									{
										if (num != 1455891426U)
										{
											goto IL_1F57;
										}
										if (!(text == "F52"))
										{
											goto IL_1F57;
										}
										return BNType.BN2020;
									}
									else
									{
										if (!(text == "R58"))
										{
											goto IL_1F57;
										}
										return BNType.BN2000;
									}
								}
								else
								{
									if (!(text == "F45"))
									{
										goto IL_1F57;
									}
									return BNType.BN2020;
								}
							}
							else if (num <= 1471712361U)
							{
								if (num != 1456038521U)
								{
									if (num != 1471712361U)
									{
										goto IL_1F57;
									}
									if (!(text == "RR6"))
									{
										goto IL_1F57;
									}
								}
								else
								{
									if (!(text == "F44"))
									{
										goto IL_1F57;
									}
									return BNType.BN2020;
								}
							}
							else if (num != 1471791675U)
							{
								if (num != 1472924508U)
								{
									goto IL_1F57;
								}
								if (!(text == "R21"))
								{
									goto IL_1F57;
								}
								return BNType.BNK01X_MOTORBIKE;
							}
							else
							{
								if (!(text == "R59"))
								{
									goto IL_1F57;
								}
								return BNType.BN2000;
							}
							return BNType.BN2020;
						}
						if (num <= 1527920712U)
						{
							if (num <= 1517763202U)
							{
								if (num != 1488489980U)
								{
									if (num != 1501132678U)
									{
										if (num != 1517763202U)
										{
											goto IL_1F57;
										}
										if (!(text == "U06"))
										{
											goto IL_1F57;
										}
									}
									else if (!(text == "U11"))
									{
										goto IL_1F57;
									}
									return BNType.BN2020;
								}
								if (!(text == "RR1"))
								{
									goto IL_1F57;
								}
							}
							else if (num <= 1523148997U)
							{
								if (num != 1522045218U)
								{
									if (num != 1523148997U)
									{
										goto IL_1F57;
									}
									if (!(text == "F40"))
									{
										goto IL_1F57;
									}
									return BNType.BN2020;
								}
								else if (!(text == "RR3"))
								{
									goto IL_1F57;
								}
							}
							else if (num != 1523257365U)
							{
								if (num != 1527920712U)
								{
									goto IL_1F57;
								}
								if (!(text == "259C"))
								{
									goto IL_1F57;
								}
								return BNType.BNK01X_MOTORBIKE;
							}
							else
							{
								if (!(text == "R22"))
								{
									goto IL_1F57;
								}
								return BNType.BNK01X_MOTORBIKE;
							}
						}
						else if (num <= 1623231531U)
						{
							if (num != 1538822837U)
							{
								if (num != 1606453912U)
								{
									if (num != 1623231531U)
									{
										goto IL_1F57;
									}
									if (!(text == "R60"))
									{
										goto IL_1F57;
									}
									return BNType.BN2000;
								}
								else
								{
									if (text == "R61")
									{
										return BNType.BN2000;
									}
									goto IL_1F57;
								}
							}
							else if (!(text == "RR2"))
							{
								goto IL_1F57;
							}
						}
						else if (num <= 1628586426U)
						{
							if (num != 1623923079U)
							{
								if (num != 1628586426U)
								{
									goto IL_1F57;
								}
								if (!(text == "259E"))
								{
									goto IL_1F57;
								}
								return BNType.BNK01X_MOTORBIKE;
							}
							else
							{
								if (!(text == "R28"))
								{
									goto IL_1F57;
								}
								return BNType.BNK01X_MOTORBIKE;
							}
						}
						else if (num != 1640592330U)
						{
							if (num != 1646807088U)
							{
								goto IL_1F57;
							}
							if (!(text == "G02"))
							{
								goto IL_1F57;
							}
							return BNType.BN2020;
						}
						else
						{
							if (!(text == "F49"))
							{
								goto IL_1F57;
							}
							return BNType.BN2020;
						}
						return BNType.BN2000;
					}
					else if (num <= 2022128458U)
					{
						if (num <= 1747619897U)
						{
							if (num <= 1697287040U)
							{
								if (num != 1657369949U)
								{
									if (num != 1697139945U)
									{
										if (num != 1697287040U)
										{
											goto IL_1F57;
										}
										if (!(text == "G15"))
										{
											goto IL_1F57;
										}
										return BNType.BN2020;
									}
									else
									{
										if (!(text == "G01"))
										{
											goto IL_1F57;
										}
										return BNType.BN2020;
									}
								}
								else
								{
									if (!(text == "F48"))
									{
										goto IL_1F57;
									}
									return BNType.BN2020;
								}
							}
							else if (num <= 1714064659U)
							{
								if (num != 1713917564U)
								{
									if (num != 1714064659U)
									{
										goto IL_1F57;
									}
									if (!(text == "G14"))
									{
										goto IL_1F57;
									}
									return BNType.BN2020;
								}
								else
								{
									if (!(text == "G06"))
									{
										goto IL_1F57;
									}
									return BNType.BN2020;
								}
							}
							else if (num != 1730695183U)
							{
								if (num != 1747619897U)
								{
									goto IL_1F57;
								}
								if (!(text == "G16"))
								{
									goto IL_1F57;
								}
								return BNType.BN2020;
							}
							else
							{
								if (!(text == "G07"))
								{
									goto IL_1F57;
								}
								return BNType.BN2020;
							}
						}
						else if (num <= 1814583278U)
						{
							if (num != 1764250421U)
							{
								if (num != 1764397516U)
								{
									if (num != 1814583278U)
									{
										goto IL_1F57;
									}
									if (!(text == "G08"))
									{
										goto IL_1F57;
									}
									return BNType.BN2020;
								}
								else
								{
									if (!(text == "G11"))
									{
										goto IL_1F57;
									}
									return BNType.BN2020;
								}
							}
							else
							{
								if (!(text == "G05"))
								{
									goto IL_1F57;
								}
								return BNType.BN2020;
							}
						}
						else if (num <= 1982212373U)
						{
							if (num != 1814730373U)
							{
								if (num != 1982212373U)
								{
									goto IL_1F57;
								}
								if (!(text == "G38"))
								{
									goto IL_1F57;
								}
								return BNType.BN2020;
							}
							else
							{
								if (!(text == "G12"))
								{
									goto IL_1F57;
								}
								return BNType.BN2020;
							}
						}
						else if (num != 2005350839U)
						{
							if (num != 2022128458U)
							{
								goto IL_1F57;
							}
							if (!(text == "RR12"))
							{
								goto IL_1F57;
							}
							return BNType.BN2020;
						}
						else
						{
							if (!(text == "RR11"))
							{
								goto IL_1F57;
							}
							return BNType.BN2020;
						}
					}
					else if (num <= 2207402928U)
					{
						if (num <= 2099655706U)
						{
							if (num != 2033411980U)
							{
								if (num != 2082878087U)
								{
									if (num != 2099655706U)
									{
										goto IL_1F57;
									}
									if (!(text == "G31"))
									{
										goto IL_1F57;
									}
									return BNType.BN2020;
								}
								else
								{
									if (!(text == "G32"))
									{
										goto IL_1F57;
									}
									return BNType.BN2020;
								}
							}
							else
							{
								if (!(text == "J29"))
								{
									goto IL_1F57;
								}
								return BNType.BN2020;
							}
						}
						else if (num <= 2170907243U)
						{
							if (num != 2116433325U)
							{
								if (num != 2170907243U)
								{
									goto IL_1F57;
								}
								if (!(text == "MF3"))
								{
									goto IL_1F57;
								}
								return BNType.BN2000_WIESMANN;
							}
							else
							{
								if (!(text == "G30"))
								{
									goto IL_1F57;
								}
								return BNType.BN2020;
							}
						}
						else if (num != 2206417190U)
						{
							if (num != 2207402928U)
							{
								goto IL_1F57;
							}
							if (!(text == "K28"))
							{
								goto IL_1F57;
							}
							return BNType.BN2000_MOTORBIKE;
						}
						else
						{
							if (!(text == "K84"))
							{
								goto IL_1F57;
							}
							return BNType.BN2020_MOTORBIKE;
						}
					}
					else if (num <= 2239972428U)
					{
						if (num != 2224180547U)
						{
							if (num != 2225019190U)
							{
								if (num != 2239972428U)
								{
									goto IL_1F57;
								}
								if (!(text == "K82"))
								{
									goto IL_1F57;
								}
								return BNType.BN2020_MOTORBIKE;
							}
							else
							{
								if (!(text == "K75"))
								{
									goto IL_1F57;
								}
								return BNType.BN2000_MOTORBIKE;
							}
						}
						else
						{
							if (!(text == "K29"))
							{
								goto IL_1F57;
							}
							return BNType.BN2000_MOTORBIKE;
						}
					}
					else if (num <= 2256750047U)
					{
						if (num != 2254795338U)
						{
							if (num != 2256750047U)
							{
								goto IL_1F57;
							}
							if (!(text == "K83"))
							{
								goto IL_1F57;
							}
							return BNType.BN2020_MOTORBIKE;
						}
						else
						{
							if (!(text == "MF4"))
							{
								goto IL_1F57;
							}
							return BNType.BN2000_WIESMANN;
						}
					}
					else if (num != 2258574428U)
					{
						if (num != 2271572957U)
						{
							goto IL_1F57;
						}
						if (!(text == "MF5"))
						{
							goto IL_1F57;
						}
						return BNType.BN2000_WIESMANN;
					}
					else
					{
						if (!(text == "K73"))
						{
							goto IL_1F57;
						}
						return BNType.BN2000_MOTORBIKE;
					}
					return BNType.BN2000;
				}
				if (num <= 2635045457U)
				{
					if (num <= 2425511975U)
					{
						if (num <= 2366162268U)
						{
							if (num <= 2292276761U)
							{
								if (num <= 2275499142U)
								{
									if (num != 2273527666U)
									{
										if (num != 2275352047U)
										{
											if (num != 2275499142U)
											{
												goto IL_1F57;
											}
											if (!(text == "K48"))
											{
												goto IL_1F57;
											}
											return BNType.BN2020_MOTORBIKE;
										}
										else
										{
											if (!(text == "K72"))
											{
												goto IL_1F57;
											}
											return BNType.BN2000_MOTORBIKE;
										}
									}
									else
									{
										if (!(text == "K80"))
										{
											goto IL_1F57;
										}
										return BNType.BN2020_MOTORBIKE;
									}
								}
								else if (num != 2290305285U)
								{
									if (num != 2292129666U)
									{
										if (num != 2292276761U)
										{
											goto IL_1F57;
										}
										if (!(text == "K49"))
										{
											goto IL_1F57;
										}
										return BNType.BN2020_MOTORBIKE;
									}
									else
									{
										if (!(text == "K71"))
										{
											goto IL_1F57;
										}
										return BNType.BN2000_MOTORBIKE;
									}
								}
								else
								{
									if (!(text == "K81"))
									{
										goto IL_1F57;
									}
									return BNType.BN2020_MOTORBIKE;
								}
							}
							else if (num <= 2315829411U)
							{
								if (num != 2299051792U)
								{
									if (num != 2308907285U)
									{
										if (num != 2315829411U)
										{
											goto IL_1F57;
										}
										if (!(text == "E63"))
										{
											goto IL_1F57;
										}
									}
									else
									{
										if (!(text == "K70"))
										{
											goto IL_1F57;
										}
										return BNType.BN2000_MOTORBIKE;
									}
								}
								else if (!(text == "E62"))
								{
									goto IL_1F57;
								}
							}
							else if (num <= 2349384649U)
							{
								if (num != 2332607030U)
								{
									if (num != 2349384649U)
									{
										goto IL_1F57;
									}
									if (!(text == "E61"))
									{
										goto IL_1F57;
									}
								}
								else if (!(text == "E60"))
								{
									goto IL_1F57;
								}
							}
							else if (num != 2358401499U)
							{
								if (num != 2366162268U)
								{
									goto IL_1F57;
								}
								if (!(text == "E66"))
								{
									goto IL_1F57;
								}
							}
							else
							{
								if (!(text == "K21"))
								{
									goto IL_1F57;
								}
								if (!string.IsNullOrEmpty(vecInfo.VINType) && (vecInfo.VINType.Equals("0A06") || vecInfo.VINType.Equals("0A16")))
								{
									return BNType.BN2000_MOTORBIKE;
								}
								return BNType.BN2020_MOTORBIKE;
							}
						}
						else if (num <= 2399717506U)
						{
							if (num <= 2382939887U)
							{
								if (num != 2375179118U)
								{
									if (num != 2376164856U)
									{
										if (num != 2382939887U)
										{
											goto IL_1F57;
										}
										if (!(text == "E67"))
										{
											goto IL_1F57;
										}
									}
									else
									{
										if (!(text == "K42"))
										{
											goto IL_1F57;
										}
										return BNType.BN2000_MOTORBIKE;
									}
								}
								else
								{
									if (!(text == "K22"))
									{
										goto IL_1F57;
									}
									return BNType.BN2020_MOTORBIKE;
								}
							}
							else if (num <= 2392103832U)
							{
								if (num != 2391956737U)
								{
									if (num != 2392103832U)
									{
										goto IL_1F57;
									}
									if (!(text == "K33"))
									{
										goto IL_1F57;
									}
									return BNType.BN2020_MOTORBIKE;
								}
								else
								{
									if (!(text == "K23"))
									{
										goto IL_1F57;
									}
									return BNType.BN2020_MOTORBIKE;
								}
							}
							else if (num != 2392942475U)
							{
								if (num != 2399717506U)
								{
									goto IL_1F57;
								}
								if (!(text == "E64"))
								{
									goto IL_1F57;
								}
							}
							else
							{
								if (!(text == "K43"))
								{
									goto IL_1F57;
								}
								return BNType.BN2000_MOTORBIKE;
							}
						}
						else if (num <= 2409720094U)
						{
							if (num != 2400011696U)
							{
								if (num != 2408881451U)
								{
									if (num != 2409720094U)
									{
										goto IL_1F57;
									}
									if (!(text == "K40"))
									{
										goto IL_1F57;
									}
									return BNType.BN2000_MOTORBIKE;
								}
								else
								{
									if (!(text == "K32"))
									{
										goto IL_1F57;
									}
									return BNType.BN2020_MOTORBIKE;
								}
							}
							else if (!(text == "E88"))
							{
								goto IL_1F57;
							}
						}
						else if (num <= 2416642220U)
						{
							if (num != 2416495125U)
							{
								if (num != 2416642220U)
								{
									goto IL_1F57;
								}
								if (!(text == "E71"))
								{
									goto IL_1F57;
								}
							}
							else if (!(text == "E65"))
							{
								goto IL_1F57;
							}
						}
						else if (num != 2416789315U)
						{
							if (num != 2425511975U)
							{
								goto IL_1F57;
							}
							if (!(text == "K25"))
							{
								goto IL_1F57;
							}
							return BNType.BN2000_MOTORBIKE;
						}
						else if (!(text == "E89"))
						{
							goto IL_1F57;
						}
					}
					else if (num <= 2493755284U)
					{
						if (num <= 2459067213U)
						{
							if (num <= 2442289594U)
							{
								if (num != 2426497713U)
								{
									if (num != 2433419839U)
									{
										if (num != 2442289594U)
										{
											goto IL_1F57;
										}
										if (!(text == "K26"))
										{
											goto IL_1F57;
										}
										return BNType.BN2000_MOTORBIKE;
									}
									else if (!(text == "E70"))
									{
										goto IL_1F57;
									}
								}
								else
								{
									if (!(text == "K41"))
									{
										goto IL_1F57;
									}
									return BNType.BNK01X_MOTORBIKE;
								}
							}
							else if (num <= 2443275332U)
							{
								if (num != 2442436689U)
								{
									if (num != 2443275332U)
									{
										goto IL_1F57;
									}
									if (!(text == "K46"))
									{
										goto IL_1F57;
									}
									if (!string.IsNullOrEmpty(vecInfo.VINType) && !"XXXX".Equals(vecInfo.VINType, StringComparison.OrdinalIgnoreCase) && (vecInfo.VINType.Equals("0D10") || vecInfo.VINType.Equals("0D21") || vecInfo.VINType.Equals("0D30") || vecInfo.VINType.Equals("0D40") || vecInfo.VINType.Equals("0D50") || vecInfo.VINType.Equals("0D60") || vecInfo.VINType.Equals("0D70") || vecInfo.VINType.Equals("0D80") || vecInfo.VINType.Equals("0D90")))
									{
										return BNType.BN2020_MOTORBIKE;
									}
									return BNType.BN2000_MOTORBIKE;
								}
								else
								{
									if (!(text == "K30"))
									{
										goto IL_1F57;
									}
									return BNType.BNK01X_MOTORBIKE;
								}
							}
							else if (num != 2443422427U)
							{
								if (num != 2459067213U)
								{
									goto IL_1F57;
								}
								if (!(text == "K27"))
								{
									goto IL_1F57;
								}
								return BNType.BN2000_MOTORBIKE;
							}
							else
							{
								if (!(text == "K54"))
								{
									goto IL_1F57;
								}
								return BNType.BN2020_MOTORBIKE;
							}
						}
						else if (num <= 2466827982U)
						{
							if (num != 2460052951U)
							{
								if (num != 2461271238U)
								{
									if (num != 2466827982U)
									{
										goto IL_1F57;
									}
									if (!(text == "E68"))
									{
										goto IL_1F57;
									}
								}
								else
								{
									if (!(text == "H61"))
									{
										goto IL_1F57;
									}
									return BNType.BN2000_MOTORBIKE;
								}
							}
							else
							{
								if (!(text == "K47"))
								{
									goto IL_1F57;
								}
								return BNType.BN2020_MOTORBIKE;
							}
						}
						else if (num <= 2476830570U)
						{
							if (num != 2466975077U)
							{
								if (num != 2476830570U)
								{
									goto IL_1F57;
								}
								if (!(text == "K44"))
								{
									goto IL_1F57;
								}
								return BNType.BN2000_MOTORBIKE;
							}
							else if (!(text == "E72"))
							{
								goto IL_1F57;
							}
						}
						else if (num != 2492769546U)
						{
							if (num != 2493755284U)
							{
								goto IL_1F57;
							}
							if (!(text == "K51"))
							{
								goto IL_1F57;
							}
							return BNType.BN2020_MOTORBIKE;
						}
						else
						{
							if (!(text == "K35"))
							{
								goto IL_1F57;
							}
							return BNType.BN2020_MOTORBIKE;
						}
					}
					else if (num <= 2584565505U)
					{
						if (num <= 2527310522U)
						{
							if (num != 2509547165U)
							{
								if (num != 2510532903U)
								{
									if (num != 2527310522U)
									{
										goto IL_1F57;
									}
									if (!(text == "K53"))
									{
										goto IL_1F57;
									}
									return BNType.BN2020_MOTORBIKE;
								}
								else
								{
									if (!(text == "K50"))
									{
										goto IL_1F57;
									}
									return BNType.BN2020_MOTORBIKE;
								}
							}
							else
							{
								if (!(text == "K34"))
								{
									goto IL_1F57;
								}
								return BNType.BN2020_MOTORBIKE;
							}
						}
						else if (num <= 2551010267U)
						{
							if (num != 2544088141U)
							{
								if (num != 2551010267U)
								{
									goto IL_1F57;
								}
								if (!(text == "E81"))
								{
									goto IL_1F57;
								}
							}
							else
							{
								if (text == "K52")
								{
									return BNType.BN2020_MOTORBIKE;
								}
								goto IL_1F57;
							}
						}
						else if (num != 2567787886U)
						{
							if (num != 2584565505U)
							{
								goto IL_1F57;
							}
							if (!(text == "E83"))
							{
								goto IL_1F57;
							}
							return BNType.IBUS;
						}
						else if (!(text == "E82"))
						{
							goto IL_1F57;
						}
					}
					else if (num <= 2601490219U)
					{
						if (num != 2584712600U)
						{
							if (num != 2601343124U)
							{
								if (num != 2601490219U)
								{
									goto IL_1F57;
								}
								if (!(text == "E92"))
								{
									goto IL_1F57;
								}
							}
							else if (!(text == "E84"))
							{
								goto IL_1F57;
							}
						}
						else if (!(text == "E93"))
						{
							goto IL_1F57;
						}
					}
					else if (num <= 2618267838U)
					{
						if (num != 2618120743U)
						{
							if (num != 2618267838U)
							{
								goto IL_1F57;
							}
							if (!(text == "E91"))
							{
								goto IL_1F57;
							}
						}
						else
						{
							if (!(text == "E85"))
							{
								goto IL_1F57;
							}
							return BNType.IBUS;
						}
					}
					else if (num != 2634898362U)
					{
						if (num != 2635045457U)
						{
							goto IL_1F57;
						}
						if (!(text == "E90"))
						{
							goto IL_1F57;
						}
					}
					else
					{
						if (text == "E86")
						{
							return BNType.IBUS;
						}
						goto IL_1F57;
					}
				}
				else if (num <= 3753880776U)
				{
					if (num <= 3517567087U)
					{
						if (num <= 3123387255U)
						{
							if (num <= 2729132584U)
							{
								if (num != 2651675981U)
								{
									if (num != 2691943669U)
									{
										if (num != 2729132584U)
										{
											goto IL_1F57;
										}
										if (!(text == "K569"))
										{
											goto IL_1F57;
										}
										return BNType.BNK01X_MOTORBIKE;
									}
									else
									{
										if (!(text == "C01"))
										{
											goto IL_1F57;
										}
										return BNType.BN2000_MOTORBIKE;
									}
								}
								else if (!(text == "E87"))
								{
									goto IL_1F57;
								}
							}
							else if (num <= 2845166379U)
							{
								if (num != 2731831713U)
								{
									if (num != 2845166379U)
									{
										goto IL_1F57;
									}
									if (!(text == "247E"))
									{
										goto IL_1F57;
									}
									return BNType.BNK01X_MOTORBIKE;
								}
								else
								{
									if (!(text == "H91"))
									{
										goto IL_1F57;
									}
									return BNType.BN2000_MOTORBIKE;
								}
							}
							else if (num != 2929478274U)
							{
								if (num != 3123387255U)
								{
									goto IL_1F57;
								}
								if (!(text == "I01"))
								{
									goto IL_1F57;
								}
								return BNType.BN2020;
							}
							else
							{
								if (!(text == "K589"))
								{
									goto IL_1F57;
								}
								return BNType.BNK01X_MOTORBIKE;
							}
						}
						else if (num <= 3434009218U)
						{
							if (num != 3233663528U)
							{
								if (num != 3350847836U)
								{
									if (num != 3434009218U)
									{
										goto IL_1F57;
									}
									if (!(text == "E169"))
									{
										goto IL_1F57;
									}
									return BNType.BNK01X_MOTORBIKE;
								}
								else
								{
									if (text == "AERO")
									{
										return BNType.BN2000_MORGAN;
									}
									goto IL_1F57;
								}
							}
							else
							{
								if (text == "E189")
								{
									return BNType.BNK01X_MOTORBIKE;
								}
								goto IL_1F57;
							}
						}
						else if (num <= 3484158944U)
						{
							if (num != 3484011849U)
							{
								if (num != 3484158944U)
								{
									goto IL_1F57;
								}
								if (!(text == "F83"))
								{
									goto IL_1F57;
								}
								return BNType.BN2020;
							}
							else
							{
								if (!(text == "F93"))
								{
									goto IL_1F57;
								}
								return BNType.BN2020;
							}
						}
						else if (num != 3500936563U)
						{
							if (num != 3517567087U)
							{
								goto IL_1F57;
							}
							if (!(text == "F95"))
							{
								goto IL_1F57;
							}
							return BNType.BN2020;
						}
						else
						{
							if (!(text == "F82"))
							{
								goto IL_1F57;
							}
							return BNType.BN2020;
						}
					}
					else if (num <= 3568047039U)
					{
						if (num <= 3534491801U)
						{
							if (num != 3517714182U)
							{
								if (num != 3534344706U)
								{
									if (num != 3534491801U)
									{
										goto IL_1F57;
									}
									if (!(text == "F80"))
									{
										goto IL_1F57;
									}
									return BNType.BN2020;
								}
								else
								{
									if (!(text == "F96"))
									{
										goto IL_1F57;
									}
									return BNType.BN2020;
								}
							}
							else
							{
								if (!(text == "F81"))
								{
									goto IL_1F57;
								}
								return BNType.BN2020;
							}
						}
						else if (num <= 3551269420U)
						{
							if (num != 3551122325U)
							{
								if (num != 3551269420U)
								{
									goto IL_1F57;
								}
								if (!(text == "F87"))
								{
									goto IL_1F57;
								}
								return BNType.BN2020;
							}
							else
							{
								if (!(text == "F97"))
								{
									goto IL_1F57;
								}
								return BNType.BN2020;
							}
						}
						else if (num != 3567899944U)
						{
							if (num != 3568047039U)
							{
								goto IL_1F57;
							}
							if (!(text == "F86"))
							{
								goto IL_1F57;
							}
							return BNType.BN2020;
						}
						else
						{
							if (!(text == "F98"))
							{
								goto IL_1F57;
							}
							return BNType.BN2020;
						}
					}
					else if (num <= 3703400824U)
					{
						if (num != 3569179872U)
						{
							if (num != 3584824658U)
							{
								if (num != 3703400824U)
								{
									goto IL_1F57;
								}
								if (!(text == "F10"))
								{
									goto IL_1F57;
								}
								return BNType.BN2020;
							}
							else
							{
								if (!(text == "F85"))
								{
									goto IL_1F57;
								}
								return BNType.BN2020;
							}
						}
						else
						{
							if (!(text == "F18"))
							{
								goto IL_1F57;
							}
							return BNType.BN2020;
						}
					}
					else if (num <= 3736956062U)
					{
						if (num != 3720178443U)
						{
							if (num != 3736956062U)
							{
								goto IL_1F57;
							}
							if (!(text == "F12"))
							{
								goto IL_1F57;
							}
							return BNType.BN2020;
						}
						else
						{
							if (!(text == "F11"))
							{
								goto IL_1F57;
							}
							return BNType.BN2020;
						}
					}
					else if (num != 3753733681U)
					{
						if (num != 3753880776U)
						{
							goto IL_1F57;
						}
						if (!(text == "F03"))
						{
							goto IL_1F57;
						}
						return BNType.BN2020;
					}
					else
					{
						if (!(text == "F13"))
						{
							goto IL_1F57;
						}
						return BNType.BN2020;
					}
				}
				else if (num <= 3871471204U)
				{
					if (num <= 3817627881U)
					{
						if (num <= 3787288919U)
						{
							if (num != 3770511300U)
							{
								if (num != 3770658395U)
								{
									if (num != 3787288919U)
									{
										goto IL_1F57;
									}
									if (!(text == "F15"))
									{
										goto IL_1F57;
									}
									return BNType.BN2020;
								}
								else
								{
									if (!(text == "F02"))
									{
										goto IL_1F57;
									}
									return BNType.BN2020;
								}
							}
							else
							{
								if (!(text == "F14"))
								{
									goto IL_1F57;
								}
								return BNType.BN2020;
							}
						}
						else if (num <= 3787583109U)
						{
							if (num != 3787436014U)
							{
								if (num != 3787583109U)
								{
									goto IL_1F57;
								}
								if (!(text == "F39"))
								{
									goto IL_1F57;
								}
								return BNType.BN2020;
							}
							else
							{
								if (!(text == "F01"))
								{
									goto IL_1F57;
								}
								return BNType.BN2020;
							}
						}
						else if (num != 3804066538U)
						{
							if (num != 3817627881U)
							{
								goto IL_1F57;
							}
							if (!(text == "RR31"))
							{
								goto IL_1F57;
							}
							return BNType.BN2020;
						}
						else
						{
							if (!(text == "F16"))
							{
								goto IL_1F57;
							}
							return BNType.BN2020;
						}
					}
					else if (num <= 3837915966U)
					{
						if (num != 3820991252U)
						{
							if (num != 3837768871U)
							{
								if (num != 3837915966U)
								{
									goto IL_1F57;
								}
								if (!(text == "F34"))
								{
									goto IL_1F57;
								}
								return BNType.BN2020;
							}
							else
							{
								if (!(text == "F06"))
								{
									goto IL_1F57;
								}
								return BNType.BN2020;
							}
						}
						else
						{
							if (!(text == "F07"))
							{
								goto IL_1F57;
							}
							return BNType.BN2020;
						}
					}
					else if (num <= 3854693585U)
					{
						if (num != 3854546490U)
						{
							if (num != 3854693585U)
							{
								goto IL_1F57;
							}
							if (!(text == "F35"))
							{
								goto IL_1F57;
							}
							return BNType.BN2020;
						}
						else
						{
							if (!(text == "F05"))
							{
								goto IL_1F57;
							}
							return BNType.BN2020;
						}
					}
					else if (num != 3871324109U)
					{
						if (num != 3871471204U)
						{
							goto IL_1F57;
						}
						if (!(text == "F32"))
						{
							goto IL_1F57;
						}
						return BNType.BN2020;
					}
					else
					{
						if (!(text == "F04"))
						{
							goto IL_1F57;
						}
						return BNType.BN2020;
					}
				}
				else if (num <= 3935218309U)
				{
					if (num <= 3888248823U)
					{
						if (num != 3872309847U)
						{
							if (num != 3884885452U)
							{
								if (num != 3888248823U)
								{
									goto IL_1F57;
								}
								if (!(text == "F33"))
								{
									goto IL_1F57;
								}
								return BNType.BN2020;
							}
							else
							{
								if (!(text == "RR21"))
								{
									goto IL_1F57;
								}
								return BNType.BN2020;
							}
						}
						else
						{
							if (!(text == "F60"))
							{
								goto IL_1F57;
							}
							return BNType.BN2020;
						}
					}
					else if (num <= 3921804061U)
					{
						if (num != 3905026442U)
						{
							if (num != 3921804061U)
							{
								goto IL_1F57;
							}
							if (!(text == "F31"))
							{
								goto IL_1F57;
							}
							return BNType.BN2020;
						}
						else
						{
							if (!(text == "F30"))
							{
								goto IL_1F57;
							}
							return BNType.BN2020;
						}
					}
					else if (num != 3921912429U)
					{
						if (num != 3935218309U)
						{
							goto IL_1F57;
						}
						if (text == "RR22")
						{
							return BNType.BN2020;
						}
						goto IL_1F57;
					}
					else
					{
						if (text == "R13")
						{
							return BNType.BN2000_MOTORBIKE;
						}
						goto IL_1F57;
					}
				}
				else if (num <= 4045609247U)
				{
					if (num != 3993770053U)
					{
						if (num != 4028831628U)
						{
							if (num != 4045609247U)
							{
								goto IL_1F57;
							}
							if (!(text == "G29"))
							{
								goto IL_1F57;
							}
							return BNType.BN2020;
						}
						else
						{
							if (!(text == "G28"))
							{
								goto IL_1F57;
							}
							return BNType.BN2020;
						}
					}
					else
					{
						if (text == "GT1")
						{
							return BNType.BN2000_GIBBS;
						}
						goto IL_1F57;
					}
				}
				else if (num <= 4163052580U)
				{
					if (num != 4159668674U)
					{
						if (num != 4163052580U)
						{
							goto IL_1F57;
						}
						if (!(text == "G20"))
						{
							goto IL_1F57;
						}
						return BNType.BN2020;
					}
					else
					{
						if (!(text == "MF4-S"))
						{
							goto IL_1F57;
						}
						return BNType.BN2000_WIESMANN;
					}
				}
				else if (num != 4179830199U)
				{
					if (num != 4288944288U)
					{
						goto IL_1F57;
					}
					if (text == "MF28")
					{
						return BNType.BN2000_WIESMANN;
					}
					goto IL_1F57;
				}
				else
				{
					if (text == "G21")
					{
						return BNType.BN2020;
					}
					goto IL_1F57;
				}
				return BNType.BN2000;
			}
			IL_1F57:
			if (!vecInfo.Ereihe.ToUpper().StartsWith("F", StringComparison.Ordinal) && !vecInfo.Ereihe.ToUpper().StartsWith("G", StringComparison.Ordinal) && !vecInfo.Ereihe.ToUpper().StartsWith("U", StringComparison.Ordinal))
			{
				return BNType.UNKNOWN;
			}
			return BNType.BN2020;
		}

	}
}
