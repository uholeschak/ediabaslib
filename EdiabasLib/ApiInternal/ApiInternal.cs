using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using EdiabasLib;
// ReSharper disable InconsistentNaming
// ReSharper disable UseNullPropagation
// ReSharper disable once CheckNamespace
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable MergeCastWithTypeCheck

namespace Ediabas
{
    public class ApiInternal
    {
        public class APIRESULTFIELD
        {
            public List<Dictionary<string, EdiabasNet.ResultData>> ResultSets { get; set; }

            public APIRESULTFIELD(List<Dictionary<string, EdiabasNet.ResultData>> resultSets)
            {
                ResultSets = resultSets;
            }
        }

        public static readonly Encoding Encoding = Encoding.GetEncoding(1252);
        public static readonly CultureInfo Culture = CultureInfo.CreateSpecificCulture("en");
        protected static readonly long TickResolMs = Stopwatch.Frequency / 1000;
        private static bool _firstLog = true;

        private volatile EdiabasNet _ediabas;
        private string _lastIfh;
        private string _lastUnit;
        private string _lastApp;
        private string _lastConfig;
        private readonly object _apiLogLock = new object();
        private StreamWriter _swLog;
        private int _logLevelApi = -1;
        private int _busyCount;
        private volatile int _apiStateValue;
        private volatile int _localError;
        private volatile Thread _jobThread;
        private volatile string _jobName;
        private volatile string _jobEcuName;
        private volatile bool _abortJob;
        private volatile List<Dictionary<string, EdiabasNet.ResultData>> _resultSets;

        public enum ApiLogLevel
        {
            // ReSharper disable UnusedMember.Local
            Off = 0,
            Normal = 1,
            // ReSharper restore UnusedMember.Local
        };

