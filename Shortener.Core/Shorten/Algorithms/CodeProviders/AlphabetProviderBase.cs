namespace Shortener.Core.Shorten.Algorithms.CodeProviders
{
    public abstract class AlphabetProviderBase
    {
        protected AlphabetProviderBase(string conversionCode)
        {
            CONVERSION_CODE = conversionCode;
            Base = conversionCode.Length;
        }

        public readonly string CONVERSION_CODE;
        public readonly int Base;
    }
}