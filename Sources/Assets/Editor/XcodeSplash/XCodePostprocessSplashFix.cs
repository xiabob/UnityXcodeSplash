using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
#if UNITY_IPHONE
using UnityEditor.iOS.Xcode;
#endif
using System.IO;

public static class XCodePostprocessSplashFix
{
#if UNITY_IPHONE

    const string XCODE_IMAGES_FOLDER = "Unity-iPhone/Images.xcassets";
    const string SOURCE_FOLDER_NAME = "customlaunchscreen.imageset";
    const string SOURCE_FOLDER_ROOT = "Editor/XcodeSplash";
    const string XCODE_PROJECT_NAME = "Unity-iPhone";

    [PostProcessBuildAttribute(1)]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
    {
        // https://docs.unity3d.com/2019.3/Documentation/Manual/PlayerSettingsiOS-SplashImage.html
        // https://juejin.im/post/5e1463d4f265da5d716e572d#heading-1
        if (buildTarget == BuildTarget.iOS)
        {
            string plistPath = path + "/Info.plist";
            PlistDocument plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));
            bool useStoryboard = plist.root.values.ContainsKey("UILaunchStoryboardName");
            if (!useStoryboard) return;

            Debug.Log("XCodePostprocessSplashFix: will fix");

            // set status bar hidden at launch screen
            plist.root.SetBoolean("UIViewControllerBasedStatusBarAppearance", false);
            File.WriteAllText(plistPath, plist.WriteToString());

            // https://forum.unity.com/threads/xcode-storyboard-option-for-splash-screens-launch-screens-in-ios-build.811131/
            // copy launch image
            string sourcePath = string.Format("{0}/{1}/{2}", Application.dataPath, SOURCE_FOLDER_ROOT, SOURCE_FOLDER_NAME);
            string targetPath = string.Format("{0}/{1}/{2}", path, XCODE_IMAGES_FOLDER, SOURCE_FOLDER_NAME);
            FileUtil.DeleteFileOrDirectory(targetPath);
            FileUtil.CopyFileOrDirectory(sourcePath, targetPath);

            // 处理横屏异常
            var oi = PlayerSettings.defaultInterfaceOrientation;
            if (oi == UIOrientation.AutoRotation || oi == UIOrientation.LandscapeLeft || oi == UIOrientation.LandscapeRight)
            {
                // unity bug issue: https://issuetracker.unity3d.com/issues/ios-storyboard-shows-in-a-landscape-mode-for-a-second-even-though-device-orientation-in-xcode-is-set-to-portrait?_ga=2.47487086.819118809.1583413565-660799904.1526867658
                // fix: https://forum.unity.com/threads/cant-get-launchscreen-storyboard-to-work-need-docs.524865/
                FixLandscapeIssue(path);
            }

            Debug.Log("XCodePostprocessSplashFix: did fix");
        }
    }

    private static void FixLandscapeIssue(string pathToBuiltProject)
    {
        //Launch screen
        //Set team ID manually
        string projPath = pathToBuiltProject + "/" + XCODE_PROJECT_NAME + ".xcodeproj/project.pbxproj";

        PBXProject proj = new PBXProject();
        proj.ReadFromString(File.ReadAllText(projPath));
        AddFileToProject(ref proj, pathToBuiltProject, "Editor/XcodeSplash/Landscape/ActualLaunchScreen.storyboard", "ActualLaunchScreen.storyboard"); //This one is used by iOS
        //This uses the CustomSplashViewController class to disable Unity orientation hacks
        AddFileToProject(ref proj, pathToBuiltProject, "Editor/XcodeSplash/Landscape/CustomSplashViewController.h", "CustomSplashViewController.h");
        AddFileToProject(ref proj, pathToBuiltProject, "Editor/XcodeSplash/Landscape/CustomSplashViewController.m", "CustomSplashViewController.m");

        File.WriteAllText(projPath, proj.WriteToString());

        // Get plist
        string plistPath = pathToBuiltProject + "/Info.plist";
        PlistDocument plist = new PlistDocument();
        plist.ReadFromString(File.ReadAllText(plistPath));

        //Set the Storyboard without custom class
        plist.root.SetString("UILaunchStoryboardName", "ActualLaunchScreen");

        // Write to file
        File.WriteAllText(plistPath, plist.WriteToString());
    }

    static void AddFileToProject(ref PBXProject proj, string pathToBuiltProject, string assetPath, string projectPath)
    {

        string sourcepath = Path.Combine(Application.dataPath, assetPath);
        string targetpath = Path.Combine(pathToBuiltProject, projectPath);

        string targetguid = proj.TargetGuidByName(XCODE_PROJECT_NAME);

        Debug.Log("Add File To Xcode project: " + sourcepath + " -> " + targetpath);

        File.Copy(sourcepath, targetpath, true);
        string guid = proj.AddFile(targetpath, projectPath, PBXSourceTree.Source);
        proj.AddFileToBuild(targetguid, guid);
    }

#endif
}