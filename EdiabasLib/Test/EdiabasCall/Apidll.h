/**
***    apidll.h - DLL header file of EDIABAS for Windows
***
*************************************************************************/


#ifndef _APIDLL_

#define _APIDLL_

#ifdef __cplusplus
extern "C" {
#endif 


#include <windows.h>
#include "api.h"


/* DLL function prototypes */

APIBOOL FAR PASCAL __apiCheckVersion(int versionCompatibility,char far *versionInfo);
APIBOOL FAR PASCAL __apiInit(unsigned int far *handle);
APIBOOL FAR PASCAL __apiInitExt(unsigned int far *handle,
                            const char far *device,
                            const char far *devConnection,
                            const char far *devApplication,
                            const char far *reserved);
void FAR PASCAL __apiEnd(unsigned int handle);
APIBOOL FAR PASCAL __apiSwitchDevice(unsigned int handle,
                            const char far *deviceConnection,
                            const char far *deviceApplication);
void FAR PASCAL __apiJob(unsigned int handle,
                            const char far *ecu,const char far *job,
                            const char far *para,const char far *result);
void FAR PASCAL __apiJobData(unsigned int handle,
                            const char far *ecu,const char far *job,
                            const unsigned char far *parabuf,int paralen,
                            const char far *result);
void FAR PASCAL __apiJobExt(unsigned int handle,
                            const char far *ecu,const char far *job,
                            const unsigned char far *stdparabuf,int stdparalen,
                            const unsigned char far *parabuf,int paralen,
                            const char far *result,long reserved);
int FAR PASCAL __apiJobInfo(unsigned int handle,char far *infoText);
APIBOOL FAR PASCAL __apiResultChar(unsigned int handle,
                            APICHAR far *buf,const char far *result,
                            APIWORD set);
APIBOOL FAR PASCAL __apiResultByte(unsigned int handle,
                            APIBYTE far *buf,const char far *result,
                            APIWORD set);
APIBOOL FAR PASCAL __apiResultInt(unsigned int handle,
                            APIINTEGER far *buf,const char far *result,
                            APIWORD set);
APIBOOL FAR PASCAL __apiResultWord(unsigned int handle,
                            APIWORD far *buf,const char far *result,
                            APIWORD set);
APIBOOL FAR PASCAL __apiResultLong(unsigned int handle,
                            APILONG far *buf,const char far *result,
                            APIWORD set);
APIBOOL FAR PASCAL __apiResultDWord(unsigned int handle,
                            APIDWORD far *buf,const char far *result,
                            APIWORD set);
APIBOOL FAR PASCAL __apiResultReal(unsigned int handle,
                            APIREAL far *buf,const char far *result,
                            APIWORD set);
APIBOOL FAR PASCAL __apiResultText(unsigned int handle,
                            APITEXT far *buf,const char far *result,
                            APIWORD set,const char far *format);
APIBOOL FAR PASCAL __apiResultBinary(unsigned int handle,
                            APIBINARY far *buf,APIWORD far *buflen,
                            const char far *result,APIWORD set);
APIBOOL FAR PASCAL __apiResultBinaryExt(unsigned int handle,
                            APIBINARY far *buf,APIDWORD far *buflen,APIDWORD bufSize,
                            const char far *result,APIWORD set);
APIBOOL FAR PASCAL __apiResultFormat(unsigned int handle,
                            APIRESULTFORMAT far *buf,const char far *result,
                            APIWORD set);
APIBOOL FAR PASCAL __apiResultNumber(unsigned int handle,
                            APIWORD far *buf,APIWORD set);
APIBOOL FAR PASCAL __apiResultName(unsigned int handle,char far *buf,
                            APIWORD index,APIWORD set);
APIBOOL FAR PASCAL __apiResultSets(unsigned int handle,APIWORD far *sets);
APIBOOL FAR PASCAL __apiResultVar(unsigned int handle,APITEXT far *ecu);
APIRESULTFIELD FAR PASCAL __apiResultsNew(unsigned int handle);
void FAR PASCAL __apiResultsScope(unsigned int handle,APIRESULTFIELD field);
void FAR PASCAL __apiResultsDelete(unsigned int handle,APIRESULTFIELD field);
int FAR PASCAL __apiState(unsigned int handle);
int FAR PASCAL __apiStateExt(unsigned int handle,int suspendTime);
void FAR PASCAL __apiBreak(unsigned int handle);
int FAR PASCAL __apiErrorCode(unsigned int handle);
void FAR PASCAL __apiErrorText(unsigned int handle,
                            char far *buf,int bufsize); /* "" -> no error */
APIBOOL FAR PASCAL __apiSetConfig(unsigned int handle,
                            const char far *configName,
                            const char far *configValue);
APIBOOL FAR PASCAL __apiGetConfig(unsigned int handle,
                            const char far *configName,
                            char far *configValue);
void FAR PASCAL __apiTrace(unsigned int handle,const char far *msg);


#ifdef __cplusplus
}
#endif 


#endif
