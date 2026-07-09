using System;
using UnityEditor;
using UnityEngine;
using static UnityEditor.PlayerSettings;

namespace Framework.Editor.Misc
{
    public class TextureCompresser
    {
        public const string Android = "Android";
        public const string iPhone = "iPhone";
        public const string WebGL = "WebGL";
        private static string[] m_NoCompressTexture = new string[] { };
        public static bool isNeedCompress(string path)
        {
            return Array.IndexOf(m_NoCompressTexture, path) == -1;
        }
        public static TextureImporterFormat getTextureFormat(bool compress, string platform, bool isTransparent)
        {
            if (compress)
            {
                if(platform == Android)
                {
                    if (isTransparent)
                    {
                        return TextureImporterFormat.ETC2_RGBA8;
                    }
                    else
                    {
                        return TextureImporterFormat.ETC_RGB4;
                    }
                }
                else if(platform == iPhone)
                {
                    if (isTransparent)
                    {
                        return TextureImporterFormat.ASTC_4x4;
                    }
                    else
                    {
                        return TextureImporterFormat.PVRTC_RGB4;
                    }
                }else if(platform == WebGL)
                {
                    if (isTransparent)
                    {
                        return TextureImporterFormat.ASTC_8x8;
                    }
                    else
                    {
                        return TextureImporterFormat.ASTC_8x8;
                    }
                }
            }
            else
            {
                if (isTransparent)
                {
                    return TextureImporterFormat.RGBA32;
                }
                else
                {
                    return TextureImporterFormat.RGB24;
                }
            }

            return TextureImporterFormat.RGBA32;
        }
    }
}