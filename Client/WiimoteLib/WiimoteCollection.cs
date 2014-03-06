using System;
using System.Collections.ObjectModel;
namespace WiimoteLib
{
    public class WiimoteCollection : Collection<Wiimote>
    {
        public void FindAllWiimotes()
        {
            Wiimote.FindWiimote(new Wiimote.WiimoteFoundDelegate(this.WiimoteFound));
        }
        private bool WiimoteFound(string devicePath)
        {
            base.Add(new Wiimote(devicePath));
            return true;
        }
    }
}
