using System;
namespace WiimoteLib
{
    [DataContract]
    [Serializable]
    public class WiimoteState
    {
        [DataMember]
        public AccelCalibrationInfo AccelCalibrationInfo;
        [DataMember]
        public AccelState AccelState;
        [DataMember]
        public ButtonState ButtonState;
        [DataMember]
        public IRState IRState;
        [DataMember]
        public byte BatteryRaw;
        [DataMember]
        public float Battery;
        [DataMember]
        public bool Rumble;
        [DataMember]
        public bool Extension;
        [DataMember]
        public ExtensionType ExtensionType;
        [DataMember]
        public NunchukState NunchukState;
        [DataMember]
        public ClassicControllerState ClassicControllerState;
        [DataMember]
        public GuitarState GuitarState;
        [DataMember]
        public DrumsState DrumsState;
        public BalanceBoardState BalanceBoardState;
        [DataMember]
        public LEDState LEDState;
        public WiimoteState()
        {
            this.IRState.IRSensors = new IRSensor[4];
        }
    }
}
