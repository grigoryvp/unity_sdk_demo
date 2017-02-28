﻿using UnityEditor;
using UnityEditor.Callbacks;
using System.Linq;
using Xcodeapi;

namespace Voximplant
{
    public class VoxiOSExport
    {
        [PostProcessBuild]
        public static void export(BuildTarget buildTarget, string path) {
            if (buildTarget != BuildTarget.iOS)
                return;

            var projectPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
            var proj = new PBXProject();
            proj.ReadFromFile(projectPath);

            var targets = new[]{ PBXProject.GetUnityTargetName(), PBXProject.GetUnityTestTargetName() }
                .Select(x => proj.TargetGuidByName(x))
                .ToArray();

            var unityTargetGuid = targets.First();

            foreach (var target in targets) {
                proj.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
                proj.SetBuildProperty(target, "LD_RUNPATH_SEARCH_PATHS", "$(inherited) @executable_path/Frameworks");
            }

            // Link twice as we have circular dependencies
			proj.AddBuildProperty(targets, "OTHER_LDFLAGS", "-lVoxImplantSDK");
            proj.AddBuildProperty(targets, "OTHER_LDFLAGS", "-lvoximplant-bridge");

            proj.AddDynamicFramework(unityTargetGuid, "Frameworks/Plugins/iOS/WebRTC.framework");

            proj.WriteToFile(projectPath);
        }

    }
}