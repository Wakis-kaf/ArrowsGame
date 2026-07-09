using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System;
using System.IO;
using System.Linq;
using Framework.Runtime;

namespace Framework.Editor.AssetAutoImport
{
    public class AssetAutoImporter : AssetPostprocessor
    {
        #region 配置类

        [System.Serializable]
        public class PlatformTextureSettings
        {
            public BuildTarget platform;
            public int maxTextureSize = 1024;
            public TextureImporterFormat format = TextureImporterFormat.Automatic;
            public int compressionQuality = 100;
            public bool overridden = true;
        }

        [System.Serializable]
        public class SpriteQualitySettings
        {
            public string folderKeyword;
            public int maxTextureSize = 1024;
            public TextureImporterCompression compression = TextureImporterCompression.Compressed;
            public bool mipmapEnabled = false;
            public TextureImporterType textureType = TextureImporterType.Sprite;

            // 平台特定的覆盖设置
            public List<PlatformTextureSettings> platformOverrides = new List<PlatformTextureSettings>();
        }

        [System.Serializable]
        public class ModelPlatformSettings
        {
            public BuildTarget platform;
            public ModelImporterMeshCompression meshCompression = ModelImporterMeshCompression.Medium;
            public bool isReadable = false;
            public float scaleFactor = 1.0f;
        }

        public const string SpritesFoldName = "Sprites";
        public const string IconSpritesFoldName = "IconSprites";
        public const string SmallSpritesFoldName = "SmallSprites";
        public const string MediumSpritesFoldName = "MediumSprites";
        public const string HighSpritesFoldName = "HighSprites";
        public const string LargeSpritesFoldName = "LargeSprites";
        public const string UltraFoldName = "UltraSprites";

