using System;
namespace WiimoteLib
{
    [DataContract]
    public enum ExtensionType : long
    {
        None,
        Nunchuk = 2753560576L,
        ClassicController = 1102265188609, //2753560833L,
        Guitar = 2753560835L,
        Drums = 1102265188611L,
        BalanceBoard = 2753561602L,
        ParitallyInserted = 281474976710655L
    }
}
