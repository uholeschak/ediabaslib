// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
// ReSharper disable ArrangeAccessorOwnerBody
namespace Ediabas
{
    public class API
    {
        public class APIRESULTFIELD
        {
            private uint _apiResult;

            public uint value
            {
                get
                {
                    return _apiResult;
                }
                set
                {
                    _apiResult = value;
                }
            }

            public ApiInternal.APIRESULTFIELD ApiResultField { get; set; }

            public APIRESULTFIELD(uint v)
            {
                _apiResult = v;
            }

            public static implicit operator APIRESULTFIELD(uint v)
            {
                return new APIRESULTFIELD(v);
            }

            public static implicit operator uint(APIRESULTFIELD v)
            {
                return v._apiResult;
            }
        }

        private static readonly ApiInternal _apiInternal;

        public const int APICOMPATIBILITYVERSION = 0x0800;
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
        public const int APIFORMAT_LONGLONG = 9;
        public const int APIFORMAT_QWORD = 10;
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

        static API()
        {
            _apiInternal = new ApiInternal();
        }

        public static bool apiCheckVersion(int versionCompatibility, out string versionInfo)
        {
            return ApiInternal.apiCheckVersion(versionCompatibility, out versionInfo);
        }

        public static bool apiInit()
        {
            return _apiInternal.apiInit();
        }

        public static bool apiInitExt(string ifh, string unit, string app, string config)
        {
            return _apiInternal.apiInitExt(ifh, unit, app, config);
        }

        public static void apiEnd()
        {
            _apiInternal.apiEnd();
        }

        public static bool apiSwitchDevice(string unit, string app)
        {
            return _apiInternal.apiSwitchDevice(unit, app);
        }

        public static void apiJob(string ecu, string job, string para, string result)
        {
            _apiInternal.apiJob(ecu, job, para, result);
        }

        public static void apiJobData(string ecu, string job, byte[] para, int paralen, string result)
        {
            _apiInternal.apiJobData(ecu, job, para, paralen, result);
        }

        public static void apiJobExt(string ecu, string job, byte[] stdpara, int stdparalen, byte[] para, int paralen, string result, int reserved)
        {
            _apiInternal.apiJobExt(ecu, job, stdpara, stdparalen, para, paralen, result, reserved);
        }

        public static int apiJobInfo(out string infoText)
        {
            return _apiInternal.apiJobInfo(out infoText);
        }

        public static bool apiResultChar(out char buffer, string result, ushort rset)
        {
            return _apiInternal.apiResultChar(out buffer, result, rset);
        }

        public static bool apiResultByte(out byte buffer, string result, ushort rset)
        {
            return _apiInternal.apiResultByte(out buffer, result, rset);
        }

        public static bool apiResultInt(out short buffer, string result, ushort rset)
        {
            return _apiInternal.apiResultInt(out buffer, result, rset);
        }

        public static bool apiResultWord(out ushort buffer, string result, ushort rset)
        {
            return _apiInternal.apiResultWord(out buffer, result, rset);
        }

        public static bool apiResultLong(out int buffer, string result, ushort rset)
        {
            return _apiInternal.apiResultLong(out buffer, result, rset);
        }

        public static bool apiResultDWord(out uint buffer, string result, ushort rset)
        {
            return _apiInternal.apiResultDWord(out buffer, result, rset);
        }

        public static bool apiResultLongLong(out long buffer, string result, ushort rset)
        {
            return _apiInternal.apiResultLongLong(out buffer, result, rset);
        }

        public static bool apiResultQWord(out ulong buffer, string result, ushort rset)
        {
            return _apiInternal.apiResultQWord(out buffer, result, rset);
        }

        public static bool apiResultReal(out double buffer, string result, ushort rset)
        {
            return _apiInternal.apiResultReal(out buffer, result, rset);
        }

        public static bool apiResultText(out string buffer, string result, ushort rset, string format)
        {
            return _apiInternal.apiResultText(out buffer, result, rset, format);
        }

        public static bool apiResultText(out char[] buffer, string result, ushort rset, string format)
        {
            return _apiInternal.apiResultTextChar(out buffer, result, rset, format);
        }

        public static bool apiResultBinary(out byte[] buffer, out ushort bufferLen, string result, ushort rset)
        {
            return _apiInternal.apiResultBinary(out buffer, out bufferLen, result, rset);
        }

        public static bool apiResultBinaryExt(out byte[] buffer, out uint bufferLen, uint bufferSize, string result, ushort rset)
        {
            return _apiInternal.apiResultBinaryExt(out buffer, out bufferLen, bufferSize, result, rset);
        }

        public static bool apiResultFormat(out int buffer, string result, ushort rset)
        {
            return _apiInternal.apiResultFormat(out buffer, result, rset);
        }

        public static bool apiResultNumber(out ushort buffer, ushort rset)
        {
            return _apiInternal.apiResultNumber(out buffer, rset);
        }

        public static bool apiResultName(out string buffer, ushort index, ushort rset)
        {
            return _apiInternal.apiResultName(out buffer, index, rset);
        }

        public static bool apiResultSets(out ushort rsets)
        {
            return _apiInternal.apiResultSets(out rsets);
        }

        public static bool apiResultVar(out string var)
        {
            return _apiInternal.apiResultVar(out var);
        }

        public static APIRESULTFIELD apiResultsNew()
        {
            APIRESULTFIELD resultField = new APIRESULTFIELD(0)
            {
                ApiResultField = _apiInternal.apiResultsNew()
            };
            return resultField;
        }

        public static void apiResultsScope(APIRESULTFIELD resultField)
        {
            _apiInternal.apiResultsScope(resultField.ApiResultField);
        }

        public static void apiResultsDelete(APIRESULTFIELD resultField)
        {
            _apiInternal.apiResultsDelete(resultField.ApiResultField);
        }

        public static int apiState()
        {
            return _apiInternal.apiState();
        }

        public static int apiStateExt(int suspendTime)
        {
            return _apiInternal.apiStateExt(suspendTime);
        }

        public static void apiBreak()
        {
            _apiInternal.apiBreak();
        }

        public static int apiErrorCode()
        {
            return _apiInternal.apiErrorCode();
        }

        public static string apiErrorText()
        {
            return _apiInternal.apiErrorText();
        }

        public static bool apiSetConfig(string cfgName, string cfgValue)
        {
            return _apiInternal.apiSetConfig(cfgName, cfgValue);
        }

        public static bool apiGetConfig(string cfgName, out string cfgValue)
        {
            return _apiInternal.apiGetConfig(cfgName, out cfgValue);
        }

        public static void apiTrace(string msg)
        {
            _apiInternal.apiTrace(msg);
        }

        public static bool apiXSysSetConfig(string cfgName, string cfgValue)
        {
            return ApiInternal.apiXSysSetConfig(cfgName, cfgValue);
        }

        public static void closeServer()
        {
            ApiInternal.closeServer();
        }

        public static bool enableServer(bool onOff)
        {
            return ApiInternal.enableServer(onOff);
        }

        public static bool enableMultiThreading(bool onOff)
        {
            return ApiInternal.enableMultiThreading(onOff);
        }
    }
}
