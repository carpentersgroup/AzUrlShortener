using Fizzibly.Auth.Handlers.Abstractions;

namespace Fizzibly.Auth.Handlers
{
    public class HandlerContainer
    {
        public HandlerContainer(IHandler handler, string? key = null)
        {
            Handler = handler;
            Key = key;
        }

        public IHandler Handler { get; }
        public string? Key { get; }
    }
}