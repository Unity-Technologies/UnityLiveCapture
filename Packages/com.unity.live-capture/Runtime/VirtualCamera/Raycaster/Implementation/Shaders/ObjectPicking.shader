Shader "Hidden/LiveCapture/ObjectPicking"
{
    Properties
    {
        _ObjectID("ObjectID", Vector) = (0, 0, 0, 0)
    }

    SubShader
    {
        PackageRequirements
        {
            "com.unity.render-pipelines.core" : "10.2.2"
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.5
            #pragma multi_compile_instancing
            #pragma instancing_options assumeuniformscaling
            #include "ObjectPickingSRP.cginc"
            ENDHLSL
        }
    }

    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "ObjectPickingLegacy.cginc"
            ENDCG
        }
    }
}
