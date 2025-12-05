Clone git source (4096a34a: Update Gradle to v9.2.0 (#868))
https://github.com/woxblom/DragListView.git

Set JAVA_HOME to JDK installation directory first, currently (newer version is not working):
JAVA_HOME=C:\Program Files (x86)\Android\openjdk\jdk-17.0.14

Build:
gradlew.bat clean
gradlew.bat build -x lint
copy balloon\build\outputs\aar\balloon-release.aar to SkydovesBalloonBinding
