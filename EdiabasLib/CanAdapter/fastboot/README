Time-stamp: <2010-01-14 23:56:41 hcz>

DESCRIPTION
===========

 This is an adaption of Peter Dannegger's fine AVR Bootloader
 (Fastboot) for Linux and WinAVR.


NEW in BUILD# 22
================

 The entire configuration has been moved into Makefile.  You don't
 need to go into another file anymore to adapt the bootloader to your
 configuration.


QUICK START (LINUX)
===================

 Unpack the fastboot_buildNN.tar.gz file.  Change into the
 directory fastboot/.

 Edit the marked section at the beginning of the Makefile.  There are
 comments with every topic which should be enough to get you started.
 Note that the CPU frequency is now defined in Makefile, not in
 fastload.h as with the original, as well as the MCU type.  So don't
 change anything in the other files even if the comments say so.

 Then add the mXXXdef.inc file which you named in Makefile into the
 current directory.  See the Makefile for instructions on how to get
 at it.  (It is needed to create atmel_def.mak.)

 Edit the section after that to reflect your transmitter
 and receiver pins.  Make sure none of the include files is selected.

 After this is done, simply run make.  This creates the output files
 bootload.elf and bootload.hex (ELF and IHEX format), ready for
 programming into the AVR's flash.


QUICK START (WinAVR/AVR Studio)
===============================
 
 If you're on Windows, consider using the original sources and AVR
 Assembler.  For now, I'll assume you have a some reason not to do so.

 Unpack, edit Makefile and add the mXXXdef.inc file as described above
 for Linux.
 
 Create a new project using AVR Studio.  Give it the name 'bootload'
 (ignoring for now that there is no bootload.c file) and use the
 fastboot/ directory as project directory.  Other AVR Studio options
 as usual.

 In Project->Configuration Options, check the box "Use External
 Makefile".  Press the '...' button to the right and select the
 Makefile in the above project subdirectory.

 Now you can build the bootloader.


NOTES
=====

 See http://www.mikrocontroller.net/topic/73196 for the original
 software (German). 

 This bootloader speaks a protocol of its own, see
 http://www.mikrocontroller.net/attachment/27570/Bootloaderprotokoll.txt
 and thus needs its own PC download software.  For Linux, one is
 available at
 http://www.avrfreaks.net/index.php?module=Freaks%20Academy&func=viewItem&item_type=project&item_id=1927
 You must be registered at avrfreaks.net to access this location.


BUGS
====

 Currently, only the ATmega168 is really thoroughly tested.  If you
 have success with other devices, please pass me a note so I can
 include them here.  

 You don't need to be very adventurous to try with other devices, but
 some familiarity with the tools and with AVRs in general won't hurt
 and may be needed.  Be prepared for some problems to show up,
 especially for the current early releases.  If they do show up and if
 you have access to a Windows machine to assemble the original, have a
 look at the Makefile section about the target 'cmp'.  It will allow
 you to compare the original with the Linux version, displaying in
 more detail where something went wrong.  Please include the result in
 case you're writing me for assistance.

 The directory structure may seem unintuitive.  It is a result of the
 build process: most of the original sources were auto-converted by a
 script, the goal being to use as few edits as possible in order to
 ease keeping track with future changes to the original.  Readability
 was definitely not the very first concern during the make of this
 port - staying as close to the original as possible was the primary
 intention.


AUTHOR
======

 The bootloader 'Fastload' is written by Peter Dannegger.  Thanks for
 a useful and compact solution.  The Linux adaption (including this
 file) was done by H. C. Zimmerer <hcz@hczim.de>.
