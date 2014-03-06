using System;
namespace WiimoteLib
{
    [DataContract]
    [Serializable]
    public struct IRState
    {
        [DataMember]
        public IRMode Mode;
        [DataMember]
        public IRSensor[] IRSensors;
        [DataMember]
        public Point RawMidpoint;
        [DataMember]
        public PointF Midpoint;
    }
}
