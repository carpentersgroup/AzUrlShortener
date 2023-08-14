namespace Fizzibly.Auth
{
    public abstract class BaseHandler
    {
        protected IHandler? _next;

        public IHandler? Use(IHandler? handler)
        {
            this._next = handler;
            return handler;
        }
    }
}