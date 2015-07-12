#!/usr/bin/gawk -f

# _conv.awk
#
# gawk script to convert AVR Assembler syntax into GNU gas syntax.
#
# This catches some of the more common conversion tasks.
# One big problem - that AVR Asm uses word addresses, but gas
# takes byte addresses instead - cannot be solved this way.
#
# Time-stamp: <2009-07-20 13:27:06 hcz>
#

BEGIN { IGNORECASE = 1 }

### Below are conversions specific to the project (bootload)
# 	'rjmp pc+1' (2 cycle NOP)
/rjmp[[:space:]]+pc *\+ *1/ ||
/rjmp[[:space:]]+PC *\+ *1/ {
    sub(/rjmp[[:space:]]+pc *\+ *1/, "rjmp .")
}

/low *\(2 *\* *BootStart\)/ { sub(/low *\(2 *\* *BootStart\)/, "(2*BOOTSTART)\\&0xff")}
/high *\(2 *\* *BootStart\)/ { sub(/high *\(2 *\* *BootStart\)/, "BOOTSTART>>7")}

$1 == ".nolist" {next}
/2 *\* *PageSize/ { sub(/2 *\* *PageSize/, "PageSize") }
/PageSize *\* *2/ { sub(/PageSize *\* *2/, "PageSize") }


$1 == ".org" \
&& ($2 == "Flashend" || $2 == "FlashEnd" || $2 == "FLASHEND") {
    printf("/* %s */  ; removed by _conv.awk\n", $0)
    getline                     # eat 'rjmp api_call'
    printf("/* %s */  ; removed by _conv.awk\n", $0)    # and output it as comment
    next
}
$1 == ".org" { next }
#
### End project specific conversions

/\$[0-9a-fA-F]+/  { $0 = gensub(/\$([0-9a-fA-F]+)/, "0x\\1", "g") } 

$1 == ".macro" { in_macro = 1; }
$1 == ".endmacro" { $1 = "  .endm"; in_macro = 0 }
$1 == ".byte" { $1 = ".space" }  # FIXME $1 -> everywhere(?)
$1 == ".db" { $1 = ".byte" }
$1 == ".device" { next }
$1 == ".dseg" { $1 = ".section .bss" }
$1 == ".cseg" { $1 = ".section .text" }

$1 == ".set" ||
$1 == ".def" ||
$1 ~ /;?\.equ/ {
    if ($1 ~ /^;/)
        printf "// "
    $1 = ""
    split($0, args, / *= */)
    split(args[2], op_cmnt, / *; */)    
    printf("#define %s %s", args[1], op_cmnt[1])
    if (op_cmnt[2] != "")
        printf("\t// %s", op_cmnt[2])
    printf("\n")
    #if (tolower(args[1]) != args[1])
    #    printf("#define %s %s\n", tolower(args[1]), op_cmnt[1])
    if (toupper(args[1]) != args[1])
        printf("#define %s %s\n", toupper(args[1]), op_cmnt[1])
    next
}

in_macro == 0 &&
$1 ~ /^\.(if(n?def)?|include|endif|else)$/ {
    sub(/^\./, "#", $1)
    sub(/[ \t];/, " //", $0)
}

# byte masking
/high *\(/ ||
/low *\(/ {
    if ($0 ~ /2\*/) {
         gsub(/2\*/, "/* 2* */");
     }
     sub(/low[ ]*/, "lo8", $0) 
     sub(/high[ ]*/, "hi8", $0)
}

# line continuation (trailing '\'):
/\\$/ {
    sub(/[\t]*\\/, " ")
    concat_line = concat_line $0;
    next
} 

# Word operations, e.g.
# 	sbiw	twh:twl, 1
$1 ~ /w$/ && $2 ~ /:/ { gsub(/[a-zA-Z0-9_]+:/, "")}     
$2 ~ /w$/ && $3 ~ /:/ { gsub(/[a-zA-Z0-9_]+:/, "")}     


# Output what we've got so far
concat_line !~ /^$/ {
     print concat_line
     concat_line = ""
     next
}

{print}
  
