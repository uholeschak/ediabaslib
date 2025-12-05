Clone git source:
https://github.com/woxblom/DragListView.git

Set JAVA_HOME to JDK installation directory first, currently:
JAVA_HOME=C:\Program Files\Android\openjdk\jdk-21.0.8

Build:
gradlew.bat clean
gradlew.bat build -x lint -x compileBenchmarkReleaseKotlin -x processBenchmarkManifest -x processBenchmarkReleaseJavaRes -x processBenchmarkResources
copy balloon\build\outputs\aar\balloon-release.aar to SkydovesBalloonBinding
