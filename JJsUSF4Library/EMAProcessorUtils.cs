using JJsUSF4Library.FileClasses.SubfileClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace JJsUSF4Library
{
    public static class EMAProcessorUtils
    {
        public static void EulerToQuaternionXYZ(float pitch, float yaw, float roll, out Quaternion q)
        {
            double dpitch = (double)pitch;
            double dyaw = (double)yaw;
            double droll = (double)roll;

            double dSinPitch = Math.Sin(dpitch * 0.5);
            double dCosPitch = Math.Cos(dpitch * 0.5);
            double dSinYaw = Math.Sin(dyaw * 0.5);
            double dCosYaw = Math.Cos(dyaw * 0.5);
            double dSinRoll = Math.Sin(droll * 0.5);
            double dCosRoll = Math.Cos(droll * 0.5);
            double dCosPitchCosYaw = dCosPitch * dCosYaw;
            double dSinPitchSinYaw = dSinPitch * dSinYaw;

            q.X = (float)(dSinRoll * dCosPitchCosYaw - dCosRoll * dSinPitchSinYaw);
            q.Y = (float)(dCosRoll * dSinPitch * dCosYaw + dSinRoll * dCosPitch * dSinYaw);
            q.Z = (float)(dCosRoll * dCosPitch * dSinYaw - dSinRoll * dSinPitch * dCosYaw);
            q.W = (float)(dCosRoll * dCosPitchCosYaw + dSinRoll * dSinPitchSinYaw);
        }
        public static void DecomposeMatrix_AE(Matrix4x4 matrix, out float tx, out float ty, out float tz, out float rx, out float ry, out float rz, out float sx, out float sy, out float sz)
        {
            Vector3 scale = new Vector3();
            Quaternion quatrotation = new Quaternion();
            Vector3 translation = new Vector3();

            Matrix4x4.Decompose(matrix, out scale, out quatrotation, out translation);

            LeftHandToEulerAnglesXYZ(Matrix4x4.Transpose(matrix), out rx, out ry, out rz);

            tx = translation.X;
            ty = translation.Y;
            tz = translation.Z;

            rx *= (float)(180 / Math.PI);
            ry *= (float)(180 / Math.PI);
            rz *= (float)(180 / Math.PI);

            //rx = (float)drx;
            //ry = (float)dry;
            //rz = (float)drz;

            sx = scale.X;
            sy = scale.Y;
            sz = scale.Z;
        }

        public static void LeftHandToEulerAnglesXYZ(Matrix4x4 m, out float rfXAngle, out float rfYAngle, out float rfZAngle)
        {
            // +-           -+   +-                                      -+
            // | r00 r01 r02 |   |  cy*cz  cz*sx*sy-cx*sz  cx*cz*sy+sx*sz |
            // | r10 r11 r12 | = |  cy*sz  cx*cz+sx*sy*sz -cz*sx+cx*sy*sz |
            // | r20 r21 r22 |   | -sy     cy*sx           cx*cy          |
            // +-           -+   +-                                      -+

            if (m.M31 < 1.0f)
            {
                if (m.M31 > -1.0f)
                {
                    // y_angle = asin(-r20)
                    // z_angle = atan2(r10,r00)
                    // x_angle = atan2(r21,r22)
                    rfYAngle = (float)Math.Asin(-m.M31);
                    rfZAngle = (float)Math.Atan2(m.M21, m.M11);
                    rfXAngle = (float)Math.Atan2(m.M32, m.M33);
                    return;//EA_UNIQUE
                }
                else
                {
                    // y_angle = +pi/2
                    // x_angle - z_angle = atan2(r01,r02)
                    // WARNING.  The solution is not unique.  Choosing x_angle = 0.
                    rfYAngle = (float)Math.PI / 2;
                    rfZAngle = -(float)Math.Atan2(m.M12, m.M13);
                    rfXAngle = 0.0f;
                    return;//EA_NOT_UNIQUE_DIF
                }
            }

            else
            {
                // y_angle = -pi/2
                // x_angle + z_angle = atan2(-r01,-r02)
                // WARNING.  The solution is not unique.  Choosing x_angle = 0;
                rfYAngle = -(float)Math.PI / 2;
                rfZAngle = (float)Math.Atan2(-m.M12, -m.M13);
                rfXAngle = 0.0f;
                return;//EA_NOT_UNIQUE_SUM
            }
        }
        public static void LeftHandToEulerAnglesXYZ(Matrix4x4 m, out Vector3 rotation)
        {
            // +-           -+   +-                                      -+
            // | r00 r01 r02 |   |  cy*cz  cz*sx*sy-cx*sz  cx*cz*sy+sx*sz |
            // | r10 r11 r12 | = |  cy*sz  cx*cz+sx*sy*sz -cz*sx+cx*sy*sz |
            // | r20 r21 r22 |   | -sy     cy*sx           cx*cy          |
            // +-           -+   +-                                      -+

            if (m.M31 < 1.0f)
            {
                if (m.M31 > -1.0f)
                {
                    // y_angle = asin(-r20)
                    // z_angle = atan2(r10,r00)
                    // x_angle = atan2(r21,r22)
                    rotation.Y = (float)Math.Asin(-m.M31);
                    rotation.Z = (float)Math.Atan2(m.M21, m.M11);
                    rotation.X = (float)Math.Atan2(m.M32, m.M33);
                    return;//EA_UNIQUE
                }
                else
                {
                    // y_angle = +pi/2
                    // x_angle - z_angle = atan2(r01,r02)
                    // WARNING.  The solution is not unique.  Choosing x_angle = 0.
                    rotation.Y = (float)Math.PI / 2;
                    rotation.Z = -(float)Math.Atan2(m.M12, m.M13);
                    rotation.X = 0.0f;
                    return;//EA_NOT_UNIQUE_DIF
                }
            }

            else
            {
                // y_angle = -pi/2
                // x_angle + z_angle = atan2(-r01,-r02)
                // WARNING.  The solution is not unique.  Choosing x_angle = 0;
                rotation.Y = -(float)Math.PI / 2;
                rotation.Z = (float)Math.Atan2(-m.M12, -m.M13);
                rotation.X = 0.0f;
                return;//EA_NOT_UNIQUE_SUM
            }
        }
        public static void ComposeMatrixQuat(Quaternion rotation, Vector3 scale, Vector3 translation, out Matrix4x4 matrix)
        {

            Matrix4x4 translationMatrix = Matrix4x4.CreateTranslation(translation);
            Matrix4x4 rotationMatrix = Matrix4x4.CreateFromQuaternion(rotation);
            Matrix4x4 scalingMatrix = Matrix4x4.CreateScale(scale);

            //store result
            matrix = scalingMatrix * rotationMatrix * translationMatrix;
        }

        public static float InterpolateRelativeKeyFrames(CMDTrack c, float frame)
        {
            //Declare default
            float value = 0;

            for (int i = 0; i < c.Steps.Count; i++)
            {
                if (c.Steps[i].Frame > frame)
                {
                    float p1 = c.Steps[i - 1].Value;
                    float p2 = c.Steps[i].Value;
                    float t1 = c.Steps[i - 1].Tangent;
                    float t2 = c.Steps[i].Tangent;
                    float f_initial = c.Steps[i - 1].Frame;
                    float f_end = c.Steps[i].Frame;

                    float s = (frame - (float)f_initial) / (float)(f_end - f_initial);

                    value = HermiteInterpolation(p1, t1, p2, t2, s);

                    break;
                }
                else if (c.Steps[i].Frame == frame)
                {
                    value = c.Steps[i].Value;

                    break;
                }
            }

            return value;
        }

        private static float HermiteInterpolation(float P1, float T1, float P2, float T2, float s)
        {
            float s2 = s * s;
            float s3 = s * s * s;
            float h1 = 2 * s3 - 3 * s2 + 1;          // calculate basis function 1
            float h2 = -2 * s3 + 3 * s2;              // calculate basis function 2
            float h3 = s3 - 2 * s2 + s;          // calculate basis function 3
            float h4 = s3 - s2;              // calculate basis function 4

            return h1 * P1 +                    // multiply and sum all funtions
                    h2 * P2 +                    // together to build the interpolated
                    h3 * T1 +                    // point along the curve.
                    h4 * T2;
        }

    }
}
