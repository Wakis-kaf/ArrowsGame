using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Framework.Editor.AssetAutoImport;
using Framework.Editor.Misc;
using Framework.Runtime.UI;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

public class TexturePacker
{
    public static string RESTYPE = "";
    // 请确保此路径指向您本地的 TexturePacker 命令行工具
    public const string TEXTURE_CMD = @"D:\UnityGame\MiniGame\ArrowsGameGit\ArrowsGame\Tools\TexturePacker\bin\TexturePacker";

    // 缓存旧的 Sprite 信息：key=纹理路径, value=字典(key=sprite名称, value=旧的SpriteMetaData)
    private static Dictionary<string, Dictionary<string, SpriteMetaData>> oldSpriteBackups = new Dictionary<string, Dictionary<string, SpriteMetaData>>();

    [MenuItem("Assets/TexturePackerSelect(选中打包图集)", false, 0)]
    public static void Select()
    {
        Object[] selectobj = Selection.GetFiltered(typeof(object), SelectionMode.Assets);
        if (selectobj.Length > 0)
        {
            PackSelect("Assets/AddressableResources/UISprites/" + selectobj[0].name);
        }
    }

    [MenuItem("Assets/TexturePackerSelect(选中打包所有图集)", false, 0)]
    public static void PackAll()
    {
        PackAll("");
    }

    private static void PackAll(string resType)
    {
        string uiDirPath = Application.dataPath + "/AddressableResources/UISprites/";
        foreach (string path in Directory.GetDirectories(uiDirPath))
        {
            string modulePath = path.Substring(path.IndexOf("Assets", StringComparison.Ordinal));
            PackSelect(modulePath, resType);
        }
    }

    private static readonly List<string> ExtPaths = new List<string>
    {
        AssetAutoImporter.IconSpritesFoldName,
        AssetAutoImporter.SmallSpritesFoldName,
        AssetAutoImporter.MediumSpritesFoldName,
        AssetAutoImporter.HighSpritesFoldName,
        AssetAutoImporter.LargeSpritesFoldName,
        AssetAutoImporter.UltraFoldName
    };

    public static void PackSelect(string modulePath, string resType = "")
    {
        if (!ModuleHasTexture(modulePath))
        {
            Debug.Log("模块无可打包图片" + modulePath);
            return;
        }
        string prefabPath = modulePath.Replace("UISprites", "UIAtlas");
        string moduleName = Path.GetFileNameWithoutExtension(modulePath);
        EditorUtility.DisplayCancelableProgressBar("打包图集", $"打包图集{moduleName}中，请稍等", 0);
        prefabPath = prefabPath + "/" + moduleName + ".prefab";
        StartPacket(modulePath, moduleName, prefabPath);
        EditorUtility.ClearProgressBar();
    }

    private static bool ModuleHasTexture(string modulePath)
    {
        if (FileHasTexture(modulePath + "/Sprites")) return true;
        foreach (string extPath in ExtPaths)
        {
            if (FileHasTexture(modulePath + "/" + extPath)) return true;
        }
        return false;
    }

    private static bool FileHasTexture(string path)
    {
        if (!Directory.Exists(path)) return false;
        string[] imageExtensions = { ".png", ".jpg", ".PNG", ".JPG", ".Png", ".Jpg" };
        foreach (string ext in imageExtensions)
        {
            if (Directory.GetFiles(path, "*" + ext).Length > 0) return true;
        }
        return false;
    }

