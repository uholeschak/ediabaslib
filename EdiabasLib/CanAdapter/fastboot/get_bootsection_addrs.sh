#!/bin/sh

# Time-stamp: <2010-02-01 12:20:02 hcz>
# written by H. C. Zimmerer

# This small scripts selects the bootloader start address.  It expects
# as ists first argument the flash size (in words), in the following
# arguments the available word start addresses of the boot sections
# and it reads from bootload.o the size of the bootloader.

# It outputs on stdout in a form suitable to feeding it to sh two
# assignemnts: LOADER_START (the start address of the bootloader in
# bytes) and STUB_OFFSET (the offset of added/stub.S relative to
# LOADER_START (bytes)).
#
# (c) 2010 Heike C. Zimmerer <hcz@hczim.de>

#set -x

[ $# -lt 2 ] || [ "${1#-}" != "$1" ] && {
    echo "\
Syntax: ${0##*/} higest_flash_word_address bootsection_word_addresses ..."
    exit 1
}

end_wordaddr=$(($1))
shift

boot_map=$(avr-objdump -h bootload.o) || exit
boot_bytes=$(echo "$boot_map" | gawk '/.text/ {print "0x" $3}')
boot_bytes=$((boot_bytes + 2))  # add stub size
boot_words=$((boot_bytes / 2))


diff=65535
for bootsection_words; do
    d=$(( (end_wordaddr - bootsection_words) - boot_words))
    if [ "$d" -ge 0 ] && [ "$d" -lt "$diff" ]; then
        diff=$d
        bootsection_words_start=$bootsection_words
    fi
done


printf "LOADER_START=%#x\n" $((bootsection_words_start * 2))
printf "STUB_OFFSET=%#x\n" $(( (end_wordaddr - bootsection_words_start) * 2))

echo >&2 "*** Note: set BOOTSZ fuses to the word address $bootsection_words_start ***"
