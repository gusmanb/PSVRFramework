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
        int minSpikes = 4;
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
        
        public void Feed(PSVRSensorReport Report)
        {
            if (intervalMeasure.ElapsedMilliseconds < intervalLapse)
                return;

            intervalMeasure.Restart();

            Vector3 forceVector = new Vector3((float)Report.LinearAcceleration1.X, (float)Report.LinearAcceleration1.Y, (float)Report.LinearAcceleration1.Z);

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
            Debug.WriteLine("Spikes: {0}, Time: {1}", spikes, time);
            if (spikes >= minSpikes && spikes <= maxSpikes && time <= maxTapTime && Tapped != null)
                Tapped(this, EventArgs.Empty);
        }
    }
        
}
