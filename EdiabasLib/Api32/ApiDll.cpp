#include <Windows.h>
#include <msclr\marshal.h>
#include "Api.h"

using namespace System;
using namespace System::Threading;
using namespace System::Runtime::InteropServices;
using namespace msclr::interop;
using namespace Ediabas;

#define DLLEXPORT __declspec(dllexport)

ref class GlobalObjects
{
    public:
        static array<ApiInternal ^>^ handles = gcnew array<ApiInternal ^>(10);
        static Object ^ handleLock = gcnew Object();

        static int GetNewApiInstance()
        {
            try
            {
                Monitor::Enter( handleLock );
                for (int i = 0; i < handles->Length; i++)
                {
                    if (handles[i] == nullptr)
                    {
                        ApiInternal ^apiInternal = gcnew ApiInternal();
                        handles[i] = apiInternal;
                        return i + 1;
                    }
                }
            }
            finally
            {
                Monitor::Exit( handleLock );
            }
            return 0;
        }

        static void DeleteApiInstance(int handle)
        {
            try
            {
                Monitor::Enter( handleLock );
                int index = handle - 1;
                if (index >= 0 && index < handles->Length)
                {
                    handles[index] = nullptr;
                }
            }
            finally
            {
                Monitor::Exit( handleLock );
            }
        }

        static ApiInternal ^ GetApiInstance(int handle)
        {
            try
            {
                Monitor::Enter( handleLock );
                int index = handle - 1;
                if (index >= 0 && index < handles->Length)
                {
                    return handles[index];
                }
            }
            finally
            {
                Monitor::Exit( handleLock );
            }
            return nullptr;
        }
};

#ifdef __cplusplus
extern "C" {
#endif 

DLLEXPORT APIBOOL FAR PASCAL __apiCheckVersion(int versionCompatibility,char far *versionInfo)
{
#pragma comment(linker, "/EXPORT:_"__FUNCTION__"=_"__FUNCTION__"@8")
    String ^ verInfo;
    if (!ApiInternal::apiCheckVersion(versionCompatibility, verInfo))
    {
        return APIFALSE;
    }
    marshal_context context;
    strcpy_s(versionInfo, APIMAXRESULT, context.marshal_as<const char*>(verInfo));
    return APITRUE;
}

DLLEXPORT APIBOOL FAR PASCAL __apiInit(unsigned int far *handle)
{
#pragma comment(linker, "/EXPORT:_"__FUNCTION__"=_"__FUNCTION__"@4")
    *handle = 0;
    unsigned int index = GlobalObjects::GetNewApiInstance();
    if (index == 0)
    {
        return APIFALSE;
    }
    ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(index);
    if (apiInternal == nullptr)
    {
        return APIFALSE;
    }
    if (!apiInternal->apiInit())
    {
        return APIFALSE;
    }
    *handle = index;
    return APITRUE;
}

DLLEXPORT APIBOOL FAR PASCAL __apiInitExt(unsigned int far *handle,
                            const char far *device,
                            const char far *devConnection,
                            const char far *devApplication,
                            const char far *reserved)
{
#pragma comment(linker, "/EXPORT:_"__FUNCTION__"=_"__FUNCTION__"@20")
    *handle = 0;
    int index = GlobalObjects::GetNewApiInstance();
    if (index < 0)
    {
        return APIFALSE;
    }
    ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(index);
    if (apiInternal == nullptr)
    {
        return APIFALSE;
    }
    if (!apiInternal->apiInitExt(
        (device == NULL) ? nullptr : gcnew String(device),
        (devConnection == NULL) ? nullptr : gcnew String(devConnection),
        (devApplication == NULL) ? nullptr : gcnew String(devApplication),
        (reserved == NULL) ? nullptr : gcnew String(reserved)))
    {
        return APIFALSE;
    }
    *handle = index;
    return APITRUE;
}

DLLEXPORT void FAR PASCAL __apiEnd(unsigned int handle)
{
#pragma comment(linker, "/EXPORT:_"__FUNCTION__"=_"__FUNCTION__"@4")
    ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal != nullptr)
    {
        apiInternal->apiEnd();
        GlobalObjects::DeleteApiInstance(handle);
    }
}

