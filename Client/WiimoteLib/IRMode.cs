using System;
namespace WiimoteLib
{
    [DataContract]
    public enum IRMode : byte
    {
        Off,
        Basic,
        Extended = 3,
        Full = 5
    }
}
