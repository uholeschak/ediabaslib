#include <windows.h>
#include <stdio.h>
#include <stdint.h>
#include <tchar.h>
#include <Shlwapi.h>
#include <WinNT.h>
#include <string>
#define ZLIB_WINAPI
extern "C"
{
    #include "zlib.h"
}

#define MAX_SECTIONS    2

typedef struct
{
	const char *name;
	const unsigned char *data;
	const unsigned int size;
} MonoBundledAssembly;

typedef struct
{
    MonoBundledAssembly assembly;
    int compressed_size;
} CompressedAssembly;

typedef struct
{
    DWORD data_segment_raw;
    DWORD data_segment_virt_start;
    DWORD data_segment_size;
} SectionInfo;

const DWORD alignment = 0x0010;
SectionInfo SectionList[MAX_SECTIONS];
void *file_data;

static int my_inflate (const Byte *compr, uLong compr_len, Byte *uncompr, uLong uncompr_len)
{
    int err;
    z_stream stream;

    memset (&stream, 0, sizeof (z_stream));
    stream.next_in = (Byte *) compr;
    stream.avail_in = (uInt) compr_len;
    err = inflateInit (&stream);
    if (err != Z_OK)
    {
        return 1;
    }

    for (;;)
    {
        stream.next_out = uncompr;
        stream.avail_out = (uInt) uncompr_len;
        err = inflate (&stream, Z_NO_FLUSH);
        if (err == Z_STREAM_END) break;
        if (err != Z_OK)
        {
            printf ("%d\n", err);
            return 2;
        }
    }

    err = inflateEnd (&stream);
    if (err != Z_OK)
    {
        return 3;
    }

    if (stream.total_out != uncompr_len)
    {
        return 4;
    }

    return 0;
}

BOOL read_pe_header()
{
    PIMAGE_DOS_HEADER pdos_header = (PIMAGE_DOS_HEADER) file_data;
    if (pdos_header->e_magic != IMAGE_DOS_SIGNATURE)
    {
        return FALSE;
    }
    LONG pe_offset = pdos_header->e_lfanew;
    PIMAGE_NT_HEADERS pnt_header = (PIMAGE_NT_HEADERS) ((uint8_t*) file_data + pe_offset);
    if (pnt_header->Signature != IMAGE_NT_SIGNATURE)
    {
        return FALSE;
    }
    WORD num_sections = pnt_header->FileHeader.NumberOfSections;
    PIMAGE_SECTION_HEADER section_header = IMAGE_FIRST_SECTION(pnt_header);

    for (int i = 0; i < MAX_SECTIONS; i++)
    {
        SectionList[i].data_segment_size = 0;
    }
    for (int i = 0; i < num_sections; i++)
    {
        if (memcmp(section_header->Name, ".data\0\0\0", IMAGE_SIZEOF_SHORT_NAME) == 0)
        {   // section found
            SectionList[0].data_segment_raw = section_header->PointerToRawData;
            SectionList[0].data_segment_virt_start = section_header->VirtualAddress + pnt_header->OptionalHeader.ImageBase;
            SectionList[0].data_segment_size = section_header->SizeOfRawData;
        }
        if (memcmp(section_header->Name, ".rdata\0\0", IMAGE_SIZEOF_SHORT_NAME) == 0)
        {   // section found
            SectionList[1].data_segment_raw = section_header->PointerToRawData;
            SectionList[1].data_segment_virt_start = section_header->VirtualAddress + pnt_header->OptionalHeader.ImageBase;
            SectionList[1].data_segment_size = section_header->SizeOfRawData;
        }
        section_header++;
    }
    if (SectionList[0].data_segment_size == 0)
    {   // no data segment
        return FALSE;
    }
    return TRUE;
}

void *map_address(uint32_t address)
{
    for (int i = 0; i < MAX_SECTIONS; i++)
    {
        SectionInfo *pSectionInfo = &SectionList[i];
        if (address >= pSectionInfo->data_segment_virt_start &&
            address < (pSectionInfo->data_segment_virt_start + pSectionInfo->data_segment_size))
        {
            return ((uint8_t*) file_data + (address - pSectionInfo->data_segment_virt_start + pSectionInfo->data_segment_raw));
        }
    }
    return NULL;
}

BOOL validate_data_ptr(void *ptr)
{
    for (int i = 0; i < MAX_SECTIONS; i++)
    {
        SectionInfo *pSectionInfo = &SectionList[i];
        if (ptr < (void *) pSectionInfo->data_segment_virt_start)
        {
            continue;
        }
        if (ptr >= (void *) (pSectionInfo->data_segment_virt_start + pSectionInfo->data_segment_size))
        {
            continue;
        }
        return TRUE;
    }
    return FALSE;
}

