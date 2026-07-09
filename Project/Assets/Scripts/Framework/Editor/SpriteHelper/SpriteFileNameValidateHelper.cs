using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class SpriteFileNameValidateHelper
{
    [MenuItem("Assets/规范重命名当前目录所有图片 %#r", false, 20)]
    private static void ValidateAndRenameSprites()
    {
        string selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (string.IsNullOrEmpty(selectedPath) || !Directory.Exists(selectedPath))
        {
            return;
        }

        string[] searchPatterns = { "*.png", "*.jpg", "*.jpeg", "*.tga", "*.bmp" };
        int renameCount = 0;

        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var pattern in searchPatterns)
            {
                string[] files = Directory.GetFiles(selectedPath, pattern, SearchOption.TopDirectoryOnly);
                foreach (string filePath in files)
                {
                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
                    string extension = Path.GetExtension(filePath);

                    // 1. 将所有空格（无论是单个还是连续多个）替换为单个下划线
                    string processedName = Regex.Replace(fileNameWithoutExt, @" +", "_");

                    // 2. 去掉除了英文字母、数字和下划线以外的所有字符
                    processedName = Regex.Replace(processedName, @"[^a-zA-Z0-9_]", "");

                    // 3. 再次处理：如果因为剔除特殊字符导致产生了连续的下划线（例如 "a# b" 变成 "a__b"），将其合并为单个下划线
                    string cleanName = Regex.Replace(processedName, @"_+", "_");

                    // 去除首尾可能残留的下划线（可选，让命名更美观）
                    cleanName = cleanName.Trim('_');

                    // 如果文件名没有发生变化，或者清理后为空，则跳过
                    if (cleanName == fileNameWithoutExt || string.IsNullOrEmpty(cleanName))
                    {
                        continue;
                    }

                    // 检查新名称是否冲突，若冲突则自动加后缀
                    string newName = cleanName;
                    string targetFilePath = Path.Combine(selectedPath, newName + extension);
                    int counter = 1;
                    while (File.Exists(targetFilePath))
                    {
                        newName = $"{cleanName}_{counter}";
                        targetFilePath = Path.Combine(selectedPath, newName + extension);
                        counter++;
                    }

                    // 执行重命名
                    string error = AssetDatabase.RenameAsset(filePath, newName);
                    if (string.IsNullOrEmpty(error))
                    {
                        renameCount++;
                    }
                    else
                    {
                        Debug.LogError($"重命名失败: {filePath} -> {newName}, 原因: {error}");
                    }
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

        AssetDatabase.Refresh();
        Debug.Log($"重命名图片完成！共规范化重命名了 {renameCount} 张图片。");
    }

    [MenuItem("Assets/规范重命名当前目录所有图片 %#r", true)]
    private static bool ValidateAndRenameSpritesValidation()
    {
        if (Selection.activeObject == null) return false;
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        return Directory.Exists(path);
    }
}