namespace Fizzibly.Auth
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