using System;
namespace WiimoteLib
{
    [DataContract]
    [Serializable]
    public struct LEDState
    {
        [DataMember]
        public bool LED1;
        [DataMember]
        public bool LED2;
        [DataMember]
        public bool LED3;
        [DataMember]
        public bool LED4;
    }
}
