using System;
namespace WiimoteLib
{
    [DataContract]
    [Serializable]
    public struct DrumsState
    {
        public bool Red;
        public bool Green;
        public bool Blue;
        public bool Orange;
        public bool Yellow;
        public bool Pedal;
        public int RedVelocity;
        public int GreenVelocity;
        public int BlueVelocity;
        public int OrangeVelocity;
        public int YellowVelocity;
        public int PedalVelocity;
        public bool Plus;
        public bool Minus;
        public Point RawJoystick;
        public PointF Joystick;
    }
}
