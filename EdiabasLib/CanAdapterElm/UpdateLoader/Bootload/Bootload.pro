#-------------------------------------------------
#
# Project created by QtCreator 2010-08-02T16:40:28
#
#-------------------------------------------------

QT -= gui
QT += sql
TARGET = Bootload
TEMPLATE = lib
CONFIG += staticlib
QMAKE_CXXFLAGS_RELEASE = -Os
INCLUDEPATH += ../
SOURCES += ImportExportHex.cpp \
    DeviceWriter.cpp \
    DeviceWritePlanner.cpp \
    DeviceVerifyPlanner.cpp \
    DeviceVerifier.cpp \
    DeviceSqlLoader.cpp \
    DeviceReader.cpp \
    DeviceData.cpp \
    Device.cpp \
    Crc.cpp \
    Comm.cpp \
    BootPackets.cpp
HEADERS += ImportExportHex.h \
    DeviceWriter.h \
    DeviceWritePlanner.h \
    DeviceVerifyPlanner.h \
    DeviceVerifier.h \
    DeviceSqlLoader.h \
    DeviceReader.h \
    DeviceData.h \
    Device.h \
    Crc.h \
    Comm.h \
    BootPackets.h
unix:DEFINES += _TTY_POSIX_
win32:DEFINES += _TTY_WIN_
