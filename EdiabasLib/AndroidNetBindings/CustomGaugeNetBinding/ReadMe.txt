Clone git source:
https://github.com/woxblom/DragListView.git

Apply patch CustomGauge.patch:
Copy CustomGauge.patch to CustomGauge folder
cd CustomGauge
git apply CustomGauge.patch

This required the old JDK 11 to build the project.
set JAVA_HOME=C:\Program Files\Java\jdk-11.0.17

Build:
gradlew.bat clean
gradlew.bat build -x lint
copy build\outputs\aar\CustomGauge-release.aar to CustomGaugeNetBinding