BOOL validate_compressed_struct(uint32_t address)
{
    void **compressed = (void **) ((uint8_t *) file_data + address);
    CompressedAssembly **ptr = (CompressedAssembly **) compressed;
    CompressedAssembly **ptr_last = NULL;
    int count = 0;
    while (*ptr != NULL)
    {
        if (!validate_data_ptr(*ptr)) return FALSE;
        if (ptr_last != NULL)
        {
            if ((uint32_t)(*ptr) - (uint32_t)(*ptr_last) != alignment)
            {
                return FALSE;
            }
        }
        CompressedAssembly *c_assem = (CompressedAssembly *) map_address((uint32_t) (*ptr));
        if (c_assem == NULL) return FALSE;
        if (!validate_data_ptr((void *) c_assem->assembly.name)) return FALSE;
        if (!validate_data_ptr((void *) c_assem->assembly.data)) return FALSE;
        if (!validate_data_ptr((void *) (c_assem->assembly.data + c_assem->compressed_size - 1))) return FALSE;
        if ((DWORD) c_assem->compressed_size > c_assem->assembly.size) return FALSE;

        count++;
        if (count > 100)
        {
            return FALSE;
        }
        ptr_last = ptr;
        ptr++;
    }
    if (count < 2)
    {
        return FALSE;
    }
    return TRUE;
}

uint32_t search_compressed_struct()
{
    uint32_t address = SectionList[0].data_segment_raw;
    for (;;)
    {
        if (address >= SectionList[0].data_segment_raw + SectionList[0].data_segment_size)
        {
            return 0;
        }
        if (validate_compressed_struct(address))
        {
            break;
        }
        address += alignment;
    }
    return address;
}

BOOL store_xml(std::wstring output_path)
{
    unsigned int file_count = 0;
    for (int i = 0; i < MAX_SECTIONS; i++)
    {
        SectionInfo *pSectionInfo = &SectionList[i];
        DWORD address = pSectionInfo->data_segment_raw;
        for (;;)
        {
            if ((address + alignment) >= (pSectionInfo->data_segment_raw + pSectionInfo->data_segment_size))
            {
                break;
            }
            uint8_t *ptr = (uint8_t *)file_data + address;
            if (memcmp(ptr, "<?xml", 5) == 0)
            {
                wchar_t name_buffer[MAX_PATH];
                swprintf(name_buffer, MAX_PATH, _T("info%u.xml"), file_count + 1);
                file_count++;

                std::wstring output_file = output_path + _T("\\") + name_buffer;
                FILE *fw = _wfopen(output_file.c_str(), _T("wb"));
                if (fw == NULL)
                {
                    return FALSE;
                }
                DWORD length = 0;
                uint8_t *ptr2 = ptr;
                while (*ptr2 != 0)
                {
                    if (ptr2 >= ((uint8_t *) file_data + pSectionInfo->data_segment_raw + pSectionInfo->data_segment_size))
                    {
                        break;
                    }
                    ptr2++;
                    length++;
                }
                fwrite(ptr, length, 1, fw);
                fclose(fw);
            }
            address += alignment;
        }
    }
    return TRUE;
}

BOOL AnsiToUnicode16(const CHAR *in_Src, WCHAR *out_Dst, INT in_MaxLen)
{
    /* locals */
    INT lv_Len;

  // do NOT decrease maxlen for the eos
  if (in_MaxLen <= 0)
    return FALSE;

  // let windows find out the meaning of ansi
  // - the SrcLen=-1 triggers MBTWC to add a eos to Dst and fails if MaxLen is too small.
  // - if SrcLen is specified then no eos is added
  // - if (SrcLen+1) is specified then the eos IS added
  lv_Len = MultiByteToWideChar(CP_ACP, 0, in_Src, -1, out_Dst, in_MaxLen);

  // validate
  if (lv_Len < 0)
    lv_Len = 0;

  // ensure eos, watch out for a full buffersize
  // - if the buffer is full without an eos then clear the output like MBTWC does
  //   in case of too small outputbuffer
  // - unfortunately there is no way to let MBTWC return shortened strings,
  //   if the outputbuffer is too small then it fails completely
  if (lv_Len < in_MaxLen)
    out_Dst[lv_Len] = 0;
  else if (out_Dst[in_MaxLen-1])
    out_Dst[0] = 0;

  // done
  return TRUE;
}

