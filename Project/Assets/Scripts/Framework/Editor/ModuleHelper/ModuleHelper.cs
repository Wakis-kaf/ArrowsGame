using System.Collections;
using System.Collections.Generic;
using System.IO;
using Framework.Runtime;
using Framework.Utils;
using UnityEditor;
using UnityEngine;

namespace Framework.Editor.ModuleHelpes
{
    public class ModuleHelper
    {

        private static string csTemplateDir =
            Application.dataPath + "/Scripts/Framework/Editor/ModuleHelper/CSTemplate";

        [MenuItem("Assets/模块助手/创建C#模块", false, 0)]
        public static void CreateCsharpModule()
        {
            StrInputWindow.AddWindow("创建完整C#模块", "请输入模块名称", moduleName => { CreateCSModuleDir(moduleName); });
        }
        [MenuItem("Assets/模块助手/创建UI模块", false, 0)]
        public static void CreateUIModule()
        {
            StrInputWindow.AddWindow("创建完整模块", "请输入模块名称", moduleName => { CreateUIModuleDir(moduleName); });
        }

        [MenuItem("Assets/模块助手/创建UI图集文件夹", false, 0)]
        public static void CreateUIAtlasModule()
        {
            StrInputWindow.AddWindow("创建完整模块", "请输入模块名称", moduleName => { CreateUIAtlasDir(moduleName); });
        }
        [MenuItem("Assets/模块助手/创建UI图集文件夹(详细)", false, 0)]
        public static void CreateUIAtlasModuleDetail()
        {
            StrInputWindow.AddWindow("创建完整模块", "请输入模块名称", moduleName => { CreateUIAtlasDirDetail(moduleName); });
        }

        [MenuItem("Assets/模块助手/创建完整模块", false, 0)]
        public static void CreateTSAndUIModule()
        {
            StrInputWindow.AddWindow("创建完整模块", "请输入模块名称", moduleName =>
            {
                CreateCSModuleDir(moduleName);
                CreateUIModuleDir(moduleName);
            });
        }

        private static string GetModuleMainPath()
        {
            string platformPath = Utility.Path.PathCombine(Application.dataPath, "Scripts/Game/Runtime/Modules");
            return platformPath;
        }

        private static string GetModuleUIMainPath()
        {
            return Application.dataPath + "/AddressableResources/UI";
        }

        private static string GetModuleUIAtlasPath()
        {
            return Application.dataPath + "/AddressableResources/UISprites";
        }

        private static void CreateUIAtlasDir(string moduleName)
        {
            string errorStr = ModuleHelperUtils.CheckModuleName(moduleName);
            if (!string.IsNullOrEmpty(errorStr))
            {
                EditorUtility.DisplayDialog("错误", errorStr, "确定");
                return;
            }
            string modulePath = Path.Combine(GetModuleUIAtlasPath(), moduleName);
            if (Directory.Exists(modulePath))
            {
                Debug.LogFormat("创建图集模块中断，目标路径已存在：{0}", modulePath);
                return;
            }
            Directory.CreateDirectory(modulePath);

            //Directory.CreateDirectory(Path.Combine(modulePath, AssetAutoImport.AssetAutoImporter.SmallSpritesFoldName));
            //Directory.CreateDirectory(Path.Combine(modulePath, AssetAutoImport.AssetAutoImporter.MediumSpritesFoldName));
            //Directory.CreateDirectory(Path.Combine(modulePath, AssetAutoImport.AssetAutoImporter.HighSpritesFoldName));
            //Directory.CreateDirectory(Path.Combine(modulePath, AssetAutoImport.AssetAutoImporter.LargeSpritesFoldName));
            //Directory.CreateDirectory(Path.Combine(modulePath, AssetAutoImport.AssetAutoImporter.UltraFoldName));
            Directory.CreateDirectory(Path.Combine(modulePath, AssetAutoImport.AssetAutoImporter.SpritesFoldName));
            Directory.CreateDirectory(Path.Combine(modulePath, "BGSprites"));
            //Directory.CreateDirectory(Path.Combine(modulePath, AssetAutoImport.AssetAutoImporter.IconSpritesFoldName));
            //Directory.CreateDirectory(Path.Combine(modulePath, "Prefabs"));
            AssetDatabase.Refresh();
        }
        private static void CreateUIAtlasDirDetail(string moduleName)
        {
            string errorStr = ModuleHelperUtils.CheckModuleName(moduleName);
            if (!string.IsNullOrEmpty(errorStr))
            {
                EditorUtility.DisplayDialog("错误", errorStr, "确定");
                return;
            }
            string modulePath = Path.Combine(GetModuleUIAtlasPath(), moduleName);
            if (Directory.Exists(modulePath))
            {
                Debug.LogFormat("创建图集模块中断，目标路径已存在：{0}", modulePath);
                return;
            }
            Directory.CreateDirectory(modulePath);

            Directory.CreateDirectory(Path.Combine(modulePath, AssetAutoImport.AssetAutoImporter.SmallSpritesFoldName));
            Directory.CreateDirectory(Path.Combine(modulePath, AssetAutoImport.AssetAutoImporter.MediumSpritesFoldName));
            Directory.CreateDirectory(Path.Combine(modulePath, AssetAutoImport.AssetAutoImporter.HighSpritesFoldName));
            Directory.CreateDirectory(Path.Combine(modulePath, AssetAutoImport.AssetAutoImporter.LargeSpritesFoldName));
            Directory.CreateDirectory(Path.Combine(modulePath, AssetAutoImport.AssetAutoImporter.UltraFoldName));
            Directory.CreateDirectory(Path.Combine(modulePath, AssetAutoImport.AssetAutoImporter.SpritesFoldName));
            Directory.CreateDirectory(Path.Combine(modulePath, AssetAutoImport.AssetAutoImporter.IconSpritesFoldName));
            //Directory.CreateDirectory(Path.Combine(modulePath, "Prefabs"));
            AssetDatabase.Refresh();
        }

