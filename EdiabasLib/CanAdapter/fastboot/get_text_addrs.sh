#!/bin/sh

# Time-stamp: <2010-02-01 12:19:18 hcz>
# written by H. C. Zimmerer

# This is somewhat tricky: for devices without a boot section, the
# bootloader is linked directly at the end of flash so no space is
# wasted.  The original bootloader achieves this by using a fixed
# number and subtracting offsets for the sections which are not
# included.  The approach used here is fully automatic: it extracts
# the size of the bootloader from the relocatable object file,
# calculates the load addresses, and passes them to the linker script.
# This way the size of the bootloader may change, but no need arises
# to fiddle around with fixed numbers until they match.

# What happens is: Use bootload.o to get the size of the bootloader's
# text section (without the final jmp stub).  Output a line containing
# shell assignments to LOADER_START (byte start address of the
# bootloader) and STUB_OFFSET (Offset from the beginning of the
# bootloader to the final api_call jmp) so that the bootloader exactly
# fits at the end of flash without any gap.

# Invocation: Expects in $1 the word address of the higest flash cell
# (e.g. '0x1fff' for 16 kByte devices) (#define FLASHEND in the Atmel
# def file).

if [ "$1" = "-c" ]; then
    copt=1
    shift
fi

[ $# -ne 1 ] && {
    echo "\
Syntax: ${0##*/} [-c] higest_flash_word_address
Function: compute linker parameters for peda's bootloader
Opts:
  -c  Use an as compact as possible code layout (smaller than the original)"
    exit 1
}


end_wordaddr=$(($1))
flash_end=$(printf "%#x\n" $(($1 * 2 + 1)))

boot_map=$(avr-objdump -h bootload.o) || exit
boot_bytes=$(echo "$boot_map" | gawk '/.text/ {print "0x" $3}')
boot_bytes=$((boot_bytes + 2))  # add stub size
boot_words=$((boot_bytes / 2))

[ -z "$copt" ] && boot_bytes="$(( (boot_bytes | 0xff) + 1 ))"

printf >&2 "*** Last available byte address for the user program: %#x\n" \
 $((flash_end + 1 - boot_bytes - 3))
printf "LOADER_START=%#x\n" $((flash_end + 1 - boot_bytes))
printf "STUB_OFFSET=%#x\n" $((boot_bytes - 2))
