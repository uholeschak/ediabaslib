Clone git source
https://github.com/woxblom/DragListView.git

Set JAVA_HOME to JDK installation directory first, currently (newer version is not working):
JAVA_HOME=C:\Program Files (x86)\Android\openjdk\jdk-17.0.14
change:
balloon\Configuration.kt:
  const val minSdk = 23

Build:
gradlew.bat clean
gradlew.bat build -x lint
copy balloon\build\outputs\aar\balloon-release.aar to SkydovesBalloonBinding