        [System.Serializable]
        public class ImportSettings
        {
            // 精灵质量等级设置
            public List<SpriteQualitySettings> spriteQualitySettings = new List<SpriteQualitySettings>
            {
                 new SpriteQualitySettings
                {
                    folderKeyword = IconSpritesFoldName,
                    maxTextureSize = 64,
                    platformOverrides = new List<PlatformTextureSettings>
                    {
                        new PlatformTextureSettings { platform = BuildTarget.StandaloneWindows, format = TextureImporterFormat.DXT5, maxTextureSize = 64 },
                        new PlatformTextureSettings { platform = BuildTarget.Android, format = TextureImporterFormat.ETC2_RGBA8, maxTextureSize = 64 },
                        new PlatformTextureSettings { platform = BuildTarget.iOS, format = TextureImporterFormat.ASTC_6x6, maxTextureSize = 64 },
                        new PlatformTextureSettings { platform = BuildTarget.WebGL, format = TextureImporterFormat.ASTC_12x12, maxTextureSize = 64 },
                    }
                },
                new SpriteQualitySettings
                {
                    folderKeyword = SmallSpritesFoldName,
                    maxTextureSize = 128,
                    platformOverrides = new List<PlatformTextureSettings>
                    {
                        new PlatformTextureSettings { platform = BuildTarget.StandaloneWindows, format = TextureImporterFormat.DXT5, maxTextureSize = 128 },
                        new PlatformTextureSettings { platform = BuildTarget.Android, format = TextureImporterFormat.ETC2_RGBA8, maxTextureSize = 128 },
                        new PlatformTextureSettings { platform = BuildTarget.iOS, format = TextureImporterFormat.ASTC_6x6, maxTextureSize = 128 },
                        new PlatformTextureSettings { platform = BuildTarget.WebGL, format = TextureImporterFormat.ASTC_12x12, maxTextureSize = 128 },
                    }
                },
                new SpriteQualitySettings
                {
                    folderKeyword = MediumSpritesFoldName,
                    maxTextureSize = 256,
                    platformOverrides = new List<PlatformTextureSettings>
                    {
                        new PlatformTextureSettings { platform = BuildTarget.StandaloneWindows, format = TextureImporterFormat.DXT5, maxTextureSize = 256  },
                        new PlatformTextureSettings { platform = BuildTarget.Android, format = TextureImporterFormat.ETC2_RGBA8, maxTextureSize = 256 },
                        new PlatformTextureSettings { platform = BuildTarget.iOS, format = TextureImporterFormat.ASTC_6x6, maxTextureSize = 256 },
                        new PlatformTextureSettings { platform = BuildTarget.WebGL, format = TextureImporterFormat.ASTC_12x12, maxTextureSize = 256 },
                    }
                },
                new SpriteQualitySettings
                {
                    folderKeyword = HighSpritesFoldName,
                    maxTextureSize = 512,
                    platformOverrides = new List<PlatformTextureSettings>
                    {
                        new PlatformTextureSettings { platform = BuildTarget.StandaloneWindows, format = TextureImporterFormat.DXT5, maxTextureSize = 512  },
                        new PlatformTextureSettings { platform = BuildTarget.Android, format = TextureImporterFormat.ETC2_RGBA8, maxTextureSize = 512 },
                        new PlatformTextureSettings { platform = BuildTarget.iOS, format = TextureImporterFormat.ASTC_6x6, maxTextureSize = 512 },
                           new PlatformTextureSettings { platform = BuildTarget.WebGL, format = TextureImporterFormat.ASTC_8x8, maxTextureSize = 512 },
                    }
                },
                new SpriteQualitySettings
                {
                    folderKeyword = LargeSpritesFoldName,
                    maxTextureSize = 1024,
                    platformOverrides = new List<PlatformTextureSettings>
                    {
                        new PlatformTextureSettings { platform = BuildTarget.StandaloneWindows, format = TextureImporterFormat.DXT5 , maxTextureSize = 1024},
                        new PlatformTextureSettings { platform = BuildTarget.Android, format = TextureImporterFormat.ETC2_RGBA8, maxTextureSize = 1024 },
                        new PlatformTextureSettings { platform = BuildTarget.iOS, format = TextureImporterFormat.ASTC_6x6, maxTextureSize = 1024 },
                        new PlatformTextureSettings { platform = BuildTarget.WebGL, format = TextureImporterFormat.ASTC_8x8, maxTextureSize = 1024 },
                    }
                },
                new SpriteQualitySettings
                {
                    folderKeyword = UltraFoldName,
                    maxTextureSize = 2048,
                    platformOverrides = new List<PlatformTextureSettings>
                    {
                        new PlatformTextureSettings { platform = BuildTarget.StandaloneWindows, format = TextureImporterFormat.DXT5 , maxTextureSize = 2048},
                        new PlatformTextureSettings { platform = BuildTarget.Android, format = TextureImporterFormat.ETC2_RGBA8, maxTextureSize = 2048 },
                        new PlatformTextureSettings { platform = BuildTarget.iOS, format = TextureImporterFormat.ASTC_6x6, maxTextureSize = 2048 },
                        new PlatformTextureSettings { platform = BuildTarget.WebGL, format = TextureImporterFormat.ASTC_6x6, maxTextureSize = 2048 },
                    }
                }
            };

            // 默认精灵设置
            public SpriteQualitySettings defaultSpriteSettings = new SpriteQualitySettings
            {
                folderKeyword = SpritesFoldName,
                maxTextureSize = 512,
                compression = TextureImporterCompression.Compressed,
                mipmapEnabled = false,
                textureType = TextureImporterType.Sprite,
                platformOverrides = new List<PlatformTextureSettings>
                {
                    new PlatformTextureSettings { platform = BuildTarget.StandaloneWindows, format = TextureImporterFormat.DXT5, maxTextureSize = 512 },
                    new PlatformTextureSettings { platform = BuildTarget.Android, format = TextureImporterFormat.ASTC_4x4, maxTextureSize = 512 },
                    new PlatformTextureSettings { platform = BuildTarget.iOS, format = TextureImporterFormat.ASTC_6x6, maxTextureSize = 512 },
                    new PlatformTextureSettings { platform = BuildTarget.WebGL, format = TextureImporterFormat.ASTC_12x12, maxTextureSize = 512 }
                }
            };

