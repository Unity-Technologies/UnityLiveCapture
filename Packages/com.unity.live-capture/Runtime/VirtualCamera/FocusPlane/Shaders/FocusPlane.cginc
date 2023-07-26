float _CameraDepthThreshold;
float4 _Color;
float _CellSize;
float _IntersectionLineWidth;
float _GridOpacity;
float _BackgroundOpacity;

float4 FocusPlane(float eyeSpaceDepth, float2 positionNDC, float screenAspect)
{
    // Relative camera-space depth to the focus distance.
    float deltaDepth = eyeSpaceDepth - _CameraDepthThreshold;

    // We are in front of the focus plane, nothing to render.
    if (deltaDepth < _IntersectionLineWidth * -0.5)
        return float4(0, 0, 0, 0);

    // At this point we have discarded fragments who are in front of the focus plane by a margin corresponding to (_IntersectionLineWidth * -0.5).
    // We need the grid to be *precisely* at the focus distance, hence the use of (beyondDepthThreshold).
    float beyondDepthThreshold = deltaDepth > 0;

    // Evaluate the intersection with scene geometry, which is characterized by a very small deltaDepth,
    // aka "this fragment's depth is approximately the focus distance".
    float edge = pow(lerp(
        smoothstep(-0.5 * _IntersectionLineWidth, 0, deltaDepth),
        smoothstep(0.5 * _IntersectionLineWidth, 0, deltaDepth), beyondDepthThreshold), 4);

    float opacity = edge + _BackgroundOpacity * beyondDepthThreshold;

    #if defined(USE_GRID)
    // Offset position so that scaling is centered, apply aspect so that the grid is regular.
    float2 scaled = (positionNDC - 0.5) * float2(screenAspect, 1) * _CellSize;
    float2 steps = abs(frac(scaled) - 0.5) * 2;

    // Line width control.
    float2 df = fwidth(scaled) * 2;
    float2 grid2 = smoothstep(-df ,df , steps);
    float grid = 1.0 - saturate(grid2.x * grid2.y);
    grid *= beyondDepthThreshold;
    opacity += grid * _GridOpacity;
    #endif

    return float4(_Color.rgb, _Color.a * opacity);
}
