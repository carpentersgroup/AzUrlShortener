namespace Fizzibly.Auth.Handlers.Abstractions
{
    public abstract class BaseHandler
    {
        protected IHandler? _next;

        public IHandler? Use(IHandler? handler)
        {
            _next = handler;
            return handler;
        }
    }
}