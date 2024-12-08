#include <Windows.h>
#include <msclr\marshal.h>
#include "Api.h"

using namespace System;
using namespace System::Threading;
using namespace System::Collections::Generic;
using namespace System::Runtime::InteropServices;
using namespace msclr::interop;

#define DLLEXPORT __declspec(dllexport)

ref class GlobalInit
{
private:
    static List<Reflection::Assembly^> _resourceAssemblies = gcnew List<Reflection::Assembly^>();

public:

    static GlobalInit()
    {
        LoadAllResourceAssemblies();

        AppDomain::CurrentDomain->AssemblyResolve += gcnew ResolveEventHandler(&GlobalInit::OnAssemblyResolve);
    }

    static Reflection::Assembly^ OnAssemblyResolve(Object^ Sender, ResolveEventArgs^ args)
    {
        String^ fullName = args->Name;
        if (!String::IsNullOrEmpty(fullName))
        {
            if (_resourceAssemblies.Count > 0)
            {
                for each (Reflection::Assembly ^ resourceAssembly in _resourceAssemblies)
                {
                    try
                    {
                        if (String::Compare(resourceAssembly->FullName, fullName, StringComparison::OrdinalIgnoreCase) == 0)
                        {
                            return resourceAssembly;
                        }
                    }
                    catch (...)
                    {
                    }
                }

                return nullptr;
            }

            try
            {
                array<String^>^ names = fullName->Split(',');
                if (names->Length < 1)
                {
                    return nullptr;
                }

                String^ assemblyName = names[0];
                String^ assemblyDllName = assemblyName + ".dll";
                String^ assemblyDir = GetAssemblyDirectory();
                if (String::IsNullOrEmpty(assemblyDir))
                {
                    return nullptr;
                }

                String^ assemblyFileName = IO::Path::Combine(assemblyDir, assemblyDllName);
                if (!IO::File::Exists(assemblyFileName))
                {
                    return nullptr;
                }

                return Reflection::Assembly::LoadFrom(assemblyFileName);
            }
            catch (...)
            {
                return nullptr;
            }
        }

        return nullptr;
    }

    static String^ GetAssemblyDirectory()
    {
        String^ codeBase = Reflection::Assembly::GetExecutingAssembly()->CodeBase;
        UriBuilder^ uri = gcnew UriBuilder(codeBase);
        String^ path = Uri::UnescapeDataString(uri->Path);
        return IO::Path::GetDirectoryName(path);
    }

    static bool LoadAllResourceAssemblies()
    {
        if (_resourceAssemblies.Count > 0)
        {
            return true;
        }

        try
        {
            Reflection::Assembly^ assembly = Reflection::Assembly::GetExecutingAssembly();
            array<String^>^ resourceNames = assembly->GetManifestResourceNames();

            for each (String ^ resourceName in resourceNames)
            {
                if (!resourceName->EndsWith(".dll", StringComparison::OrdinalIgnoreCase))
                {
                    continue;
                }

                IO::Stream^ stream = nullptr;
                IO::MemoryStream^ memoryStream = nullptr;
                try
                {
                    stream = assembly->GetManifestResourceStream(resourceName);
                    if (stream != nullptr)
                    {
                        memoryStream = gcnew IO::MemoryStream();
                        stream->CopyTo(memoryStream);
                        Reflection::Assembly^ loadedAssembly = Reflection::Assembly::Load(memoryStream->ToArray());
                        if (loadedAssembly != nullptr)
                        {
                            _resourceAssemblies.Add(loadedAssembly);
                        }
                    }
                }
                finally
                {
                    if (memoryStream != nullptr)
                    {
                        delete memoryStream;
                    }

                    if (stream != nullptr)
                    {
                        delete stream;
                    }
                }
            }
        }
        catch (...)
        {
            return false;
        }

        return _resourceAssemblies.Count > 0;
    }
};

