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

        Quaternion current = new Quaternion();
        public Quaternion Value { get { return current; } }
        public PSVRMouseEmulator()
        {

        }

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
                integrator.Update(Report.FilteredSensorReading1.GyroX - calValues.GyroX,
                    Report.FilteredSensorReading1.GyroY - calValues.GyroY,
                    -(Report.FilteredSensorReading1.GyroZ - calValues.GyroZ),
                    Report.FilteredSensorReading1.AccelX - calValues.AccelX,
                    Report.FilteredSensorReading1.AccelY - calValues.AccelY,
                    Report.FilteredSensorReading1.AccelZ - calValues.AccelZ);

                integrator.Update(Report.FilteredSensorReading2.GyroX - calValues.GyroX,
                    Report.FilteredSensorReading2.GyroY - calValues.GyroY,
                    -(Report.FilteredSensorReading1.GyroZ - calValues.GyroZ),
                    Report.FilteredSensorReading2.AccelX - calValues.AccelX,
                    Report.FilteredSensorReading2.AccelY - calValues.AccelY,
                    Report.FilteredSensorReading2.AccelZ - calValues.AccelZ);

                current.Update(integrator.Quaternion[0], integrator.Quaternion[1], integrator.Quaternion[2], integrator.Quaternion[3]);

                //currentValue = Vector3.FromQuaternion(integrator.Quaternion);
                //currentValue.X = currentValue.X * 180 / Math.PI;
                //currentValue.Y = currentValue.Y * 180 / Math.PI;
                //currentValue.Z = currentValue.Z * 180 / Math.PI;
            }
            
        }
        
    }
}