            // 默认纹理平台设置
            public List<PlatformTextureSettings> defaultTexturePlatformSettings = new List<PlatformTextureSettings>
            {
                new PlatformTextureSettings { platform = BuildTarget.StandaloneWindows, format = TextureImporterFormat.DXT5, maxTextureSize = 2048 },
                new PlatformTextureSettings { platform = BuildTarget.Android, format = TextureImporterFormat.ASTC_8x8, maxTextureSize = 1024 },
                new PlatformTextureSettings { platform = BuildTarget.iOS, format = TextureImporterFormat.ASTC_8x8, maxTextureSize = 1024 },
                new PlatformTextureSettings { platform = BuildTarget.WebGL, format = TextureImporterFormat.ASTC_12x12, maxTextureSize = 1024 }
            };

            // 模型平台设置
            public List<ModelPlatformSettings> modelPlatformSettings = new List<ModelPlatformSettings>
            {
                new ModelPlatformSettings { platform = BuildTarget.StandaloneWindows, meshCompression = ModelImporterMeshCompression.Medium },
                new ModelPlatformSettings { platform = BuildTarget.Android, meshCompression = ModelImporterMeshCompression.High, scaleFactor = 0.8f },
                new ModelPlatformSettings { platform = BuildTarget.iOS, meshCompression = ModelImporterMeshCompression.High, scaleFactor = 0.8f }
            };

            // 通用模型设置
            public float modelScale = 1.0f;

            public bool modelImportMaterials = true;
            public ModelImporterMaterialLocation materialLocation = ModelImporterMaterialLocation.External;
            public ModelImporterAnimationCompression animationCompression = ModelImporterAnimationCompression.Optimal;
        }

        private static ImportSettings _settings;

        public static ImportSettings Settings
        {
            get
            {
                if (_settings == null)
                {
                    _settings = new ImportSettings();
                    // 可以在这里加载保存的设置文件
                }
                return _settings;
            }
        }

        #endregion 配置类

        #region 工具方法

        private BuildTarget GetCurrentBuildTarget()
        {
            // 获取当前激活的构建目标
            return EditorUserBuildSettings.activeBuildTarget;
        }

        private PlatformTextureSettings GetPlatformTextureSettings(List<PlatformTextureSettings> settingsList, BuildTarget target)
        {
            // 查找特定平台的设置
            var platformSettings = settingsList.FirstOrDefault(s => s.platform == target);
            if (platformSettings != null)
            {
                return platformSettings;
            }

            // 如果没有找到特定平台的设置，返回第一个设置或创建默认设置
            return settingsList.FirstOrDefault() ?? new PlatformTextureSettings
            {
                platform = target,
                format = TextureImporterFormat.Automatic,
                maxTextureSize = 1024
            };
        }

        private ModelPlatformSettings GetModelPlatformSettings(BuildTarget target)
        {
            var settings = Settings.modelPlatformSettings.FirstOrDefault(s => s.platform == target);
            return settings ?? new ModelPlatformSettings
            {
                platform = target,
                meshCompression = ModelImporterMeshCompression.Medium,
                scaleFactor = 1.0f
            };
        }

        #endregion 工具方法

        #region 路径检测方法

