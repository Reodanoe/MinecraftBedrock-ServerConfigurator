using System;

namespace BedrockServerConfigurator.Library.Location
{
    public class Coordinate
    {
        public PublicPoint X { get; }
        public PublicPoint Y { get; }
        public PublicPoint Z { get; }

        public Coordinate(PublicPoint x, PublicPoint y, PublicPoint z)
        {
            if (x.Axis != Axis.X || y.Axis != Axis.Y || z.Axis != Axis.Z)
            {
                throw new Exception("Points must be X - Y - Z");
            }

            X = x;
            Y = y;
            Z = z;
        }

        public override string ToString()
        {
            return $"{X} {Y} {Z}";
        }
    }
}
