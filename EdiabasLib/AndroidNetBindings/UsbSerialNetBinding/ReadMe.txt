Clone git source:
https://github.com/mik3y/usb-serial-for-android.git

Set JAVA_HOME to JDK installation directory first, currently:
JAVA_HOME=C:\Program Files (x86)\Android\openjdk\jdk-17.0.12

Build:
gradlew.bat clean
gradlew.bat build
copy build\outputs\aar\usbSerialForAndroid-release.aar to UsbSerialBinding
