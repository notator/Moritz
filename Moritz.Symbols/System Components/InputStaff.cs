using System.Collections.Generic;
using System.Diagnostics;

using Moritz.Xml;

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
        public override void WriteSVG(SvgWriter w, int pageNumber, int systemNumber, int staffNumber)
        {
            w.SvgStartGroup("inputStaff", "p" + pageNumber.ToString() + "_sys" + systemNumber.ToString() + "_staff" + staffNumber.ToString());

            base.WriteSVG(w, true);

            w.SvgEndGroup(); // InputStaff
        }
    }
}
