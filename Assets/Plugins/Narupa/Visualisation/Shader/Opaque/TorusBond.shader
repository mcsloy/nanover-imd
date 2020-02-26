// Copyright (c) Intangible Realities Laboratory. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

Shader "NarupaXR/Opaque/Torus Bond"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Diffuse ("Diffuse", Range(0, 1)) = 0.5
        _ParticleScale ("Particle Scale", Float) = 1
        _GradientWidth ("Gradient Width", Range(0, 1)) = 1
        _Radius ("Radius", Range(0, 1)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        
        Cull Front

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
           
            #pragma instancing_options procedural:setup
            
            #define POSITION_ARRAY
            #define EDGE_ARRAY
            #pragma multi_compile __ SCALE_ARRAY
            #pragma multi_compile __ COLOR_ARRAY
            #pragma multi_compile __ FILTER_ARRAY
            
            // Width of the gradient
            float _GradientWidth;
            
            // The scales to apply to the particles
            float _ParticleScale;
            
            // Color multiplier
            float4 _Color;
            
            // Diffuse factor (0 for flat, 1 for full diffuse)
            float _Diffuse;
            
            float _Radius;
            
            #include "UnityCG.cginc"
            #include "../Instancing.cginc"
            #include "../Transformation.cginc"
            #include "../Intersection.cginc"
            #include "../Depth.cginc"
            #include "../Lighting.cginc"
            
            void setup() {
                
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
                fixed4 color1 : TEXCOORD2;
                fixed4 color2 : TEXCOORD3;
                float3 a : TEXCOORD4;
                float3 o : TEXCOORD5;
            };
            
            v2f vert (appdata i)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(i);
                float overall_scale = length(ObjectToWorld._11_21_31);
                
                #if !defined(PROCEDURAL_INSTANCING_ON)
                    
                    
                    float3 p = unity_ObjectToWorld._14_24_34;
                     o.a = float3(0.5, 0, 0);
                     float scale = 1;
                #else
                    
                    float3 p1 = edge_position(0);
                    float3 p2 = edge_position(1);
                    
                    
                    float3 off = normalize(p2 - p1);
                
                   float rad1 = edge_scale(0) * _ParticleScale;
                   float rad2 = edge_scale(1) * _ParticleScale;
                   
                   float scale = max(rad1, rad2);
                   
                    setup_isotropic_edge_transformation(p1, p2, scale);
                    
                    float3 p = 0.5 * (p1 + p2);
                    
                    o.a = 0.5 * (p2 - p1);
                    
                    p = mul(ObjectToWorld, float4(p, 1)).xyz;
                    
                    o.a = mul(ObjectToWorld, float4(o.a, 0)).xyz;
                #endif
                
                
                o.vertex = UnityObjectToClipPos(i.vertex);
                
                float3 v = mul(unity_ObjectToWorld, i.vertex);
                float3 c = _WorldSpaceCameraPos.xyz;
                
                o.o = v.xyz;
                
                float s = scale * 0.5 * overall_scale;
                
                o.q = float4(v - p, s);
                o.d = float4(v - c, 0);
               
                o.color1 = _Color * edge_color(0);
                o.color2 = _Color * edge_color(1);
                return o;
            }
            
            
            struct fout {
                fixed4 color : SV_Target;
                float depth : SV_Depth;
            };
            
            #include "../Sturm.cginc"
            
            fout frag (v2f i)
            {
                fout o;
                float3 q = i.q.xyz;
                float3 d = normalize(i.d.xyz);
                float s = i.q.w;
                float3 a = i.a.xyz;
                
                float rho = _Radius;
                float halfD = length(a);
                float R = sqrt((s + rho) * (s + rho) - halfD * halfD);
                float phi = s / (s + rho);
                float Rinsec = sqrt(phi*phi*R*R + (1-phi)*(1-phi)*halfD*halfD);
               
                torus_hit hit = intersect_torus(q, d, R, Rinsec, rho, a);
                
                float t = hit.t;
                
                float3 n = normalize(-hit.normal);
                float3 l = normalize(_WorldSpaceLightPos0.xyz);
                float3 c = _WorldSpaceCameraPos.xyz;
                
                float proj = dot(q + d * t, a) / dot(a, a);
                
                float lerpt = 0.5 + 0.5 * proj;
    
                lerpt = clamp((lerpt - 0.5) / (_GradientWidth + 0.0001) + 0.5, 0, 1);
                o.color = DIFFUSE(lerp(i.color1, i.color2, lerpt), n, l, _Diffuse);
                            
                OUTPUT_FRAG_DEPTH(o, i.o + d * t);
                return o;
            }
            
            ENDCG
        }
    }
}