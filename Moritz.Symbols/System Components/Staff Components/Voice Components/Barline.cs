
using System;
using System.Collections.Generic;
using System.Diagnostics;

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

        /// <summary>
        /// This function only writes the staff name and barnumber to the SVG file (if they are present).
        /// The barline itself is drawn when the system (and staff edges) is complete.
        /// </summary>
        public void WriteStaffNameAndBarNumberSVG(SvgWriter w, bool staffIsVisible, bool isInput)
        {
            BarlineMetrics barlineMetrics = Metrics as BarlineMetrics;
            if(barlineMetrics != null && staffIsVisible)
            {
                barlineMetrics.WriteStaffNameAndBarNumberSVG(w, isInput);
            }
        }

        public override void WriteSVG(SvgWriter w, bool staffIsVisible)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes out an SVG Barline.
        /// May be called twice per staff.barline:
        ///     1. for the range between top and bottom stafflines (if Barline.Visible is true)
        ///     2. for the range between the staff's lower edge and the next staff's upper edge
        ///        (if the staff's lower neighbour is in the same group)
        /// </summary>
        /// <param name="w"></param>
        public virtual void WriteSVG(SvgWriter w, float topStafflineY, float bottomStafflineY, float stafflineStrokeWidth, bool isLastNoteObject)
        {
			float topY = topStafflineY;
			float bottomY = bottomStafflineY;
			if(isLastNoteObject)
			{
				float halfStafflineWidth = (stafflineStrokeWidth / 2);
				topY -= halfStafflineWidth;
				bottomY += halfStafflineWidth;
			}

            w.SvgLine(CSSClass.barline, this.Metrics.OriginX, topY, this.Metrics.OriginX, bottomY);
        }

        public override string ToString()
		{
			return "barline: ";
		}
        /// <summary>
        /// Default is true
        /// </summary>
        public bool IsVisible = true;
	}

    public class EndBarline : Barline
    {
        public EndBarline(Voice voice)
            : base(voice)
        {

        }

        /// <summary>
        /// Writes out an SVG endBarline.
        /// May be called twice per staff.barline:
        ///     1. for the range between top and bottom stafflines (if Barline.Visible is true)
        ///     2. for the range between the staff's lower edge and the next staff's upper edge
        ///        (if the staff's lower neighbour is in the same group)
        /// </summary>
        /// <param name="w"></param>
        public override void WriteSVG(SvgWriter w, float topStafflineY, float bottomStafflineY, float stafflineStrokeWidth, bool unused)
        {
            float halfStafflineThickness = (stafflineStrokeWidth / 2);

            float topY = topStafflineY - halfStafflineThickness;
            float bottomY = bottomStafflineY + halfStafflineThickness;

            float leftLineOriginX = ((EndBarlineMetrics) Metrics).LeftLine.OriginX;
            w.SvgStartGroup(CSSClass.endBarline.ToString());
            w.SvgLine(CSSClass.barline, leftLineOriginX, topY, leftLineOriginX, bottomY);

            float rightLineOriginX = ((EndBarlineMetrics) Metrics).RightLine.OriginX;
            w.SvgLine(CSSClass.thickBarline, rightLineOriginX, topY, rightLineOriginX, bottomY);
            w.SvgEndGroup();            
        }


        public override string ToString()
        {
            return "endBarline: ";
        }
    }
}
