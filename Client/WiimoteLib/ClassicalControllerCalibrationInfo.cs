using System;
namespace WiimoteLib
{
    [DataContract]
    [Serializable]
    public struct ClassicControllerCalibrationInfo
    {
        [DataMember]
        public byte MinXL;
        [DataMember]
        public byte MidXL;
        [DataMember]
        public byte MaxXL;
        [DataMember]
        public byte MinYL;
        [DataMember]
        public byte MidYL;
        [DataMember]
        public byte MaxYL;
        [DataMember]
        public byte MinXR;
        [DataMember]
        public byte MidXR;
        [DataMember]
        public byte MaxXR;
        [DataMember]
        public byte MinYR;
        [DataMember]
        public byte MidYR;
        [DataMember]
        public byte MaxYR;
        [DataMember]
        public byte MinTriggerL;
        [DataMember]
        public byte MaxTriggerL;
        [DataMember]
        public byte MinTriggerR;
        [DataMember]
        public byte MaxTriggerR;
    }
}
