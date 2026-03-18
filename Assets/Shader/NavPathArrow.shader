Shader "Custom/NavPathArrow"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _ScrollYSpeed("Y Scroll Speed", Range(-20, 20)) = 2
    }
        SubShader
        {
            Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
            LOD 100

            ZWrite Off

            // 双面渲染
            Cull Off
            // Alpha混合
            Blend SrcAlpha OneMinusSrcAlpha

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
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;
                fixed _ScrollYSpeed;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    fixed4 col = tex2D(_MainTex, frac(i.uv * _MainTex_ST.xy + float2(0, _ScrollYSpeed) * _Time));
                    col.a *= (1 - i.uv.y);
                    return col;
                }
                ENDCG
            }
        }
}