DLLEXPORT APIBOOL FAR PASCAL __apiSwitchDevice(unsigned int handle,
                            const char far *deviceConnection,
                            const char far *deviceApplication)
{
#pragma comment(linker, "/EXPORT:_"__FUNCTION__"=_"__FUNCTION__"@12")
    ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return APIFALSE;
    }
    if (!apiInternal->apiSwitchDevice(
        (deviceConnection == NULL) ? nullptr : gcnew String(deviceConnection),
        (deviceApplication == NULL) ? nullptr : gcnew String(deviceApplication)))
    {
        return APIFALSE;
    }
    return APITRUE;
}

DLLEXPORT void FAR PASCAL __apiJob(unsigned int handle,
                            const char far *ecu,const char far *job,
                            const char far *para,const char far *result)
{
#pragma comment(linker, "/EXPORT:_"__FUNCTION__"=_"__FUNCTION__"@20")
    ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return;
    }
    apiInternal->apiJob(
        (ecu == NULL) ? nullptr : gcnew String(ecu),
        (job == NULL) ? nullptr : gcnew String(job),
        (para == NULL) ? nullptr : gcnew String(para),
        (result == NULL) ? nullptr : gcnew String(result));
}

DLLEXPORT void FAR PASCAL __apiJobData(unsigned int handle,
                            const char far *ecu,const char far *job,
                            const unsigned char far *parabuf,int paralen,
                            const char far *result)
{
#pragma comment(linker, "/EXPORT:_"__FUNCTION__"=_"__FUNCTION__"@24")
    ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return;
    }

    array<byte>^ paraBuffer = gcnew array<byte>(paralen);
    for (int i = 0; i < paralen; i++)
    {
        paraBuffer[i] = parabuf[i];
    }

    apiInternal->apiJobData(
        (ecu == NULL) ? nullptr : gcnew String(ecu),
        (job == NULL) ? nullptr : gcnew String(job),
        paraBuffer, paralen,
        (result == NULL) ? nullptr : gcnew String(result));
}

DLLEXPORT void FAR PASCAL __apiJobExt(unsigned int handle,
                            const char far *ecu,const char far *job,
                            const unsigned char far *stdparabuf,int stdparalen,
                            const unsigned char far *parabuf,int paralen,
                            const char far *result,long reserved)
{
#pragma comment(linker, "/EXPORT:_"__FUNCTION__"=_"__FUNCTION__"@36")
    ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return;
    }

    array<byte>^ stdParaBuffer = gcnew array<byte>(stdparalen);
    for (int i = 0; i < stdparalen; i++)
    {
        stdParaBuffer[i] = stdparabuf[i];
    }

    array<byte>^ paraBuffer = gcnew array<byte>(paralen);
    for (int i = 0; i < paralen; i++)
    {
        paraBuffer[i] = parabuf[i];
    }

    apiInternal->apiJobExt(
        (ecu == NULL) ? nullptr : gcnew String(ecu),
        (job == NULL) ? nullptr : gcnew String(job),
        stdParaBuffer, stdparalen,
        paraBuffer, paralen,
        (result == NULL) ? nullptr : gcnew String(result),
        reserved);
}

DLLEXPORT int FAR PASCAL __apiJobInfo(unsigned int handle,char far *infoText)
{
#pragma comment(linker, "/EXPORT:_"__FUNCTION__"=_"__FUNCTION__"@8")
    ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return 0;
    }
    String ^ text;
    int percent = apiInternal->apiJobInfo(text);

    marshal_context context;
    strcpy_s(infoText, APIMAXTEXT, context.marshal_as<const char*>(text));
    return percent;
}

