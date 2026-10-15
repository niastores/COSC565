# ClassScheduler build file.
# Requires: GNU make and the .NET SDK (dotnet) on the PATH.
# On Windows, run from Git Bash or WSL (the recipes use mkdir -p and rm -rf).

DOTNET   ?= dotnet
CONFIG   ?= Release

# Project locations. Change these if your folders differ.
SRC_DIR  ?= /home/ns/Desktop/Masters/Project
LIB_PROJ ?= $(SRC_DIR)/ClassSchedulerCommon/ClassSchedulerCommon.csproj
APP_PROJ ?= $(SRC_DIR)/ClassScheduler/ClassScheduler.csproj
#TEST_PROJ?= tests/ClassScheduler.Tests/ClassScheduler.Tests.csproj

# Build directories.
BUILD_DIR   = build
LIB_OUT     = $(BUILD_DIR)/lib
BIN_DIR     = $(BUILD_DIR)/bin
PUBLISH_DIR = $(BUILD_DIR)/publish
OUTPUT_DIR  = output

# Inputs for "make run". Input 1 = class schedule (.xlsx, .csv, .json).
# Input 2 = professor information (.docx or .pdf syllabus, or .csv, .json, .xlsx).
SCHEDULE ?=
SYLLABUS ?=

APP_DLL = $(BIN_DIR)/ClassScheduler.dll

.PHONY: all restore lib app link run test publish clean help

# Default: compile everything (library first, then the program linked against it).
all: app

help:
	@echo "make            build the static library and the program"
	@echo "make lib        build only the static library (ClassScheduler.Common)"
	@echo "make app        build the program and link the library into it"
	@echo "make run SCHEDULE=<schedule.xlsx> SYLLABUS=<syllabus.docx>"
	@echo "                run the program and generate the final output in $(OUTPUT_DIR)/"
	@echo "make test       run the unit tests"
	@echo "make publish    create a self-contained single-file executable"
	@echo "make clean      remove build output"

# Step 0: restore NuGet packages for the whole dependency graph.
restore:
	$(DOTNET) restore $(APP_PROJ)

# Step 1: build the static library on its own.
# Output: build/lib/ClassSchedulerCommon.dll
lib: restore
	$(DOTNET) build $(LIB_PROJ) -c $(CONFIG) --no-restore -o $(LIB_OUT)

# Step 2: compile the program. The Main project references Common
# (and the other modules) with <ProjectReference>, so this step links the
# library: the ClassSchedulerCommon DLL is copied into bin with the other executables.
app: lib
	$(DOTNET) build $(APP_PROJ) -c $(CONFIG) --no-restore
	mkdir -p $(BIN_DIR)
	cp -r $(dir $(APP_PROJ))bin/$(CONFIG)/net10.0/. $(BIN_DIR)/

link: app

# Step 3: generate the final output by running the program on the two inputs.
# Run from the repository root so "output/" is created here.
run: app
	@test -n "$(SCHEDULE)" -a -n "$(SYLLABUS)" \
	  || (echo 'Usage: make run SCHEDULE="path/schedule.xlsx" SYLLABUS="path/syllabus.docx"'; exit 1)
	$(DOTNET) $(APP_DLL) "$(SCHEDULE)" "$(SYLLABUS)"
	@echo "Output written to $(OUTPUT_DIR)/"

test: restore
	$(DOTNET) test $(TEST_PROJ) -c $(CONFIG)

# Optional: merge everything, including the library, into one executable file.
# Change the runtime identifier (-r) for your platform (win-x64, linux-x64, osx-arm64).
RID ?= win-x64
publish: restore
	$(DOTNET) publish $(APP_PROJ) -c $(CONFIG) -r $(RID) --self-contained true \
	  -p:PublishSingleFile=true -o $(PUBLISH_DIR)

clean:
	$(DOTNET) clean $(APP_PROJ) -c $(CONFIG) || true
	rm -rf $(BUILD_DIR) $(OUTPUT_DIR)
	find $(SRC_DIR) tests -type d \( -name bin -o -name obj \) -prune -exec rm -rf {} + 2>/dev/null || true