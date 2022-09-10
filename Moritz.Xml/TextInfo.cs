
using System;
using System.Diagnostics;

namespace Moritz.Xml
{
    public class TextInfo
    {
        public TextInfo(string text, string fontFamily, float fontHeight, TextHorizAlign textHorizAlign)
            : this(text, fontFamily, fontHeight, new ColorString("000000"), textHorizAlign)
        {
        }

        public TextInfo(string text, string fontFamily, float fontHeight, ColorString colorString,
            TextHorizAlign textHorizAlign)
        {
            Debug.Assert(!String.IsNullOrEmpty(text));
            _text = text;
            _fontFamily = fontFamily;
            _fontHeight = fontHeight;
            _textHorizAlign = textHorizAlign;
            _colorString = colorString;
        }

        public string FontFamily { get { return _fontFamily; } }
        private readonly string _fontFamily = null;

        public string Text { get { return _text; } }
        private readonly string _text = null;

        public float FontHeight { get { return _fontHeight; } }
        private readonly float _fontHeight = 0F;

        public TextHorizAlign TextHorizAlign { get { return _textHorizAlign; } }
        private readonly TextHorizAlign _textHorizAlign = 0F;

        /// <summary>
		/// A string of 6 Hex digits (RRGGBB).
		/// </summary>
        public ColorString ColorString { get { return _colorString; } }
        private readonly ColorString _colorString = null;
    }
}