DLLEXPORT APIBOOL FAR PASCAL __apiResultChar(unsigned int handle,
                            APICHAR far *buf,const char far *result,
                            APIWORD set)
{
#pragma comment(linker, "/EXPORT:_"__FUNCTION__"=_"__FUNCTION__"@16")
    ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return APIFALSE;
    }

    wchar_t buffer;
    if (!apiInternal->apiResultChar(
        buffer,
        (result == NULL) ? nullptr : gcnew String(result),
        set))
    {
        return APIFALSE;
    }
    *buf = (APICHAR) buffer;
    return APITRUE;
}

DLLEXPORT APIBOOL FAR PASCAL __apiResultByte(unsigned int handle,
                            APIBYTE far *buf,const char far *result,
                            APIWORD set)
{
#pragma comment(linker, "/EXPORT:_"__FUNCTION__"=_"__FUNCTION__"@16")
    ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return APIFALSE;
    }

    byte buffer;
    if (!apiInternal->apiResultByte(
        buffer,
        (result == NULL) ? nullptr : gcnew String(result),
        set))
    {
        return APIFALSE;
    }
    *buf = buffer;
    return APITRUE;
}

DLLEXPORT APIBOOL FAR PASCAL __apiResultInt(unsigned int handle,
                            APIINTEGER far *buf,const char far *result,
                            APIWORD set)
{
#pragma comment(linker, "/EXPORT:_"__FUNCTION__"=_"__FUNCTION__"@16")
    ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return APIFALSE;
    }

    short buffer;
    if (!apiInternal->apiResultInt(
        buffer,
        (result == NULL) ? nullptr : gcnew String(result),
        set))
    {
        return APIFALSE;
    }
    *buf = buffer;
    return APITRUE;
}

DLLEXPORT APIBOOL FAR PASCAL __apiResultWord(unsigned int handle,
                            APIWORD far *buf,const char far *result,
                            APIWORD set)
{
#pragma comment(linker, "/EXPORT:_"__FUNCTION__"=_"__FUNCTION__"@16")
    ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return APIFALSE;
    }

    unsigned short buffer;
    if (!apiInternal->apiResultWord(
        buffer,
        (result == NULL) ? nullptr : gcnew String(result),
        set))
    {
        return APIFALSE;
    }
    *buf = buffer;
    return APITRUE;
}

DLLEXPORT APIBOOL FAR PASCAL __apiResultLong(unsigned int handle,
                            APILONG far *buf,const char far *result,
                            APIWORD set)
{
#pragma comment(linker, "/EXPORT:_"__FUNCTION__"=_"__FUNCTION__"@16")
    ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return APIFALSE;
    }

    int buffer;
    if (!apiInternal->apiResultLong(
        buffer,
        (result == NULL) ? nullptr : gcnew String(result),
        set))
    {
        return APIFALSE;
    }
    *buf = buffer;
    return APITRUE;
}

DLLEXPORT APIBOOL FAR PASCAL __apiResultDWord(unsigned int handle,
                            APIDWORD far *buf,const char far *result,
                            APIWORD set)
{
#pragma comment(linker, "/EXPORT:_"__FUNCTION__"=_"__FUNCTION__"@16")
    ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return APIFALSE;
    }

    unsigned int buffer;
    if (!apiInternal->apiResultDWord(
        buffer,
        (result == NULL) ? nullptr : gcnew String(result),
        set))
    {
        return APIFALSE;
    }
    *buf = buffer;
    return APITRUE;
}

DLLEXPORT APIBOOL FAR PASCAL __apiResultReal(unsigned int handle,
                            APIREAL far *buf,const char far *result,
                            APIWORD set)
{
#pragma comment(linker, "/EXPORT:_"__FUNCTION__"=_"__FUNCTION__"@16")
    ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return APIFALSE;
    }

    double buffer;
    if (!apiInternal->apiResultReal(
        buffer,
        (result == NULL) ? nullptr : gcnew String(result),
        set))
    {
        return APIFALSE;
    }
    *buf = buffer;
    return APITRUE;
}

