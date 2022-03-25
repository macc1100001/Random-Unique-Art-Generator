namespace RUAG_Random_Unique_Art_Generator_.Utils
{
    internal class NFTAttribute
    {

        public NFTAttribute(string trait, string value)
        {
            this.trait_type = trait;
            this.value = value;
        }
        public string trait_type { get; set; }
        public string value { get; set; }
    }
}
