Shader "UI/Custom/ShineEffect_Integrated"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        [Header(Common Settings)]
        _ShineColor ("Shine Color", Color) = (1,1,1,1)
        _Speed ("Speed", Float) = 1.0
        _Interval ("Wait Time (Seconds)", Float) = 2.0
        _Intensity ("Intensity", Float) = 1.5
        _Angle ("Angle (0-360)", Range(0, 360)) = 45

        [Header(Mode Selection)]
        [Toggle] _UseShineTex ("Use Shine Texture", Float) = 0
        
        [Header(Texture Mode Settings)]
        [NoScaleOffset] _ShineTex ("Shine Texture", 2D) = "black" {}
        _UVRange ("Scan Range (Texture Only)", Range(1.0, 5.0)) = 2.5

        [Header(Procedural Mode Settings)]
        _Width ("Shine Width (Procedural Only)", Range(0.01, 1.0)) = 0.2

        // UI Mask 必要属性
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
        }

        Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha One 
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0; 
                float4 worldPosition : TEXCOORD1;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex, _ShineTex;
            fixed4 _Color, _ShineColor;
            float _Speed, _Interval, _Intensity, _Angle, _UseShineTex, _UVRange, _Width;
            float4 _ClipRect;

            v2f vert (appdata_t v) {
                v2f o;
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(o.worldPosition);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float cycle = 1.0 + _Interval;
                float timeNow = fmod(_Time.y * _Speed, cycle);
                float active = step(timeNow, 1.0);
                float progress = saturate(timeNow);
                float shineMask = 0;

                // --- 逻辑分支 ---
                if (_UseShineTex > 0.5) {
                    // 1. 贴图采样模式
                    float offset = lerp(-_UVRange, _UVRange, progress);
                    float2 shineUV = i.texcoord;
                    shineUV.x -= offset;

                    // 边界硬裁切（防止白面片）
                    float edgeMask = step(0, shineUV.x) * step(shineUV.x, 1) * step(0, shineUV.y) * step(shineUV.y, 1);
                    
                    // 同时读取 R 和 A 通道（兼容 From Gray Scale）
                    fixed4 tex = tex2D(_ShineTex, shineUV);
                    shineMask = max(tex.r, tex.a) * edgeMask;
                }
                else {
                    // 2. 数学计算模式
                    float currentPos = lerp(-1.5, 1.5, progress);
                    float angleRad = _Angle * 0.0174533;
                    float dir = i.texcoord.x * cos(angleRad) + i.texcoord.y * sin(angleRad);
                    float dist = abs(dir - currentPos);
                    shineMask = saturate(1.0 - (dist / _Width));
                    shineMask = smoothstep(0, 1, shineMask);
                }

                // 统一应用强度和激活状态
                shineMask *= active;

                fixed4 col = _ShineColor;
                col.rgb *= shineMask * _Intensity;
                col.a = shineMask * _ShineColor.a;

                // UI 裁剪支持
                col.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                
                return col;
            }
            ENDCG
        }
    }
}