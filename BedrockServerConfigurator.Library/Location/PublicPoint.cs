namespace BedrockServerConfigurator.Library.Location
{
    public class PublicPoint : Point
    {
        public PublicPoint(Axis axis, float pos) : base(axis, pos)
        {
        }

        public override string ToString()
        {
            return Pos.ToString();
        }
    }
}
