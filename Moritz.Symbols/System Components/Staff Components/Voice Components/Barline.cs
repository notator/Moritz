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
        public void WriteSVG(SvgWriter w, float topStafflineY, float bottomStafflineY, float singleBarlineStrokeWidth, float stafflineStrokeWidth, bool isLastNoteObject, bool isConnector)
        {
			float topY = topStafflineY;
			float bottomY = bottomStafflineY;
			if(isLastNoteObject)
			{
				float halfStrokeWidth = (stafflineStrokeWidth / 2);
				topY -= halfStrokeWidth;
				bottomY += halfStrokeWidth;
			}

			string barlineClass;
            if(BarlineType == BarlineType.end)
            {
				barlineClass = isConnector ? null : "endBarlineLeft";
				w.SvgLine(barlineClass,
					this.Metrics.OriginX - (singleBarlineStrokeWidth * 3F), topStafflineY,
					this.Metrics.OriginX - (singleBarlineStrokeWidth * 3F), bottomStafflineY,
                    "black", singleBarlineStrokeWidth, null);

				barlineClass = isConnector ? null : "endBarlineRight";
				w.SvgLine(barlineClass,
                    this.Metrics.OriginX, topY,
                    this.Metrics.OriginX, bottomY,
                    "black", singleBarlineStrokeWidth * 2F, null);
            }
            else
            {
				barlineClass = isConnector ? null : "barline";
				w.SvgLine(barlineClass, this.Metrics.OriginX, topY, this.Metrics.OriginX, bottomY, "black", singleBarlineStrokeWidth, null);
            }
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
			return "Barline: " + type;
		}

		public BarlineType BarlineType = BarlineType.single; // default
        /// <summary>
        /// Default is true
        /// </summary>
        public bool Visible = true;
	}
}
