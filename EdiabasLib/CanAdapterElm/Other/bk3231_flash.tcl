set BK3000_MFC_BASE_ADDR    0x800000
set BK3000_MFC_KEYWORD      [expr {$BK3000_MFC_BASE_ADDR + 0x0}]
set BK3000_MFC_CTL          [expr {$BK3000_MFC_BASE_ADDR + 0x4}]
set BK3000_MFC_ADDR         [expr {$BK3000_MFC_BASE_ADDR + 0x8}]
set BK3000_MFC_DATA         [expr {$BK3000_MFC_BASE_ADDR + 0xC}]
set BK3000_MFC_WE_P1        [expr {$BK3000_MFC_BASE_ADDR + 0x10}]
set BK3000_MFC_WE_P2        [expr {$BK3000_MFC_BASE_ADDR + 0x14}]
set BK3000_MFC_WE_P3        [expr {$BK3000_MFC_BASE_ADDR + 0x18}]

set MAIN_SPACE              0x00
set NVR_SPACE               0x1
set RDN_SPACE               0x2

set LOADER_ADDR             0x3EE00

proc flash_erase_sector { ADDR SPACE_CTL } {
	global BK3000_MFC_KEYWORD
	global BK3000_MFC_CTL
	global BK3000_MFC_ADDR
	global BK3000_MFC_DATA
	global BK3000_MFC_WE_P1
	global BK3000_MFC_WE_P2
	global BK3000_MFC_WE_P3

	set cmd [expr { (2 << 2) | ($SPACE_CTL << 5) | 1 }]
	mww $BK3000_MFC_WE_P1 0xA5
	mww $BK3000_MFC_WE_P2 0xC3
	mww $BK3000_MFC_ADDR [expr {$ADDR >> 2}]
	mww $BK3000_MFC_KEYWORD 0x58A9
	mww $BK3000_MFC_KEYWORD 0xA958
	mww $BK3000_MFC_CTL $cmd

	while 1 {
		mem2array data 32 $BK3000_MFC_CTL 1
		#echo [format "ctl=0x%08X" $data(0)]
		if {[expr { ($data(0) & 0x01) == 0x00}]} {
			break
		}
	}
}

proc flash_erase_chip { } {
	global BK3000_MFC_KEYWORD
	global BK3000_MFC_CTL
	global BK3000_MFC_ADDR
	global BK3000_MFC_DATA
	global BK3000_MFC_WE_P1
	global BK3000_MFC_WE_P2
	global BK3000_MFC_WE_P3
	global MAIN_SPACE

	set cmd [expr { (7 << 2) | ($MAIN_SPACE << 5) | 1 }]
	mww $BK3000_MFC_WE_P1 0xA5
	mww $BK3000_MFC_WE_P2 0xC3
	mww $BK3000_MFC_ADDR 0
	mww $BK3000_MFC_KEYWORD 0x58A9
	mww $BK3000_MFC_KEYWORD 0xA958
	mww $BK3000_MFC_CTL $cmd

	while 1 {
		mem2array data 32 $BK3000_MFC_CTL 1
		#echo [format "ctl=0x%08X" $data(0)]
		if {[expr { ($data(0) & 0x01) == 0x00}]} {
			break
		}
	}
}

proc flash_read_dword { ADDR SPACE_CTL } {
	global BK3000_MFC_KEYWORD
	global BK3000_MFC_CTL
	global BK3000_MFC_ADDR
	global BK3000_MFC_DATA
	global BK3000_MFC_WE_P1
	global BK3000_MFC_WE_P2
	global BK3000_MFC_WE_P3

	set cmd [expr { (0 << 2) | ($SPACE_CTL << 5) | 1 }]
	mww $BK3000_MFC_ADDR [expr {$ADDR >> 2}]
	mww $BK3000_MFC_KEYWORD 0x58A9
	mww $BK3000_MFC_KEYWORD 0xA958
	mww $BK3000_MFC_CTL $cmd
	while 1 {
		mem2array data 32 $BK3000_MFC_CTL 1
		#echo [format "ctl=0x%08X" $data(0)]
		if {[expr { ($data(0) & 0x01) == 0x00}]} {
			break
		}
	}
	mem2array data 32 $BK3000_MFC_DATA 1
	#echo [format "data=0x%08X" $data(0)]
	return $data(0)
}

proc flash_write_dword { ADDR VALUE SPACE_CTL {UNLOCK 1} } {
	global BK3000_MFC_KEYWORD
	global BK3000_MFC_CTL
	global BK3000_MFC_ADDR
	global BK3000_MFC_DATA
	global BK3000_MFC_WE_P1
	global BK3000_MFC_WE_P2
	global BK3000_MFC_WE_P3

	set cmd [expr { (1 << 2) | ($SPACE_CTL << 5) | 1 }]
	if {$UNLOCK != 0} {
		mww $BK3000_MFC_WE_P1 0xA5
		mww $BK3000_MFC_WE_P2 0xC3
	}
	mww $BK3000_MFC_DATA $VALUE
	mww $BK3000_MFC_ADDR [expr {$ADDR >> 2}]
	mww $BK3000_MFC_KEYWORD 0x58A9
	mww $BK3000_MFC_KEYWORD 0xA958
	mww $BK3000_MFC_CTL $cmd
	while 1 {
		mem2array data 32 $BK3000_MFC_CTL 1
		#echo [format "ctl=0x%08X" $data(0)]
		if {[expr { ($data(0) & 0x01) == 0x00}]} {
			break
		}
	}
}

