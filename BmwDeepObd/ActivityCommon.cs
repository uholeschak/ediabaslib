using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Android.Bluetooth;
using Android.Content;
using Android.Hardware.Usb;
using Android.Net;
using Android.Net.Wifi;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using EdiabasLib;
using Hoho.Android.UsbSerial.Driver;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using Android.Content.PM;
using Android.Support.V4.Content;
using Android.Text.Method;
using Android.Views;
using System.Xml.Serialization;

// ReSharper disable InlineOutVariableDeclaration

namespace BmwDeepObd
{
    public class ActivityCommon : IDisposable
    {
        public class FileSystemBlockInfo
        {
            /// <summary>
            /// The path you asked to check file allocation blocks for
            /// </summary>
            public string Path { get; set; }

            /// <summary>
            /// The file system block size, in bytes, for the given path
            /// </summary>
            public double BlockSizeBytes { get; set; }

            /// <summary>
            /// Total size of the file system at the given path
            /// </summary>
            public double TotalSizeBytes { get; set; }

            /// <summary>
            /// Available size of the file system at the given path
            /// </summary>
            public double AvailableSizeBytes { get; set; }

            /// <summary>
            /// Total free size of the file system at the given path
            /// </summary>
            public double FreeSizeBytes { get; set; }
        }

        public class VagEcuEntry
        {
            public VagEcuEntry(string sysName, int address)
            {
                SysName = sysName;
                Address = address;
            }

            public string SysName { get; }
            public int Address { get; }
        }

        public class MwTabEntry
        {
            public MwTabEntry(int blockNumber, int? valueIndex, string description, string comment, string valueUnit, string valueType, double? valueMin, double? valueMax, bool dummy = false)
            {
                BlockNumber = blockNumber;
                ValueIndex = valueIndex;
                Description = description;
                Comment = comment;
                ValueUnit = valueUnit;
                ValueType = valueType;
                ValueMin = valueMin;
                ValueMax = valueMax;
                Dummy = dummy;
            }

            public int BlockNumber { get; }
            public int? ValueIndex { get; }
            public int ValueIndexTrans
            {
                get
                {
                    if (!ValueIndex.HasValue)
                    {
                        return 0;
                    }
                    return ValueIndex.Value;
                }
            }
            public string Description { get; }
            public string Comment { get; }
            public string ValueUnit { get; }
            public string ValueType { get; }
            public double? ValueMin { get; }
            public double? ValueMax { get; }
            public bool Dummy { get; }
        }

        public class MwTabFileEntry : IComparable<MwTabFileEntry>
        {
            public MwTabFileEntry(string fileName, List<MwTabEntry> mwTabList)
            {
                FileName = fileName;
                MwTabList = mwTabList;
                ExistCount = 0;
                CompareCount = 0;
                MatchCount = 0;
                MatchRatio = 0;
            }

            public const int MaxMatchRatio = 1000;
            public string FileName { get; }
            public List<MwTabEntry> MwTabList { get; }
            public int ExistCount { get; set; }
            public int CompareCount { get; set; }
            public int MatchCount { get; set; }
            public int MatchRatio { get; set; }

            public int CompareTo(MwTabFileEntry mwTabFileEntry)
            {
                if (MatchRatio == mwTabFileEntry.MatchRatio)
                {
                    if (MatchCount == mwTabFileEntry.MatchCount)
                    {
                        // ReSharper disable once StringCompareToIsCultureSpecific
                        return mwTabFileEntry.FileName.CompareTo(FileName);
                    }
                    return MatchCount - mwTabFileEntry.MatchCount;
                }
                return MatchRatio - mwTabFileEntry.MatchRatio;
            }
        }

        public enum InterfaceType
        {
            None,
            Bluetooth,
            Enet,
            ElmWifi,
            DeepObdWifi,
            Ftdi,
        }

        public enum ManufacturerType
        {
            Bmw,
            Audi,
            Seat,
            Skoda,
            Vw,
        }

        public enum LockType
        {
            None,                   // no lock
            Cpu,                    // CPU lock
            ScreenDim,              // screen dim lock
            ScreenBright,           // screen bright lock
        }

        public enum BtEnableType
        {
            Ask,                    // ask for enbale
            Always,                 // always enable
            Nothing                 // no handling
        }

        public enum BtDisableType
        {
            DisableIfByApp,         // disable if enabled by app
            Nothing                 // no handling
        }

        public enum AutoConnectType
        {
            Offline,                // no auto connect
            Connect,                // auto connect
            ConnectClose,           // auto connect and close app
        }

        public delegate bool ProgressZipDelegate(int percent, bool decrypt = false);
        public delegate bool ProgressVerifyDelegate(int percent);
        public delegate void BcReceiverUpdateDisplayDelegate();
        public delegate void BcReceiverReceivedDelegate(Context context, Intent intent);
        public delegate void TranslateDelegate(List<string> transList);
        public delegate void EnetSsidWarnDelegate(bool retry);
        public delegate void WifiConnectedWarnDelegate();
        public const int UdsDtcStatusOverride = 0x2C;
        public const string MtcBtAppName = @"com.microntek.bluetooth";
        public const string TraceFileName = "ifh.trc.zip";
        public const string EmulatorEnetIp = "192.168.10.244";
        public const string AdapterSsid = "Deep OBD BMW";
        public const string DownloadDir = "Download";
        public const string EcuBaseDir = "Ecu";
        public const string AppNameSpace = "de.holeschak.bmw_deep_obd";
        public const string ActionUsbPermission = AppNameSpace + ".USB_PERMISSION";
        public const string SettingBluetoothHciLog = "bluetooth_hci_log";
        private const string MailInfoDownloadUrl = @"http://www.holeschak.de/BmwDeepObd/Mail.xml";

        private static readonly Dictionary<long, string> VagDtcSaeDict = new Dictionary<long, string>
        {
            {16394, "P0010"}, {16395, "P0020"}, {16449, "P0065"}, {16450, "P0066"}, {16451, "P0067"},
            {16485, "P0101"}, {16486, "P0102"}, {16487, "P0103"}, {16489, "P0105"}, {16490, "P0106"},
            {16491, "P0107"}, {16492, "P0108"}, {16496, "P0112"}, {16497, "P0113"}, {16500, "P0116"},
            {16501, "P0117"}, {16502, "P0118"}, {16504, "P0120"}, {16505, "P0121"}, {16506, "P0122"},
            {16507, "P0123"}, {16509, "P0125"}, {16512, "P0128"}, {16514, "P0130"}, {16515, "P0131"},
            {16516, "P0132"}, {16517, "P0133"}, {16518, "P0134"}, {16519, "P0135"}, {16520, "P0136"},
            {16521, "P0137"}, {16522, "P0138"}, {16523, "P0139"}, {16524, "P0140"}, {16525, "P0141"},
            {16534, "P0150"}, {16535, "P0151"}, {16536, "P0152"}, {16537, "P0153"}, {16538, "P0154"},
            {16539, "P0155"}, {16540, "P0156"}, {16541, "P0157"}, {16542, "P0158"}, {16543, "P0159"},
            {16544, "P0160"}, {16545, "P0161"}, {16554, "P0170"}, {16555, "P0171"}, {16556, "P0172"},
            {16557, "P0173"}, {16558, "P0174"}, {16559, "P0175"}, {16566, "P0182"}, {16567, "P0183"},
            {16581, "P0197"}, {16582, "P0198"}, {16585, "P0201"}, {16586, "P0202"}, {16587, "P0203"},
            {16588, "P0204"}, {16589, "P0205"}, {16590, "P0206"}, {16591, "P0207"}, {16592, "P0208"},
            {16599, "P0215"}, {16600, "P0216"}, {16603, "P0219"}, {16605, "P0221"}, {16606, "P0222"},
            {16607, "P0223"}, {16609, "P0225"}, {16610, "P0226"}, {16611, "P0227"}, {16612, "P0228"},
            {16614, "P0230"}, {16618, "P0234"}, {16619, "P0235"}, {16620, "P0236"}, {16621, "P0237"},
            {16622, "P0238"}, {16627, "P0243"}, {16629, "P0245"}, {16630, "P0246"}, {16636, "P0252"},
            {16645, "P0261"}, {16646, "P0262"}, {16648, "P0264"}, {16649, "P0265"}, {16651, "P0267"},
            {16652, "P0268"}, {16654, "P0270"}, {16655, "P0271"}, {16657, "P0273"}, {16658, "P0274"},
            {16660, "P0276"}, {16661, "P0277"}, {16663, "P0279"}, {16664, "P0280"}, {16666, "P0282"},
            {16667, "P0283"}, {16684, "P0300"}, {16685, "P0301"}, {16686, "P0302"}, {16687, "P0303"},
            {16688, "P0304"}, {16689, "P0305"}, {16690, "P0306"}, {16691, "P0307"}, {16692, "P0308"},
            {16697, "P0313"}, {16698, "P0314"}, {16705, "P0321"}, {16706, "P0322"}, {16709, "P0325"},
            {16710, "P0326"}, {16711, "P0327"}, {16712, "P0328"}, {16716, "P0332"}, {16717, "P0333"},
            {16719, "P0335"}, {16720, "P0336"}, {16721, "P0337"}, {16724, "P0340"}, {16725, "P0341"},
            {16726, "P0342"}, {16727, "P0343"}, {16735, "P0351"}, {16736, "P0352"}, {16737, "P0353"},
            {16738, "P0354"}, {16739, "P0355"}, {16740, "P0356"}, {16741, "P0357"}, {16742, "P0358"},
            {16764, "P0380"}, {16784, "P0400"}, {16785, "P0401"}, {16786, "P0402"}, {16787, "P0403"},
            {16788, "P0404"}, {16789, "P0405"}, {16790, "P0406"}, {16791, "P0407"}, {16792, "P0408"},
            {16794, "P0410"}, {16795, "P0411"}, {16796, "P0412"}, {16802, "P0418"}, {16804, "P0420"},
            {16806, "P0422"}, {16811, "P0427"}, {16812, "P0428"}, {16816, "P0432"}, {16820, "P0436"},
            {16821, "P0437"}, {16822, "P0438"}, {16824, "P0440"}, {16825, "P0441"}, {16826, "P0442"},
            {16827, "P0443"}, {16836, "P0452"}, {16837, "P0453"}, {16839, "P0455"}, {16845, "P0461"},
            {16846, "P0462"}, {16847, "P0463"}, {16885, "P0501"}, {16887, "P0503"}, {16889, "P0505"},
            {16890, "P0506"}, {16891, "P0507"}, {16894, "P0510"}, {16915, "P0531"}, {16916, "P0532"},
            {16917, "P0533"}, {16935, "P0551"}, {16944, "P0560"}, {16946, "P0562"}, {16947, "P0563"},
            {16952, "P0568"}, {16955, "P0571"}, {16984, "P0600"}, {16985, "P0601"}, {16986, "P0602"},
            {16987, "P0603"}, {16988, "P0604"}, {16989, "P0605"}, {16990, "P0606"}, {17026, "P0642"},
            {17029, "P0645"}, {17034, "P0650"}, {17038, "P0654"}, {17040, "P0656"}, {17084, "P0700"},
            {17086, "P0702"}, {17087, "P0703"}, {17089, "P0705"}, {17090, "P0706"}, {17091, "P0707"},
            {17092, "P0708"}, {17094, "P0710"}, {17095, "P0711"}, {17096, "P0712"}, {17097, "P0713"},
            {17099, "P0715"}, {17100, "P0716"}, {17101, "P0717"}, {17105, "P0721"}, {17106, "P0722"},
            {17109, "P0725"}, {17110, "P0726"}, {17111, "P0727"}, {17114, "P0730"}, {17115, "P0731"},
            {17116, "P0732"}, {17117, "P0733"}, {17118, "P0734"}, {17119, "P0735"}, {17124, "P0740"},
            {17125, "P0741"}, {17132, "P0748"}, {17134, "P0750"}, {17135, "P0751"}, {17136, "P0752"},
            {17137, "P0753"}, {17140, "P0756"}, {17141, "P0757"}, {17142, "P0758"}, {17145, "P0761"},
            {17146, "P0762"}, {17147, "P0763"}, {17152, "P0768"}, {17157, "P0773"}, {17174, "P0790"},
            {17509, "P1101"}, {17510, "P1102"}, {17511, "P1103"}, {17512, "P1104"}, {17513, "P1105"},
            {17514, "P1106"}, {17515, "P1107"}, {17516, "P1108"}, {17517, "P1109"}, {17518, "P1110"},
            {17519, "P1111"}, {17520, "P1112"}, {17521, "P1113"}, {17522, "P1114"}, {17523, "P1115"},
            {17524, "P1116"}, {17525, "P1117"}, {17526, "P1118"}, {17527, "P1119"}, {17528, "P1120"},
            {17529, "P1121"}, {17530, "P1122"}, {17531, "P1123"}, {17532, "P1124"}, {17533, "P1125"},
            {17534, "P1126"}, {17535, "P1127"}, {17536, "P1128"}, {17537, "P1129"}, {17538, "P1130"},
            {17539, "P1131"}, {17540, "P1132"}, {17541, "P1133"}, {17542, "P1134"}, {17543, "P1135"},
            {17544, "P1136"}, {17545, "P1137"}, {17546, "P1138"}, {17547, "P1139"}, {17548, "P1140"},
            {17549, "P1141"}, {17550, "P1142"}, {17551, "P1143"}, {17552, "P1144"}, {17553, "P1145"},
            {17554, "P1146"}, {17555, "P1147"}, {17556, "P1148"}, {17557, "P1149"}, {17558, "P1150"},
            {17559, "P1151"}, {17560, "P1152"}, {17562, "P1154"}, {17563, "P1155"}, {17564, "P1156"},
            {17565, "P1157"}, {17566, "P1158"}, {17568, "P1160"}, {17569, "P1161"}, {17570, "P1162"},
            {17571, "P1163"}, {17572, "P1164"}, {17573, "P1165"}, {17574, "P1166"}, {17579, "P1171"},
            {17580, "P1172"}, {17581, "P1173"}, {17582, "P1174"}, {17584, "P1176"}, {17585, "P1177"},
            {17586, "P1178"}, {17587, "P1179"}, {17588, "P1180"}, {17589, "P1181"}, {17590, "P1182"},
            {17591, "P1183"}, {17592, "P1184"}, {17593, "P1185"}, {17594, "P1186"}, {17595, "P1187"},
            {17596, "P1188"}, {17597, "P1189"}, {17598, "P1190"}, {17604, "P1196"}, {17605, "P1197"},
            {17606, "P1198"}, {17607, "P1199"}, {17609, "P1201"}, {17610, "P1202"}, {17611, "P1203"},
            {17612, "P1204"}, {17613, "P1205"}, {17614, "P1206"}, {17615, "P1207"}, {17616, "P1208"},
            {17617, "P1209"}, {17618, "P1210"}, {17619, "P1211"}, {17621, "P1213"}, {17622, "P1214"},
            {17623, "P1215"}, {17624, "P1216"}, {17625, "P1217"}, {17626, "P1218"}, {17627, "P1219"},
            {17628, "P1220"}, {17629, "P1221"}, {17630, "P1222"}, {17631, "P1223"}, {17633, "P1225"},
            {17634, "P1226"}, {17635, "P1227"}, {17636, "P1228"}, {17637, "P1229"}, {17638, "P1230"},
            {17639, "P1231"}, {17640, "P1232"}, {17645, "P1237"}, {17646, "P1238"}, {17647, "P1239"},
            {17648, "P1240"}, {17649, "P1241"}, {17650, "P1242"}, {17651, "P1243"}, {17652, "P1244"},
            {17653, "P1245"}, {17654, "P1246"}, {17655, "P1247"}, {17656, "P1248"}, {17657, "P1249"},
            {17658, "P1250"}, {17659, "P1251"}, {17660, "P1252"}, {17661, "P1253"}, {17662, "P1254"},
            {17663, "P1255"}, {17664, "P1256"}, {17665, "P1257"}, {17666, "P1258"}, {17667, "P1259"},
            {17688, "P1280"}, {17691, "P1283"}, {17692, "P1284"}, {17693, "P1285"}, {17694, "P1286"},
            {17695, "P1287"}, {17696, "P1288"}, {17697, "P1289"}, {17704, "P1296"}, {17705, "P1297"},
            {17708, "P1300"}, {17721, "P1319"}, {17728, "P1320"}, {17729, "P1321"}, {17730, "P1322"},
            {17731, "P1323"}, {17732, "P1324"}, {17733, "P1325"}, {17734, "P1326"}, {17735, "P1327"},
            {17736, "P1328"}, {17737, "P1329"}, {17738, "P1330"}, {17739, "P1331"}, {17740, "P1332"},
            {17743, "P1335"}, {17744, "P1336"}, {17745, "P1337"}, {17746, "P1338"}, {17747, "P1339"},
            {17748, "P1340"}, {17749, "P1341"}, {17750, "P1342"}, {17751, "P1343"}, {17752, "P1344"},
            {17753, "P1345"}, {17754, "P1346"}, {17755, "P1347"}, {17756, "P1348"}, {17757, "P1349"},
            {17758, "P1350"}, {17762, "P1354"}, {17763, "P1355"}, {17764, "P1356"}, {17765, "P1357"},
            {17766, "P1358"}, {17767, "P1359"}, {17768, "P1360"}, {17769, "P1361"}, {17770, "P1362"},
            {17771, "P1363"}, {17772, "P1364"}, {17773, "P1365"}, {17774, "P1366"}, {17775, "P1367"},
            {17776, "P1368"}, {17777, "P1369"}, {17778, "P1370"}, {17779, "P1371"}, {17780, "P1372"},
            {17781, "P1373"}, {17782, "P1374"}, {17783, "P1375"}, {17784, "P1376"}, {17785, "P1377"},
            {17786, "P1378"}, {17794, "P1386"}, {17795, "P1387"}, {17796, "P1388"}, {17799, "P1391"},
            {17800, "P1392"}, {17801, "P1393"}, {17802, "P1394"}, {17803, "P1395"}, {17804, "P1396"},
            {17805, "P1397"}, {17806, "P1398"}, {17807, "P1399"}, {17808, "P1400"}, {17809, "P1401"},
            {17810, "P1402"}, {17811, "P1403"}, {17812, "P1404"}, {17814, "P1406"}, {17815, "P1407"},
            {17816, "P1408"}, {17817, "P1409"}, {17818, "P1410"}, {17819, "P1411"}, {17820, "P1412"},
            {17821, "P1413"}, {17822, "P1414"}, {17825, "P1417"}, {17826, "P1418"}, {17828, "P1420"},
            {17829, "P1421"}, {17830, "P1422"}, {17831, "P1423"}, {17832, "P1424"}, {17833, "P1425"},
            {17834, "P1426"}, {17840, "P1432"}, {17841, "P1433"}, {17842, "P1434"}, {17843, "P1435"},
            {17844, "P1436"}, {17847, "P1439"}, {17848, "P1440"}, {17849, "P1441"}, {17850, "P1442"},
            {17851, "P1443"}, {17852, "P1444"}, {17853, "P1445"}, {17854, "P1446"}, {17855, "P1447"},
            {17856, "P1448"}, {17857, "P1449"}, {17858, "P1450"}, {17859, "P1451"}, {17860, "P1452"},
            {17861, "P1453"}, {17862, "P1454"}, {17863, "P1455"}, {17864, "P1456"}, {17865, "P1457"},
            {17866, "P1458"}, {17867, "P1459"}, {17868, "P1460"}, {17869, "P1461"}, {17870, "P1462"},
            {17873, "P1465"}, {17874, "P1466"}, {17875, "P1467"}, {17876, "P1468"}, {17877, "P1469"},
            {17878, "P1470"}, {17879, "P1471"}, {17880, "P1472"}, {17881, "P1473"}, {17882, "P1474"},
            {17883, "P1475"}, {17884, "P1476"}, {17885, "P1477"}, {17886, "P1478"}, {17908, "P1500"},
            {17909, "P1501"}, {17910, "P1502"}, {17911, "P1503"}, {17912, "P1504"}, {17913, "P1505"},
            {17914, "P1506"}, {17915, "P1507"}, {17916, "P1508"}, {17917, "P1509"}, {17918, "P1510"},
            {17919, "P1511"}, {17920, "P1512"}, {17921, "P1513"}, {17922, "P1514"}, {17923, "P1515"},
            {17924, "P1516"}, {17925, "P1517"}, {17926, "P1518"}, {17927, "P1519"}, {17928, "P1520"},
            {17929, "P1521"}, {17930, "P1522"}, {17931, "P1523"}, {17933, "P1525"}, {17934, "P1526"},
            {17935, "P1527"}, {17936, "P1528"}, {17937, "P1529"}, {17938, "P1530"}, {17939, "P1531"},
            {17941, "P1533"}, {17942, "P1534"}, {17943, "P1535"}, {17944, "P1536"}, {17945, "P1537"},
            {17946, "P1538"}, {17947, "P1539"}, {17948, "P1540"}, {17949, "P1541"}, {17950, "P1542"},
            {17951, "P1543"}, {17952, "P1544"}, {17953, "P1545"}, {17954, "P1546"}, {17955, "P1547"},
            {17956, "P1548"}, {17957, "P1549"}, {17958, "P1550"}, {17959, "P1551"}, {17960, "P1552"},
            {17961, "P1553"}, {17962, "P1554"}, {17963, "P1555"}, {17964, "P1556"}, {17965, "P1557"},
            {17966, "P1558"}, {17967, "P1559"}, {17968, "P1560"}, {17969, "P1561"}, {17970, "P1562"},
            {17971, "P1563"}, {17972, "P1564"}, {17973, "P1565"}, {17974, "P1566"}, {17975, "P1567"},
            {17976, "P1568"}, {17977, "P1569"}, {17978, "P1570"}, {17979, "P1571"}, {17980, "P1572"},
            {17981, "P1573"}, {17982, "P1574"}, {17983, "P1575"}, {17984, "P1576"}, {17985, "P1577"},
            {17986, "P1578"}, {17987, "P1579"}, {17988, "P1580"}, {17989, "P1581"}, {17990, "P1582"},
            {17991, "P1583"}, {17992, "P1584"}, {17993, "P1585"}, {17994, "P1586"}, {17995, "P1587"},
            {17996, "P1588"}, {18008, "P1600"}, {18010, "P1602"}, {18011, "P1603"}, {18012, "P1604"},
            {18013, "P1605"}, {18014, "P1606"}, {18015, "P1607"}, {18016, "P1608"}, {18017, "P1609"},
            {18019, "P1611"}, {18020, "P1612"}, {18021, "P1613"}, {18022, "P1614"}, {18023, "P1615"},
            {18024, "P1616"}, {18025, "P1617"}, {18026, "P1618"}, {18027, "P1619"}, {18028, "P1620"},
            {18029, "P1621"}, {18030, "P1622"}, {18031, "P1623"}, {18032, "P1624"}, {18033, "P1625"},
            {18034, "P1626"}, {18035, "P1627"}, {18036, "P1628"}, {18037, "P1629"}, {18038, "P1630"},
            {18039, "P1631"}, {18040, "P1632"}, {18041, "P1633"}, {18042, "P1634"}, {18043, "P1635"},
            {18044, "P1636"}, {18045, "P1637"}, {18046, "P1638"}, {18047, "P1639"}, {18048, "P1640"},
            {18049, "P1641"}, {18050, "P1642"}, {18051, "P1643"}, {18052, "P1644"}, {18053, "P1645"},
            {18054, "P1646"}, {18055, "P1647"}, {18056, "P1648"}, {18057, "P1649"}, {18058, "P1650"},
            {18059, "P1651"}, {18060, "P1652"}, {18061, "P1653"}, {18062, "P1654"}, {18063, "P1655"},
            {18064, "P1656"}, {18065, "P1657"}, {18066, "P1658"}, {18084, "P1676"}, {18085, "P1677"},
            {18086, "P1678"}, {18087, "P1679"}, {18089, "P1681"}, {18092, "P1684"}, {18094, "P1686"},
            {18098, "P1690"}, {18099, "P1691"}, {18100, "P1692"}, {18101, "P1693"}, {18102, "P1694"},
            {18112, "P1704"}, {18113, "P1705"}, {18119, "P1711"}, {18124, "P1716"}, {18129, "P1721"},
            {18131, "P1723"}, {18132, "P1724"}, {18134, "P1726"}, {18136, "P1728"}, {18137, "P1729"},
            {18141, "P1733"}, {18147, "P1739"}, {18148, "P1740"}, {18149, "P1741"}, {18150, "P1742"},
            {18151, "P1743"}, {18152, "P1744"}, {18153, "P1745"}, {18154, "P1746"}, {18155, "P1747"},
            {18156, "P1748"}, {18157, "P1749"}, {18158, "P1750"}, {18159, "P1751"}, {18160, "P1752"},
            {18168, "P1760"}, {18169, "P1761"}, {18170, "P1762"}, {18171, "P1763"}, {18172, "P1764"},
            {18173, "P1765"}, {18174, "P1766"}, {18175, "P1767"}, {18176, "P1768"}, {18177, "P1769"},
            {18178, "P1770"}, {18179, "P1771"}, {18180, "P1772"}, {18181, "P1773"}, {18182, "P1774"},
            {18183, "P1775"}, {18184, "P1776"}, {18185, "P1777"}, {18186, "P1778"}, {18189, "P1781"},
            {18190, "P1782"}, {18192, "P1784"}, {18193, "P1785"}, {18194, "P1786"}, {18195, "P1787"},
            {18196, "P1788"}, {18197, "P1789"}, {18198, "P1790"}, {18199, "P1791"}, {18200, "P1792"},
            {18201, "P1793"}, {18203, "P1795"}, {18204, "P1796"}, {18205, "P1797"}, {18206, "P1798"},
            {18207, "P1799"}, {18221, "P1813"}, {18222, "P1814"}, {18223, "P1815"}, {18226, "P1818"},
            {18227, "P1819"}, {18228, "P1820"}, {18231, "P1823"}, {18232, "P1824"}, {18233, "P1825"},
            {18236, "P1828"}, {18237, "P1829"}, {18238, "P1830"}, {18242, "P1834"}, {18243, "P1835"},
            {18249, "P1841"}, {18250, "P1842"}, {18251, "P1843"}, {18252, "P1844"}, {18255, "P1847"},
            {18256, "P1848"}, {18257, "P1849"}, {18258, "P1850"}, {18259, "P1851"}, {18260, "P1852"},
            {18261, "P1853"}, {18262, "P1854"}, {18263, "P1855"}, {18264, "P1856"}, {18265, "P1857"},
            {18266, "P1858"}, {18267, "P1859"}, {18268, "P1860"}, {18269, "P1861"}, {18270, "P1862"},
            {18271, "P1863"}, {18272, "P1864"}, {18273, "P1865"}, {18274, "P1866"}
        };

