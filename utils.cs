using System.Collections.Generic;
using System.Text.Json;

namespace RUAG_Random_Unique_Art_Generator_utils
{
    public class Element {
        public uint Id {get; set;}
        public string Name {get; set;}
        public string Path {get; set;}
        public int Weight {get; set;}

        public Element(uint id, string name, string path, int weight) { 
            this.Id = id;
            this.Weight = weight;
            this.Path = path;
            this.Name = name;
        }
    }
    public class Layer
    {
        public Layer(string name)
        {
            this.Name = name;
            this.Elements = new List<Element>();
        }
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public List<Element> Elements { get; set; }
    }
    public class NFTAttribute { 
        public string trait_type {get; set;}
        public string value {get; set;}

        public NFTAttribute(string trait, string value) { 
            this.trait_type = trait;
            this.value = value;
        }
    
    }
    public class NFTMetaData {
        public string dna { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string image { get; set; }
        public int edition { get; set; }
        public long date { get; set; }
        public List<NFTAttribute> attributes { get; set; }

        public NFTMetaData(string dna, string name, int edition, string description, string imageUri, List<NFTAttribute> attributes) {
            this.dna = dna;
            this.name = name + " #" + edition.ToString();
            this.edition = edition;
            this.description = description;
            this.image = imageUri + "/" + edition.ToString() + ".png";
            this.date = DateTime.UtcNow.Ticks / 10000; // 10,000 ticks per milisecond
            this.attributes = attributes;
        }

    }
    public static class Utils
    {
        private static Random random = new Random();
        private static List<List<int>> GetWeightListPerLayer(ref List<Layer> layers) {
            List<List<int>> weights = new List<List<int>>();
            foreach (Layer layer in layers){
                List<int> w = new List<int>();
                foreach (Element element in layer.Elements) { 
                    w.Add(element.Weight);
                }
                weights.Add(w);
            }
            return weights;
        }

        private static List<int> CalculateWeightSumPerLayer(ref List<List<int>> ws) {
            List<int> weightSumPerLayer = new List<int>();
            foreach (var layer in ws) {
                int weightSum = 0;
                foreach (var weight in layer) {
                    weightSum += weight;
                }
                weightSumPerLayer.Add(weightSum);
            }
            return weightSumPerLayer;
        }

        public static List<List<int>>? GenerateNFTCollection(string name, string description, string baseImageUri, int collectionSize, ref List<Layer> layers, 
                                                            ref System.ComponentModel.BackgroundWorker bw, ref System.ComponentModel.DoWorkEventArgs e,
                                                            out List<NFTMetaData> metadataArg) {

            List<List<int>> weightsPerLayer = GetWeightListPerLayer(ref layers);
            List<int> weightSum = CalculateWeightSumPerLayer(ref weightsPerLayer);
            List<List<int>> selectedPerLayer = new List<List<int>>();

            //List<string> dnaList = new List<string>();
            metadataArg = new List<NFTMetaData>();
            Dictionary<string, int> dnaList = new Dictionary<string, int>();
            int tmp = 0;
            string dnaStr;

            for (int i = 1; i <= collectionSize; i++)
            {
                List<int> selected = new List<int>();
                List<string> dna = new List<string>();
                List<NFTAttribute> tmpAttr = new List<NFTAttribute>();
                //bw.ReportProgress(0, "Calculating DNA...");
                do
                {
                    selected.Clear();
                    dna.Clear();
                    for (int j = 0; j < weightsPerLayer.Count; j++)
                    {
                        if (bw.CancellationPending)
                        {
                            e.Cancel = true;
                            return null;
                        }
                        selected.Add(GetWeightedRandomNumber(weightSum[j], weightsPerLayer[j]));
                        dna.Add(String.Format("{00}", selected[j]));
                    }
                    dnaStr = String.Join("-", dna);
                } while (dnaList.TryGetValue(dnaStr, out tmp));
                //using (SHA256 mySHA256 = SHA256.Create()) {
                    //byte[] hashValue = mySHA256.ComputeHash(Encoding.Default.GetBytes(dnaStr));
                dnaList.Add(dnaStr, i);
                for(int k = 0; k < selected.Count; k++) {
                    tmpAttr.Add(new NFTAttribute(layers[k].Name.Substring(2), layers[k].Elements[selected[k]].Name));
                //}
                }
                metadataArg.Add(new NFTMetaData(dnaStr, name, i, description, baseImageUri, tmpAttr));
                //bw.ReportProgress((i * 100) / (collectionSize), "Adding DNA: " + dnaStr + "...");
                selectedPerLayer.Add(selected);
            }
            return selectedPerLayer;
        }
        public static void GenerateNFTMetaDataFile(string path, NFTMetaData data) {
            string jsonString = JsonSerializer.Serialize(data);
            File.WriteAllText(path + "\\" + data.edition.ToString() + ".json", jsonString);
        }

        private static int GetWeightedRandomNumber(int sumWeight, List<int> w) {
            int rnd = random.Next(0, sumWeight);
            int i;
            for(i = 0; i < w.Count; i++) {
                if (rnd < w[i])
                    break;
                rnd -= w[i];
            }
            return i;
        }
    }
}
