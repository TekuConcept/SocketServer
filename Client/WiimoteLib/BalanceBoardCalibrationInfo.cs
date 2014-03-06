using System;
namespace WiimoteLib
{
    [DataContract]
    [Serializable]
    public struct BalanceBoardCalibrationInfo
    {
        [DataMember]
        public BalanceBoardSensors Kg0;
        [DataMember]
        public BalanceBoardSensors Kg17;
        [DataMember]
        public BalanceBoardSensors Kg34;
    }
}
