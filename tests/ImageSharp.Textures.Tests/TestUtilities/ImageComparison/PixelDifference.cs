using System.Numerics;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;

namespace SixLabors.ImageSharp.Textures.Tests.TestUtilities.ImageComparison
{
    public readonly struct PixelDifference
    {
        public PixelDifference(
            Point position,
            float redDifference,
            float greenDifference,
            float blueDifference,
            float alphaDifference)
        {
            this.Position = position;
            this.RedDifference = redDifference;
            this.GreenDifference = greenDifference;
            this.BlueDifference = blueDifference;
            this.AlphaDifference = alphaDifference;
        }

        public PixelDifference(Point position, Vector4 expected, Vector4 actual)
            : this(position,
                actual.X - expected.X,
                actual.Y - expected.Y,
                actual.Z - expected.Z,
                actual.W - expected.W)
        {
        }

        public Point Position { get; }

        public float RedDifference { get; }
        public float GreenDifference { get; }
        public float BlueDifference { get; }
        public float AlphaDifference { get; }

        public override string ToString() =>
            $"[Δ({this.RedDifference},{this.GreenDifference},{this.BlueDifference},{this.AlphaDifference}) @ ({this.Position.X},{this.Position.Y})]";
    }
}