ref class GlobalObjects
{
    public:
        static List<Ediabas::ApiInternal ^>^ handles = gcnew List<Ediabas::ApiInternal ^>();
        static Object ^ handleLock = gcnew Object();

        static GlobalObjects()
        {
            _onexit_m(OnExit);
        }

        static int OnExit()
        {
            try
            {
                Monitor::Enter(handleLock);
                for (int i = 0; i < handles->Count; i++)
                {
                    Ediabas::ApiInternal ^ apiInternal = handles[i];
                    if (apiInternal != nullptr)
                    {
                        handles[i]->apiEnd();
                        handles[i] = nullptr;
                    }
                }
            }
            finally
            {
                Monitor::Exit(handleLock);
            }

            Ediabas::ApiInternal::InterfaceDisconnect();
            return 0;
        }

        static int GetNewApiInstance()
        {
            try
            {
                Monitor::Enter( handleLock );
                Ediabas::ApiInternal ^ apiInternal = gcnew Ediabas::ApiInternal();
                for (int i = 0; i < handles->Count; i++)
                {
                    if (handles[i] == nullptr)
                    {
                        handles[i] = apiInternal;
                        return i + 1;
                    }
                }
                // no free handle found, create new one
                handles->Add(apiInternal);
                return handles->Count;
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
                if (index >= 0 && index < handles->Count)
                {
                    handles[index] = nullptr;
                }
            }
            finally
            {
                Monitor::Exit( handleLock );
            }
        }

        static Ediabas::ApiInternal ^ GetApiInstance(int handle)
        {
            try
            {
                Monitor::Enter( handleLock );
                int index = handle - 1;
                if (index >= 0 && index < handles->Count)
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

static String ^ ConvertCString(const char far *string)
{
    if (string == NULL)
    {
        return nullptr;
    }
    long length = (long) strlen(string);
    if (length <= 0)
    {
        return String::Empty;
    }

    array<byte>^ bytes = gcnew array<byte>(length);
    pin_ptr<byte> p = &bytes[0];
    memcpy(p, string, length);
    return Ediabas::ApiInternal::Encoding->GetString(bytes);
}

static array<byte> ^ ConvertNetString(String ^string, char far *buf, int bufsize)
{
    if (bufsize <= 0)
    {
        return nullptr;
    }

    if (string == nullptr)
    {
        buf[0] = 0;
        return nullptr;
    }

    array<byte> ^ bytesEncode = Ediabas::ApiInternal::Encoding->GetBytes(string);
    int length = min(bytesEncode->Length, bufsize - 1);
    array<byte>^ bytesResult = gcnew array<byte>(length + 1);
    for (size_t i = 0; i < length; i++)
    {
        bytesResult[i] = bytesEncode[i];
    }
    bytesResult[length] = 0;

    pin_ptr<byte> p = &bytesResult[0];
    memcpy_s(buf, bufsize, p, bytesResult->Length);

    return bytesResult;
}

#ifdef __cplusplus
extern "C" {
#endif 

__declspec(noinline)
static APIBOOL ApiCheckVersion(int versionCompatibility, char far* versionInfo)
{
    String^ verInfo;
    if (!Ediabas::ApiInternal::apiCheckVersion(versionCompatibility, verInfo))
    {
        return APIFALSE;
    }
    ConvertNetString(verInfo, versionInfo, APIMAXRESULT);
    return APITRUE;
}

DLLEXPORT APIBOOL FAR PASCAL __apiCheckVersion(int versionCompatibility,char far *versionInfo)
{
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:___apiCheckVersion=___apiCheckVersion@8")
#endif
    GlobalInit();
    return ApiCheckVersion(versionCompatibility, versionInfo);
}

__declspec(noinline)
static APIBOOL ApiInit(unsigned int far* handle)
{
    *handle = 0;
    const int index = GlobalObjects::GetNewApiInstance();
    if (index == 0)
    {
        return APIFALSE;
    }
    Ediabas::ApiInternal^ apiInternal = GlobalObjects::GetApiInstance(index);
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

DLLEXPORT APIBOOL FAR PASCAL __apiInit(unsigned int far *handle)
{
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:___apiInit=___apiInit@4")
#endif
    GlobalInit();
    return ApiInit(handle);
}

__declspec(noinline)
static APIBOOL ApiInitExt(unsigned int far* handle,
    const char far* device,
    const char far* devConnection,
    const char far* devApplication,
    const char far* reserved)
{
    *handle = 0;
    const int index = GlobalObjects::GetNewApiInstance();
    if (index < 0)
    {
        return APIFALSE;
    }
    Ediabas::ApiInternal^ apiInternal = GlobalObjects::GetApiInstance(index);
    if (apiInternal == nullptr)
    {
        return APIFALSE;
    }
    if (!apiInternal->apiInitExt(
        ConvertCString(device),
        ConvertCString(devConnection),
        ConvertCString(devApplication),
        ConvertCString(reserved)))
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
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:___apiInitExt=___apiInitExt@20")
#endif
    GlobalInit();
    return ApiInitExt(handle, device, devConnection, devApplication, reserved);
}

__declspec(noinline)
static  void ApiEnd(unsigned int handle)
{
    Ediabas::ApiInternal^ apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal != nullptr)
    {
        apiInternal->apiEnd();
        GlobalObjects::DeleteApiInstance(handle);
    }
}

DLLEXPORT void FAR PASCAL __apiEnd(unsigned int handle)
{
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:___apiEnd=___apiEnd@4")
#endif
    GlobalInit();
    return ApiEnd(handle);
}

DLLEXPORT APIBOOL FAR PASCAL __apiSwitchDevice(unsigned int handle,
                            const char far *deviceConnection,
                            const char far *deviceApplication)
{
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:___apiSwitchDevice=___apiSwitchDevice@12")
#endif
    Ediabas::ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return APIFALSE;
    }
    if (!apiInternal->apiSwitchDevice(
        ConvertCString(deviceConnection),
        ConvertCString(deviceApplication)))
    {
        return APIFALSE;
    }
    return APITRUE;
}

DLLEXPORT void FAR PASCAL __apiJob(unsigned int handle,
                            const char far *ecu,const char far *job,
                            const char far *para,const char far *result)
{
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:___apiJob=___apiJob@20")
#endif
    Ediabas::ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return;
    }

#if true
    String^ ecuString = ConvertCString(ecu);
    String^ jobString = ConvertCString(job);
    String^ resultString = ConvertCString(result);
    // convert to binary to prevent encoding problems
    long paralen = (para == NULL) ? 0 : (long) strlen(para);
    array<byte>^ paraBuffer = gcnew array<byte>(paralen);
    for (int i = 0; i < paralen; i++)
    {
        paraBuffer[i] = para[i];
    }
    apiInternal->logFormat(Ediabas::ApiInternal::ApiLogLevel::Normal, "__apiJob({0}, {1}, {2}={3}, {4})",
        ecuString, jobString,
        Ediabas::ApiInternal::Encoding->GetString(paraBuffer),
        paraBuffer, resultString);
    apiInternal->executeJob(ecuString, jobString, nullptr, 0, paraBuffer, paraBuffer->Length, resultString);
#else
    apiInternal->apiJob(
        ConvertCString(ecu),
        ConvertCString(job),
        ConvertCString(para),
        ConvertCString(result));
#endif
}

DLLEXPORT void FAR PASCAL __apiJobData(unsigned int handle,
                            const char far *ecu,const char far *job,
                            const unsigned char far *parabuf,int paralen,
                            const char far *result)
{
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:___apiJobData=___apiJobData@24")
#endif
    Ediabas::ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
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
        ConvertCString(ecu),
        ConvertCString(job),
        paraBuffer, paralen,
        ConvertCString(result));
}

DLLEXPORT void FAR PASCAL __apiJobExt(unsigned int handle,
                            const char far *ecu,const char far *job,
                            const unsigned char far *stdparabuf,int stdparalen,
                            const unsigned char far *parabuf,int paralen,
                            const char far *result,long reserved)
{
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:___apiJobExt=___apiJobExt@36")
#endif
    Ediabas::ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
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
        ConvertCString(ecu),
        ConvertCString(job),
        stdParaBuffer, stdparalen,
        paraBuffer, paralen,
        ConvertCString(result),
        reserved);
}

