Set JAVA_HOME to JDK installation directory first

Build:
gradlew.bat clean
gradlew.bat build
copy usbSerialForAndroid\build\outputs\aar\usbSerialForAndroid-release.aar to UsbSerialBinding\Jars

Merge master modifications into branch:
git rebase origin/master
