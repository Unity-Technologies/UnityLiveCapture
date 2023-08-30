using System;
using UnityEngine;

namespace Unity.LiveCapture
{
    static class MathUtility
    {
        enum RotationOrder { OrderXYZ, OrderXZY, OrderYZX, OrderYXZ, OrderZXY, OrderZYX }

        const float k_EpsilonLegacyEuler = 1.0E-3f;
        static readonly Vector4[] k_RotationOrderLUT =
        {
            new Vector4(1f, 1f, 1f, 1f), new Vector4(-1f, 1f, -1f, 1f), //XYZ
            new Vector4(1f, 1f, 1f, 1f), new Vector4(1f, 1f, -1f, -1f), //XZY
            new Vector4(1f, -1f, 1f, 1f), new Vector4(-1f, 1f, 1f, 1f), //YZX
            new Vector4(1f, 1f, 1f, 1f), new Vector4(-1f, 1f, 1f, -1f), //YXZ
            new Vector4(1f, -1f, 1f, 1f), new Vector4(1f, 1f, -1f, 1f), //ZXY
            new Vector4(1f, -1f, 1f, 1f), new Vector4(1f, 1f, 1f, -1f) //ZYX
        };

        static Vector3 XZY(this Vector3 v) => new Vector3(v.x, v.z, v.y);
        static Vector3 YZX(this Vector3 v) => new Vector3(v.y, v.z, v.x);
        static Vector3 YXZ(this Vector3 v) => new Vector3(v.y, v.x, v.z);
        static Vector3 ZXY(this Vector3 v) => new Vector3(v.z, v.x, v.y);
        static Vector3 ZYX(this Vector3 v) => new Vector3(v.z, v.y, v.x);
        static Vector3 XYZ(this Vector4 v) => new Vector3(v.x, v.y, v.z);
        static Vector4 XYXY(this Vector4 v) => new Vector4(v.x, v.y, v.x, v.y);
        static Vector4 XWYZ(this Vector4 v) => new Vector4(v.x, v.w, v.y, v.z);
        static Vector4 YYWW(this Vector4 v) => new Vector4(v.y, v.y, v.w, v.w);
        static Vector4 YWZX(this Vector4 v) => new Vector4(v.y, v.w, v.z, v.x);
        static Vector4 YZXW(this Vector4 v) => new Vector4(v.y, v.z, v.x, v.w);
        static Vector4 ZXZX(this Vector4 v) => new Vector4(v.z, v.x, v.z, v.x);
        static Vector4 ZZWW(this Vector4 v) => new Vector4(v.z, v.z, v.w, v.w);
        static Vector4 ZWXY(this Vector4 v) => new Vector4(v.z, v.w, v.x, v.y);
        static Vector4 WWWW(this Vector4 v) => new Vector4(v.w, v.w, v.w, v.w);
        static Vector4 WXYZ(this Vector4 v) => new Vector4(v.w, v.x, v.y, v.z);
        static Vector4 WZXY(this Vector4 v) => new Vector4(v.w, v.z, v.x, v.y);
        static float Chgsign(float x, float y) => y < 0 ? -x : x;
        static Vector3 Chgsign(Vector3 x, Vector3 y) => new Vector3(Chgsign(x.x, y.x), Chgsign(x.y, y.y), Chgsign(x.z, y.z));
        static Vector4 Chgsign(Vector4 x, Vector4 y) => new Vector4(Chgsign(x.x, y.x), Chgsign(x.y, y.y), Chgsign(x.z, y.z), Chgsign(x.w, y.w));
        static Vector3 Mul(this Vector3 a, Vector3 b) => Vector3.Scale(a, b);
        static Vector4 Mul(this Vector4 a, Vector4 b) => Vector4.Scale(a, b);
        static Vector3 Div(this Vector3 a, Vector3 b) => new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        static Vector3 Round(Vector3 v) => new Vector3(Mathf.Round(v.x), Mathf.Round(v.y), Mathf.Round(v.z));
        static float Degrees(float v) => v * Mathf.Rad2Deg;
        static Vector3 Degrees(Vector3 v) => new Vector3(Degrees(v.x), Degrees(v.y), Degrees(v.z));
        static float Radians(float v) => v * Mathf.Deg2Rad;
        static Vector3 Radians(Vector3 v) => new Vector3(Radians(v.x), Radians(v.y), Radians(v.z));
        static Vector3 Select(Vector3 a, Vector3 b, bool c) => c ? b : a;
        static float CSum(Vector4 v) => v.x + v.y + v.z + v.w;
        static void SinCos(Vector3 v, out Vector3 s, out Vector3 c)
        {
            s = new Vector3(Mathf.Sin(v.x), Mathf.Sin(v.y), Mathf.Sin(v.z));
            c = new Vector3(Mathf.Cos(v.x), Mathf.Cos(v.y), Mathf.Cos(v.z));
        }

