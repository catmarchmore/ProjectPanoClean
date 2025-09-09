namespace ProjectPano.Model
{
    public class CircleNode
    {
        public string name { get; set; }
        public decimal value { get; set; }
        public List<CircleNode> children { get; set; } = new List<CircleNode>();
    }
}
