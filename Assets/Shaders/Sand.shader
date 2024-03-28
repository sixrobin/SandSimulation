Shader "Sand"
{
    Properties
    {
        _SandColor ("Sand Color", Color) = (1, 1, 0.25, 1)
        _BackgroundColor ("Background Color", Color) = (0, 0, 0.1, 1)
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

            float4 _SandColor;
            float4 _BackgroundColor;
            
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
                return lerp(_BackgroundColor, _SandColor, sandData.x);
            }
            
            ENDCG
        }
    }
}
