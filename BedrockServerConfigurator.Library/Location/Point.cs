namespace BedrockServerConfigurator.Library.Location
{
    public class LocalPoint
    {
        public Axis Axis { get; }
        public float Pos { get; }

        public LocalPoint(Axis axis, float pos)
        {
            Axis = axis;
            Pos = pos;
        }

        public override string ToString()
        {
            return $"~{Pos}";
        }
    }

    public class PublicPoint : LocalPoint
    {
        public PublicPoint(Axis axis, float pos) : base(axis, pos)
        {
        }

        public override string ToString()
        {
            return Pos.ToString();
        }
    }

    public enum Axis
    {
        X, Y, Z
    }
}
