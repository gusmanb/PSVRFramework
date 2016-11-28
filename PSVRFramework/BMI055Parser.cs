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
    public static class BMI055Parser
    {

        static double aRes;
        static double gRes;

        public static void Init(AScale AccelerometerScale, Gscale GyroscopeScale)
        {
            aRes = GetAres(AccelerometerScale);
            gRes = GetGres(GyroscopeScale);
        }

        public static BMI055SensorData Parse(byte[] RawData, int AccelOffset, int GyroOffset)
        {
            BMI055SensorData data = new BMI055SensorData();

            data.AccelX = ((short)(((short)RawData[AccelOffset + 1] << 8) | RawData[AccelOffset]) >> 4) * aRes;
            data.AccelY = ((short)(((short)RawData[AccelOffset + 3] << 8) | RawData[AccelOffset + 2]) >> 4) * aRes;
            data.AccelZ = ((short)(((short)RawData[AccelOffset + 5] << 8) | RawData[AccelOffset + 4]) >> 4) * aRes;

            data.GyroX = ((short)(((short)RawData[GyroOffset + 1] << 8) | RawData[GyroOffset])) * gRes;
            data.GyroY = ((short)(((short)RawData[GyroOffset + 3] << 8) | RawData[GyroOffset + 2])) * gRes;
            data.GyroZ = ((short)(((short)RawData[GyroOffset + 5] << 8) | RawData[GyroOffset + 4])) * gRes;

            return data;

        }

        public static double GetGres(Gscale Scale)
        {
            switch (Scale)
            {
                // Possible gyro scales (and their register bit settings) are:
                // 125 DPS (100), 250 DPS (011), 500 DPS (010), 1000 DPS (001), and 2000 DPS (000). 
                case Gscale.GFS_125DPS:
                    return 124.87 / 32768.0; // per data sheet, not exactly 125!?
                case Gscale.GFS_250DPS:
                    return 249.75 / 32768.0;
                case Gscale.GFS_500DPS:
                    return 499.5 / 32768.0;
                case Gscale.GFS_1000DPS:
                    return 999.0 / 32768.0;
                case Gscale.GFS_2000DPS:
                    return 1998.0 / 32768.0;
            }

            return 0;
        }

        public static double GetAres(AScale Scale)
        {
            switch (Scale)
            {
                // Possible accelerometer scales (and their register bit settings) are:
                // 2 Gs (0011), 4 Gs (0101), 8 Gs (1000), and 16 Gs  (1100). 
                // BMX055 ACC data is signed 12 bit
                case AScale.AFS_2G:
                    return 2.0 / 2048.0;
                case AScale.AFS_4G:
                    return 4.0 / 2048.0;
                case AScale.AFS_8G:
                    return 8.0 / 2048.0;
                    break;
                case AScale.AFS_16G:
                    return 16.0 / 2048.0;
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
    
    public class BMI055SensorData
    {
        public double AccelX;
        public double AccelY;
        public double AccelZ;

        public double GyroX;
        public double GyroY;
        public double GyroZ;
    }
}
