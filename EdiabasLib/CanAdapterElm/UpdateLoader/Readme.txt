To build the host PC application source code for the Serial Bootloader, follow 
these steps:

1. You first need to download and install the Qt 4.6.1 SDK. A copy of this 
software can be found at:

Windows      - ftp://ftp.qt.nokia.com/qtsdk/qt-sdk-win-opensource-2010.01.exe
32-bit Linux - ftp://ftp.qt.nokia.com/qtsdk/qt-sdk-linux-x86-opensource-2010.01.bin
64-bit Linux - ftp://ftp.qt.nokia.com/qtsdk/qt-sdk-linux-x86_64-opensource-2010.01.bin

The AN1310 installation package incorporates re-compiled Qt runtime DLLs 
optimized to minimize file size and memory footprint. However, none of 
the Qt source code was modified and the original Qt DLLs can be substituted 
without recompiling the application.

2. Once the Qt SDK is installed, run the "Qt Creator" program and open the 
"QextSerialPort/QextSerialPort.pro" project file. 

Build this project twice; use the "Build" -> "Set Build Configuration" menu options to select
"Debug" and "Release" mode for each build. 

3. Open the "Bootload/Bootload.pro" project file. Build this project for Debug and Release
mode as well.

4. Open the "AN1310ui/AN1310ui.pro" project file. Build and run this project for the
graphical user interface enabled version of the host PC bootloader software.

5. Open the "AN1310cl/AN1310cl.pro" project. Build and run this project for
the command line only version of the host PC bootloader software.

-------------------------------------------------------------------------------

How to re-compile the Qt DLLs optimized for minimum size yourself:

1. Edit the file C:\Qt\20XX.XX\qt\mkspecs\win32-g++\qmake.conf. Change
the definition for QMAKE_CFLAGS_RELEASE so that the "-Os" compiler 
optimization level is used instead of "-O2"

2. Open a command prompt (Start->Run->"cmd") and run the batch file:

	C:\Qt\20XX.XX\qt\bin>qtenv.bat

   Do not close this command prompt, use it for all the remaining steps.

3. Run the configure tool with these options:

	C:\Qt\20XX.XX\qt>configure -release -platform win32-g++ 
		-nomake examples -nomake demos 
		-no-stl -no-qt3support -no-scripttools -no-openssl 
		-no-webkit -no-phonon -plugin-sql-sqlite -no-opengl 
		-no-dsp -no-vcproj -no-dbus -no-phonon-backend 
		-no-multimedia -no-audio-backend -no-script 
		-no-declarative -no-style-plastique -no-style-cleanlooks 
		-no-style-motif -no-style-cde -opensource

4. Run the build with:

	C:\Qt\20XX.XX\qt>mingw32-make

