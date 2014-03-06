using System;
namespace WiimoteLib
{
    [DataContract]
    [Serializable]
    public struct Point
    {
        [DataMember]
        public int X;
        [DataMember]
        public int Y;
        public override string ToString()
        {
            return string.Format("{{X={0}, Y={1}}}", this.X, this.Y);
        }
    }
}
