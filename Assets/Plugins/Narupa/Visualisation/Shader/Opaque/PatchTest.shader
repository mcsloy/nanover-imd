Shader "NarupaXR/Patch Test"
{
    Properties
    {
       _Radius ("Radius", Float) = 0.5
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
            
            #include "UnityCG.cginc"

            float _Radius;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 q : TEXCOORD0;
                float4 d : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                float3 w = mul(unity_ObjectToWorld, v.vertex);
                float3 c = _WorldSpaceCameraPos.xyz;
                float3 p = float3(0, 0.5, 0);
                float s = _Radius;
                
                
                o.q = float4(c - p, s);
                o.d = float4(w - c, 0);
                
                return o;
            }
            
            struct fout {
                fixed4 color : SV_Target;
                float depth : SV_Depth;
            };
            
            
#include "../Instancing.cginc"
#include "../Transformation.cginc"
#include "../Intersection.cginc"
#include "../Depth.cginc"
#include "../Lighting.cginc"

            
            // Moller-Trumbore Intersection
            int RayIntersectsTriangle(float3 rayOrigin, 
                           float3 rayVector, 
                           float3 vertex0,
                           float3 vertex1,
                           float3 vertex2)
            {
                const float EPSILON = 0.0000001;
                float3 edge1 = vertex1 - vertex0;
                float3 edge2 = vertex2 - vertex0;
                float h = cross(rayVector, edge2);
                float a = dot(edge1, h);
                if (a > -EPSILON && a < EPSILON)
                    return 0;    // This ray is parallel to this triangle.
                float f = 1.0/a;
                float3 s = rayOrigin - vertex0;
                float u = f * dot(s, h);
                if (u < 0.0 || u > 1.0)
                    return 0;
                float3 q = cross(s, edge1);
                float v = f * dot(rayVector, q);
                if (v < 0.0 || u + v > 1.0)
                    return 0;
                // At this stage we can compute t to find out where the intersection point is on the line.
                float t = f * dot(edge2, q);
                if (t > EPSILON) // ray intersection
                {
                    return 1;
                }
                else // This means that there is a line intersection but not a ray intersection.
                    return 0;
            }
            
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
                
                float3 p = float3(0, 0.5, 0);
                float3 p0 = float3(0.5,0,0);
                float3 p1 = float3(-0.2, 0, 0.4);
                float3 p2 = float3(-0.3, 0, -0.3);
                
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