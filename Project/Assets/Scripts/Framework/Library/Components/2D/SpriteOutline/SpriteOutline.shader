Shader "Custom/SpriteOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        // 描边属性
        [PerRendererData] _OutlineWidth ("Outline Width", Range(0,10)) = 0
        [PerRendererData] _OutlineColor ("Outline Color", Color) = (1,1,1,1)
    }
    
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        
        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"
            
            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            float _OutlineWidth;
            fixed4 _OutlineColor;
            
            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                #ifdef PIXELSNAP_ON
                o.vertex = UnityPixelSnap(o.vertex);
                #endif
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.texcoord) * i.color;
                
                // 如果没有描边，直接返回
                if (_OutlineWidth < 0.001)
                {
                    clip(col.a - 0.01);
                    return col;
                }
                
                // 采样周围像素
                float alphaSum = 0;
                float centerAlpha = col.a;
                
                if (centerAlpha < 0.5)
                {
                    // 8方向采样
                    for (int x = -1; x <= 1; x++)
                    {
                        for (int y = -1; y <= 1; y++)
                        {
                            if (x == 0 && y == 0) continue;
                            
                            float2 offset = float2(x, y) * _MainTex_TexelSize.xy * _OutlineWidth;
                            float2 sampleUV = i.texcoord + offset;
                            alphaSum += tex2D(_MainTex, sampleUV).a;
                        }
                    }
                    
                    if (alphaSum > 0.1)
                    {
                        fixed4 outlineCol = _OutlineColor;
                        outlineCol.a *= saturate(alphaSum);
                        return lerp(outlineCol, col, saturate(centerAlpha * 3));
                    }
                }
                
                clip(centerAlpha - 0.01);
                return col;
            }
            ENDCG
        }
    }
    
    Fallback "Sprites/Default"
}