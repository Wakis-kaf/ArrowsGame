// Assets/AddressableResources/Shaders/GuideMask.shader
Shader "UI/GuideMask"
{
    Properties{
        [PerRendererData] _MainTex("Sprite Texture", 2D)="white"{}
        _Color("Tint",Color)=(1,1,1,1)

        _StencilComp("Stencil Comparison", Float) = 8
        _Stencil("Stencil ID", Float) = 0
        _StencilOp("Stencil Operation", Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask("Stencil Read Mask", Float) = 255		
        _ColorMask("Color Mask", Float) = 15

        _Origin("Rect",Vector) = (0,0,0,0)  // centerX, centerY, width, height (screen px)
        _TopOri("TopCircle",Vector) = (0,0,0,0)  // centerX, centerY, radius, unused (screen px)
        _Raid("RectRaid",Range(0,100)) = 0  // corner radius (px)
        _MaskType("Type",Float) = 0		// 0:circle 1:rect 2:horizontal rounded 3:vertical rounded
        _Feather("Feather(px)",Range(0,200)) = 8 // feather width in pixels
    }
    SubShader{
        Tags{
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }
        Stencil{
            Ref[_Stencil]
            Comp[_StencilComp]
            Pass[_StencilOp]
            ReadMask[_StencilReadMask]
            WriteMask[_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest[unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask[_ColorMask]

        Pass{
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f{
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 screenPos : TEXCOORD1;  // screen pos
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _Origin;
            float4 _TopOri;
            float _Raid;
            float _MaskType;
            float _Feather;

            v2f vert(appdata_t IN){
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.screenPos = ComputeScreenPos(OUT.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            // convert clip-space ComputeScreenPos to screen pixel coords
            float2 GetScreenPixelPos(float4 screenPos)
            {
                float2 ndc = screenPos.xy / screenPos.w; // 0..1
                return float2(ndc.x * _ScreenParams.x, ndc.y * _ScreenParams.y);
            }

            // SDF for circle (negative inside)
            float sdCircle(float2 p, float2 c, float r)
            {
                return distance(p, c) - r;
            }

            // SDF for rounded rectangle centered at center, halfSize and corner radius r
            float sdRoundRect(float2 p, float2 center, float2 halfSize, float r)
            {
                // q = abs(p - center) - (halfSize - r)
                float2 q = abs(p - center) - (halfSize - r);
                float2 qMax = max(q, 0.0);
                float outside = length(qMax);
                float inside = min(max(q.x, q.y), 0.0);
                return outside + inside - r;
            }

            // convert signed distance to smooth mask: 0 inside (cut), 1 outside (keep), transition over feather
            float smoothMaskFromSD(float sd, float feather)
            {
                float f = max(feather, 0.0001);
                // sd <= 0 => inside (fully cut => 0).
                // sd >= f => outside (fully keep => 1).
                // transition for sd in [0, f] => outward feather
                return saturate(smoothstep(0.0, f, sd));
            }

            fixed checkInCircleFeather(float2 p)
            {
                float sd1 = sdCircle(p, _Origin.xy, _Origin.z);
                float sd2 = sdCircle(p, _TopOri.xy, _TopOri.z);
    
                // 调试：输出两个圆形的SDF值
                // if (p.x < 50 && p.y < 50) {
                //     return fixed4(sd1/100.0, sd2/100.0, 0, 1);
                // }
    
                // 问题可能：_TopOri的默认值是(0,0,0,0)，在原点处有一个半径为0的圆
                // 这会导致sd2在原点处为负值，影响min操作
    
                // 解决方案：只有当_TopOri.z > 0时才计算第二个圆
                float sd = sd1;
                if (_TopOri.z > 0.0) {
                    sd = min(sd1, sd2);
                }
    
                return smoothMaskFromSD(sd, _Feather);
            }

            fixed checkInRectFeather(float2 p)
            {
                float2 center = _Origin.xy;
                float2 halfSize = float2(_Origin.z * 0.5, _Origin.w * 0.5);
                float sd = sdRoundRect(p, center, halfSize, 0.0);
                return smoothMaskFromSD(sd, _Feather);
            }

            fixed checkInRoundedRectFeather(float2 p)
            {
                float2 center = _Origin.xy;
                float2 halfSize = float2(_Origin.z * 0.5, _Origin.w * 0.5);
                float r = clamp(_Raid, 0.0, min(halfSize.x, halfSize.y));
                float sd = sdRoundRect(p, center, halfSize, r);
                return smoothMaskFromSD(sd, _Feather);
            }

            fixed4 frag(v2f IN) : SV_Target{
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                float2 sp = GetScreenPixelPos(IN.screenPos);

                float mask = 1.0;
                if(_MaskType == 0.0){
                    mask = checkInCircleFeather(sp);
                }
                else if(_MaskType == 1.0){
                    mask = checkInRectFeather(sp);
                }
                else if(_MaskType == 3.0){
                    mask = checkInRoundedRectFeather(sp);
                }
                else if(_MaskType == 2.0){
                    mask = checkInRoundedRectFeather(sp);
                }

                // Apply mask: inside -> transparent (mask near 0), outside -> keep (mask near 1)
                color.a *= mask;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.vertex.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}