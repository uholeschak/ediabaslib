The patch ist not required for the current git version from:
https://github.com/mik3y/usb-serial-for-android.git
any more.

Set JAVA_HOME to JDK installation directory first

Build:
gradlew.bat clean
gradlew.bat build
copy usbSerialForAndroid\build\outputs\aar\usbSerialForAndroid-release.aar to UsbSerialBinding\Jars

Merge master modifications into branch:
git rebase origin/master
