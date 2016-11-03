using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSVRFramework;

namespace PSVRToolbox
{
    public class BMI055Fusion : IDisposable
    {
        float fRad2Deg = 57.295779513f; //将弧度转为角度的乘数
        const int MPU = 0x68; //MPU-6050的I2C地址
        const int nValCnt = 6; //一次读取寄存器的数量

        const int nCalibTimes = 100; //校准时读数的次数
        short[] calibData = new short[nValCnt]; //校准数据
        float[] tempCal = new float[nValCnt];
        
        float fLastRoll = 0.0f; //上一次滤波得到的Roll角
        float fLastPitch = 0.0f; //上一次滤波得到的Pitch角
        KalmanFilter kalmanRoll = new KalmanFilter(); //Roll角滤波器
        KalmanFilter kalmanPitch = new KalmanFilter(); //Pitch角滤波器

        int calLeft = nCalibTimes;

        bool first = true;
        Stopwatch lapseCounter;
        
        short[] readouts = new short[nValCnt];
        float[] realVals = new float[nValCnt];

        public float RollRate { get; private set; }
        public float PitchRate { get; private set; }

        public float Roll { get { return fLastRoll; } }
        public float Pitch { get { return fLastPitch; } }


        float maxX = 0;

        public void Feed(PSVRSensor SensorData)
        {
            if (calLeft > 0)
            {
                calLeft--;
                FeedCalibration(SensorData);
                return;
            }

            float dt = 0;

            if (first)
            {
                kalmanRoll.Angle = 1;
                kalmanPitch.Angle = 0.08f;
                lapseCounter = new Stopwatch();
                lapseCounter.Start();
                first = false;
            }
            else
                dt = ((float)lapseCounter.ElapsedTicks / (float)Stopwatch.Frequency);

            lapseCounter.Restart();

            readouts[0] = (short)SensorData.MotionX1;
            readouts[1] = (short)SensorData.MotionY1;
            readouts[2] = (short)SensorData.MotionZ1;

            readouts[3] = (short)SensorData.GyroYaw1;
            readouts[4] = (short)SensorData.GyroPitch1;
            readouts[5] = (short)SensorData.GyroRoll1;

            Rectify(readouts, realVals);

            float fNorm = (float)Math.Sqrt(realVals[0] * realVals[0] + realVals[1] * realVals[1] + realVals[2] * realVals[2]);
            float fRoll = GetRoll(realVals, fNorm); //计算Roll角
            if (realVals[1] > 0)
            {
                fRoll = -fRoll;
            }
            float fPitch = GetPitch(realVals, fNorm); //计算Pitch角
            if (realVals[0] < 0)
            {
                fPitch = -fPitch;
            }

            float fNewRoll = kalmanRoll.GetAngle(fRoll, realVals[3], dt);
            float fNewPitch = kalmanPitch.GetAngle(fPitch, realVals[4], dt);

            RollRate = (fNewRoll - fLastRoll) / dt;
            PitchRate = (fNewPitch - fLastPitch) / dt;

            //更新Roll角和Pitch角
            fLastRoll = fNewRoll;
            fLastPitch = fNewPitch;
        }

        void FeedCalibration(PSVRSensor SensorData)
        {
            tempCal[0] += SensorData.MotionX1;
            tempCal[1] += SensorData.MotionY1;
            tempCal[2] += SensorData.MotionZ1;

            tempCal[3] += SensorData.GyroYaw1;
            tempCal[4] += SensorData.GyroPitch1;
            tempCal[5] += SensorData.GyroRoll1;

            if(calLeft == 0)
            {
                for (int buc = 0; buc < nValCnt; buc++)
                    calibData[buc] = (short)(tempCal[buc] / (float)nCalibTimes);

                calibData[0] -= 1024;
            }

            
        }

        float GetRoll(float[] pRealVals, float fNorm)
        {
            float fNormXZ = (float)Math.Sqrt(pRealVals[0] * pRealVals[0] + pRealVals[2] * pRealVals[2]);
            float fCos = fNormXZ / fNorm;
            return (float)Math.Sqrt(fCos);
        }

        //算得Pitch角。算法见文档。
        float GetPitch(float[] pRealVals, float fNorm)
        {
            float fNormYZ = (float)Math.Sqrt(pRealVals[1] * pRealVals[1] + pRealVals[2] * pRealVals[2]);
            float fCos = fNormYZ / fNorm;
            return (float)Math.Sqrt(fCos);
        }

        //对读数进行纠正，消除偏移，并转换为物理量。公式见文档。
        void Rectify(short[] pReadout, float[] pRealVals)
        {
            for (int i = 0; i < 3; ++i)
            {
                //pRealVals[i] = (float)(pReadout[i] - calibData[i]) / 16384.0f;
                pRealVals[i] = ((float)(pReadout[i] - calibData[i])) / 1024.0f;
            }
            // pRealVals[3] = pReadout[3] / 340.0f + 36.53;
            for (int i = 3; i < 6; ++i)
            {
                pRealVals[i] = (float)(pReadout[i] - calibData[i]) / 131.2f;

            }
        }

        public void Dispose()
        {
            if (lapseCounter != null)
            {
                lapseCounter.Stop();
                lapseCounter = null;
            }
        }
    }
}
