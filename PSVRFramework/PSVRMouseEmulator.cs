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
using System.Threading;
using System.Threading.Tasks;

namespace PSVRFramework
{
    public class PSVRMouseEmulator
    {
        MahonyAHRS integrator = new MahonyAHRS(1.0 / 2000);
        BMI055SensorData calValues = new BMI055SensorData();

        int left = 1000;
        //Vector3 currentValue = new Vector3();

        //public Vector3 Value { get { return currentValue; } }

        //Quaternion current = new Quaternion();
        //public Quaternion Value { get { return current; } }

        public Vector3 Value { get { return new Vector3(kalmanRoll.Angle, kalmanPitch.Angle, 0); } }

        public PSVRMouseEmulator()
        {

        }

        SimpleKalman kalmanRoll = new SimpleKalman(1 / 2000.0);
        SimpleKalman kalmanPitch = new SimpleKalman(1 / 2000.0);

        const double fRad2Deg = 57.295779513;

        public void Feed(PSVRSensorReport Report)
        {
            if (left > 0)
            {
                left--;

                calValues.AccelX += Report.FilteredSensorReading1.AccelX;
                calValues.AccelY += Report.FilteredSensorReading1.AccelY;
                calValues.AccelZ += Report.FilteredSensorReading1.AccelZ;

                calValues.AccelX += Report.FilteredSensorReading2.AccelX;
                calValues.AccelY += Report.FilteredSensorReading2.AccelY;
                calValues.AccelZ += Report.FilteredSensorReading2.AccelZ;

                calValues.GyroX += Report.FilteredSensorReading1.GyroX;
                calValues.GyroY += Report.FilteredSensorReading1.GyroY;
                calValues.GyroZ += Report.FilteredSensorReading1.GyroZ;

                calValues.GyroX += Report.FilteredSensorReading2.GyroX;
                calValues.GyroY += Report.FilteredSensorReading2.GyroY;
                calValues.GyroZ += Report.FilteredSensorReading2.GyroZ;

                if (left == 0)
                {
                    calValues.AccelX /= 2000.0;
                    calValues.AccelY /= 2000.0;
                    calValues.AccelZ /= 2000.0;

                    calValues.GyroX /= 2000.0;
                    calValues.GyroY /= 2000.0;
                    calValues.GyroZ /= 2000.0;

                    Vector3 gNorm = new Vector3(calValues.AccelX, calValues.AccelY, calValues.AccelZ);
                    gNorm.Normalize();

                    calValues.AccelX -= gNorm.X;
                    calValues.AccelY -= gNorm.Y;
                    calValues.AccelZ -= gNorm.Z;
                    
                }

            }
            else
            {

                double GyroX = Report.FilteredSensorReading1.GyroX - calValues.GyroX;
                double GyroY = Report.FilteredSensorReading1.GyroY - calValues.GyroY;
                double GyroZ = Report.FilteredSensorReading1.GyroZ - calValues.GyroZ;

                double AccelX = Report.FilteredSensorReading1.AccelX - calValues.AccelX;
                double AccelY = Report.FilteredSensorReading1.AccelY - calValues.AccelY;
                double AccelZ = Report.FilteredSensorReading1.AccelZ - calValues.AccelZ;

                //double fNorm = Math.Sqrt(GyroX * GyroX + GyroY * GyroY + GyroZ * GyroZ);
                //double fRoll = GetRoll(GyroX, GyroZ, fNorm);
                //double fPitch = GetPitch(GyroY, GyroZ, fNorm);


                kalmanRoll.Update(GyroX, AccelX);
                kalmanPitch.Update(GyroY, AccelY);

                GyroX = Report.FilteredSensorReading2.GyroX - calValues.GyroX;
                GyroY = Report.FilteredSensorReading2.GyroY - calValues.GyroY;
                GyroZ = Report.FilteredSensorReading2.GyroZ - calValues.GyroZ;

                AccelX = Report.FilteredSensorReading2.AccelX - calValues.AccelX;
                AccelY = Report.FilteredSensorReading2.AccelY - calValues.AccelY;
                AccelZ = Report.FilteredSensorReading2.AccelZ - calValues.AccelZ;

                //fNorm = Math.Sqrt(GyroX * GyroX + GyroY * GyroY + GyroZ * GyroZ);
                //fRoll = GetRoll(GyroX, GyroZ, fNorm);
                //fPitch = GetPitch(GyroY, GyroZ, fNorm);


                kalmanRoll.Update(GyroX, AccelX);
                kalmanPitch.Update(GyroY, AccelY);

                //integrator.Update(Report.FilteredSensorReading1.GyroX - calValues.GyroX,
                //    Report.FilteredSensorReading1.GyroY - calValues.GyroY,
                //    -(Report.FilteredSensorReading1.GyroZ - calValues.GyroZ),
                //    Report.FilteredSensorReading1.AccelX - calValues.AccelX,
                //    Report.FilteredSensorReading1.AccelY - calValues.AccelY,
                //    Report.FilteredSensorReading1.AccelZ - calValues.AccelZ);

                //integrator.Update(Report.FilteredSensorReading2.GyroX - calValues.GyroX,
                //    Report.FilteredSensorReading2.GyroY - calValues.GyroY,
                //    -(Report.FilteredSensorReading1.GyroZ - calValues.GyroZ),
                //    Report.FilteredSensorReading2.AccelX - calValues.AccelX,
                //    Report.FilteredSensorReading2.AccelY - calValues.AccelY,
                //    Report.FilteredSensorReading2.AccelZ - calValues.AccelZ);

                //current.Update(integrator.Quaternion[0], integrator.Quaternion[1], integrator.Quaternion[2], integrator.Quaternion[3]);

                //currentValue = Vector3.FromQuaternion(integrator.Quaternion);
                //currentValue.X = currentValue.X * 180 / Math.PI;
                //currentValue.Y = currentValue.Y * 180 / Math.PI;
                //currentValue.Z = currentValue.Z * 180 / Math.PI;
            }
            
        }


        double GetRoll(double GyroX, double GyroZ, double fNorm)
        {
            double fNormXZ = Math.Sqrt(GyroX * GyroX + GyroZ * GyroZ);
            double fCos = fNormXZ / fNorm;
            return Math.Acos(fCos) * fRad2Deg;
        }

        double GetPitch(double GyroY, double GyroZ, double fNorm)
        {
            double fNormYZ = Math.Sqrt(GyroY * GyroY + GyroZ * GyroZ);
            double fCos = fNormYZ / fNorm;
            return Math.Acos(fCos) * fRad2Deg;
        }
    }
}