        private static void CreateUIModuleDir(string moduleName)
        {
            string errorStr = ModuleHelperUtils.CheckModuleName(moduleName);
            if (!string.IsNullOrEmpty(errorStr))
            {
                EditorUtility.DisplayDialog("错误", errorStr, "确定");
                return;
            }
            string modulePath = Path.Combine(GetModuleUIMainPath(), moduleName);
            if (Directory.Exists(modulePath))
            {
                Debug.LogFormat("创建Lua模块中断，目标路径已存在：{0}", modulePath);
                return;
            }
            Directory.CreateDirectory(modulePath);
            Directory.CreateDirectory(Path.Combine(modulePath, "Prefabs"));
            Directory.CreateDirectory(Path.Combine(modulePath, "Prefabs/Components"));
            Directory.CreateDirectory(Path.Combine(modulePath, "Prefabs/Renders"));
            Directory.CreateDirectory(Path.Combine(modulePath, "Prefabs/View"));
            Directory.CreateDirectory(Path.Combine(modulePath, "Textures"));
            AssetDatabase.Refresh();
        }
        private static void CreateCSModuleDir(string moduleName)
        {
            string errorStr = ModuleHelperUtils.CheckModuleName(moduleName);
            if (!string.IsNullOrEmpty(errorStr))
            {
                EditorUtility.DisplayDialog("错误", errorStr, "确定");
                return;
            }

            string csModulePath = Path.Combine(GetModuleMainPath(), moduleName);
            if (Directory.Exists(csModulePath))
            {
                Debug.LogWarningFormat("创建Lua模块注意，目标路径已存在：{0}", csModulePath);
                //return;
            }
            Debug.Log("创建模块，目录 " + csModulePath);
            // 创建基础文件夹
            Directory.CreateDirectory(csModulePath);
            Directory.CreateDirectory(Path.Combine(csModulePath, "View"));
            Directory.CreateDirectory(Path.Combine(csModulePath, "View/Renders"));
            Directory.CreateDirectory(Path.Combine(csModulePath, "Model"));

            // 创建文件
            ModuleHelperUtils.CreateAndWriteFileByTemplate(Path.Combine(csTemplateDir, "GameTemplateModule.txt"),
                csModulePath + "/Game" + moduleName + "Module.cs", "Template", moduleName);
            ModuleHelperUtils.CreateAndWriteFileByTemplate(Path.Combine(csTemplateDir, "GameTemplateViewHandler.txt"),
                csModulePath + "/Game" + moduleName + "ViewHandler.cs", "Template", moduleName);
            //ModuleHelperUtils.CreateAndWriteFileByTemplate(Path.Combine(csTemplateDir, "GameTemplateServerHandler.txt"),
            //    csModulePath + "/Game" + moduleName + "ServerHandler.cs", "Template", moduleName);
            ModuleHelperUtils.CreateAndWriteFileByTemplate(Path.Combine(csTemplateDir, "GameTemplateClientHandler.txt"),
                csModulePath + "/Game" + moduleName + "ClientHandler.cs", "Template", moduleName);
            ModuleHelperUtils.CreateAndWriteFileByTemplate(Path.Combine(csTemplateDir, "GameTemplateDataHandler.txt"),
                csModulePath + "/Game" + moduleName + "DataHandler.cs", "Template", moduleName);
            ModuleHelperUtils.CreateAndWriteFileByTemplate(Path.Combine(csTemplateDir, "GameTemplateConstant.txt"),
                csModulePath + "/Game" + moduleName + "Constant.cs", "Template", moduleName);
            //ModuleHelperUtils.CreateAndWriteFileByTemplate(Path.Combine(luaTemplateDir, "TemplateEvents.cs"),
            //    csModulePath + "/" + moduleName + "Events.cs", "Template", moduleName);
            ModuleHelperUtils.CreateAndWriteFileByTemplate(Path.Combine(csTemplateDir, "GameTemplateUtils.txt"),
                csModulePath + "/Game" + moduleName + "Utils.cs", "Template", moduleName);
            //ModuleHelperUtils.CreateAndWriteFileByTemplate(Path.Combine(luaTemplateDir, "TemplateHandlersGroup.cs"),
            //                csModulePath + "/" + moduleName + "HandlersGroup.ts", "Template", moduleName);
        }
        private static void CreateTSModuleDir(string moduleName)
        {
            string errorStr = ModuleHelperUtils.CheckModuleName(moduleName);
            if (!string.IsNullOrEmpty(errorStr))
            {
                EditorUtility.DisplayDialog("错误", errorStr, "确定");
                return;
            }

            string luaModulePath = Path.Combine(GetModuleMainPath(), moduleName);
            if (Directory.Exists(luaModulePath))
            {
                Debug.LogWarningFormat("创建Lua模块注意，目标路径已存在：{0}", luaModulePath);
                //return;
            }
            Debug.Log("创建模块，目录 " + luaModulePath);
            // 创建基础文件夹
            Directory.CreateDirectory(luaModulePath);
            Directory.CreateDirectory(Path.Combine(luaModulePath, "View"));
            Directory.CreateDirectory(Path.Combine(luaModulePath, "View/Renders"));
            Directory.CreateDirectory(Path.Combine(luaModulePath, "Model"));

            // 创建文件
            ModuleHelperUtils.CreateAndWriteFileByTemplate(Path.Combine(csTemplateDir, "TemplateViewHandler.ts"),
                luaModulePath + "/" + moduleName + "ViewHandler.ts", "Template", moduleName);
            ModuleHelperUtils.CreateAndWriteFileByTemplate(Path.Combine(csTemplateDir, "TemplateServerHandler.ts"),
                luaModulePath + "/" + moduleName + "ServerHandler.ts", "Template", moduleName);
            ModuleHelperUtils.CreateAndWriteFileByTemplate(Path.Combine(csTemplateDir, "TemplateClientHandler.ts"),
                luaModulePath + "/" + moduleName + "ClientHandler.ts", "Template", moduleName);
            ModuleHelperUtils.CreateAndWriteFileByTemplate(Path.Combine(csTemplateDir, "TemplateDataHandler.ts"),
                luaModulePath + "/" + moduleName + "DataHandler.ts", "Template", moduleName);
            ModuleHelperUtils.CreateAndWriteFileByTemplate(Path.Combine(csTemplateDir, "TemplateConstant.ts"),
                luaModulePath + "/" + moduleName + "Constant.ts", "Template", moduleName);
            ModuleHelperUtils.CreateAndWriteFileByTemplate(Path.Combine(csTemplateDir, "TemplateEvents.ts"),
                luaModulePath + "/" + moduleName + "Events.ts", "Template", moduleName);
            ModuleHelperUtils.CreateAndWriteFileByTemplate(Path.Combine(csTemplateDir, "TemplateUtils.ts"),
                luaModulePath + "/" + moduleName + "Utils.ts", "Template", moduleName);
            ModuleHelperUtils.CreateAndWriteFileByTemplate(Path.Combine(csTemplateDir, "TemplateHandlersGroup.ts"),
                            luaModulePath + "/" + moduleName + "HandlersGroup.ts", "Template", moduleName);
        }
    }
}