proc flash_read_area { FILENAME ADDR LEN {SPACE_CTL 0} } {
	reset init
	set fp [open $FILENAME w]
	fconfigure $fp -translation binary
	for { set x 0 } { $x < $LEN } { set x [expr $x + 4]} {
		set addr [expr { $ADDR + $x }]
		set data [flash_read_dword $addr $SPACE_CTL]
		#echo [format "addr=0x%08X, data=0x%08X" $addr $data]
		set bin_data [binary format iu $data ]
		puts -nonewline $fp $bin_data
		if {[expr {($x & 0xFF) == 0}]} {
			echo [format "addr=0x%08X" $addr]
		}
	}
	close $fp
}

proc flash_write_area { FILENAME ADDR {SPACE_CTL 0} } {
	global BK3000_MFC_KEYWORD
	global BK3000_MFC_CTL
	global BK3000_MFC_ADDR
	global BK3000_MFC_DATA
	global BK3000_MFC_WE_P1
	global BK3000_MFC_WE_P2
	global BK3000_MFC_WE_P3
	global MAIN_SPACE
	global NVR_SPACE
	global RDN_SPACE

	if {[file exists $FILENAME] == 0} {
		echo [format "file %s not existing" $FILENAME]
		return
	}
	reset init
	if {$SPACE_CTL == $RDN_SPACE} {
		echo "invalid space control"
		return
	}
	set fsize [file size $FILENAME]
	set fp [open $FILENAME r]
	fconfigure $fp -translation binary

	mww $BK3000_MFC_WE_P1 0xA5
	mww $BK3000_MFC_WE_P2 0xC3
	for { set x 0 } { $x < $fsize } { set x [expr $x + 4]} {
		set addr [expr { $ADDR + $x }]
		if {[expr {($addr & 0xFF) == 0x00}]} {
			echo [format "erasing sector=0x%08X" $addr]
			flash_erase_sector $addr $SPACE_CTL
		}
		set bin_data [read $fp 4]
		binary scan $bin_data iu data
		#echo [format "addr=0x%08X, data=0x%08X" $addr $data]
		flash_write_dword $addr $data $SPACE_CTL 0
		if {[expr {($x & 0xFF) == 0}]} {
			echo [format "addr=0x%08X" $addr]
		}
	}
	mww $BK3000_MFC_WE_P1 0x00
	mww $BK3000_MFC_WE_P2 0x00
	close $fp
}

proc flash_write_loader { {FILENAME "Bk3231Flash/write_flash.bin"} } {
	global LOADER_ADDR

	flash_write_area $FILENAME $LOADER_ADDR
}

proc flash_write_area_fast { FILENAME ADDR } {
	global cpu_halted
	global LOADER_ADDR

	if {[file exists $FILENAME] == 0} {
		echo [format "file %s not existing" $FILENAME]
		return
	}
	reset init
	mem2array data 32 $LOADER_ADDR 1
	if {[expr $data(0) == 0xFFFFFFFF]} {
		echo "No loader present. Write it with flash_write_loader first."
		return
	}

	set fsize [file size $FILENAME]
	set fp [open $FILENAME r]
	fconfigure $fp -translation binary

	set flash_addr $ADDR
	set flash_count 0
	set data_list {}
	for { set x 0 } { $x < $fsize } { set x [expr $x + 4]} {
		set bin_data [read $fp 4]
		binary scan $bin_data iu data
		lappend data_list $flash_count $data
		set flash_count [expr $flash_count + 1]
		set flash_len [expr $flash_count * 4]
		if {[expr {($flash_len >= 0x3000) || (($x + 4) >= $fsize})]} {
			if {$flash_count == 0} {
				break;
			}
			if {$flash_addr >= $LOADER_ADDR} {
				break;
			}
			array set data_array $data_list
			array2mem data_array 32 0x400000 $flash_count
			mww 0x403E00 $flash_addr
			mww 0x403E04 $flash_len
			echo [format "addr=0x%08X, len=0x%04X" $flash_addr $flash_len]
			set cpu_halted 0
			resume $LOADER_ADDR
			while 1 {
				if {$cpu_halted != 0} {
					break
				}
				sleep 10
			}
			set flash_addr [expr $flash_addr + $flash_len]
			set flash_count 0
			set data_list {}
		}
	}
	close $fp
}

proc flash_erase_area { ADDR LEN {SPACE_CTL 0} } {
	global MAIN_SPACE
	global NVR_SPACE
	global RDN_SPACE

	reset init
	if {$SPACE_CTL == $RDN_SPACE} {
		echo "invalid space control"
		return
	}
	for { set x 0 } { $x < $LEN } { set x [expr $x + 4]} {
		set addr [expr { $ADDR + $x }]
		if {[expr {($addr & 0xFF) == 0x00}]} {
			echo [format "erasing sector=0x%08X" $addr]
			flash_erase_sector $addr $SPACE_CTL
		}
	}
}

proc set_cpu_speed {} {
	mww 0x920000 0x01
	mww 0x920004 0x00

	adapter_khz 1000
	arm7_9 dcc_downloads enable
	arm7_9 fast_memory_access enable

}
