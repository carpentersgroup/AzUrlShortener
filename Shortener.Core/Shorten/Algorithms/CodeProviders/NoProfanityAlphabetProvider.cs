namespace Shortener.Core.Shorten.Algorithms.CodeProviders
{
    public class NoProfanityAlphabetProvider : AlphabetProviderBase
    {
        public NoProfanityAlphabetProvider() : base("-023456789ABCDEFGHJKLMNOPQRSTVWXYZ_abcdefghjklmnopqrstvwxyz")
        {
        }
    }
}