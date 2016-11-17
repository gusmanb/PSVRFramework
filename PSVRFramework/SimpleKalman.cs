using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSVRFramework
{
    public class SimpleKalman
    {

        double dt;

        double rate;
        double bias;
        double angle;

        public double Angle { get { return angle; } }

        public double QAngle = 0.001;
        public double QBias = 0.003;
        public double RMeasure = 0.03;

        double[][] P = { new double[] { 0, 0 }, new double[]{ 0, 0 } };

        public SimpleKalman(double DT)
        {
            dt = DT;
        }

        public double Update(double NewAngle, double NewRate)
        {
            rate = NewRate - bias;
            angle += dt * rate;

            //P[0][0] += dt * (dt * P[1][1] - P[0][1] - P[1][0] + QAngle);
            //P[0][1] -= dt * P[1][1];
            //P[1][0] -= dt * P[1][1];
            //P[1][1] += QBias * dt;

            //double S = P[0][0] + RMeasure;
            //double[] K = new double[2];

            //K[0] = P[0][0] / S;
            //K[1] = P[1][0] / S;

            //double y = NewAngle - angle;
            //angle += K[0] * y;
            //bias += K[1] * y;
            
            //double P00_temp = P[0][0];
            //double P01_temp = P[0][1];

            //P[0][0] -= K[0] * P00_temp;
            //P[0][1] -= K[0] * P01_temp;
            //P[1][0] -= K[1] * P00_temp;
            //P[1][1] -= K[1] * P01_temp;

            return angle;
        }
    }
}
