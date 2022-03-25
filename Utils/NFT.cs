using ImageMagick;

namespace RUAG_Random_Unique_Art_Generator_.Utils
{
    internal class NFT
    {
        public NFT(List<Element> els, NFTMetadata meta)
        {
            this.Metadata = meta;
            this.Elements = els;
        }
        public NFTMetadata Metadata { get; set; }
        public List<Element> Elements { get; set; }

        private void SaveToImage(string path) {
            using (var images = new MagickImageCollection())
            {
                foreach(var el in this.Elements)
                {
                    var img = new MagickImage(el.ImageBytes);
                    images.Add(img);
                }
                using (var result = images.Mosaic())
                {
                    result.WriteAsync(path + "\\" + this.Metadata.edition.ToString() + ".png");
                }
            }
        }
        public void Save(string path) {
            SaveToImage(path);
            this.Metadata.GenerateFile(path);
        }
    }
}
