namespace Shortener.Azure.Pocos
{
    public class WellKnownPoco
    {
        public string Filename { get; set; }
        public string Content { get; set; }

        public WellKnownPoco(string filename, string content)
        {
            Filename = filename;
            Content = content;
        }
    }
}
