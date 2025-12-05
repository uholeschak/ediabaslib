Clone git source:
https://github.com/mik3y/usb-serial-for-android.git

This required the old JDK 17 to build the project.
set JAVA_HOME=C:\Program Files (x86)\Android\openjdk\jdk-17.0.14

Build:
gradlew.bat clean
gradlew.bat build
copy build\outputs\aar\usbSerialForAndroid-release.aar to UsbSerialBinding
