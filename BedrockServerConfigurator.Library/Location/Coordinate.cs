using System;

namespace BedrockServerConfigurator.Library.Location
{
    public class Coordinate
    {
        public Point X { get; }
        public Point Y { get; }
        public Point Z { get; }

        public Coordinate(Point x, Point y, Point z)
        {
            if (x.Axis != Axis.X || y.Axis != Axis.Y || z.Axis != Axis.Z)
            {
                throw new Exception("Point axis must be X, Y, Z");
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
