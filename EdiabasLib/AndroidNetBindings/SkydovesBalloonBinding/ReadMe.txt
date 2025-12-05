Clone git source
https://github.com/woxblom/DragListView.git

This required the old JDK 17 to build the project.
set JAVA_HOME=C:\Program Files (x86)\Android\openjdk\jdk-17.0.14
change:
balloon\Configuration.kt:
  const val minSdk = 23

Build:
gradlew.bat clean
gradlew.bat build -x lint
copy balloon\build\outputs\aar\balloon-release.aar to SkydovesBalloonBinding