DLLEXPORT int FAR PASCAL __apiJobInfo(unsigned int handle,char far *infoText)
{
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:___apiJobInfo=___apiJobInfo@8")
#endif
    Ediabas::ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return 0;
    }
    String ^ text;
    int percent = apiInternal->apiJobInfo(text);
    ConvertNetString(text, infoText, APIMAXTEXT);
    return percent;
}

DLLEXPORT APIBOOL FAR PASCAL __apiResultChar(unsigned int handle,
                            APICHAR far *buf,const char far *result,
                            APIWORD set)
{
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:___apiResultChar=___apiResultChar@16")
#endif
    Ediabas::ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return APIFALSE;
    }

    wchar_t buffer;
    if (!apiInternal->apiResultChar(
        buffer,
        ConvertCString(result),
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
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:___apiResultByte=___apiResultByte@16")
#endif
    Ediabas::ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return APIFALSE;
    }

    byte buffer;
    if (!apiInternal->apiResultByte(
        buffer,
        ConvertCString(result),
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
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:___apiResultInt=___apiResultInt@16")
#endif
    Ediabas::ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return APIFALSE;
    }

    short buffer;
    if (!apiInternal->apiResultInt(
        buffer,
        ConvertCString(result),
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
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:___apiResultWord=___apiResultWord@16")
#endif
    Ediabas::ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return APIFALSE;
    }

    unsigned short buffer;
    if (!apiInternal->apiResultWord(
        buffer,
        ConvertCString(result),
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
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:___apiResultLong=___apiResultLong@16")
#endif
    Ediabas::ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return APIFALSE;
    }

    int buffer;
    if (!apiInternal->apiResultLong(
        buffer,
        ConvertCString(result),
        set))
    {
        return APIFALSE;
    }
    *buf = buffer;
    return APITRUE;
}

DLLEXPORT APIBOOL FAR PASCAL __apiResultLongLong(unsigned int handle,
    APILONGLONG far* buf, const char far* result,
    APIWORD set)
{
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:___apiResultLongLong=___apiResultLongLong@16")
#endif
    Ediabas::ApiInternal^ apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return APIFALSE;
    }

    int64_t buffer;
    if (!apiInternal->apiResultLongLong(
        buffer,
        ConvertCString(result),
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
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:___apiResultDWord=___apiResultDWord@16")
#endif
    Ediabas::ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return APIFALSE;
    }

    unsigned int buffer;
    if (!apiInternal->apiResultDWord(
        buffer,
        ConvertCString(result),
        set))
    {
        return APIFALSE;
    }
    *buf = buffer;
    return APITRUE;
}

DLLEXPORT APIBOOL FAR PASCAL __apiResultQWord(unsigned int handle,
    APIQWORD far* buf, const char far* result,
    APIWORD set)
{
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:___apiResultQWord=___apiResultQWord@16")
#endif
    Ediabas::ApiInternal^ apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return APIFALSE;
    }

    uint64_t buffer;
    if (!apiInternal->apiResultQWord(
        buffer,
        ConvertCString(result),
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
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:___apiResultReal=___apiResultReal@16")
#endif
    Ediabas::ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return APIFALSE;
    }

    double buffer;
    if (!apiInternal->apiResultReal(
        buffer,
        ConvertCString(result),
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
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:___apiResultText=___apiResultText@20")
#endif
    Ediabas::ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return APIFALSE;
    }

    String^ buffer;
    String^ resultString = ConvertCString(result);
    String^ formatString = ConvertCString(format);
    if (!apiInternal->apiResultText(
        buffer,
        resultString,
        set,
        formatString))
    {
        return APIFALSE;
    }
    array<byte> ^ bytes = ConvertNetString(buffer, buf, APIMAXTEXT);

    apiInternal->logFormat(Ediabas::ApiInternal::ApiLogLevel::Normal, "__apiResultText({0}, {1}, {2}={3})", resultString, set, formatString, bytes);
    return APITRUE;
}

DLLEXPORT APIBOOL FAR PASCAL __apiResultBinary(unsigned int handle,
                            APIBINARY far *buf,APIWORD far *buflen,
                            const char far *result,APIWORD set)
{
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:___apiResultBinary=___apiResultBinary@20")
#endif
    Ediabas::ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return APIFALSE;
    }

    array<byte> ^ buffer;
    unsigned short len;
    if (!apiInternal->apiResultBinary(
        buffer, len,
        ConvertCString(result),
        set))
    {
        return APIFALSE;
    }

    *buflen = len;
    if (len > 0)
    {
        pin_ptr<byte> p = &buffer[0];
        memcpy(buf, p, len);
    }
    return APITRUE;
}

DLLEXPORT APIBOOL FAR PASCAL __apiResultBinaryExt(unsigned int handle,
                            APIBINARY far *buf,APIDWORD far *buflen,APIDWORD bufSize,
                            const char far *result,APIWORD set)
{
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:___apiResultBinaryExt=___apiResultBinaryExt@24")
#endif
    Ediabas::ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return APIFALSE;
    }

    array<byte> ^ buffer;
    unsigned int len;
    if (!apiInternal->apiResultBinaryExt(
        buffer, len,
        bufSize,
        ConvertCString(result),
        set))
    {
        return APIFALSE;
    }

    *buflen = len;
    if (len > 0)
    {
        pin_ptr<byte> p = &buffer[0];
        memcpy(buf, p, len);
    }
    return APITRUE;
}

DLLEXPORT APIBOOL FAR PASCAL __apiResultFormat(unsigned int handle,
                            APIRESULTFORMAT far *buf,const char far *result,
                            APIWORD set)
{
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:___apiResultFormat=___apiResultFormat@16")
#endif
    Ediabas::ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return APIFALSE;
    }

    int buffer;
    if (!apiInternal->apiResultFormat(
        buffer,
        ConvertCString(result),
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
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:___apiResultNumber=___apiResultNumber@12")
#endif
    Ediabas::ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
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
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:___apiResultName=___apiResultName@16")
#endif
    Ediabas::ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
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
    ConvertNetString(buffer, buf, APIMAXNAME);
    return APITRUE;
}

DLLEXPORT APIBOOL FAR PASCAL __apiResultSets(unsigned int handle,APIWORD far *sets)
{
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:___apiResultSets=___apiResultSets@8")
#endif
    Ediabas::ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
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
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:___apiResultVar=___apiResultVar@8")
#endif
    Ediabas::ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return APIFALSE;
    }

    String ^ ecuBuffer;
    if (!apiInternal->apiResultVar(ecuBuffer))
    {
        return APIFALSE;
    }
    ConvertNetString(ecuBuffer, ecu, APIMAXNAME);
    return APITRUE;
}

DLLEXPORT APIRESULTFIELD FAR PASCAL __apiResultsNew(unsigned int handle)
{
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:___apiResultsNew=___apiResultsNew@4")
#endif
    Ediabas::ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return NULL;
    }

    Ediabas::ApiInternal::APIRESULTFIELD ^ apiResultField = apiInternal->apiResultsNew();
    GCHandle hResult = GCHandle::Alloc(apiResultField);
    IntPtr pointer = GCHandle::ToIntPtr(hResult);
    return pointer.ToPointer();
}

DLLEXPORT void FAR PASCAL __apiResultsScope(unsigned int handle,APIRESULTFIELD field)
{
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:___apiResultsScope=___apiResultsScope@8")
#endif
    Ediabas::ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return;
    }

    IntPtr pointer(field);
    GCHandle hResult = GCHandle::FromIntPtr(pointer);
    Ediabas::ApiInternal::APIRESULTFIELD ^ apiResultField = dynamic_cast<Ediabas::ApiInternal::APIRESULTFIELD ^>(hResult.Target);
    if (apiResultField != nullptr)
    {
        apiInternal->apiResultsScope(apiResultField);
    }
}

