using System;
namespace WiimoteLib
{
    [DataContract]
    [Serializable]
    public struct GuitarFretButtonState
    {
        [DataMember]
        public bool Green;
        [DataMember]
        public bool Red;
        [DataMember]
        public bool Yellow;
        [DataMember]
        public bool Blue;
        [DataMember]
        public bool Orange;
    }
}
