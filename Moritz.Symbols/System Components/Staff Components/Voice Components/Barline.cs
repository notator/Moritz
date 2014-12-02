using System.Drawing;

using Moritz.Globals;
using Moritz.Xml;

namespace Moritz.Symbols
{
    /// <summary>
    /// This barline is an "explicit" capella barline.
    /// </summary>
	public class Barline : AnchorageSymbol 
	{
        public Barline(Voice voice)
            : base(voice)
        {
        }
        public Barline(Voice voice, BarlineType barlineType)
            : base(voice)
        {
            BarlineType = barlineType;
        }

        /// <summary>
        /// This function only writes the staff name and barnumber to the SVG file (if they are present).
        /// The barline itself is drawn when the system (and staff edges) is complete.
        /// </summary>
        public override void WriteSVG(SvgWriter w, bool staffIsVisible)
        {
            BarlineMetrics barlineMetrics = Metrics as BarlineMetrics;
            if(barlineMetrics != null && staffIsVisible)
            {
                barlineMetrics.WriteSVG(w);
            }
        }

        /// <summary>
        /// Writes out an SVG Barline.
        /// May be called twice per staff.barline:
        ///     1. for the range between top and bottom stafflines (if Barline.Visible is true)
        ///     2. for the range between the staff's lower edge and the next staff's upper edge
        ///        (if the staff's lower neighbour is in the same group)
        /// </summary>
        /// <param name="w"></param>
        public void WriteSVG(SvgWriter w, float topY, float bottomY, float strokeWidth)
        {
            w.SvgStartGroup("barline", null);
            if(BarlineType == BarlineType.end)
            {
                w.SvgLine(null,
                    this.Metrics.OriginX - (strokeWidth * 3F), topY,
                    this.Metrics.OriginX - (strokeWidth * 3F), bottomY,
                    this.ColorString.String, strokeWidth, null, null);
                w.SvgLine(null,
                    this.Metrics.OriginX, topY,
                    this.Metrics.OriginX, bottomY,
                    this.ColorString.String, strokeWidth * 2F, null, null);
            }
            else
            {
                w.SvgLine(null, this.Metrics.OriginX, topY, this.Metrics.OriginX, bottomY, this.ColorString.String, strokeWidth, null, null);
            }
            w.SvgEndGroup();
        }

        internal Text SetFramedText(string text, string fontFace, float textFontHeight, float paddingX, float paddingY, float strokeWidth)
        {
            TextInfo textInfo = new TextInfo(text, fontFace, textFontHeight, TextHorizAlign.center);
            FrameInfo frameInfo = new FrameInfo(TextFrameType.rectangle, paddingX, paddingY, strokeWidth, new ColorString(Color.Black));
            Text framedText = new Text(this, textInfo, frameInfo);
            return framedText;
        }

        public override string ToString()
		{
			string type = M.GetEnumDescription(BarlineType);
			return "ExplicitBarline: " + type;
		}

		public BarlineType BarlineType = BarlineType.single; // capella default
        /// <summary>
        /// Default is true
        /// </summary>
        public bool Visible = true;
	}

    internal class RedBarline : Barline
    {
        public RedBarline(Voice voice)
            :base(voice)
        {
            ColorString = new ColorString(Color.Red);
        }
    }
}
