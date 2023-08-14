namespace Shortener.Core.Shorten
{
    public interface IUrlGenerator
    {
        Task<string> GenerateAsync(string? host);
    }
}