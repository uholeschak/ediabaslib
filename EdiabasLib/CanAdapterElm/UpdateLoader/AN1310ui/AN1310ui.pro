TARGET = "AN1310ui"
TEMPLATE = app
QT += sql
QMAKE_CXXFLAGS_RELEASE = -Os
INCLUDEPATH += ../
SOURCES += main.cpp \
    MainWindow.cpp \
    Settings.cpp \
    FlashViewModel.cpp \
    EepromViewModel.cpp \
    QSerialTerminal.cpp \
    ConfigBitsItem.cpp \
    ConfigBitsDelegate.cpp \
    ConfigBitsView.cpp \
    HexExporter.cpp
HEADERS += MainWindow.h \
    Settings.h \
    FlashViewModel.h \
    EepromViewModel.h \
    QSerialTerminal.h \
    ConfigBitsItem.h \
    ConfigBitsDelegate.h \
    ConfigBitsView.h \
    ../version.h \
    HexExporter.h
unix { 
    DEFINES += _TTY_POSIX_
    LIBS += -L../QextSerialPort
    LIBS += -L../Bootload
    LIBS += -lBootload \
        -lQextSerialPort
}
win32 { 
    DEFINES += _TTY_WIN_
    CONFIG(debug)
     { 
        LIBS += -L../QextSerialPort/debug
        LIBS += -L../Bootload/debug
        OTHER_FILES += ../Bootload/debug/libBootload.a
    }
    CONFIG(release)
     { 
        LIBS += -L../QextSerialPort/release
        LIBS += -L../Bootload/release
        OTHER_FILES += ../Bootload/release/libBootload.a
    }
    LIBS += -lBootload \
        -lQextSerialPort
    LIBS += -lsetupapi
    RC_FILE = windows.rc
}
FORMS += MainWindow.ui \
    Settings.ui
RESOURCES += resources.qrc
OTHER_FILES += windows.rc
