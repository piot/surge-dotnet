using System;

namespace Piot.Surge
{
    public class DeserializeException : Exception
    {
        public DeserializeException(string something) : base(something)
        {
            
        }
    }
}