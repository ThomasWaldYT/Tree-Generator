Shader "Unlit/LineDrawShader"
{
    SubShader
    {
        Pass
        {
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // The buffers assigned from C#:
            StructuredBuffer<float3> linePositions;
            StructuredBuffer<float4> lineColors;

            struct appdata
            {
                uint vertexID : SV_VertexID;
            };

            struct v2f
            {
                float4 pos   : SV_POSITION;
                float4 color : COLOR;
            };

            v2f vert(appdata IN)
            {
                v2f OUT;
                // Find the correct index
                uint i = IN.vertexID;

                float3 worldPos = linePositions[i];
                float4 col      = lineColors[i];

                OUT.pos   = UnityObjectToClipPos(float4(worldPos, 1));
                OUT.color = col;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                return IN.color;
            }
            ENDCG
        }
    }
}
