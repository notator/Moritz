
using Moritz.Xml;

namespace Moritz.Symbols
{
	internal class FramePadding
	{
        public FramePadding(TextFrameType frameType, float paddingTop, float paddingRight, float paddingBottom, float paddingLeft)
        {
            FrameType = frameType;
            Top = paddingTop;
            Right = paddingRight;
            Bottom = paddingBottom;
            Left = paddingLeft;
        }

		public TextFrameType FrameType { get; }
		public float Top { get; }
		public float Right { get; }
		public float Bottom { get; }
		public float Left { get; }
	}
}
