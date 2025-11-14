namespace PointProducer.API.Models
{
    public class Point
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public Geometry Geometry { get; set; }
        public Dictionary<string, string> Properties { get; set; } 
    }
}
