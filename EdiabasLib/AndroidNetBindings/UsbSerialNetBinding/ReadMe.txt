Clone git source:
https://github.com/mik3y/usb-serial-for-android.git

Set JAVA_HOME to JDK installation directory first, currently (newer version is not working):
JAVA_HOME=C:\Program Files (x86)\Android\openjdk\jdk-17.0.14

Build:
gradlew.bat clean
gradlew.bat build
copy build\outputs\aar\usbSerialForAndroid-release.aar to UsbSerialBinding
