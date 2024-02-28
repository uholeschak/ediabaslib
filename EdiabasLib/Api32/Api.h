/**
***    @(#)  api.h - Header file of API/EDIABAS, Version 7.0  @(#)
***
*******************************************************************/


#ifndef _API_

#define _API_

#ifdef __cplusplus
extern "C" {
#endif 


/*--- define's ---*/

#define APICOMPATIBILITYVERSION 0x0700  /* API compatibility version */

#define APIBUSY                 0       /* API state's */
#define APIREADY                1
#define APIBREAK                2
#define APIERROR                3

#define APIMAXDEVICE            64      /* maximal device incl. '\0'
                                           (connection, application)       */
#define APIMAXNAME              64      /* maximal name length incl. '\0'
                                           (job, ecu, result)              */
#define APIMAXPARA              1024    /* maximal job para length */
#define APIMAXPARAEXT           65536L  /* maximal job para length / v7 */
#define APIMAXSTDPARA           256     /* maximal standard job para length */
#define APIMAXRESULT            256     /* maximal result length incl. '\0'*/

#define APIMAXTEXT              1024    /* maximal text length incl. '\0'  */
#define APIMAXBINARY            1024    /* maximal binary buffer length    */
#define APIMAXBINARYEXT         65536L  /* maximal binary buffer length / v7 */

#define APIMAXCONFIG            256     /* maximal config buffer length incl.'\0' */

#define APIMAXFILENAME          256     /* maximal file name length incl.'\0' */

/*--- API error's ---*/

#define EDIABAS_ERR_NONE        0       /* no error */
#define EDIABAS_RESERVED        1
#define EDIABAS_ERROR_CODE_OUT_OF_RANGE  2

#define EDIABAS_IFH_0000        10
#define EDIABAS_IFH_0001        11
#define EDIABAS_IFH_0002        12
#define EDIABAS_IFH_0003        13
#define EDIABAS_IFH_0004        14
#define EDIABAS_IFH_0005        15
#define EDIABAS_IFH_0006        16
#define EDIABAS_IFH_0007        17
#define EDIABAS_IFH_0008        18
#define EDIABAS_IFH_0009        19
#define EDIABAS_IFH_0010        20
#define EDIABAS_IFH_0011        21
#define EDIABAS_IFH_0012        22
#define EDIABAS_IFH_0013        23
#define EDIABAS_IFH_0014        24
#define EDIABAS_IFH_0015        25
#define EDIABAS_IFH_0016        26
#define EDIABAS_IFH_0017        27
#define EDIABAS_IFH_0018        28
#define EDIABAS_IFH_0019        29
#define EDIABAS_IFH_0020        30
#define EDIABAS_IFH_0021        31
#define EDIABAS_IFH_0022        32
#define EDIABAS_IFH_0023        33
#define EDIABAS_IFH_0024        34
#define EDIABAS_IFH_0025        35
#define EDIABAS_IFH_0026        36
#define EDIABAS_IFH_0027        37
#define EDIABAS_IFH_0028        38
#define EDIABAS_IFH_0029        39
#define EDIABAS_IFH_0030        40
#define EDIABAS_IFH_0031        41
#define EDIABAS_IFH_0032        42
#define EDIABAS_IFH_0033        43
#define EDIABAS_IFH_0034        44
#define EDIABAS_IFH_0035        45
#define EDIABAS_IFH_0036        46
#define EDIABAS_IFH_0037        47
#define EDIABAS_IFH_0038        48
#define EDIABAS_IFH_0039        49
#define EDIABAS_IFH_0040        50
#define EDIABAS_IFH_0041        51
#define EDIABAS_IFH_0042        52
#define EDIABAS_IFH_0043        53
#define EDIABAS_IFH_0044        54
#define EDIABAS_IFH_0045        55
#define EDIABAS_IFH_0046        56
#define EDIABAS_IFH_0047        57
#define EDIABAS_IFH_0048        58
#define EDIABAS_IFH_0049        59
#define EDIABAS_IFH_LAST        EDIABAS_IFH_0049

#define EDIABAS_BIP_0000        60
#define EDIABAS_BIP_0001        61
#define EDIABAS_BIP_0002        62
#define EDIABAS_BIP_0003        63
#define EDIABAS_BIP_0004        64
#define EDIABAS_BIP_0005        65
#define EDIABAS_BIP_0006        66
#define EDIABAS_BIP_0007        67
#define EDIABAS_BIP_0008        68
#define EDIABAS_BIP_0009        69
#define EDIABAS_BIP_0010        70
#define EDIABAS_BIP_0011        71
#define EDIABAS_BIP_0012        72
#define EDIABAS_BIP_0013        73
#define EDIABAS_BIP_0014        74
#define EDIABAS_BIP_0015        75
#define EDIABAS_BIP_0016        76
#define EDIABAS_BIP_0017        77
#define EDIABAS_BIP_0018        78
#define EDIABAS_BIP_0019        79
#define EDIABAS_BIP_0020        80
#define EDIABAS_BIP_0021        81
#define EDIABAS_BIP_0022        82
#define EDIABAS_BIP_0023        83
#define EDIABAS_BIP_0024        84
#define EDIABAS_BIP_0025        85
#define EDIABAS_BIP_0026        86
#define EDIABAS_BIP_0027        87
#define EDIABAS_BIP_0028        88
#define EDIABAS_BIP_0029        89
#define EDIABAS_BIP_LAST        EDIABAS_BIP_0029

#define EDIABAS_SYS_0000        90
#define EDIABAS_SYS_0001        91
#define EDIABAS_SYS_0002        92
#define EDIABAS_SYS_0003        93
#define EDIABAS_SYS_0004        94
#define EDIABAS_SYS_0005        95
#define EDIABAS_SYS_0006        96
#define EDIABAS_SYS_0007        97
#define EDIABAS_SYS_0008        98
#define EDIABAS_SYS_0009        99
#define EDIABAS_SYS_0010        100
#define EDIABAS_SYS_0011        101
#define EDIABAS_SYS_0012        102
#define EDIABAS_SYS_0013        103
#define EDIABAS_SYS_0014        104
#define EDIABAS_SYS_0015        105
#define EDIABAS_SYS_0016        106
#define EDIABAS_SYS_0017        107
#define EDIABAS_SYS_0018        108
#define EDIABAS_SYS_0019        109
#define EDIABAS_SYS_0020        110
#define EDIABAS_SYS_0021        111
#define EDIABAS_SYS_0022        112
#define EDIABAS_SYS_0023        113
#define EDIABAS_SYS_0024        114
#define EDIABAS_SYS_0025        115
#define EDIABAS_SYS_0026        116
#define EDIABAS_SYS_0027        117
#define EDIABAS_SYS_0028        118
#define EDIABAS_SYS_0029        119
#define EDIABAS_SYS_LAST        EDIABAS_SYS_0029

#define EDIABAS_API_0000        120
#define EDIABAS_API_0001        121
#define EDIABAS_API_0002        122
#define EDIABAS_API_0003        123
#define EDIABAS_API_0004        124
#define EDIABAS_API_0005        125
#define EDIABAS_API_0006        126
#define EDIABAS_API_0007        127
#define EDIABAS_API_0008        128
#define EDIABAS_API_0009        129
#define EDIABAS_API_0010        130
#define EDIABAS_API_0011        131
#define EDIABAS_API_0012        132
#define EDIABAS_API_0013        133
#define EDIABAS_API_0014        134
#define EDIABAS_API_0015        135
#define EDIABAS_API_0016        136
#define EDIABAS_API_0017        137
#define EDIABAS_API_0018        138
#define EDIABAS_API_0019        139
#define EDIABAS_API_0020        140
#define EDIABAS_API_0021        141
#define EDIABAS_API_0022        142
#define EDIABAS_API_0023        143
#define EDIABAS_API_0024        144
#define EDIABAS_API_0025        145
#define EDIABAS_API_0026        146
#define EDIABAS_API_0027        147
#define EDIABAS_API_0028        148
#define EDIABAS_API_0029        149
#define EDIABAS_API_LAST        EDIABAS_API_0029

#define EDIABAS_NET_0000        150
#define EDIABAS_NET_0001        151
#define EDIABAS_NET_0002        152
#define EDIABAS_NET_0003        153
#define EDIABAS_NET_0004        154
#define EDIABAS_NET_0005        155
#define EDIABAS_NET_0006        156
#define EDIABAS_NET_0007        157
#define EDIABAS_NET_0008        158
#define EDIABAS_NET_0009        159
#define EDIABAS_NET_0010        160
#define EDIABAS_NET_0011        161
#define EDIABAS_NET_0012        162
#define EDIABAS_NET_0013        163
#define EDIABAS_NET_0014        164
#define EDIABAS_NET_0015        165
#define EDIABAS_NET_0016        166
#define EDIABAS_NET_0017        167
#define EDIABAS_NET_0018        168
#define EDIABAS_NET_0019        169
#define EDIABAS_NET_0020        170
#define EDIABAS_NET_0021        171
#define EDIABAS_NET_0022        172
#define EDIABAS_NET_0023        173
#define EDIABAS_NET_0024        174
#define EDIABAS_NET_0025        175
#define EDIABAS_NET_0026        176
#define EDIABAS_NET_0027        177
#define EDIABAS_NET_0028        178
#define EDIABAS_NET_0029        179
#define EDIABAS_NET_0030        180
#define EDIABAS_NET_0031        181
#define EDIABAS_NET_0032        182
#define EDIABAS_NET_0033        183
#define EDIABAS_NET_0034        184
#define EDIABAS_NET_0035        185
#define EDIABAS_NET_0036        186
#define EDIABAS_NET_0037        187
#define EDIABAS_NET_0038        188
#define EDIABAS_NET_0039        189
#define EDIABAS_NET_0040        190
#define EDIABAS_NET_0041        191
#define EDIABAS_NET_0042        192
#define EDIABAS_NET_0043        193
#define EDIABAS_NET_0044        194
#define EDIABAS_NET_0045        195
#define EDIABAS_NET_0046        196
#define EDIABAS_NET_0047        197
#define EDIABAS_NET_0048        198
#define EDIABAS_NET_0049        199
#define EDIABAS_NET_LAST        EDIABAS_NET_0049

#define EDIABAS_IFH_0050        200
#define EDIABAS_IFH_0051        201
#define EDIABAS_IFH_0052        202
#define EDIABAS_IFH_0053        203
#define EDIABAS_IFH_0054        204
#define EDIABAS_IFH_0055        205
#define EDIABAS_IFH_0056        206
#define EDIABAS_IFH_0057        207
#define EDIABAS_IFH_0058        208
#define EDIABAS_IFH_0059        209
#define EDIABAS_IFH_0060        210
#define EDIABAS_IFH_0061        211
#define EDIABAS_IFH_0062        212
#define EDIABAS_IFH_0063        213
#define EDIABAS_IFH_0064        214
#define EDIABAS_IFH_0065        215
#define EDIABAS_IFH_0066        216
#define EDIABAS_IFH_0067        217
#define EDIABAS_IFH_0068        218
#define EDIABAS_IFH_0069        219
#define EDIABAS_IFH_0070        220
#define EDIABAS_IFH_0071        221
#define EDIABAS_IFH_0072        222
#define EDIABAS_IFH_0073        223
#define EDIABAS_IFH_0074        224
#define EDIABAS_IFH_0075        225
#define EDIABAS_IFH_0076        226
#define EDIABAS_IFH_0077        227
#define EDIABAS_IFH_0078        228
#define EDIABAS_IFH_0079        229
#define EDIABAS_IFH_0080        230
#define EDIABAS_IFH_0081        231
#define EDIABAS_IFH_0082        232
#define EDIABAS_IFH_0083        233
#define EDIABAS_IFH_0084        234
#define EDIABAS_IFH_0085        235
#define EDIABAS_IFH_0086        236
#define EDIABAS_IFH_0087        237
#define EDIABAS_IFH_0088        238
#define EDIABAS_IFH_0089        239
#define EDIABAS_IFH_0090        240
#define EDIABAS_IFH_0091        241
#define EDIABAS_IFH_0092        242
#define EDIABAS_IFH_0093        243
#define EDIABAS_IFH_0094        244
#define EDIABAS_IFH_0095        245
#define EDIABAS_IFH_0096        246
#define EDIABAS_IFH_0097        247
#define EDIABAS_IFH_0098        248
#define EDIABAS_IFH_0099        249
#define EDIABAS_IFH_LAST2       EDIABAS_IFH_0099

#define EDIABAS_RUN_0000        250
#define EDIABAS_RUN_0001        251
#define EDIABAS_RUN_0002        252
#define EDIABAS_RUN_0003        253
#define EDIABAS_RUN_0004        254
#define EDIABAS_RUN_0005        255
#define EDIABAS_RUN_0006        256
#define EDIABAS_RUN_0007        257
#define EDIABAS_RUN_0008        258
#define EDIABAS_RUN_0009        259
#define EDIABAS_RUN_0010        260
#define EDIABAS_RUN_0011        261
#define EDIABAS_RUN_0012        262
#define EDIABAS_RUN_0013        263
#define EDIABAS_RUN_0014        264
#define EDIABAS_RUN_0015        265
#define EDIABAS_RUN_0016        266
#define EDIABAS_RUN_0017        267
#define EDIABAS_RUN_0018        268
#define EDIABAS_RUN_0019        269
#define EDIABAS_RUN_0020        270
#define EDIABAS_RUN_0021        271
#define EDIABAS_RUN_0022        272
#define EDIABAS_RUN_0023        273
#define EDIABAS_RUN_0024        274
#define EDIABAS_RUN_0025        275
#define EDIABAS_RUN_0026        276
#define EDIABAS_RUN_0027        277
#define EDIABAS_RUN_0028        278
#define EDIABAS_RUN_0029        279
#define EDIABAS_RUN_0030        280
#define EDIABAS_RUN_0031        281
#define EDIABAS_RUN_0032        282
#define EDIABAS_RUN_0033        283
#define EDIABAS_RUN_0034        284
#define EDIABAS_RUN_0035        285
#define EDIABAS_RUN_0036        286
#define EDIABAS_RUN_0037        287
#define EDIABAS_RUN_0038        288
#define EDIABAS_RUN_0039        289
#define EDIABAS_RUN_0040        290
#define EDIABAS_RUN_0041        291
#define EDIABAS_RUN_0042        292
#define EDIABAS_RUN_0043        293
#define EDIABAS_RUN_0044        294
#define EDIABAS_RUN_0045        295
#define EDIABAS_RUN_0046        296
#define EDIABAS_RUN_0047        297
#define EDIABAS_RUN_0048        298
#define EDIABAS_RUN_0049        299
#define EDIABAS_RUN_0050        300
#define EDIABAS_RUN_0051        301
#define EDIABAS_RUN_0052        302
#define EDIABAS_RUN_0053        303
#define EDIABAS_RUN_0054        304
#define EDIABAS_RUN_0055        305
#define EDIABAS_RUN_0056        306
#define EDIABAS_RUN_0057        307
#define EDIABAS_RUN_0058        308
#define EDIABAS_RUN_0059        309
#define EDIABAS_RUN_0060        310
#define EDIABAS_RUN_0061        311
#define EDIABAS_RUN_0062        312
#define EDIABAS_RUN_0063        313
#define EDIABAS_RUN_0064        314
#define EDIABAS_RUN_0065        315
#define EDIABAS_RUN_0066        316
#define EDIABAS_RUN_0067        317
#define EDIABAS_RUN_0068        318
#define EDIABAS_RUN_0069        319
#define EDIABAS_RUN_0070        320
#define EDIABAS_RUN_0071        321
#define EDIABAS_RUN_0072        322
#define EDIABAS_RUN_0073        323
#define EDIABAS_RUN_0074        324
#define EDIABAS_RUN_0075        325
#define EDIABAS_RUN_0076        326
#define EDIABAS_RUN_0077        327
#define EDIABAS_RUN_0078        328
#define EDIABAS_RUN_0079        329
#define EDIABAS_RUN_0080        330
#define EDIABAS_RUN_0081        331
#define EDIABAS_RUN_0082        332
#define EDIABAS_RUN_0083        333
#define EDIABAS_RUN_0084        334
#define EDIABAS_RUN_0085        335
#define EDIABAS_RUN_0086        336
#define EDIABAS_RUN_0087        337
#define EDIABAS_RUN_0088        338
#define EDIABAS_RUN_0089        339
#define EDIABAS_RUN_0090        340
#define EDIABAS_RUN_0091        341
#define EDIABAS_RUN_0092        342
#define EDIABAS_RUN_0093        343
#define EDIABAS_RUN_0094        344
#define EDIABAS_RUN_0095        345
#define EDIABAS_RUN_0096        346
#define EDIABAS_RUN_0097        347
#define EDIABAS_RUN_0098        348
#define EDIABAS_RUN_0099        349
#define EDIABAS_RUN_LAST        EDIABAS_RUN_0099

#define EDIABAS_ERROR_LAST      EDIABAS_RUN_LAST

/*--- typedef's and enum's ---*/

typedef enum { APIFALSE,APITRUE } APIBOOL;
               
typedef char APICHAR;                   /* 8 bit            */
typedef unsigned char APIBYTE;          /* 8 bit, unsigned  */
typedef short APIINTEGER;               /* 16 bit           */
typedef unsigned short APIWORD;         /* 16 bit, unsigned */
typedef long APILONG;                   /* 32 bit           */
typedef unsigned long APIDWORD;         /* 32 bit, unsigned */
typedef int64_t APILONGLONG;            /* 64 bit           */
typedef uint64_t APIQWORD;              /* 64 bit, unsigned */
typedef APICHAR APITEXT;                /* 8 bit            */
typedef APIBYTE APIBINARY;              /* 8 bit, unsigned  */
typedef double APIREAL;

typedef enum { APIFORMAT_CHAR,          /* CHAR format      */
               APIFORMAT_BYTE,          /* BYTE format      */
               APIFORMAT_INTEGER,       /* INTEGER format   */
               APIFORMAT_WORD,          /* WORD format      */
               APIFORMAT_LONG,          /* LONG format      */
               APIFORMAT_DWORD,         /* DWORD format     */
               APIFORMAT_TEXT,          /* TEXT format      */
               APIFORMAT_BINARY,        /* BINARY format    */
               APIFORMAT_REAL           /* REAL format      */
             } APIRESULTFORMAT;

typedef void *APIRESULTFIELD;           /* results handle   */

/*--- prototypes ---*/

void apiBreak(void);
void apiCallBack(APIBOOL (*)(void));
void apiEnd(void);
int apiErrorCode(void);
void apiErrorHandler(void (*)(void));
const char *apiErrorText(void);
APIBOOL apiGetConfig(const char *,char *);
APIBOOL apiInit(void);
APIBOOL apiInitExt(const char *,const char *,const char *,const char *);
void apiJob(const char *,const char *,const char *,const char *);
void apiJobData(const char *,const char *,const unsigned char *,int,const char *);
void apiJobExt(const char *,const char *,const unsigned char *,int,
               const unsigned char *,int,const char *,long);
int apiJobInfo(char *);
APIBOOL apiResultBinary(APIBINARY *,APIWORD *,const char *,APIWORD);
APIBOOL apiResultByte(APIBYTE *,const char *,APIWORD);
APIBOOL apiResultChar(APICHAR *,const char *,APIWORD);
APIBOOL apiResultDWord(APIDWORD *,const char *,APIWORD);
APIBOOL apiResultQWord(APIQWORD*, const char*, APIWORD);
APIBOOL apiResultFormat(APIRESULTFORMAT *,const char *,APIWORD);
APIBOOL apiResultInt(APIINTEGER *,const char *,APIWORD);
APIBOOL apiResultLong(APILONG *,const char *,APIWORD);
APIBOOL apiResultLongLong(APILONGLONG*, const char*, APIWORD);
APIBOOL apiResultName(char *,APIWORD,APIWORD);
APIBOOL apiResultNumber(APIWORD *,APIWORD);
APIBOOL apiResultReal(APIREAL *,const char *,APIWORD);
void apiResultsDelete(APIRESULTFIELD);
APIBOOL apiResultSets(APIWORD *);
APIRESULTFIELD apiResultsNew(void);
void apiResultsScope(APIRESULTFIELD);
APIBOOL apiResultText(APITEXT *,const char *,APIWORD,const char *);
APIBOOL apiResultVar(APITEXT *);
APIBOOL apiResultWord(APIWORD *,const char *,APIWORD);
APIBOOL apiSetConfig(const char *,const char *);
int apiState(void);
APIBOOL apiSwitchDevice(const char *,const char *);

/*--- additional prototypes / v7 ---*/

APIBOOL apiCheckVersion(int,char *);
APIBOOL apiResultBinaryExt(APIBINARY *,APIDWORD *,APIDWORD,const char *,APIWORD);
int apiStateExt(int);
void apiTrace(const char *);


#ifdef __cplusplus
}
#endif 


#endif
