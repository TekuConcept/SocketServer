using System;
namespace WiimoteLib
{
    public enum InputReport : byte
    {
        Status = 32,
        ReadData,
        OutputReportAck,
        Buttons = 48,
        ButtonsAccel,
        IRAccel = 51,
        ButtonsExtension,
        ExtensionAccel,
        IRExtensionAccel = 55
    }
}
