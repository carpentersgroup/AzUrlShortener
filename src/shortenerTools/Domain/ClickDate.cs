
using System;

namespace Cloud5mins.domain
{
    public class ClickDate
    {
        public string DateClicked { get; set; }
        public int Count { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public DateOnly DateForOrdering { get; internal set; }
    }
}
