namespace BedrockServerConfigurator.Library.Location
{
    public class LocalCoordinate : Coordinate
    {
        public LocalCoordinate(LocalPoint x, LocalPoint y, LocalPoint z) : base(x, y, z)
        {
        }

        public LocalCoordinate(float x = 0, float y = 0, float z = 0) : 
            this(new LocalPoint(Axis.X, x), new LocalPoint(Axis.Y, y), new LocalPoint(Axis.Z, z))
        {
        }
    }
}
