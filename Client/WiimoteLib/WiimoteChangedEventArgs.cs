using System;
namespace WiimoteLib
{
    public class WiimoteChangedEventArgs : EventArgs
    {
        public WiimoteState WiimoteState;
        public WiimoteChangedEventArgs(WiimoteState ws)
        {
            this.WiimoteState = ws;
        }
    }
}
