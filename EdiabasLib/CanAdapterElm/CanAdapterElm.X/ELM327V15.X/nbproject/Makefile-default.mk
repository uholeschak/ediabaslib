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
OUTPUT_SUFFIX=cof
DEBUGGABLE_SUFFIX=cof
FINAL_IMAGE=dist/${CND_CONF}/${IMAGE_TYPE}/ELM327V15.X.${IMAGE_TYPE}.${OUTPUT_SUFFIX}
else
IMAGE_TYPE=production
OUTPUT_SUFFIX=hex
DEBUGGABLE_SUFFIX=cof
FINAL_IMAGE=dist/${CND_CONF}/${IMAGE_TYPE}/ELM327V15.X.${IMAGE_TYPE}.${OUTPUT_SUFFIX}
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
SOURCEFILES_QUOTED_IF_SPACED=ELM327V15.asm

# Object Files Quoted if spaced
OBJECTFILES_QUOTED_IF_SPACED=${OBJECTDIR}/ELM327V15.o
POSSIBLE_DEPFILES=${OBJECTDIR}/ELM327V15.o.d

# Object Files
OBJECTFILES=${OBJECTDIR}/ELM327V15.o

# Source Files
SOURCEFILES=ELM327V15.asm


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

# The following macros may be used in the pre and post step lines
Device=PIC18F25K80
ProjectDir="D:\Projects\EdiabasLib\EdiabasLib\CanAdapterElm\CanAdapterElm.X\ELM327V15.X"
ConfName=default
ImagePath="dist\default\${IMAGE_TYPE}\ELM327V15.X.${IMAGE_TYPE}.${OUTPUT_SUFFIX}"
ImageDir="dist\default\${IMAGE_TYPE}"
ImageName="ELM327V15.X.${IMAGE_TYPE}.${OUTPUT_SUFFIX}"
ifeq ($(TYPE_IMAGE), DEBUG_RUN)
IsDebug="true"
else
IsDebug="false"
endif

.build-conf:  ${BUILD_SUBPROJECTS}
ifneq ($(INFORMATION_MESSAGE), )
	@echo $(INFORMATION_MESSAGE)
endif
	${MAKE}  -f nbproject/Makefile-default.mk dist/${CND_CONF}/${IMAGE_TYPE}/ELM327V15.X.${IMAGE_TYPE}.${OUTPUT_SUFFIX}
	@echo "--------------------------------------"
	@echo "User defined post-build step: [hexmate -FILL=0xFFFD@0x0800:0x7FFD -CK=0800-7FFD@7FFEw2 -FORMAT=INHX32 -O"dist/default/production/ELM327V15.X.production.hex" "dist/default/production/ELM327V15.X.production.hex"]"
	@hexmate -FILL=0xFFFD@0x0800:0x7FFD -CK=0800-7FFD@7FFEw2 -FORMAT=INHX32 -O"dist/default/production/ELM327V15.X.production.hex" "dist/default/production/ELM327V15.X.production.hex"
	@echo "--------------------------------------"

MP_PROCESSOR_OPTION=18f25k80
MP_LINKER_DEBUG_OPTION= 
# ------------------------------------------------------------------------------------
# Rules for buildStep: assemble
ifeq ($(TYPE_IMAGE), DEBUG_RUN)
${OBJECTDIR}/ELM327V15.o: ELM327V15.asm  nbproject/Makefile-${CND_CONF}.mk
	@${MKDIR} "${OBJECTDIR}" 
	@${RM} ${OBJECTDIR}/ELM327V15.o.d 
	@${RM} ${OBJECTDIR}/ELM327V15.o 
	@${FIXDEPS} dummy.d -e "D:/Projects/EdiabasLib/EdiabasLib/CanAdapterElm/CanAdapterElm.X/ELM327V15.X/ELM327V15.ERR" $(SILENT) -c ${MP_AS} $(MP_EXTRA_AS_PRE) -d__DEBUG -d__MPLAB_DEBUGGER_PK3=1 -q -p$(MP_PROCESSOR_OPTION)  $(ASM_OPTIONS)    \"D:/Projects/EdiabasLib/EdiabasLib/CanAdapterElm/CanAdapterElm.X/ELM327V15.X/ELM327V15.asm\" 
	@${MV}  D:/Projects/EdiabasLib/EdiabasLib/CanAdapterElm/CanAdapterElm.X/ELM327V15.X/ELM327V15.O ${OBJECTDIR}/ELM327V15.o
	@${MV}  D:/Projects/EdiabasLib/EdiabasLib/CanAdapterElm/CanAdapterElm.X/ELM327V15.X/ELM327V15.ERR ${OBJECTDIR}/ELM327V15.o.err
	@${MV}  D:/Projects/EdiabasLib/EdiabasLib/CanAdapterElm/CanAdapterElm.X/ELM327V15.X/ELM327V15.LST ${OBJECTDIR}/ELM327V15.o.lst
	@${RM}  D:/Projects/EdiabasLib/EdiabasLib/CanAdapterElm/CanAdapterElm.X/ELM327V15.X/ELM327V15.HEX 
	@${DEP_GEN} -d "${OBJECTDIR}/ELM327V15.o"
	@${FIXDEPS} "${OBJECTDIR}/ELM327V15.o.d" $(SILENT) -rsi ${MP_AS_DIR} -c18 
	
