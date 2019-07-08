// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/AlphaMask" {
    Properties {
        _MainTex ("Base", 2D) = "white" {}
        _MaskTex ("Mask", 2D) = "white" {}
    }
    
    SubShader {
    
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
    
        Pass {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _MaskTex;
            float4 _Color;

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv1 : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
            };

            float4 _MainTex_ST;
            float4 _MaskTex_ST;

            v2f vert (appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos (v.vertex);
                o.uv1 = TRANSFORM_TEX (v.texcoord, _MainTex);
                o.uv2 = TRANSFORM_TEX (v.texcoord, _MaskTex);
                return o;
            }

            half4 frag (v2f i) : COLOR
            {
                half4 base = tex2D (_MainTex, i.uv1);
                half4 mask = tex2D (_MaskTex, i.uv2);
                base.w = mask.x * mask.x * mask.x;
                return base;
            }
        ENDCG
        
        }
    } 
    FallBack Off
}
