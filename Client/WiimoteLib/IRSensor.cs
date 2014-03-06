using System;
namespace WiimoteLib
{
    [DataContract]
    [Serializable]
    public struct IRSensor
    {
        [DataMember]
        public Point RawPosition;
        [DataMember]
        public PointF Position;
        [DataMember]
        public int Size;
        [DataMember]
        public bool Found;
        public override string ToString()
        {
            return string.Format("{{{0}, Size={1}, Found={2}}}", this.Position, this.Size, this.Found);
        }
    }
}
