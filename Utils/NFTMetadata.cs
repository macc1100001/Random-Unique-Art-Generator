using System.Text.Json;

namespace RUAG_Random_Unique_Art_Generator_.Utils
{
    internal class NFTMetadata
    {

        public NFTMetadata(string dna, string name, int edition, string description, string imageUri, List<NFTAttribute> attributes)
        {
            this.dna = dna;
            this.name = name + " #" + edition.ToString();
            this.edition = edition;
            this.description = description;
            this.image = imageUri + "/" + edition.ToString() + ".png";
            this.date = DateTime.UtcNow.Ticks / 10000; // 10,000 ticks per milisecond, divide by 10,000 to get seconds
            this.attributes = attributes;
        }
        public string dna { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string image { get; set; }
        public int edition { get; set; }
        public long date { get; set; }
        public List<NFTAttribute> attributes { get; set; }
        public void GenerateFile(string path)
        {
            string jsonString = JsonSerializer.Serialize(this);
            File.WriteAllTextAsync(path + "\\" + this.edition.ToString() + ".json", jsonString);
        }
    }
}