DLLEXPORT APIBOOL FAR PASCAL __apiResultText(unsigned int handle,
                            APITEXT far *buf,const char far *result,
                            APIWORD set,const char far *format)
{
#pragma comment(linker, "/EXPORT:_"__FUNCTION__"=_"__FUNCTION__"@20")
    ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return APIFALSE;
    }

    String ^ buffer;
    if (!apiInternal->apiResultText(
        buffer,
        (result == NULL) ? nullptr : gcnew String(result),
        set,
        (format == NULL) ? nullptr : gcnew String(format)))
    {
        return APIFALSE;
    }
    marshal_context context;
    strcpy_s(buf, APIMAXTEXT, context.marshal_as<const char*>(buffer));
    return APITRUE;
}

DLLEXPORT APIBOOL FAR PASCAL __apiResultBinary(unsigned int handle,
                            APIBINARY far *buf,APIWORD far *buflen,
                            const char far *result,APIWORD set)
{
#pragma comment(linker, "/EXPORT:_"__FUNCTION__"=_"__FUNCTION__"@20")
    ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return APIFALSE;
    }

    array<byte> ^ buffer;
    unsigned short len;
    if (!apiInternal->apiResultBinary(
        buffer, len,
        (result == NULL) ? nullptr : gcnew String(result),
        set))
    {
        return APIFALSE;
    }

    *buflen = len;
    pin_ptr<byte> p = &buffer[0];
    memcpy(buf, p, len);
    return APITRUE;
}

DLLEXPORT APIBOOL FAR PASCAL __apiResultBinaryExt(unsigned int handle,
                            APIBINARY far *buf,APIDWORD far *buflen,APIDWORD bufSize,
                            const char far *result,APIWORD set)
{
#pragma comment(linker, "/EXPORT:_"__FUNCTION__"=_"__FUNCTION__"@24")
    ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return APIFALSE;
    }

    array<byte> ^ buffer;
    unsigned int len;
    if (!apiInternal->apiResultBinaryExt(
        buffer, len,
        bufSize,
        (result == NULL) ? nullptr : gcnew String(result),
        set))
    {
        return APIFALSE;
    }

    *buflen = len;
    pin_ptr<byte> p = &buffer[0];
    memcpy(buf, p, len);
    return APITRUE;
}

DLLEXPORT APIBOOL FAR PASCAL __apiResultFormat(unsigned int handle,
                            APIRESULTFORMAT far *buf,const char far *result,
                            APIWORD set)
{
#pragma comment(linker, "/EXPORT:_"__FUNCTION__"=_"__FUNCTION__"@16")
    ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return APIFALSE;
    }

    int buffer;
    if (!apiInternal->apiResultFormat(
        buffer,
        (result == NULL) ? nullptr : gcnew String(result),
        set))
    {
        return APIFALSE;
    }
    *buf = (APIRESULTFORMAT) buffer;
    return APITRUE;
}

DLLEXPORT APIBOOL FAR PASCAL __apiResultNumber(unsigned int handle,
                            APIWORD far *buf,APIWORD set)
{
#pragma comment(linker, "/EXPORT:_"__FUNCTION__"=_"__FUNCTION__"@12")
    ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return APIFALSE;
    }

    unsigned short buffer;
    if (!apiInternal->apiResultNumber(
        buffer,
        set))
    {
        return APIFALSE;
    }
    *buf = buffer;
    return APITRUE;
}

DLLEXPORT APIBOOL FAR PASCAL __apiResultName(unsigned int handle,char far *buf,
                            APIWORD index,APIWORD set)
{
#pragma comment(linker, "/EXPORT:_"__FUNCTION__"=_"__FUNCTION__"@16")
    ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return APIFALSE;
    }

    String ^ buffer;
    if (!apiInternal->apiResultName(
        buffer,
        index,
        set))
    {
        return APIFALSE;
    }
    marshal_context context;
    strcpy_s(buf, APIMAXNAME, context.marshal_as<const char*>(buffer));
    return APITRUE;
}

