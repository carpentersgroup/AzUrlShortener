namespace Shortener.Core.Shorten.Algorithms
{
    public enum ShortenerAlgorithm
    {
        None,
        IdPlusRandomFixedLength,
        RandomFixedLength,
        RandomExtendableLength,
        EncodeDecode,
    }
}