    public static void StartPacket(string modulePath, string moduleName, string prefabPath)
    {
        string prefabsDir = Path.GetDirectoryName(prefabPath);
        if (!Directory.Exists(prefabsDir)) Directory.CreateDirectory(prefabsDir);

        string texPath = prefabPath.Replace(".prefab", "_Tex.png");
        string jsonPath = prefabPath.Replace(".prefab", "_Tex.txt");
        string fullTexPath = texPath.Replace("Assets/", Application.dataPath + "/");

        // 步骤1：在删除旧文件前，备份所有旧的 Sprite 信息（Rect 和 Border）
        BackupOldSpriteSettings(texPath);

        if (File.Exists(fullTexPath)) File.Delete(fullTexPath);
        string fullJsonPath = jsonPath.Replace("Assets/", Application.dataPath + "/");
        if (File.Exists(fullJsonPath)) File.Delete(fullJsonPath);

        AssetDatabase.Refresh();

        GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (go == null)
        {
            go = new GameObject(moduleName);
            go.AddComponent<UAtlas>();
            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
        }

        if (ExePackerCmd(modulePath, prefabPath, moduleName))
        {
            System.Threading.Thread.Sleep(200);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            if (ImportTheTp(prefabPath))
            {
                Debug.Log("[打包图集Success]:" + moduleName);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    private static void BackupOldSpriteSettings(string texturePath)
    {
        TextureImporter textureImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;
        if (textureImporter == null || textureImporter.spritesheet == null) return;

        var backups = new Dictionary<string, SpriteMetaData>();
        foreach (var sprite in textureImporter.spritesheet)
        {
            backups[sprite.name] = sprite;
        }
        oldSpriteBackups[texturePath] = backups;
    }

    private static bool ImportTheTp(string prefabPath)
    {
        try
        {
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            UAtlas mAtlas = go.GetComponent<UAtlas>();
            string mainTexPath = prefabPath.Replace(".prefab", "_Tex.png");
            string jsonTxtPath = prefabPath.Replace(".prefab", "_Tex.txt");
            string fullMainTexPath = mainTexPath.Replace("Assets/", Application.dataPath + "/");

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            TextAsset ta = AssetDatabase.LoadAssetAtPath<TextAsset>(jsonTxtPath);

            // 关键：直接读取磁盘文件获取高度，避免 Unity 缓存旧高度导致计算错位
            byte[] fileData = File.ReadAllBytes(fullMainTexPath);
            Texture2D tempTex = new Texture2D(2, 2);
            tempTex.LoadImage(fileData);
            int realHeight = tempTex.height;

            List<SpriteMetaData> newSprites = ProcessToSprites(ta.text, realHeight);

            if (!ApplySpriteSheet(mainTexPath, newSprites)) return false;

            AssetDatabase.ImportAsset(mainTexPath, ImportAssetOptions.ForceUpdate);
            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(mainTexPath);
            List<Sprite> spriteList = allAssets.OfType<Sprite>().OrderBy(s => s.name).ToList();

            SerializedObject serializedObject = new SerializedObject(mAtlas);
            serializedObject.FindProperty("mainTexture").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Texture2D>(mainTexPath);
            SerializedProperty spriteListProp = serializedObject.FindProperty("_spriteList");
            spriteListProp.ClearArray();
            for (int i = 0; i < spriteList.Count; i++)
            {
                spriteListProp.InsertArrayElementAtIndex(i);
                spriteListProp.GetArrayElementAtIndex(i).objectReferenceValue = spriteList[i];
            }
            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(mAtlas);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Import failed: {e.Message}");
            return false;
        }
    }

    private static bool ApplySpriteSheet(string texturePath, List<SpriteMetaData> newSprites)
    {
        TextureImporter textureImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;
        if (textureImporter == null) return false;

        // 获取备份
        oldSpriteBackups.TryGetValue(texturePath, out var backups);

        SpriteMetaData[] finalSprites = new SpriteMetaData[newSprites.Count];
        for (int i = 0; i < newSprites.Count; i++)
        {
            SpriteMetaData sprite = newSprites[i];

            // 只有当名字存在且尺寸（Width/Height）未改变时，才还原 Border
            if (backups != null && backups.TryGetValue(sprite.name, out var oldData))
            {
                if (Mathf.Approximately(sprite.rect.width, oldData.rect.width) &&
                    Mathf.Approximately(sprite.rect.height, oldData.rect.height))
                {
                    sprite.border = oldData.border;
                }
                else
                {
                    Debug.Log($"<color=yellow>[TexturePacker] 子图 {sprite.name} 尺寸已改变，重置 Border</color>");
                    sprite.border = Vector4.zero;
                }
            }
            finalSprites[i] = sprite;
        }

        textureImporter.textureType = TextureImporterType.Sprite;
        textureImporter.spriteImportMode = SpriteImportMode.Multiple;
        textureImporter.spritesheet = finalSprites;
        textureImporter.alphaIsTransparency = true;
        textureImporter.mipmapEnabled = false;

        // 设置平台格式 (调用您原有的 TextureCompresser)
        bool isCompress = TextureCompresser.isNeedCompress(texturePath);
        string[] platforms = { "Android", "iPhone", "WebGL" };
        foreach (var p in platforms)
        {
            var settings = textureImporter.GetPlatformTextureSettings(p);
            settings.overridden = true;
            settings.format = TextureCompresser.getTextureFormat(isCompress, p, true);
            textureImporter.SetPlatformTextureSettings(settings);
        }

        textureImporter.SaveAndReimport();
        oldSpriteBackups.Remove(texturePath); // 清理缓存
        return true;
    }

    private static List<SpriteMetaData> ProcessToSprites(string jsonText, int textureHeight)
    {
        List<SpriteMetaData> sprites = new List<SpriteMetaData>();
        try
        {
            Hashtable decodedHash = jsonText.hashtableFromJson();
            Hashtable frames = (Hashtable)decodedHash["frames"];
            foreach (DictionaryEntry item in frames)
            {
                string spriteName = Path.GetFileNameWithoutExtension(item.Key.ToString());
                Hashtable frameData = (Hashtable)item.Value;
                Hashtable frame = (Hashtable)frameData["frame"];

                int x = Convert.ToInt32(frame["x"]);
                int y = Convert.ToInt32(frame["y"]);
                int width = Convert.ToInt32(frame["w"]);
                int height = Convert.ToInt32(frame["h"]);

                sprites.Add(new SpriteMetaData
                {
                    name = spriteName,
                    // 使用传入的真实高度计算 Y 轴偏移
                    rect = new Rect(x, textureHeight - y - height, width, height),
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f)
                });
            }
        }
        catch (Exception e) { Debug.LogError("JSON Process Error: " + e.Message); }
        return sprites;
    }

    public static bool ExePackerCmd(string modulePath, string prefabPath, string moduleName)
    {
        StringBuilder sb = new StringBuilder();
        List<string> paths = new List<string> { "Sprites" };
        paths.AddRange(ExtPaths);

        foreach (string path in paths)
        {
            string fullPath = $"{modulePath}/{path}";
            if (Directory.Exists(fullPath)) sb.Append($" \"{fullPath}\" ");
        }

        string pngPath = prefabPath.Replace(".prefab", "_Tex.png");
        string txtPath = prefabPath.Replace(".prefab", "_Tex.txt");
        sb.Append($" --sheet \"{pngPath}\" --data \"{txtPath}\"");
        sb.Append(" --format unity --algorithm MaxRects --max-size 4096 --trim-mode None");
        sb.Append(" --size-constraints POT --disable-rotation --border-padding 0 --shape-padding 2");

        return ExeCmd(TEXTURE_CMD, sb.ToString());
    }

    public static bool ExeCmd(string cmdExe, string cmdParam)
    {
        try
        {
            ProcessStartInfo start = new ProcessStartInfo(cmdExe)
            {
                Arguments = cmdParam,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using (Process process = Process.Start(start))
            {
                process.WaitForExit();
                if (process.ExitCode == 0)
                {
                    return true;
                }
                Debug.LogError("CMD Error : process.ExitCode == " + process.ExitCode);
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("CMD Error: " + e.Message);
            return false;
        }
    }

    [MenuItem("Tools/Clear Border Backups")]
    public static void ClearBorderBackups() { oldSpriteBackups.Clear(); }
}