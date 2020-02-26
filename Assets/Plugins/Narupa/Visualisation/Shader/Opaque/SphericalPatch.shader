Shader "NarupaXR/Spherical Patch"
{
    Properties
    {
        _Point1 ("Point1", Vector) = (1,-1,0,0)
        _Point2 ("Point2", Vector) = (-1,-1,-1,0)
        _Point3 ("Point3", Vector) = (-1,-1,1,0)
        _Axis ("Axis", Vector) = (0,1,0,0)
        _Depth ("Depth", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            
           
            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 origin : TEXCOORD2;
                float4 camera : TEXCOORD3;
            };
            
            float4 _Point1;
            float4 _Point2;
            float4 _Point3;
            
            float _Depth;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.origin = v.vertex;
                o.camera = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos.xyz, 1));
                return o;
            }
            
            struct fout {
                fixed4 col : SV_Target;
                fixed depth : SV_Depth;
            };
            
            
            void clip_spherical_triangle(float3 objPos) {
            
                float3 r = normalize(objPos);
                
                float3 r1 = normalize(_Point1.xyz);
                float3 r2 = normalize(_Point2.xyz);
                float3 r3 = normalize(_Point3.xyz);
                
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
            
            #include "../Depth.cginc"
            
            float4 _Axis;

            fout frag (v2f i)
            {
                fout o;
                
                float4 d = normalize(i.origin - i.camera);
                float4 p = i.origin;
                
                float dp = dot(p, d);
                
                float R = 0.5;
                
                float D = dp * dp - dot(p, p) + 1 + R * R;
                
                clip(D);
                
                float4 r = p + d * (-dp + sqrt(D));
                
                if(dot(r, _Axis) < -_Depth)
                    discard;
                
                clip_spherical_triangle(r);
                
                float4 n = -normalize(float4(r.xyz, 0));
                
                float4 worldPoint = mul(unity_ObjectToWorld, r);
                
                o.depth = calculateFragmentDepth(worldPoint);
                
               float4 worldNormal = normalize(mul(unity_ObjectToWorld, n));
                
                float4 l = normalize(_WorldSpaceLightPos0);
                float nl = dot(worldNormal, l);
                
                o.col = fixed4(1,1,1,1) * (0.5 + 0.5 * clamp(nl, -1, 1));
                
                return o;
            }
            
            ENDCG
        }
    }
}
