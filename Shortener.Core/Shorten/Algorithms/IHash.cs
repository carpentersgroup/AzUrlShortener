namespace Shortener.Core.Shorten.Algorithms
{
    public interface IHash
    {
        string Generate(int length);
    }
}