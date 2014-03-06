using System;
namespace WiimoteLib
{
    [DataContract]
    [Serializable]
    public struct Point3
    {
        [DataMember]
        public int X;
        [DataMember]
        public int Y;
        [DataMember]
        public int Z;
        public override string ToString()
        {
            return string.Format("{{X={0}, Y={1}, Z={2}}}", this.X, this.Y, this.Z);
        }
    }
}
