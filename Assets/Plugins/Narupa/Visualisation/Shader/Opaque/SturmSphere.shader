// Copyright (c) Intangible Realities Laboratory. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

Shader "NarupaXR/Opaque/Sturm Sphere"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Scale ("Scale", Float) = 1
        _Diffuse ("Diffuse", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        
        Cull Front

        Pass
        {
            Name "Color"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
           
            #pragma instancing_options procedural:setup
            
            #define POSITION_ARRAY
            #pragma multi_compile __ SCALE_ARRAY
            #pragma multi_compile __ COLOR_ARRAY
            #pragma multi_compile __ FILTER_ARRAY
            
            
            #include "UnityCG.cginc"
            
            #include "../Instancing.cginc"
            #include "../Transformation.cginc"
            #include "../Intersection.cginc"
            #include "../Depth.cginc"
            #include "../Lighting.cginc"
            #include "../Sturm.cginc"
            
            float _Scale;
            float4 _Color;
            float _Diffuse;
            
            void setup() {
                setup_isotropic_transformation(instance_position(), instance_scale() * _Scale);
            }
            
            struct appdata
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 q : TEXCOORD0;
                float4 d : TEXCOORD1;
                fixed4 color : TEXCOORD2;
            };
            
            v2f vert (appdata i)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(i);
                
                o.vertex = UnityObjectToClipPos(i.vertex);
                
                float3 v = mul(unity_ObjectToWorld, i.vertex);
                float3 c = _WorldSpaceCameraPos.xyz;
                
                #if !defined(PROCEDURAL_INSTANCING_ON)
                    float3 p = unity_ObjectToWorld._14_24_34;
                #else
                    float3 p = mul(ObjectToWorld, float4(instance_position(), 1)).xyz;
                #endif
                float s = length(ObjectToWorld._11_21_31) * 0.5 * instance_scale() * _Scale;
                
                o.q = float4(c - p, s);
                o.d = float4(v - c, 0);
                o.color = _Color * instance_color();
                
                return o;
            }
            
            struct fout {
                fixed4 color : SV_Target;
                float depth : SV_Depth;
            };
            
            
            
            fout frag (v2f i)
            {
                fout o;
                float3 q = i.q.xyz;
                float3 d = normalize(i.d.xyz);
                float R = i.q.w;
                
                float qd = dot(q, d);
                float qq = dot(q, q);
                
                float tmid = -qd;
                
                float tspac = sqrt(qd*qd + R*R*2 - qq);
                
                float t = solve_quadratic_sturms_method(1, 2 * qd, qq - R*R, tmid - tspac, tmid + tspac);
                
                //t = - qd - sqrt(qd * qd + R*R - qq);
                
                float3 r = q + d * t;
                float3 n = normalize(r);
                
                float3 l = normalize(_WorldSpaceLightPos0.xyz);
                float3 c = _WorldSpaceCameraPos.xyz;
                
                o.color = DIFFUSE(i.color, n, l, _Diffuse);
                
                OUTPUT_FRAG_DEPTH(o, c + d * 1);
                return o;
            }
            
            ENDCG
        }
    }
}