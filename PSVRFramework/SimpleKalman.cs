/*
* PSVRFramework - PlayStation VR PC framework
* Copyright (C) 2016 Agustín Giménez Bernad <geniwab@gmail.com>
*
* This program is free software: you can redistribute it and/or modify
* it under the terms of the GNU Affero General Public License as
* published by the Free Software Foundation, either version 3 of the
* License, or (at your option) any later version.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU Affero General Public License for more details.
*
* You should have received a copy of the GNU Affero General Public License
* along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

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
