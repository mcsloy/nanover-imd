Shader "NarupaXR/Patch"
{
    Properties
    {
       _Radius ("Radius", Float) = 0.5
       _Flip ("Flip", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
             #pragma multi_compile_instancing
           
            #pragma instancing_options procedural:setup
            
            #define POSITION_ARRAY
            #define TRIPLE_ARRAY
            
            
            #include "UnityCG.cginc"
#include "../Transformation.cginc"
#include "../Instancing.cginc"
            float _Radius;
            
            float _Flip;

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
                float3 p : TEXCOORD2;
                float3 p0 : TEXCOORD3;
                float3 p1 : TEXCOORD4;
                float3 p2 : TEXCOORD5;
            };
            
            float3 sphere_pos(float3 p1, float3 p2, float3 p3, float R1, float R2, float R3) {
                
                float3 p12 = p2 - p1;
                float3 p13 = p3 - p1;
                
                float3 x = cross(p12, p13);
                
                float rho = _Radius*2;
                 
                float D12 = dot(p12, p12);
                float D13 = dot(p13, p13);
                float Q123 = dot(p12, p13);
                float D23 = D12 + D13 - Q123;
                
                float G1 = (rho + R1) * (rho + R1);
                float G2 = (rho + R2) * (rho + R2);
                float G3 = (rho + R3) * (rho + R3);
                
                float X2 = dot(x, x);
                
                float t2 = (D12 * D13 - D13 * (Q123 - G1 + G2) + Q123 * (-G1 + G3)) / (2.0 * X2);
                float t3 = (Q123 * (-G1 + G2) +  D12 * (D13 - Q123 + G1 - G3))/(2.0 * X2);
              
                float h2 = 1.0/(4.0 *  X2) * (-D12 * D12 * D13 + (-2 * Q123 * G1 + D13 * (G1 - G2)) * (2 * Q123 - G1 + G2) + 2 * Q123 * (-G1 + G2) * G3 - D12 * (D13 * D13 + (G1 - G3) * (-2 * Q123 + G1 - G3) - 2 * D13 * (Q123 + G2 + G3)));
              
                return (1 - t2 - t3) * p1 + t2 * p2 + t3 * p3 + _Flip * sqrt(h2) * normalize(x);
            }

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                
                
                Triple triple = instance_triple();
                float3 p0 = PositionArray[triple.a];
                float3 p1 = PositionArray[triple.b];
                float3 p2 = PositionArray[triple.c];
                float3 p = sphere_pos(p0,p1,p2,0.05,0.05,0.05);
                
                setup_isotropic_transformation((p0+p1+p2)/3.0, 0.3);
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                
                float3 w = mul(unity_ObjectToWorld, v.vertex);
                float3 c = _WorldSpaceCameraPos.xyz;
                float s = _Radius*2;
                
                
                o.q = float4(c - p, s);
                o.d = float4(w - c, 0);
                
                o.p = p;
                o.p0 = p0;
                o.p1 = p1;
                o.p2 = p2;
                
                return o;
            }
            
            struct fout {
                fixed4 color : SV_Target;
                float depth : SV_Depth;
            };
            
            void setup() {
                
            }
            
            #include "../Instancing.cginc"
            #include "../Transformation.cginc"
            #include "../Intersection.cginc"
            #include "../Depth.cginc"
            #include "../Lighting.cginc"

            
            void clip_spherical_triangle(float3 pos, float3 p1, float3 p2, float3 p3) {
            
                float3 r = normalize(pos);
                
                float3 r1 = normalize(p1.xyz);
                float3 r2 = normalize(p2.xyz);
                float3 r3 = normalize(p3.xyz);
                
                float3 v1 = float3(
                    r3.y * r2.z - r2.y * r3.z,
                    r3.z * r2.x - r2.z * r3.x,
                    r3.x * r2.y - r2.x * r3.y
                );
                
                float3 a1 = dot(v1, r) / dot(v1, r1);
                
                clip(a1);
                    
                float3 v2 = float3(
                    r3.y * r1.z - r1.y * r3.z,
                    r3.z * r1.x - r1.z * r3.x,
                    r3.x * r1.y - r1.x * r3.y
                );
                
                float3 a2 = dot(v2, r) / dot(v2, r2);
                
                clip(a2);
                
                float3 v3 = float3(
                    r2.y * r1.z - r1.y * r2.z,
                    r2.z * r1.x - r1.z * r2.x,
                    r2.x * r1.y - r1.x * r2.y
                );
                
                float3 a3 = dot(v3, r) / dot(v3, r3);
                
                clip(a3);
            
            }

            
            fout frag (v2f i)
            {
                fout o;
                
                float3 q = i.q.xyz;
                float3 d = i.d.xyz;
                float s = i.q.w;
                
                float4 rt = solve_ray_intersection2(q, d, s);
                float3 r = rt.xyz;
                float t = rt.w;
                
                float3 n = -normalize(r);
                float3 l = normalize(_WorldSpaceLightPos0.xyz);
                float3 c = _WorldSpaceCameraPos.xyz;
                
                r = c + d * t;
                
                float3 p = i.p;
                float3 p0 = i.p0;
                float3 p1 = i.p1;
                float3 p2 = i.p2;
                
                p0 = normalize(p0 - p);
                p1 = normalize(p1 - p);
                p2 = normalize(p2 - p);
                
                clip_spherical_triangle(-n, p0, p1, p2);
                
                o.color = DIFFUSE(fixed4(1,1,1,1), n, l, 0.5);
                OUTPUT_FRAG_DEPTH(o, c + d * t);
                return o;
            }
            
            ENDCG
        }
    }
}