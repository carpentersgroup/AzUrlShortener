namespace Shortener.Azure.Pocos
{
    public class NextIdPoco
    {
        public NextIdPoco(string authority, int id)
        {
            Authority = authority;
            Id = id;
        }

        public string Authority { get; set; }

        public int Id { get; set; }
    }
}