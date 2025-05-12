Shader "Custom/DepthMask"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry-1" }
        Cull Front       // <-- this allows both front and back faces to render
        Cull Back       // <-- this allows both front and back faces to render
        ColorMask 0
        ZWrite On
        Pass {}
    }
}
