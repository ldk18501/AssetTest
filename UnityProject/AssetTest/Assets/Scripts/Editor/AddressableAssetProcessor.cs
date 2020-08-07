using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using System;
using System.Data;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Experimental.SceneManagement;
using Qarth;

namespace Qarth.Editor
{
    public class AddressableAssetProcessor : AssetPostprocessor
    {
        private static AddressableAssetSettings m_Setting = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>("Assets/AddressableAssetsData/AddressableAssetSettings.asset");

        public static void OnPostprocessAllAssets(string[] importedAsset, string[] deleteAsset, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (m_Setting == null)
            {
                Debug.LogError("#Error: Create Addressable Setting First");
                return;
            }
            ProcessImportedAssets(importedAsset);
            ProcessMovedAsset(movedAssets, movedFromAssetPaths);
            ProcessDeleteAsset(deleteAsset);
        }

        private static void ProcessImportedAssets(string[] assetPath)
        {
            if (assetPath == null || assetPath.Length == 0)
            {
                return;
            }

            for (int i = 0; i < assetPath.Length; ++i)
            {
                if (CheckIsRes4Addresable(assetPath[i]))
                {
                    ProcessAssetGroup(assetPath[i]);
                }
            }
        }

        private static void ProcessMovedAsset(string[] movedAssets, string[] movedFromAssets)
        {
            if (movedAssets != null && movedAssets.Length > 0)
            {
                for (int i = 0; i < movedAssets.Length; ++i)
                {
                    ProcessAssetGroup(movedAssets[i], movedFromAssets[i]);
                }
            }
        }

        private static void ProcessDeleteAsset(string[] assetPath)
        {
            if (assetPath == null || assetPath.Length == 0)
            {
                return;
            }

            for (int i = 0; i < assetPath.Length; ++i)
            {
                if (CheckIsRes4Addresable(assetPath[i]))
                {
                    ProcessAssetGroup(assetPath[i]);
                }
            }
        }


        private static bool CheckIsRes4Addresable(string name)
        {
            return name.StartsWith("Assets/") && name.Contains("/AddressableRes/");
        }

        private static void ProcessAssetGroup(string assetPath)
        {
            AssetImporter ai = AssetImporter.GetAtPath(assetPath);
            if (ai == null)
            {
                Debug.LogError("Not Find Asset:" + assetPath);
                return;
            }

            string fullPath = EditorUtils.AssetsPath2ABSPath(assetPath);
            Log.e(fullPath);
            if (Directory.Exists(fullPath))
            {
                return;
            }

            string groupName = string.Empty;

            string dirName = Path.GetDirectoryName(assetPath);
            string assetBundleName = EditorUtils.AssetPath2ReltivePath(dirName).ToLower();
            assetBundleName = assetBundleName.Replace("addressableres/", "");

            if (assetPath.Contains("FolderMode"))
            {
                groupName = assetBundleName;
            }
            else
            {
                groupName = m_Setting.DefaultGroup.name;
            }

            groupName = groupName.Replace("/", "-");
            var group = m_Setting.FindGroup(groupName);
            if (group == null)
            {
                //Debug.LogError("ProcessAssetGroup:" + groupName);
                group = m_Setting.CreateGroup(groupName, false, false, false, new List<AddressableAssetGroupSchema> { m_Setting.DefaultGroup.Schemas[0] }, typeof(SchemaType));

            }

            if (group == null)
            {
                return;
            }

            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            var entry = m_Setting.CreateOrMoveEntry(guid, group);
            entry.SetAddress(PathHelper.FileNameWithoutSuffix(Path.GetFileName(assetPath)), true);
            //EditorUtility.SetDirty(setting);
        }

        /// <summary>
        /// 处理移动
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="moveFromPath"></param>
        private static void ProcessAssetGroup(string assetPath, string moveFromPath)
        {
            AssetImporter ai = AssetImporter.GetAtPath(assetPath);
            if (ai == null)
            {
                Debug.LogError("Not Find Asset:" + assetPath);
                return;
            }

            string fullPath = EditorUtils.AssetsPath2ABSPath(assetPath);
            if (Directory.Exists(fullPath))
            {
                return;
            }

            if (CheckIsRes4Addresable(assetPath))//如果移动到了另一个资源文件夹
            {
                ProcessAssetGroup(assetPath);
            }
            else
            {
                var guid = AssetDatabase.AssetPathToGUID(assetPath);
                m_Setting.RemoveAssetEntry(guid);
            }

            if (CheckIsRes4Addresable(moveFromPath))
            {
                //处理移动前的Group
                string removeFromGroupName = string.Empty;
                string dirName = Path.GetDirectoryName(moveFromPath);
                string assetBundleName = EditorUtils.AssetPath2ReltivePath(dirName).ToLower();
                assetBundleName = assetBundleName.Replace("addressableres/", "");

                if (moveFromPath.Contains("FolderMode"))
                {
                    removeFromGroupName = assetBundleName;
                }
                else
                {
                    removeFromGroupName = m_Setting.DefaultGroup.name;
                }
                removeFromGroupName = removeFromGroupName.Replace("/", "-");
                //Debug.LogError("removeFromGroupName:" + removeFromGroupName);
                var group = m_Setting.FindGroup(removeFromGroupName);
                if (group != null)
                {
                    if (group.entries.Count == 0)
                    {
                        m_Setting.RemoveGroup(group);
                    }

                }
            }

            //EditorUtility.SetDirty(setting);
        }
    }
}