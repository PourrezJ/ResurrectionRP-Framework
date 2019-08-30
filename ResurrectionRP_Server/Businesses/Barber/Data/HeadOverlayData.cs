namespace ResurrectionRP_Server.Businesses.Barber
{
    public struct HeadOverlayData
    {
        public uint Index;
        public float Opacity;
        public uint ColorId;
        public uint SecondaryColorId;

        public HeadOverlayData(uint index, float opacity, uint colorId, uint secondaryColorId)
        {
            Index = index;
            Opacity = opacity;
            ColorId = colorId;
            SecondaryColorId = secondaryColorId;
        }
    }
}
