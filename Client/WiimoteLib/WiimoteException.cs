using System;
using System.Runtime.Serialization;
namespace WiimoteLib
{
    [Serializable]
    public class WiimoteException : ApplicationException
    {
        public WiimoteException()
        {
        }
        public WiimoteException(string message)
            : base(message)
        {
        }
        public WiimoteException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
        protected WiimoteException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
