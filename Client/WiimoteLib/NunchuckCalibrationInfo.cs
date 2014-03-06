using System;
namespace WiimoteLib
{
    [DataContract]
    [Serializable]
    public struct NunchukCalibrationInfo
    {
        public AccelCalibrationInfo AccelCalibration;
        [DataMember]
        public byte MinX;
        [DataMember]
        public byte MidX;
        [DataMember]
        public byte MaxX;
        [DataMember]
        public byte MinY;
        [DataMember]
        public byte MidY;
        [DataMember]
        public byte MaxY;
    }
}
