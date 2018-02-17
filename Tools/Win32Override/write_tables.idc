#include <idc.idc>

/*
// File:
//  write decryptor tables
//
*/


static main(void)
{
  auto fname;
  auto fhandle;
  auto signature;

  auto offset;

  Message("-------------------------------------------------------------------------------\n");
  Message("Write decryptor tables\n");
  Message("-------------------------------------------------------------------------------\n");

  fname = ask_file(-1, "*.bin", "Binary table 1");
  if(fname == 0)
  {
    return -1;
  }
  offset = ask_addr(0xFA7E14D0, "Start address of table 1");

  fhandle = fopen(fname, "wb");
  if(fhandle == 0)
  {
    Message("\nUnable to open file\n");
    return -1;
  }

  savefile(fhandle, 0, offset, 0x400);

  fclose(fhandle);

  Message("The file has been stored\n");

  fname = ask_file(-1, "*.bin", "Binary table 2");
  if(fname == 0)
  {
    return -1;
  }
  offset = ask_addr(0x249B910E, "Start address of table 2");

  fhandle = fopen(fname, "wb");
  if(fhandle == 0)
  {
    Message("\nUnable to open file\n");
    return -1;
  }

  savefile(fhandle, 0, offset, 0x400);

  fclose(fhandle);

  Message("The file has been stored\n");

  return 0;
}
