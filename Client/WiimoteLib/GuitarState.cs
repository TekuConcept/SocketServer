using System;
namespace WiimoteLib
{
    [DataContract]
    [Serializable]
    public struct GuitarState
    {
        [DataMember]
        public GuitarType GuitarType;
        [DataMember]
        public GuitarButtonState ButtonState;
        [DataMember]
        public GuitarFretButtonState FretButtonState;
        [DataMember]
        public GuitarFretButtonState TouchbarState;
        [DataMember]
        public Point RawJoystick;
        [DataMember]
        public PointF Joystick;
        [DataMember]
        public byte RawWhammyBar;
        [DataMember]
        public float WhammyBar;
    }
}