DLLEXPORT void FAR PASCAL __apiResultsDelete(unsigned int handle,APIRESULTFIELD field)
{
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:___apiResultsDelete=___apiResultsDelete@8")
#endif
    Ediabas::ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return;
    }

    IntPtr pointer(field);
    GCHandle hResult = GCHandle::FromIntPtr(pointer);
    Ediabas::ApiInternal::APIRESULTFIELD ^ apiResultField = dynamic_cast<Ediabas::ApiInternal::APIRESULTFIELD ^>(hResult.Target);
    if (apiResultField != nullptr)
    {
        apiInternal->apiResultsDelete(apiResultField);
        hResult.Free();
    }
}

DLLEXPORT int FAR PASCAL __apiState(unsigned int handle)
{
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:___apiState=___apiState@4")
#endif
    Ediabas::ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return APIERROR;
    }
    return apiInternal->apiState();
}

DLLEXPORT int FAR PASCAL __apiStateExt(unsigned int handle,int suspendTime)
{
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:___apiStateExt=___apiStateExt@8")
#endif
    Ediabas::ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return APIERROR;
    }
    return apiInternal->apiStateExt(suspendTime);
}

DLLEXPORT void FAR PASCAL __apiBreak(unsigned int handle)
{
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:___apiBreak=___apiBreak@4")
#endif
    Ediabas::ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return;
    }
    apiInternal->apiBreak();
}

