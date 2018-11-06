# Makefile

CC=msbuild
CFLAGS=/p:Configuration=Release /v:m

SRC_DIR=LeekWarsAPI/
SLN=${SRC_DIR}LeekWarsAPI.sln
BIN_DIR=bin/

MKDIR=mkdir -p
RM=rm -rf
CP=cp -r

all: build
	
build: $(BIN_DIR) restore
	@cd ${SRC_DIR} && ${CC} ${CFLAGS} 
	${CP} $(CURRENT_DIR)${SRC_DIR}bin/Release/* ${BIN_DIR}

restore:
	nuget restore ${SLN}

${BIN_DIR}:
	${MKDIR} ${BIN_DIR}
	
clean: clean-build
	${RM} ${BIN_DIR}

clean-build:
	${RM} ${SRC_DIR}bin/
	${RM} ${SRC_DIR}obj/	

# end