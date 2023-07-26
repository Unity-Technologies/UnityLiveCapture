// Camera matrix is:
// | fx  0 cx |
// |  0 fy cy |
// |  0  0  0 |
//
// Normalized coords to pixels:
// Xn = (Xp - cx) / fx
// Yn = (Yp - cy) / fy
//
// Pixels to normalized coords:
// Xp = Xn.fx + cx
// Yp = Yn.fy + cy
//
// with:
// cx, cy: principal, normalized resolution
// fx, fy: focal length, normalized resolution
// (those are camera intrinsinc parameters)

float3 _RadialLensDistParams;
float2 _TangentialLensDistParams;
float2 _PrincipalPoint;
float2 _DistortedFocalLength;
float2 _UndistortedFocalLength;
    
// Brown-Conrady distortion model.
float2 EvaluateDistortionModel(float2 coords)
{
    float2 coords2 = coords * coords;
    float r2 = coords2.x + coords2.y;

    float k1 = _RadialLensDistParams.x;
    float k2 = _RadialLensDistParams.y;
    float k3 = _RadialLensDistParams.z;

    float radial = (1 + r2 * (k1 + r2 * (k2 + r2 * k3)));

    float p1 = _TangentialLensDistParams.x;
    float p2 = _TangentialLensDistParams.y;

    float xy = coords.x * coords.y;

    float2 tangential = float2(
        2 * p1 * xy + p2 * (r2 + 2 * coords2.x),
        p1 * (r2 + 2 * coords2.y) + 2 * p2 * xy);

    return coords * radial + tangential;
}

float2 ImagePlaneToCamera(float2 coords)
{
    return (coords - _PrincipalPoint.xy) / _DistortedFocalLength.xy;
}

float2 CameraToImagePlane(float2 coords)
{
    return coords * _UndistortedFocalLength.xy + _PrincipalPoint.xy;
}

float2 UndistortUV(float2 uv)
{
    float2 coords = ImagePlaneToCamera(uv);
    coords = EvaluateDistortionModel(coords);
    return CameraToImagePlane(coords);
}