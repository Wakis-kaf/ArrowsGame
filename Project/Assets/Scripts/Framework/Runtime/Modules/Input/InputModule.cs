using Framework.Runtime.Module.Core;
using Framework.Runtime.UnitSystem.BIInterfaces;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.Runtime.MLanAndTheme
{
    [Serializable]
    public class InputLayerOption
    {
        [SerializeField] private bool m_Enable = true;
        [SerializeField] private InputModule.InputData[] m_Inputs;
        [SerializeField] private string m_LayerName;

        public InputLayerOption()
        {
        }

        public InputLayerOption(string layerName)
        {
            m_LayerName = layerName;
        }

        public bool Enable
        {
            get => m_Enable;
            set => m_Enable = value;
        }

        public InputModule.InputData[] Inputs
        {
            get { return m_Inputs; }
            set => m_Inputs = value;
        }

        public string LayerName => m_LayerName;
    }

    public class InputModule : ModuleUnit, IUnitUpdate
    {
        private const string AxisName = "axisName";
        private const string BtnName = "btnName";
        private const string EnableName = "enable";
        private const string InputName = "inputName";
        private const string InputsName = "inputs";
        private const string InputTypeName = "inputType";
        private const string KeyCodeName = "keyCode";
        private const string LayerName = "layerName";
        private const string MouseKeyName = "mouseKey";

        private Dictionary<string, InputLayer> mLayerMap;

        private LayerBase mSourceInput;

        public InputModule()
        {
            mSourceInput = new LayerBase();
            mLayerMap = new Dictionary<string, InputLayer>();
        }

        public enum ShortcutType
        {
            Down,   // 刚按下时触发一次
            Press,  // 按住持续触发
            Up,     // 释放时触发一次
        }

        /// <summary>
        /// 快捷键回调记录
        /// </summary>
        private class ShortcutCallback
        {
            public InputLayer layer;
            public string inputName;
            public ShortcutType shortcutType;
            public Action<object> callback;
            public object data;

            public bool CheckAndFire()
            {
                if (layer == null || !layer.Enable) return false;
                bool fired = false;
                switch (shortcutType)
                {
                    case ShortcutType.Down:
                        if (layer.IsKeyDown(inputName)) { callback?.Invoke(data); fired = true; }
                        break;
                    case ShortcutType.Press:
                        if (layer.IsKeyPushing(inputName)) { callback?.Invoke(data); fired = true; }
                        break;
                    case ShortcutType.Up:
                        if (layer.IsKeyUp(inputName)) { callback?.Invoke(data); fired = true; }
                        break;
                }
                return fired;
            }
        }

        private List<ShortcutCallback> m_ShortcutCallbacks = new List<ShortcutCallback>();

        /// <summary>
        /// 注册快捷键回调
        /// </summary>
        /// <param name="layerName">输入层名称</param>
        /// <param name="inputName">输入名称</param>
        /// <param name="shortcutType">触发类型</param>
        /// <param name="callback">回调 (data 为注册时传入的透传数据)</param>
        /// <param name="data">透传给回调的附加数据</param>
        public void RegisterShortcut(string layerName, string inputName, ShortcutType shortcutType, Action<object> callback, object data = null)
        {
            var layer = GetController(layerName);
            if (layer == null)
            {
                Debug.LogWarning($"[InputModule] RegisterShortcut failed: layer '{layerName}' not found.");
                return;
            }
            var sc = new ShortcutCallback
            {
                layer = layer,
                inputName = inputName,
                shortcutType = shortcutType,
                callback = callback,
                data = data
            };
            m_ShortcutCallbacks.Add(sc);
        }

        /// <summary>
        /// 取消注册指定回调
        /// </summary>
        public void UnRegisterShortcut(string layerName, string inputName, ShortcutType shortcutType, Action<object> callback)
        {
            m_ShortcutCallbacks.RemoveAll(sc =>
                sc.layer != null &&
                sc.layer.LayerBlocker != null &&
                sc.layer.LayerBlocker.LayerName == layerName &&
                sc.inputName == inputName &&
                sc.shortcutType == shortcutType &&
                sc.callback == callback
            );
        }

        /// <summary>
        /// 取消注册该 Layer 上所有快捷键
        /// </summary>
        public void UnRegisterAllShortcuts(string layerName)
        {
            m_ShortcutCallbacks.RemoveAll(sc => sc.layer != null && sc.layer.LayerBlocker != null && sc.layer.LayerBlocker.LayerName == layerName);
        }

        /// <summary>
        /// 通过回调注销快捷键（不关心 layer/input/type，自动匹配所有匹配的 callback）
        /// </summary>
        public void UnRegisterShortcut(Action<object> callback)
        {
            if (callback == null) return;
            m_ShortcutCallbacks.RemoveAll(sc => sc.callback == callback);
        }
        public enum InputType
        {
            Keyboard,
            MouseButton,
            Button,
            ScrollXValue,
            ScrollYValue,
            AxisValue,
            MousePosition,
        }

        public InputLayer this[string controllerName]
        {
            get { return GetController(controllerName); }
        }

        public InputLayer GetController(string controllerName)
        {
            if (string.IsNullOrEmpty(controllerName)) return null;
            if (!mLayerMap.ContainsKey(controllerName))
            {
                var layerSourceInput = new LayerBase();
                InputLayer controller = new InputLayer(new LayerBlocker(layerSourceInput));
                controller.LayerBlocker.SetLayerName(controllerName);
                mLayerMap.Add(controllerName, controller);
                return controller;
            }

            return mLayerMap[controllerName];
        }

        public float GetMousePositionX()
        {
            return Input.mousePosition.x;
        }

        public float GetMousePositionY()
        {
            return Input.mousePosition.y;
        }

        public bool IsPointOverGameObject()
        {
            return EventSystem.current.IsPointerOverGameObject();
        }

        public void OnUnitUpdate()
        {
            for (int i = 0; i < mLayerMap.Keys.Count; i++)
            {
                mLayerMap[mLayerMap.Keys.ElementAt(i)].Update();
            }
            // 处理快捷键回调
            for (int i = 0; i < m_ShortcutCallbacks.Count; i++)
            {
                m_ShortcutCallbacks[i].CheckAndFire();
            }
        }

        public void RegisterLayerMap(InputLayerOption[] layers)
        {
            if (layers == null) return;
            for (int i = 0; i < layers.Length; i++)
            {
                var layer = layers[i];
                var inputs = layer.Inputs;
                if (!ReferenceEquals(inputs, null))
                {
                    string name = ValidateLayerName(layer.LayerName);
                    InputLayer controller = GetController(name);
                    var layerSourceInput = new LayerBase();
                    // 创建input
                    for (int j = 0; j < inputs.Length; j++)
                    {
                        var input = inputs[j];
                        switch (input.inputType)
                        {
                            case InputType.Keyboard:
                                layerSourceInput.Register(input.inputName, new KeyboardKey(input.keyCode));
                                break;

                            case InputType.MouseButton:
                                layerSourceInput.Register(input.inputName, new MouseKey(input.mouseKey));
                                break;

                            case InputType.Button:
                                layerSourceInput.Register(input.inputName, new ButtonKey(input.btnName));
                                break;

                            case InputType.ScrollXValue:
                                layerSourceInput.Register(input.inputName, new ScrollInputX());
                                break;

                            case InputType.ScrollYValue:
                                layerSourceInput.Register(input.inputName, new ScrollInputY());
                                break;

                            case InputType.AxisValue:
                                layerSourceInput.Register(input.inputName, new AxisInput(input.axisName));
                                break;

                            case InputType.MousePosition:
                                layerSourceInput.Register(input.inputName, new MousePosition());
                                break;
                        }
                    }

                    controller.InitLayerData(layer);
                    controller.LayerBlocker.SetSourceInput(layerSourceInput);
                }
            }
        }

        protected override void OnModuleConstructed()
        {
            base.OnModuleConstructed();
            InputRegister();
        }

        private void InputRegister()
        {
            //ModuleManager.GetModuleUnit<ContextModule>().FindHelper<IInputMap>(inputMap =>
            //{
            //    RegisterLayerMap(inputMap.Layers);
            //});
        }

        private string ValidateControllerName(string name)
        {
            return name;
        }

        private string ValidateLayerName(string name)
        {
            return name;
        }

        [Serializable]
        public class ControllerData
        {
            public bool awakeEnable = true;
            public string controllerName;
        }

        [Serializable]
        public class InputData
        {
            public string inputName;
            public InputType inputType;
            [ShowIf("inputType", InputType.AxisValue)]
            public string axisName;

            [ShowIf("inputType", InputType.Button)]
            public string btnName;



            [ShowIf("inputType", InputType.Keyboard)]
            public KeyCode keyCode;

            [ShowIf("inputType", InputType.MouseButton)]
            public int mouseKey;
        }
    }
}