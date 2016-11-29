using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PSVRFramework
{
    public class PSVRMouseEmulator
    {
        Vector2 size;
        Vector2 resolution;


        float xScale;
        float yScale;

        float smoothFactor;

        Vector3 rayNormal = new Vector3(0, 0, 1);
        Vector3 planeNormal = new Vector3(0, 0, -1);

        Vector3 pointOnPlane;
        Vector2 screenZero;

        public event EventHandler<MouseEventArgs> MouseMove;

        int prevX = 0;
        int prevY = 0;

        public PSVRMouseEmulator(float ScreenDistance, Vector2 ScreenSize, Vector2 ScreenResolution, float SmoothingFactor)
        {
            UpdateParameters(ScreenDistance, ScreenSize, ScreenResolution, SmoothingFactor);
        }

        public void UpdateParameters(float ScreenDistance, Vector2 ScreenSize, Vector2 ScreenResolution, float SmoothingFactor)
        {
            smoothFactor = SmoothingFactor;
            size = ScreenSize;
            resolution = ScreenResolution;

            screenZero = resolution / 2;

            xScale = ScreenResolution.X / ScreenSize.X;
            yScale = ScreenResolution.Y / ScreenSize.Y;
            
            pointOnPlane = new Vector3(0, 0, ScreenDistance);
            
        }

        public void UpdateInput(Quaternion Orientation)
        {
            Vector3 rotatedNormal = Vector3.Normalize(Vector3.Transform(rayNormal, Orientation));
            float T = Vector3.Dot(planeNormal, pointOnPlane) / Vector3.Dot(planeNormal, rotatedNormal);
            Vector3 pointInPlane = rotatedNormal * -T;

            int x = (int)(pointInPlane.X * xScale + screenZero.X);
            int y = (int)(pointInPlane.Y * yScale + screenZero.Y);

            Vector2 np = new Vector2(x, y);
            Vector2 p = new Vector2(prevX, prevY);

            Vector2 ip = Vector2.Lerp(p, np, smoothFactor);

            x = (int)ip.X;
            y = (int)ip.Y;

            if (x != prevX || y != prevY)
            {
                prevX = x;
                prevY = y;

                if (MouseMove != null)
                    MouseMove(this, new MouseEventArgs { X = x, Y = y });
            }

        }

        public class MouseEventArgs : EventArgs
        {
            public int X { get; set; }
            public int Y { get; set; }
        }
    }
}
