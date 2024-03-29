#
# Generated Makefile - do not edit!
#
# Edit the Makefile in the project folder instead (../Makefile). Each target
# has a -pre and a -post target defined where you can add customized code.
#
# This makefile implements configuration specific macros and targets.


# Include project Makefile
ifeq "${IGNORE_LOCAL}" "TRUE"
# do not include local makefile. User is passing all local related variables already
else
include Makefile
# Include makefile containing local settings
ifeq "$(wildcard nbproject/Makefile-local-default.mk)" "nbproject/Makefile-local-default.mk"
include nbproject/Makefile-local-default.mk
endif
endif

# Environment
MKDIR=gnumkdir -p
RM=rm -f 
MV=mv 
CP=cp 

# Macros
CND_CONF=default
ifeq ($(TYPE_IMAGE), DEBUG_RUN)
IMAGE_TYPE=debug
OUTPUT_SUFFIX=hex
DEBUGGABLE_SUFFIX=elf
FINAL_IMAGE=${DISTDIR}/ELM327V23.X.${IMAGE_TYPE}.${OUTPUT_SUFFIX}
else
IMAGE_TYPE=production
OUTPUT_SUFFIX=hex
DEBUGGABLE_SUFFIX=elf
FINAL_IMAGE=${DISTDIR}/ELM327V23.X.${IMAGE_TYPE}.${OUTPUT_SUFFIX}
endif

ifeq ($(COMPARE_BUILD), true)
COMPARISON_BUILD=
else
COMPARISON_BUILD=
endif

ifdef SUB_IMAGE_ADDRESS

else
SUB_IMAGE_ADDRESS_COMMAND=
endif

# Object Directory
OBJECTDIR=build/${CND_CONF}/${IMAGE_TYPE}

# Distribution Directory
DISTDIR=dist/${CND_CONF}/${IMAGE_TYPE}

# Source Files Quoted if spaced
SOURCEFILES_QUOTED_IF_SPACED=ELM327V23.asm

# Object Files Quoted if spaced
OBJECTFILES_QUOTED_IF_SPACED=${OBJECTDIR}/ELM327V23.o
POSSIBLE_DEPFILES=${OBJECTDIR}/ELM327V23.o.d

# Object Files
OBJECTFILES=${OBJECTDIR}/ELM327V23.o

# Source Files
SOURCEFILES=ELM327V23.asm



CFLAGS=
ASFLAGS=
LDLIBSOPTIONS=

############# Tool locations ##########################################
# If you copy a project from one host to another, the path where the  #
# compiler is installed may be different.                             #
# If you open this project with MPLAB X in the new host, this         #
# makefile will be regenerated and the paths will be corrected.       #
#######################################################################
# fixDeps replaces a bunch of sed/cat/printf statements that slow down the build
FIXDEPS=fixDeps

.build-conf:  ${BUILD_SUBPROJECTS}
ifneq ($(INFORMATION_MESSAGE), )
	@echo $(INFORMATION_MESSAGE)
endif
	${MAKE}  -f nbproject/Makefile-default.mk ${DISTDIR}/ELM327V23.X.${IMAGE_TYPE}.${OUTPUT_SUFFIX}

MP_PROCESSOR_OPTION=PIC18F25K80
FINAL_IMAGE_NAME_MINUS_EXTENSION=${DISTDIR}/ELM327V23.X.${IMAGE_TYPE}
# ------------------------------------------------------------------------------------
# Rules for buildStep: pic-as-assembler
ifeq ($(TYPE_IMAGE), DEBUG_RUN)
${OBJECTDIR}/ELM327V23.o: ELM327V23.asm  nbproject/Makefile-${CND_CONF}.mk 
	@${MKDIR} "${OBJECTDIR}" 
	@${RM} ${OBJECTDIR}/ELM327V23.o 
	${MP_AS} -mcpu=PIC18F25K80 -c \
	-o ${OBJECTDIR}/ELM327V23.o \
	ELM327V23.asm \
	 -D__DEBUG=1  -misa=std -msummary=+mem,-psect,-class,-hex,-file,-sha1,-sha256,-xml,-xmlfull -fmax-errors=20 -mwarn=0 -DADAPTER_TYPE=0x02 -xassembler-with-cpp --msgdisable=1522
	
else
${OBJECTDIR}/ELM327V23.o: ELM327V23.asm  nbproject/Makefile-${CND_CONF}.mk 
	@${MKDIR} "${OBJECTDIR}" 
	@${RM} ${OBJECTDIR}/ELM327V23.o 
	${MP_AS} -mcpu=PIC18F25K80 -c \
	-o ${OBJECTDIR}/ELM327V23.o \
	ELM327V23.asm \
	 -misa=std -msummary=+mem,-psect,-class,-hex,-file,-sha1,-sha256,-xml,-xmlfull -fmax-errors=20 -mwarn=0 -DADAPTER_TYPE=0x02 -xassembler-with-cpp --msgdisable=1522
	
endif

# ------------------------------------------------------------------------------------
# Rules for buildStep: pic-as-linker
ifeq ($(TYPE_IMAGE), DEBUG_RUN)
${DISTDIR}/ELM327V23.X.${IMAGE_TYPE}.${OUTPUT_SUFFIX}: ${OBJECTFILES}  nbproject/Makefile-${CND_CONF}.mk    
	@${MKDIR} ${DISTDIR} 
	${MP_LD} -mcpu=PIC18F25K80 ${OBJECTFILES_QUOTED_IF_SPACED} \
	-o ${DISTDIR}/ELM327V23.X.${IMAGE_TYPE}.${OUTPUT_SUFFIX} \
	 -D__DEBUG=1  -misa=std -msummary=+mem,-psect,-class,-hex,-file,-sha1,-sha256,-xml,-xmlfull -mcallgraph=std -Wl,-Map=${FINAL_IMAGE_NAME_MINUS_EXTENSION}.map -mno-download-hex
else
${DISTDIR}/ELM327V23.X.${IMAGE_TYPE}.${OUTPUT_SUFFIX}: ${OBJECTFILES}  nbproject/Makefile-${CND_CONF}.mk   
	@${MKDIR} ${DISTDIR} 
	${MP_LD} -mcpu=PIC18F25K80 ${OBJECTFILES_QUOTED_IF_SPACED} \
	-o ${DISTDIR}/ELM327V23.X.${IMAGE_TYPE}.${OUTPUT_SUFFIX} \
	 -misa=std -msummary=+mem,-psect,-class,-hex,-file,-sha1,-sha256,-xml,-xmlfull -mcallgraph=std -Wl,-Map=${FINAL_IMAGE_NAME_MINUS_EXTENSION}.map -mno-download-hex
endif


# Subprojects
.build-subprojects:


# Subprojects
.clean-subprojects:

# Clean Targets
.clean-conf: ${CLEAN_SUBPROJECTS}
	${RM} -r ${OBJECTDIR}
	${RM} -r ${DISTDIR}

# Enable dependency checking
.dep.inc: .depcheck-impl

DEPFILES=$(shell mplabwildcard ${POSSIBLE_DEPFILES})
ifneq (${DEPFILES},)
include ${DEPFILES}
endif
