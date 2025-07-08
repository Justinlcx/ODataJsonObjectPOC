
using System.Text.Json.Nodes;

namespace ODataConfigPOCnet9.Models
{
    public class DataSet
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        public Config? Data { get; set; }
    }

    public abstract class Config
    {
    }

    public class JsonConfig : Config 
    {
        public JsonObject? Value { get; set; } = new JsonObject();
    }

    public class XmlConfig : Config
    {
        public string? Value { get; set; }
    }

}
