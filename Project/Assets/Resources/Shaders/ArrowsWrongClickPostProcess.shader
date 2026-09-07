Shader "ArrowsGame/WrongClickPostProcess"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Intensity ("Intensity", Range(0, 1)) = 0
        _EdgeWidth ("Edge Width", Range(0, 1)) = 0.22
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _Intensity;
            float _EdgeWidth;

            fixed4 frag(v2f_img input) : SV_Target
            {
                fixed4 sceneColor = tex2D(_MainTex, input.uv);
                float edge = max(abs(input.uv.x * 2.0 - 1.0), abs(input.uv.y * 2.0 - 1.0));
                float edgeMask = smoothstep(1.0 - _EdgeWidth, 1.0, edge) * _Intensity;
                sceneColor.rgb = lerp(sceneColor.rgb, fixed3(1.0, 0.03, 0.03), edgeMask);
                return sceneColor;
            }
            ENDCG
        }
    }
}
