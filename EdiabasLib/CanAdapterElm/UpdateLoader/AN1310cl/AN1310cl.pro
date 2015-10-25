# -------------------------------------------------
# Project created by QtCreator 2010-08-02T17:44:13
# -------------------------------------------------
QT += sql
QT -= gui
TARGET = AN1310cl
CONFIG += console
CONFIG -= app_bundle
TEMPLATE = app
QMAKE_CXXFLAGS_RELEASE = -Os
INCLUDEPATH += ../
SOURCES += main.cpp \
    Bootload.cpp
unix {
    DEFINES += _TTY_POSIX_
    LIBS += -L../QextSerialPort
    LIBS += -L../Bootload
}

win32 { 
    DEFINES += _TTY_WIN_
    LIBS += -lsetupapi
    CONFIG(debug) {
        LIBS += -L../QextSerialPort/debug
        LIBS += -L../Bootload/debug
    }
    CONFIG(release) {
        LIBS += -L../QextSerialPort/release
        LIBS += -L../Bootload/release
    }
}
LIBS += -lBootload \
    -lQextSerialPort
HEADERS += Bootload.h
