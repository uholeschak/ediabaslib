#-------------------------------------------------
#
# Project created by QtCreator 2010-08-02T16:28:22
#
#-------------------------------------------------

QT       -= gui
TARGET = QextSerialPort
TEMPLATE = lib
CONFIG += staticlib
QMAKE_CXXFLAGS_RELEASE = -Os
SOURCES += qextserialenumerator.cpp \
    qextserialport.cpp \
    qextserialbase.cpp \

HEADERS += qextserialenumerator.h \
    qextserialport.h \
    qextserialbase.h \

unix {
    HEADERS += posix_qextserialport.h
    SOURCES += posix_qextserialport.cpp
    DEFINES += _TTY_POSIX_
}
win32 {
    HEADERS += win_qextserialport.h
    SOURCES += win_qextserialport.cpp
    DEFINES += _TTY_WIN_
}