int DeleteDirectory(const std::wstring &refcstrRootDirectory,
                    bool              bDeleteSubdirectories = true)
{
  bool            bSubdirectory = false;       // Flag, indicating whether
                                               // subdirectories have been found
  HANDLE          hFile;                       // Handle to directory
  std::wstring    strFilePath;                 // Filepath
  std::wstring    strPattern;                  // Pattern
  WIN32_FIND_DATA FileInformation;             // File information


  strPattern = refcstrRootDirectory + _T("\\*.*");
  hFile = ::FindFirstFile(strPattern.c_str(), &FileInformation);
  if(hFile != INVALID_HANDLE_VALUE)
  {
    do
    {
      if(FileInformation.cFileName[0] != '.')
      {
        strFilePath.erase();
        strFilePath = refcstrRootDirectory + _T("\\") + FileInformation.cFileName;

        if(FileInformation.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
        {
          if(bDeleteSubdirectories)
          {
            // Delete subdirectory
            int iRC = DeleteDirectory(strFilePath, bDeleteSubdirectories);
            if(iRC)
              return iRC;
          }
          else
            bSubdirectory = true;
        }
        else
        {
          // Set file attributes
          if(::SetFileAttributes(strFilePath.c_str(),
                                 FILE_ATTRIBUTE_NORMAL) == FALSE)
            return ::GetLastError();

          // Delete file
          if(::DeleteFile(strFilePath.c_str()) == FALSE)
            return ::GetLastError();
        }
      }
    } while(::FindNextFile(hFile, &FileInformation) == TRUE);

    // Close handle
    ::FindClose(hFile);

    DWORD dwError = ::GetLastError();
    if(dwError != ERROR_NO_MORE_FILES)
      return dwError;
    else
    {
      if(!bSubdirectory)
      {
        // Set directory attributes
        if(::SetFileAttributes(refcstrRootDirectory.c_str(),
                               FILE_ATTRIBUTE_NORMAL) == FALSE)
          return ::GetLastError();

        // Delete directory
        if(::RemoveDirectory(refcstrRootDirectory.c_str()) == FALSE)
          return ::GetLastError();
      }
    }
  }

  return 0;
}

int _tmain(int argc, const wchar_t *argv[])
{
    int result = 0;

    if (argc < 2)
    {
        printf("Missing arguments\n");
        return 1;
    }
    const wchar_t *filename = argv[1];
    wchar_t name_buffer[MAX_PATH];
    wcscpy(name_buffer, filename);
    PathRemoveFileSpec(name_buffer);
    std::wstring output_path = name_buffer + std::wstring(_T("\\extract"));

    FILE *fp = _wfopen(filename, _T("rb"));
    if (fp == NULL)
    {
        printf("Opening file failed\n");
        return 1;
    }

    fseek(fp, 0L, SEEK_END);
    long file_length = ftell(fp);
    rewind(fp);

    file_data = malloc(file_length);
    fread(file_data, file_length, 1, fp);
    fclose(fp);

    if (!read_pe_header())
    {
        printf("Invalid PE header\n");
        result = 0;
        goto DONE;
    }
    uint32_t comp_struct = search_compressed_struct();
    if (comp_struct == 0)
    {
        printf("Invalid compressed stucture\n");
        result = 0;
        goto DONE;
    }

    DeleteDirectory(output_path, false);
    Sleep(10);
    if (!CreateDirectory(output_path.c_str(), NULL))
    {
        printf("Unable to create output directory\n");
        result = 0;
        goto DONE;
    }

    if (!store_xml(output_path))
    {
        printf("Unable to store XML\n");
        result = 0;
        goto DONE;
    }

    void **compressed = (void **) ((uint8_t *) file_data + comp_struct);
    CompressedAssembly **ptr = (CompressedAssembly **) compressed;
    while (*ptr != NULL)
    {
        uLong real_size;
        uLongf zsize;
        int result;
        CompressedAssembly *c_assem = (CompressedAssembly *) map_address((uint32_t) (*ptr));
        if (c_assem == NULL)
        {
            printf ("Invalid pointer %p\n", *ptr);
            result = 0;
            goto DONE;
        }
        const char *name = (const char *) map_address((uint32_t) (c_assem->assembly.name));
        const unsigned char *data = (const unsigned char *) map_address((uint32_t) (c_assem->assembly.data));

        real_size = c_assem->assembly.size;
        zsize = c_assem->compressed_size;
        Bytef *buffer = (Bytef *) malloc (real_size);
        result = my_inflate (data, zsize, buffer, real_size);
        if (result != 0)
        {
            free(buffer);
            printf ("Error %d decompressing data for %s\n", result, name);
            result = 0;
            goto DONE;
        }
        AnsiToUnicode16(name, name_buffer, MAX_PATH);
        std::wstring output_file = output_path + _T("\\") + name_buffer;
        FILE *fw = _wfopen(output_file.c_str(), _T("wb"));
        if (fw == NULL)
        {
            free(buffer);
            printf ("Unable to store data for %s\n", name);
            result = 0;
            goto DONE;
        }
        fwrite(buffer, real_size, 1, fw);
        fclose(fw);
        free(buffer);
        ptr++;
    }
DONE:
    free(file_data);

    return result;
}