DLLEXPORT APIBOOL FAR PASCAL __apiResultSets(unsigned int handle,APIWORD far *sets)
{
#pragma comment(linker, "/EXPORT:_"__FUNCTION__"=_"__FUNCTION__"@8")
    ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return APIFALSE;
    }

    unsigned short setBuffer;
    if (!apiInternal->apiResultSets(
        setBuffer))
    {
        return APIFALSE;
    }
    *sets = setBuffer;
    return APITRUE;
}

DLLEXPORT APIBOOL FAR PASCAL __apiResultVar(unsigned int handle,APITEXT far *ecu)
{
#pragma comment(linker, "/EXPORT:_"__FUNCTION__"=_"__FUNCTION__"@8")
    ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return APIFALSE;
    }

    String ^ ecuBuffer;
    if (!apiInternal->apiResultVar(
        ecuBuffer))
    {
        return APIFALSE;
    }
    marshal_context context;
    strcpy_s(ecu, APIMAXNAME, context.marshal_as<const char*>(ecuBuffer));
    return APITRUE;
}

DLLEXPORT APIRESULTFIELD FAR PASCAL __apiResultsNew(unsigned int handle)
{
#pragma comment(linker, "/EXPORT:_"__FUNCTION__"=_"__FUNCTION__"@4")
    ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return NULL;
    }

    ApiInternal::APIRESULTFIELD ^ apiResultField = apiInternal->apiResultsNew();
    GCHandle hResult = GCHandle::Alloc(apiResultField);
    IntPtr pointer = GCHandle::ToIntPtr(hResult);
    return pointer.ToPointer();
}

DLLEXPORT void FAR PASCAL __apiResultsScope(unsigned int handle,APIRESULTFIELD field)
{
#pragma comment(linker, "/EXPORT:_"__FUNCTION__"=_"__FUNCTION__"@8")
    ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return;
    }

    IntPtr pointer(field);
    GCHandle hResult = GCHandle::FromIntPtr(pointer);
    ApiInternal::APIRESULTFIELD ^ apiResultField = dynamic_cast<ApiInternal::APIRESULTFIELD ^>(hResult.Target);
    if (apiResultField != nullptr)
    {
        apiInternal->apiResultsScope(apiResultField);
    }
}

DLLEXPORT void FAR PASCAL __apiResultsDelete(unsigned int handle,APIRESULTFIELD field)
{
#pragma comment(linker, "/EXPORT:_"__FUNCTION__"=_"__FUNCTION__"@8")
    ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return;
    }

    IntPtr pointer(field);
    GCHandle hResult = GCHandle::FromIntPtr(pointer);
    ApiInternal::APIRESULTFIELD ^ apiResultField = dynamic_cast<ApiInternal::APIRESULTFIELD ^>(hResult.Target);
    if (apiResultField != nullptr)
    {
        apiInternal->apiResultsDelete(apiResultField);
        delete hResult;
    }
}

DLLEXPORT int FAR PASCAL __apiState(unsigned int handle)
{
#pragma comment(linker, "/EXPORT:_"__FUNCTION__"=_"__FUNCTION__"@4")
    ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return APIERROR;
    }
    return apiInternal->apiState();
}

DLLEXPORT int FAR PASCAL __apiStateExt(unsigned int handle,int suspendTime)
{
#pragma comment(linker, "/EXPORT:_"__FUNCTION__"=_"__FUNCTION__"@8")
    ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return APIERROR;
    }
    return apiInternal->apiStateExt(suspendTime);
}

DLLEXPORT void FAR PASCAL __apiBreak(unsigned int handle)
{
#pragma comment(linker, "/EXPORT:_"__FUNCTION__"=_"__FUNCTION__"@4")
    ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return;
    }
    apiInternal->apiBreak();
}

