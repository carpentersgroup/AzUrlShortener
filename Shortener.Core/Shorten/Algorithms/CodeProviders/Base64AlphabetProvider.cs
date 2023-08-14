namespace Shortener.Core.Shorten.Algorithms.CodeProviders
{
    public class Base64AlphabetProvider : AlphabetProviderBase
    {
        public Base64AlphabetProvider() : base("-0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz")
        {
        }
    }
}