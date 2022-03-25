namespace RUAG_Random_Unique_Art_Generator_.Utils
{
    internal class Layer
    {
        public Layer(string name, List<Element> elems)
        {
            this.Name = name;
            this.Elements = elems;
            this.Weights = GetWeights();
        }
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public List<Element> Elements { get; set; }
        public List<int> Weights { get; set; }
        private List<int> GetWeights() { 
            List<int> weights = new List<int>();
            foreach (var w in this.Elements) { 
                weights.Add(w.Weight);
            }
            return weights;
        }
    }
}