DLLEXPORT int FAR PASCAL __apiErrorCode(unsigned int handle)
{
#pragma comment(linker, "/EXPORT:_"__FUNCTION__"=_"__FUNCTION__"@4")
    ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return EDIABAS_API_0006;
    }
    return apiInternal->apiErrorCode();
}

DLLEXPORT void FAR PASCAL __apiErrorText(unsigned int handle,
                            char far *buf,int bufsize)
{
#pragma comment(linker, "/EXPORT:_"__FUNCTION__"=_"__FUNCTION__"@12")
    ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        buf[0] = 0;
        return;
    }
    String ^ buffer = apiInternal->apiErrorText();
    marshal_context context;
    strcpy_s(buf, bufsize, context.marshal_as<const char*>(buffer));
}

DLLEXPORT APIBOOL FAR PASCAL __apiSetConfig(unsigned int handle,
                            const char far *configName,
                            const char far *configValue)
{
#pragma comment(linker, "/EXPORT:_"__FUNCTION__"=_"__FUNCTION__"@12")
    ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return APIFALSE;
    }
    if (!apiInternal->apiSetConfig(
        (configName == NULL) ? nullptr : gcnew String(configName),
        (configValue == NULL) ? nullptr : gcnew String(configValue)))
    {
        return APIFALSE;
    }
    return APITRUE;
}

DLLEXPORT APIBOOL FAR PASCAL __apiGetConfig(unsigned int handle,
                            const char far *configName,
                            char far *configValue)
{
#pragma comment(linker, "/EXPORT:_"__FUNCTION__"=_"__FUNCTION__"@12")
    ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return APIFALSE;
    }
    String ^ buffer;
    if (!apiInternal->apiGetConfig(
        (configName == NULL) ? nullptr : gcnew String(configName),
        buffer))
    {
        return APIFALSE;
    }
    marshal_context context;
    strcpy_s(configValue, APIMAXTEXT, context.marshal_as<const char*>(buffer));
    return APITRUE;
}

DLLEXPORT void FAR PASCAL __apiTrace(unsigned int handle,const char far *msg)
{
#pragma comment(linker, "/EXPORT:_"__FUNCTION__"=_"__FUNCTION__"@8")
    ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return;
    }
    apiInternal->apiTrace(
        (msg == NULL) ? nullptr : gcnew String(msg));
}

DLLEXPORT APIBOOL FAR PASCAL __apiXSysSetConfig(const char far *cfgName, const char far *cfgValue)
{
#pragma comment(linker, "/EXPORT:_"__FUNCTION__"=_"__FUNCTION__"@8")
    if (!ApiInternal::apiXSysSetConfig(
        (cfgName == NULL) ? nullptr : gcnew String(cfgName),
        (cfgValue == NULL) ? nullptr : gcnew String(cfgValue)))
    {
        return APIFALSE;
    }
    return APITRUE;
}

DLLEXPORT void FAR PASCAL closeServer()
{
#pragma comment(linker, "/EXPORT:"__FUNCTION__"=_"__FUNCTION__"@0")
    ApiInternal::closeServer();
}

DLLEXPORT APIBOOL FAR PASCAL enableServer(APIBOOL onOff)
{
#pragma comment(linker, "/EXPORT:"__FUNCTION__"=_"__FUNCTION__"@4")
    if (!ApiInternal::enableServer(onOff ? true : false))
    {
        return APIFALSE;
    }
    return APITRUE;
}

DLLEXPORT APIBOOL FAR PASCAL enableMultiThreading(bool onOff)
{
#pragma comment(linker, "/EXPORT:"__FUNCTION__"=_"__FUNCTION__"@4")
    if (!ApiInternal::enableMultiThreading(onOff ? true : false))
    {
        return APIFALSE;
    }
    return APITRUE;
}

#ifdef __cplusplus
}
#endif 
