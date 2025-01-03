Clone git source:
https://github.com/woxblom/DragListView.git

This required the old JDK 11 to build the project.
set JAVA_HOME=C:\Program Files\Java\jdk-11.0.17

Build:
gradlew.bat clean
gradlew.bat build
copy build\outputs\aar\CustomGauge-release.aar to CustomGaugeNetBinding
