using System.Collections.Generic;
using System.Diagnostics;

using Moritz.Xml;
using Moritz.Spec;

namespace Moritz.Symbols
{
    public class InputStaff : Staff
    {
        public InputStaff(SvgSystem svgSystem, string staffName, int numberOfStafflines, float gap, float stafflineStemStrokeWidth)
            : base(svgSystem, staffName, numberOfStafflines, gap, stafflineStemStrokeWidth)
        {
        }

        /// <summary>
        /// Writes out the stafflines, and noteObjects for an InputStaff.
        /// </summary>
        public override void WriteSVG(SvgWriter w, int systemNumber, int staffNumber, List<CarryMsgs> carryMsgsPerChannel, bool graphicsOnly)
        {
            w.SvgStartGroup(Metrics.CSSObjectClass.ToString()); // "inputStaff"

            base.WriteSVG(w, systemNumber, staffNumber, null, graphicsOnly); // carryMsgsPerChannel only for OutputStaff

            w.SvgEndGroup(); // InputStaff
        }
    }
}
