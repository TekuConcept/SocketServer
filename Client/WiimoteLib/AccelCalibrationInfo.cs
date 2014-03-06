using System;
namespace WiimoteLib
{
    [DataContract]
    [Serializable]
    public struct AccelCalibrationInfo
    {
        [DataMember]
        public byte X0;
        [DataMember]
        public byte Y0;
        [DataMember]
        public byte Z0;
        [DataMember]
        public byte XG;
        [DataMember]
        public byte YG;
        [DataMember]
        public byte ZG;
    }
}
