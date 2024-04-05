Shader "Sand"
{
    Properties
    {
        _BackgroundColor ("Background Color", Color) = (0, 0, 0.1, 1)
        _SandColor ("Sand Color", Color) = (1, 1, 0.25, 1)
        _RockColor ("Rock Color", Color) = (0.2, 0.2, 0.2, 1)
        _WaterColor ("Water Color", Color) = (0, 0.5, 1, 1)
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
    }
    
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }

        Pass
        {
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv     : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _BackgroundColor;
            float4 _SandColor;
            float4 _RockColor;
            float4 _WaterColor;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 sandData = tex2D(_MainTex, i.uv);

                if (sandData.r == 1)
                    return _SandColor;
                if (sandData.r == 2)
                    return _RockColor;
                if (sandData.r == 3)
                    return _WaterColor;

                return _BackgroundColor;
            }
            
            ENDCG
        }
    }
}
