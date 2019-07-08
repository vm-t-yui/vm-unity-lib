// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Simple Unlit Outline" {
    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (0.315,0.06,0.06,1)
        _Outline ("Outline width", Float) = 1
        _MainTex ("Base (RGB)", 2D) = "white" { }
    }

    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        UsePass "Unlit/Transparent/BASE"
        Pass {
            Name "OUTLINE"
            Tags { "LightMode" = "Always" }
            Cull Front
            ZWrite Off
            ColorMask RGB
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f {
                float4 pos : POSITION;
                float4 color : COLOR;
            };
            
            uniform float _Outline;
            uniform float4 _Color;
            uniform float4 _OutlineColor;
            
            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                float3 norm   = mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal);
                float2 offset = TransformViewToProjection(norm.xy);

                o.pos.xy += offset * _Outline * 0.015;
                //o.pos.xy += offset * o.pos.z * _Outline * 0.001;
                o.color = _OutlineColor;
                o.color.a = _Color.a;
                return o;
            }

            half4 frag(v2f i) :COLOR {
                return i.color; 
            }
            
            ENDCG
        }
    }
    
    Fallback "Unlit/Transparent"
}
