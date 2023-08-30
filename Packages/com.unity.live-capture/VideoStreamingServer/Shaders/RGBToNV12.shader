Shader "Hidden/Live Capture/RGBToNV12"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ VERTICAL_FLIP

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv     : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            UNITY_DECLARE_TEX2D(_MainTex);
            float4 _SrcTex_ScaleOffset;
            float4 _DstTex_TexelSize;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // Adobe-flavored HDTV Rec.709 (2.2 gamma, 16-235 limit)
            fixed3 RGB2YUV(half3 rgb)
            {
                const half K_B = 0.0722;
                const half K_R = 0.2126;

#if !UNITY_COLORSPACE_GAMMA
                rgb = LinearToGammaSpace(rgb);
#endif
                half y = dot(half3(K_R, 1 - K_B - K_R, K_B), rgb);
                half u = ((rgb.b - y) / (1 - K_B) * 112 + 128) / 255;
                half v = ((rgb.r - y) / (1 - K_R) * 112 + 128) / 255;

                y = (y * 219 + 16) / 255;

                return fixed3(y, u, v);
            }

            half3 sampleMainTex(float2 uv)
            {
#if defined(VERTICAL_FLIP)
                uv.y = 1.0 - uv.y;
#endif
                return UNITY_SAMPLE_TEX2D(_MainTex, uv);
            }

            fixed3 RGB2YUV601(half3 rgb)
            {
#if !UNITY_COLORSPACE_GAMMA
                rgb = LinearToGammaSpace(rgb);
#endif
                half y = ( 16.0F/255.0F + 0.258348F * rgb.r + 0.50676F  * rgb.g + 0.099756F * rgb.b);
                half u = (128.0F/255.0F - 0.150534F * rgb.r - 0.295278F * rgb.g + 0.445811F * rgb.b);
                half v = (128.0F/255.0F + 0.440022F * rgb.r - 0.367650F * rgb.g - 0.072372F * rgb.b);

                return fixed3(y, u, v);
            }

            fixed frag(v2f i) : SV_Target
            {
                if (3.0F * i.uv.y < 2.0F)
                {
                    // Y
                    i.uv.y = i.uv.y * 1.5F;
                    return RGB2YUV(sampleMainTex(mad(i.uv, _SrcTex_ScaleOffset.xy, _SrcTex_ScaleOffset.zw))).r;
                }

                float2 halfTexelSize = _DstTex_TexelSize.xy * 0.5F;

                // Move the y coordinate back into the main image to do the chroma subsampling.
                i.uv.y = 3.0F * i.uv.y - 2.0F;
                i.uv.y += halfTexelSize.y; // Offset by 1/2 line to sample in the middle of the 4 pixels.

                int x = (int)floor(i.uv.x * (_DstTex_TexelSize.z - 0.5F) + 0.5F);
                if (fmod(x, 2.0F) == 0.0F)
                {
                   // Even columns: U
                   // Get x position between current pixel and the one to the right, so U and V are sampled at the same column.
                   i.uv.x += halfTexelSize.x;
                   return RGB2YUV(sampleMainTex(mad(i.uv, _SrcTex_ScaleOffset.xy, _SrcTex_ScaleOffset.zw))).g;
                }

                // Odd columns: V
                // Get x position between current pixel and the one to the left, so U and V are sampled at the same column.
                i.uv.x -= halfTexelSize.x;
                return RGB2YUV(sampleMainTex(mad(i.uv, _SrcTex_ScaleOffset.xy, _SrcTex_ScaleOffset.zw))).b;
            }

            ENDCG
        }
    }
}
