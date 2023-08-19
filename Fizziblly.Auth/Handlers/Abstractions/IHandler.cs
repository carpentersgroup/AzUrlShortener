using Fizzibly.Auth.Models;

namespace Fizzibly.Auth.Handlers.Abstractions
{
    public interface IHandler
    {
        Task<AuthResult> Handle(object request);
        IHandler? Use(IHandler? handler);
    }
}