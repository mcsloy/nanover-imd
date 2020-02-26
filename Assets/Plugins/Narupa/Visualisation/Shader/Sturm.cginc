// Copyright (c) Intangible Realities Laboratory. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

/// Contains methods for solving intersections using Sturm's Method

#ifndef STURM_CGINC_INCLUDED

    #define STURM_CGINC_INCLUDED
    
    float get_sign_variations(float a2, float a1, float a0, float b1, float b0, float c0, float t) {
            
        float v = 0;
        
        float s1 = sign(a2 * t * t + a1 * t + a0);
        float s2 = sign(b1 * t + b0);
        float s3 = sign(c0);
        
        v += max(-s1 * s2, 0);
        v += max(-s2 * s3, 0);
        
        return v;
    }
    
    struct quadric_sturm_sequence {
        float a4;
        float a3;
        float a2;
        float a1;
        float a0;
        float b3;
        float b2;
        float b1;
        float b0;
        float c2;
        float c1;
        float c0;
        float d1;
        float d0;
        float e0;
    };
    
    float get_sign_variations_quadric(quadric_sturm_sequence s, float t) {
    
        float v = 0;
        
        float s1 = sign(s.a4 * t * t * t * t + s.a3 * t * t * t + s.a2 * t * t + s.a1 * t + s.a0);
        float s2 = sign(s.b3 * t * t * t + s.b2 * t * t + s.b1 * t + s.b0);
        float s3 = sign(s.c2 * t * t + s.c1 * t + s.c0);
        float s4 = sign(s.d1 * t + s.d0);
        float s5 = sign(s.e0);
        
        v += max(-s1 * s2, 0);
        v += max(-s2 * s3, 0);
        v += max(-s3 * s4, 0);
        v += max(-s4 * s5, 0);
        
        return v;
    }
    
    
    
    float solve_quartic_sturms_method(float a4, float a3, float a2, float a1, float a0, float t0, float t1) {
        
        float b3 = 4.0 * a4;
        float b2 = 3.0 * a3;
        float b1 = 2.0 * a2;
        float b0 = a1;
        
        float c2 = - a2 / 2.0 + 3.0 * a3 * a3 / (16.0 * a4);
        float c1 = -3.0 * a1 / 4.0 + a2 * a3 / (8.0 * a4);
        float c0 = -a0 + a1 * a3 / (16.0 * a4);
        
        float d1 = -2.0 * a2 - 4.0 * a4 * c1 * c1 / (c2 * c2) + 4.0 * a4 * c0 / c2 + 3.0 * a3 * c1 / c2;
        float d0 = -a1 - 4.0 * a4 * c0 * c1 / (c2 * c2) + 3.0 * a3 * c0 / c2;
        
        float e0 = - c0 - c2 * d0 * d0 / (d1 * d1) + c1 * d0 / d1;
        
        quadric_sturm_sequence sturm;
        
        sturm.a4 = a4;
        sturm.a3 = a3;
        sturm.a2 = a2;
        sturm.a1 = a1;
        sturm.a0 = a0;
        sturm.b3 = b3;
        sturm.b2 = b2;
        sturm.b1 = b1;
        sturm.b0 = b0;
        sturm.c2 = c2;
        sturm.c1 = c1;
        sturm.c0 = c0;
        sturm.d1 = d1;
        sturm.d0 = d0;
        sturm.e0 = e0;
        
        float v0 = get_sign_variations_quadric(sturm, t0);
        float v1 = get_sign_variations_quadric(sturm, t1);
        
        if(v0 - v1 < 1)
            discard;
        
        for(int i = 0; i < 10; i++) {
            float tmid = 0.5 * (t0 + t1);
            float vmid = get_sign_variations_quadric(sturm, tmid);
            if(v0 > vmid) {
                v1 = vmid;
                t1 = tmid;
            }
            else {
                v0 = vmid;
                t0 = tmid;
            }
        }
        
        return 0.5 * (t0 + t1);
    }
    
    float solve_quadratic_sturms_method(float a2, float a1, float a0, float t0, float t1) {
        
        float b1 = 2 * a2;
        float b0 = a1;
        
        float c0 = a1 * a1 / (4 * a2) - a0;
        
        float v0 = get_sign_variations(a2, a1, a0, b1, b0, c0, t0);
        float v1 = get_sign_variations(a2, a1, a0, b1, b0, c0, t1);
        
        if(v0 - v1 < 1)
            discard;
        
        for(int i = 0; i < 2; i++) {
            float tmid = 0.5 * (t0 + t1);
            float vmid = get_sign_variations(a2, a1, a0, b1, b0, c0, tmid);
            if(v0 > vmid) {
                v1 = vmid;
                t1 = tmid;
            }
            else {
                v0 = vmid;
                t0 = tmid;
            }
        }
        
        return 0.5 * (t0 + t1);
    }
    
    struct torus_hit {
        float t;
        float3 normal;
    };
    
    torus_hit intersect_torus(float3 q, float3 d, float R, float Rinsec, float rho, float3 a) {
    
        d = normalize(d);
        
        float qd = dot(q, d);
        float qq = dot(q, q);
        
        float R2 = R * R;
        float rho2 = rho * rho;
        
        a = normalize(a);
        float da = dot(d, a);
        float qa = dot(q, a);
        
        float a4 = 1;
        float a3 = 4 * qd;
        float a2 = 4 * qd * qd + 2 * qq - 2 * R2 + 4 * da * da * R2 - 2 * rho2;
        float a1 = 4 * qd * qq + 8 * da * qa * R2 - 4 * qd * R2 - 4 * qd * rho2;
        float a0 =  qq * qq + 4 * qa * qa * R2 - 2 * qq * R2 + R2 * R2 - 2 * qq * rho2 - 2 * R2 * rho2 + rho2 * rho2;
        
        float tmid = -qd;
        
        float tspac = sqrt(qd*qd + Rinsec*Rinsec - qq);
        
        float tstart = tmid - tspac;
        
        if(rho > R) {
            float b2a = qd;
            float r2 = rho2 - R2;
            float ca = qq - r2;
            
            float det = b2a * b2a - ca;
            
            if(det > 0) {
                tstart = -b2a + sqrt(det);
            }
        }
        
        float t = solve_quartic_sturms_method(a4, a3, a2, a1, a0, tstart, tmid + tspac);
        
        //t = - qd - sqrt(qd * qd + R*R - qq);
        
        float3 r = q + d * t;
        
        float3 n = -8 * R2 * (r - dot(a, r) * a) + 4 * r * (dot(r, r) + R2 - rho2);
        
        n = normalize(n);
        
        torus_hit hit;
        hit.t = t;
        hit.normal = n;
        
        return hit;
    }

#endif
