Clone git source:
https://github.com/woxblom/DragListView.git

Set JAVA_HOME to JDK installation directory first, currently:
JAVA_HOME=C:\Program Files (x86)\Android\openjdk\jdk-17.0.12

Build:
gradlew.bat clean
gradlew.bat build -x lint
copy balloon\build\outputs\aar\balloon-release.aar to SkydovesBalloonBinding
