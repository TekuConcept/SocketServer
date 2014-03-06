using System;
namespace WiimoteLib
{
    [DataContract]
    [Serializable]
    public struct BalanceBoardSensors
    {
        [DataMember]
        public short TopRight;
        [DataMember]
        public short TopLeft;
        [DataMember]
        public short BottomRight;
        [DataMember]
        public short BottomLeft;
    }
}