        static Vector3 EulerReorder(Vector3 euler, RotationOrder order)
        {
            switch (order)
            {
                case RotationOrder.OrderXYZ:
                    return euler;
                case RotationOrder.OrderXZY:
                    return euler.XZY();
                case RotationOrder.OrderYZX:
                    return euler.YZX();
                case RotationOrder.OrderYXZ:
                    return euler.YXZ();
                case RotationOrder.OrderZXY:
                    return euler.ZXY();
                case RotationOrder.OrderZYX:
                    return euler.ZYX();
            }

            throw new ArgumentException("invalid rotationOrder");
        }

        static Vector3 EulerReorderBack(Vector3 euler, RotationOrder order)
        {
            switch (order)
            {
                case RotationOrder.OrderXYZ:
                    return euler;
                case RotationOrder.OrderXZY:
                    return euler.XZY();
                case RotationOrder.OrderYZX:
                    return euler.ZXY();
                case RotationOrder.OrderYXZ:
                    return euler.YXZ();
                case RotationOrder.OrderZXY:
                    return euler.YZX();
                case RotationOrder.OrderZYX:
                    return euler.ZYX();
            }

            throw new ArgumentException("invalid rotationOrder");
        }

        static Vector3 QuatToEuler(Vector4 q, RotationOrder order)
        {
            //prepare the data
            Vector4 d1 = q.Mul(q.WWWW()) * 2f; //xw, yw, zw, ww
            Vector4 d2 = q.Mul(q.YZXW()) * 2f; //xy, yz, zx, ww
            Vector4 d3 = q.Mul(q);
            Vector3 euler = Vector3.zero;

            const float CUTOFF = (1f - 2f * float.Epsilon) * (1f - 2f * float.Epsilon);

            switch (order)
            {
                case RotationOrder.OrderZYX: //ZYX
                {
                    float y1 = d2.z + d1.y;
                    if (y1 * y1 < CUTOFF)
                    {
                        float x1 = -d2.x + d1.z;
                        float x2 = d3.x + d3.w - d3.y - d3.z;
                        float z1 = -d2.y + d1.x;
                        float z2 = d3.z + d3.w - d3.y - d3.x;
                        euler = new Vector3(Mathf.Atan2(x1, x2), Mathf.Asin(y1), Mathf.Atan2(z1, z2));
                    }
                    else     //zxz
                    {
                        y1 = Mathf.Clamp(y1, -1.0f, 1.0f);
                        Vector4 abcd = new Vector4(d2.z, d1.y, d2.y, d1.x);
                        float x1 = 2.0f * (abcd.x * abcd.w + abcd.y * abcd.z);     //2(ad+bc)
                        float x2 = CSum(abcd.Mul(abcd).Mul(new Vector4(-1f, 1f, -1f, 1f)));
                        euler = new Vector3(Mathf.Atan2(x1, x2), Mathf.Asin(y1), 0f);
                    }
                    break;
                }
                case RotationOrder.OrderZXY: //ZXY
                {
                    float y1 = d2.y - d1.x;
                    if (y1 * y1 < CUTOFF)
                    {
                        float x1 = d2.x + d1.z;
                        float x2 = d3.y + d3.w - d3.x - d3.z;
                        float z1 = d2.z + d1.y;
                        float z2 = d3.z + d3.w - d3.x - d3.y;
                        euler = new Vector3(Mathf.Atan2(x1, x2), -Mathf.Asin(y1), Mathf.Atan2(z1, z2));
                    }
                    else     //zxz
                    {
                        y1 = Mathf.Clamp(y1, -1f, 1f);
                        Vector4 abcd = new Vector4(d2.z, d1.y, d2.y, d1.x);
                        float x1 = 2f * (abcd.x * abcd.w + abcd.y * abcd.z);     //2(ad+bc)
                        float x2 = CSum(abcd.Mul(abcd).Mul(new Vector4(-1f, 1f, -1f, 1f)));
                        euler = new Vector3(Mathf.Atan2(x1, x2), -Mathf.Asin(y1), 0f);
                    }
                    break;
                }
                case RotationOrder.OrderYXZ: //YXZ
                {
                    float y1 = d2.y + d1.x;
                    if (y1 * y1 < CUTOFF)
                    {
                        float x1 = -d2.z + d1.y;
                        float x2 = d3.z + d3.w - d3.x - d3.y;
                        float z1 = -d2.x + d1.z;
                        float z2 = d3.y + d3.w - d3.z - d3.x;
                        euler = new Vector3(Mathf.Atan2(x1, x2), Mathf.Asin(y1), Mathf.Atan2(z1, z2));
                    }
                    else     //yzy
                    {
                        y1 = Mathf.Clamp(y1, -1f, 1f);
                        Vector4 abcd = new Vector4(d2.x, d1.z, d2.y, d1.x);
                        float x1 = 2.0f * (abcd.x * abcd.w + abcd.y * abcd.z);     //2(ad+bc)
                        float x2 = CSum(abcd.Mul(abcd).Mul(new Vector4(-1f, 1f, -1f, 1f)));
                        euler = new Vector3(Mathf.Atan2(x1, x2), Mathf.Asin(y1), 0f);
                    }
                    break;
                }
                case RotationOrder.OrderYZX: //YZX
                {
                    float y1 = d2.x - d1.z;
                    if (y1 * y1 < CUTOFF)
                    {
                        float x1 = d2.z + d1.y;
                        float x2 = d3.x + d3.w - d3.z - d3.y;
                        float z1 = d2.y + d1.x;
                        float z2 = d3.y + d3.w - d3.x - d3.z;
                        euler = new Vector3(Mathf.Atan2(x1, x2), -Mathf.Asin(y1), Mathf.Atan2(z1, z2));
                    }
                    else     //yxy
                    {
                        y1 = Mathf.Clamp(y1, -1f, 1f);
                        Vector4 abcd = new Vector4(d2.x, d1.z, d2.y, d1.x);
                        float x1 = 2f * (abcd.x * abcd.w + abcd.y * abcd.z);     //2(ad+bc)
                        float x2 = CSum(abcd.Mul(abcd).Mul(new Vector4(-1f, 1f, -1f, 1f)));
                        euler = new Vector3(Mathf.Atan2(x1, x2), -Mathf.Asin(y1), 0f);
                    }
                    break;
                }

                case RotationOrder.OrderXZY: //XZY
                {
                    float y1 = d2.x + d1.z;
                    if (y1 * y1 < CUTOFF)
                    {
                        float x1 = -d2.y + d1.x;
                        float x2 = d3.y + d3.w - d3.z - d3.x;
                        float z1 = -d2.z + d1.y;
                        float z2 = d3.x + d3.w - d3.y - d3.z;
                        euler = new Vector3(Mathf.Atan2(x1, x2), Mathf.Asin(y1), Mathf.Atan2(z1, z2));
                    }
                    else     //xyx
                    {
                        y1 = Mathf.Clamp(y1, -1f, 1f);
                        Vector4 abcd = new Vector4(d2.x, d1.z, d2.z, d1.y);
                        float x1 = 2f * (abcd.x * abcd.w + abcd.y * abcd.z);     //2(ad+bc)
                        float x2 = CSum(abcd.Mul(abcd).Mul(new Vector4(-1f, 1f, -1f, 1f)));
                        euler = new Vector3(Mathf.Atan2(x1, x2), Mathf.Asin(y1), 0f);
                    }
                    break;
                }
                case RotationOrder.OrderXYZ: //XYZ
                {
                    float y1 = d2.z - d1.y;
                    if (y1 * y1 < CUTOFF)
                    {
                        float x1 = d2.y + d1.x;
                        float x2 = d3.z + d3.w - d3.y - d3.x;
                        float z1 = d2.x + d1.z;
                        float z2 = d3.x + d3.w - d3.y - d3.z;
                        euler = new Vector3(Mathf.Atan2(x1, x2), -Mathf.Asin(y1), Mathf.Atan2(z1, z2));
                    }
                    else     //xzx
                    {
                        y1 = Mathf.Clamp(y1, -1f, 1f);
                        Vector4 abcd = new Vector4(d2.z, d1.y, d2.x, d1.z);
                        float x1 = 2f * (abcd.x * abcd.w + abcd.y * abcd.z);     //2(ad+bc)
                        float x2 = CSum(abcd.Mul(abcd).Mul(new Vector4(-1f, 1f, -1f, 1f)));
                        euler = new Vector3(Mathf.Atan2(x1, x2), -Mathf.Asin(y1), 0f);
                    }
                    break;
                }
            }

            return EulerReorderBack(euler, order);
        }

