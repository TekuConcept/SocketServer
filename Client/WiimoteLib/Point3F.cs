using System;
namespace WiimoteLib
{
    [DataContract]
    [Serializable]
    public struct Point3F
    {
        [DataMember]
        public float X;
        [DataMember]
        public float Y;
        [DataMember]
        public float Z;
        public override string ToString()
        {
            return string.Format("{{X={0}, Y={1}, Z={2}}}", this.X, this.Y, this.Z);
        }
    }
}
