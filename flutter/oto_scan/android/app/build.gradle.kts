plugins {
    id("com.android.application")
    id("kotlin-android")
    // The Flutter Gradle Plugin must be applied after the Android and Kotlin Gradle plugins.
    id("dev.flutter.flutter-gradle-plugin")
}

val mlkitTrial = System.getenv("OTO_MLKIT_TRIAL") == "true"
val googleScanner = mlkitTrial || System.getenv("OTO_GOOGLE_SCANNER") == "true"

android {
    namespace = "id.ayani.oto.oto_scan"
    compileSdk = 36
    ndkVersion = "27.0.12077973"

    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_11
        targetCompatibility = JavaVersion.VERSION_11
    }

    kotlinOptions {
        jvmTarget = JavaVersion.VERSION_11.toString()
    }

    defaultConfig {
        buildConfigField("boolean", "QA", (System.getenv("OTO_SCAN_QA") == "true").toString())
        buildConfigField("boolean", "MLKIT_TRIAL", mlkitTrial.toString())
        buildConfigField("boolean", "GOOGLE_SCANNER", googleScanner.toString())
        applicationId = if (mlkitTrial) "com.niam.ayani.oto.uji" else "com.niam.ayani.oto"
        manifestPlaceholders["otoAppLabel"] = if (mlkitTrial) "OTO Scan Uji" else "OTO Scan"
        minSdk = 29
        testInstrumentationRunner = "id.ayani.oto.oto_scan.ScanInstrumentation"
        targetSdk = 36
        versionCode = flutter.versionCode
        versionName = flutter.versionName
    }

    val uploadStore = System.getenv("OTO_UPLOAD_STORE")
    signingConfigs {
        if (!uploadStore.isNullOrBlank()) {
            create("upload") {
                storeFile = file(uploadStore)
                storePassword = System.getenv("OTO_UPLOAD_PASSWORD")
                keyAlias = "oto-scan-upload"
                keyPassword = System.getenv("OTO_UPLOAD_PASSWORD")
            }
        }
    }
    buildTypes {
        release {
            // Upload signing is opt-in; never fall back to Android's debug key.
            signingConfig = if (uploadStore.isNullOrBlank()) null else signingConfigs.getByName("upload")
        }
    }
    buildFeatures { buildConfig = true }
    testBuildType = "release"
    sourceSets.getByName("main").java.srcDir(if (googleScanner) "src/mlkit/java" else "src/local/java")
}

dependencies {
    if (googleScanner) implementation("com.google.android.gms:play-services-mlkit-document-scanner:16.0.0")
}

flutter {
    source = "../.."
}

// The first-party instrumentation uses only android.app.Instrumentation and Java.
// Do not acquire the unused legacy Kotlin extension introduced on this test graph.
configurations.matching { it.name.contains("AndroidTest") }.configureEach {
    exclude(group = "androidx.core", module = "core-ktx")
}
