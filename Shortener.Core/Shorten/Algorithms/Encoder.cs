using Shortener.Core.Shorten.Algorithms.CodeProviders;

namespace Shortener.Core.Shorten.Algorithms
{
    public class Encoder : IEncode
    {
        private readonly AlphabetProviderBase _codeProvider;

        public Encoder(AlphabetProviderBase codeProvider)
        {
            _codeProvider = codeProvider;
        }

        public string Encode(int id)
        {
            string characters = string.Empty;

            while (id > 0)
            {
                int index = id % _codeProvider.Base;
                id /= _codeProvider.Base;
                characters += _codeProvider.CONVERSION_CODE[index];
            }
            return string.Concat(characters.Reverse());
        }
    }
}