// SplitTexture.cs

using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 将图片分离成多张小图
/// </summary>
public class SplitTexture
{
    [MenuItem("Tools/SplitTexture")]
    static void DoSplitTexture()
    {
        // 获取所选图片
        Texture2D selectedImg = Selection.activeObject as Texture2D;
        if (selectedImg == null)
        {
            Debug.LogError("请选择一个Texture2D资源");
            return;
        }

        string path = AssetDatabase.GetAssetPath(selectedImg);
        TextureImporter texImp = AssetImporter.GetAtPath(path) as TextureImporter;

        if (texImp == null)
        {
            Debug.LogError("无法获取TextureImporter");
            return;
        }

        // 检查是否为Multiple Sprite模式
        if (texImp.spriteImportMode != SpriteImportMode.Multiple)
        {
            Debug.LogError("请选择Multiple Sprite模式的纹理");
            return;
        }

        // 保存原始的readable设置
        bool originalReadable = texImp.isReadable;

        // 设置为可读
        texImp.isReadable = true;
        texImp.SaveAndReimport();

        // 重新加载纹理（确保数据是最新的）
        selectedImg = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

        // 获取根目录
        string rootPath = Path.GetDirectoryName(path);
        string folderName = selectedImg.name;

        // 创建文件夹（如果不存在）
        string folderPath = Path.Combine(rootPath, folderName);
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder(rootPath, folderName);
        }

        // 获取所有sprite数据
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(path)
            .OfType<Sprite>()
            .ToArray();

        Debug.Log($"开始分割纹理: {selectedImg.name}, 包含 {sprites.Length} 个sprite");

        // 遍历每个sprite
        foreach (Sprite sprite in sprites)
        {
            // 获取sprite的矩形区域
            Rect rect = sprite.rect;
            int width = Mathf.RoundToInt(rect.width);
            int height = Mathf.RoundToInt(rect.height);
            int startX = Mathf.RoundToInt(rect.x);
            int startY = Mathf.RoundToInt(rect.y);

            // 创建新的纹理
            Texture2D smallImg = new Texture2D(width, height, TextureFormat.RGBA32, false);

            try
            {
                // 确保纹理可读
                if (!selectedImg.isReadable)
                {
                    Debug.LogWarning($"纹理 {selectedImg.name} 不可读，可能需要重新导入");
                    continue;
                }

                // 复制像素数据
                // 注意：Texture2D的GetPixels以左下角为原点，但rect坐标通常以左上角为原点
                // 在Sprite Editor中切割时，Y坐标是从上往下的
                // 所以我们需要调整Y坐标

                // 方法1：使用GetPixels直接获取区域
                Color[] pixels = selectedImg.GetPixels(
                    startX,
                    startY,
                    width,
                    height
                );

                // 由于Unity中纹理坐标原点在左下角，而我们的切割区域可能需要垂直翻转
                // 但实际上GetPixels已经处理了坐标转换，所以直接使用即可
                smallImg.SetPixels(pixels);
                smallImg.Apply();

                // 保存为PNG文件
                string fileName = $"{sprite.name}.png";
                string filePath = Path.Combine(folderPath, fileName);

                byte[] pngData = smallImg.EncodeToPNG();
                File.WriteAllBytes(filePath, pngData);

                Debug.Log($"已保存: {fileName} ({width}x{height}) 位置: ({startX}, {startY})");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"处理sprite {sprite.name} 时出错: {e.Message}");
                continue;
            }
            finally
            {
                // 销毁临时纹理
                if (Application.isPlaying)
                    Object.Destroy(smallImg);
                else
                    Object.DestroyImmediate(smallImg);
            }
        }

        // 恢复原始的readable设置
        texImp.isReadable = originalReadable;
        texImp.SaveAndReimport();

        // 刷新资源窗口
        AssetDatabase.Refresh();

        // 重新导入保存的图片，设置正确的导入设置
        string[] generatedFiles = Directory.GetFiles(folderPath, "*.png");
        foreach (string filePath in generatedFiles)
        {
            TextureImporter smallTextureImp = AssetImporter.GetAtPath(filePath) as TextureImporter;
            if (smallTextureImp != null)
            {
                smallTextureImp.textureType = TextureImporterType.Sprite;
                smallTextureImp.spriteImportMode = SpriteImportMode.Single;
                smallTextureImp.isReadable = true;
                smallTextureImp.alphaIsTransparency = true;
                smallTextureImp.mipmapEnabled = false;
                smallTextureImp.wrapMode = TextureWrapMode.Clamp;
                smallTextureImp.filterMode = FilterMode.Bilinear;

                // 设置平台相关压缩
                TextureImporterPlatformSettings platformSettings = new TextureImporterPlatformSettings();
                platformSettings.name = "Default";
                platformSettings.overridden = false;
                smallTextureImp.SetPlatformTextureSettings(platformSettings);

                smallTextureImp.SaveAndReimport();
            }
        }

        Debug.Log($"分割完成！共生成 {generatedFiles.Length} 个文件");
    }

    [MenuItem("Tools/SplitTexture", true)]
    static bool ValidateDoSplitTexture()
    {
        // 只允许在选中Multi Sprite纹理时显示菜单
        if (Selection.activeObject is Texture2D)
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
            return ti != null && ti.spriteImportMode == SpriteImportMode.Multiple;
        }
        return false;
    }
}