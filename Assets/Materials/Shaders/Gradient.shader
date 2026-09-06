Shader "Custom/Gradient"
{
    Properties
    {
        _GradientLUT ("Gradient LUT", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            //#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            //#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata
            {
                float2 uv : TEXCOORD0;
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 vertex : SV_POSITION;
            };

            sampler2D _GradientLUT;
            float4 _GradientLUT_ST;

            v2f vert (appdata v)
            {
                v2f o;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS.xyz);
                o.vertex = vertexInput.positionCS;

                VertexNormalInputs normalInput = GetVertexNormalInputs(v.normalOS, v.tangentOS);
                o.normal = normalInput.normalWS;

                o.uv = TRANSFORM_TEX(v.uv, _GradientLUT);

                return o;
            }

            float4 frag (v2f i) : SV_Target
            {

                // get the light
                Light light = GetMainLight();
                float3 lightDir = light.direction;

                // get the dot product of the surface normal and the light
                float nDotL = dot(i.normal, lightDir);

                // sample the texture using nDotL as the x coord (y doesnt matter as the gradient texture extends only in width)
                float4 col = tex2D(_GradientLUT, float2(nDotL, 0));

                return col;
            }
            ENDHLSL
        }
    }
}
