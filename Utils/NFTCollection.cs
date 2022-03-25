using System.Text;
using System.Security.Cryptography;

namespace RUAG_Random_Unique_Art_Generator_.Utils
{
    internal class NFTCollection
    {
        private List<NFT> nfts;
        private Random random;
        private int collectionSize;
        private string name, description, baseImgUri;

        public NFTCollection(string name, string description, string baseImageUri, int collSize, List<Layer> layers) { 
            this.name = name;
            this.description = description; 
            this.baseImgUri = baseImageUri;
            this.collectionSize = collSize;
            this.Layers = layers;
            this.nfts = new List<NFT>();
            this.random = new Random();
        }
        public List<Layer> Layers { get; set; }
        public void GenerateCollection(string savePath, System.ComponentModel.BackgroundWorker bw, System.ComponentModel.DoWorkEventArgs e, ref CancellationTokenSource tokenSource) {
            List<int> weightSum = CalculateWeightSumPerLayer();
            Dictionary<string, int> dnaList = new Dictionary<string, int>();
            StringBuilder dna = new StringBuilder();
            bw.ReportProgress(0, "Calculating DNA...");
            for (int i = 1; i <= this.collectionSize; i++) {
                List<Element> els = new List<Element>();
                List<NFTAttribute> tmpAttrs = new List<NFTAttribute>();
                do {
                    els.Clear();
                    dna.Clear();
                    for (int j = 0; j < weightSum.Count; j++) {
                        if (bw.CancellationPending)
                        {
                            e.Cancel = true;
                            return;
                        }
                        int elemIdx = GetWeightedRandomNumber(weightSum[j], this.Layers[j].Weights);
                        els.Add(this.Layers[j].Elements[elemIdx]);
                        dna.AppendJoin('-', elemIdx, "");
                    }
                    dna.Remove(dna.ToString().Length - 1, 1);
                } while (dnaList.TryGetValue(dna.ToString(), out _));
                var dnaHash = new StringBuilder();
                using (SHA256 mySHA1 = SHA256.Create()) {
                    byte[] hashValue = mySHA1.ComputeHash(Encoding.Default.GetBytes(dna.ToString()));
                    // Loop through each byte of the hashed data
                    // and format each one as a hexadecimal string.
                    for (int k = 0; k < hashValue.Length; k++)
                    {
                        dnaHash.Append(hashValue[k].ToString("x2"));
                    }
                }
                dnaList.Add(dna.ToString(), i);
                for (int k = 0; k < els.Count; k++)
                {
                    tmpAttrs.Add(new NFTAttribute(this.Layers[k].Name[2..], els[k].Name));
                }
                NFTMetadata tmpMetadata = new NFTMetadata(dnaHash.ToString(), this.name, i, this.description, this.baseImgUri, tmpAttrs);
                this.nfts.Add(new NFT(els, tmpMetadata));
            }
            var token = tokenSource.Token;

            List<Task> tasks = new List<Task>();
            foreach (var nft in this.nfts)
            {
                tasks.Add(Task.Run(() =>
                {
                    if (token.IsCancellationRequested)
                        token.ThrowIfCancellationRequested();
                    nft.Save(savePath);
                }, token));
            }
            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch (ImageMagick.MagickException)
            {
                e.Cancel = true;
            }
            catch (OperationCanceledException)
            {
                e.Cancel = true;
            }
            finally
            {
                tokenSource.Dispose();
            }
        }
        private List<int> CalculateWeightSumPerLayer() {
            List<int> weightSumPerLayer = new List<int>();
            foreach(var l in this.Layers) { 
                int weightSum = 0;
                foreach (var e in l.Elements) { 
                    weightSum += e.Weight;
                }
                weightSumPerLayer.Add(weightSum);
            }
            return weightSumPerLayer;
        }
        private int GetWeightedRandomNumber(int sumWeight, List<int> w)
        {
            int rnd = random.Next(0, sumWeight);
            int i;
            for (i = 0; i < w.Count; i++)
            {
                if (rnd < w[i])
                    break;
                rnd -= w[i];
            }
            return i;
        }
    }
}
