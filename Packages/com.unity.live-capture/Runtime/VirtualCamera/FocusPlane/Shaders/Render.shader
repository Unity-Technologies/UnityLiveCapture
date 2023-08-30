Shader "Hidden/LiveCapture/FocusPlane/Render"
{
    SubShader
    {
        PackageRequirements
        {
            "com.unity.render-pipelines.high-definition" : "10.2.2"
        }

        ZTest Always
        ZWrite Off
        Blend Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma target 4.5
            #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ USE_GRID
            #include "HdrpRender.cginc"
            ENDHLSL
        }
    }

    SubShader
    {
        PackageRequirements
        {
            "com.unity.render-pipelines.universal" : "10.2.2"
        }

        ZTest Always
        ZWrite Off
        Blend Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ USE_GRID
            #include "UrpRender.cginc"
            ENDHLSL
        }
    }

    SubShader
    {
        ZTest Always
        ZWrite Off
        Blend Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ USE_GRID
            #include "LegacyRender.cginc"
            ENDCG
        }
    }
}