        public const int APICOMPATIBILITYVERSION = 0x700;
        public const int APIBUSY = 0;
        public const int APIREADY = 1;
        public const int APIBREAK = 2;
        public const int APIERROR = 3;
        public const int APIMAXDEVICE = 64;
        public const int APIMAXNAME = 64;
        public const int APIMAXPARAEXT = 65536;
        public const int APIMAXPARA = 1024;
        public const int APIMAXSTDPARA = 256;
        public const int APIMAXRESULT = 256;
        public const int APIMAXFILENAME = 256;
        public const int APIMAXCONFIG = 256;
        public const int APIMAXTEXT = 1024;
        public const int APIMAXBINARYEXT = 65536;
        public const int APIMAXBINARY = 1024;
        public const int APIFORMAT_CHAR = 0;
        public const int APIFORMAT_BYTE = 1;
        public const int APIFORMAT_INTEGER = 2;
        public const int APIFORMAT_WORD = 3;
        public const int APIFORMAT_LONG = 4;
        public const int APIFORMAT_DWORD = 5;
        public const int APIFORMAT_TEXT = 6;
        public const int APIFORMAT_BINARY = 7;
        public const int APIFORMAT_REAL = 8;
        public const int EDIABAS_ERR_NONE = 0;
        public const int EDIABAS_RESERVED = 1;
        public const int EDIABAS_ERROR_CODE_OUT_OF_RANGE = 2;
        public const int EDIABAS_IFH_0000 = 10;
        public const int EDIABAS_IFH_0001 = 11;
        public const int EDIABAS_IFH_0002 = 12;
        public const int EDIABAS_IFH_0003 = 13;
        public const int EDIABAS_IFH_0004 = 14;
        public const int EDIABAS_IFH_0005 = 15;
        public const int EDIABAS_IFH_0006 = 16;
        public const int EDIABAS_IFH_0007 = 17;
        public const int EDIABAS_IFH_0008 = 18;
        public const int EDIABAS_IFH_0009 = 19;
        public const int EDIABAS_IFH_0010 = 20;
        public const int EDIABAS_IFH_0011 = 21;
        public const int EDIABAS_IFH_0012 = 22;
        public const int EDIABAS_IFH_0013 = 23;
        public const int EDIABAS_IFH_0014 = 24;
        public const int EDIABAS_IFH_0015 = 25;
        public const int EDIABAS_IFH_0016 = 26;
        public const int EDIABAS_IFH_0017 = 27;
        public const int EDIABAS_IFH_0018 = 28;
        public const int EDIABAS_IFH_0019 = 29;
        public const int EDIABAS_IFH_0020 = 30;
        public const int EDIABAS_IFH_0021 = 31;
        public const int EDIABAS_IFH_0022 = 32;
        public const int EDIABAS_IFH_0023 = 33;
        public const int EDIABAS_IFH_0024 = 34;
        public const int EDIABAS_IFH_0025 = 35;
        public const int EDIABAS_IFH_0026 = 36;
        public const int EDIABAS_IFH_0027 = 37;
        public const int EDIABAS_IFH_0028 = 38;
        public const int EDIABAS_IFH_0029 = 39;
        public const int EDIABAS_IFH_0030 = 40;
        public const int EDIABAS_IFH_0031 = 41;
        public const int EDIABAS_IFH_0032 = 42;
        public const int EDIABAS_IFH_0033 = 43;
        public const int EDIABAS_IFH_0034 = 44;
        public const int EDIABAS_IFH_0035 = 45;
        public const int EDIABAS_IFH_0036 = 46;
        public const int EDIABAS_IFH_0037 = 47;
        public const int EDIABAS_IFH_0038 = 48;
        public const int EDIABAS_IFH_0039 = 49;
        public const int EDIABAS_IFH_0040 = 50;
        public const int EDIABAS_IFH_0041 = 51;
        public const int EDIABAS_IFH_0042 = 52;
        public const int EDIABAS_IFH_0043 = 53;
        public const int EDIABAS_IFH_0044 = 54;
        public const int EDIABAS_IFH_0045 = 55;
        public const int EDIABAS_IFH_0046 = 56;
        public const int EDIABAS_IFH_0047 = 57;
        public const int EDIABAS_IFH_0048 = 58;
        public const int EDIABAS_IFH_0049 = 59;
        public const int EDIABAS_IFH_LAST = 59;
        public const int EDIABAS_BIP_0000 = 60;
        public const int EDIABAS_BIP_0001 = 61;
        public const int EDIABAS_BIP_0002 = 62;
        public const int EDIABAS_BIP_0003 = 63;
        public const int EDIABAS_BIP_0004 = 64;
        public const int EDIABAS_BIP_0005 = 65;
        public const int EDIABAS_BIP_0006 = 66;
        public const int EDIABAS_BIP_0007 = 67;
        public const int EDIABAS_BIP_0008 = 68;
        public const int EDIABAS_BIP_0009 = 69;
        public const int EDIABAS_BIP_0010 = 70;
        public const int EDIABAS_BIP_0011 = 71;
        public const int EDIABAS_BIP_0012 = 72;
        public const int EDIABAS_BIP_0013 = 73;
        public const int EDIABAS_BIP_0014 = 74;
        public const int EDIABAS_BIP_0015 = 75;
        public const int EDIABAS_BIP_0016 = 76;
        public const int EDIABAS_BIP_0017 = 77;
        public const int EDIABAS_BIP_0018 = 78;
        public const int EDIABAS_BIP_0019 = 79;
        public const int EDIABAS_BIP_0020 = 80;
        public const int EDIABAS_BIP_0021 = 81;
        public const int EDIABAS_BIP_0022 = 82;
        public const int EDIABAS_BIP_0023 = 83;
        public const int EDIABAS_BIP_0024 = 84;
        public const int EDIABAS_BIP_0025 = 85;
        public const int EDIABAS_BIP_0026 = 86;
        public const int EDIABAS_BIP_0027 = 87;
        public const int EDIABAS_BIP_0028 = 88;
        public const int EDIABAS_BIP_0029 = 89;
        public const int EDIABAS_BIP_LAST = 89;
        public const int EDIABAS_SYS_0000 = 90;
        public const int EDIABAS_SYS_0001 = 91;
        public const int EDIABAS_SYS_0002 = 92;
        public const int EDIABAS_SYS_0003 = 93;
        public const int EDIABAS_SYS_0004 = 94;
        public const int EDIABAS_SYS_0005 = 95;
        public const int EDIABAS_SYS_0006 = 96;
        public const int EDIABAS_SYS_0007 = 97;
        public const int EDIABAS_SYS_0008 = 98;
        public const int EDIABAS_SYS_0009 = 99;
        public const int EDIABAS_SYS_0010 = 100;
        public const int EDIABAS_SYS_0011 = 101;
        public const int EDIABAS_SYS_0012 = 102;
        public const int EDIABAS_SYS_0013 = 103;
        public const int EDIABAS_SYS_0014 = 104;
        public const int EDIABAS_SYS_0015 = 105;
        public const int EDIABAS_SYS_0016 = 106;
        public const int EDIABAS_SYS_0017 = 107;
        public const int EDIABAS_SYS_0018 = 108;
        public const int EDIABAS_SYS_0019 = 109;
        public const int EDIABAS_SYS_0020 = 110;
        public const int EDIABAS_SYS_0021 = 111;
        public const int EDIABAS_SYS_0022 = 112;
        public const int EDIABAS_SYS_0023 = 113;
        public const int EDIABAS_SYS_0024 = 114;
        public const int EDIABAS_SYS_0025 = 115;
        public const int EDIABAS_SYS_0026 = 116;
        public const int EDIABAS_SYS_0027 = 117;
        public const int EDIABAS_SYS_0028 = 118;
        public const int EDIABAS_SYS_0029 = 119;
        public const int EDIABAS_SYS_LAST = 119;
        public const int EDIABAS_API_0000 = 120;
        public const int EDIABAS_API_0001 = 121;
        public const int EDIABAS_API_0002 = 122;
        public const int EDIABAS_API_0003 = 123;
        public const int EDIABAS_API_0004 = 124;
        public const int EDIABAS_API_0005 = 125;
        public const int EDIABAS_API_0006 = 126;
        public const int EDIABAS_API_0007 = 127;
        public const int EDIABAS_API_0008 = 128;
        public const int EDIABAS_API_0009 = 129;
        public const int EDIABAS_API_0010 = 130;
        public const int EDIABAS_API_0011 = 131;
        public const int EDIABAS_API_0012 = 132;
        public const int EDIABAS_API_0013 = 133;
        public const int EDIABAS_API_0014 = 134;
        public const int EDIABAS_API_0015 = 135;
        public const int EDIABAS_API_0016 = 136;
        public const int EDIABAS_API_0017 = 137;
        public const int EDIABAS_API_0018 = 138;
        public const int EDIABAS_API_0019 = 139;
        public const int EDIABAS_API_0020 = 140;
        public const int EDIABAS_API_0021 = 141;
        public const int EDIABAS_API_0022 = 142;
        public const int EDIABAS_API_0023 = 143;
        public const int EDIABAS_API_0024 = 144;
        public const int EDIABAS_API_0025 = 145;
        public const int EDIABAS_API_0026 = 146;
        public const int EDIABAS_API_0027 = 147;
        public const int EDIABAS_API_0028 = 148;
        public const int EDIABAS_API_0029 = 149;
        public const int EDIABAS_API_LAST = 149;
        public const int EDIABAS_NET_0000 = 150;
        public const int EDIABAS_NET_0001 = 151;
        public const int EDIABAS_NET_0002 = 152;
        public const int EDIABAS_NET_0003 = 153;
        public const int EDIABAS_NET_0004 = 154;
        public const int EDIABAS_NET_0005 = 155;
        public const int EDIABAS_NET_0006 = 156;
        public const int EDIABAS_NET_0007 = 157;
        public const int EDIABAS_NET_0008 = 158;
        public const int EDIABAS_NET_0009 = 159;
        public const int EDIABAS_NET_0010 = 160;
        public const int EDIABAS_NET_0011 = 161;
        public const int EDIABAS_NET_0012 = 162;
        public const int EDIABAS_NET_0013 = 163;
        public const int EDIABAS_NET_0014 = 164;
        public const int EDIABAS_NET_0015 = 165;
        public const int EDIABAS_NET_0016 = 166;
        public const int EDIABAS_NET_0017 = 167;
        public const int EDIABAS_NET_0018 = 168;
        public const int EDIABAS_NET_0019 = 169;
        public const int EDIABAS_NET_0020 = 170;
        public const int EDIABAS_NET_0021 = 171;
        public const int EDIABAS_NET_0022 = 172;
        public const int EDIABAS_NET_0023 = 173;
        public const int EDIABAS_NET_0024 = 174;
        public const int EDIABAS_NET_0025 = 175;
        public const int EDIABAS_NET_0026 = 176;
        public const int EDIABAS_NET_0027 = 177;
        public const int EDIABAS_NET_0028 = 178;
        public const int EDIABAS_NET_0029 = 179;
        public const int EDIABAS_NET_0030 = 180;
        public const int EDIABAS_NET_0031 = 181;
        public const int EDIABAS_NET_0032 = 182;
        public const int EDIABAS_NET_0033 = 183;
        public const int EDIABAS_NET_0034 = 184;
        public const int EDIABAS_NET_0035 = 185;
        public const int EDIABAS_NET_0036 = 186;
        public const int EDIABAS_NET_0037 = 187;
        public const int EDIABAS_NET_0038 = 188;
        public const int EDIABAS_NET_0039 = 189;
        public const int EDIABAS_NET_0040 = 190;
        public const int EDIABAS_NET_0041 = 191;
        public const int EDIABAS_NET_0042 = 192;
        public const int EDIABAS_NET_0043 = 193;
        public const int EDIABAS_NET_0044 = 194;
        public const int EDIABAS_NET_0045 = 195;
        public const int EDIABAS_NET_0046 = 196;
        public const int EDIABAS_NET_0047 = 197;
        public const int EDIABAS_NET_0048 = 198;
        public const int EDIABAS_NET_0049 = 199;
        public const int EDIABAS_NET_LAST = 199;
        public const int EDIABAS_IFH_0050 = 200;
        public const int EDIABAS_IFH_0051 = 201;
        public const int EDIABAS_IFH_0052 = 202;
        public const int EDIABAS_IFH_0053 = 203;
        public const int EDIABAS_IFH_0054 = 204;
        public const int EDIABAS_IFH_0055 = 205;
        public const int EDIABAS_IFH_0056 = 206;
        public const int EDIABAS_IFH_0057 = 207;
        public const int EDIABAS_IFH_0058 = 208;
        public const int EDIABAS_IFH_0059 = 209;
        public const int EDIABAS_IFH_0060 = 210;
        public const int EDIABAS_IFH_0061 = 211;
        public const int EDIABAS_IFH_0062 = 212;
        public const int EDIABAS_IFH_0063 = 213;
        public const int EDIABAS_IFH_0064 = 214;
        public const int EDIABAS_IFH_0065 = 215;
        public const int EDIABAS_IFH_0066 = 216;
        public const int EDIABAS_IFH_0067 = 217;
        public const int EDIABAS_IFH_0068 = 218;
        public const int EDIABAS_IFH_0069 = 219;
        public const int EDIABAS_IFH_0070 = 220;
        public const int EDIABAS_IFH_0071 = 221;
        public const int EDIABAS_IFH_0072 = 222;
        public const int EDIABAS_IFH_0073 = 223;
        public const int EDIABAS_IFH_0074 = 224;
        public const int EDIABAS_IFH_0075 = 225;
        public const int EDIABAS_IFH_0076 = 226;
        public const int EDIABAS_IFH_0077 = 227;
        public const int EDIABAS_IFH_0078 = 228;
        public const int EDIABAS_IFH_0079 = 229;
        public const int EDIABAS_IFH_0080 = 230;
        public const int EDIABAS_IFH_0081 = 231;
        public const int EDIABAS_IFH_0082 = 232;
        public const int EDIABAS_IFH_0083 = 233;
        public const int EDIABAS_IFH_0084 = 234;
        public const int EDIABAS_IFH_0085 = 235;
        public const int EDIABAS_IFH_0086 = 236;
        public const int EDIABAS_IFH_0087 = 237;
        public const int EDIABAS_IFH_0088 = 238;
        public const int EDIABAS_IFH_0089 = 239;
        public const int EDIABAS_IFH_0090 = 240;
        public const int EDIABAS_IFH_0091 = 241;
        public const int EDIABAS_IFH_0092 = 242;
        public const int EDIABAS_IFH_0093 = 243;
        public const int EDIABAS_IFH_0094 = 244;
        public const int EDIABAS_IFH_0095 = 245;
        public const int EDIABAS_IFH_0096 = 246;
        public const int EDIABAS_IFH_0097 = 247;
        public const int EDIABAS_IFH_0098 = 248;
        public const int EDIABAS_IFH_0099 = 249;
        public const int EDIABAS_IFH_LAST2 = 249;
        public const int EDIABAS_RUN_0000 = 250;
        public const int EDIABAS_RUN_0001 = 251;
        public const int EDIABAS_RUN_0002 = 252;
        public const int EDIABAS_RUN_0003 = 253;
        public const int EDIABAS_RUN_0004 = 254;
        public const int EDIABAS_RUN_0005 = 255;
        public const int EDIABAS_RUN_0006 = 256;
        public const int EDIABAS_RUN_0007 = 257;
        public const int EDIABAS_RUN_0008 = 258;
        public const int EDIABAS_RUN_0009 = 259;
        public const int EDIABAS_RUN_0010 = 260;
        public const int EDIABAS_RUN_0011 = 261;
        public const int EDIABAS_RUN_0012 = 262;
        public const int EDIABAS_RUN_0013 = 263;
        public const int EDIABAS_RUN_0014 = 264;
        public const int EDIABAS_RUN_0015 = 265;
        public const int EDIABAS_RUN_0016 = 266;
        public const int EDIABAS_RUN_0017 = 267;
        public const int EDIABAS_RUN_0018 = 268;
        public const int EDIABAS_RUN_0019 = 269;
        public const int EDIABAS_RUN_0020 = 270;
        public const int EDIABAS_RUN_0021 = 271;
        public const int EDIABAS_RUN_0022 = 272;
        public const int EDIABAS_RUN_0023 = 273;
        public const int EDIABAS_RUN_0024 = 274;
        public const int EDIABAS_RUN_0025 = 275;
        public const int EDIABAS_RUN_0026 = 276;
        public const int EDIABAS_RUN_0027 = 277;
        public const int EDIABAS_RUN_0028 = 278;
        public const int EDIABAS_RUN_0029 = 279;
        public const int EDIABAS_RUN_0030 = 280;
        public const int EDIABAS_RUN_0031 = 281;
        public const int EDIABAS_RUN_0032 = 282;
        public const int EDIABAS_RUN_0033 = 283;
        public const int EDIABAS_RUN_0034 = 284;
        public const int EDIABAS_RUN_0035 = 285;
        public const int EDIABAS_RUN_0036 = 286;
        public const int EDIABAS_RUN_0037 = 287;
        public const int EDIABAS_RUN_0038 = 288;
        public const int EDIABAS_RUN_0039 = 289;
        public const int EDIABAS_RUN_0040 = 290;
        public const int EDIABAS_RUN_0041 = 291;
        public const int EDIABAS_RUN_0042 = 292;
        public const int EDIABAS_RUN_0043 = 293;
        public const int EDIABAS_RUN_0044 = 294;
        public const int EDIABAS_RUN_0045 = 295;
        public const int EDIABAS_RUN_0046 = 296;
        public const int EDIABAS_RUN_0047 = 297;
        public const int EDIABAS_RUN_0048 = 298;
        public const int EDIABAS_RUN_0049 = 299;
        public const int EDIABAS_RUN_0050 = 300;
        public const int EDIABAS_RUN_0051 = 301;
        public const int EDIABAS_RUN_0052 = 302;
        public const int EDIABAS_RUN_0053 = 303;
        public const int EDIABAS_RUN_0054 = 304;
        public const int EDIABAS_RUN_0055 = 305;
        public const int EDIABAS_RUN_0056 = 306;
        public const int EDIABAS_RUN_0057 = 307;
        public const int EDIABAS_RUN_0058 = 308;
        public const int EDIABAS_RUN_0059 = 309;
        public const int EDIABAS_RUN_0060 = 310;
        public const int EDIABAS_RUN_0061 = 311;
        public const int EDIABAS_RUN_0062 = 312;
        public const int EDIABAS_RUN_0063 = 313;
        public const int EDIABAS_RUN_0064 = 314;
        public const int EDIABAS_RUN_0065 = 315;
        public const int EDIABAS_RUN_0066 = 316;
        public const int EDIABAS_RUN_0067 = 317;
        public const int EDIABAS_RUN_0068 = 318;
        public const int EDIABAS_RUN_0069 = 319;
        public const int EDIABAS_RUN_0070 = 320;
        public const int EDIABAS_RUN_0071 = 321;
        public const int EDIABAS_RUN_0072 = 322;
        public const int EDIABAS_RUN_0073 = 323;
        public const int EDIABAS_RUN_0074 = 324;
        public const int EDIABAS_RUN_0075 = 325;
        public const int EDIABAS_RUN_0076 = 326;
        public const int EDIABAS_RUN_0077 = 327;
        public const int EDIABAS_RUN_0078 = 328;
        public const int EDIABAS_RUN_0079 = 329;
        public const int EDIABAS_RUN_0080 = 330;
        public const int EDIABAS_RUN_0081 = 331;
        public const int EDIABAS_RUN_0082 = 332;
        public const int EDIABAS_RUN_0083 = 333;
        public const int EDIABAS_RUN_0084 = 334;
        public const int EDIABAS_RUN_0085 = 335;
        public const int EDIABAS_RUN_0086 = 336;
        public const int EDIABAS_RUN_0087 = 337;
        public const int EDIABAS_RUN_0088 = 338;
        public const int EDIABAS_RUN_0089 = 339;
        public const int EDIABAS_RUN_0090 = 340;
        public const int EDIABAS_RUN_0091 = 341;
        public const int EDIABAS_RUN_0092 = 342;
        public const int EDIABAS_RUN_0093 = 343;
        public const int EDIABAS_RUN_0094 = 344;
        public const int EDIABAS_RUN_0095 = 345;
        public const int EDIABAS_RUN_0096 = 346;
        public const int EDIABAS_RUN_0097 = 347;
        public const int EDIABAS_RUN_0098 = 348;
        public const int EDIABAS_RUN_0099 = 349;
        public const int EDIABAS_RUN_LAST = 349;
        public const int EDIABAS_ERROR_LAST = 349;

