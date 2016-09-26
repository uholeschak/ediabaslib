LOADER_ADDR = 0x3E000

all: write_flash.bin

write_flash.bin: write_flash.axf
	fromelf --bin --output write_flash.bin write_flash.axf

write_flash.axf: flash.o
	armlink --remove --info Totals,Sizes --ro-base $(LOADER_ADDR) --rw-base 0x403000 --no-startup --entry=start --info=any --map --symbols --list mapfile.txt -o write_flash.axf flash.o

%.o: %.c
	armcc -DLOADER_ADDR=$(LOADER_ADDR) --c99 --gnu --arm -c --cpu ARM966E-S --asm $< -o $@

clean:
	-$(RM) write_flash.axf *.o
