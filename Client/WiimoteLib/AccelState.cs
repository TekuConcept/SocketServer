using System;
namespace WiimoteLib
{
    [DataContract]
    [Serializable]
    public struct AccelState
    {
        [DataMember]
        public Point3 RawValues;
        [DataMember]
        public Point3F Values;
    }
}