DLLEXPORT int FAR PASCAL __apiErrorCode(unsigned int handle)
{
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:___apiErrorCode=___apiErrorCode@4")
#endif
    Ediabas::ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return EDIABAS_API_0006;
    }
    return apiInternal->apiErrorCode();
}

DLLEXPORT void FAR PASCAL __apiErrorText(unsigned int handle,
                            char far *buf,int bufsize)
{
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:___apiErrorText=___apiErrorText@12")
#endif
    Ediabas::ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        buf[0] = 0;
        return;
    }
    String ^ buffer = apiInternal->apiErrorText();
    ConvertNetString(buffer, buf, bufsize);
}

DLLEXPORT APIBOOL FAR PASCAL __apiSetConfig(unsigned int handle,
                            const char far *configName,
                            const char far *configValue)
{
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:___apiSetConfig=___apiSetConfig@12")
#endif
    Ediabas::ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return APIFALSE;
    }
    if (!apiInternal->apiSetConfig(
        ConvertCString(configName),
        ConvertCString(configValue)))
    {
        return APIFALSE;
    }
    return APITRUE;
}

DLLEXPORT APIBOOL FAR PASCAL __apiGetConfig(unsigned int handle,
                            const char far *configName,
                            char far *configValue)
{
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:___apiGetConfig=___apiGetConfig@12")
#endif
    Ediabas::ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return APIFALSE;
    }
    String ^ buffer;
    if (!apiInternal->apiGetConfig(
        ConvertCString(configName),
        buffer))
    {
        return APIFALSE;
    }
    array<byte> ^ bytes = Ediabas::ApiInternal::Encoding->GetBytes(buffer);
    ConvertNetString(buffer, configValue, APIMAXRESULT);
    return APITRUE;
}

