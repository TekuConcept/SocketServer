using System;
namespace WiimoteLib
{
    [DataContract]
    [Serializable]
    public struct BalanceBoardState
    {
        [DataMember]
        public BalanceBoardCalibrationInfo CalibrationInfo;
        [DataMember]
        public BalanceBoardSensors SensorValuesRaw;
        [DataMember]
        public BalanceBoardSensorsF SensorValuesKg;
        [DataMember]
        public BalanceBoardSensorsF SensorValuesLb;
        [DataMember]
        public float WeightKg;
        [DataMember]
        public float WeightLb;
        [DataMember]
        public PointF CenterOfGravity;
    }
}
