using Framework.Runtime;
using Framework.Runtime.UI;
using Game.Modules.GModuleSceneUnit;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Modules.GModuleModelCapture
{
    public class ModelCaptureRecord
    {
        public object key;
        public int sceneUnitId;
        public ModelCaptureVO modelCapture;
        public ModelSceneUnit modelSceneUnit;
    }
    public class GameModelCapture : GameModuleBaseInstance<GameModelCapture>
    {
        private Queue<ModelCaptureVO> captureVOList = new Queue<ModelCaptureVO>();
        private Dictionary<object, ModelCaptureRecord> m_KeyToRecord = new Dictionary<object, ModelCaptureRecord>();
        private int currentIndex = 1;
        private int spacing = 1000;
        public Transform ModelCaptureRoot
        {
            get
            {
               if(m_ModelCaptureRoot == null)
                {
                    m_ModelCaptureRoot = new GameObject("ModelCaptureRoot").transform;
                    m_ModelCaptureRoot.SetParent(GameApp.Ins.GameAppShell.transform);
                }
                return m_ModelCaptureRoot;
            }
        }
        private Transform m_ModelCaptureRoot;
        public void DisposeCapture(object key)
        {
            if (!m_KeyToRecord.ContainsKey(key)) return;
            var record = m_KeyToRecord[key];
            if (record.modelCapture != null)
            {
                record.modelCapture.Hide();
                GameSceneUnitClientHandler.Ins.GameSceneUnitPool.PutSceneUnit(record.sceneUnitId, record.modelSceneUnit);
                record.modelCapture.Dispose();
                record.modelCapture = null;
                record.modelSceneUnit = null;
            }
            m_KeyToRecord.Remove(key);
        }
        public bool IsInCapture(object key, int sceneUnitId)
        {
            return m_KeyToRecord.ContainsKey(key) && m_KeyToRecord[key].sceneUnitId == sceneUnitId;
        }
        public ModelCaptureRecord CaptureSceneUnitModel(object key,
            int sceneUnitId,
            RawImage rawImage,
            ModelCaptureOption option)
        {
            DisposeCapture(key);
            var record = new ModelCaptureRecord();
            m_KeyToRecord.Add(key, record);
            record.sceneUnitId = sceneUnitId;
            record.key = key;
            if (record.modelSceneUnit == null)
            {
                record.modelSceneUnit = GameSceneUnitClientHandler.Ins.GameSceneUnitPool.
                GetSceneUnit<ModelSceneUnit>(sceneUnitId);
            }
            option.model = record.modelSceneUnit.gameObject;
            record.modelCapture = GetModelCapture(option);
            rawImage.texture = record.modelCapture.RenderTexture;
            record.modelCapture.Show();
            return record;
        }
        
        private Vector3 GetModelCapturePostion(ModelCaptureVO captureVO)
        {
            return new Vector3(spacing * captureVO.Index, 0, 0);
        }

        public ModelCaptureVO GetModelCapture(ModelCaptureOption option)
        {
           
            ModelCaptureVO captureVO;
            if (captureVOList.Count > 0)
            {
                captureVO = captureVOList.Dequeue();
            }
            else
            {
                captureVO = new ModelCaptureVO();
                currentIndex++;
                captureVO.Index = currentIndex;
            }
            option.birthPostion = GetModelCapturePostion(captureVO); 
            captureVO.SetOption(option);
            return captureVO;
        }
        public void PutModelCapture(ModelCaptureVO captureVO)
        {
            captureVO.Close();
            captureVOList.Enqueue(captureVO);
        }
        
        /// <summary>
        /// 构造函数中调用，托管对象可以在这初始化
        /// </summary>
        protected override void OnConstructed()
        {
            
        }
        /// <summary>
        /// 注册所有的处理类
        /// </summary>
        protected override void GenerateHandlers()
        {
            RegisterHandler<GameModelCaptureClientHandler>();
            RegisterHandler<GameModelCaptureDataHandler>();
            RegisterHandler<GameModelCaptureViewHandler>();
        }
        /// <summary>
        /// 当所有游戏模块刚被构建的时候回传触发
        /// </summary>
        protected override void OnModuleAwake()
        {
         
        }
        /// <summary>
        /// 当所有游戏模块已被创建成功的时候回传触发
        /// </summary>
        protected override void OnModuleStart()
        {
          
        }

        /// <summary>
        /// 当游戏模块被销毁的时候回传触发
        /// </summary>
        protected override void OnModuleDestroy()
        {
            
        }
    }

}
