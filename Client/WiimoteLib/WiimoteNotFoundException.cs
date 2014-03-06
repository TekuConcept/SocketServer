using System;
using System.Runtime.Serialization;
namespace WiimoteLib
{
    [Serializable]
    public class WiimoteNotFoundException : ApplicationException
    {
        public WiimoteNotFoundException()
        {
        }
        public WiimoteNotFoundException(string message)
            : base(message)
        {
        }
        public WiimoteNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
        protected WiimoteNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
