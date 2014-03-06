using System;
namespace WiimoteLib
{
    [DataContract]
    [Serializable]
    public struct PointF
    {
        [DataMember]
        public float X;
        [DataMember]
        public float Y;
        public override string ToString()
        {
            return string.Format("{{X={0}, Y={1}}}", this.X, this.Y);
        }
    }
}
