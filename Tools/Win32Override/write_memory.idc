#include <idc.idc>

/*
// File:
//  write memory block
//
*/


static main(void)
{
  auto fname;
  auto fhandle;
  auto signature;

  auto offset;
  auto size;

  Message("-------------------------------------------------------------------------------\n");
  Message("Write memory block\n");
  Message("-------------------------------------------------------------------------------\n");

  fname = ask_file(-1, "*.bin", "Binary file");
  if(fname == 0)
  {
    return -1;
  }
  offset = ask_addr(0, "Start address memory");
  size = ask_addr(0x100, "Memory size");

  fhandle = fopen(fname, "wb");
  if(fhandle == 0)
  {
    Message("\nUnable to open file\n");
    return -1;
  }

  savefile(fhandle, 0, offset, size);

  fclose(fhandle);

  Message("The file has been stored\n");

  return 0;
}
