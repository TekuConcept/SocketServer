using System;
namespace WiimoteLib
{
    [DataContract]
    [Serializable]
    public struct ClassicControllerButtonState
    {
        [DataMember]
        public bool A;
        [DataMember]
        public bool B;
        [DataMember]
        public bool Plus;
        [DataMember]
        public bool Home;
        [DataMember]
        public bool Minus;
        [DataMember]
        public bool Up;
        [DataMember]
        public bool Down;
        [DataMember]
        public bool Left;
        [DataMember]
        public bool Right;
        [DataMember]
        public bool X;
        [DataMember]
        public bool Y;
        [DataMember]
        public bool ZL;
        [DataMember]
        public bool ZR;
        [DataMember]
        public bool TriggerL;
        [DataMember]
        public bool TriggerR;
    }
}
