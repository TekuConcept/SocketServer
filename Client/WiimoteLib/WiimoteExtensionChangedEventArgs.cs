using System;
namespace WiimoteLib
{
    public class WiimoteExtensionChangedEventArgs : EventArgs
    {
        public ExtensionType ExtensionType;
        public bool Inserted;
        public WiimoteExtensionChangedEventArgs(ExtensionType type, bool inserted)
        {
            this.ExtensionType = type;
            this.Inserted = inserted;
        }
    }
}
