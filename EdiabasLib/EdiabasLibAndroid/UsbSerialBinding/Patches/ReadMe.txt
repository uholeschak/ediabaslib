Set JAVA_HOME to JDK installation directory first

Build:
gradlew.bat clean
gradlew.bat build
extract (7zip) classes.jar from: usbSerialForAndroid\build\outputs\aar\usbSerialForAndroid-release.aar

Merge master modifications into branch:
git rebase origin/master
