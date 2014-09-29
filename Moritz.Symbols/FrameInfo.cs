
using Moritz.Xml;

namespace Moritz.Symbols
{
	internal class FrameInfo
	{
        public FrameInfo(TextFrameType frameType, float paddingX, float paddingY, float strokeWidth, ColorString colorString)
        {
            _frameType = frameType;
            _paddingX = paddingX;
            _paddingY = paddingY;
            _strokeWidth = strokeWidth;
            _colorString = colorString;
        }

        public TextFrameType FrameType { get { return _frameType; } }
        private TextFrameType _frameType = TextFrameType.none;
        
        public float PaddingX { get { return _paddingX; } }
        private float _paddingX = 0F;

        public float PaddingY { get { return _paddingY; } }
        private float _paddingY = 0F;

        public float StrokeWidth { get { return _strokeWidth; } }
        private float _strokeWidth = 0F;

        /// <summary>
        /// A string of 6 Hex digits (RRGGBB).
        /// </summary>
        public ColorString ColorString { get { return _colorString; } }
        private ColorString _colorString = null; 

    }
}