else
${OBJECTDIR}/ELM327V15.o: ELM327V15.asm  nbproject/Makefile-${CND_CONF}.mk
	@${MKDIR} "${OBJECTDIR}" 
	@${RM} ${OBJECTDIR}/ELM327V15.o.d 
	@${RM} ${OBJECTDIR}/ELM327V15.o 
	@${FIXDEPS} dummy.d -e "D:/Projects/EdiabasLib/EdiabasLib/CanAdapterElm/CanAdapterElm.X/ELM327V15.X/ELM327V15.ERR" $(SILENT) -c ${MP_AS} $(MP_EXTRA_AS_PRE) -q -p$(MP_PROCESSOR_OPTION)  $(ASM_OPTIONS)    \"D:/Projects/EdiabasLib/EdiabasLib/CanAdapterElm/CanAdapterElm.X/ELM327V15.X/ELM327V15.asm\" 
	@${MV}  D:/Projects/EdiabasLib/EdiabasLib/CanAdapterElm/CanAdapterElm.X/ELM327V15.X/ELM327V15.O ${OBJECTDIR}/ELM327V15.o
	@${MV}  D:/Projects/EdiabasLib/EdiabasLib/CanAdapterElm/CanAdapterElm.X/ELM327V15.X/ELM327V15.ERR ${OBJECTDIR}/ELM327V15.o.err
	@${MV}  D:/Projects/EdiabasLib/EdiabasLib/CanAdapterElm/CanAdapterElm.X/ELM327V15.X/ELM327V15.LST ${OBJECTDIR}/ELM327V15.o.lst
	@${RM}  D:/Projects/EdiabasLib/EdiabasLib/CanAdapterElm/CanAdapterElm.X/ELM327V15.X/ELM327V15.HEX 
	@${DEP_GEN} -d "${OBJECTDIR}/ELM327V15.o"
	@${FIXDEPS} "${OBJECTDIR}/ELM327V15.o.d" $(SILENT) -rsi ${MP_AS_DIR} -c18 
	
endif

# ------------------------------------------------------------------------------------
# Rules for buildStep: link
ifeq ($(TYPE_IMAGE), DEBUG_RUN)
dist/${CND_CONF}/${IMAGE_TYPE}/ELM327V15.X.${IMAGE_TYPE}.${OUTPUT_SUFFIX}: ${OBJECTFILES}  nbproject/Makefile-${CND_CONF}.mk    
	@${MKDIR} dist/${CND_CONF}/${IMAGE_TYPE} 
	${MP_LD} $(MP_EXTRA_LD_PRE)   -p$(MP_PROCESSOR_OPTION)  -w -x -u_DEBUG -z__ICD2RAM=1 -m"${DISTDIR}/${PROJECTNAME}.${IMAGE_TYPE}.map"   -z__MPLAB_BUILD=1  -z__MPLAB_DEBUG=1 -z__MPLAB_DEBUGGER_PK3=1 $(MP_LINKER_DEBUG_OPTION) -odist/${CND_CONF}/${IMAGE_TYPE}/ELM327V15.X.${IMAGE_TYPE}.${OUTPUT_SUFFIX}  ${OBJECTFILES_QUOTED_IF_SPACED}     
else
dist/${CND_CONF}/${IMAGE_TYPE}/ELM327V15.X.${IMAGE_TYPE}.${OUTPUT_SUFFIX}: ${OBJECTFILES}  nbproject/Makefile-${CND_CONF}.mk   
	@${MKDIR} dist/${CND_CONF}/${IMAGE_TYPE} 
	${MP_LD} $(MP_EXTRA_LD_PRE)   -p$(MP_PROCESSOR_OPTION)  -w  -m"${DISTDIR}/${PROJECTNAME}.${IMAGE_TYPE}.map"   -z__MPLAB_BUILD=1  -odist/${CND_CONF}/${IMAGE_TYPE}/ELM327V15.X.${IMAGE_TYPE}.${DEBUGGABLE_SUFFIX}  ${OBJECTFILES_QUOTED_IF_SPACED}     
endif


# Subprojects
.build-subprojects:


# Subprojects
.clean-subprojects:

# Clean Targets
.clean-conf: ${CLEAN_SUBPROJECTS}
	${RM} -r build/default
	${RM} -r dist/default

# Enable dependency checking
.dep.inc: .depcheck-impl

DEPFILES=$(shell mplabwildcard ${POSSIBLE_DEPFILES})
ifneq (${DEPFILES},)
include ${DEPFILES}
endif
