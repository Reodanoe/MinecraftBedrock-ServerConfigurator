using System;

namespace BedrockServerConfigurator.Library.Location
{
    public class PublicCoordinate : Coordinate
    {
        private PublicCoordinate(PublicPoint x, PublicPoint y, PublicPoint z) : base(x, y, z)
        {
        }

        public static PublicCoordinate GetPublicCoordinate(float x = 0, float y = 0, float z = 0)
        {
            return new PublicCoordinate(new PublicPoint(Axis.X, x), new PublicPoint(Axis.Y, y), new PublicPoint(Axis.Z, z));
        }

        public static double Distance(PublicCoordinate one, PublicCoordinate two)
        {
            return Math.Sqrt(Math.Pow(two.X.Pos - one.X.Pos, 2) +
                             Math.Pow(two.Y.Pos - one.Y.Pos, 2) +
                             Math.Pow(two.Z.Pos - one.Z.Pos, 2));
        }
    }
}