        static Vector4 EulerToQuat(Vector3 euler, RotationOrder order = RotationOrder.OrderXYZ)
        {
            Vector3 c, s;
            SinCos(euler * 0.5f, out s, out c);

            Vector4 t = new Vector4(s.x * c.z, s.x * s.z, c.x * s.z, c.x * c.z);

            return c.y * t.Mul(k_RotationOrderLUT[2 * (int)order]) + s.y * k_RotationOrderLUT[2 * (int)order + 1].Mul(t.ZWXY());
        }

        static Vector4 QuatMul(Vector4 q1, Vector4 q2)
        {
            return Chgsign(
                (q1.YWZX().Mul(q2.XWYZ()) -
                    q1.WXYZ().Mul(q2.ZXZX()) -
                    q1.ZZWW().Mul(q2.WZXY()) -
                    q1.XYXY().Mul(q2.YYWW())).ZWXY(), new Vector4(-1f, -1f, -1f, 1f));
        }

        static float QuatDiff(Vector4 a, Vector4 b)
        {
            float diff = Mathf.Asin(QuatMul(QuatConj(a), b).normalized.XYZ().magnitude);
            return diff + diff;
        }

        static Vector4 QuatConj(Vector4 q)
        {
            return Chgsign(q, new Vector4(-1f, -1f, -1f, 1f));
        }

