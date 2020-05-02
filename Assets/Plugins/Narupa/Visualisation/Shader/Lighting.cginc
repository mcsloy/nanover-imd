// Copyright (c) Intangible Realities Laboratory. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

/// Contains methods for lighting

#ifndef LIGHTING_CGINC_INCLUDED

    #define LIGHTING_CGINC_INCLUDED
    
    #define DIFFUSE(c, n, l, d) c * saturate(lerp(1, dot(n, l), d));
    
    float SmoothStep(float x, float blend) {
        if(blend > 0.98)
            return step(0.5, x);
        float c = 2.0 / (1.0 - blend * 0.999) - 1.0;
        if(x <= 0.5)
            return pow(x, c) / pow(0.5, c-1);
        else
            return 1 - pow(1-x, c) / pow(0.5, c-1);
    }
    
    
    fixed4 ColorSample(fixed4 color1, fixed4 color2, float t, float blend) {
        return lerp(color1, color2, SmoothStep(t, blend));
    }
    
#endif