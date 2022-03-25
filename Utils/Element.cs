namespace RUAG_Random_Unique_Art_Generator_.Utils
{
    internal class Element
    {
        public Element(uint id, string name, string path, int weight)
        {
            this.Id = id;
            this.Weight = weight;
            this.Path = path;
            this.Name = name;
            var ms = new MemoryStream();
            var img = Image.FromFile(this.Path);
            img.Save(ms, img.RawFormat);
            this.ImageBytes = ms.ToArray();
        }
        public uint Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public int Weight { get; set; }
        public byte[] ImageBytes { get; set; }

    }
}
