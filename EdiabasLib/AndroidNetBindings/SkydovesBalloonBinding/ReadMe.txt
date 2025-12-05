Clone git source:
https://github.com/woxblom/DragListView.git

Set JAVA_HOME to JDK installation directory first, currently:
JAVA_HOME=C:\Program Files\Android\openjdk\jdk-21.0.8

Build:
gradlew.bat clean
gradlew.bat build -x lint -x compileBenchmarkReleaseKotlin -x processBenchmarkManifest -x processBenchmarkReleaseJavaRes -x processBenchmarkResources -x processDebugMainManifest -x processDebugManifest -x processDebugResources -x compileNonMinifiedReleaseKotlin -x processNonMinifiedReleaseJavaRes -x compileDebugKotlin -x processDebugJavaRes -x mergeDebugGeneratedProguardFiles -x processBenchmarkReleaseMainManifest -x processBenchmarkReleaseManifest -x processReleaseMainManifest -x processReleaseManifest -x processBenchmarkReleaseResources -x processReleaseResources -x compileBenchmarkReleaseJavaWithJavac -x optimizeReleaseResources -x dexBuilderBenchmarkRelease -x compileDebugJavaWithJavac -x dexBuilderDebug -x compileBenchmarkKotlin -x processBenchmarkJavaRes -x verifyReleaseResources -x optimizeBenchmarkReleaseResources -x compileBenchmarkJavaWithJavac -x bundleDebugClassesToRuntimeJar -x bundleDebugClassesToCompileJar -x compileReleaseKotlin -x processReleaseJavaRes -x compileReleaseJavaWithJavac -x mergeReleaseGeneratedProguardFiles -x dexBuilderBenchmark -x dexBuilderRelease -x mergeDexBenchmarkRelease -x apiCheck -x mergeDexRelease -x bundleReleaseClassesToRuntimeJar -x bundleReleaseClassesToCompileJar -x processNonMinifiedReleaseMainManifest -x processNonMinifiedReleaseManifest -x processNonMinifiedReleaseResources -x compileNonMinifiedReleaseJavaWithJavac -x dexBuilderNonMinifiedRelease -x bundleBenchmarkClassesToRuntimeJar -x bundleBenchmarkClassesToCompileJar -x mergeDexNonMinifiedRelease -x optimizeNonMinifiedReleaseResources

copy balloon\build\outputs\aar\balloon-release.aar to SkydovesBalloonBinding
