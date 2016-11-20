using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PSVRFramework
{
    public class TapDetector
    {
        Stopwatch intervalMeasure;
        Stopwatch tapMeasure;
        Timer debounceEvent;

        int intervalLapse = 5;
        int debounceLapse = 200;
        int maxTapTime = 500;
        int minSpikes = 5;
        int maxSpikes = 7;

        bool onTap = false;
        bool onSpike = false;

        int spikeCount = 0;

        float sensibility = 0;

        float zeroForce = 0;

        int calCycles = 200;

        public event EventHandler Tapped;

        public TapDetector(float Sensibility)
        {
            sensibility = Sensibility;
            intervalMeasure = new Stopwatch();
            tapMeasure = new Stopwatch();
            debounceEvent = new Timer(CheckTap);

            intervalMeasure.Start();
        }
        
        public void Feed(BMI055SensorData SensorReadings)
        {
            if (intervalMeasure.ElapsedMilliseconds < intervalLapse)
                return;

            intervalMeasure.Restart();

            Vector3 forceVector = new Vector3((float)SensorReadings.AccelX, (float)SensorReadings.AccelY, (float)SensorReadings.AccelZ);

            float magnitude = forceVector.Length();

            if (calCycles > 0)
            {
                calCycles--;
                zeroForce += magnitude;
                return;
            }
            else if (calCycles == 0)
            {
                calCycles--;
                zeroForce /= 200;
                return;
            }
            
            magnitude = magnitude - zeroForce;
       
            if (magnitude < sensibility)
            {
                onSpike = false;
                return;
            }
            
            if (onSpike)
                return;

            if (!onTap)
            {
                onTap = true;
                spikeCount = 1;
                tapMeasure.Restart();
            }
            else
                spikeCount++;

            debounceEvent.Change(debounceLapse, Timeout.Infinite);
            onSpike = true;
            
        }

        public void CheckTap(object State)
        {
            double time = tapMeasure.ElapsedMilliseconds;
            tapMeasure.Stop();

            int spikes = spikeCount;

            onTap = false;
            onSpike = false;
            spikeCount = 0;

            if (spikes >= minSpikes && spikes <= maxSpikes && time <= maxTapTime && Tapped != null)
                Tapped(this, EventArgs.Empty);
        }
    }
        
}
