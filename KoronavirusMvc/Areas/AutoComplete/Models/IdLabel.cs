using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KoronavirusMvc.Areas.AutoComplete.Models
{
    public class IdLabel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("label")]
        public string Label { get; set; }

        public IdLabel() {}
        public IdLabel(int id, string label)
        {
            Id = id;
            Label = label;
        }
    }
}
