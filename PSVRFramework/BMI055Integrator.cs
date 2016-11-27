using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PSVRFramework
{
    public static class BMI055Integrator
    {

        static float aRes;
        static float gRes;
        static Vector3 gravityVector;
        static Vector3 accelOffset;
        static Vector3 gyroOffset;

        static int samplesLeft = 2000;

        static uint prevTimestamp;

        public static bool LightLed = true;

        static MadgwickAHRS fusion;
        static Quaternion ZeroPose;

        static bool recalibrate = false;
        static bool recenter = false;

        public static bool calibrating = true;

        public static void Init(AScale AccelerometerScale, Gscale GyroscopeScale)
        {
            aRes = GetAres(AccelerometerScale);
            gRes = GetGres(GyroscopeScale);
            
        }

        public static void Recalibrate()
        {
            calibrating = true;
            recalibrate = true;
        }

        public static void Recenter()
        {
            recenter = true;
        }

        public static void Parse(PSVRSensorReport Report)
        {

            if (recalibrate)
            {
                samplesLeft = 2000;
                accelOffset = Vector3.Zero;
                gyroOffset = Vector3.Zero;
                recalibrate = false;
            }

            //Ok para euler??
            //Report.LinearAcceleration1 = new Vector3(Report.RawMotionX1 * aRes, Report.RawMotionY1 * aRes, Report.RawMotionZ1 * aRes);
            //Report.AngularAcceleration1 = new Vector3(Report.RawGyroYaw1 * gRes, Report.RawGyroPitch1 * gRes, Report.RawGyroRoll1 * gRes);

            //Integrate(Report.LinearAcceleration1, Report.AngularAcceleration1, Report.Timestamp1);

            //Report.LinearAcceleration2 = new Vector3(Report.RawMotionX2 * aRes, Report.RawMotionY2 * aRes, Report.RawMotionZ2 * aRes);
            //Report.AngularAcceleration2 = new Vector3(Report.RawGyroYaw2 * gRes, Report.RawGyroPitch2 * gRes, Report.RawGyroRoll2 * gRes);

            //Report.Pose = Integrate(Report.LinearAcceleration2, Report.AngularAcceleration2, Report.Timestamp2);
            //Report.Orientation = ToEuler(ref Report.Pose);


            //Ok para quaternion??
            Report.LinearAcceleration1 = new Vector3(-Report.RawMotionY1 * aRes, Report.RawMotionX1 * aRes, Report.RawMotionZ1 * aRes);
            Report.AngularAcceleration1 = new Vector3(-Report.RawGyroPitch1 * gRes, Report.RawGyroYaw1 * gRes, Report.RawGyroRoll1 * gRes);
            Integrate(Report.LinearAcceleration1, Report.AngularAcceleration1, Report.Timestamp1);

            Report.LinearAcceleration2 = new Vector3(-Report.RawMotionY2 * aRes, Report.RawMotionX2 * aRes, Report.RawMotionZ2 * aRes);
            Report.AngularAcceleration2 = new Vector3(-Report.RawGyroPitch2 * gRes, Report.RawGyroYaw2 * gRes, Report.RawGyroRoll2 * gRes);
            Report.Pose = Integrate(Report.LinearAcceleration2, Report.AngularAcceleration2, Report.Timestamp2);

            

        }

        static Quaternion Integrate(Vector3 linearAcceleration, Vector3 angularAcceleration, uint Timestamp)
        {
            

            if (samplesLeft > 0)
            {
                samplesLeft--;
                accelOffset += linearAcceleration;
                gyroOffset += angularAcceleration;
                return Quaternion.Identity;
            }
            else if (samplesLeft == 0)
            {
                samplesLeft--;
                accelOffset /= 2000;
                gyroOffset /= 2000;
                gravityVector = Vector3.Normalize(accelOffset);
                accelOffset -= gravityVector;
                prevTimestamp = Timestamp;
                fusion = new PSVRFramework.MadgwickAHRS(Quaternion.Identity);
                return Quaternion.Identity;
            }
            else if (samplesLeft > -1500)
            {
                samplesLeft--;

                linearAcceleration = Vector3.Normalize(linearAcceleration - accelOffset);
                angularAcceleration -= gyroOffset;

                float interval = 0;

                if (prevTimestamp > Timestamp)
                    interval = (Timestamp + (0xFFFFFF - prevTimestamp)) / 1000000.0f;
                else
                    interval = (Timestamp - prevTimestamp) / 1000000.0f;

                fusion.Update(angularAcceleration.X, angularAcceleration.Y, angularAcceleration.Z, linearAcceleration.X, linearAcceleration.Y, linearAcceleration.Z, 1.5f, interval);
                prevTimestamp = Timestamp;
                return Quaternion.Identity;

            }
            else if (samplesLeft > -2000)
            {
                samplesLeft--;

                linearAcceleration = Vector3.Normalize(linearAcceleration - accelOffset);
                angularAcceleration -= gyroOffset;

                float interval = 0;

                if (prevTimestamp > Timestamp)
                    interval = (Timestamp + (0xFFFFFF - prevTimestamp)) / 1000000.0f;
                else
                    interval = (Timestamp - prevTimestamp) / 1000000.0f;

                fusion.Update(angularAcceleration.X, angularAcceleration.Y, angularAcceleration.Z, linearAcceleration.X, linearAcceleration.Y, linearAcceleration.Z, 0.05f, interval);
                prevTimestamp = Timestamp;
                return Quaternion.Identity;

            }
            else if (samplesLeft == -2000)
            {
                samplesLeft--;

                linearAcceleration = Vector3.Normalize(linearAcceleration - accelOffset);
                angularAcceleration -= gyroOffset;

                float interval = 0;

                if (prevTimestamp > Timestamp)
                    interval = (Timestamp + (0xFFFFFF - prevTimestamp)) / 1000000.0f;
                else
                    interval = (Timestamp - prevTimestamp) / 1000000.0f;

                fusion.Update(angularAcceleration.X, angularAcceleration.Y, angularAcceleration.Z, linearAcceleration.X, linearAcceleration.Y, linearAcceleration.Z, 0.035f, interval);
                prevTimestamp = Timestamp;

                ZeroPose = Quaternion.Identity * Quaternion.Inverse(fusion.Quaternion);

                calibrating = false;

                return Quaternion.Identity;

            }
            else
            {
                linearAcceleration = Vector3.Normalize(linearAcceleration - accelOffset);
                angularAcceleration -= gyroOffset;

                float interval = 0;

                if (prevTimestamp > Timestamp)
                    interval = (Timestamp + (0xFFFFFF - prevTimestamp)) / 1000000.0f;
                else
                    interval = (Timestamp - prevTimestamp) / 1000000.0f;

                fusion.Update(angularAcceleration.X, angularAcceleration.Y, angularAcceleration.Z, linearAcceleration.X, linearAcceleration.Y, linearAcceleration.Z, 0.035f, interval);
                prevTimestamp = Timestamp;

                if (recenter)
                {
                    ZeroPose = Quaternion.Identity * Quaternion.Inverse(fusion.Quaternion);
                    recenter = false;
                }

                return Quaternion.Inverse(ZeroPose * fusion.Quaternion);

            }
        }

        static Vector3 ToEuler(ref Quaternion Q)
        {
            Vector3 pitchYawRoll = new Vector3();

            double sqw = Q.W * Q.W;
            double sqx = Q.X * Q.X;
            double sqy = Q.Y * Q.Y;
            double sqz = Q.Z * Q.Z;

            pitchYawRoll.X = -(float)Math.Atan2(2f * Q.X * Q.W + 2f * Q.Y * Q.Z, 1 - 2f * (sqz + sqw));     // Yaw 
            pitchYawRoll.Y = -(float)Math.Asin(2f * (Q.X * Q.Z - Q.W * Q.Y));                             // Pitch 
            pitchYawRoll.Z = (float)Math.Atan2(2f * Q.X * Q.Y + 2f * Q.Z * Q.W, 1 - 2f * (sqy + sqz));      // Roll 

            return pitchYawRoll;
        }

        public static float GetGres(Gscale Scale)
        {
            switch (Scale)
            {
                // Possible gyro scales (and their register bit settings) are:
                // 125 DPS (100), 250 DPS (011), 500 DPS (010), 1000 DPS (001), and 2000 DPS (000). 
                case Gscale.GFS_125DPS:
                    return (float)(0.00381f * (Math.PI / 180)); //return 124.87 / (32768.0 * 4); // per data sheet, not exactly 125!?
                case Gscale.GFS_250DPS:
                    return (float)(0.007622f * (Math.PI / 180)); //1.0 / 262.4; //return 249.75 / 32768.0;
                case Gscale.GFS_500DPS:
                    return (float)(0.01524f * (Math.PI / 180));//1.0 / 262.4; //return 499.5 / 32768.0;
                case Gscale.GFS_1000DPS:
                    return (float)(0.03048f * (Math.PI / 180)); //1.0 / 262.4; //return 999.0 / 32768.0;
                case Gscale.GFS_2000DPS:
                    return (float)(0.06097f * (Math.PI / 180));//1.0 / 262.4; //return 1998.0 / 32768.0;
            }

            return 0;
        }

        public static float GetAres(AScale Scale)
        {
            switch (Scale)
            {
                
                case AScale.AFS_2G:
                    return 2.0f / 2048.0f;
                case AScale.AFS_4G:
                    return 4.0f / 2048.0f;
                case AScale.AFS_8G:
                    return 8.0f / 2048.0f;
                case AScale.AFS_16G:
                    return 16.0f / 2048.0f;
            }

            return 0;

        }

        public enum AScale
        {
            AFS_2G = 0x03,
            AFS_4G = 0x05,
            AFS_8G = 0x08,
            AFS_16G = 0x0C
        }

        public enum Gscale
        {
            GFS_2000DPS = 0,
            GFS_1000DPS,
            GFS_500DPS,
            GFS_250DPS,
            GFS_125DPS
        };

    }
}