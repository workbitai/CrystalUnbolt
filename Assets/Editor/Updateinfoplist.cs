#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using PlistDocument = UnityEditor.iOS.Xcode.PlistDocument;
using PlistElementDict = UnityEditor.iOS.Xcode.PlistElementDict;

public static class Updateinfoplist
{
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target != BuildTarget.iOS) return;

        string plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);

        PlistElementDict root = plist.root;
root.SetString("NSUserTrackingUsageDescription", "We use this identifier to show you personalized ads and improve your app experience.");


        plist.WriteToFile(plistPath);
    }
}
#endif