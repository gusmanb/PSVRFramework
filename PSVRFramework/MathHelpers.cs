using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSVRFramework
{
    public class Quaternion
    {
        public double Yaw { get; private set; }
        public double Roll { get; private set; }
        public double Pitch { get; private set; }

        public void Update(double w, double x, double y, double z, bool conjugate)
        {
            // normalize the vector
            double len = Math.Sqrt((w * w) + (x * x) + (y * y) + (z * z));
            w /= len;
            x /= len;
            y /= len;
            z /= len;

            // The Freespace quaternion gives the rotation in terms of
            // rotating the world around the object. We take the conjugate to
            // get the rotation in the object's reference frame.
            if (conjugate)
            {
                w = w;
                x = -x;
                y = -y;
                z = -z;
            }

            // Convert to angles in radians
            double m11 = (2.0f * w * w) + (2.0f * x * x) - 1.0f;
            double m12 = (2.0f * x * y) + (2.0f * w * z);
            double m13 = (2.0f * x * z) - (2.0f * w * y);
            double m23 = (2.0f * y * z) + (2.0f * w * x);
            double m33 = (2.0f * w * w) + (2.0f * z * z) - 1.0f;

            Roll = Math.Atan2(m23, m33);
            Pitch = Math.Asin(-m13);
            Yaw = Math.Atan2(m12, m11);
            if (Double.IsNaN(Roll))
                Roll = 0d;
            if (Double.IsNaN(Pitch))
                Pitch = 0d;
            if (Double.IsNaN(Yaw))
                Yaw = 0d;
        }

        public void Update(double w, double x, double y, double z)
        {
            Update(w, x, y, z, true);
        }
    }
    //public class Quaternion
    //{
    //    public double W;
    //    public double X;
    //    public double Y;
    //    public double Z;

    //    public Quaternion() { }

    //    public Quaternion(double W, double X, double Y, double Z)
    //    {
    //        this.W = W;
    //        this.X = X;
    //        this.Y = Y;
    //        this.Z = Z;
    //    }

    //    public static Quaternion FromEuler(Vector3 Orientation)
    //    {
    //        Quaternion q = new Quaternion();
    //        double t0 = Math.Cos(Orientation.X * 0.5f);
    //        double t1 = Math.Sin(Orientation.X * 0.5f);
    //        double t2 = Math.Cos(Orientation.Z * 0.5f);
    //        double t3 = Math.Sin(Orientation.Z * 0.5f);
    //        double t4 = Math.Cos(Orientation.Y * 0.5f);
    //        double t5 = Math.Sin(Orientation.Y * 0.5f);

    //        q.W = t0 * t2 * t4 + t1 * t3 * t5;
    //        q.X = t0 * t3 * t4 - t1 * t2 * t5;
    //        q.Y = t0 * t2 * t5 + t1 * t3 * t4;
    //        q.Z = t1 * t2 * t4 - t0 * t3 * t5;
    //        return q;
    //    }
    //}

    public class Vector3
    {
        public double X;
        public double Y;
        public double Z;

        public Vector3() { }

        public Vector3(double X, double Y, double Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }

        //public static Vector3 FromQuaternion(Quaternion Orientation)
        //{
        //    Vector3 v = new Vector3();

        //    double ysqr = Orientation.Y * Orientation.Y;
        //    double t0 = -2.0f * (ysqr + Orientation.Z * Orientation.Z) + 1.0f;
        //    double t1 = +2.0f * (Orientation.X * Orientation.Y - Orientation.W * Orientation.Z);
        //    double t2 = -2.0f * (Orientation.X * Orientation.Z + Orientation.W * Orientation.Y);
        //    double t3 = +2.0f * (Orientation.Y * Orientation.Z - Orientation.W * Orientation.X);
        //    double t4 = -2.0f * (Orientation.X * Orientation.X + ysqr) + 1.0f;

        //    t2 = t2 > 1.0f ? 1.0f : t2;
        //    t2 = t2 < -1.0f ? -1.0f : t2;

        //    v.Y = Math.Asin(t2);
        //    v.Z = Math.Atan2(t3, t4);
        //    v.X = Math.Atan2(t1, t0);

        //    return v;
        //}

        //public static Vector3 FromQ(Quaternion Orientation)
        //{
        //    Vector3 newV = new Vector3();
        //    newV.X = Math.Atan2(2.0f * (Orientation.X * Orientation.Y + Orientation.W * Orientation.Z), Orientation.W * Orientation.W + Orientation.X * Orientation.X - Orientation.Y * Orientation.Y - Orientation.Z * Orientation.Z);
        //    newV.Y = -Math.Asin(2.0f * (Orientation.X * Orientation.Z - Orientation.W * Orientation.Y));
        //    newV.Z = Math.Atan2(2.0f * (Orientation.W * Orientation.X + Orientation.Y * Orientation.Z), Orientation.W * Orientation.W - Orientation.X * Orientation.X - Orientation.Y * Orientation.Y + Orientation.Z * Orientation.Z);
        //    newV.Y *= 180.0f / Math.PI;
        //    newV.X *= 180.0f / Math.PI;
        //    newV.Z *= 180.0f / Math.PI;

        //    return newV;
        //}

        public double Length { get { return Math.Sqrt(X * X + Y * Y + Z * Z); } }

        public void Normalize()
        {
            double l = Length;
            X /= l;
            Y /= l;
            Z /= l;
        }
    }
}
