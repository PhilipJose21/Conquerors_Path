Shader "Hidden/Outline"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            Name "OUTLINE"
            Cull Front
            ZWrite Off
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float _OutlineThickness;
            float4 _OutlineColor;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;

                float3 norm = normalize(v.normal);
                o.pos = UnityObjectToClipPos(v.vertex + float4(norm * _OutlineThickness, 0));

                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                return _OutlineColor;
            }

            ENDHLSL
        }
    }
}