namespace ResurrectionRP_Server.Models
{
    struct Mask
    {
        public string name;
        public byte variation;
        public byte texture;

        public Mask(string name, byte variation, byte texture = 0)
        {
            this.name = name;
            this.variation = variation;
            this.texture = texture;
        }
    }
}
