Shader "Saleh/Circle" 
{
    Properties 
    {
        _Color ("Color", Color) = (1, 1, 1)
        _Radius ("Radius", Range(5,200)) = 30
        _LineWidth ("LineWidth", Range(1,100)) = 5
      
    }
    SubShader 
    {
        Tags { "RenderType"="Opaque" }
       
        CGPROGRAM
        #pragma surface surf Lambert
 
        fixed3 _Color;
        float3 _Center;
        int _LineWidth;
        int _Radius;
 
        struct Input 
        {
            float3 worldPos;
        };
 
        void surf (Input IN, inout SurfaceOutput o) 
        {   
            fixed4 center = (0,0,0,0);                              // Center at 0,0,0
            int dist = (int)distance(center, IN.worldPos);          // Distance of Current Pixel

            // Using Bitwise operators to check pixel lies between radius and radius+linewidth
            o.Albedo = _Color * (  ((((_Radius) + (~dist + 1)) >> 31) & 1)     &&    (!((((_Radius + _LineWidth) + (~dist + 1)) >> 31) & 1)));
            o.Alpha = 1;
        }
        ENDCG
    }
   
}