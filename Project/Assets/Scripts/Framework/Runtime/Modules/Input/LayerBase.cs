using Framework.Runtime.LogSystem;

using System.Collections.Generic;
using UnityEngine;

namespace Framework.Runtime.MLanAndTheme
{
    public class LayerBase
    {
        /* 将所有的映射都写到字典中，以方便改键 */
        private Dictionary<string, MousePosition> mMousePositionMaps;
        private Dictionary<string, PushKey> mPushKeyMaps;
        private Dictionary<string, ValueInput> mValueInputMaps;

        public LayerBase()
        {
            mPushKeyMaps = new Dictionary<string, PushKey>();
            mValueInputMaps = new Dictionary<string, ValueInput>();
            mMousePositionMaps = new Dictionary<string, MousePosition>();
        }

        public bool IsKeyDown(string key)
        {
            if (mPushKeyMaps.ContainsKey(key))
                return mPushKeyMaps[key].IsDown;
            return false;
        }


        public bool IsKeyPushing(string key)
        {
            if (mPushKeyMaps.ContainsKey(key))
                return mPushKeyMaps[key].IsPushing;
            LogError(key);
            return false;
        }

        public bool IsKeyUp(string key)
        {
            if (mPushKeyMaps.ContainsKey(key))
                return mPushKeyMaps[key].IsUp;
            return false;
        }

        public Vector2 Pos(string key)
        {
            if (mMousePositionMaps.ContainsKey(key))
                return mMousePositionMaps[key].Pos;
            return Vector2.zero;
        }

        public void Register(string key, PushKey pushKey)
        {
            if (mPushKeyMaps.ContainsKey(key))
            {
                mPushKeyMaps[key] = pushKey;
            }
            else
            {
                mPushKeyMaps.Add(key, pushKey);
            }
        }

        public void Register(string key, ValueInput valueInput)
        {
            if (mValueInputMaps.ContainsKey(key))
            {
                mValueInputMaps[key] = valueInput;
            }
            else
            {
                mValueInputMaps.Add(key, valueInput);
            }
        }

        public void Register(string key, MousePosition pos)
        {
            if (mMousePositionMaps.ContainsKey(key))
            {
                mMousePositionMaps[key] = pos;
            }
            else
            {
                mMousePositionMaps.Add(key, pos);
            }
        }

        public float Value(string key)
        {
            if (mValueInputMaps.ContainsKey(key))
                return mValueInputMaps[key].Value;
            // LogError(key);
            return 0;
        }

        public float ValueRaw(string key)
        {
            if (mValueInputMaps.ContainsKey(key))
                return mValueInputMaps[key].ValueRaw;
            //LogError(key);
            return 0;
        }

        private void LogError(string key)
        {
            Log.Error($"curMap doesn't contains {key}!");
        }
    }
}