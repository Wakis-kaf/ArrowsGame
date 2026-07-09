using Framework.Utils;
using UnityEditor;
using UnityEngine;

namespace Framework.Runtime.UI.Editor
{
    public class UnitUIEditor
    {
        private static string rootPath = "Assets/Scripts/Framework/Runtime/Modules/UI/PresetPrefabs/";
        [MenuItem("GameObject/UI组件", false, -20)]
        [MenuItem("GameObject/UI组件/UIRoot(画布)", false, -2)]
        public static void CreateUIRoot()
        {
            GetUIRoot();
        }
        [MenuItem("GameObject/UI组件/UPanel(面板)", false, -1)]
        public static void CreateUPanel()
        {
            Instantiate("UPanel");
        }
        [MenuItem("GameObject/UI组件/UView(视图)", false, 0)]
        public static void CreateUView()
        {
            Instantiate("UView");
        }
        [MenuItem("GameObject/UI组件/UComponent(组件)", false, 0)]
        public static void CreateUComponent()
        {
            Instantiate("UComponent");
        }
        [MenuItem("GameObject/UI组件/UContainer(容器)", false, 1)]
        public static void CreateUContainer()
        {
            Instantiate("UContainer");
        }
        [MenuItem("GameObject/UI组件/USprite(雪碧图)", false, 2)]
        public static void CreateUSprite()
        {
            Instantiate("USprite");
        }
        [MenuItem("GameObject/UI组件/UTabBar(选项卡)", false, 3)]
        public static void CreateUTabBar()
        {
            Instantiate("UTabBar");
        }
        [MenuItem("GameObject/UI组件/UText(文本)", false, 4)]
        public static void CreateUText()
        {
            Instantiate("UText");
        }

        [MenuItem("GameObject/UI组件/UTexture(贴图)", false, 5)]
        public static void CreateUTexture()
        {
            Instantiate("UTexture");
        }

        [MenuItem("GameObject/UI组件/UTMPButton(富文本按钮)", false, 6)]
        public static void CreateUTMPButton()
        {
            Instantiate("UTMPButton");
        }

        [MenuItem("GameObject/UI组件/UTMPText(富文本)", false, 7)]
        public static void CreateUTMPText()
        {
            Instantiate("UTMPText");
        }

        [MenuItem("GameObject/UI组件/UImage(图片)", false, 8)]
        public static void CreateUImage()
        {
            Instantiate("UImage");
        }
        [MenuItem("GameObject/UI组件/UButton(按钮)", false, 9)]
        public static void CreateUButton()
        {
            Instantiate("UButton");
        }
        [MenuItem("GameObject/UI组件/UCheckBox(勾选框)", false, 10)]
        public static void CreateUCheckBox()
        {
            Instantiate("UCheckBox");
        }
        [MenuItem("GameObject/UI组件/UDropSelect(下拉菜单)", false, 11)]
        public static void CreateUDropSelect()
        {
            Instantiate("UDropSelect");
        }

        [MenuItem("GameObject/UI组件/UInputField(输入框)", false, 12)]
        public static void CreateUInputField()
        {
            Instantiate("UInputField");
        }
        [MenuItem("GameObject/UI组件/UScrollbar(滚动条)", false, 13)]
        public static void CreateUScrollbar()
        {
            Instantiate("UScrollbar");
        }
        [MenuItem("GameObject/UI组件/UProgressBar(进度条)", false, 14)]
        public static void CreateUProgressBar()
        {
            Instantiate("UProgressBar");
        }
        [MenuItem("GameObject/UI组件/UValueProgress(值进度条)", false, 14)]
        public static void CreateUValueProgressBar()
        {
            Instantiate("UValueProgress");
        }
        [MenuItem("GameObject/UI组件/UBaseRender(基本渲染元素)", false, 15)]
        public static void CreateUBaseRender()
        {
            Instantiate("UBaseRender");
        }



        [MenuItem("GameObject/UI组件/UCheckBoxGroup(勾选框集合)", false, 16)]
        public static void CreateUCheckBoxGroup()
        {
            Instantiate("UCheckBoxGroup");
        }

        [MenuItem("GameObject/UI组件/UList(列表集合)", false, 17)]
        public static void CreateUList()
        {
            Instantiate("UList");
        }

        [MenuItem("GameObject/UI组件/UTabGroup(选项卡集合)", false, 18)]
        public static void CreateUTabGroup()
        {
            Instantiate("UTabGroup");
        }

        [MenuItem("GameObject/UI组件/UTabNavigation(选项页)", false, 19)]
        public static void CreateUTabNavigation()
        {
            Instantiate("UTabNavigation");
        }


        [MenuItem("GameObject/UI组件/Old/USimpleCheckBox", priority = -100)]
        public static void CreateUSimpleCheckBox()
        {
            Instantiate("USimpleCheckBox");
        }

        [MenuItem("GameObject/UI组件/Old/USimpleCheckBoxGroup", priority = -100)]
        public static void CreateUSimpleCheckBoxGroup()
        {
            Instantiate("USimpleCheckBoxGroup");
        }

        [MenuItem("GameObject/UI组件/Old/USimpleTabBar", priority = -100)]
        public static void CreateUSimpleTabBar()
        {
            Instantiate("USimpleTabBar");
        }

        [MenuItem("GameObject/UI组件/Old/USimpleTabGroup", priority = -100)]
        public static void CreateUSimpleTabGroup()
        {
            Instantiate("USimpleTabGroup");
        }

        [MenuItem("GameObject/UI组件/Old/USimpleTabNavigation", priority = -100)]
        public static void CreateUSimpleTabNavigation()
        {
            Instantiate("USimpleTabNavigation");
        }



        //[MenuItem("GameObject/UI组件/USprite2", priority = 0)]
        //public static void CreateUSprite2()
        //{
        //    Instantiate("USprite2");
        //}





        private static GameObject GetPrefab(string prefabName)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(GetPrefabPath(prefabName));
        }

        private static string GetPrefabPath(string prefabName)
        {
            return rootPath + prefabName + ".prefab";
        }

        private static UIRoot GetUIRoot()
        {
            UIRoot root = UIRoot.Root;
            if (root == null)
            {
                root = GameObject.FindObjectOfType<UIRoot>();
                if (root == null)
                {
                    root = GameObjectUtil.GetOrAddComponent<UIRoot>(Instantiate("UIRoot", null));
                }

                root.gameObject.layer = 5;
                UIRoot.Root = root;
            }

            return root;
        }

        private static GameObject Instantiate(string prefabName)
        {
            var prefab = GetPrefab(prefabName);
            Transform parent = Selection.activeTransform ?? GetUIRoot().transform;
            var obj = GameObject.Instantiate(prefab, parent);
            obj.name = prefab.name;
            return obj;
        }

        private static GameObject Instantiate(string prefabName, Transform parent)
        {
            var prefab = GetPrefab(prefabName);
            var obj = GameObject.Instantiate(prefab, parent);
            obj.name = prefab.name;
            return obj;
        }
    }
}