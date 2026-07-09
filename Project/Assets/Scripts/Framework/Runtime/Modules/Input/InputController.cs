using System;
using UnityEngine;

namespace Framework.Runtime.MLanAndTheme
{
    public class InputLayer : IInputController
    {
        private LayerBlocker mBlocker;
        private bool mEnable = true;

        public string LayerName => mBlocker.LayerName;

        public InputLayer(LayerBlocker blocker, InputModule.ControllerData ctrData) : this(blocker)
        {
            InitControllerData(ctrData);
        }

        public InputLayer(LayerBlocker blocker)
        {
            mBlocker = blocker;
        }

        public InputLayer(LayerBlocker blocker, InputLayerOption layerOptionData) : this(blocker)
        {
            InitLayerData(layerOptionData);
        }

        public bool Enable
        {
            get => mEnable;
        }

        public LayerBlocker LayerBlocker => mBlocker;

        public void AddKeyDown(string name, Action action)
        {
            mBlocker.AddKeyDown(name, action);
        }

        public void AddKeyPushing(string name, Action action)
        {
            mBlocker.AddKeyPushing(name, action);
        }

        public void AddKeyUp(string name, Action action)
        {
            mBlocker.AddKeyUp(name, action);
        }

        public void InitLayerData(InputLayerOption layerOptionData)
        {
            mBlocker.SetLayerName(layerOptionData.LayerName);
            SetEnable(layerOptionData.Enable);
        }
        public bool IsKeyDown(string name)
        {
            return mBlocker.IsKeyDown(name);
        }

        public bool IsKeyPushing(string name)
        {
            return mBlocker.IsKeyPushing(name);
        }

        public bool IsKeyUp(string name)
        {
            return mBlocker.IsKeyUp(name);
        }

        public Vector2 Pos(string name)
        {
            return mBlocker.Pos(name);
        }

        public void RemoveKeyDown(string name, Action action)
        {
            mBlocker.RemoveKeyDown(name, action);
        }

        public void RemoveKeyPushing(string name, Action action)
        {
            mBlocker.RemoveKeyPushing(name, action);
        }

        public void RemoveKeyUp(string name, Action action)
        {
            mBlocker.RemoveKeyUp(name, action);
        }

        public void SetEnable(bool enable)
        {
            mEnable = enable;
            mBlocker.SetEnabled(enable);
        }

        public void SetKeyDown(string name, Action action)
        {
            mBlocker.SetKeyDown(name, action);
        }

        public void SetKeyPushing(string name, Action action)
        {
            mBlocker.SetKeyPushing(name, action);
        }

        public void SetKeyUp(string name, Action action)
        {
            mBlocker.SetKeyUp(name, action);
        }

        public void Update()
        {
            mBlocker?.Update();
        }

        public float Value(string name)
        {
            return mBlocker.Value(name);
        }

        public float ValueRaw(string name)
        {
            return mBlocker.ValueRaw(name);
        }

        private void InitControllerData(InputModule.ControllerData ctrData)
        {
            SetEnable(ctrData.awakeEnable);
        }
    }
}