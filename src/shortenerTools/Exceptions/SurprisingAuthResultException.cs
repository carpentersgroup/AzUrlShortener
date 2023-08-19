using Fizzibly.Auth.Models;
using System;
using System.Runtime.Serialization;

namespace ShortenerTools.Exceptions
{
    [Serializable]
    public class SurprisingAuthResultException : Exception
    {
        public AuthResult AuthResult { get; set; }

        public SurprisingAuthResultException()
        {
            AuthResult = AuthResult.Ok;
        }

        public SurprisingAuthResultException(AuthResult authResult)
        {
            AuthResult = authResult;
        }

        public SurprisingAuthResultException(string message) : base(message)
        {
        }

        public SurprisingAuthResultException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SurprisingAuthResultException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}