        private bool _disposed;
        private readonly Context _context;
        private readonly Android.App.Activity _activity;
        private readonly BcReceiverUpdateDisplayDelegate _bcReceiverUpdateDisplayHandler;
        private readonly BcReceiverReceivedDelegate _bcReceiverReceivedHandler;
        private bool? _usbSupport;
        private bool? _mtcBtService;
        private bool? _mtcBtManager;
        private static readonly object LockObject = new object();
        private static int _instanceCount;
        private static string _externalPath;
        private static string _externalWritePath;
        private static string _customStorageMedia;
        private static string _appId;
        private static int _btEnableCounter;
        private readonly BluetoothAdapter _btAdapter;
        private readonly Java.Lang.Object _clipboardManager;
        private readonly WifiManager _maWifi;
        private readonly ConnectivityManager _maConnectivity;
        private readonly UsbManager _usbManager;
        private readonly Android.App.NotificationManager _notificationManager;
        private readonly PowerManager _powerManager;
        private readonly PackageManager _packageManager;
        private readonly Android.App.ActivityManager _activityManager;
        private readonly MtcServiceConnection _mtcServiceConnection;
        private PowerManager.WakeLock _wakeLockScreenBright;
        private PowerManager.WakeLock _wakeLockScreenDim;
        private PowerManager.WakeLock _wakeLockCpu;
        private readonly Tuple<LockType, PowerManager.WakeLock>[] _lockArray;
        private CellularCallback _cellularCallback;
        private Network _mobileNetwork;
        private Handler _btUpdateHandler;
        private Timer _usbCheckTimer;
        private Timer _networkTimer;
        private int _usbDeviceDetectCount;
        private Receiver _bcReceiver;
        private InterfaceType _selectedInterface;
        private string _selectedEnetIp;
        private AlertDialog _activateAlertDialog;
        private AlertDialog _selectMediaAlertDialog;
        private AlertDialog _selectInterfaceAlertDialog;
        private AlertDialog _selectManufacturerAlertDialog;
        private AlertDialog _ftdiWarningAlertDialog;
        private CustomProgressDialog _translateProgress;
        private WebClient _translateWebClient;
        private bool _translateLockAquired;
        private List<string> _yandexLangList;
        private List<string> _yandexTransList;
        private List<string> _yandexReducedStringList;
        private string _yandexCurrentLang;
        private readonly Dictionary<string, Dictionary<string, string>> _yandexTransDict;
        private Dictionary<string, string> _yandexCurrentLangDict;
        private Dictionary<string, List<string>> _vagDtcCodeDict;
        private string _lastEnetSsid = string.Empty;
        private bool? _lastInvertfaceAvailable;
        private bool _usbPermissionRequested;

        public bool Emulator { get; }

        public bool UsbSupport
        {
            get
            {
                if (_usbSupport == null)
                {
                    try
                    {
                        _usbSupport = _usbManager?.DeviceList != null && (Build.VERSION.SdkInt >= BuildVersionCodes.HoneycombMr1);
                    }
                    catch (Exception)
                    {
                        _usbSupport = false;
                    }
                }
                return _usbSupport??false;
            }
        }