        private bool ContainsFolderInPath(string folderName)
        {
            string directory = Path.GetDirectoryName(assetPath);
            string[] folders = directory.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return folders.Any(f => f.Equals(folderName, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsInSpritesFolder()
        {
            if (!IsInAutoAssetPath() || !IsInAutoSpriteAssetPath())
            {
                return false;
            }
            string directory = Path.GetDirectoryName(assetPath);
            string[] folders = directory.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            return folders.Any(f =>
                f.Equals(IconSpritesFoldName, StringComparison.OrdinalIgnoreCase) ||
                f.Equals(SpritesFoldName, StringComparison.OrdinalIgnoreCase) ||
                f.Equals(SmallSpritesFoldName, StringComparison.OrdinalIgnoreCase) ||
                f.Equals(MediumSpritesFoldName, StringComparison.OrdinalIgnoreCase) ||
                f.Equals(HighSpritesFoldName, StringComparison.OrdinalIgnoreCase) ||
                f.Equals(UltraFoldName, StringComparison.OrdinalIgnoreCase) ||
                f.EndsWith(SpritesFoldName, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsInTexturesFolder()
        {
            if (!IsInAutoAssetPath() || IsInAutoSpriteAssetPath())
            {
                return false;
            }
            string directory = Path.GetDirectoryName(assetPath);
            string[] folders = directory.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            return folders.Any(f =>
                f.Equals("Textures", StringComparison.OrdinalIgnoreCase) ||
                f.Equals("SmallTextures", StringComparison.OrdinalIgnoreCase) ||
                f.Equals("MediumTextures", StringComparison.OrdinalIgnoreCase) ||
                f.Equals("HighTextures", StringComparison.OrdinalIgnoreCase) ||
                f.Equals("UltraTextures", StringComparison.OrdinalIgnoreCase) ||
                f.EndsWith("Textures", StringComparison.OrdinalIgnoreCase));
        }

        private bool IsInModelsFolder()
        {
            string directory = Path.GetDirectoryName(assetPath);
            string[] folders = directory.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (!IsInAutoAssetPath())
            {
                return false;
            }
            return folders.Any(f =>
                f.Equals("Models", StringComparison.OrdinalIgnoreCase) ||
                f.Equals("FBX", StringComparison.OrdinalIgnoreCase) ||
                f.Equals("Meshes", StringComparison.OrdinalIgnoreCase)) ||
                assetPath.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase) ||
                assetPath.EndsWith(".obj", StringComparison.OrdinalIgnoreCase);
        }
        string suffix = $"Assets/{GameConfig.NameField_ResBuildDir}";
        string spriteDirPathSuffix = $"Assets/{GameConfig.NameField_ResBuildDir}/UISprites";
        //string suffix = $"Assets/{GameConfig.NameField_ResBuildDir}/UISprites";
        private bool IsInAutoAssetPath()
        {
            if (assetPath.Contains(suffix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }
        private bool IsInAutoSpriteAssetPath()
        {
            if (assetPath.Contains(spriteDirPathSuffix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }
        private SpriteQualitySettings GetSpriteQualitySettings()
        {
            string directory = Path.GetDirectoryName(assetPath);
            string[] folders = directory.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            foreach (var qualitySetting in Settings.spriteQualitySettings)
            {
                if (folders.Any(f => f.Equals(qualitySetting.folderKeyword, StringComparison.OrdinalIgnoreCase)))
                {
                    return qualitySetting;
                }
            }

            if (folders.Any(f => f.Equals("Sprites", StringComparison.OrdinalIgnoreCase)))
            {
                return Settings.defaultSpriteSettings;
            }

            return null;
        }

        #endregion 路径检测方法

        #region 模型处理

        public void OnPreprocessModel()
        {
            if (!IsInModelsFolder())
                return;

            Debug.Log($"处理模型: {assetPath}");

            ModelImporter modelImporter = (ModelImporter)assetImporter;
            BuildTarget currentTarget = GetCurrentBuildTarget();
            ModelPlatformSettings platformSettings = GetModelPlatformSettings(currentTarget);

            // 基础设置
            modelImporter.globalScale = Settings.modelScale * platformSettings.scaleFactor;
            modelImporter.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            modelImporter.materialLocation = Settings.materialLocation;

            // 优化设置
            modelImporter.optimizeGameObjects = true;
            modelImporter.animationCompression = Settings.animationCompression;
            modelImporter.animationRotationError = 0.5f;
            modelImporter.animationPositionError = 0.5f;
            modelImporter.animationScaleError = 0.5f;

            // 根据平台设置
            SetModelPlatformSettings(modelImporter, platformSettings);
        }

        private void SetModelPlatformSettings(ModelImporter modelImporter, ModelPlatformSettings platformSettings)
        {
            modelImporter.meshCompression = platformSettings.meshCompression;
            modelImporter.isReadable = platformSettings.isReadable;

            //Debug.Log($"模型平台设置: {platformSettings.platform}, 网格压缩: {platformSettings.meshCompression}, 缩放因子: {platformSettings.scaleFactor}");
        }

        public void OnPostprocessModel(GameObject go)
        {
            if (!IsInModelsFolder())
                return;

            //Debug.Log($"模型导入完成: {assetPath}");
            ProcessAnimationClips(go);
        }

        private void ProcessAnimationClips(GameObject go)
        {
            // 原有的动画剪辑处理逻辑
            // ...
        }

        #endregion 模型处理

        #region 纹理处理

        private bool IsIgnoreDir(string dirPath)
        {
            return dirPath.Contains("/Prefabs/");
        }

        private bool IsAtlasTexture()
        {
            string directory = Path.GetDirectoryName(assetPath);
            string fileName = Path.GetFileName(assetPath);

            // 检测是否是图集纹理（路径包含 Prefabs 且文件名包含 _Tex）
            return directory != null &&
                   directory.Contains("UIAtlas") &&
                   fileName.Contains("_Tex");
        }

        public void OnPreprocessTexture()
        {
            if (!IsInAutoAssetPath() || !IsInAutoSpriteAssetPath())
            {
                return;
            }
            // 排除图集纹理的处理
            if (IsAtlasTexture())
            {
                //Debug.Log($"跳过图集纹理处理: {assetPath}");
                return;
            }
            TextureImporter textureImporter = (TextureImporter)assetImporter;
            string fileName = Path.GetFileName(assetPath);
            BuildTarget currentTarget = GetCurrentBuildTarget();
            string directory = Path.GetDirectoryName(assetPath);
            if (IsIgnoreDir(directory)) return;
            // 处理精灵纹理
            if (IsInSpritesFolder())
            {
                ProcessSpriteTexture(textureImporter, fileName, currentTarget);
                return;
            }

            // 处理普通纹理
            if (IsInTexturesFolder())
            {
                ProcessRegularTexture(textureImporter, fileName, currentTarget);
                return;
            }

            // 其他纹理的默认处理
            //ProcessDefaultTexture(textureImporter, fileName, currentTarget);
        }

        private void ProcessSpriteTexture(TextureImporter textureImporter, string fileName, BuildTarget currentTarget)
        {
            SpriteQualitySettings qualitySettings = GetSpriteQualitySettings();

            if (qualitySettings != null)
            {
                Debug.Log($"处理精灵纹理 [{qualitySettings.folderKeyword}]: {assetPath}");

                textureImporter.textureType = qualitySettings.textureType;
                textureImporter.mipmapEnabled = qualitySettings.mipmapEnabled;
                textureImporter.maxTextureSize = qualitySettings.maxTextureSize;
                textureImporter.textureCompression = qualitySettings.compression;

                // 设置精灵相关属性
                textureImporter.spriteImportMode = SpriteImportMode.Single;
                textureImporter.spritePixelsPerUnit = 100;
                textureImporter.filterMode = FilterMode.Bilinear;

                // 设置平台特定的压缩
                SetTexturePlatformSettings(textureImporter, qualitySettings.platformOverrides, currentTarget, qualitySettings.maxTextureSize);
            }
            else
            {
                // 默认精灵设置
                textureImporter.textureType = TextureImporterType.Sprite;
                textureImporter.mipmapEnabled = false;
                textureImporter.maxTextureSize = 1024;
                SetTexturePlatformSettings(textureImporter, Settings.defaultSpriteSettings.platformOverrides, currentTarget, 1024);
            }

            ProcessAlphaChannel(textureImporter, fileName);
        }

        private void ProcessRegularTexture(TextureImporter textureImporter, string fileName, BuildTarget currentTarget)
        {
            //Debug.Log($"处理普通纹理: {assetPath}");

            textureImporter.textureType = TextureImporterType.Default;
            textureImporter.mipmapEnabled = true;
            textureImporter.maxTextureSize = 2048;

            // 设置平台特定的压缩
            SetTexturePlatformSettings(textureImporter, Settings.defaultTexturePlatformSettings, currentTarget, 2048);
            ProcessAlphaChannel(textureImporter, fileName);
        }

        private void ProcessDefaultTexture(TextureImporter textureImporter, string fileName, BuildTarget currentTarget)
        {
            textureImporter.textureType = TextureImporterType.Default;
            textureImporter.mipmapEnabled = true;
            SetTexturePlatformSettings(textureImporter, Settings.defaultTexturePlatformSettings, currentTarget, 1024);
            ProcessAlphaChannel(textureImporter, fileName);
        }

        private void SetTexturePlatformSettings(TextureImporter textureImporter, List<PlatformTextureSettings> platformSettingsList, BuildTarget currentTarget, int defaultMaxSize)
        {
            var platformSettings = GetPlatformTextureSettings(platformSettingsList, currentTarget);

            TextureImporterPlatformSettings importerSettings = new TextureImporterPlatformSettings
            {
                name = platformSettings.platform.ToString(),
                overridden = platformSettings.overridden,
                maxTextureSize = platformSettings.maxTextureSize > 0 ? platformSettings.maxTextureSize : defaultMaxSize,
                format = platformSettings.format,
                compressionQuality = platformSettings.compressionQuality
            };

            textureImporter.SetPlatformTextureSettings(importerSettings);

            //Debug.Log($"纹理平台设置: {platformSettings.platform}, 格式: {platformSettings.format}, 最大尺寸: {importerSettings.maxTextureSize}");
        }

        private void ProcessAlphaChannel(TextureImporter textureImporter, string fileName)
        {
            string retrivalTextureIsAlpha = @"[Aa][Ll][Pp][Hh][Aa]";
            if (Regex.IsMatch(fileName, retrivalTextureIsAlpha))
            {
                textureImporter.alphaIsTransparency = true;
            }
            else
            {
                textureImporter.alphaIsTransparency = false;
            }
        }

        public void OnPostprocessTexture(Texture2D tex)
        {
            //Debug.Log($"纹理导入完成: {assetPath}");
        }

        private void OnPostprocessSprites(Texture2D texture, Sprite[] sprites)
        {
            //Debug.Log($"精灵处理完成: {sprites.Length} 个精灵从纹理 {texture.name}");
        }

        #endregion 纹理处理

        #region 音频处理和其他方法

        // 原有的音频处理和其他方法保持不变
        // ...

        #endregion 音频处理和其他方法

        #region 编辑器扩展

        [MenuItem("Tools/Asset Import/Reload Settings")]
        public static void ReloadSettings()
        {
            _settings = null;
            Debug.Log("资源导入设置已重新加载");
        }

        [MenuItem("Tools/Asset Import/Show Current Platform Settings")]
        public static void ShowCurrentPlatformSettings()
        {
            BuildTarget currentTarget = EditorUserBuildSettings.activeBuildTarget;
            Debug.Log($"当前构建平台: {currentTarget}");

            var settings = Settings;
            Debug.Log($"默认纹理设置:");
            foreach (var platformSetting in settings.defaultTexturePlatformSettings)
            {
                Debug.Log($"- {platformSetting.platform}: {platformSetting.format} ({platformSetting.maxTextureSize}px)");
            }
        }

        [MenuItem("Tools/Asset Import/Show All Sprite Quality Settings")]
        public static void ShowAllSpriteQualitySettings()
        {
            Debug.Log($"精灵质量设置:");
            foreach (var setting in Settings.spriteQualitySettings)
            {
                Debug.Log($"- {setting.folderKeyword}: MaxSize={setting.maxTextureSize}");
                foreach (var platform in setting.platformOverrides)
                {
                    Debug.Log($"  → {platform.platform}: {platform.format} ({platform.maxTextureSize}px)");
                }
            }
        }

        #endregion 编辑器扩展
    }
}