        static Vector3 AlternateEuler(Vector3 euler, RotationOrder rotationOrder)
        {
            Vector3 eulerAlt = EulerReorder(euler, rotationOrder);
            eulerAlt += new Vector3(180f, 180f, 180f);
            eulerAlt = Chgsign(eulerAlt, new Vector3(1f, -1f, 1f));
            return EulerReorderBack(eulerAlt, rotationOrder);
        }

        static Vector3 SyncEuler(Vector3 euler, Vector3 eulerHint)
        {
            return euler + Round((eulerHint - euler).Div(new Vector3(360f, 360f, 360f))).Mul(new Vector3(360f, 360f, 360f));
        }

        static Vector3 ClosestEuler(Vector3 euler, Vector3 eulerHint, RotationOrder rotationOrder)
        {
            Vector3 eulerSynced = SyncEuler(euler, eulerHint);
            Vector3 altEulerSynced = SyncEuler(AlternateEuler(euler, rotationOrder), eulerHint);

            Vector3 diff = eulerSynced - eulerHint;
            Vector3 altDiff = altEulerSynced - eulerHint;

            return Select(altEulerSynced, eulerSynced, Vector3.Dot(diff, diff) < Vector3.Dot(altDiff, altDiff));
        }

        static Vector3 ClosestEuler(Vector4 q, Vector3 eulerHint, RotationOrder rotationOrder)
        {
            var eps = new Vector3(k_EpsilonLegacyEuler, k_EpsilonLegacyEuler, k_EpsilonLegacyEuler);
            Vector3 euler = Degrees(QuatToEuler(q, rotationOrder));
            euler = Round(euler.Div(eps)).Mul(eps);
            Vector4 qHint = EulerToQuat(Radians(eulerHint), rotationOrder);
            float angleDiff = Degrees(QuatDiff(q, qHint));

            return Select(ClosestEuler(euler, eulerHint, rotationOrder), eulerHint, angleDiff < k_EpsilonLegacyEuler);
        }

        static Vector3 ClosestEuler(Quaternion quaternion, Vector3 eulerHint, RotationOrder rotationOrder)
        {
            return ClosestEuler(new Vector4(quaternion.x, quaternion.y, quaternion.z, quaternion.w), eulerHint, rotationOrder);
        }

        public static Vector3 ClosestEuler(Quaternion quaternion, Vector3 eulerHint)
        {
            return ClosestEuler(quaternion, eulerHint, RotationOrder.OrderZXY);
        }

        public static double Clamp(double value, double min, double max)
        {
            return Math.Max(Math.Min(value, max), min);
        }

        public static Quaternion Add(this in Quaternion a, in Quaternion b)
        {
            return new Quaternion(
                a.x + b.x,
                a.y + b.y,
                a.z + b.z,
                a.w + b.w
            );
        }

        public static Quaternion Sub(this in Quaternion a, in Quaternion b)
        {
            return new Quaternion(
                a.x - b.x,
                a.y - b.y,
                a.z - b.z,
                a.w - b.w
            );
        }

        public static Quaternion Mul(this in Quaternion a, float s)
        {
            return new Quaternion(
                a.x * s,
                a.y * s,
                a.z * s,
                a.w * s
            );
        }

