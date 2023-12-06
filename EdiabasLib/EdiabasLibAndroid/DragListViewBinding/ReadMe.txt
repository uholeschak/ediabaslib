https://github.com/woxblom/DragListView.git

Set JAVA_HOME to JDK installation directory first

Build:
gradlew.bat clean
gradlew.bat build -x library:testDebugUnitTest -x library:testReleaseUnitTest
copy DragListView\library\build\outputs\aar\library-release.aar to DragListViewBinding\Jars
