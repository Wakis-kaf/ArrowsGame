Shader "Custom/SoftRadialBlurTransition"
{
    Properties
    {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color Tint", Color) = (0,0,0,1)
        _Progress ("Progress (0 to 1)", Range(0, 1)) = 0
        // 模糊度范围调大，越大越柔和
        _Blurriness ("Blur Strength", Range(0.1, 2.0)) = 1.0 
        [HideInInspector] _Aspect ("Aspect Ratio", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "PreviewType"="Plane" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            fixed4 _Color;
            float _Progress;
            float _Blurriness;
            float _Aspect;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 1. 修正 UV 和纵横比，计算距离
                float2 centeredUV = i.uv - float2(0.5, 0.5);
                centeredUV.x *= _Aspect;
                float dist = length(centeredUV);

                // 2. 计算核心逻辑：重新映射距离
                // 我们定义一个“扩散因子”。
                // 当进度为0时，因子为1；进度接近1时，因子接近0（加一个微小值防止除零）。
                float spreadFactor = 1.0 - _Progress + 0.0001;

                // 将距离除以扩散因子。
                // 进度越小，分母越大，结果越小（透明）。
                // 进度越大，分母越小，结果迅速变大（变黑）。
                float remappedDist = dist / spreadFactor;

                // 3. 应用柔和的渐变
                // 使用 smoothstep 将重新映射后的距离转换为 0 到 1 的 Alpha 值。
                // 0 是中心起点，_Blurriness 控制渐变的坡度有多缓。
                float alphaMask = smoothstep(0, _Blurriness, remappedDist);
                
                // 确保进度为1时完全变黑（修正浮点数误差）
                if (_Progress >= 0.999) alphaMask = 1.0;

                fixed4 finalColor = i.color;
                finalColor.a *= alphaMask;

                return finalColor;
            }
            ENDCG
        }
    }
}