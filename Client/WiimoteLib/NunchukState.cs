using System;
namespace WiimoteLib
{
    [DataContract]
    [Serializable]
    public struct NunchukState
    {
        [DataMember]
        public NunchukCalibrationInfo CalibrationInfo;
        [DataMember]
        public AccelState AccelState;
        [DataMember]
        public Point RawJoystick;
        [DataMember]
        public PointF Joystick;
        [DataMember]
        public bool C;
        [DataMember]
        public bool Z;
    }
}
