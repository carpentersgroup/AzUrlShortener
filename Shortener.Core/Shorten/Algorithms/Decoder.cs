using Shortener.Core.Shorten.Algorithms.CodeProviders;

namespace Shortener.Core.Shorten.Algorithms
{
    public class Decoder : IDecode
    {
        private readonly AlphabetProviderBase _codeProvider;

        public Decoder(AlphabetProviderBase codeProvider)
        {
            _codeProvider = codeProvider;
        }

        public int Decode(string code)
        {
            int id = 0;
            for (int i = 0; i < code.Length; i++)
            {
                int index = _codeProvider.CONVERSION_CODE.IndexOf(code[i]);
                id = id * _codeProvider.Base + index;
            }
            return id;
        }

        
    }
}