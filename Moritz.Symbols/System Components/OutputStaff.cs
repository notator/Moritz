using System.Collections.Generic;
using System.Diagnostics;

using Moritz.Xml;

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
        public override void WriteSVG(SvgWriter w, int pageNumber, int systemNumber, int staffNumber)
        {
            w.SvgStartGroup("outputStaff", "p" + pageNumber.ToString() + "_sys" + systemNumber.ToString() + "_staff" + staffNumber.ToString());

            base.WriteSVG(w, true);

            w.SvgEndGroup(); // outputStaff
        }
    }

    public class InvisibleOutputStaff : OutputStaff
    {
        public InvisibleOutputStaff(SvgSystem svgSystem)
            : base(svgSystem, "", 1, 1, 1) // These default values will never be used.
        {
        }

        /// <summary>
        /// Writes out the (invisible) voices
        /// </summary>
        public override void WriteSVG(SvgWriter w, int pageNumber, int systemNumber, int staffNumber)
        {
            w.SvgStartGroup("outputStaff", "p" + pageNumber.ToString() + "_sys" + systemNumber.ToString() + "_staff" + staffNumber.ToString());
            w.WriteAttributeString("score", "invisible", null, "1");

            foreach(Voice voice in Voices)
            {
                voice.WriteSVG(w, false);
            }

            w.SvgEndGroup(); // outputStaff
        }
    }
}
