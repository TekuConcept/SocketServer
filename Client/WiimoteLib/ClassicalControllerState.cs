using System;
namespace WiimoteLib
{
    [DataContract]
    [Serializable]
    public struct ClassicControllerState
    {
        [DataMember]
        public ClassicControllerCalibrationInfo CalibrationInfo;
        [DataMember]
        public ClassicControllerButtonState ButtonState;
        [DataMember]
        public Point RawJoystickL;
        [DataMember]
        public Point RawJoystickR;
        [DataMember]
        public PointF JoystickL;
        [DataMember]
        public PointF JoystickR;
        [DataMember]
        public byte RawTriggerL;
        [DataMember]
        public byte RawTriggerR;
        [DataMember]
        public float TriggerL;
        [DataMember]
        public float TriggerR;
    }
}
