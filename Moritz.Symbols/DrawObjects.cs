using Moritz.Globals;
using Moritz.Xml;

using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace Moritz.Symbols
{
    /// <summary>
    /// Base class of all DrawObject classes.
    /// </summary>
    public abstract class DrawObject
    {
        public DrawObject()
        { }

        public DrawObject(object container)
        {
            Container = container;
        }

        public abstract void WriteSVG(SvgWriter w);

        public object Container;

        /// <summary>
        /// Set when this drawObject is inside a Transposable drawObject
        /// </summary>
        public string EnharmonicNote
        {
            get { return _enharmonicNote; }
            set
            {
                // possible Values:
                // "C", "C#", "Db", "D", "D#", "Eb", "E", "E#", "Fb", "F", "F#" "Gb",
                // "G", "G#", "Ab", "A", "A#", "Bb", "B", "B#", "Cb"
                M.Assert(Regex.Matches(value, @"^[A-G][#b]?$") != null);
                _enharmonicNote = value;
            }
        }
        /// <summary>
        /// Set when this object is part of the gallery
        /// </summary>
        public string Name = "";
        /// <summary>
        /// Used by AssistantComposer
        /// </summary>
        public Metrics Metrics = null;

        private string _enharmonicNote = "";
    }

    public class Text : DrawObject
    {
        public Text(object container, string text, string fontName, float fontHeight, TextHorizAlign align)
            : base(container)
        {
            _textInfo = new TextInfo(text, fontName, fontHeight, align);
        }

        public override void WriteSVG(SvgWriter w)
        {
            //w.SvgText(TextInfo, Metrics as TextMetrics); // does not work with DynamicMetrics
            if(Metrics != null)
                Metrics.WriteSVG(w);
            if(_frameInfo != null)
            {
                switch(_frameInfo.FrameType)
                {
                    case TextFrameType.none:
                        break;
                    case TextFrameType.rectangle:
                        w.SvgRect(CSSObjectClass.regionInfoFrame, Metrics.Left, Metrics.Top, Metrics.Right - Metrics.Left, Metrics.Bottom - Metrics.Top);
                        break;
                    case TextFrameType.ellipse:
                        w.SvgEllipse(CSSObjectClass.regionInfoFrame, Metrics.Left, Metrics.Top, (Metrics.Right - Metrics.Left) / 2, ((Metrics.Bottom - Metrics.Top) / 2));
                        break;
                    case TextFrameType.circle:
                        w.SvgCircle(CSSObjectClass.regionInfoFrame, Metrics.Right - Metrics.Left, Metrics.Bottom - Metrics.Top, ((Metrics.Right - Metrics.Left) / 2));
                        break;
                }
            }
        }

        // attributes
        public TextInfo TextInfo { get { return _textInfo; } }
        private readonly TextInfo _textInfo = null;
        public FramePadding FrameInfo { get { return _frameInfo; } }
        protected FramePadding _frameInfo = null;
    }

    internal class StaffNameText : Text
    {
        public StaffNameText(object container, string staffName, float fontHeight)
            : base(container, staffName, "Arial", fontHeight, TextHorizAlign.center)
        {
        }

        public override string ToString()
        {
            return "staffname: " + TextInfo.Text;
        }
    }

    internal class FramedBarNumberText : Text
    {
        public FramedBarNumberText(object container, string text, float gap, float stafflinethickness)
            : base(container, text, "Arial", (gap * 2F), TextHorizAlign.center)
        {
            float paddingX = 22F;
            if(text.Length > 1)
                paddingX = 10F;
            float paddingY = 22F;

            float strokeWidth = stafflinethickness * 1.2F;

            _frameInfo = new FramePadding(TextFrameType.rectangle, paddingY, paddingX, paddingY, paddingX);
        }

        public override string ToString()
        {
            return "barnumber: " + TextInfo.Text;
        }
    }

    #region Framed region texts

    internal class TextList : DrawObject
    {
        public TextList(object container, List<string> textStrings, float gap, float stafflinethickness, TextHorizAlign hAlign)
        {
            foreach(string textString in textStrings)
            {
                this.Texts.Add(new Text(container, textString, "Arial", (gap * 2.5F), hAlign));
            }
        }

        public override void WriteSVG(SvgWriter w)
        {
            //w.SvgText(TextInfo, Metrics as TextMetrics); // does not work with DynamicMetrics
            if(Metrics != null)
                Metrics.WriteSVG(w);

            foreach(Text text in Texts)
            {
                w.SvgText(CSSObjectClass.regionInfoString, text.TextInfo.Text, Metrics.Left, Metrics.Bottom);
            }
            if(_frameInfo != null)
            {
                switch(_frameInfo.FrameType)
                {
                    case TextFrameType.none:
                        break;
                    case TextFrameType.rectangle:
                        w.SvgRect(CSSObjectClass.regionInfoFrame, Metrics.Left, Metrics.Top, Metrics.Right - Metrics.Left, Metrics.Bottom - Metrics.Top);
                        break;
                    case TextFrameType.ellipse:
                        w.SvgEllipse(CSSObjectClass.regionInfoFrame, Metrics.Left, Metrics.Top, (Metrics.Right - Metrics.Left) / 2, (Metrics.Bottom - Metrics.Top) / 2);
                        break;
                    case TextFrameType.circle:
                        w.SvgCircle(CSSObjectClass.regionInfoFrame, Metrics.Right - Metrics.Left, Metrics.Bottom - Metrics.Top, ((Metrics.Right - Metrics.Left) / 2));
                        break;
                }
            }
        }

        public readonly List<Text> Texts = new List<Text>();
        public FramePadding FrameInfo { get { return _frameInfo; } }
        protected FramePadding _frameInfo = null;
    }

    internal class FramedMultilineText : TextList
    {
        public FramedMultilineText(object container, List<string> textStrings, float gap, float stafflinethickness, TextHorizAlign hAlign)
            : base(container, textStrings, gap, stafflinethickness, hAlign)
        {
            float paddingTop = 40F;
            float paddingRight = 26F;
            float paddingBottom = 30F;
            float paddingLeft = 26F;

            _frameInfo = new FramePadding(TextFrameType.rectangle, paddingTop, paddingRight, paddingBottom, paddingLeft);
        }

        protected string StringList()
        {
            StringBuilder sb = new StringBuilder();
            foreach(Text t in this.Texts)
            {
                sb.Append(t.TextInfo.Text);
                sb.Append(", ");
            }
            sb.Remove(sb.Length - 2, 2);

            return sb.ToString();
        }
    }

    internal class FramedRegionStartText : FramedMultilineText
    {
        public FramedRegionStartText(object container, List<string> textStrings, float gap, float stafflinethickness)
            : base(container, textStrings, gap, stafflinethickness, TextHorizAlign.left)
        {
        }

        public override string ToString()
        {
            return "framedRegionStartText: " + StringList();
        }
    }

    internal class FramedRegionEndText : FramedMultilineText
    {
        public FramedRegionEndText(object container, List<string> textStrings, float gap, float stafflinethickness)
            : base(container, textStrings, gap, stafflinethickness, TextHorizAlign.right)
        {
            foreach(string text in textStrings)
            {
                if(text.Contains("➔"))
                {
                    TextFrameType frameType = _frameInfo.FrameType;
                    float paddingTop = _frameInfo.Top;
                    float paddingRight = _frameInfo.Right + 15F; // compensate for missing arrow width
                    float paddingBottom = _frameInfo.Bottom;
                    float paddingLeft = _frameInfo.Left + 15F; // compensate for missing arrow width

                    _frameInfo = new FramePadding(frameType, paddingTop, paddingRight, paddingBottom, paddingLeft);

                    break;
                }
                else
                {
                    M.Assert(text.Contains("end"));
                }
            }
        }

        public override string ToString()
        {
            return "framedRegionEndText: " + StringList();
        }
    }

    #endregion

    internal class OrnamentText : Text
    {
        public OrnamentText(object container, string text, float ornamentFontHeight)
            : base(container, text, "Open Sans Condensed", ornamentFontHeight, TextHorizAlign.center)
        {
        }

        public override string ToString()
        {
            return "ornament: " + TextInfo.Text;
        }
    }

    internal class LyricText : Text
    {
        public LyricText(object container, string text, float chordFontHeight)
            : base(container, text, "Arial", (chordFontHeight / 2F), TextHorizAlign.center)
        {
        }

        public override string ToString()
        {
            return "lyric: " + TextInfo.Text;
        }
    }

    internal class DynamicText : Text
    {
        public DynamicText(object container, string text, float chordFontHeight)
            : base(container, text, "CLicht", chordFontHeight * 0.75F, TextHorizAlign.left)
        {
        }

        public override string ToString()
        {
            return "dynamic: " + TextInfo.Text;
        }
    }
}
