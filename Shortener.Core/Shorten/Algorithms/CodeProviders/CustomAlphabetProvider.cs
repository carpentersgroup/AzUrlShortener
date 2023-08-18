namespace Shortener.Core.Shorten.Algorithms.CodeProviders
{
    public class CustomAlphabetProvider : AlphabetProviderBase
    {
        public CustomAlphabetProvider(string alphabet) : base(alphabet)
        {
            if (string.IsNullOrWhiteSpace(alphabet))
                throw new ArgumentException("CustomAlphabet has not been supplied");
        }
    }
}