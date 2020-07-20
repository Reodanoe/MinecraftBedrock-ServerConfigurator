namespace BedrockServerConfigurator.Library.Location
{
    public class LocalCoordinate : Coordinate
    {
        private LocalCoordinate(LocalPoint x, LocalPoint y, LocalPoint z) : base(x, y, z)
        {
        }

        public static LocalCoordinate GetLocalCoordinate(float x = 0, float y = 0, float z = 0)
        {
            return new LocalCoordinate(new LocalPoint(Axis.X, x), new LocalPoint(Axis.Y, y), new LocalPoint(Axis.Z, z));
        }
    }
}