DLLEXPORT void FAR PASCAL __apiTrace(unsigned int handle,const char far *msg)
{
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:___apiTrace=___apiTrace@8")
#endif
    Ediabas::ApiInternal ^apiInternal = GlobalObjects::GetApiInstance(handle);
    if (apiInternal == nullptr)
    {
        return;
    }
    apiInternal->apiTrace(ConvertCString(msg));
}

__declspec(noinline)
static APIBOOL ApiXSysSetConfig(const char far* cfgName, const char far* cfgValue)
{
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:___apiXSysSetConfig=___apiXSysSetConfig@8")
#endif
    if (!Ediabas::ApiInternal::apiXSysSetConfig(
        ConvertCString(cfgName),
        ConvertCString(cfgValue)))
    {
        return APIFALSE;
    }
    return APITRUE;
}

DLLEXPORT APIBOOL FAR PASCAL __apiXSysSetConfig(const char far *cfgName, const char far *cfgValue)
{
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:___apiXSysSetConfig=___apiXSysSetConfig@8")
#endif
    GlobalInit();
    return ApiXSysSetConfig(cfgName, cfgValue);
}

__declspec(noinline)
void CloseServer()
{
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:closeServer=_closeServer@0")
#endif
    Ediabas::ApiInternal::closeServer();
}

DLLEXPORT void FAR PASCAL closeServer()
{
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:closeServer=_closeServer@0")
#endif
    GlobalInit();
    CloseServer();
}

__declspec(noinline)
static APIBOOL EnableServer(APIBOOL onOff)
{
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:enableServer=_enableServer@4")
#endif
    if (!Ediabas::ApiInternal::enableServer(onOff ? true : false))
    {
        return APIFALSE;
    }
    return APITRUE;
}

DLLEXPORT APIBOOL FAR PASCAL enableServer(APIBOOL onOff)
{
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:enableServer=_enableServer@4")
#endif
    GlobalInit();
    return EnableServer(onOff);
}

__declspec(noinline)
static APIBOOL EnableMultiThreading(bool onOff)
{
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:enableMultiThreading=_enableMultiThreading@4")
#endif
    if (!Ediabas::ApiInternal::enableMultiThreading(onOff ? true : false))
    {
        return APIFALSE;
    }
    return APITRUE;
}

DLLEXPORT APIBOOL FAR PASCAL enableMultiThreading(bool onOff)
{
#if defined(_M_IX86)
#pragma comment(linker, "/EXPORT:enableMultiThreading=_enableMultiThreading@4")
#endif
    GlobalInit();
    return EnableMultiThreading(onOff);
}

#ifdef __cplusplus
}
#endif 
