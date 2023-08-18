namespace Fizzibly.Auth
{
    public interface IHandler
    {
        Task<AuthResult> Handle(object request);
        IHandler? Use(IHandler? handler);
    }
}