        static ApiInternal()
        {
            LoadAllResourceAssemblies();
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                string fullName = args.Name;
                if (!string.IsNullOrEmpty(fullName))
                {
                    Assembly[] currentAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                    foreach (Assembly loadedAssembly in currentAssemblies)
                    {
                        if (string.IsNullOrEmpty(loadedAssembly.Location) &&
                            string.Compare(loadedAssembly.FullName, fullName, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            return loadedAssembly;
                        }
                    }

                    string[] names = fullName.Split(',');
                    if (names.Length < 1)
                    {
                        return null;
                    }

                    string assemblyName = names[0];
                    string assemblyDllName = assemblyName + ".dll";
                    string assemblyDir = AssemblyDirectory;
                    if (string.IsNullOrEmpty(assemblyDir))
                    {
                        return null;
                    }

                    string assemblyFileName = Path.Combine(assemblyDir, assemblyDllName);
                    if (!File.Exists(assemblyFileName))
                    {
                        return null;
                    }

                    return Assembly.LoadFrom(assemblyFileName);
                }
                return null;
            };
        }

        public ApiInternal()
        {
            _ediabas = null;
            _busyCount = 0;
            _apiStateValue = APIREADY;
            _localError = EDIABAS_ERR_NONE;
            _jobThread = null;
            _jobName = string.Empty;
            _jobEcuName = string.Empty;
            _abortJob = false;
            _resultSets = null;
        }

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public static bool LoadAllResourceAssemblies()
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                string[] resourceNames = assembly.GetManifestResourceNames();

