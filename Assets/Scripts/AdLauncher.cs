using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine.Advertisements;
using System.Linq;

public class AdLauncher : MonoBehaviour
{
    public string[] QtLibs = new[]
    {
        "gnustl_shared",
        "Qt5Core",
        "Qt5Network",
        "Qt5Qml",
        "Qt5Gui",
        "Qt5Quick",
        "Qt5QuickTemplates2",
        "Qt5QuickParticles",
        "Qt5QuickControls2",
        "Qt5Sql",
        "Qt5AndroidExtras",
    };

    public string[] BundledLibs = new[]
    {
        "plugins_bearer_libqandroidbearer",
        "plugins_qmltooling_libqmldbg_debugger",
        "plugins_qmltooling_libqmldbg_inspector",
        "plugins_qmltooling_libqmldbg_local",
        "plugins_qmltooling_libqmldbg_native",
        "plugins_qmltooling_libqmldbg_profiler",
        "plugins_qmltooling_libqmldbg_quickprofiler",
        "plugins_qmltooling_libqmldbg_server",
        "plugins_qmltooling_libqmldbg_tcp",
        "plugins_platforms_android_libqtforandroid",
        "plugins_imageformats_libqdds",
        "plugins_imageformats_libqgif",
        "plugins_imageformats_libqicns",
        "plugins_imageformats_libqico",
        "plugins_imageformats_libqjpeg",
        "plugins_imageformats_libqtga",
        "plugins_imageformats_libqtiff",
        "plugins_imageformats_libqwbmp",
        "plugins_imageformats_libqwebp",
        "plugins_sqldrivers_libqsqlite",
        "qml_QtQuick.2_libqtquick2plugin",
        "qml_QtQuick_Controls.2_libqtquickcontrols2plugin",
        "qml_QtQuick_Controls.2_Material_libqtquickcontrols2materialstyleplugin",
        "qml_QtQuick_Controls.2_Universal_libqtquickcontrols2universalstyleplugin",
        "qml_QtQuick_Templates.2_libqtquicktemplates2plugin",
        "qml_QtQuick_Window.2_libwindowplugin"
    };

    [DllImport("untitled")]
    private static extern int untitled_start(int argc, char[] argv);

    void Start()
    {


    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ShowAdPlacement()
    {

        var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        var classLoader = activity.Call<AndroidJavaObject>("getClassLoader");

        AndroidJNI.AttachCurrentThread();

        var packageName = activity.Call<string>("getPackageName");
        var pm = activity.Call<AndroidJavaObject>("getPackageManager");
        var appInfo = pm.Call<AndroidJavaObject>("getApplicationInfo", packageName, (int)128);
        var bundle = appInfo.Get<AndroidJavaObject>("metaData");

        var qtLibs = fromStrings(QtLibs);
        var bundledLibs = fromStrings(BundledLibs);

        bundle.Call("putStringArrayList", "native.libraries", qtLibs);
        bundle.Call("putStringArrayList", "bundled.libraries", bundledLibs);
        bundle.Call("putString", "main.library", "app");

        // QtNative.setActivity(m_activity, this);
        // QtNative.setClassLoader(classLoader);

        var activityDelegate = new AndroidJavaObject("org/qtproject/qt5/android/QtActivityDelegate");
        // var newActivity = new AndroidJavaObject("org/qtproject/qt5/android/QtActivity");
        var native = new AndroidJavaClass("org/qtproject/qt5/android/QtNative");
        var root = Path.Combine("/data/data", Application.bundleIdentifier);
        var lib = Path.GetFullPath(Path.Combine(root, "/"));

        var libraries = new[]
{
                // Path.Combine(lib, "plugins_bearer_libqandroidbearer.so"),
                Path.Combine(lib, "plugins_platforms_android_libqtforandroid.so"),
                // Path.Combine(lib, "Qt5Core.so"),
            };


        var librariesJni = fromStrings(libraries);
        native.CallStatic("loadQtLibraries", new object[]
        {
            librariesJni
        });

        try
        {
            activityDelegate.Call<bool>("loadApplication", activity, classLoader, bundle);
            activityDelegate.Call<bool>("startApplication");
        }
        catch
        {
            // ignored
        }

        activityDelegate.Call("createSurface", 100, true, 0, 0, 1920, 1080, 32);
        untitled_start(0, null);
        




        // if I figure out the resources


        //native.CallStatic("setActivity", new object[]
        //{
        //    activity,
        //    activityDelegate
        //});

        //native.CallStatic("setClassLoader", new object[]
        //{
        //    classLoader
        //});







        //native.CallStatic<bool>("startApplication", new object[]
        //{
        //                (string) null,
        //                string.Empty,
        //                "app",
        //                lib
        //});




    }

    private static AndroidJavaObject fromStrings(string[] libraries)
    {
        var librariesJni = new AndroidJavaObject("java/util/ArrayList");
        libraries.ToList().ForEach(library =>
        {
            Debug.Log("adding: " + library);
            librariesJni.Call<bool>("add", library);
        });
        return librariesJni;
    }
}
