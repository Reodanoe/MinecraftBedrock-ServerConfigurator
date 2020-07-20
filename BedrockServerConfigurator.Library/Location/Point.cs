namespace BedrockServerConfigurator.Library.Location
{
    public abstract class Point
    {
        public Axis Axis { get; }
        public float Pos { get; }

        public Point(Axis axis, float pos)
        {
            Axis = axis;
            Pos = pos;
        }
    }

    public enum Axis
    {
        X, Y, Z
    }
}
