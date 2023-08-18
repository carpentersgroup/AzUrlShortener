using System;
using System.Runtime.Serialization;

namespace ShortenerTools.Exceptions
{
    [Serializable]
    public class VanityAlreadyExistsException : ApplicationException
    {
        public VanityAlreadyExistsException()
        {
        }

        public VanityAlreadyExistsException(string message) : base(message)
        {
        }

        public VanityAlreadyExistsException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected VanityAlreadyExistsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}