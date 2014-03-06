using System;
namespace WiimoteLib
{
    [DataContract]
    [Serializable]
    public struct BalanceBoardSensorsF
    {
        [DataMember]
        public float TopRight;
        [DataMember]
        public float TopLeft;
        [DataMember]
        public float BottomRight;
        [DataMember]
        public float BottomLeft;
    }
}