        public bool MtcBtService
        {
            get
            {
                if (_mtcBtService == null)
                {
                    try
                    {
                        IList<ApplicationInfo> appList = _packageManager?.GetInstalledApplications(PackageInfoFlags.MatchAll);
                        if (appList != null)
                        {
                            foreach (ApplicationInfo appInfo in appList)
                            {
                                if (string.Compare(appInfo.PackageName, "android.microntek.mtcser", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    _mtcBtService = true;
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        _mtcBtService = false;
                    }
                }
                return _mtcBtService ?? false;
            }
        }

        public bool MtcBtServiceBound
        {
            get
            {
                if (!MtcBtService)
                {
                    return false;
                }
                return _mtcServiceConnection != null && _mtcServiceConnection.Bound;
            }
        }

        public bool MtcBtConnected
        {
            get
            {
                if (!MtcBtService)
                {
                    return false;
                }
                return MtcBtConnectState;
            }
        }

        public bool MtcBtManager
        {
            get
            {
                if (_mtcBtManager == null)
                {
                    try
                    {
                        _mtcBtManager = _packageManager?.GetLaunchIntentForPackage(MtcBtAppName) != null;
                    }
                    catch (Exception)
                    {
                        _mtcBtManager = false;
                    }
                }
                return _mtcBtManager ?? false;
            }
        }

        public MtcServiceConnection MtcServiceConnection => _mtcServiceConnection;

        public bool MtcBtDisconnectWarnShown { get; set; }

        public static object GlobalLockObject => LockObject;

        public static string ExternalPath => _externalPath;

        public static string ExternalWritePath => _externalWritePath;

        public string CustomStorageMedia
        {
            get => _customStorageMedia;
            set => _customStorageMedia = IsWritable(value) ? value : null;
        }

        public static string AppId
        {
            get
            {
                if (string.IsNullOrEmpty(_appId))
                {
                    _appId = Guid.NewGuid().ToString();
                }
                return _appId;
            }
            set => _appId = value;
        }

        public static bool ActivityStartedFromMain { get; set; }

        // default is true, to disable warning at startup
        public static bool MtcBtConnectState { get; set; } = true;

        public static bool BtInitiallyEnabled { get; set; }

        public static bool BtNoEvents { get; set; }

        public static ManufacturerType SelectedManufacturer { get; set; }

        public static BtEnableType BtEnbaleHandling { get; set; }

        public static BtDisableType BtDisableHandling { get; set; }

        public static LockType LockTypeCommunication { get; set; }

        public static LockType LockTypeLogging { get; set; }

        public static bool StoreDataLogSettings { get; set; }

        public static AutoConnectType AutoConnectHandling { get; set; }

        public static bool DoubleClickForAppExit { get; set; }

        public static bool SendDataBroadcast { get; set; }

        public static bool CheckCpuUsage { get; set; }

        public static bool CheckEcuFiles { get; set; }

        public static bool CollectDebugInfo { get; set; }

        public static string YandexApiKey { get; set; }

        public static bool EnableTranslation { get; set; }

        public static bool EnableTranslateRequested { get; set; }

        public static ActivityMain ActivityMainCurrent { get; set; }

        public static EdiabasThread EdiabasThread { get; set; }

        public static JobReader JobReader { get; }

        public InterfaceType SelectedInterface
        {
            get => _selectedInterface;
            set
            {
                if (_selectedInterface != value)
                {
                    _lastEnetSsid = CommActive ? null : string.Empty;
                    _lastInvertfaceAvailable = null;
                }
                _selectedInterface = value;
                SetPreferredNetworkInterface();
            }
        }

        public string SelectedEnetIp
        {
            get => _selectedEnetIp;
            set => _selectedEnetIp = value;
        }

        public Java.Lang.Object ClipboardManager => _clipboardManager;

        public BluetoothAdapter BtAdapter => _btAdapter;

        public WifiManager MaWifi => _maWifi;

        public ConnectivityManager MaConnectivity => _maConnectivity;

        public UsbManager UsbManager => _usbManager;

        // ReSharper disable once ConvertToAutoProperty
        public Android.App.NotificationManager NotificationManager => _notificationManager;

        public PowerManager PowerManager => _powerManager;

        public PackageManager PackageManager => _packageManager;

        // ReSharper disable once ConvertToAutoProperty
        public Android.App.ActivityManager ActivityManager => _activityManager;

        public Receiver BcReceiver => _bcReceiver;

        public XDocument XmlDocDtcCodes { get; set; }

        static ActivityCommon()
        {
            JobReader = new JobReader();
        }

        public ActivityCommon(Context context, BcReceiverUpdateDisplayDelegate bcReceiverUpdateDisplayHandler = null,
            BcReceiverReceivedDelegate bcReceiverReceivedHandler = null, ActivityCommon cacheActivity = null)
        {
            lock (LockObject)
            {
                _instanceCount++;
            }
            _context = context;
            _activity = context as Android.App.Activity;
            _bcReceiverUpdateDisplayHandler = bcReceiverUpdateDisplayHandler;
            _bcReceiverReceivedHandler = bcReceiverReceivedHandler;
            Emulator = IsEmulator();
            _clipboardManager = context?.GetSystemService(Context.ClipboardService);
            _btAdapter = BluetoothAdapter.DefaultAdapter;
            _btUpdateHandler = new Handler();
            _maWifi = (WifiManager)context?.GetSystemService(Context.WifiService);
            _maConnectivity = (ConnectivityManager)context?.GetSystemService(Context.ConnectivityService);
            _usbManager = context?.GetSystemService(Context.UsbService) as UsbManager;
            _notificationManager = context?.GetSystemService(Context.NotificationService) as Android.App.NotificationManager;
            _powerManager = context?.GetSystemService(Context.PowerService) as PowerManager;
            if (_powerManager != null)
            {
                _wakeLockScreenBright = _powerManager.NewWakeLock(WakeLockFlags.ScreenBright | WakeLockFlags.OnAfterRelease, "ScreenBrightLock");
                _wakeLockScreenBright.SetReferenceCounted(false);

                _wakeLockScreenDim = _powerManager.NewWakeLock(WakeLockFlags.ScreenDim | WakeLockFlags.OnAfterRelease, "ScreenDimLock");
                _wakeLockScreenDim.SetReferenceCounted(false);

                _wakeLockCpu = _powerManager.NewWakeLock(WakeLockFlags.Partial, "PartialLock");
                _wakeLockCpu.SetReferenceCounted(false);
                _lockArray = new []
                    {
                        new Tuple<LockType, PowerManager.WakeLock>(LockType.Cpu, _wakeLockCpu),
                        new Tuple<LockType, PowerManager.WakeLock>(LockType.ScreenDim, _wakeLockScreenDim),
                        new Tuple<LockType, PowerManager.WakeLock>(LockType.ScreenBright, _wakeLockScreenBright)
                    };
            }
            _packageManager = context?.PackageManager;
            _activityManager = context?.GetSystemService(Context.ActivityService) as Android.App.ActivityManager;
            _selectedInterface = InterfaceType.None;
            _yandexTransDict = cacheActivity?._yandexTransDict ?? new Dictionary<string, Dictionary<string, string>>();

            if (context != null && ((_bcReceiverUpdateDisplayHandler != null) || (_bcReceiverReceivedHandler != null)))
            {
                _bcReceiver = new Receiver(this);
                LocalBroadcastManager.GetInstance(context).RegisterReceiver(_bcReceiver, new IntentFilter(ForegroundService.NotificationBroadcastAction));
                context.RegisterReceiver(_bcReceiver, new IntentFilter(ForegroundService.ActionBroadcastCommand));
                context.RegisterReceiver(_bcReceiver, new IntentFilter(BluetoothAdapter.ActionStateChanged));
                context.RegisterReceiver(_bcReceiver, new IntentFilter(ConnectivityManager.ConnectivityAction));
                context.RegisterReceiver(_bcReceiver, new IntentFilter(GlobalBroadcastReceiver.NotificationBroadcastAction));
                if (UsbSupport)
                {   // usb handling
                    context.RegisterReceiver(_bcReceiver, new IntentFilter(UsbManager.ActionUsbDeviceDetached));
                    context.RegisterReceiver(_bcReceiver, new IntentFilter(UsbManager.ActionUsbDeviceAttached));
                    context.RegisterReceiver(_bcReceiver, new IntentFilter(ActionUsbPermission));
                    if (Build.VERSION.SdkInt < BuildVersionCodes.Kitkat)
                    {   // attached event fails
                        _usbCheckTimer = new Timer(UsbCheckEvent, null, 1000, 1000);
                    }
                }
            }
            if (context != null && MtcBtService)
            {
                _mtcServiceConnection = new MtcServiceConnection(_context, connected =>
                {
                    try
                    {
                        if (connected)
                        {
                            sbyte state = _mtcServiceConnection.GetBtState();
                            MtcBtConnectState = state != 0;
                            if (!_mtcServiceConnection.GetAutoConnect())
                            {
                                _mtcServiceConnection.SetAutoConnect(true);
                            }
                        }

                        if (_bcReceiverUpdateDisplayHandler != null)
                        {
                            _activity?.RunOnUiThread(() =>
                            {
                                if (_disposed)
                                {
                                    return;
                                }
                                _bcReceiverUpdateDisplayHandler?.Invoke();
                            });
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                });
            }
        }

        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    StopMtcService();
                    if (_btUpdateHandler != null)
                    {
                        try
                        {
                            _btUpdateHandler.RemoveCallbacksAndMessages(null);
                            _btUpdateHandler.Dispose();
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                        _btUpdateHandler = null;
                    }
                    if (_usbCheckTimer != null)
                    {
                        _usbCheckTimer.Dispose();
                        _usbCheckTimer = null;
                    }
                    if (_context != null && _bcReceiver != null)
                    {
                        LocalBroadcastManager.GetInstance(_context).UnregisterReceiver(_bcReceiver);
                        _context.UnregisterReceiver(_bcReceiver);
                        _bcReceiver = null;
                    }
                    if (_wakeLockScreenBright != null)
                    {
                        try
                        {
                            _wakeLockScreenBright.Release();
                            _wakeLockScreenBright.Dispose();
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                        _wakeLockScreenBright = null;
                    }
                    if (_wakeLockScreenDim != null)
                    {
                        try
                        {
                            _wakeLockScreenDim.Release();
                            _wakeLockScreenDim.Dispose();
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                        _wakeLockScreenDim = null;
                    }
                    if (_wakeLockCpu != null)
                    {
                        try
                        {
                            _wakeLockCpu.Release();
                            _wakeLockCpu.Dispose();
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                        _wakeLockCpu = null;
                    }
                    lock (LockObject)
                    {
                        _instanceCount--;
                        if (_instanceCount == 0)
                        {
                            if (EdiabasThread != null)
                            {
                                EdiabasThread.StopThread(true);
                                EdiabasThread.Dispose();
                                EdiabasThread = null;
                            }
                            BluetoothDisableAtExit();
                            MemoryStreamReader.CleanUp();
                        }
                    }
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }

        public static bool CommActive => EdiabasThread != null && EdiabasThread.ThreadRunning();

        public string InterfaceName()
        {
            switch (_selectedInterface)
            {
                case InterfaceType.Bluetooth:
                    return _context.GetString(Resource.String.select_interface_bt);

                case InterfaceType.Enet:
                    return _context.GetString(Resource.String.select_interface_enet);

                case InterfaceType.ElmWifi:
                    return _context.GetString(Resource.String.select_interface_elmwifi);

                case InterfaceType.DeepObdWifi:
                    return _context.GetString(Resource.String.select_interface_deepobdwifi);

                case InterfaceType.Ftdi:
                    return _context.GetString(Resource.String.select_interface_ftdi);
            }
            return string.Empty;
        }

        public string ManufacturerName()
        {
            switch (SelectedManufacturer)
            {
                case ManufacturerType.Bmw:
                    return _context.GetString(Resource.String.select_manufacturer_bmw);

                case ManufacturerType.Audi:
                    return _context.GetString(Resource.String.select_manufacturer_audi);

                case ManufacturerType.Seat:
                    return _context.GetString(Resource.String.select_manufacturer_seat);

                case ManufacturerType.Skoda:
                    return _context.GetString(Resource.String.select_manufacturer_skoda);

                case ManufacturerType.Vw:
                    return _context.GetString(Resource.String.select_manufacturer_vw);

            }
            return string.Empty;
        }

        public bool IsInterfaceEnabled()
        {
            switch (_selectedInterface)
            {
                case InterfaceType.Bluetooth:
                    if (_btAdapter == null)
                    {
                        return false;
                    }
                    return _btAdapter.IsEnabled;

                case InterfaceType.Enet:
                case InterfaceType.ElmWifi:
                case InterfaceType.DeepObdWifi:
                    if (_maWifi == null)
                    {
                        return false;
                    }
                    return _maWifi.IsWifiEnabled;

                case InterfaceType.Ftdi:
                    return true;
            }
            return false;
        }

        public bool IsInterfaceAvailable()
        {
            switch (_selectedInterface)
            {
                case InterfaceType.Bluetooth:
                    if (_btAdapter == null)
                    {
                        return false;
                    }
                    return _btAdapter.IsEnabled;

                case InterfaceType.Enet:
                case InterfaceType.ElmWifi:
                case InterfaceType.DeepObdWifi:
                    NetworkInfo networkInfo = _maConnectivity?.ActiveNetworkInfo;
                    if (networkInfo == null)
                    {
                        return false;
                    }
                    return networkInfo.IsConnected;

                case InterfaceType.Ftdi:
                {
                    List<IUsbSerialDriver> availableDrivers = EdFtdiInterface.GetDriverList(_usbManager);
                    if (availableDrivers.Count <= 0)
                    {
                        return false;
                    }
                    if (!_usbManager.HasPermission(availableDrivers[0].Device))
                    {
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }

        public bool StartForegroundService()
        {
            Intent startServiceIntent = new Intent(_context, typeof(ForegroundService));
            startServiceIntent.SetAction(ForegroundService.ActionStartService);
            _context.StartService(startServiceIntent);
            return true;
        }

        public bool StopForegroundService()
        {
            Intent stopServiceIntent = new Intent(_context, typeof(ForegroundService));
            stopServiceIntent.SetAction(ForegroundService.ActionStopService);
            _context.StopService(stopServiceIntent);
            return true;
        }

        public bool StartMtcService()
        {
            if (_mtcServiceConnection == null)
            {
                return false;
            }
            if (_mtcServiceConnection.Bound)
            {
                return true;
            }
            try
            {
                Intent startServiceIntent = new Intent();
                startServiceIntent.SetComponent(new ComponentName(MtcServiceConnection.ServicePkg, MtcServiceConnection.ServiceClsV1));
                if (!_context.BindService(startServiceIntent, _mtcServiceConnection, Bind.AutoCreate))
                {
                    startServiceIntent.SetComponent(new ComponentName(MtcServiceConnection.ServicePkg, MtcServiceConnection.ServiceClsV2));
                    if (!_context.BindService(startServiceIntent, _mtcServiceConnection, Bind.AutoCreate))
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool StopMtcService()
        {
            if (_mtcServiceConnection == null)
            {
                return false;
            }
            if (!_mtcServiceConnection.Connected)
            {
                return true;
            }
            try
            {
                _context.UnbindService(_mtcServiceConnection);
                _mtcServiceConnection.Bound = false;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool ConnectMtcBtDevice(string deviceAddress)
        {
            if (SelectedInterface == InterfaceType.Bluetooth)
            {
                try
                {
                    if (MtcBtServiceBound)
                    {
                        string mac = deviceAddress.Replace(":", string.Empty);
                        MtcServiceConnection.ConnectObd(mac);
                        return true;
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            return false;
        }

        public static bool ReadHciSnoopLogSettings(out bool enabled, out string logFileName)
        {
            enabled = false;
            logFileName = null;
            try
            {
                string[] confFileList =
                {
                    @"/etc/bluetooth/bt_stack.conf",
                    @"/vendor/etc/bluetooth/bt_stack.conf",
                    @"/system/etc/bluetooth/bt_stack.conf",
                };

                if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                {
                    return false;
                }

                string confFileName = null;
                foreach (string file in confFileList)
                {
                    if (File.Exists(file))
                    {
                        confFileName = file;
                        break;
                    }
                }
                if (string.IsNullOrEmpty(confFileName))
                {
                    return false;
                }
                bool? enabledLocal = null;
                // ReSharper disable once AssignNullToNotNullAttribute
                using (StreamReader file = new StreamReader(confFileName))
                {
                    string line;
                    Regex regexLogOutput = new Regex("^BtSnoopLogOutput\\s*=\\s*(true|false)\\s*$", RegexOptions.IgnoreCase);
                    Regex regexFileName = new Regex("^BtSnoopFileName\\s*=\\s*(.*)$", RegexOptions.IgnoreCase);
                    while ((line = file.ReadLine()) != null)
                    {
                        MatchCollection matchesLogOutput = regexLogOutput.Matches(line);
                        if ((matchesLogOutput.Count == 1) && (matchesLogOutput[0].Groups.Count == 2))
                        {
                            enabledLocal = string.Compare(matchesLogOutput[0].Groups[1].Value, "true", StringComparison.OrdinalIgnoreCase) == 0;
                        }
                        MatchCollection matchesFile = regexFileName.Matches(line);
                        if ((matchesFile.Count == 1) && (matchesFile[0].Groups.Count == 2))
                        {
                            logFileName = matchesFile[0].Groups[1].Value;
                        }
                    }
                }
                if (!enabledLocal.HasValue || string.IsNullOrEmpty(logFileName))
                {
                    return false;
                }
                enabled = enabledLocal.Value;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool GetConfigHciSnoopLog(out bool enable)
        {
            enable = false;
            try
            {
                if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                {
                    return false;
                }
                int value = Android.Provider.Settings.Secure.GetInt(_context.ContentResolver, SettingBluetoothHciLog, 0);
                enable = value != 0;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool SetConfigHciSnoopLog(bool enable)
        {
            try
            {
                if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                {
                    return false;
                }
                // ReSharper disable once UseNullPropagation
                if (_btAdapter == null)
                {
                    return false;
                }
                Java.Lang.Reflect.Method configHciSnoopLog = _btAdapter.Class.GetMethod("configHciSnoopLog", Java.Lang.Boolean.Type);
                // ReSharper disable once UseNullPropagation
                if (configHciSnoopLog == null)
                {
                    return false;
                }
                if (enable)
                {
                    try
                    {
                        Android.Provider.Settings.Secure.PutInt(_context.ContentResolver, SettingBluetoothHciLog, 1);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
                // ReSharper disable once UsePatternMatching
                Java.Lang.Boolean result = configHciSnoopLog.Invoke(_btAdapter, new Java.Lang.Boolean(enable)) as Java.Lang.Boolean;
                if (!enable)
                {
                    try
                    {
                        Android.Provider.Settings.Secure.PutInt(_context.ContentResolver, SettingBluetoothHciLog, 0);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
                if (result == null || result == Java.Lang.Boolean.False)
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool StartApp(String packageName)
        {
            try
            {
                Intent intent = _packageManager?.GetLaunchIntentForPackage(packageName);
                if (intent == null)
                {
                    return false;
                }
                intent.AddCategory(Intent.CategoryLauncher);
                intent.SetFlags(ActivityFlags.NewTask);
                _context.StartActivity(intent);
                return true;
            }
            catch (Exception)
            {
                // ignored
            }
            return false;
        }

        public static object GetInstanceState(Bundle savedInstanceState, object lastInstanceData)
        {
            if (savedInstanceState != null)
            {
                try
                {
                    string xml = savedInstanceState.GetString("InstanceData", string.Empty);
                    if (!string.IsNullOrEmpty(xml))
                    {
                        XmlSerializer xmlSerializer = new XmlSerializer(lastInstanceData.GetType());
                        using (StringReader sr = new StringReader(xml))
                        {
                            object instanceData = xmlSerializer.Deserialize(sr);
                            if (instanceData.GetType() == lastInstanceData.GetType())
                            {
                                return instanceData;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            return lastInstanceData;
        }

        public static bool StoreInstanceState(Bundle outState, object instanceData)
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(instanceData.GetType());
                using (StringWriter sw = new StringWriter())
                {
                    using (XmlWriter writer = XmlWriter.Create(sw))
                    {
                        xmlSerializer.Serialize(writer, instanceData);
                        string xml = sw.ToString();
                        outState.PutString("InstanceData", xml);
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
            return false;
        }

        public static bool IsBtReliable()
        {
            if (Build.VERSION.SdkInt == BuildVersionCodes.M &&
                string.Compare(Build.Model, "px5", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return false;
            }
            return true;
        }

        public static bool IsCpuStatisticsSupported()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
            {
                return false;
            }
            return true;
        }

        public static List<int> GetCpuUsageStatistic()
        {
            if (!IsCpuStatisticsSupported())
            {
                return null;
            }
            string tempString = ExecuteTop();
            if (tempString == null)
            {
                return null;
            }
            MatchCollection matches = Regex.Matches(tempString, "User +(\\d+)%, +System +(\\d+)%, +IOW +(\\d+)%, +IRQ +(\\d+)%", RegexOptions.IgnoreCase);
            if ((matches.Count != 1) || (matches[0].Groups.Count != 5))
            {
                return null;
            }
            List<int> resultList = new List<int>();
            int index = 0;
            foreach (Group group in matches[0].Groups)
            {
                if (index > 0)
                {
                    if (Int32.TryParse(group.Value, out int value))
                    {
                        resultList.Add(value);
                    }
                }
                index++;
            }
            return resultList;
        }

        private static string ExecuteTop()
        {
            Java.Lang.Process process = null;
            Java.IO.BufferedReader input = null;
            try
            {
                string returnString = null;
                process = Java.Lang.Runtime.GetRuntime().Exec("top -n 1");
                input = new Java.IO.BufferedReader(new Java.IO.InputStreamReader(process.InputStream));
                if (process.WaitFor() == 0)
                {
                    for (;;)
                    {
                        returnString = input.ReadLine();
                        if ((returnString == null) || (returnString.Length > 0))
                        {
                            break;
                        }
                    }
                }
                return returnString;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                try
                {
                    input?.Close();
                    process?.Destroy();
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        public bool RegisterInternetCellular()
        {
            if (_maConnectivity == null)
            {
                return false;
            }
            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                if (_networkTimer == null)
                {
                    _networkTimer = new Timer(NetworkTimerEvent, null, 2000, 2000);
                }
                try
                {
#pragma warning disable 618
                    if (_maConnectivity.StartUsingNetworkFeature(ConnectivityType.Mobile, "enableHIPRI") < 0)
                    {
                        return false;
                    }
#pragma warning restore 618
                }
                catch (Exception)
                {
                    return false;
                }
                return true;
            }
            UnRegisterInternetCellular();
            try
            {
                NetworkRequest.Builder builder = new NetworkRequest.Builder();
                builder.AddCapability(NetCapability.Internet);
                builder.AddTransportType(Android.Net.TransportType.Cellular);
                NetworkRequest networkRequest = builder.Build();
                _cellularCallback = new CellularCallback(this);
                _maConnectivity.RequestNetwork(networkRequest, _cellularCallback);
                _maConnectivity.RegisterNetworkCallback(networkRequest, _cellularCallback);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool UnRegisterInternetCellular()
        {
            if (_maConnectivity == null)
            {
                return false;
            }
            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                if (_networkTimer != null)
                {
                    _networkTimer.Dispose();
                    _networkTimer = null;
                }
                try
                {
#pragma warning disable 618
                    _maConnectivity.StopUsingNetworkFeature(ConnectivityType.Mobile, "enableHIPRI");
#pragma warning restore 618
                }
                catch (Exception)
                {
                    return false;
                }
                return true;
            }
            if (_cellularCallback != null)
            {
                try
                {
                    _maConnectivity.UnregisterNetworkCallback(_cellularCallback);
                }
                catch (Exception)
                {
                    return false;
                }
                _cellularCallback = null;
            }
            return true;
        }

        public bool SetPreferredNetworkInterface(bool hipri = true)
        {
            bool forceMobile = false;
            switch (_selectedInterface)
            {
                case InterfaceType.Enet:
                case InterfaceType.ElmWifi:
                case InterfaceType.DeepObdWifi:
                    forceMobile = true;
                    break;
            }

            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                if (hipri)
                {
                    try
                    {
#pragma warning disable 618
                        if (_maConnectivity.StartUsingNetworkFeature(ConnectivityType.Mobile, "enableHIPRI") < 0)
                        {
                            return false;
                        }
#pragma warning restore 618
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
                return true;
            }

            Network defaultNetwork = null;
            if (forceMobile && _mobileNetwork != null)
            {
                NetworkInfo networkInfo = _maConnectivity.GetNetworkInfo(_mobileNetwork);
                if (networkInfo != null && networkInfo.IsConnected && networkInfo.Type == ConnectivityType.Mobile)
                {
                    defaultNetwork = _mobileNetwork;
                }
            }
            //Android.Util.Log.WriteLine(Android.Util.LogPriority.Debug, "Network", (defaultNetwork != null) ? "Mobile selected" : "Mobile not selected");
            try
            {
                if (Build.VERSION.SdkInt < BuildVersionCodes.M)
                {
#pragma warning disable 618
                    ConnectivityManager.SetProcessDefaultNetwork(defaultNetwork);
#pragma warning restore 618
                }
                else
                {
                    _maConnectivity.BindProcessToNetwork(defaultNetwork);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool AllowAdapterConfig(string deviceAddress)
        {
            switch (_selectedInterface)
            {
                case InterfaceType.Bluetooth:
                {
                    if (string.IsNullOrEmpty(deviceAddress))
                    {
                        return false;
                    }
                    string[] stringList = deviceAddress.Split('#', ';');
                    if (stringList.Length > 1)
                    {
                        if (string.Compare(stringList[1], EdBluetoothInterface.RawTag, StringComparison.OrdinalIgnoreCase) == 0)
                        {   // allow flashing of corrupted Deep OBD adapters
                            return true;
                        }
                        return false;
                    }
                    return true;
                }

                case InterfaceType.Enet:
                    if (string.IsNullOrEmpty(GetEnetAdapterIp()))
                    {
                        return false;
                    }
                    return true;

                case InterfaceType.ElmWifi:
                    return false;

                case InterfaceType.DeepObdWifi:
                    return true;

                case InterfaceType.Ftdi:
                    return true;
            }
            return false;
        }

        public string GetEnetAdapterIp()
        {
            if ((_maWifi == null) || !_maWifi.IsWifiEnabled)
            {
                return null;
            }
            WifiInfo wifiInfo = _maWifi.ConnectionInfo;
            if (wifiInfo != null && _maWifi.DhcpInfo != null &&
                !string.IsNullOrEmpty(wifiInfo.SSID) && wifiInfo.SSID.Contains(AdapterSsid))
            {
                return ConvertIpAddress(_maWifi.DhcpInfo.ServerAddress);
            }
            return null;
        }

        public bool ElmWifiAdapterValid()
        {
            if ((_maWifi == null) || !_maWifi.IsWifiEnabled)
            {
                return false;
            }
            WifiInfo wifiInfo = _maWifi.ConnectionInfo;
            if (wifiInfo != null && _maWifi.DhcpInfo != null)
            {
                string adapterIp = ConvertIpAddress(_maWifi.DhcpInfo.ServerAddress);
                if (string.Compare(adapterIp, EdElmWifiInterface.ElmIp, StringComparison.Ordinal) != 0)
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        public string ConvertIpAddress(int ipAddress)
        {
            if (Java.Nio.ByteOrder.NativeOrder().Equals(Java.Nio.ByteOrder.LittleEndian))
            {
                ipAddress = Java.Lang.Integer.ReverseBytes(ipAddress);
            }
            byte[] ipByteArray = Java.Math.BigInteger.ValueOf(ipAddress).ToByteArray();
            try
            {
                Java.Net.InetAddress inetAddress = Java.Net.InetAddress.GetByAddress(ipByteArray);
                string address = inetAddress.HostAddress;
                if (address == null)
                {
                    string text = inetAddress.ToString();
                    Match match = Regex.Match(text, @"\d+\.\d+\.\d+\.\d+$", RegexOptions.Singleline);
                    if (match.Success)
                    {
                        address = match.Value;
                    }
                }
                return address;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool EnetAdapterConfig()
        {
            if (SelectedInterface == InterfaceType.Enet)
            {
                string adapterIp = GetEnetAdapterIp();
                if (!string.IsNullOrEmpty(adapterIp))
                {
                    new AlertDialog.Builder(_context)
                    .SetMessage(Resource.String.enet_adapter_web_info)
                    .SetTitle(Resource.String.alert_title_info)
                    .SetNeutralButton(Resource.String.button_ok, (s, e) =>
                    {
                        try
                        {
                            _context.StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(@"http://" + adapterIp)));
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    })
                    .Show();
                    return true;
                }
            }
            return false;
        }

        public bool IsConnectedToWifiAdapter()
        {
            if (!string.IsNullOrEmpty(GetEnetAdapterIp()))
            {
                return true;
            }
            if (ElmWifiAdapterValid())
            {
                return true;
            }
            return false;
        }

        public bool ShowWifiConnectedWarning(WifiConnectedWarnDelegate handler)
        {
            if (!IsConnectedToWifiAdapter())
            {
                handler();
                return true;
            }
            new AlertDialog.Builder(_context)
            .SetMessage(Resource.String.connected_with_wifi_adapter)
            .SetTitle(Resource.String.alert_title_warning)
            .SetPositiveButton(Resource.String.button_yes, (s, e) =>
            {
                try
                {
                    _maWifi?.SetWifiEnabled(false);
                }
                catch (Exception)
                {
                    // ignored
                }
                handler();
            })
            .SetNegativeButton(Resource.String.button_no, (s, e) =>
            {
                handler();
            })
            .Show();
            return false;
        }

        public bool ShowConnectWarning(EnetSsidWarnDelegate handler)
        {
            if (_selectedInterface == InterfaceType.Bluetooth)
            {
                if (!MtcBtService)
                {
                    return false;
                }
                if (MtcBtConnected)
                {
                    return false;
                }
                if (MtcBtDisconnectWarnShown)
                {
                    return false;
                }
                bool ignoreDismiss = false;
                AlertDialog alertDialog = new AlertDialog.Builder(_context)
                    .SetMessage(Resource.String.mtc_disconnect_warn)
                    .SetTitle(Resource.String.alert_title_warning)
                    .SetPositiveButton(Resource.String.button_yes, (s, e) =>
                    {
                    })
                    .SetNegativeButton(Resource.String.button_no, (s, e) =>
                    {
                        ignoreDismiss = true;
                    })
                    .Show();
                alertDialog.DismissEvent += (sender, args) =>
                {
                    if (_disposed)
                    {
                        return;
                    }
                    if (!ignoreDismiss)
                    {
                        handler(true);
                    }
                };
                MtcBtDisconnectWarnShown = true;
                return true;
            }
            if (_selectedInterface == InterfaceType.ElmWifi || _selectedInterface == InterfaceType.DeepObdWifi)
            {
                if (ElmWifiAdapterValid())
                {
                    return false;
                }
                bool ignoreDismiss = false;
                int resourceId = _selectedInterface == InterfaceType.ElmWifi
                    ? Resource.String.elmwifi_adapter_warn
                    : Resource.String.deepobdwifi_adapter_warn;
                AlertDialog alertDialog = new AlertDialog.Builder(_context)
                .SetMessage(resourceId)
                .SetTitle(Resource.String.alert_title_warning)
                .SetPositiveButton(Resource.String.button_yes, (s, e) =>
                {
                    ignoreDismiss = true;
                    ShowWifiSettings((sender, args) =>
                    {
                        handler(false);
                    });
                })
                .SetNegativeButton(Resource.String.button_no, (s, e) => { })
                .Show();
                alertDialog.DismissEvent += (sender, args) =>
                {
                    if (_disposed)
                    {
                        return;
                    }
                    if (!ignoreDismiss)
                    {
                        handler(false);
                    }
                };
                return true;
            }

            if (_selectedInterface == InterfaceType.Enet)
            {
                bool result = false;
                string enetSsid = "NoSsid";
                if ((_maWifi != null) && _maWifi.IsWifiEnabled)
                {
                    WifiInfo wifiInfo = _maWifi.ConnectionInfo;
                    if (wifiInfo != null && _maWifi.DhcpInfo != null && !string.IsNullOrEmpty(wifiInfo.SSID))
                    {
                        enetSsid = wifiInfo.SSID;
                    }
                }
                if (_lastEnetSsid == null)
                {
                    _lastEnetSsid = enetSsid;
                }
                if (!IsEmulator() && string.Compare(_lastEnetSsid, enetSsid, StringComparison.Ordinal) != 0)
                {
                    _lastEnetSsid = enetSsid;
                    if (!enetSsid.Contains(AdapterSsid))
                    {
                        bool ignoreDismiss = false;
                        AlertDialog alertDialog = new AlertDialog.Builder(_context)
                        .SetMessage(Resource.String.enet_adapter_ssid_warn)
                        .SetTitle(Resource.String.alert_title_warning)
                        .SetPositiveButton(Resource.String.button_yes, (s, e) =>
                        {
                            ignoreDismiss = true;
                            ShowWifiSettings((sender, args) =>
                            {
                                handler(false);
                            });
                        })
                        .SetNegativeButton(Resource.String.button_no, (s, e) => { })
                        .Show();
                        alertDialog.DismissEvent += (sender, args) =>
                        {
                            if (_disposed)
                            {
                                return;
                            }
                            if (!ignoreDismiss)
                            {
                                handler(true);
                            }
                        };

                        result = true;
                    }
                }
                return result;
            }
            return false;
        }

        public bool ShowWifiSettings(EventHandler handler)
        {
            if (_selectedInterface == InterfaceType.Enet)
            {
                AlertDialog alterDialog = new AlertDialog.Builder(_context)
                .SetMessage(Resource.String.enet_adapter_wifi_info)
                .SetTitle(Resource.String.alert_title_info)
                .SetNeutralButton(Resource.String.button_ok, (s, e) =>
                {
                    try
                    {
                        if (_maWifi != null && !_maWifi.IsWifiEnabled)
                        {
                            _maWifi.SetWifiEnabled(true);
                        }
                        _context.StartActivity(new Intent(Android.Provider.Settings.ActionWifiSettings));
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                })
                .Show();
                alterDialog.DismissEvent += (sender, args) =>
                {
                    if (_disposed)
                    {
                        return;
                    }
                    handler(sender, args);
                };
                return true;
            }
            try
            {
                if (_maWifi != null && !_maWifi.IsWifiEnabled)
                {
                    _maWifi.SetWifiEnabled(true);
                }
                _context.StartActivity(new Intent(Android.Provider.Settings.ActionWifiSettings));
            }
            catch (Exception)
            {
                // ignored
            }
            return true;
        }

        public void ShowAlert(string message, int titleId)
        {
            new AlertDialog.Builder(_context)
            .SetMessage(message)
            .SetTitle(titleId)
            .SetNeutralButton(Resource.String.button_ok, (s, e) => { })
            .Show();
        }

        public void SelectMedia(EventHandler<DialogClickEventArgs> handler)
        {
            if (_selectMediaAlertDialog != null)
            {
                return;
            }
            AlertDialog.Builder builder = new AlertDialog.Builder(_context);
            builder.SetTitle(Resource.String.select_media);
            ListView listView = new ListView(_context);

            List<string> mediaNames = GetAllStorageMedia();
            int mediaIndex = 0;
            if (!string.IsNullOrEmpty(_customStorageMedia))
            {
                int index = 0;
                foreach (string name in mediaNames)
                {
                    if (string.CompareOrdinal(name, _customStorageMedia) == 0)
                    {
                        mediaIndex = index + 1;
                        break;
                    }
                    index++;
                }
            }
            List<string> displayNames = new List<string>();
            foreach (string name in mediaNames)
            {
                const int maxLength = 40;
                string displayName = name;
                try
                {
                    FileSystemBlockInfo blockInfo = GetFileSystemBlockInfo(name);
                    displayName = String.Format(new FileSizeFormatProvider(), "{0} ({1:fs1}/{2:fs1} {3})",
                        name, blockInfo.AvailableSizeBytes, blockInfo.TotalSizeBytes, _context.GetString(Resource.String.free_space));
                    if (displayName.Length > maxLength)
                    {
                        displayName = "..." + displayName.Substring(displayName.Length - maxLength);
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
                displayNames.Add(displayName);
            }
            displayNames.Insert(0, _context.GetString(Resource.String.default_media));

            ArrayAdapter<string> adapter = new ArrayAdapter<string>(_context,
                Android.Resource.Layout.SimpleListItemSingleChoice, displayNames);
            listView.Adapter = adapter;
            listView.ChoiceMode = ChoiceMode.Single;
            listView.SetItemChecked(mediaIndex, true);
            builder.SetView(listView);
            builder.SetPositiveButton(Resource.String.button_ok, (sender, args) =>
            {
                switch (listView.CheckedItemPosition)
                {
                    case 0:
                        _customStorageMedia = null;
                        handler(sender, args);
                        break;

                    default:
                        _customStorageMedia = mediaNames[listView.CheckedItemPosition - 1];
                        handler(sender, args);
                        break;
                }
            });
            builder.SetNegativeButton(Resource.String.button_abort, (sender, args) =>
            {
            });
            _selectMediaAlertDialog = builder.Show();
            _selectMediaAlertDialog.DismissEvent += (sender, args) => { _selectMediaAlertDialog = null; };
        }

        public void SelectInterface(EventHandler<DialogClickEventArgs> handler)
        {
            if (_selectInterfaceAlertDialog != null)
            {
                return;
            }
            AlertDialog.Builder builder = new AlertDialog.Builder(_context);
            builder.SetTitle(Resource.String.select_interface);
            ListView listView = new ListView(_context);

            List<InterfaceType> interfaceTypes = new List<InterfaceType>();
            // ReSharper disable once UseObjectOrCollectionInitializer
            List<string> interfaceNames = new List<string>();
            interfaceNames.Add(_context.GetString(Resource.String.select_interface_bt));
            interfaceTypes.Add(InterfaceType.Bluetooth);

            if (SelectedManufacturer == ManufacturerType.Bmw)
            {
                interfaceNames.Add(_context.GetString(Resource.String.select_interface_enet));
                interfaceTypes.Add(InterfaceType.Enet);

                interfaceNames.Add(_context.GetString(Resource.String.select_interface_elmwifi));
                interfaceTypes.Add(InterfaceType.ElmWifi);
            }

            interfaceNames.Add(_context.GetString(Resource.String.select_interface_deepobdwifi));
            interfaceTypes.Add(InterfaceType.DeepObdWifi);

            if (SelectedManufacturer == ManufacturerType.Bmw && UsbSupport)
            {
                interfaceNames.Add(_context.GetString(Resource.String.select_interface_ftdi));
                interfaceTypes.Add(InterfaceType.Ftdi);
            }
            ArrayAdapter<string> adapter = new ArrayAdapter<string>(_context,
                Android.Resource.Layout.SimpleListItemSingleChoice, interfaceNames.ToArray());
            listView.Adapter = adapter;
            listView.ChoiceMode = ChoiceMode.Single;
            int index = interfaceTypes.IndexOf(_selectedInterface);
            if (index >= 0)
            {
                listView.SetItemChecked(index, true);
            }
            builder.SetView(listView);
            builder.SetPositiveButton(Resource.String.button_ok, (sender, args) =>
            {
                int pos = listView.CheckedItemPosition;
                if (pos >= 0 && pos < interfaceTypes.Count)
                {
                    SelectedInterface = interfaceTypes[pos];
                    handler(sender, args);
                }
            });
            builder.SetNegativeButton(Resource.String.button_abort, (sender, args) =>
                {
                });
            _selectInterfaceAlertDialog = builder.Show();
            _selectInterfaceAlertDialog.DismissEvent += (sender, args) => { _selectInterfaceAlertDialog = null; };
        }

        public void SelectManufacturer(EventHandler<DialogClickEventArgs> handler)
        {
            if (_selectManufacturerAlertDialog != null)
            {
                return;
            }
            AlertDialog.Builder builder = new AlertDialog.Builder(_context);
            builder.SetTitle(Resource.String.select_manufacturer);
            ListView listView = new ListView(_context);

            List<string> manufacturerNames = new List<string>
            {
                _context.GetString(Resource.String.select_manufacturer_bmw),
                _context.GetString(Resource.String.select_manufacturer_audi),
                _context.GetString(Resource.String.select_manufacturer_seat),
                _context.GetString(Resource.String.select_manufacturer_skoda),
                _context.GetString(Resource.String.select_manufacturer_vw),
            };
            ArrayAdapter<string> adapter = new ArrayAdapter<string>(_context,
                Android.Resource.Layout.SimpleListItemSingleChoice, manufacturerNames.ToArray());
            listView.Adapter = adapter;
            listView.ChoiceMode = ChoiceMode.Single;
            switch (SelectedManufacturer)
            {
                case ManufacturerType.Bmw:
                    listView.SetItemChecked(0, true);
                    break;

                case ManufacturerType.Audi:
                    listView.SetItemChecked(1, true);
                    break;

                case ManufacturerType.Seat:
                    listView.SetItemChecked(2, true);
                    break;

                case ManufacturerType.Skoda:
                    listView.SetItemChecked(3, true);
                    break;

                case ManufacturerType.Vw:
                    listView.SetItemChecked(4, true);
                    break;
            }
            builder.SetView(listView);
            builder.SetPositiveButton(Resource.String.button_ok, (sender, args) =>
            {
                switch (listView.CheckedItemPosition)
                {
                    case 0:
                        SelectedManufacturer = ManufacturerType.Bmw;
                        handler(sender, args);
                        break;

                    case 1:
                        SelectedManufacturer = ManufacturerType.Audi;
                        handler(sender, args);
                        break;

                    case 2:
                        SelectedManufacturer = ManufacturerType.Seat;
                        handler(sender, args);
                        break;

                    case 3:
                        SelectedManufacturer = ManufacturerType.Skoda;
                        handler(sender, args);
                        break;

                    case 4:
                        SelectedManufacturer = ManufacturerType.Vw;
                        handler(sender, args);
                        break;
                }
            });
            builder.SetNegativeButton(Resource.String.button_abort, (sender, args) =>
            {
            });
            _selectManufacturerAlertDialog = builder.Show();
            _selectManufacturerAlertDialog.DismissEvent += (sender, args) => { _selectManufacturerAlertDialog = null; };
        }

        public void EnableInterface()
        {
            switch (_selectedInterface)
            {
                case InterfaceType.Bluetooth:
                    if (_btAdapter == null)
                    {
                        Toast.MakeText(_context, Resource.String.bt_not_available, ToastLength.Long).Show();
                        break;
                    }
                    if (!_btAdapter.IsEnabled)
                    {
                        try
                        {
#pragma warning disable 0618
                            _btAdapter.Enable();
#pragma warning restore 0618
                            _btEnableCounter = 2;
                            if (_bcReceiverUpdateDisplayHandler != null)
                            {   // some device don't send the update event
                                _btUpdateHandler.PostDelayed(() =>
                                {
                                    if (_btUpdateHandler == null)
                                    {
                                        return;
                                    }
                                    if (_btEnableCounter == 2)
                                    {   // update event not received
                                        BtNoEvents = true;
                                    }
                                    _bcReceiverUpdateDisplayHandler?.Invoke();
                                }, 1000);
                            }
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                    break;

                case InterfaceType.Enet:
                case InterfaceType.ElmWifi:
                case InterfaceType.DeepObdWifi:
                    if (_maWifi == null)
                    {
                        Toast.MakeText(_context, Resource.String.wifi_not_available, ToastLength.Long).Show();
                        break;
                    }
                    if (!_maWifi.IsWifiEnabled)
                    {
                        _lastEnetSsid = string.Empty;
                        try
                        {
                            _maWifi.SetWifiEnabled(true);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                    break;
            }
        }

        public bool RequestInterfaceEnable(EventHandler handler)
        {
            if (_activateAlertDialog != null)
            {
                return true;
            }
            if (IsInterfaceAvailable())
            {
                return false;
            }
            if (IsInterfaceEnabled())
            {
                return false;
            }
            bool ignoreDismiss = false;
            switch (_selectedInterface)
            {
                case InterfaceType.Bluetooth:
                    if (MtcBtService)
                    {
                        EnableInterface();
                        return false;
                    }
                    switch (BtEnbaleHandling)
                    {
                        case BtEnableType.Ask:
                            break;

                        case BtEnableType.Always:
                            EnableInterface();
                            return false;

                        default:
                            return false;
                    }
                    _activateAlertDialog = new AlertDialog.Builder(_context)
                        .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                        {
                            EnableInterface();
                        })
                        .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                        {
                        })
                        .SetCancelable(true)
                        .SetMessage(Resource.String.bt_enable)
                        .SetTitle(Resource.String.alert_title_question)
                        .Show();
                    break;

                case InterfaceType.Enet:
                case InterfaceType.ElmWifi:
                case InterfaceType.DeepObdWifi:
                    _activateAlertDialog = new AlertDialog.Builder(_context)
                        .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                        {
                            EnableInterface();
                        })
                        .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                        {
                        })
                        .SetNeutralButton(Resource.String.button_select, (sender, args) =>
                        {
                            ignoreDismiss = true;
                            EnableInterface();
                            ShowWifiSettings((o, eventArgs) =>
                            {
                                _activateAlertDialog = null;
                                handler(sender, args);
                            });
                        })
                        .SetCancelable(true)
                        .SetMessage(Resource.String.wifi_enable)
                        .SetTitle(Resource.String.alert_title_question)
                        .Show();
                    break;

                default:
                    return false;
            }
            _activateAlertDialog.DismissEvent += (sender, args) =>
            {
                if (_disposed)
                {
                    return;
                }
                if (!ignoreDismiss)
                {
                    _activateAlertDialog = null;
                    handler(sender, args);
                }
            };
            return true;
        }

        public bool IsBluetoothEnabled()
        {
            if (_btAdapter == null)
            {
                return false;
            }
            return _btAdapter.IsEnabled;
        }

        public static bool IsBluetoothEnabledByApp()
        {
            return _btEnableCounter > 0;
        }

        public bool IsBluetoothConnected()
        {
            if (!IsBluetoothEnabled())
            {
                return false;
            }

            bool result = false;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.IceCreamSandwich)
            {
                foreach (ProfileType profile in Enum.GetValues(typeof(ProfileType)))
                {
                    try
                    {
                        if (_btAdapter.GetProfileConnectionState(profile) != ProfileState.Disconnected)
                        {
                            result = true;
                            break;
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
            return result;
        }

        public bool BluetoothDisable()
        {
            if (_btAdapter == null)
            {
                return false;
            }
            try
            {
                return _btAdapter.Disable();
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool BluetoothDisableAtExit()
        {
            if (!ActivityStartedFromMain && !BtInitiallyEnabled && BtDisableHandling == BtDisableType.DisableIfByApp &&
                IsBluetoothEnabledByApp() && !IsBluetoothConnected() && !MtcBtService &&
                !CommActive)
            {
                return BluetoothDisable();
            }
            return false;
        }

        public bool RequestBluetoothDeviceSelect(int requestCode, string appDataDir, EventHandler<DialogClickEventArgs> handler)
        {
            if (SelectedInterface != InterfaceType.Bluetooth)
            {
                return true;
            }
            if (!IsInterfaceAvailable())
            {
                return true;
            }
            new AlertDialog.Builder(_context)
                .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                {
                    if (SelectBluetoothDevice(requestCode, appDataDir))
                    {
                        handler(sender, args);
                    }
                })
                .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                {
                })
                .SetCancelable(true)
                .SetMessage(Resource.String.bt_device_select)
                .SetTitle(Resource.String.alert_title_question)
                .Show();
            return false;
        }

        public bool SelectBluetoothDevice(int requestCode, string appDataDir)
        {
            if (!IsInterfaceAvailable())
            {
                return false;
            }
            if (SelectedInterface != InterfaceType.Bluetooth)
            {
                return false;
            }
            Intent serverIntent = new Intent(_context, typeof(DeviceListActivity));
            serverIntent.PutExtra(XmlToolActivity.ExtraAppDataDir, appDataDir);
            _activity?.StartActivityForResult(serverIntent, requestCode);
            return true;
        }

        public bool SelectEnetIp(EventHandler<DialogClickEventArgs> handler)
        {
            CustomProgressDialog progress = new CustomProgressDialog(_context);
            progress.SetMessage(_context.GetString(Resource.String.select_enet_ip_search));
            progress.ButtonAbort.Visibility = ViewStates.Gone;
            progress.Show();
            SetLock(LockTypeCommunication);

            Thread detectThread = new Thread(() =>
            {
                EdInterfaceEnet edInterface = new EdInterfaceEnet
                {
                    ConnectParameter = new EdInterfaceEnet.ConnectParameterType(_maConnectivity)
                };
                List<IPAddress> detectedVehicles = edInterface.DetectedVehicles("auto:all");
                edInterface.Dispose();
                _activity?.RunOnUiThread(() =>
                {
                    if (_disposed)
                    {
                        return;
                    }
                    if (progress != null)
                    {
                        progress.Dismiss();
                        progress.Dispose();
                        progress = null;
                        SetLock(LockType.None);
                    }
                    AlertDialog.Builder builder = new AlertDialog.Builder(_context);
                    builder.SetTitle(Resource.String.select_enet_ip);
                    ListView listView = new ListView(_context);

                    List<string> interfaceNames = new List<string>
                    {
                        _context.GetString(Resource.String.select_enet_ip_auto)
                    };
                    int selIndex = 0;
                    int index = 0;
                    if (detectedVehicles != null)
                    {
                        foreach (IPAddress ipAddress in detectedVehicles)
                        {
                            if (!string.IsNullOrEmpty(_selectedEnetIp) &&
                                string.Compare(_selectedEnetIp, ipAddress.ToString(), StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                selIndex = index + 1;
                            }
                            interfaceNames.Add(ipAddress.ToString());
                            index++;
                        }
                    }
                    ArrayAdapter<string> adapter = new ArrayAdapter<string>(_context,
                        Android.Resource.Layout.SimpleListItemSingleChoice, interfaceNames.ToArray());
                    listView.Adapter = adapter;
                    listView.ChoiceMode = ChoiceMode.Single;
                    listView.SetItemChecked(selIndex, true);
                    builder.SetView(listView);
                    builder.SetPositiveButton(Resource.String.button_ok, (sender, args) =>
                    {
                        switch (listView.CheckedItemPosition)
                        {
                            case 0:
                                _selectedEnetIp = null;
                                handler(sender, args);
                                break;

                            default:
                                if (detectedVehicles != null && listView.CheckedItemPosition >= 1 &&
                                    listView.CheckedItemPosition - 1 < detectedVehicles.Count)
                                {
                                    _selectedEnetIp = detectedVehicles[listView.CheckedItemPosition - 1].ToString();
                                    handler(sender, args);
                                }
                                break;
                        }
                    });
                    builder.SetNegativeButton(Resource.String.button_abort, (sender, args) =>
                    {
                    });
                    builder.Show();
                });
            });
            detectThread.Start();
            return true;
        }

        public void RequestUsbPermission(UsbDevice usbDevice)
        {
            if (!UsbSupport)
            {
                return;
            }
            if (_usbPermissionRequested)
            {
                return;
            }
            if (usbDevice == null)
            {
                List<IUsbSerialDriver> availableDrivers = EdFtdiInterface.GetDriverList(_usbManager);
                if (availableDrivers.Count > 0)
                {
                    usbDevice = availableDrivers[0].Device;
                }
            }
            if (usbDevice != null)
            {
                if (!_usbManager.HasPermission(usbDevice) && EdFtdiInterface.IsValidUsbDevice(usbDevice, out bool fakeDevice))
                {
                    if (fakeDevice)
                    {
                        if (_ftdiWarningAlertDialog == null)
                        {
                            _ftdiWarningAlertDialog = new AlertDialog.Builder(_context)
                                .SetNeutralButton(Resource.String.button_ok, (sender, args) =>
                                {
                                })
                                .SetCancelable(true)
                                .SetMessage(Resource.String.ftdi_fake_device)
                                .SetTitle(Resource.String.alert_title_warning)
                                .Show();
                            _ftdiWarningAlertDialog.DismissEvent += (sender, args) =>
                            {
                                _ftdiWarningAlertDialog = null;
                            };
                        }
                    }
                    Android.App.PendingIntent intent = Android.App.PendingIntent.GetBroadcast(_context, 0, new Intent(ActionUsbPermission), 0);
                    try
                    {
                        _usbManager.RequestPermission(usbDevice, intent);
                        _usbPermissionRequested = true;
                    }
                    catch (Exception)
                    {
                        // seems to crash on Samsung 5.1.1 with android.permission.sec.MDM_APP_MGMT
                    }
                }
            }
        }

        public LockType GetLock()
        {
            foreach (Tuple<LockType, PowerManager.WakeLock> tempLock in _lockArray)
            {
                if (tempLock.Item2.IsHeld)
                {
                    return tempLock.Item1;
                }
            }
            return LockType.None;
        }

        public bool SetLock(LockType lockType)
        {
            bool result = true;
            PowerManager.WakeLock wakeLock = null;
            switch (lockType)
            {
                case LockType.None:
                    break;

                case LockType.Cpu:
                    wakeLock = _wakeLockCpu;
                    break;

                case LockType.ScreenDim:
                    wakeLock = _wakeLockScreenDim;
                    break;

                case LockType.ScreenBright:
                    wakeLock = _wakeLockScreenBright;
                    break;
            }

            LockType currentLock = GetLock();

            if (wakeLock != null)
            {
                if (currentLock != LockType.None)
                {
                    result = false;
                }
                else
                {
                    try
                    {
                        wakeLock.Acquire();
                    }
                    catch (Exception)
                    {
                        result = false;
                    }
                }
            }
            else
            {
                foreach (Tuple<LockType, PowerManager.WakeLock> tempLock in _lockArray)
                {
                    try
                    {
                        if (tempLock.Item2.IsHeld)
                        {
                            tempLock.Item2.Release();
                        }
                    }
                    catch (Exception)
                    {
                        result = false;
                    }
                }
            }
            return result;
        }

        public bool SetClipboardText(string text)
        {
            try
            {
                // ReSharper disable once UsePatternMatching
                ClipboardManager clipboardManagerNew = _clipboardManager as ClipboardManager;
                if (clipboardManagerNew != null)
                {
                    ClipData clipData = ClipData.NewPlainText(@"text", text);
                    if (clipData != null)
                    {
                        clipboardManagerNew.PrimaryClip = clipData;
                    }
                }
                else
                {
#pragma warning disable 618
                    // ReSharper disable once UsePatternMatching
                    Android.Text.ClipboardManager clipboardManagerOld = _clipboardManager as Android.Text.ClipboardManager;
#pragma warning restore 618
                    if (clipboardManagerOld != null)
                    {
                        clipboardManagerOld.Text = text;
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string GetClipboardText()
        {
            try
            {
                string clipText = null;
                // ReSharper disable once UsePatternMatching
                ClipboardManager clipboardManagerNew = _clipboardManager as ClipboardManager;
                if (clipboardManagerNew != null)
                {
                    if (clipboardManagerNew.HasPrimaryClip)
                    {
                        ClipData.Item item = clipboardManagerNew.PrimaryClip.GetItemAt(0);
                        if (item != null)
                        {
                            clipText = clipboardManagerNew.PrimaryClipDescription.HasMimeType(ClipDescription.MimetypeTextPlain) ?
                                item.Text : item.CoerceToText(_context);
                        }
                    }
                }
                else
                {
#pragma warning disable 618
                    Android.Text.ClipboardManager clipboardManagerOld = _clipboardManager as Android.Text.ClipboardManager;
#pragma warning restore 618
                    clipText = clipboardManagerOld?.Text;
                }
                return clipText;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public EdInterfaceBase GetEdiabasInterfaceClass()
        {
            if (SelectedManufacturer != ManufacturerType.Bmw)
            {
                return new EdInterfaceEdic();
            }
            if (SelectedInterface == InterfaceType.Enet)
            {
                return new EdInterfaceEnet();
            }
            return new EdInterfaceObd();
        }

        public void SetEdiabasInterface(EdiabasNet ediabas, string btDeviceAddress)
        {
            object connectParameter = null;
            // ReSharper disable once CanBeReplacedWithTryCastAndCheckForNull
            if (ediabas.EdInterfaceClass is EdInterfaceObd)
            {
                ((EdInterfaceObd) ediabas.EdInterfaceClass).UdsDtcStatusOverride = UdsDtcStatusOverride;
                if (SelectedInterface == InterfaceType.Ftdi)
                {
                    ((EdInterfaceObd)ediabas.EdInterfaceClass).ComPort = "FTDI0";
                    connectParameter = new EdFtdiInterface.ConnectParameterType(_usbManager);
                }
                else if (SelectedInterface == InterfaceType.ElmWifi)
                {
                    ((EdInterfaceObd)ediabas.EdInterfaceClass).ComPort = "ELM327WIFI";
                    connectParameter = new EdElmWifiInterface.ConnectParameterType(_maConnectivity);
                }
                else if (SelectedInterface == InterfaceType.DeepObdWifi)
                {
                    ((EdInterfaceObd)ediabas.EdInterfaceClass).ComPort = "DEEPOBDWIFI";
                    connectParameter = new EdCustomWiFiInterface.ConnectParameterType(_maConnectivity);
                }
                else
                {
                    ((EdInterfaceObd)ediabas.EdInterfaceClass).ComPort = "BLUETOOTH:" + btDeviceAddress;
                    connectParameter = new EdBluetoothInterface.ConnectParameterType(_maConnectivity, MtcBtService, () => _context);
                    ConnectMtcBtDevice(btDeviceAddress);
                }
            }
            else if (ediabas.EdInterfaceClass is EdInterfaceEnet)
            {
                string remoteHost = string.IsNullOrEmpty(_selectedEnetIp) ? "auto:all" : _selectedEnetIp;
                if (Emulator)
                {   // broadcast is not working with emulator
                    remoteHost = EmulatorEnetIp;
                }
                ((EdInterfaceEnet)ediabas.EdInterfaceClass).RemoteHost = remoteHost;
                connectParameter = new EdInterfaceEnet.ConnectParameterType(_maConnectivity);
            }
            ediabas.EdInterfaceClass.ConnectParameter = connectParameter;
        }

        public static string FormatResult(JobReader.PageInfo pageInfo, JobReader.DisplayInfo displayInfo, MultiMap<string, EdiabasNet.ResultData> resultDict, out Android.Graphics.Color? textColor)
        {
            textColor = null;
            if (pageInfo == null)
            {
                return string.Empty;
            }

            MethodInfo formatResult = null;
            MethodInfo formatResultColor = null;
            MethodInfo formatResultMulti = null;
            if (pageInfo.ClassObject != null)
            {
                Type pageType = pageInfo.ClassObject.GetType();
                formatResult = pageType.GetMethod("FormatResult", new[] { typeof(JobReader.PageInfo), typeof(Dictionary<string, EdiabasNet.ResultData>), typeof(string) });
                formatResultColor = pageType.GetMethod("FormatResult", new[] { typeof(JobReader.PageInfo), typeof(Dictionary<string, EdiabasNet.ResultData>), typeof(string), typeof(Android.Graphics.Color?).MakeByRefType() });
                formatResultMulti = pageType.GetMethod("FormatResult", new[] { typeof(JobReader.PageInfo), typeof(MultiMap<string, EdiabasNet.ResultData>), typeof(string), typeof(Android.Graphics.Color?).MakeByRefType() });
            }
            string result = string.Empty;
            if (displayInfo.Format == null)
            {
                if (resultDict != null)
                {
                    try
                    {
                        if (formatResultMulti != null)
                        {
                            object[] args = { pageInfo, resultDict, displayInfo.Result, null };
                            result = formatResultMulti.Invoke(pageInfo.ClassObject, args) as string;
                            textColor = args[3] as Android.Graphics.Color?;
                            //result = pageInfo.ClassObject.FormatResult(pageInfo, resultDict, displayInfo.Result, ref textColor);
                        }
                        else if (formatResultColor != null)
                        {
                            object[] args = { pageInfo, resultDict.ToDictionary(), displayInfo.Result, null };
                            result = formatResultColor.Invoke(pageInfo.ClassObject, args) as string;
                            textColor = args[3] as Android.Graphics.Color?;
                            //result = pageInfo.ClassObject.FormatResult(pageInfo, resultDict.ToDictionary(), displayInfo.Result, ref textColor);
                        }
                        else if (formatResult != null)
                        {
                            object[] args = { pageInfo, resultDict.ToDictionary(), displayInfo.Result };
                            result = formatResult.Invoke(pageInfo.ClassObject, args) as string;
                            //result = pageInfo.ClassObject.FormatResult(pageInfo, resultDict.ToDictionary(), displayInfo.Result);
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
            else
            {
                result = FormatResultEdiabas(resultDict, displayInfo.Result, displayInfo.Format);
            }
            return result;
        }

        public static String FormatResultEdiabas(MultiMap<string, EdiabasNet.ResultData> resultDict, string dataName, string format)
        {
            StringBuilder sbResult = new StringBuilder();
            IList<EdiabasNet.ResultData> resultDataList;
            if (resultDict != null && resultDict.TryGetValue(dataName.ToUpperInvariant(), out resultDataList))
            {
                foreach (EdiabasNet.ResultData resultData in resultDataList)
                {
                    string result;
                    if (resultData.OpData.GetType() == typeof(byte[]))
                    {
                        StringBuilder sb = new StringBuilder();
                        byte[] data = (byte[])resultData.OpData;
                        foreach (byte value in data)
                        {
                            sb.Append(string.Format(CultureInfo.InvariantCulture, "{0:X02} ", value));
                        }
                        result = sb.ToString();
                    }
                    else
                    {
                        result = EdiabasNet.FormatResult(resultData, format) ?? "?";
                    }
                    if (sbResult.Length > 0)
                    {
                        sbResult.Append("\r\n");
                    }
                    sbResult.Append(result);
                }
            }
            return sbResult.ToString();
        }

        public static bool IsTraceFilePresent(string traceDir)
        {
            if (string.IsNullOrEmpty(traceDir))
            {
                return false;
            }
            string traceFile = Path.Combine(traceDir, TraceFileName);
            return File.Exists(traceFile);
        }

        public bool RequestSendTraceFile(string appDataDir, string traceDir, PackageInfo packageInfo, Type classType, EventHandler<EventArgs> handler = null)
        {
            try
            {
                string traceFile = Path.Combine(traceDir, TraceFileName);
                if (!File.Exists(traceFile))
                {
                    return false;
                }
                FileInfo fileInfo = new FileInfo(traceFile);
                string message = string.Format(new FileSizeFormatProvider(), _context.GetString(Resource.String.send_trace_file_request), fileInfo.Length);
                new AlertDialog.Builder(_context)
                    .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                    {
                        SendTraceFile(appDataDir, traceFile, null, packageInfo, classType, handler, true);
                    })
                    .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                    {
                        handler?.Invoke(this, new EventArgs());
                    })
                    .SetCancelable(true)
                    .SetMessage(message)
                    .SetTitle(Resource.String.alert_title_question)
                    .Show();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool SendTraceFile(string appDataDir, string traceDir, PackageInfo packageInfo, Type classType, EventHandler<EventArgs> handler = null)
        {
            try
            {
                string traceFile = Path.Combine(traceDir, TraceFileName);
                if (!File.Exists(traceFile))
                {
                    return false;
                }
                SendTraceFile(appDataDir, traceFile, null, packageInfo, classType, handler, true);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool RequestSendMessage(string appDataDir, string message, PackageInfo packageInfo, Type classType, EventHandler<EventArgs> handler = null)
        {
            try
            {
                if (string.IsNullOrEmpty(message))
                {
                    return false;
                }
                new AlertDialog.Builder(_context)
                    .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                    {
                        SendTraceFile(appDataDir, null, message, packageInfo, classType, handler);
                    })
                    .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                    {
                        handler?.Invoke(this, new EventArgs());
                    })
                    .SetCancelable(true)
                    .SetMessage(Resource.String.send_message_request)
                    .SetTitle(Resource.String.alert_title_question)
                    .Show();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool SendTraceFile(string appDataDir, string traceFile, string message, PackageInfo packageInfo, Type classType, EventHandler<EventArgs> handler, bool deleteFile = false)
        {
            if ((string.IsNullOrEmpty(traceFile) || !File.Exists(traceFile)) &&
                string.IsNullOrEmpty(message))
            {
                return false;
            }
            CustomProgressDialog progress = new CustomProgressDialog(_context);
            progress.SetMessage(_context.GetString(Resource.String.send_trace_file));
            progress.ButtonAbort.Enabled = false;
            progress.Show();
            SetLock(LockTypeCommunication);
            SetPreferredNetworkInterface();

            Thread sendThread = new Thread(() =>
            {
                try
                {
                    bool cancelled = false;
                    WebClient webClient = new WebClient();
#pragma warning disable 618
                    SmtpClient smtpClient = new SmtpClient
#pragma warning restore 618
                    {
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                    };
                    MailMessage mail = new MailMessage()
                    {
                        Subject = "Deep OBD trace info",
                        BodyEncoding = Encoding.UTF8
                    };

                    if (!string.IsNullOrEmpty(traceFile) && File.Exists(traceFile))
                    {
                        mail.Attachments.Add(new Attachment(traceFile));
                    }

                    if (SelectedInterface == InterfaceType.Bluetooth)
                    {
                        if (GetConfigHciSnoopLog(out bool enabledConfig) && ReadHciSnoopLogSettings(out bool enabledSettings, out string logFileName))
                        {
                            if (enabledConfig && enabledSettings && !string.IsNullOrEmpty(logFileName) && File.Exists(logFileName))
                            {
                                mail.Attachments.Add(new Attachment(logFileName));
                            }
                        }
                    }

                    string downloadDir = Path.Combine(appDataDir, DownloadDir);
                    string mailInfoFile = Path.Combine(downloadDir, "Mail.xml");
                    Directory.CreateDirectory(downloadDir);
                    webClient.DownloadFile(MailInfoDownloadUrl, mailInfoFile);

                    string regEx;
                    int maxWords;
                    if (!GetKeyWordsInfo(mailInfoFile, out regEx, out maxWords))
                    {
                        throw new Exception("Invalid mail info");
                    }

                    StringBuilder sb = new StringBuilder();
                    sb.Append("Deep OBD Trace info");
                    sb.Append(string.Format("\nDate: {0:u}", DateTime.Now));
                    sb.Append(string.Format("\nLanguage: {0}", GetCurrentLanguage()));
                    sb.Append(string.Format("\nAndroid version: {0}", Build.VERSION.Sdk ?? string.Empty));
                    sb.Append(string.Format("\nAndroid id: {0}", Build.Id ?? string.Empty));
                    sb.Append(string.Format("\nAndroid display: {0}", Build.Display ?? string.Empty));
                    sb.Append(string.Format("\nAndroid fingerprint: {0}", Build.Fingerprint ?? string.Empty));
                    sb.Append(string.Format("\nAndroid type: {0}", Build.Type ?? string.Empty));
                    sb.Append(string.Format("\nAndroid tags: {0}", Build.Tags ?? string.Empty));
                    sb.Append(string.Format("\nAndroid manufacturer: {0}", Build.Manufacturer ?? string.Empty));
                    sb.Append(string.Format("\nAndroid model: {0}", Build.Model ?? string.Empty));
                    sb.Append(string.Format("\nAndroid product: {0}", Build.Product ?? string.Empty));
                    sb.Append(string.Format("\nAndroid device: {0}", Build.Device ?? string.Empty));
                    sb.Append(string.Format("\nAndroid board: {0}", Build.Board ?? string.Empty));
                    sb.Append(string.Format("\nAndroid brand: {0}", Build.Brand ?? string.Empty));
                    sb.Append(string.Format("\nAndroid hardware: {0}", Build.Hardware ?? string.Empty));
                    sb.Append(string.Format("\nAndroid user: {0}", Build.User ?? string.Empty));
                    sb.Append(string.Format("\nApp version name: {0}", packageInfo.VersionName));
                    sb.Append(string.Format("\nApp version code: {0}", packageInfo.VersionCode));
                    sb.Append(string.Format("\nApp id: {0}", AppId));
                    sb.Append(string.Format("\nEnable translation: {0}", EnableTranslation));
                    sb.Append(string.Format("\nManufacturer: {0}", ManufacturerName()));
                    sb.Append(string.Format("\nClass name: {0}", classType.FullName));

                    if (MtcBtService)
                    {
                        if (MtcBtServiceBound)
                        {
                            sb.Append(string.Format("\nMTC API Version: {0}", _mtcServiceConnection.ApiVersion));
                            try
                            {
                                IList<string> matchList = _mtcServiceConnection.GetMatchList();
                                foreach (string device in matchList)
                                {
                                    sb.Append(string.Format("\nMTC match device: {0}", device));
                                }
                                IList<string> deviceList = _mtcServiceConnection.GetDeviceList();
                                foreach (string device in deviceList)
                                {
                                    sb.Append(string.Format("\nMTC found device: {0}", device));
                                }
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                        else
                        {
                            sb.Append("\nMTC service not bound");
                        }
                    }

                    if (!string.IsNullOrEmpty(message))
                    {
                        sb.Append("\nInformation:\n");
                        sb.Append(message);
                    }

                    if (!string.IsNullOrEmpty(traceFile))
                    {
                        Dictionary<string, int> wordDict = ExtractKeyWords(traceFile, new Regex(regEx), maxWords, null);
                        if (wordDict != null)
                        {
                            sb.Append("\nKeywords:");
                            foreach (var entry in wordDict)
                            {
                                sb.Append(" ");
                                sb.Append(entry.Key);
                                sb.Append("=");
                                sb.Append(entry.Value);
                            }
                        }
                    }
                    mail.Body = sb.ToString();

                    string mailHost;
                    int mailPort;
                    bool mailSsl;
                    string mailFrom;
                    string mailTo;
                    string mailUser;
                    string mailPassword;
                    if (!GetMailInfo(mailInfoFile, out mailHost, out mailPort, out mailSsl, out mailFrom, out mailTo,
                        out mailUser, out mailPassword))
                    {
                        throw new Exception("Invalid mail info");
                    }
                    try
                    {
                        File.Delete(mailInfoFile);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                    smtpClient.Host = mailHost;
                    smtpClient.Port = mailPort;
                    smtpClient.EnableSsl = mailSsl;
                    smtpClient.UseDefaultCredentials = false;
                    if (string.IsNullOrEmpty(mailUser) || string.IsNullOrEmpty(mailPassword))
                    {
                        smtpClient.Credentials = null;
                    }
                    else
                    {
                        smtpClient.Credentials = new NetworkCredential(mailUser, mailPassword);
                    }
                    mail.From = new MailAddress(mailFrom);
                    mail.To.Clear();
                    mail.To.Add(new MailAddress(mailTo));
                    smtpClient.SendCompleted += (s, e) =>
                    {
                        _activity?.RunOnUiThread(() =>
                        {
                            if (_disposed)
                            {
                                return;
                            }
                            if (progress != null)
                            {
                                progress.Dismiss();
                                progress.Dispose();
                                progress = null;
                                SetLock(LockType.None);
                            }
                            if (e.Cancelled || cancelled)
                            {
                                return;
                            }
                            if (e.Error != null)
                            {
                                new AlertDialog.Builder(_context)
                                    .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                                    {
                                        SendTraceFile(appDataDir, traceFile, message, packageInfo, classType, handler, deleteFile);
                                    })
                                    .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                                    {
                                    })
                                    .SetCancelable(true)
                                    .SetMessage(Resource.String.send_trace_file_failed_retry)
                                    .SetTitle(Resource.String.alert_title_error)
                                    .Show();
                                return;
                            }
                            if (deleteFile)
                            {
                                try
                                {
                                    if (!string.IsNullOrEmpty(traceFile))
                                    {
                                        File.Delete(traceFile);
                                    }
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }
                            }
                            handler?.Invoke(this, new EventArgs());
                        });
                    };
                    _activity?.RunOnUiThread(() =>
                    {
                        if (_disposed)
                        {
                            return;
                        }
                        progress.AbortClick = sender =>
                        {
                            cancelled = true;   // cancel flag in event seems to be missing
                            try
                            {
                                smtpClient.SendAsyncCancel();
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        };
                        progress.ButtonAbort.Enabled = true;
                    });
                    smtpClient.SendAsync(mail, null);
                }
                catch (Exception)
                {
                    _activity?.RunOnUiThread(() =>
                    {
                        if (_disposed)
                        {
                            return;
                        }
                        if (progress != null)
                        {
                            progress.Dismiss();
                            progress.Dispose();
                            progress = null;
                            SetLock(LockType.None);
                        }
                        new AlertDialog.Builder(_context)
                            .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                            {
                                SendTraceFile(appDataDir, traceFile, message, packageInfo, classType, handler, deleteFile);
                            })
                            .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                            {
                            })
                            .SetCancelable(true)
                            .SetMessage(Resource.String.send_trace_file_failed_retry)
                            .SetTitle(Resource.String.alert_title_error)
                            .Show();
                    });
                }
            });
            sendThread.Start();
            return true;
        }

        private bool GetMailInfo(string xmlFile, out string host, out int port, out bool ssl, out string from, out string to, out string name, out string password)
        {
            host = null;
            port = 0;
            ssl = false;
            from = null;
            to = null;
            name = null;
            password = null;
            try
            {
                if (!File.Exists(xmlFile))
                {
                    return false;
                }
                XDocument xmlDoc = XDocument.Load(xmlFile);
                XElement emailNode = xmlDoc.Root?.Element("email");
                XAttribute hostAttr = emailNode?.Attribute("host");
                if (hostAttr == null)
                {
                    return false;
                }
                host = hostAttr.Value;
                XAttribute portAttr = emailNode.Attribute("port");
                if (portAttr == null)
                {
                    return false;
                }
                port = XmlConvert.ToInt32(portAttr.Value);
                XAttribute sslAttr = emailNode.Attribute("ssl");
                if (sslAttr == null)
                {
                    return false;
                }
                ssl = XmlConvert.ToBoolean(sslAttr.Value);
                XAttribute fromAttr = emailNode.Attribute("from");
                if (fromAttr == null)
                {
                    return false;
                }
                from = fromAttr.Value;
                XAttribute toAttr = emailNode.Attribute("to");
                if (toAttr == null)
                {
                    return false;
                }
                to = toAttr.Value;
                XAttribute usernameAttr = emailNode.Attribute("username");
                if (usernameAttr != null)
                {
                    name = usernameAttr.Value;
                }
                XAttribute passwordAttr = emailNode.Attribute("password");
                if (passwordAttr != null)
                {
                    password = passwordAttr.Value;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private bool GetKeyWordsInfo(string xmlFile, out string regEx, out int maxWords)
        {
            regEx = null;
            maxWords = 0;
            try
            {
                if (!File.Exists(xmlFile))
                {
                    return false;
                }
                XDocument xmlDoc = XDocument.Load(xmlFile);
                XElement keyWordsNode = xmlDoc.Root?.Element("keywords");
                XAttribute regexAttr = keyWordsNode?.Attribute("regex");
                if (regexAttr == null)
                {
                    return false;
                }
                regEx = regexAttr.Value;
                XAttribute maxWordsAttr = keyWordsNode.Attribute("max_words");
                if (maxWordsAttr != null)
                {
                    maxWords = XmlConvert.ToInt32(maxWordsAttr.Value);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static string GetCurrentLanguage()
        {
            return Java.Util.Locale.Default.Language ?? "en";
        }

        public static string GetVagDatUkdDir(string ecuPath, bool ignoreManufacturer = false)
        {
            string lang = GetCurrentLanguage();
            string dirName = Path.Combine(ecuPath, "dat.ukd", lang);
            if (!Directory.Exists(dirName))
            {
                dirName = Path.Combine(ecuPath, "dat.ukd", "en");
            }

            if (!ignoreManufacturer)
            {
                string manufactName = string.Empty;
                switch (SelectedManufacturer)
                {
                    case ManufacturerType.Audi:
                        manufactName = "audi";
                        break;

                    case ManufacturerType.Seat:
                        manufactName = "seat";
                        break;

                    case ManufacturerType.Skoda:
                        manufactName = "skoda";
                        break;

                    case ManufacturerType.Vw:
                        manufactName = "vw";
                        break;
                }
                if (!string.IsNullOrEmpty(manufactName))
                {
                    dirName = Path.Combine(dirName, manufactName);
                }
            }
            return dirName;
        }

        public static string SaeCode16ToString(long code)
        {
            string locationName;
            switch ((code >> 14) & 0x03)
            {
                default:    // power train
                    locationName = "P";
                    break;

                case 0x1:  // chassis
                    locationName = "C";
                    break;

                case 0x2:  // body
                    locationName = "B";
                    break;

                case 0x3:  // network
                    locationName = "U";
                    break;
            }
            return string.Format("{0}{1:X04}", locationName, code & 0x3FFF);
        }

        public List<string> ConvertVagDtcCode(string ecuPath, long code, List<long> typeList, bool kwp1281, bool saeMode)
        {
            try
            {
                if (_vagDtcCodeDict == null)
                {
                    _vagDtcCodeDict = new Dictionary<string, List<string>>();
                }
                StringBuilder srKey = new StringBuilder();
                srKey.Append("L=");
                srKey.Append(GetCurrentLanguage());
                srKey.Append(";P=");
                srKey.Append(ecuPath);
                srKey.Append(string.Format(";C={0}", code));
                foreach (long errorType in typeList)
                {
                    srKey.Append(string.Format(";T={0}", errorType));
                }
                srKey.Append(string.Format(";K={0}", kwp1281));
                srKey.Append(string.Format(";S={0}", saeMode));

                string dictKey = srKey.ToString();
                List<string> textListDict;
                if (_vagDtcCodeDict.TryGetValue(dictKey, out textListDict))
                {
                    return textListDict;
                }

                string datUkdPath = GetVagDatUkdDir(ecuPath);
                string xmlFile = Path.Combine(datUkdPath, "xml", "Zustand_KonzernASAM.xml");
                if (XmlDocDtcCodes == null)
                {
                    XmlDocDtcCodes = XDocument.Load(xmlFile);
                    if (XmlDocDtcCodes.Root == null)
                    {
                        XmlDocDtcCodes = null;
                        return null;
                    }
                }
                string tableNameDtc = "DTC-table";
                if (saeMode)
                {
                    string codeName = SaeCode16ToString(code >> 8);
                    long detailCode = code & 0xFF;
                    string detailName = string.Format("{0:X02}", detailCode);
                    List<string> textList = ReadVagDtcEntry(XmlDocDtcCodes, tableNameDtc, codeName + detailName);
                    if (textList.Count == 0 && detailCode != 0x00)
                    {
                        textList = ReadVagDtcEntry(XmlDocDtcCodes, tableNameDtc, codeName + "00");
                        if (textList.Count > 0)
                        {
                            // get detail text from other entry
                            List<string> textListDetail =  ReadVagDtcEntry(XmlDocDtcCodes, tableNameDtc, "[PCBU][0-9A-F]{4}" + detailName);
                            if (textListDetail.Count >= 2)
                            {
                                textList.Add(textListDetail[1]);
                            }
                        }
                    }
                    if (typeList.Count == 8)
                    {
                        if (typeList[3] != 0)
                        {
                            textList.Add(_context.GetString(Resource.String.error_code_error_present));
                        }
                        else
                        {
                            if (typeList[2] != 0)
                            {
                                textList.Add(_context.GetString(Resource.String.error_code_error_temporary));
                            }
                        }
                        if (typeList[5] != 0)
                        {
                            textList.Add(_context.GetString(Resource.String.error_code_error_stored));
                        }
                    }

                    _vagDtcCodeDict.Add(dictKey, textList);
                    return textList;
                }
                else
                {
                    string codeName = string.Format("VAG{0:00000}", code);
                    List<string> textList = ReadVagDtcEntry(XmlDocDtcCodes, tableNameDtc, codeName);
                    if (textList.Count == 0)
                    {
                        if (!VagDtcSaeDict.TryGetValue(code, out codeName))
                        {
                            return null;
                        }
                        codeName += "00";
                        textList = ReadVagDtcEntry(XmlDocDtcCodes, tableNameDtc, codeName);
                    }
                    else
                    {
                        if (typeList.Count > 0)
                        {
                            string tableNameType = kwp1281 ? "DTC fault symptoms KWP 1281" : "DTC fault symptoms KWP 2000";
                            string typeName = string.Format(kwp1281 ? "FSE{0:X05}" : "FST{0:X05}", typeList[0]);
                            textList.AddRange(ReadVagDtcEntry(XmlDocDtcCodes, tableNameType, typeName));
                        }
                    }

                    _vagDtcCodeDict.Add(dictKey, textList);
                    return textList;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private List<string> ReadVagDtcEntry(XDocument xmlDoc, string tableName, string entryRegex)
        {
            List<string> textList = new List<string>();
            try
            {
                if (xmlDoc.Root == null)
                {
                    return textList;
                }
                Regex regex = new Regex(entryRegex, RegexOptions.IgnoreCase);
                foreach (XElement tableNode in xmlDoc.Root.Elements("textTable"))
                {
                    XAttribute attrName = tableNode.Attribute("name");
                    if (attrName == null)
                    {
                        continue;
                    }
                    if (string.Compare(attrName.Value, tableName, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        continue;
                    }
                    // ReSharper disable once LoopCanBeConvertedToQuery
                    foreach (XElement textNode in tableNode.Elements("text"))
                    {
                        XAttribute attrId = textNode.Attribute("id");
                        if (attrId == null)
                        {
                            continue;
                        }
                        if (!regex.IsMatch(attrId.Value))
                        {
                            continue;
                        }
                        foreach (XElement lineNode in textNode.Elements("line"))
                        {
                            using (XmlReader reader = lineNode.CreateReader())
                            {
                                reader.MoveToContent();
                                string line = reader.ReadInnerXml();
                                if (!string.IsNullOrWhiteSpace(line))
                                {
                                    textList.Add(line);
                                }
                            }
                        }
                        return textList;
                    }
                }
            }
            catch (Exception)
            {
                return textList;
            }
            return textList;
        }

        public static List<VagEcuEntry> ReadVagEcuList(string ecuPath)
        {
            try
            {
                string datUkdPath = GetVagDatUkdDir(ecuPath);
                string xmlFile = Path.Combine(datUkdPath, "xml", "GWVL-ECU_config.xml");
                XDocument xmlDoc = XDocument.Load(xmlFile);
                XElement ecuListNode = xmlDoc.Root?.Element("EcuList");
                if (ecuListNode == null)
                {
                    return null;
                }
                List<VagEcuEntry> ecuList = new List<VagEcuEntry>();
                foreach (XElement ecuNode in ecuListNode.Elements("Ecu"))
                {
                    XElement sysNode = ecuNode.Element("SysName");
                    if (sysNode == null)
                    {
                        continue;
                    }
                    if (sysNode.Attribute("Type") != null)
                    {
                        continue;
                    }
                    string sysName;
                    using (XmlReader reader = sysNode.CreateReader())
                    {
                        reader.MoveToContent();
                        sysName = reader.ReadInnerXml();
                    }
                    if (string.IsNullOrWhiteSpace(sysName))
                    {
                        continue;
                    }

                    XAttribute attrAddr = ecuNode.Attribute("AddrDez");
                    if (attrAddr == null)
                    {
                        continue;
                    }
                    int address;
                    try
                    {
                        address = XmlConvert.ToInt32(attrAddr.Value);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    ecuList.Add(new VagEcuEntry(sysName, address));
                }
                return ecuList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Convert russian mwtab xml files first with (linux):
        // mkdir ../mwru
        // find -name "*.xml" -exec sh -c "recode UTF8..ISO-8859-1 < '{}' | recode windows-1251..UTF8 > ../mwru/'{}'" \;
        public static List<MwTabEntry> ReadVagMwTab(string fileName, bool ignoreEmptyUnits = false)
        {
            try
            {
                Regex commentRegex = new Regex(@"(^I|\\(N[YZ ]|.))", RegexOptions.Multiline);
                XDocument xmlDoc = XDocument.Load(fileName);
                if (xmlDoc.Root == null)
                {
                    return null;
                }
                List<MwTabEntry> mwTabList = new List<MwTabEntry>();
                foreach (XElement measureNode in xmlDoc.Root.Elements("Messwert"))
                {
                    try
                    {
                        int blockNumber;
                        XElement agNode = measureNode.Element("AG");
                        if (agNode == null)
                        {
                            continue;
                        }
                        using (XmlReader reader = agNode.CreateReader())
                        {
                            reader.MoveToContent();
                            blockNumber = XmlConvert.ToInt32(reader.ReadInnerXml());
                        }

                        int? valueIndex = null;
                        XElement afNode = measureNode.Element("AF");
                        if (afNode != null)
                        {
                            using (XmlReader reader = afNode.CreateReader())
                            {
                                reader.MoveToContent();
                                try
                                {
                                    valueIndex = XmlConvert.ToInt32(reader.ReadInnerXml());
                                }
                                catch (Exception)
                                {
                                    valueIndex = null;
                                }
                            }
                        }

                        string description = string.Empty;
                        XElement nameNode = measureNode.Element("Name");
                        if (nameNode == null)
                        {
                            continue;
                        }
                        foreach (XElement textNode in nameNode.Elements("Text"))
                        {
                            XAttribute tiAttrib = textNode.Attribute("TI");
                            if (tiAttrib?.Value.Length > 0 && valueIndex.HasValue)
                            {
                                continue;
                            }
                            using (XmlReader reader = textNode.CreateReader())
                            {
                                reader.MoveToContent();
                                description = reader.ReadInnerXml();
                            }
                        }
                        if (string.IsNullOrEmpty(description))
                        {
                            continue;
                        }

                        XElement commentNode = measureNode.Element("Meldungstext");
                        string comment = string.Empty;
                        if (commentNode != null)
                        {
                            using (XmlReader reader = commentNode.CreateReader())
                            {
                                reader.MoveToContent();
                                comment = reader.ReadInnerXml();
                            }
                        }
                        comment = commentRegex.Replace(comment, string.Empty);

                        double? valueMin = null;
                        XElement minNode = measureNode.Element("analogerSWunten");
                        if (minNode != null)
                        {
                            using (XmlReader reader = minNode.CreateReader())
                            {
                                reader.MoveToContent();
                                string value = reader.ReadInnerXml();
                                if (!string.IsNullOrEmpty(value))
                                {
                                    try
                                    {
                                        valueMin = XmlConvert.ToDouble(value);
                                    }
                                    catch (Exception)
                                    {
                                        valueMin = null;
                                    }
                                }
                            }
                        }

                        double? valueMax = null;
                        XElement maxNode = measureNode.Element("analogerSWoben");
                        if (maxNode != null)
                        {
                            using (XmlReader reader = maxNode.CreateReader())
                            {
                                reader.MoveToContent();
                                string value = reader.ReadInnerXml();
                                if (!string.IsNullOrEmpty(value))
                                {
                                    try
                                    {
                                        valueMax = XmlConvert.ToDouble(value);
                                    }
                                    catch (Exception)
                                    {
                                        valueMax = null;
                                    }
                                }
                            }
                        }

                        string valueUnit = string.Empty;
                        XElement sweNode = measureNode.Element("SWE");
                        if (sweNode != null)
                        {
                            using (XmlReader reader = sweNode.CreateReader())
                            {
                                reader.MoveToContent();
                                valueUnit = reader.ReadInnerXml();
                            }
                        }
                        if (ignoreEmptyUnits && string.IsNullOrWhiteSpace(valueUnit))
                        {
                            continue;
                        }
                        if (valueUnit.ToLowerInvariant().StartsWith("proz"))
                        {
                            valueUnit = "%";
                        }

                        string valueType = string.Empty;
                        XElement swfNode = measureNode.Element("SWF");
                        if (swfNode != null)
                        {
                            using (XmlReader reader = swfNode.CreateReader())
                            {
                                reader.MoveToContent();
                                valueType = reader.ReadInnerXml();
                            }
                        }

                        mwTabList.Add(new MwTabEntry(blockNumber, valueIndex, description, comment, valueUnit, valueType, valueMin, valueMax));
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
                return mwTabList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static List<string> GetMatchingVagMwTabUdsFiles(string mwTabDir, long ecuAddr)
        {
            try
            {
                string fileMask = string.Format("UDS_{0:X02}_*.xml", ecuAddr);
                string[] files = Directory.GetFiles(mwTabDir, fileMask, SearchOption.TopDirectoryOnly);
                return files.ToList();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static List<string> GetMatchingVagMwTabFiles(string mwTabDir, string sgName)
        {
            try
            {
                List<string> fileList = new List<string>();
                string xmlEntry = string.Format("<SgVariante>{0}</SgVariante>", sgName);
                string[] files = Directory.GetFiles(mwTabDir, "*.xml", SearchOption.TopDirectoryOnly);

                foreach (string file in files)
                {
                    try
                    {
                        using (StreamReader streamReader = new StreamReader(file))
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                string line = streamReader.ReadLine();
                                if (line == null)
                                {
                                    break;
                                }
                                line = line.Trim();
                                if (string.Compare(line, xmlEntry, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    fileList.Add(file);
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
                return fileList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static List<MwTabFileEntry> GetMatchingVagMwTabsUds(string mwTabDir, long ecuAddr)
        {
            List<MwTabFileEntry> mwTabFileList = new List<MwTabFileEntry>();
            List<string> fileList = GetMatchingVagMwTabUdsFiles(mwTabDir, ecuAddr);
            if (fileList == null)
            {
                return mwTabFileList;
            }

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (string fileName in fileList)
            {
                List<MwTabEntry> mwTabList = ReadVagMwTab(fileName);
                if (mwTabList == null)
                {
                    continue;
                }
                List<MwTabEntry> mwTabListCleaned = mwTabList.Where(entry => !entry.ValueIndex.HasValue).ToList();
                mwTabFileList.Add(new MwTabFileEntry(fileName, mwTabListCleaned));
            }
            return mwTabFileList;
        }

        public static List<MwTabFileEntry> GetMatchingVagMwTabs(string mwTabDir, string sgName)
        {
            List<MwTabFileEntry> mwTabFileList = new List<MwTabFileEntry>();
            List<string> fileList = GetMatchingVagMwTabFiles(mwTabDir, sgName);
            if (fileList == null)
            {
                return mwTabFileList;
            }

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (string fileName in fileList)
            {
                List<MwTabEntry> mwTabList = ReadVagMwTab(fileName, true);
                if (mwTabList == null)
                {
                    continue;
                }
                List<MwTabEntry> mwTabListCleaned = mwTabList.Where(entry => entry.ValueIndex.HasValue).ToList();
                mwTabFileList.Add(new MwTabFileEntry(fileName, mwTabListCleaned));
            }
            return mwTabFileList;
        }

        public static SortedSet<int> ExtractVagMwBlocks(List<MwTabFileEntry> mwTabFileList)
        {
            SortedSet<int> mwBlocks = new SortedSet<int>();

            foreach (MwTabFileEntry mwTabFileEntry in mwTabFileList)
            {
                foreach (MwTabEntry mwTabEntry in mwTabFileEntry.MwTabList)
                {
                    if (!mwBlocks.Contains(mwTabEntry.BlockNumber))
                    {
                        mwBlocks.Add(mwTabEntry.BlockNumber);
                    }
                }
            }

            return mwBlocks;
        }

        public static Dictionary<int, string> GetVagEcuNamesDict(string ecuPath)
        {
            try
            {
                string datUkdBasePath = GetVagDatUkdDir(ecuPath, true);
                string xmlFile = Path.Combine(datUkdBasePath, "xml", "ECU_Names.xml");
                XDocument xmlDoc = XDocument.Load(xmlFile);
                XElement ecuListNode = xmlDoc.Root?.Element("EcuList");
                if (ecuListNode == null)
                {
                    return null;
                }
                Dictionary<int, string> ecuNamesDict = new Dictionary<int, string>();
                foreach (XElement ecuNode in ecuListNode.Elements("Ecu"))
                {
                    XAttribute attrName = ecuNode.Attribute("Name");
                    string ecuName = attrName?.Value;
                    if (string.IsNullOrWhiteSpace(ecuName))
                    {
                        continue;
                    }
                    XAttribute attrAddr = ecuNode.Attribute("AddrHex");
                    if (attrAddr == null)
                    {
                        continue;
                    }
                    int address;
                    try
                    {
                        address = Convert.ToInt32(attrAddr.Value, 16);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    string existingName;
                    if (ecuNamesDict.TryGetValue(address, out existingName))
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        if (existingName.Length > ecuName.Length)
                        {   // use shortest name
                            ecuNamesDict[address] = ecuName;
                        }
                    }
                    else
                    {
                        ecuNamesDict.Add(address, ecuName);
                    }
                }
                return ecuNamesDict;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool IsTranslationRequired()
        {
#if true
            if (SelectedManufacturer != ManufacturerType.Bmw)
            {
                return false;
            }
            string lang = GetCurrentLanguage();
            return string.Compare(lang, "de", StringComparison.OrdinalIgnoreCase) != 0;
#else
            return false;
#endif
        }

        public bool StoreTranslationCache(string fileName)
        {
            try
            {
                XElement xmlLangNodes = new XElement("LanguageCache");
                foreach (string language in _yandexTransDict.Keys)
                {
                    Dictionary<string, string> langDict = _yandexTransDict[language];
                    XElement xmlLang = new XElement("Language");
                    xmlLang.Add(new XAttribute("Name", language));
                    foreach (string key in langDict.Keys)
                    {
                        XElement xmlTransNode = new XElement("Trans");
                        xmlTransNode.Add(new XAttribute("Key", key));
                        xmlTransNode.Add(new XAttribute("Value", langDict[key]));
                        xmlLang.Add(xmlTransNode);
                    }
                    xmlLangNodes.Add(xmlLang);
                }
                using (MemoryStream ms = new MemoryStream())
                {
                    XDocument doc = new XDocument(xmlLangNodes);
                    doc.Save(ms);
                    ms.Position = 0;
                    return CreateZipFile(ms, Path.GetFileName(fileName), fileName +".zip");
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool ReadTranslationCache(string fileName)
        {
            try
            {
                _yandexTransDict.Clear();
                using (MemoryStream ms = new MemoryStream())
                {
                    if (!ExtractZipFile(fileName + ".zip", Path.GetFileName(fileName), ms))
                    {
                        return false;
                    }
                    ms.Position = 0;
                    XDocument xmlLangDoc = XDocument.Load(ms);
                    if (xmlLangDoc.Root == null)
                    {
                        return false;
                    }
                    foreach (XElement langNode in xmlLangDoc.Root.Elements("Language"))
                    {
                        XAttribute attrName = langNode.Attribute("Name");
                        if (attrName == null)
                        {
                            continue;
                        }
                        string language = attrName.Value;
                        if (!_yandexTransDict.ContainsKey(language))
                        {
                            Dictionary<string, string> langDict = new Dictionary<string, string>();
                            foreach (XElement transNode in langNode.Elements("Trans"))
                            {
                                XAttribute attrKey = transNode.Attribute("Key");
                                if (attrKey == null)
                                {
                                    continue;
                                }
                                XAttribute attrValue = transNode.Attribute("Value");
                                if (attrValue == null)
                                {
                                    continue;
                                }
                                if (!langDict.ContainsKey(attrKey.Value))
                                {
                                    langDict.Add(attrKey.Value, attrValue.Value);
                                }
                            }
                            _yandexTransDict.Add(language, langDict);
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public void ClearTranslationCache()
        {
            _yandexTransDict.Clear();
        }

        public bool IsTranslationCacheEmpty()
        {
            return _yandexTransDict.Count == 0;
        }

        public bool RequestEnableTranslate(EventHandler<EventArgs> handler = null)
        {
            try
            {
                if (!IsTranslationRequired() || EnableTranslation || EnableTranslateRequested)
                {
                    return false;
                }
                EnableTranslateRequested = true;
                AlertDialog alertDialog = new AlertDialog.Builder(_context)
                    .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                    {
                        EnableTranslation = true;
                    })
                    .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                    {
                    })
                    .SetCancelable(true)
                    .SetMessage(Resource.String.translate_enable_request)
                    .SetTitle(Resource.String.alert_title_question)
                    .Show();
                alertDialog.DismissEvent += (sender, args) =>
                {
                    if (_disposed)
                    {
                        return;
                    }
                    handler?.Invoke(sender, args);
                };
                TextView messageView = alertDialog.FindViewById<TextView>(Android.Resource.Id.Message);
                if (messageView != null)
                {
                    messageView.MovementMethod = new LinkMovementMethod();
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool TranslateStrings(List<string> stringList, TranslateDelegate handler, bool disableCache = false)
        {
            if ((stringList.Count == 0) || (handler == null))
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(YandexApiKey))
            {
                return false;
            }
            if (_translateProgress == null)
            {
                _yandexTransList = null;
                // try to translate with cache first
                _yandexCurrentLang = (GetCurrentLanguage()).ToLowerInvariant();

                if (!_yandexTransDict.TryGetValue(_yandexCurrentLang, out _yandexCurrentLangDict) || (_yandexCurrentLangDict == null))
                {
                    _yandexCurrentLangDict = new Dictionary<string, string>();
                    _yandexTransDict.Add(_yandexCurrentLang, _yandexCurrentLangDict);
                }
                _yandexReducedStringList = new List<string>();
                List<string> translationList = new List<string>();
                foreach (string text in stringList)
                {
                    string translation;
                    if (!disableCache && _yandexCurrentLangDict.TryGetValue(text, out translation))
                    {
                        translationList.Add(translation);
                    }
                    else
                    {
                        _yandexReducedStringList.Add(text);
                    }
                }
                if (_yandexReducedStringList.Count == 0)
                {
                    handler(translationList);
                    return true;
                }

                _translateProgress = new CustomProgressDialog(_context);
                _translateProgress.SetMessage(_context.GetString(Resource.String.translate_text));
                _translateProgress.AbortClick = sender =>
                {
                    if (_translateWebClient != null && _translateWebClient.IsBusy)
                    {
                        _translateWebClient.CancelAsync();
                    }
                };
                _translateProgress.Indeterminate = false;
                _translateProgress.Progress = 0;
                _translateProgress.Max = 100;
                _translateProgress.Show();
                _translateLockAquired = SetLock(LockTypeCommunication);
            }
            _translateProgress.ButtonAbort.Enabled = false;
            _translateProgress.Progress = (_yandexTransList?.Count ?? 0) * 100 / _yandexReducedStringList.Count;
            SetPreferredNetworkInterface();

            Thread translateThread = new Thread(() =>
            {
                try
                {
                    int stringCount = 0;
                    _translateWebClient = new WebClient();
                    StringBuilder sbUrl = new StringBuilder();
                    if (_yandexLangList == null)
                    {
                        // no language list present, get it first
                        sbUrl.Append(@"https://translate.yandex.net/api/v1.5/tr/getLangs?");
                        sbUrl.Append("key=");
                        sbUrl.Append(System.Uri.EscapeDataString(YandexApiKey));
                    }
                    else
                    {
                        string langPair = "de-" + _yandexCurrentLang;
                        string langPairTemp = langPair;     // prevent warning
                        if (_yandexLangList.All(lang => string.Compare(lang, langPairTemp, StringComparison.OrdinalIgnoreCase) != 0))
                        {
                            // language not found
                            langPair = "de-en";
                        }

                        sbUrl.Append(@"https://translate.yandex.net/api/v1.5/tr/translate?");
                        sbUrl.Append("key=");
                        sbUrl.Append(System.Uri.EscapeDataString(YandexApiKey));
                        sbUrl.Append("&lang=");
                        sbUrl.Append(langPair);
                        int offset = _yandexTransList?.Count ?? 0;
                        for (int i = offset; i < _yandexReducedStringList.Count; i++)
                        {
                            sbUrl.Append("&text=");
                            sbUrl.Append(System.Uri.EscapeDataString(_yandexReducedStringList[i]));
                            stringCount++;
                            if (sbUrl.Length > 8000)
                            {
                                break;
                            }
                        }
                    }
                    _translateWebClient.DownloadStringCompleted += (sender, args) =>
                    {
                        if (!args.Cancelled && (args.Error == null))
                        {
                            if (_yandexLangList == null)
                            {
                                _yandexLangList = GetLanguages(args.Result);
                                if (_yandexLangList != null)
                                {
                                    _activity?.RunOnUiThread(() =>
                                    {
                                        if (_disposed)
                                        {
                                            return;
                                        }
                                        TranslateStrings(stringList, handler, disableCache);
                                    });
                                    return;
                                }
                            }
                            else
                            {
                                List<string> transList = GetTranslations(args.Result);
                                if (transList != null && transList.Count == stringCount)
                                {
                                    if (_yandexTransList == null)
                                    {
                                        _yandexTransList = transList;
                                    }
                                    else
                                    {
                                        _yandexTransList.AddRange(transList);
                                    }
                                    if (_yandexTransList.Count < _yandexReducedStringList.Count)
                                    {
                                        _activity?.RunOnUiThread(() =>
                                        {
                                            if (_disposed)
                                            {
                                                return;
                                            }
                                            TranslateStrings(stringList, handler, disableCache);
                                        });
                                        return;
                                    }
                                }
                                else
                                {
                                    // error
                                    _yandexTransList = null;
                                }
                            }
                        }
                        else
                        {
                            // error
                            _yandexTransList = null;
                        }
                        _activity?.RunOnUiThread(() =>
                        {
                            if (_disposed)
                            {
                                return;
                            }
                            if (_translateProgress != null)
                            {
                                _translateProgress.Dismiss();
                                _translateProgress.Dispose();
                                _translateProgress = null;
                                if (_translateLockAquired)
                                {
                                    SetLock(LockType.None);
                                    _translateLockAquired = false;
                                }
                            }
                            if ((_yandexLangList == null) || (_yandexTransList == null))
                            {
                                string errorMessage = string.Empty;
                                if (!args.Cancelled && (args.Error != null))
                                {
                                    WebException webException = args.Error as WebException;
                                    Stream responseStream = webException?.Response?.GetResponseStream();
                                    if (responseStream != null)
                                    {
                                        string responseText;
                                        using (var reader = new StreamReader(responseStream))
                                        {
                                            responseText = reader.ReadToEnd();
                                        }
                                        // ReSharper disable once NotAccessedVariable
                                        int errorCode;
                                        errorMessage = GetTranslationError(responseText, out errorCode);
                                    }
                                }

                                string message = string.IsNullOrEmpty(errorMessage) ?
                                    _context.GetString(Resource.String.translate_failed) : string.Format(_context.GetString(Resource.String.translate_failed_message), errorMessage);
                                bool yesSelected = false;
                                AlertDialog altertDialog = new AlertDialog.Builder(_context)
                                    .SetPositiveButton(Resource.String.button_yes, (s, a) =>
                                    {
                                        yesSelected = true;
                                        TranslateStrings(stringList, handler, disableCache);
                                    })
                                    .SetNegativeButton(Resource.String.button_no, (s, a) =>
                                    {
                                    })
                                    .SetCancelable(true)
                                    .SetMessage(message)
                                    .SetTitle(Resource.String.alert_title_error)
                                    .Show();
                                altertDialog.DismissEvent += (o, eventArgs) =>
                                {
                                    if (_disposed)
                                    {
                                        return;
                                    }
                                    if (!yesSelected)
                                    {
                                        handler(null);
                                    }
                                };
                            }
                            else
                            {
                                if (disableCache)
                                {
                                    handler(_yandexReducedStringList.Count == _yandexTransList.Count
                                        ? _yandexTransList : null);
                                }
                                else
                                {
                                    // add translation to cache
                                    if (_yandexReducedStringList.Count == _yandexTransList.Count)
                                    {
                                        for (int i = 0; i < _yandexTransList.Count; i++)
                                        {
                                            string key = _yandexReducedStringList[i];
                                            if (!_yandexCurrentLangDict.ContainsKey(key))
                                            {
                                                _yandexCurrentLangDict.Add(key, _yandexTransList[i]);
                                            }
                                        }
                                    }
                                    // create full list
                                    List<string> transListFull = new List<string>();
                                    foreach (string text in stringList)
                                    {
                                        string translation;
                                        if (_yandexCurrentLangDict.TryGetValue(text, out translation))
                                        {
                                            transListFull.Add(translation);
                                        }
                                        else
                                        {
                                            // should not happen
                                            transListFull = null;
                                            break;
                                        }
                                    }
                                    handler(transListFull);
                                }
                            }
                        });
                    };
                    _activity?.RunOnUiThread(() =>
                    {
                        if (_disposed)
                        {
                            return;
                        }
                        if (_translateProgress != null)
                        {
                            _translateProgress.ButtonAbort.Enabled = true;
                        }
                    });
                    ServicePointManager.ServerCertificateValidationCallback =
                        (sender, certificate, chain, errors) => true;
                    _translateWebClient.DownloadStringAsync(new System.Uri(sbUrl.ToString()));
                }
                catch (Exception)
                {
                    _activity?.RunOnUiThread(() =>
                    {
                        if (_disposed)
                        {
                            return;
                        }
                        if (_translateProgress != null)
                        {
                            _translateProgress.Dismiss();
                            _translateProgress.Dispose();
                            _translateProgress = null;
                            if (_translateLockAquired)
                            {
                                SetLock(LockType.None);
                                _translateLockAquired = false;
                            }
                        }
                        bool yesSelected = false;
                        AlertDialog altertDialog = new AlertDialog.Builder(_context)
                            .SetPositiveButton(Resource.String.button_yes, (s, a) =>
                            {
                                yesSelected = true;
                                TranslateStrings(stringList, handler, disableCache);
                            })
                            .SetNegativeButton(Resource.String.button_no, (s, a) =>
                            {
                            })
                            .SetCancelable(true)
                            .SetMessage(Resource.String.translate_failed)
                            .SetTitle(Resource.String.alert_title_error)
                            .Show();
                        altertDialog.DismissEvent += (o, eventArgs) =>
                        {
                            if (_disposed)
                            {
                                return;
                            }
                            if (!yesSelected)
                            {
                                handler(null);
                            }
                        };
                    });
                }
            });
            translateThread.Start();
            return true;
        }

        private string GetTranslationError(string xmlResult, out int errorCode)
        {
            errorCode = -1;
            string message = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(xmlResult))
                {
                    return message;
                }
                XDocument xmlDoc = XDocument.Parse(xmlResult);
                if (xmlDoc.Root == null)
                {
                    return message;
                }
                XAttribute attrCode = xmlDoc.Root.Attribute("code");
                if (attrCode != null)
                {
                    errorCode = XmlConvert.ToInt32(attrCode.Value);
                }
                XAttribute attrMessage = xmlDoc.Root.Attribute("message");
                if (attrMessage != null)
                {
                    message = attrMessage.Value;
                }
                return message;
            }
            catch (Exception)
            {
                return message;
            }
        }

        private List<string> GetLanguages(string xmlResult)
        {
            try
            {
                if (string.IsNullOrEmpty(xmlResult))
                {
                    return null;
                }
                XDocument xmlDoc = XDocument.Parse(xmlResult);
                XElement dirsNode = xmlDoc.Root?.Element("dirs");
                if (dirsNode == null)
                {
                    return null;
                }
                List<string> transList = new List<string>();
                foreach (XElement textNode in dirsNode.Elements("string"))
                {
                    using (XmlReader reader = textNode.CreateReader())
                    {
                        reader.MoveToContent();
                        transList.Add(reader.ReadInnerXml());
                    }
                }
                return transList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private List<string> GetTranslations(string xmlResult)
        {
            try
            {
                if (string.IsNullOrEmpty(xmlResult))
                {
                    return null;
                }
                XDocument xmlDoc = XDocument.Parse(xmlResult);
                if (xmlDoc.Root == null)
                {
                    return null;
                }
                List<string> transList = new List<string>();
                foreach (XElement textNode in xmlDoc.Root.Elements("text"))
                {
                    using (XmlReader reader = textNode.CreateReader())
                    {
                        reader.MoveToContent();
                        transList.Add(reader.ReadInnerXml());
                    }
                }
                return transList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Java.Lang.ICharSequence FromHtml(string source)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
            {
                return Android.Text.Html.FromHtml(source, Android.Text.FromHtmlOptions.ModeLegacy);
            }
#pragma warning disable 618
            return Android.Text.Html.FromHtml(source);
#pragma warning restore 618
        }

        public static string MakeRelativePath(string fromPath, string toPath)
        {
            if (string.IsNullOrEmpty(fromPath))
            {
                return string.Empty;
            }
            if (string.IsNullOrEmpty(toPath))
            {
                return fromPath;
            }
            System.Uri fromUri = new System.Uri(AppendDirectorySeparatorChar(fromPath));
            System.Uri toUri = new System.Uri(AppendDirectorySeparatorChar(toPath));

            if (fromUri.Scheme != toUri.Scheme)
            {   // path can't be made relative.
                return toPath; 
            }

            System.Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            String relativePath = System.Uri.UnescapeDataString(relativeUri.ToString());

            if (string.Equals(toUri.Scheme, System.Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }

        public static string AppendDirectorySeparatorChar(string path)
        {
            // Append a slash only if the path is a directory and does not have a slash.
            if (!Path.HasExtension(path) &&
                !path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                return path + Path.DirectorySeparatorChar;
            }

            return path;
        }

        public static bool CheckZipFile(string archiveFilename)
        {
            ZipFile zf = null;
            try
            {
                try
                {
                    FileStream fs = File.OpenRead(archiveFilename);
                    zf = new ZipFile(fs);
                    return zf.TestArchive(false);
                }
                catch (Exception)
                {
                    return false;
                }
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

        public static void ExtractZipFile(string archiveFilenameIn, string outFolder, string key, ProgressZipDelegate progressHandler)
        {
            FileStream fs = null;
            ZipFile zf = null;
            string tempFile = Path.Combine(outFolder, "temp.zip");
            try
            {
                if (!string.IsNullOrEmpty(key))
                {
                    using (AesCryptoServiceProvider crypto = new AesCryptoServiceProvider())
                    {
                        crypto.Mode = CipherMode.CBC;
                        crypto.Padding = PaddingMode.PKCS7;
                        crypto.KeySize = 256;

                        using (SHA256Managed sha256 = new SHA256Managed())
                        {
                            crypto.Key = sha256.ComputeHash(Encoding.ASCII.GetBytes(key));
                        }
                        using (var md5 = MD5.Create())
                        {
                            crypto.IV = md5.ComputeHash(Encoding.ASCII.GetBytes(key));
                        }

                        using (FileStream fsRead = File.OpenRead(archiveFilenameIn))
                        {
                            using (CryptoStream crStream = new CryptoStream(fsRead, crypto.CreateDecryptor(), CryptoStreamMode.Read))
                            {
                                byte[] buffer = new byte[4096];     // 4K is optimum
                                using (FileStream fsWrite = File.Create(tempFile))
                                {
                                    bool aborted = false;
                                    StreamUtils.Copy(crStream, fsWrite, buffer, (sender, args) =>
                                    {
                                        if (progressHandler != null)
                                        {
                                            if (progressHandler((int)args.PercentComplete, true))
                                            {
                                                args.ContinueRunning = false;
                                                aborted = true;
                                            }
                                        }
                                    }, TimeSpan.FromSeconds(1), null, null, fsRead.Length);
                                    if (aborted)
                                    {
                                        return;
                                    }
                                }
                            }
                        }
                    }
                    fs = File.OpenRead(tempFile);
                }
                else
                {
                    fs = File.OpenRead(archiveFilenameIn);
                }

                zf = new ZipFile(fs);

                long index = 0;
                foreach (ZipEntry zipEntry in zf)
                {
                    if (progressHandler != null)
                    {
                        if (progressHandler((int)(100 * index / zf.Count)))
                        {
                            return;
                        }
                    }
                    if (!zipEntry.IsFile)
                    {
                        continue;           // Ignore directories
                    }
                    String entryFileName = zipEntry.Name;
                    // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                    // Optionally match entrynames against a selection list here to skip as desired.
                    // The unpacked length is available in the zipEntry.Size property.

                    byte[] buffer = new byte[4096];     // 4K is optimum
                    Stream zipStream = zf.GetInputStream(zipEntry);

                    // Manipulate the output filename here as desired.
                    String fullZipToPath = Path.Combine(outFolder, entryFileName);
                    string directoryName = Path.GetDirectoryName(fullZipToPath);
                    if (!string.IsNullOrEmpty(directoryName))
                    {
                        // ReSharper disable once AssignNullToNotNullAttribute
                        Directory.CreateDirectory(directoryName);
                    }

                    // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                    // of the file, but does not waste memory.
                    // The "using" will close the stream even if an exception occurs.
                    using (FileStream streamWriter = File.Create(fullZipToPath))
                    {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                    index++;
                }
            }
            finally
            {
                zf?.Close(); // Ensure we release resources
                fs?.Close();
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        public static bool CreateZipFile(string[] inputFiles, string archiveFilenameOut, ProgressZipDelegate progressHandler)
        {
            try
            {
                FileStream fsOut = File.Create(archiveFilenameOut);
                ZipOutputStream zipStream = new ZipOutputStream(fsOut);
                zipStream.SetLevel(3);

                try
                {
                    long index = 0;
                    foreach (string filename in inputFiles)
                    {
                        if (progressHandler != null)
                        {
                            if (progressHandler((int)(100 * index / inputFiles.Length)))
                            {
                                return false;
                            }
                        }

                        FileInfo fi = new FileInfo(filename);
                        string entryName = Path.GetFileName(filename);

                        ZipEntry newEntry = new ZipEntry(entryName)
                        {
                            DateTime = fi.LastWriteTime,
                            Size = fi.Length
                        };
                        zipStream.PutNextEntry(newEntry);

                        byte[] buffer = new byte[4096];
                        using (FileStream streamReader = File.OpenRead(filename))
                        {
                            StreamUtils.Copy(streamReader, zipStream, buffer);
                        }
                        zipStream.CloseEntry();
                        index++;
                    }
                }
                finally
                {
                    zipStream.IsStreamOwner = true;
                    zipStream.Close();
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static bool ExtractZipFile(string archiveFilenameIn, string archiveName, Stream outStream)
        {
            try
            {
                ZipFile zf = null;
                try
                {
                    FileStream fs = File.OpenRead(archiveFilenameIn);
                    zf = new ZipFile(fs);
                    foreach (ZipEntry zipEntry in zf)
                    {
                        if (!zipEntry.IsFile)
                        {
                            continue; // Ignore directories
                        }
                        if (string.Compare(zipEntry.Name, archiveName, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            byte[] buffer = new byte[4096]; // 4K is optimum
                            Stream zipStream = zf.GetInputStream(zipEntry);

                            StreamUtils.Copy(zipStream, outStream, buffer);
                            break;
                        }
                    }
                }
                finally
                {
                    if (zf != null)
                    {
                        zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                        zf.Close(); // Ensure we release resources
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool CreateZipFile(Stream inStream, string archiveName, string archiveFilenameOut)
        {
            try
            {
                FileStream fsOut = File.Create(archiveFilenameOut);
                ZipOutputStream zipStream = new ZipOutputStream(fsOut);
                zipStream.SetLevel(3);

                try
                {
                    ZipEntry newEntry = new ZipEntry(archiveName);
                    zipStream.PutNextEntry(newEntry);
                    byte[] buffer = new byte[4096];
                    StreamUtils.Copy(inStream, zipStream, buffer);
                    zipStream.CloseEntry();
                }
                finally
                {
                    zipStream.IsStreamOwner = true;
                    zipStream.Close();
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static bool VerifyContent(string contentFile, bool checkMd5, ProgressVerifyDelegate progressHandler)
        {
            try
            {
                string baseDir = Path.GetDirectoryName(contentFile);
                if (string.IsNullOrEmpty(baseDir))
                {
                    return false;
                }

                XDocument xmlDoc = XDocument.Load(contentFile);
                if (xmlDoc.Root == null)
                {
                    return false;
                }

                XElement[] fileNodes = xmlDoc.Root.Elements("file").ToArray();
                int nodeCount = fileNodes.Length;
                int index = 0;
                foreach (XElement fileNode in fileNodes)
                {
                    if (progressHandler != null)
                    {
                        if (progressHandler.Invoke(100 * index / nodeCount))
                        {
                            return false;
                        }
                    }

                    XAttribute nameAttr = fileNode.Attribute("name");
                    if (nameAttr == null)
                    {
                        return false;
                    }
                    string fileName = nameAttr.Value;
                    if (string.IsNullOrEmpty(fileName))
                    {
                        return false;
                    }

                    XAttribute sizeAttr = fileNode.Attribute("size");
                    if (sizeAttr == null)
                    {
                        return false;
                    }
                    long fileSize = XmlConvert.ToInt64(sizeAttr.Value);

                    // ReSharper disable once AssignNullToNotNullAttribute
                    string filePath = Path.Combine(baseDir, fileName);
                    FileInfo fileInfo = new FileInfo(filePath);
                    if (!fileInfo.Exists)
                    {
                        return false;
                    }

                    if (fileInfo.Length != fileSize)
                    {
                        return false;
                    }

                    if (checkMd5)
                    {
                        XAttribute md5Attr = fileNode.Attribute("md5");
                        if (md5Attr == null)
                        {
                            return false;
                        }

                        string md5String = md5Attr.Value;
                        using (var md5 = MD5.Create())
                        {
                            using (var stream = File.OpenRead(filePath))
                            {
                                byte[] md5Data = md5.ComputeHash(stream);
                                StringBuilder sb = new StringBuilder();
                                foreach (byte value in md5Data)
                                {
                                    sb.Append($"{value:X02}");
                                }

                                if (string.Compare(sb.ToString(), md5String, StringComparison.OrdinalIgnoreCase) != 0)
                                {
                                    return false;
                                }
                            }
                        }
                    }

                    index++;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static Dictionary<string, int> ExtractKeyWords(string archiveFilename, Regex regEx, int maxWords, ProgressZipDelegate progressHandler)
        {
            Dictionary<string, int> wordDict = new Dictionary<string, int>();
            ZipFile zf = null;
            try
            {
                long index = 0;
                FileStream fs = File.OpenRead(archiveFilename);
                zf = new ZipFile(fs);
                foreach (ZipEntry zipEntry in zf)
                {
                    if (progressHandler != null)
                    {
                        if (progressHandler((int)(100 * index / zf.Count)))
                        {
                            return null;
                        }
                    }
                    if (!zipEntry.IsFile)
                    {
                        continue;           // Ignore directories
                    }
                    Stream zipStream = zf.GetInputStream(zipEntry);
                    using (StreamReader sr = new StreamReader(zipStream))
                    {
                        bool exit = false;
                        for (; ; )
                        {
                            string line = sr.ReadLine();
                            if (line == null)
                            {
                                break;
                            }
                            string[] words = line.Split(' ', '\t', '\n', '\r');
                            foreach (string word in words)
                            {
                                if (string.IsNullOrEmpty(word))
                                {
                                    continue;
                                }
                                if (regEx.IsMatch(word))
                                {
                                    if (wordDict.ContainsKey(word))
                                    {
                                        wordDict[word] += 1;
                                    }
                                    else
                                    {
                                        if (maxWords > 0 && wordDict.Count > maxWords)
                                        {
                                            exit = true;
                                        }
                                        wordDict[word] = 1;
                                    }
                                }
                            }
                            if (exit)
                            {
                                break;
                            }
                        }
                    }

                    index++;
                }
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                    zf.Close(); // Ensure we release resources
                }
            }
            return wordDict;
        }

        public static string CreateValidFileName(string s, char replaceChar = '_', char[] includeChars = null)
        {
            char[] invalid = Path.GetInvalidFileNameChars();
            if (includeChars != null) invalid = invalid.Union(includeChars).ToArray();
            return string.Join(string.Empty, s.ToCharArray().Select(o => invalid.Contains(o) ? replaceChar : o));
        }

        private static bool IsEmulator()
        {
            string fing = Build.Fingerprint;
            bool isEmulator = false;
            if (fing != null)
            {
                isEmulator = fing.Contains("vbox") || fing.Contains("generic");
            }
            return isEmulator;
        }

        public static void WriteResourceToFile(string resourceName, string fileName)
        {
            using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (resource != null)
                {
                    using (var file = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                    {
                        resource.CopyTo(file);
                    }
                }
            }
        }

        public static bool IsCommunicationError(string exceptionText)
        {
            if (string.IsNullOrEmpty(exceptionText))
            {
                return false;
            }
            if (exceptionText.Contains("executeJob aborted"))
            {
                return false;
            }
            return true;
        }

        public static List<string> GetAllStorageMedia()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            {
                List<string> storageList = new List<string>();
                Java.IO.File[] externalFilesDirs = Android.App.Application.Context.GetExternalFilesDirs(null);
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (Java.IO.File file in externalFilesDirs)
                {
                    if (file != null && IsWritable(file.AbsolutePath))
                    {
                        storageList.Add(file.AbsolutePath);
                    }
                }
                return storageList;
            }
            string procMounts = ReadProcMounts();
            return ParseStorageMedia(procMounts);
        }

        public static void SetStoragePath()
        {
            _externalPath = Android.OS.Environment.ExternalStorageDirectory?.AbsolutePath;
            _externalWritePath = string.Empty;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            {   // writing to external disk is only allowed in special directories.
                Java.IO.File[] externalFilesDirs = Android.App.Application.Context.GetExternalFilesDirs(null);
                if (externalFilesDirs.Length > 0)
                {
                    // index 0 is the internal disk
                    if (externalFilesDirs.Length > 1 && externalFilesDirs[1] != null &&
                        IsWritable(externalFilesDirs[1].AbsolutePath))
                    {
                        _externalWritePath = externalFilesDirs[1].AbsolutePath;
                    }
                    else if (externalFilesDirs[0] != null &&
                        IsWritable(externalFilesDirs[0].AbsolutePath))
                    {
                        _externalWritePath = externalFilesDirs[0].AbsolutePath;
                    }
                }
            }
            else
            {
                string procMounts = ReadProcMounts();
                string sdCardEntry = ParseProcMounts(procMounts, _externalPath);
                if (!string.IsNullOrEmpty(sdCardEntry))
                {
                    _externalPath = sdCardEntry;
                }
            }
        }

        private static string ReadProcMounts()
        {
            try
            {
                string contents = File.ReadAllText("/proc/mounts");
                return contents;
            }
            catch (Exception)
            {
                // ignored
            }
            return string.Empty;
        }

        public static bool IsWritable(string pathToTest)
        {
            bool result = false;

            if (!string.IsNullOrWhiteSpace(pathToTest))
            {
                const string testText = "test text";
                string testFile = Guid.NewGuid() + ".txt";
                string testPath = Path.Combine(pathToTest, testFile);
                try
                {
                    File.WriteAllText(testPath, testText);
                    // case sensitive file systems are supported now
                    if (File.Exists(testPath))
                    {
                        result = true;
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
                finally
                {
                    try
                    {
                        if (File.Exists(testPath))
                        {
                            File.Delete(testPath);
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
            return result;
        }

        private static string ParseProcMounts(string procMounts, string externalPath)
        {
            string sdCardEntry = string.Empty;
            if (!string.IsNullOrWhiteSpace(procMounts))
            {
                List<string> procMountEntries = procMounts.Split('\n', '\r').ToList();
                foreach (string entry in procMountEntries)
                {
                    string[] sdCardEntries = entry.Split(' ');
                    if (sdCardEntries.Length > 2)
                    {
                        string path = sdCardEntries[1];
                        if (path.StartsWith(externalPath, StringComparison.OrdinalIgnoreCase) &&
                            string.Compare(path, externalPath, StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            if (IsWritable(path))
                            {
                                sdCardEntry = path;
                                break;
                            }
                        }
                    }
                }
            }
            return sdCardEntry;
        }

        private static List<string> ParseStorageMedia(string procMounts)
        {
            List<string> sdCardList = new List<string>();
            if (!string.IsNullOrWhiteSpace(procMounts))
            {
                List<string> procMountEntries = procMounts.Split('\n', '\r').ToList();
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (string entry in procMountEntries)
                {
                    string[] sdCardEntries = entry.Split(' ');
                    if (sdCardEntries.Length > 2)
                    {
                        string path = sdCardEntries[1];
                        if (IsWritable(path))
                        {
                            sdCardList.Add(path);
                        }
                    }
                }
            }
            return sdCardList;
        }

        public static FileSystemBlockInfo GetFileSystemBlockInfo(string path)
        {
            var statFs = new StatFs(path);
            var fsbi = new FileSystemBlockInfo();
            if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBeanMr2)
            {
                fsbi.Path = path;
                fsbi.BlockSizeBytes = statFs.BlockSizeLong;
                fsbi.TotalSizeBytes = statFs.BlockCountLong * statFs.BlockSizeLong;
                fsbi.AvailableSizeBytes = statFs.AvailableBlocksLong * statFs.BlockSizeLong;
                fsbi.FreeSizeBytes = statFs.FreeBlocksLong * statFs.BlockSizeLong;
            }
            else // this was deprecated in API level 18 (Android 4.3), so if your device is below level 18, this is what will be used instead.
            {
                fsbi.Path = path;
                // you may want to disable warning about obsoletes, earlier versions of Android are using the deprecated versions
#pragma warning disable 618
                fsbi.BlockSizeBytes = statFs.BlockSize;
                fsbi.TotalSizeBytes = statFs.BlockCount * (long)statFs.BlockSize;
                fsbi.AvailableSizeBytes = statFs.AvailableBlocks * (long)statFs.BlockSize;
                fsbi.FreeSizeBytes = statFs.FreeBlocks * (long)statFs.BlockSize;
#pragma warning restore 618
            }
            return fsbi;
        }

        public static long GetDirectorySize(string path)
        {
            if (!Directory.Exists(path))
            {
                return 0;
            }
            // 1.
            // Get array of all file names.
            string[] a = Directory.GetFiles(path, "*.*");

            // 2.
            // Calculate total bytes of all files in a loop.
            return a.Select(name => new FileInfo(name)).Select(info => info.Length).Sum();
        }

        private void UsbCheckEvent(Object state)
        {
            if (_usbCheckTimer == null)
            {
                return;
            }
            _activity?.RunOnUiThread(() =>
            {
                if (_disposed)
                {
                    return;
                }
                List<IUsbSerialDriver> availableDrivers = EdFtdiInterface.GetDriverList(_usbManager);
                if (availableDrivers.Count > _usbDeviceDetectCount)
                {   // device attached
                    _bcReceiverReceivedHandler?.Invoke(_context, null);
                    _bcReceiverUpdateDisplayHandler?.Invoke();
                }
                _usbDeviceDetectCount = availableDrivers.Count;
            });
        }

        private void NetworkTimerEvent(Object state)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                switch (_selectedInterface)
                {
                    case InterfaceType.Enet:
                    case InterfaceType.ElmWifi:
                    case InterfaceType.DeepObdWifi:
                        try
                        {
#pragma warning disable 618
                            _maConnectivity?.StartUsingNetworkFeature(ConnectivityType.Mobile, "enableHIPRI");
#pragma warning restore 618
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                        break;
                }
            }
        }

        public class Receiver : BroadcastReceiver
        {
            readonly ActivityCommon _activityCommon;

            public Receiver(ActivityCommon activityCommon)
            {
                _activityCommon = activityCommon;
            }

            public override void OnReceive(Context context, Intent intent)
            {
                string action = intent.Action;

                _activityCommon._bcReceiverReceivedHandler?.Invoke(context, intent);
                switch (action)
                {
                    case BluetoothAdapter.ActionStateChanged:
                    case ConnectivityManager.ConnectivityAction:
                        switch (_activityCommon._selectedInterface)
                        {
                            case InterfaceType.Bluetooth:
                            case InterfaceType.Enet:
                            case InterfaceType.ElmWifi:
                            case InterfaceType.DeepObdWifi:
                            {
                                bool interfaceAvailable = _activityCommon.IsInterfaceAvailable();
                                if (!_activityCommon._lastInvertfaceAvailable.HasValue ||
                                    _activityCommon._lastInvertfaceAvailable.Value != interfaceAvailable)
                                {
                                    _activityCommon._lastInvertfaceAvailable = interfaceAvailable;
                                    _activityCommon._bcReceiverUpdateDisplayHandler?.Invoke();
                                }
                                break;
                            }
                        }
                        if (action == BluetoothAdapter.ActionStateChanged)
                        {
                            BtNoEvents = false;
                            if (_activityCommon._activity is ActivityMain)
                            {
                                State extraState = (State)intent.GetIntExtra(BluetoothAdapter.ExtraState, (int)State.Disconnected);
                                switch (extraState)
                                {
                                    case State.TurningOn:
                                        _btEnableCounter--;
                                        break;

                                    case State.TurningOff:
                                        _btEnableCounter = 0;
                                        break;
                                }
                            }
                        }
                        if (action == ConnectivityManager.ConnectivityAction)
                        {
                            // using hipri here could create endless loop (and is set by timer anyway)
                            _activityCommon.SetPreferredNetworkInterface(false);
                        }
                        break;

                    case UsbManager.ActionUsbDeviceAttached:
                    case UsbManager.ActionUsbDeviceDetached:
                        {
                            UsbDevice usbDevice = intent.GetParcelableExtra(UsbManager.ExtraDevice) as UsbDevice;
                            if (EdFtdiInterface.IsValidUsbDevice(usbDevice))
                            {
                                _activityCommon._bcReceiverUpdateDisplayHandler?.Invoke();
                            }
                            break;
                        }

                    case ActionUsbPermission:
                        _activityCommon._usbPermissionRequested = false;
                        if (intent.GetBooleanExtra(UsbManager.ExtraPermissionGranted, false))
                        {
                            _activityCommon._bcReceiverUpdateDisplayHandler?.Invoke();
                        }
                        break;

                    case GlobalBroadcastReceiver.NotificationBroadcastAction:
                        _activityCommon._bcReceiverUpdateDisplayHandler?.Invoke();
                        break;
                }
            }
        }

        public class CellularCallback : ConnectivityManager.NetworkCallback
        {
            private readonly ActivityCommon _activityCommon;

            public CellularCallback(ActivityCommon activityCommon)
            {
                _activityCommon = activityCommon;
            }

            public override void OnAvailable(Network network)
            {
                NetworkInfo networkInfo = _activityCommon._maConnectivity.GetNetworkInfo(network);
                if (networkInfo != null && networkInfo.Type == ConnectivityType.Mobile)
                {
                    _activityCommon._mobileNetwork = network;
                    _activityCommon.SetPreferredNetworkInterface();
                }
            }

            public override void OnLost(Network network)
            {
                NetworkInfo networkInfo = _activityCommon._maConnectivity.GetNetworkInfo(network);
                if (networkInfo != null && networkInfo.Type == ConnectivityType.Mobile)
                {
                    _activityCommon._mobileNetwork = null;
                    _activityCommon.SetPreferredNetworkInterface();
                }
            }
        }
    }
}
