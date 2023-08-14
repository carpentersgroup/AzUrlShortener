using Shortener.Core.Shorten.Algorithms.CodeProviders;

namespace Shortener.Core.Shorten.Algorithms
{
    public class HashGenerator : IHash
    {
        private readonly AlphabetProviderBase _codeProvider;

        public HashGenerator(AlphabetProviderBase codeProvider)
        {
            _codeProvider = codeProvider;
        }        

        public string Generate(int length)
        {
            var bytes = new byte[length];
            System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
            Span<char> characters = stackalloc char[length];
            //iterate backwards through bytes
            for (int i = bytes.Length - 1; i >= 0; i--)
            {
                //modulus to get remainder of value divided by base
                int index = bytes[i] % _codeProvider.Base;
                //set byte to the value of the index in the conversion code
                characters[i] = _codeProvider.CONVERSION_CODE[index];
            }
            return characters.ToString();
        }
    }
}