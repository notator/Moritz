using System.Collections.Generic;
using System.Diagnostics;

using Moritz.Xml;
using Moritz.Spec;

namespace Moritz.Symbols
{
    public class OutputStaff : Staff
    {
        public OutputStaff(SvgSystem svgSystem, string staffName, int numberOfStafflines, float gap, float stafflineStemStrokeWidth)
            : base(svgSystem, staffName, numberOfStafflines, gap, stafflineStemStrokeWidth)
        {
        }

        /// <summary>
        /// Writes out the stafflines and noteObjects of an OutputStaff.
        /// </summary>
        public override void WriteSVG(SvgWriter w, int systemNumber, int staffNumber, List<CarryMsgs> carryMsgsPerChannel)
        {
			w.SvgStartGroup(CSSObjectClass.staff.ToString()); // "staff"

			// if this.Metrics != null, the staff is invisible (but MIDI info is still written).
			if(this.Metrics == null)
			{
				w.WriteAttributeString("score", "invisible", null, "invisible");
			}

			base.WriteSVG(w, systemNumber, staffNumber, carryMsgsPerChannel);

            w.SvgEndGroup(); // outputStaff
        }
    }
}
