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
        public override void WriteSVG(SvgWriter w, int systemNumber, int staffNumber)
        {
            w.SvgStartGroup("outputStaff", "sys" + systemNumber.ToString() + "staff" + staffNumber.ToString());

            base.WriteSVG(w, true, systemNumber, staffNumber);

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
        public override void WriteSVG(SvgWriter w, int systemNumber, int staffNumber)
        {
            w.SvgStartGroup("outputStaff", "sys" + systemNumber.ToString() + "staff" + staffNumber.ToString());
            w.WriteAttributeString("score", "invisible", null, "1");

			int voiceNumber = 1;
            foreach(Voice voice in Voices)
            {
                voice.WriteSVG(w, false, systemNumber, staffNumber, voiceNumber++);
            }

            w.SvgEndGroup(); // outputStaff
        }
    }
}
