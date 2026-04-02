Shader "Custom/OutlineForColor"
{
    Properties
    {
        _OutlineColor("描边颜色", Color) = (0,0,0,1)
        _OutlineWidth("描边粗细", Float) = 0.3
        _MoveToCamera("移向摄像机", Range(-20 , 20)) = 0
        [Toggle(_UseCameraPos)] _UseCameraPos("使用摄像机位置?", float) = 0
        _CameraPos("摄像机位置", Vector) = (0, 0, 0, 0)
        [Toggle(_UseTangent)] _UseTangent("使用切线数据?", float) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry"}

        Pass
        {
            Cull Front

            CGPROGRAM

            #include "UnityCG.cginc"

            #pragma vertex Vertex
            #pragma fragment Fragment

            #pragma multi_compile_local _ _UseCameraPos
            #pragma multi_compile_local _ _UseTangent

            struct VertexInput
            {
                half4 position : POSITION;
                half3 normal : NORMAL;
                float3 color : COLOR;
                half4 tangent : TANGENT;
            };

            struct VertexOutput
            {
                half4 position : SV_POSITION;
            };

            half _OutlineWidth;
            half _MoveToCamera;
            half4 _CameraPos;

            float3 VertexOffset(float Scale, float3 Vertex)
            {
                #ifndef _UseCameraPos
                    _CameraPos.xyz = _WorldSpaceCameraPos.xyz;
                #endif

                return normalize(_CameraPos.xyz - Vertex) * Scale;
            }

            VertexOutput Vertex(VertexInput input)
            {
                VertexOutput output;

                #ifdef _UseTangent
                    half3 ObjectNormal = input.tangent.xyz;
                #else
                    half3 ObjectNormal = input.color.xyz;
                #endif
                
                half3 WorldPosition = mul(unity_ObjectToWorld, input.position).xyz;
                half3 WorldNormal = UnityObjectToWorldNormal(ObjectNormal);
                half3 offset = VertexOffset(_MoveToCamera, WorldPosition);

                WorldPosition += _OutlineWidth * WorldNormal;
                WorldPosition += offset;
                output.position = mul(unity_MatrixVP, float4(WorldPosition, 1.0));

                return output;
            }

            half4 _OutlineColor;

            half4 Fragment(VertexOutput input) : SV_TARGET
            {
                return _OutlineColor;
            }

            ENDCG
        }
    }
}