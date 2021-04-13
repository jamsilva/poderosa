# Makefile for CygTerm

BINDIR = $(HOME)/bin

CC = gcc
CFLAGS = -O2 -fno-exceptions
#CFLAGS = -g -fno-exceptions
LDFLAGS = -mwindows

EXE = cygterm.exe
SRC = $(EXE:.exe=.cc)
CFG = $(EXE:.exe=.cfg)

all : $(EXE)

$(EXE) : $(SRC) $(RES)
  ifeq (0, $(shell nm /usr/lib/crt0.o | grep -c WinMainCRTStartup))
	$(CC) $(CFLAGS) $(LDFLAGS) -DNO_WIN_MAIN -o $(EXE) $(SRC)
  else
	$(CC) $(CFLAGS) $(LDFLAGS) -o $(EXE) $(SRC)
  endif
	strip $(EXE)

clean :
	rm -f $(EXE)

install : $(EXE)
	@ install -v $(EXE) $(BINDIR)/$(EXE)
	@ if [ ! -f $(BINDIR)/$(CFG) ]; then \
	    install -v $(CFG) $(BINDIR)/$(CFG) \
	; fi

uninstall :
	rm -f $(BINDIR)/$(EXE)
	rm -f $(BINDIR)/$(CFG)

