namespace BedrockServerConfigurator.Library.Location
{
    public class LocalPoint : Point
    {
        public LocalPoint(Axis axis, float pos) : base(axis, pos)
        {
        }

        public override string ToString()
        {
            return $"~{Pos}";
        }
    }
}
