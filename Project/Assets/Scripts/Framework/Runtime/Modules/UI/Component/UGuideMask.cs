using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.Runtime.UI
{
    public class UGuideMask : MonoBehaviour, ICanvasRaycastFilter
    {
        public Image _Mask; //遮罩图片
        private Material _material;

        private void Awake()
        {
            _material = _Mask.material;
        }

        /// <summary>
        /// 创建圆角矩形区域
        /// </summary>
        /// <param name="pos">矩形的屏幕位置</param>
        /// <param name="widthAndHeight">矩形宽高</param>
        /// <param name="raid">圆角半径</param>
        public void CreateCircleRectangleMask(Vector2 pos, Vector2 widthAndHeight, float raid)
        {
            _material.SetFloat("_MaskType", 2f);
            _material.SetVector("_Origin", new Vector4(pos.x, pos.y, widthAndHeight.x, widthAndHeight.y));
            _material.SetFloat("_Raid", raid);
        }
        public void SetFeather(float feather)
        {
            _material.SetFloat("_Feather", feather);
        }
        /// <summary>
        /// 创建垂直圆角矩形区域
        /// </summary>
        /// <param name="pos">矩形中心点坐标</param>
        /// <param name="widthAndHeight">矩形宽高</param>
        /// <param name="raid">圆角半径</param>
        public void CreateCircleRectangleMaskVertical(Vector2 pos, Vector2 widthAndHeight, float raid)
        {
            _material.SetFloat("_MaskType", 3f);
            _material.SetVector("_Origin", new Vector4(pos.x, pos.y, widthAndHeight.x, widthAndHeight.y));
            _material.SetFloat("_Raid", raid);
        }

        /// <summary>
        /// 创建矩形点击区域
        /// </summary>
        /// <param name="pos">矩形中心点坐标</param>
        /// <param name="widthAndHeight">矩形宽高</param>
        public void CreateRectangleMask(Vector2 pos, Vector2 widthAndHeight)
        {
            _material.SetFloat("_MaskType", 1f);
            _material.SetVector("_Origin", new Vector4(pos.x, pos.y, widthAndHeight.x, widthAndHeight.y));
        }

        /// <summary>
        /// 创建圆形点击区域
        /// </summary>
        /// <param name="pos">圆形中心点坐标</param>
        /// <param name="rad">圆形半径</param>
        public void CreateCircleMask(Vector2 pos, float rad)
        {
            _material.SetFloat("_MaskType", 0f);
            _material.SetVector("_Origin", new Vector4(pos.x, pos.y, rad, 0));
        }

        /// <summary>
        /// 创建双圆形点击区域
        /// </summary>
        /// <param name="pos">大圆形中心点坐标</param>
        /// <param name="rad">大圆形半径</param>
        /// <param name="pos1">小圆形中心点坐标</param>
        /// <param name="rad1">小圆形半径</param>
        public void CreateDoubleCircleMask(Vector2 pos, float rad, Vector2 pos1, float rad1)
        {
            _material.SetFloat("_MaskType", 0f);
            _material.SetVector("_Origin", new Vector4(pos.x, pos.y, rad, 0));
            _material.SetVector("_TopOri", new Vector4(pos1.x, pos1.y, rad1, 0));
        }

        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            // 获取当前遮罩类型
            float maskType = _material.GetFloat("_MaskType");

            // 根据遮罩类型判断点击位置是否在遮罩范围内
            bool isInMaskArea = false;

            switch (maskType)
            {
                case 0: // 圆形/双圆形
                    Vector4 origin = _material.GetVector("_Origin");
                    Vector4 topOri = _material.GetVector("_TopOri");

                    // 检查第一个圆形
                    Vector2 center1 = new Vector2(origin.x, origin.y);
                    float radius1 = origin.z;
                    if (Vector2.Distance(sp, center1) <= radius1)
                    {
                        isInMaskArea = true;
                    }

                    // 检查第二个圆形（如果存在）
                    Vector2 center2 = new Vector2(topOri.x, topOri.y);
                    float radius2 = topOri.z;
                    if (radius2 > 0 && Vector2.Distance(sp, center2) <= radius2)
                    {
                        isInMaskArea = true;
                    }
                    break;

                case 1: // 矩形
                    origin = _material.GetVector("_Origin");
                    float width = origin.z;
                    float height = origin.w;
                    Vector2 center = new Vector2(origin.x, origin.y);

                    float left = center.x - width * 0.5f;
                    float right = center.x + width * 0.5f;
                    float bottom = center.y - height * 0.5f;
                    float top = center.y + height * 0.5f;

                    isInMaskArea = (sp.x >= left && sp.x <= right && sp.y >= bottom && sp.y <= top);
                    break;

                case 2: // 水平圆角矩形
                case 3: // 垂直圆角矩形
                    origin = _material.GetVector("_Origin");
                    float raid = _material.GetFloat("_Raid");
                    width = origin.z;
                    height = origin.w;
                    center = new Vector2(origin.x, origin.y);

                    // 计算矩形边界
                    left = center.x - width * 0.5f;
                    right = center.x + width * 0.5f;
                    bottom = center.y - height * 0.5f;
                    top = center.y + height * 0.5f;

                    // 检查是否在主矩形内（包含圆角部分）
                    if (sp.x >= left - (maskType == 2 ? raid : 0) &&
                        sp.x <= right + (maskType == 2 ? raid : 0) &&
                        sp.y >= bottom - (maskType == 3 ? raid : 0) &&
                        sp.y <= top + (maskType == 3 ? raid : 0))
                    {
                        // 检查是否在内部矩形内（去除圆角部分）
                        if (sp.x >= left + (maskType == 2 ? raid : 0) &&
                            sp.x <= right - (maskType == 2 ? raid : 0) &&
                            sp.y >= bottom + (maskType == 3 ? raid : 0) &&
                            sp.y <= top - (maskType == 3 ? raid : 0))
                        {
                            isInMaskArea = true;
                        }
                        else
                        {
                            // 检查四个圆角
                            Vector2[] cornerCenters = new Vector2[4];
                            if (maskType == 2) // 水平圆角矩形
                            {
                                cornerCenters[0] = new Vector2(left - raid, top - raid);
                                cornerCenters[1] = new Vector2(left - raid, bottom + raid);
                                cornerCenters[2] = new Vector2(right + raid, top - raid);
                                cornerCenters[3] = new Vector2(right + raid, bottom + raid);
                            }
                            else // 垂直圆角矩形
                            {
                                cornerCenters[0] = new Vector2(right - raid, top + raid);
                                cornerCenters[1] = new Vector2(left + raid, bottom - raid);
                                cornerCenters[2] = new Vector2(right - raid, bottom - raid);
                                cornerCenters[3] = new Vector2(left + raid, top + raid);
                            }

                            foreach (Vector2 cornerCenter in cornerCenters)
                            {
                                if (Vector2.Distance(sp, cornerCenter) <= raid)
                                {
                                    isInMaskArea = true;
                                    break;
                                }
                            }
                        }
                    }
                    break;
            }

            // 如果点击在遮罩范围内，则阻止事件（返回false）
            // 如果点击在遮罩范围外，则允许事件穿透（返回true）
            return !isInMaskArea;
        }
    }
}