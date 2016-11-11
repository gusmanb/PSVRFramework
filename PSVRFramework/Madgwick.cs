//using PSVRFramework;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace PSVRFramework
//{
//    /// <summary>
//    /// MadgwickAHRS class. Implementation of Madgwick's IMU and AHRS algorithms.
//    /// </summary>
//    /// <remarks>
//    /// See: http://www.x-io.co.uk/node/8#open_source_ahrs_and_imu_algorithms
//    /// </remarks>
//    public class MadgwickAHRS
//    {
//        /// <summary>
//        /// Gets or sets the sample period.
//        /// </summary>
//        public double SamplePeriod { get; set; }

//        /// <summary>
//        /// Gets or sets the algorithm gain beta.
//        /// </summary>
//        public double Beta { get; set; }

//        /// <summary>
//        /// Gets or sets the Quaternion output.
//        /// </summary>
//        public Quaternion Quaternion { get; set; }

//        /// <summary>
//        /// Initializes a new instance of the <see cref="MadgwickAHRS"/> class.
//        /// </summary>
//        /// <param name="samplePeriod">
//        /// Sample period.
//        /// </param>
//        public MadgwickAHRS(double samplePeriod)
//            : this(samplePeriod, 1f)
//        {
//        }

//        /// <summary>
//        /// Initializes a new instance of the <see cref="MadgwickAHRS"/> class.
//        /// </summary>
//        /// <param name="samplePeriod">
//        /// Sample period.
//        /// </param>
//        /// <param name="beta">
//        /// Algorithm gain beta.
//        /// </param>
//        public MadgwickAHRS(double samplePeriod, double beta)
//        {
//            SamplePeriod = samplePeriod;
//            Beta = beta;
//            Quaternion = new Quaternion (1f, 0f, 0f, 0f );
//        }
        
//        /// <summary>
//        /// Algorithm IMU update method. Requires only gyroscope and accelerometer data.
//        /// </summary>
//        /// <param name="gx">
//        /// Gyroscope x axis measurement in radians/s.
//        /// </param>
//        /// <param name="gy">
//        /// Gyroscope y axis measurement in radians/s.
//        /// </param>
//        /// <param name="gz">
//        /// Gyroscope z axis measurement in radians/s.
//        /// </param>
//        /// <param name="ax">
//        /// Accelerometer x axis measurement in any calibrated units.
//        /// </param>
//        /// <param name="ay">
//        /// Accelerometer y axis measurement in any calibrated units.
//        /// </param>
//        /// <param name="az">
//        /// Accelerometer z axis measurement in any calibrated units.
//        /// </param>
//        /// <remarks>
//        /// Optimised for minimal arithmetic.
//        /// Total ±: 45
//        /// Total *: 85
//        /// Total /: 3
//        /// Total sqrt: 3
//        /// </remarks>
//        public void Update(double gx, double gy, double gz, double ax, double ay, double az)
//        {
//            double q1 = Quaternion.W, q2 = Quaternion.X, q3 = Quaternion.Y, q4 = Quaternion.Z;   // short name local variable for readability
//            double norm;
//            double s1, s2, s3, s4;
//            double qDot1, qDot2, qDot3, qDot4;

//            // Auxiliary variables to avoid repeated arithmetic
//            double _2q1 = 2f * q1;
//            double _2q2 = 2f * q2;
//            double _2q3 = 2f * q3;
//            double _2q4 = 2f * q4;
//            double _4q1 = 4f * q1;
//            double _4q2 = 4f * q2;
//            double _4q3 = 4f * q3;
//            double _8q2 = 8f * q2;
//            double _8q3 = 8f * q3;
//            double q1q1 = q1 * q1;
//            double q2q2 = q2 * q2;
//            double q3q3 = q3 * q3;
//            double q4q4 = q4 * q4;

//            // Normalise accelerometer measurement
//            norm = (double)Math.Sqrt(ax * ax + ay * ay + az * az);
//            if (norm == 0f) return; // handle NaN
//            norm = 1 / norm;        // use reciprocal for division
//            ax *= norm;
//            ay *= norm;
//            az *= norm;

//            // Gradient decent algorithm corrective step
//            s1 = _4q1 * q3q3 + _2q3 * ax + _4q1 * q2q2 - _2q2 * ay;
//            s2 = _4q2 * q4q4 - _2q4 * ax + 4f * q1q1 * q2 - _2q1 * ay - _4q2 + _8q2 * q2q2 + _8q2 * q3q3 + _4q2 * az;
//            s3 = 4f * q1q1 * q3 + _2q1 * ax + _4q3 * q4q4 - _2q4 * ay - _4q3 + _8q3 * q2q2 + _8q3 * q3q3 + _4q3 * az;
//            s4 = 4f * q2q2 * q4 - _2q2 * ax + 4f * q3q3 * q4 - _2q3 * ay;
//            norm = 1f / (double)Math.Sqrt(s1 * s1 + s2 * s2 + s3 * s3 + s4 * s4);    // normalise step magnitude
//            s1 *= norm;
//            s2 *= norm;
//            s3 *= norm;
//            s4 *= norm;

//            // Compute rate of change of quaternion
//            qDot1 = 0.5f * (-q2 * gx - q3 * gy - q4 * gz) - Beta * s1;
//            qDot2 = 0.5f * (q1 * gx + q3 * gz - q4 * gy) - Beta * s2;
//            qDot3 = 0.5f * (q1 * gy - q2 * gz + q4 * gx) - Beta * s3;
//            qDot4 = 0.5f * (q1 * gz + q2 * gy - q3 * gx) - Beta * s4;

//            // Integrate to yield quaternion
//            q1 += qDot1 * SamplePeriod;
//            q2 += qDot2 * SamplePeriod;
//            q3 += qDot3 * SamplePeriod;
//            q4 += qDot4 * SamplePeriod;
//            norm = 1f / (double)Math.Sqrt(q1 * q1 + q2 * q2 + q3 * q3 + q4 * q4);    // normalise quaternion

//            Quaternion = new Quaternion(q1 * norm, q2 * norm, q3 * norm, q4 * norm);

//            //Quaternion.W = q1 * norm;
//            //Quaternion.X = q2 * norm;
//            //Quaternion.Y = q3 * norm;
//            //Quaternion.Z = q4 * norm;
//        }
//    }
//}