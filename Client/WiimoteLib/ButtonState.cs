using System;
namespace WiimoteLib
{
    [DataContract]
    [Serializable]
    public struct ButtonState
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
        public bool One;
        [DataMember]
        public bool Two;
        [DataMember]
        public bool Up;
        [DataMember]
        public bool Down;
        [DataMember]
        public bool Left;
        [DataMember]
        public bool Right;
    }
}
