using Moritz.Spec;
using Moritz.Xml;

using System.Collections.Generic;

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
        public override void WriteSVG(SvgWriter w, int systemNumber, int staffNumber)
        {
            w.SvgStartGroup(Metrics.CSSObjectClass.ToString()); // "inputStaff"

            base.WriteSVG(w, systemNumber, staffNumber);

            w.SvgEndGroup(); // InputStaff
        }
    }
}