                foreach (string resourceName in resourceNames)
                {
                    if (!resourceName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                    {
                        if (stream != null)
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                stream.CopyTo(memoryStream);
                                Assembly loadedAssembly = Assembly.Load(memoryStream.ToArray());
                            }
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

        public static bool apiCheckVersion(int versionCompatibility, out string versionInfo)
        {
            versionInfo = string.Empty;
            if (versionCompatibility < APICOMPATIBILITYVERSION)
            {
                return false;
            }
            versionInfo = string.Format("{0}.{1}.{2}", (EdiabasNet.EdiabasVersion >> 8) & 0xF, (EdiabasNet.EdiabasVersion >> 4) & 0xF, EdiabasNet.EdiabasVersion & 0xF);
            return true;
        }

        public bool apiInit()
        {
            return apiInitExt(null, null, null, null);
        }

        public bool apiInitExt(string ifh, string unit, string app, string config)
        {
            if (_ediabas != null)
            {
                logFormat(ApiLogLevel.Normal, "apiInitExt({0}, {1}, {2}, {3})", ifh, unit, app, config);

                if (_lastIfh == ifh && _lastUnit == unit && _lastApp == app && _lastConfig == config)
                {
                    logFormat(ApiLogLevel.Normal, "={0} ()", true);
                    return true;
                }

                logFormat(ApiLogLevel.Normal, "Settings have changed, calling apiEnd()");
                apiEnd();
            }

            _busyCount = 0;
            _apiStateValue = APIREADY;
            _localError = EDIABAS_ERR_NONE;
            _jobThread = null;
            _jobName = string.Empty;
            _jobEcuName = string.Empty;
            _abortJob = false;
            _resultSets = null;

            setLocalError(EDIABAS_ERR_NONE);

            _ediabas = new EdiabasNet(config);
            logFormat(ApiLogLevel.Normal, "apiInitExt({0}, {1}, {2}, {3})", ifh, unit, app, config);

            if (!string.IsNullOrEmpty(unit))
            {
                if (char.IsLetter(unit[0]))
                {
                    setLocalError(EDIABAS_IFH_0018);
                    _ediabas.Dispose();
                    logFormat(ApiLogLevel.Normal, "Unit invalid: {0}", unit);
                    logFormat(ApiLogLevel.Normal, "={0} ()", false);
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(ifh))
            {
                string[] ifhParts = ifh.Split(':');
                if (ifhParts.Length > 0 && string.Compare(ifhParts[0], "REMOTE", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    logFormat(ApiLogLevel.Normal, "Ignoring REMOTE");
                    ifh = null;
                }
            }

            if (string.IsNullOrEmpty(ifh))
            {
                ifh = _ediabas.GetConfigProperty("Interface");
            }

            EdInterfaceBase edInterface;
            if (!string.IsNullOrEmpty(ifh))
            {
                if (EdInterfaceObd.IsValidInterfaceNameStatic(ifh))
                {
                    edInterface = new EdInterfaceObd();
                }
                else if (EdInterfaceEdic.IsValidInterfaceNameStatic(ifh))
                {
                    edInterface = new EdInterfaceEdic();
                }
#if !Android
                else if (EdInterfaceAds.IsValidInterfaceNameStatic(ifh))
                {
                    edInterface = new EdInterfaceAds();
                }
#endif
                else if (EdInterfaceEnet.IsValidInterfaceNameStatic(ifh))
                {
                    edInterface = new EdInterfaceEnet();
                }
                else
                {
                    setLocalError(EDIABAS_IFH_0027);
                    _ediabas.Dispose();
                    logFormat(ApiLogLevel.Normal, "Ifh invalid: {0}", ifh);
                    logFormat(ApiLogLevel.Normal, "={0} ()", false);
                    return false;
                }
            }
            else
            {
                edInterface = new EdInterfaceObd();
            }

            if (!edInterface.InterfaceLock())
            {
                setLocalError(EDIABAS_API_0006);
                edInterface.Dispose();
                _ediabas.Dispose();
                logFormat(ApiLogLevel.Normal, "Interface lock failed");
                logFormat(ApiLogLevel.Normal, "={0} ()", false);
                return false;
            }

            _ediabas.EdInterfaceClass = edInterface;
            _ediabas.AbortJobFunc = abortJobFunc;
            _lastIfh = ifh;
            _lastUnit = unit;
            _lastApp = app;
            _lastConfig = config;

            logFormat(ApiLogLevel.Normal, "={0} ()", true);
            return true;
        }

        public void apiEnd()
        {
            logFormat(ApiLogLevel.Normal, "apiEnd()");

            if (_ediabas != null)
            {
                _abortJob = true;
                while (_jobThread != null)
                {
                    Thread.Sleep(10);
                }
                closeLog();
                _ediabas.Dispose();
                _ediabas = null;
            }
        }

        public bool apiSwitchDevice(string unit, string app)
        {
            logFormat(ApiLogLevel.Normal, "apiSwitchDevice({0}, {1})", unit, app);

            setLocalError(EDIABAS_ERR_NONE);
            if (!string.IsNullOrEmpty(unit))
            {
                if (char.IsLetter(unit[0]))
                {
                    setLocalError(EDIABAS_IFH_0018);
                    return false;
                }
            }
            return true;
        }

        public void apiJob(string ecu, string job, string para, string result)
        {
            logFormat(ApiLogLevel.Normal, "apiJob({0}, {1}, {2}, {3})", ecu, job, para, result);

            byte[] paraBytes = (para == null) ? new byte[0] : Encoding.GetBytes(para);
            executeJob(ecu, job, null, 0, paraBytes, paraBytes.Length, result);
        }

        public void apiJobData(string ecu, string job, byte[] para, int paralen, string result)
        {
            logFormat(ApiLogLevel.Normal, "apiJobData({0}, {1}, {2}, {3}, {4})", ecu, job, para, paralen, result);

            executeJob(ecu, job, null, 0, para, paralen, result);
        }

        public void apiJobExt(string ecu, string job, byte[] stdpara, int stdparalen, byte[] para, int paralen, string result, int reserved)
        {
            logFormat(ApiLogLevel.Normal, "apiJobExt({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})", ecu, job, stdpara, stdparalen, para, paralen, result, reserved);

            executeJob(ecu, job, stdpara, stdparalen, para, paralen, result);
        }

        public int apiJobInfo(out string infoText)
        {
            logFormat(ApiLogLevel.Normal, "apiJobInfo()");

            int progressPercent = 0;
            if (_ediabas == null)
            {
                setLocalError(EDIABAS_API_0006);
                infoText = string.Empty;
                logFormat(ApiLogLevel.Normal, "={0} ({1})", progressPercent, infoText);
                return progressPercent;
            }
            infoText = _ediabas.InfoProgressText;

            progressPercent = _ediabas.InfoProgressPercent;

            logFormat(ApiLogLevel.Normal, "={0} ({1})", progressPercent, infoText);
            return progressPercent;
        }

        public bool apiResultChar(out char buffer, string result, ushort rset)
        {
            logFormat(ApiLogLevel.Normal, "apiResultChar({0}, {1})", result, rset);

            buffer = '\0';
            if (!waitJobFinish())
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            setLocalError(EDIABAS_ERR_NONE);
            Int64 int64Buffer;
            if (!getResultInt64(out int64Buffer, result, rset))
            {
                return false;
            }
            buffer = (char)int64Buffer;

            logFormat(ApiLogLevel.Normal, "={0} ({1})", true, buffer);
            return true;
        }

        public bool apiResultByte(out byte buffer, string result, ushort rset)
        {
            logFormat(ApiLogLevel.Normal, "apiResultByte({0}, {1})", result, rset);

            buffer = 0;
            if (!waitJobFinish())
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            setLocalError(EDIABAS_ERR_NONE);
            Int64 int64Buffer;
            if (!getResultInt64(out int64Buffer, result, rset))
            {
                return false;
            }
            buffer = (byte)int64Buffer;

            logFormat(ApiLogLevel.Normal, "={0} ({1})", true, buffer);
            return true;
        }

        public bool apiResultInt(out short buffer, string result, ushort rset)
        {
            logFormat(ApiLogLevel.Normal, "apiResultInt({0}, {1})", result, rset);

            buffer = 0;
            if (!waitJobFinish())
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            setLocalError(EDIABAS_ERR_NONE);
            Int64 int64Buffer;
            if (!getResultInt64(out int64Buffer, result, rset))
            {
                return false;
            }
            buffer = (short)int64Buffer;

            logFormat(ApiLogLevel.Normal, "={0} ({1})", true, buffer);
            return true;
        }

        public bool apiResultWord(out ushort buffer, string result, ushort rset)
        {
            logFormat(ApiLogLevel.Normal, "apiResultWord({0}, {1})", result, rset);

            buffer = 0;
            if (!waitJobFinish())
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            setLocalError(EDIABAS_ERR_NONE);
            Int64 int64Buffer;
            if (!getResultInt64(out int64Buffer, result, rset))
            {
                return false;
            }
            buffer = (ushort)int64Buffer;

            logFormat(ApiLogLevel.Normal, "={0} ({1})", true, buffer);
            return true;
        }

        public bool apiResultLong(out int buffer, string result, ushort rset)
        {
            logFormat(ApiLogLevel.Normal, "apiResultLong({0}, {1})", result, rset);

            buffer = 0;
            if (!waitJobFinish())
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            setLocalError(EDIABAS_ERR_NONE);
            Int64 int64Buffer;
            if (!getResultInt64(out int64Buffer, result, rset))
            {
                return false;
            }
            buffer = (int)int64Buffer;

            logFormat(ApiLogLevel.Normal, "={0} ({1})", true, buffer);
            return true;
        }

        public bool apiResultDWord(out uint buffer, string result, ushort rset)
        {
            logFormat(ApiLogLevel.Normal, "apiResultDWord({0}, {1})", result, rset);

            buffer = 0;
            if (!waitJobFinish())
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            setLocalError(EDIABAS_ERR_NONE);
            Int64 int64Buffer;
            if (!getResultInt64(out int64Buffer, result, rset))
            {
                return false;
            }
            buffer = (uint)int64Buffer;

            logFormat(ApiLogLevel.Normal, "={0} ({1})", true, buffer);
            return true;
        }

        public bool apiResultReal(out double buffer, string result, ushort rset)
        {
            logFormat(ApiLogLevel.Normal, "apiResultReal({0}, {1})", result, rset);

            buffer = 0;
            if (!waitJobFinish())
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            setLocalError(EDIABAS_ERR_NONE);
            EdiabasNet.ResultData resultData = getResultData(result, rset);
            if (resultData == null)
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }
            if ((resultData.OpData is long))
            {
                Int64 value = (Int64)resultData.OpData;
                buffer = value;
            }
            else if ((resultData.OpData is double))
            {
                buffer = (Double)resultData.OpData;
            }
            else if ((resultData.OpData is string))
            {
                buffer = EdiabasNet.StringToFloat((string) resultData.OpData);
            }
            else
            {
                setLocalError(EDIABAS_API_0005);
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            logFormat(ApiLogLevel.Normal, "={0} ({1})", true, buffer);
            return true;
        }

        public bool apiResultText(out string buffer, string result, ushort rset, string format)
        {
            logFormat(ApiLogLevel.Normal, "apiResultText({0}, {1}, {2})", result, rset, format);

            buffer = string.Empty;
            if (!waitJobFinish())
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            setLocalError(EDIABAS_ERR_NONE);
            EdiabasNet.ResultData resultData = getResultData(result, rset);
            if (resultData == null)
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            string value = EdiabasNet.FormatResult(resultData, format);
            if (value == null)
            {
                setLocalError(EDIABAS_API_0005);
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }
            buffer = value;

            logFormat(ApiLogLevel.Normal, "={0} ({1})", true, buffer);
            return true;
        }

        public bool apiResultText(out char[] buffer, string result, ushort rset, string format)
        {
            logFormat(ApiLogLevel.Normal, "apiResultText({0}, {1}, {2})", result, rset, format);

            buffer = null;
            if (!waitJobFinish())
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            setLocalError(EDIABAS_ERR_NONE);
            string text;
            if (apiResultText(out text, result, rset, format))
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }
            char[] charArray = text.ToCharArray();
            Array.Resize(ref charArray, charArray.Length + 1);
            charArray[charArray.Length - 1] = '\0';
            buffer = charArray;

            logFormat(ApiLogLevel.Normal, "={0} ({1})", true, buffer);
            return true;
        }

        public bool apiResultBinary(out byte[] buffer, out ushort bufferLen, string result, ushort rset)
        {
            logFormat(ApiLogLevel.Normal, "apiResultText({0}, {1})", result, rset);

            buffer = null;
            bufferLen = 0;
            if (!waitJobFinish())
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            setLocalError(EDIABAS_ERR_NONE);
            EdiabasNet.ResultData resultData = getResultData(result, rset);
            if (resultData == null)
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }
            if ((resultData.ResType != EdiabasNet.ResultType.TypeY) || (resultData.OpData.GetType() != typeof(byte[])))
            {
                setLocalError(EDIABAS_API_0005);
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }
            byte[] value = (byte[])resultData.OpData;
            buffer = new byte[APIMAXBINARY];
            int dataLength = value.Length;
            if (value.Length > buffer.Length)
            {
                dataLength = buffer.Length;
            }
            Array.Copy(value, buffer, dataLength);
            bufferLen = (ushort)dataLength;

            logFormat(ApiLogLevel.Normal, "={0} ({1})", true, value);
            return true;
        }

        public bool apiResultBinaryExt(out byte[] buffer, out uint bufferLen, uint bufferSize, string result, ushort rset)
        {
            logFormat(ApiLogLevel.Normal, "apiResultBinaryExt({0}, {1})", result, rset);

            buffer = null;
            bufferLen = 0;
            if (!waitJobFinish())
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            setLocalError(EDIABAS_ERR_NONE);
            EdiabasNet.ResultData resultData = getResultData(result, rset);
            if (resultData == null)
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }
            if ((resultData.ResType != EdiabasNet.ResultType.TypeY) || (resultData.OpData.GetType() != typeof(byte[])))
            {
                setLocalError(EDIABAS_API_0005);
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }
            byte[] value = (byte[])resultData.OpData;
            buffer = new byte[APIMAXBINARYEXT];
            int dataLength = value.Length;
            if (value.Length > buffer.Length)
            {
                dataLength = buffer.Length;
            }
            Array.Copy(value, buffer, dataLength);
            bufferLen = (ushort)dataLength;

            logFormat(ApiLogLevel.Normal, "={0} ({1})", true, value);
            return true;
        }

        public bool apiResultFormat(out int buffer, string result, ushort rset)
        {
            logFormat(ApiLogLevel.Normal, "apiResultFormat({0}, {1})", result, rset);

            buffer = 0;
            if (!waitJobFinish())
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            setLocalError(EDIABAS_ERR_NONE);
            EdiabasNet.ResultData resultData = getResultData(result, rset);
            if (resultData == null)
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            switch (resultData.ResType)
            {
                case EdiabasNet.ResultType.TypeB:
                    buffer = APIFORMAT_BYTE;
                    break;

                case EdiabasNet.ResultType.TypeW:
                    buffer = APIFORMAT_WORD;
                    break;

                case EdiabasNet.ResultType.TypeD:
                    buffer = APIFORMAT_DWORD;
                    break;

                case EdiabasNet.ResultType.TypeC:
                    buffer = APIFORMAT_CHAR;
                    break;

                case EdiabasNet.ResultType.TypeI:
                    buffer = APIFORMAT_INTEGER;
                    break;

                case EdiabasNet.ResultType.TypeL:
                    buffer = APIFORMAT_LONG;
                    break;

                case EdiabasNet.ResultType.TypeR:
                    buffer = APIFORMAT_REAL;
                    break;

                case EdiabasNet.ResultType.TypeS:
                    buffer = APIFORMAT_TEXT;
                    break;

                case EdiabasNet.ResultType.TypeY:
                    buffer = APIFORMAT_BINARY;
                    break;

                default:
                    setLocalError(EDIABAS_API_0005);
                    break;
            }

            logFormat(ApiLogLevel.Normal, "={0} ({1})", true, buffer);
            return true;
        }

        public bool apiResultNumber(out ushort buffer, ushort rset)
        {
            logFormat(ApiLogLevel.Normal, "apiResultNumber({0})", rset);

            buffer = 0;
            if (!waitJobFinish())
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            setLocalError(EDIABAS_ERR_NONE);
            if (_resultSets == null)
            {
                setLocalError(EDIABAS_API_0014);
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }
            if (rset >= _resultSets.Count)
            {
                setLocalError(EDIABAS_API_0014);
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }
            buffer = (ushort)_resultSets[rset].Count;

            logFormat(ApiLogLevel.Normal, "={0} ({1})", true, buffer);
            return true;
        }

        public bool apiResultName(out string buffer, ushort index, ushort rset)
        {
            logFormat(ApiLogLevel.Normal, "apiResultName({0}, {1})", index, rset);

            buffer = string.Empty;
            if (!waitJobFinish())
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            setLocalError(EDIABAS_ERR_NONE);
            if (_resultSets == null)
            {
                setLocalError(EDIABAS_API_0014);
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }
            if (rset >= _resultSets.Count)
            {
                setLocalError(EDIABAS_API_0014);
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            Dictionary<string, EdiabasNet.ResultData> resultDict = _resultSets[rset];
            if ((index < 1) || (index > resultDict.Keys.Count))
            {
                setLocalError(EDIABAS_API_0014);
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            buffer = resultDict.Values.ElementAt(index - 1).Name;
            logFormat(ApiLogLevel.Normal, "={0} ({1})", true, buffer);
            return true;
        }

        public bool apiResultSets(out ushort rsets)
        {
            logFormat(ApiLogLevel.Normal, "apiResultSets()");

            rsets = 0;
            if (!waitJobFinish())
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }

            setLocalError(EDIABAS_ERR_NONE);
            if (_ediabas == null)
            {
                setLocalError(EDIABAS_API_0006);
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }
            if (_resultSets == null)
            {
                setLocalError(EDIABAS_API_0014);
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }
            if (_resultSets.Count < 1)
            {
                setLocalError(EDIABAS_API_0014);
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }
            rsets = (ushort)(_resultSets.Count - 1);

            logFormat(ApiLogLevel.Normal, "={0} ({1})", true, rsets);
            return true;
        }

        public bool apiResultVar(out string var)
        {
            logFormat(ApiLogLevel.Normal, "apiResultSets()");

            bool result = apiResultText(out var, "VARIANTE", 0, string.Empty);

            logFormat(ApiLogLevel.Normal, "={0} ({1})", result, var);
            return result;
        }

        public APIRESULTFIELD apiResultsNew()
        {
            logFormat(ApiLogLevel.Normal, "apiResultsNew()");

            waitJobFinish();
            APIRESULTFIELD resultField = new APIRESULTFIELD(_resultSets);

            return resultField;
        }

        public void apiResultsScope(APIRESULTFIELD resultField)
        {
            logFormat(ApiLogLevel.Normal, "apiResultsScope()");

            waitJobFinish();
            _resultSets = resultField.ResultSets;
        }

        public void apiResultsDelete(APIRESULTFIELD resultField)
        {
            logFormat(ApiLogLevel.Normal, "apiResultsDelete()");

            resultField.ResultSets = null;
        }

        public int apiState()
        {
            if (_apiStateValue != APIBUSY)
            {
                logFormat(ApiLogLevel.Normal, "apiState()={0} busy={1}", _apiStateValue, _busyCount);
                _busyCount = 0;
            }
            else
            {
                _busyCount++;
            }
            return _apiStateValue;
        }

        public int apiStateExt(int suspendTime)
        {
            int state = _apiStateValue;
            if (state == APIBUSY)
            {
                long startTime = Stopwatch.GetTimestamp();
                while ((Stopwatch.GetTimestamp() - startTime) < suspendTime * TickResolMs)
                {
                    state = _apiStateValue;
                    if (state != APIBUSY)
                    {
                        break;
                    }
                    Thread.Sleep(10);
                }
            }

            if (state != APIBUSY)
            {
                logFormat(ApiLogLevel.Normal, "apiStateExt({0})={1} busy={2}", suspendTime, state, _busyCount);
                _busyCount = 0;
            }
            else
            {
                _busyCount++;
            }
            return state;
        }

        public void apiBreak()
        {
            logFormat(ApiLogLevel.Normal, "apiBreak()");

            if (_jobThread == null)
            {
                return;
            }
            _abortJob = true;
        }

        public int apiErrorCode()
        {
            //logFormat(ApiLogLevel.Normal, "apiErrorCode()");

            if (_localError != EDIABAS_ERR_NONE)
            {
                logFormat(ApiLogLevel.Normal, "apiErrorCode()");
                logFormat(ApiLogLevel.Normal, "={0} ()", (EdiabasNet.ErrorCodes)_localError);
                return _localError;
            }
            if (_ediabas == null)
            {
                logFormat(ApiLogLevel.Normal, "apiErrorCode()");
                logFormat(ApiLogLevel.Normal, "={0} ()", EDIABAS_API_0006);
                return EDIABAS_API_0006;
            }

            if (_ediabas.ErrorCodeLast != EDIABAS_ERR_NONE)
            {
                logFormat(ApiLogLevel.Normal, "apiErrorCode()");
                logFormat(ApiLogLevel.Normal, "={0} ()", _ediabas.ErrorCodeLast);
            }
            return (int)_ediabas.ErrorCodeLast;
        }

        public string apiErrorText()
        {
            logFormat(ApiLogLevel.Normal, "apiErrorText()");

            string errorText = EdiabasNet.GetErrorDescription((EdiabasNet.ErrorCodes)apiErrorCode());

            logFormat(ApiLogLevel.Normal, "={0} ()", errorText);
            return errorText;
        }

        public bool apiSetConfig(string cfgName, string cfgValue)
        {
            logFormat(ApiLogLevel.Normal, "apiSetConfig({0}, {1})", cfgName, cfgValue);

            if (_ediabas == null)
            {
                setLocalError(EDIABAS_API_0006);
                logFormat(ApiLogLevel.Normal, "={0} ()", false);
                return false;
            }
            bool setProperty = true;
#if false   // for debugging only!
            if (string.Compare(cfgName, "ApiTrace", StringComparison.OrdinalIgnoreCase) == 0)
            {
                setProperty = false;
            }
#endif
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (setProperty)
            {
                _ediabas.SetConfigProperty(cfgName, cfgValue);
            }
            if (string.Compare(cfgName, "TracePath", StringComparison.OrdinalIgnoreCase) == 0)
            {
                closeLog();
            }
            if (string.Compare(cfgName, "ApiTrace", StringComparison.OrdinalIgnoreCase) == 0)
            {
                closeLog();
            }
            if (string.Compare(cfgName, "TraceBuffering", StringComparison.OrdinalIgnoreCase) == 0)
            {
                closeLog();
            }
            if (string.Compare(cfgName, "ApiTraceName", StringComparison.OrdinalIgnoreCase) == 0)
            {
                closeLog();
            }

            logFormat(ApiLogLevel.Normal, "={0} ()", true);
            return true;
        }

        public bool apiGetConfig(string cfgName, out string cfgValue)
        {
            logFormat(ApiLogLevel.Normal, "apiGetConfig({0})", cfgName);

            cfgValue = string.Empty;
            if (_ediabas == null)
            {
                setLocalError(EDIABAS_API_0006);
                logFormat(ApiLogLevel.Normal, "={0} ()", false);
                return false;
            }
            string prop = _ediabas.GetConfigProperty(cfgName);
            if (prop != null)
            {
                cfgValue = prop;
            }

            logFormat(ApiLogLevel.Normal, "={0} ({1})", true, cfgValue);
            return true;
        }

        public void apiTrace(string msg)
        {
            logFormat(ApiLogLevel.Normal, "apiTrace({0})", msg);
        }

        public static bool apiXSysSetConfig(string cfgName, string cfgValue)
        {
            //logFormat(API_LOG_LEVEL.NORMAL, "apiXSysSetConfig({0}, {1})", cfgName, cfgValue);
            return true;
        }

        public static void closeServer()
        {
            //logFormat(API_LOG_LEVEL.NORMAL, "closeServer()");
        }

        public static bool enableServer(bool onOff)
        {
            //logFormat(API_LOG_LEVEL.NORMAL, "enableServer({0})", onOff);
            return true;
        }

        public static bool enableMultiThreading(bool onOff)
        {
            //logFormat(API_LOG_LEVEL.NORMAL, "enableMultiThreading({0})", onOff);
            return true;
        }

        private void setLocalError(int error)
        {
            if (error == EDIABAS_ERR_NONE)
            {
                switch (_localError)
                {
                    case EDIABAS_API_0005:
                    case EDIABAS_API_0014:
                        _localError = error;
                        break;
                }
            }
            _localError = error;
        }

        private void setJobError(int error)
        {
            if (error == EDIABAS_ERR_NONE)
            {
                _localError = error;
                _apiStateValue = APIREADY;
                return;
            }

            _localError = error;
            _apiStateValue = APIERROR;
        }

        private bool waitJobFinish()
        {
            while (_apiStateValue == APIBUSY)
            {
                Thread.Sleep(10);
            }
            if (_apiStateValue != APIREADY)
            {
                return false;
            }
            return true;
        }

        private EdiabasNet.ResultData getResultData(string result, ushort rset)
        {
            if (_resultSets == null)
            {
                setLocalError(EDIABAS_API_0014);
                return null;
            }
            if (rset >= _resultSets.Count)
            {
                setLocalError(EDIABAS_API_0014);
                return null;
            }

            Dictionary<string, EdiabasNet.ResultData> resultDict = _resultSets[rset];
            EdiabasNet.ResultData resultData;
            if (!resultDict.TryGetValue(result.ToUpper(Culture), out resultData))
            {
                setLocalError(EDIABAS_API_0014);
                return null;
            }
            return resultData;
        }

        private bool getResultInt64(out Int64 buffer, string result, ushort rset)
        {
            buffer = 0;
            EdiabasNet.ResultData resultData = getResultData(result, rset);
            if (resultData == null)
            {
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }
            if ((resultData.OpData is long))
            {
                buffer = (Int64)resultData.OpData;
            }
            else if ((resultData.OpData is double))
            {
                Double value = (Double)resultData.OpData;
                buffer = (Int64)value;
            }
            else if ((resultData.OpData is string))
            {
                buffer = EdiabasNet.StringToValue((string)resultData.OpData);
            }
            else
            {
                setLocalError(EDIABAS_API_0005);
                logFormat(ApiLogLevel.Normal, "={0}", false);
                return false;
            }
            return true;
        }

        public void executeJob(string ecu, string job, byte[] stdpara, int stdparalen, byte[] para, int paralen, string result)
        {
            if (_ediabas == null)
            {
                setJobError(EDIABAS_API_0006);
                return;
            }
            // wait for last job to finish
            while (_jobThread != null)
            {
                Thread.Sleep(10);
            }

            setJobError(EDIABAS_ERR_NONE);
            _resultSets = null;

            try
            {
                if (para != null && para.Length != paralen)
                {
                    byte[] binData = new byte[paralen];
                    int copyLen = paralen;
                    if (copyLen > para.Length)
                    {
                        copyLen = para.Length;
                    }
                    Array.Copy(para, binData, copyLen);
                    _ediabas.ArgBinary = binData;
                }
                else
                {
                    _ediabas.ArgBinary = para;
                }

                if (stdpara != null && stdpara.Length != stdparalen)
                {
                    byte[] binData = new byte[stdparalen];
                    int copyLen = stdparalen;
                    if (copyLen > stdpara.Length)
                    {
                        copyLen = stdpara.Length;
                    }
                    Array.Copy(stdpara, binData, copyLen);
                    _ediabas.ArgBinaryStd = binData;
                }
                else
                {
                    _ediabas.ArgBinaryStd = stdpara;
                }

                _ediabas.ResultsRequests = result;
            }
            catch (Exception)
            {
                setJobError(EDIABAS_SYS_0000);
                return;
            }
            _jobName = job;
            _jobEcuName = ecu;
            _abortJob = false;
            _apiStateValue = APIBUSY;
            _jobThread = new Thread(jobThreadFunc);
            _jobThread.Start();
        }

        private void jobThreadFunc()
        {
            try
            {
                try
                {
                    _ediabas.ResolveSgbdFile(_jobEcuName);
                }
                catch (Exception)
                {
                    if (_abortJob)
                    {
                        _apiStateValue = APIBREAK;
                    }
                    else
                    {
                        if (_ediabas.ErrorCodeLast != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                        {
                            _apiStateValue = APIERROR;
                        }
                        else
                        {
                            setJobError(EDIABAS_SYS_0002);
                        }
                    }
                    return;
                }

                _ediabas.ExecuteJob(_jobName);
                _resultSets = _ediabas.ResultSets;
                _apiStateValue = APIREADY;
            }
            catch (Exception)
            {
                if (_abortJob)
                {
                    _apiStateValue = APIBREAK;
                }
                else
                {
                    if (_ediabas.ErrorCodeLast != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                    {
                        _apiStateValue = APIERROR;
                    }
                    else
                    {
                        setJobError(EDIABAS_SYS_0000);
                    }
                }
            }
            finally
            {
                _jobThread = null;
                _abortJob = false;
            }
        }

        private bool abortJobFunc()
        {
            return _abortJob;
        }

        public void logFormat(ApiLogLevel logLevel, string format, params object[] args)
        {
            updateLogLevel();
            if ((int)logLevel > _logLevelApi)
            {
                return;
            }

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == null)
                {
                    continue;
                }
                if (args[i] is string)
                {
                    args[i] = "'" + (string)args[i] + "'";
                }
                if (args[i].GetType() == typeof(byte[]))
                {
                    byte[] argArray = (byte[])args[i];
                    StringBuilder stringBuilder = new StringBuilder(argArray.Length);
                    foreach (byte arg in argArray)
                    {
                        stringBuilder.Append(string.Format(Culture, "{0:X02} ", arg));
                    }

                    args[i] = "[" + stringBuilder +"]";
                }
            }
            logString(logLevel, string.Format(Culture, format, args));
        }

        private void logString(ApiLogLevel logLevel, string info)
        {
            updateLogLevel();
            if ((int)logLevel > _logLevelApi)
            {
                return;
            }

            try
            {
                lock (_apiLogLock)
                {
                    if (_swLog == null)
                    {
                        string tracePath = _ediabas.GetConfigProperty("TracePath");
                        if (tracePath != null)
                        {
                            string traceBuffering = _ediabas.GetConfigProperty("TraceBuffering");
                            Int64 buffering = 0;
                            if (traceBuffering != null)
                            {
                                buffering = EdiabasNet.StringToValue(traceBuffering);
                            }

                            int appendTrace = 0;
                            string propAppend = _ediabas.GetConfigProperty("AppendTrace");
                            if (propAppend != null)
                            {
                                appendTrace = (int)EdiabasNet.StringToValue(propAppend);
                            }

                            string traceFileName = "api.trc";
                            string propName = _ediabas.GetConfigProperty("ApiTraceName");
                            if (!string.IsNullOrWhiteSpace(propName))
                            {
                                traceFileName = propName;
                            }

                            string traceFile = Path.Combine(tracePath, traceFileName);
                            try
                            {
                                if (appendTrace != 0 && File.Exists(traceFile))
                                {
                                    DateTime lastWriteTime = File.GetLastWriteTime(traceFile);
                                    TimeSpan diffTime = DateTime.Now - lastWriteTime;
                                    if (diffTime.Hours > EdiabasNet.TraceAppendDiffHours)
                                    {
                                        appendTrace = 0;
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                throw;
                            }

                            Directory.CreateDirectory(tracePath);
                            FileMode fileMode = FileMode.Append;
                            if (_firstLog && appendTrace == 0)
                            {
                                _firstLog = false;
                                fileMode = FileMode.Create;
                            }
                            _swLog = new StreamWriter(
                                new FileStream(traceFile, fileMode, FileAccess.Write, FileShare.ReadWrite), Encoding)
                                {
                                    AutoFlush = buffering == 0
                                };
                        }
                    }
                    if (_swLog != null)
                    {
                        _swLog.WriteLine(info);
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void closeLog()
        {
            lock (_apiLogLock)
            {
                if (_swLog != null)
                {
                    _swLog.Dispose();
                    _swLog = null;
                }
                _logLevelApi = -1;
            }
        }

        private void updateLogLevel()
        {
            if (_logLevelApi < 0)
            {
                lock (_apiLogLock)
                {
                    if (_ediabas != null)
                    {
                        string apiTrace = _ediabas.GetConfigProperty("ApiTrace");
                        _logLevelApi = Convert.ToInt32(apiTrace);
                    }
                }
            }
        }
    }
}