        public static Quaternion Div(this in Quaternion a, float s)
        {
            return new Quaternion(
                a.x / s,
                a.y / s,
                a.z / s,
                a.w / s
            );
        }

        public static Vector2 SafeDivide(this Vector2 y, float x, float threshold = 0f)
        {
            if (Mathf.Abs(x) > threshold)
                return y / x;
            else
                return default;
        }

        public static Vector3 SafeDivide(this Vector3 y, float x, float threshold = 0f)
        {
            if (Mathf.Abs(x) > threshold)
                return y / x;
            else
                return default;
        }

        public static Vector4 SafeDivide(this Vector4 y, float x, float threshold = 0f)
        {
            if (Mathf.Abs(x) > threshold)
                return y / x;
            else
                return default;
        }

        public static Quaternion SafeDivide(this Quaternion y, float x, float threshold = 0f)
        {
            if (Mathf.Abs(x) > threshold)
                return Div(y, x);
            else
                return default;
        }

        public static float Magnitude(this Quaternion q)
        {
            return Mathf.Sqrt(Quaternion.Dot(q, q));
        }

        public static bool CompareApproximately(float f0, float f1, float epsilon = 0.000001F)
        {
            var dist = (f0 - f1);
            dist = Mathf.Abs(dist);
            return dist <= epsilon;
        }

        public static float Hermite(float t, float p0, float m0, float m1, float p1)
        {
            var t2 = t * t;
            var t3 = t2 * t;
            var a = 2.0F * t3 - 3.0F * t2 + 1.0F;
            var b = t3 - 2.0F * t2 + t;
            var c = t3 - t2;
            var d = -2.0F * t3 + 3.0F * t2;

            return a * p0 + b * m0 + c * m1 + d * p1;
        }

        public static Vector2 Hermite(float t, in Vector2 p0, in Vector2 m0, in Vector2 m1, in Vector2 p1)
        {
            var t2 = t * t;
            var t3 = t2 * t;
            var a = 2.0F * t3 - 3.0F * t2 + 1.0F;
            var b = t3 - 2.0F * t2 + t;
            var c = t3 - t2;
            var d = -2.0F * t3 + 3.0F * t2;

            return new Vector2(
                a * p0.x + b * m0.x + c * m1.x + d * p1.x,
                a * p0.y + b * m0.y + c * m1.y + d * p1.y
            );
        }

        public static Vector3 Hermite(float t, in Vector3 p0, in Vector3 m0, in Vector3 m1, in Vector3 p1)
        {
            var t2 = t * t;
            var t3 = t2 * t;
            var a = 2.0F * t3 - 3.0F * t2 + 1.0F;
            var b = t3 - 2.0F * t2 + t;
            var c = t3 - t2;
            var d = -2.0F * t3 + 3.0F * t2;

            return new Vector3(
                a * p0.x + b * m0.x + c * m1.x + d * p1.x,
                a * p0.y + b * m0.y + c * m1.y + d * p1.y,
                a * p0.z + b * m0.z + c * m1.z + d * p1.z
            );
        }

        public static Vector4 Hermite(float t, in Vector4 p0, in Vector4 m0, in Vector4 m1, in Vector4 p1)
        {
            var t2 = t * t;
            var t3 = t2 * t;
            var a = 2.0F * t3 - 3.0F * t2 + 1.0F;
            var b = t3 - 2.0F * t2 + t;
            var c = t3 - t2;
            var d = -2.0F * t3 + 3.0F * t2;

            return new Vector4(
                a * p0.x + b * m0.x + c * m1.x + d * p1.x,
                a * p0.y + b * m0.y + c * m1.y + d * p1.y,
                a * p0.z + b * m0.z + c * m1.z + d * p1.z,
                a * p0.w + b * m0.w + c * m1.w + d * p1.w
            );
        }

        public static Quaternion Hermite(float t, in Quaternion p0, in Quaternion m0, in Quaternion m1, in Quaternion p1)
        {
            var t2 = t * t;
            var t3 = t2 * t;
            var a = 2.0F * t3 - 3.0F * t2 + 1.0F;
            var b = t3 - 2.0F * t2 + t;
            var c = t3 - t2;
            var d = -2.0F * t3 + 3.0F * t2;

            return new Quaternion(
                a * p0.x + b * m0.x + c * m1.x + d * p1.x,
                a * p0.y + b * m0.y + c * m1.y + d * p1.y,
                a * p0.z + b * m0.z + c * m1.z + d * p1.z,
                a * p0.w + b * m0.w + c * m1.w + d * p1.w
            );
        }
    }
}
