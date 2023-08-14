using Microsoft.Azure.Cosmos.Table;

namespace Shortener.Azure.Entities
{
    public class NextId : TableEntity
    {
        public int Id { get; set; }
    }
}