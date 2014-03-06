using System;
namespace WiimoteLib
{
    [DataContract]
    [Serializable]
    public struct GuitarButtonState
    {
        [DataMember]
        public bool StrumUp;
        [DataMember]
        public bool StrumDown;
        [DataMember]
        public bool Minus;
        [DataMember]
        public bool Plus;
    }
}
