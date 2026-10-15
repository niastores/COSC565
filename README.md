A Makefile rule has a target, prerequisites, and a recipe. make rebuilds a target only when a prerequisite is newer, so only changed files are recompiled. In the C example, the order is object files, then the archive, then the executable. 

My Makefile is designed to express the same ordering: a lib target for the shared library, and an app target that depends on it. The other targets (run, publish, clean) wrap dotnet commands, and variables hold project paths and settings. 


**How to build and run
**

make creates a build directory with bin (main program) and lib (common library)

    make -f ClassScheduler.mk

make run SCHEDULE=<file> SYLLABUS=<file> passes the two input files to the program which 
writes the Word, HTML, Excel, and CSV schedules and a review-notes.txt file to the output directory

    make -f ClassScheduler.mk run SCHEDULE="ClassScheduler/DATA/schedule.xlsx"  SYLLABUS="ClassScheduler/DATA/syllabus.docx"

SAMPLE RUN OUTPUT

    dotnet restore /Project/ClassScheduler/ClassScheduler.csproj
    Restore complete (1.2s)
    
    Build succeeded in 2.0s
    dotnet build /Project/ClassSchedulerCommon/ClassSchedulerCommon.csproj -c Release --no-restore -o build/lib
      ClassSchedulerCommon net10.0 succeeded (0.5s) → build/lib/ClassSchedulerCommon.dll
    
    Build succeeded in 1.3s
    dotnet build /Project/ClassScheduler/ClassScheduler.csproj -c Release --no-restore
      ClassSchedulerCommon net10.0 succeeded (0.4s) → ClassSchedulerCommon/bin/Release/net10.0/ClassSchedulerCommon.dll
      ClassScheduler net10.0 succeeded (0.5s) → ClassScheduler/bin/Release/net10.0/ClassScheduler.dll
    
    
    Build succeeded in 1.9s
    mkdir -p build/bin
    cp -r /Project/ClassScheduler/bin/Release/net10.0/. build/bin/
    dotnet build/bin/ClassScheduler.dll "ClassScheduler/DATA/schedule.xlsx" "ClassScheduler/DATA/syllabus.docx"
    [INFO ] Syllabus: /Project/ClassScheduler/DATA/schedule.xlsx
    [INFO ] Faculty:  /Project/ClassScheduler/DATA/syllabus.docx
    [WARN ] Repaired/flagged: Parse issue (line 13, row): CTEC 711 555: Invalid times '5:00 PM to 5:00 PM' (end is not after start); no meeting times recorded.
    [WARN ] Repaired/flagged: Parse issue (line 15, row): CTEC 721 555: End time 9:50 AM is before start 7:00 PM; assumed 9:50 PM. Please verify.
    [WARN ] Repaired/flagged: Parse issue (line 25, row): CTEC 120 170: End time 3:15 AM is before start 12:00 PM; assumed 3:15 PM. Please verify.
    [WARN ] Repaired/flagged: Parse issue (line 26, row): CTEC 125 555: Could not read days ''.
    [WARN ] Repaired/flagged: Parse issue (line 27, row): CTEC 125 556: Could not read days ''.
    [WARN ] 'Adedoyin, Anthony' is not in the faculty file; created a placeholder (section CTEC 350 170).
    [WARN ] 'Agboli, P.' is not in the faculty file; created a placeholder (section CTEC 120 171).
    [WARN ] 'Baptiste, R.' is not in the faculty file; created a placeholder (section CTEC 214 180).
    [WARN ] 'Bemley' is not in the faculty file; created a placeholder (section CTEC 128 170).
    [WARN ] 'Bemley, J' matched to 'Bemley' by surname/initial. Please verify.
    [WARN ] 'Bemley, Jesse' matched to 'Bemley' by surname/initial. Please verify.
    [WARN ] 'Ruth Agada' is not in the faculty file; created a placeholder (section CTEC 711 555).
    [WARN ] 'Bright, Marvin' is not in the faculty file; created a placeholder (section CTEC 720 555).
    [WARN ] 'Carter, Adrian D.' is not in the faculty file; created a placeholder (section CTEC 424 555).
    [WARN ] 'Chery, Pierrot' is not in the faculty file; created a placeholder (section CTEC 305 170).
    [WARN ] 'Deal, Shawn' is not in the faculty file; created a placeholder (section CTEC 226 180).
    [WARN ] 'Dixon, Carl' is not in the faculty file; created a placeholder (section CTEC 294 170).
    [WARN ] 'Dr. Stone, D. B.' is not in the faculty file; created a placeholder (section CTEC 120 170).
    [WARN ] 'Eugene Harris' is not in the faculty file; created a placeholder (section CTEC 111 172).
    [WARN ] 'Eugene T. Harris' matched to 'Eugene Harris' by surname/initial. Please verify.
    [WARN ] 'Francis, Wilbert R.' is not in the faculty file; created a placeholder (section CTEC 475 180).
    [WARN ] 'Henri E. Cooper' is not in the faculty file; created a placeholder (section CTEC 111 173).
    [WARN ] 'Staphord Bengesi' is not in the faculty file; created a placeholder (section CTEC 111 171).
    [WARN ] 'Lamar, Courtney' is not in the faculty file; created a placeholder (section CTEC 393 555).
    [WARN ] 'Miller, Stephanie R.' is not in the faculty file; created a placeholder (section CTEC 426 170).
    [WARN ] 'Njeh, Felix' is not in the faculty file; created a placeholder (section CTEC 651 555).
    [WARN ] 'Okonofua, Henry' is not in the faculty file; created a placeholder (section CTEC 410 170).
    [WARN ] 'Roberts, Curtis' is not in the faculty file; created a placeholder (section CTEC 222 170).
    [WARN ] 'Brett Tolbert' is not in the faculty file; created a placeholder (section CTEC 345 170).
    [WARN ] 'Loubna Dali' is not in the faculty file; created a placeholder (section CTEC 450 180).
    [WARN ] 'Vega, J.' is not in the faculty file; created a placeholder (section CTEC 220 170).
    [WARN ] 'Victoria Lee' is not in the faculty file; created a placeholder (section CTEC 114 170).
    [WARN ] 'Wilson, Marcella J.' is not in the faculty file; created a placeholder (section CTEC 128 180).
    [WARN ] Dr. Appolo Tankeh has no sections in the syllabus.
    [WARN ] Bemley: CTEC 298 overlaps CTEC 671 on Tuesday.
    [WARN ] Bemley: CTEC 298 overlaps CTEC 671 on Thursday.
    [WARN ] Ruth Agada: CTEC 402 overlaps CTEC 435 on Monday.
    [WARN ] Ruth Agada: CTEC 402 overlaps CTEC 435 on Wednesday.
    [WARN ] Wilson, Marcella J.: CTEC 230 overlaps CTEC 415 on Tuesday.
    [WARN ] Wilson, Marcella J.: CTEC 230 overlaps CTEC 415 on Thursday.
    [WARN ] Room  108 double-booked on Monday: CTEC 435 and CTEC 435.
    [WARN ] Room  108 double-booked on Wednesday: CTEC 435 and CTEC 435.
    [WARN ] Room  109 double-booked on Monday: CTEC 445 and CTEC 345.
    [WARN ] Room  109 double-booked on Wednesday: CTEC 445 and CTEC 345.
    [WARN ] Room  111 double-booked on Monday: CTEC 445 and CTEC 345.
    [WARN ] Room  111 double-booked on Wednesday: CTEC 445 and CTEC 345.
    [WARN ] Room  111 double-booked on Monday: CTEC 450 and CTEC 345.
    [WARN ] Room  111 double-booked on Monday: CTEC 450 and CTEC 450.
    [WARN ] Room  111 double-booked on Wednesday: CTEC 450 and CTEC 345.
    [WARN ] Room  111 double-booked on Wednesday: CTEC 450 and CTEC 450.
    [WARN ] Room  111 double-booked on Monday: CTEC 345 and CTEC 450.
    [WARN ] Room  111 double-booked on Wednesday: CTEC 345 and CTEC 450.
    [WARN ] Room  114 double-booked on Monday: CTEC 475 and CTEC 426.
    [WARN ] Room  114 double-booked on Wednesday: CTEC 475 and CTEC 426.
    [WARN ] Room  114 double-booked on Monday: CTEC 230 and CTEC 293.
    [WARN ] Room  114 double-booked on Wednesday: CTEC 230 and CTEC 293.
    [WARN ] Room  114 double-booked on Tuesday: CTEC 230 and CTEC 415.
    [WARN ] Room  114 double-booked on Thursday: CTEC 230 and CTEC 415.
    [INFO ] Done: 0 errors, 0 warnings. Output in /Project/output
    Output written to output/
    ns@ns-VirtualBox:~/Desktop/Masters/Project$ 
