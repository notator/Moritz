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
            w.SvgStartGroup("outputStaff");
            w.WriteAttributeString("score", "hasMidi", null, "hasMidi");

            base.WriteSVG(w, true, systemNumber, staffNumber, carryMsgsPerChannel);

            w.SvgEndGroup(); // outputStaff
        }
    }

    /// <summary>
    /// An HiddenOutputStaff is invisible in all systems, regardless of its content.
    /// Such output staves are defined using PageFormat.VisibleOutputVoiceIndicesPerStaff.
    /// <para>Note also that individual staves are only printed if they contain at least one ChordSymbol.
    /// (i.e. An InputStaff or OutputStaff will be invisible if its IsEmpty property is true.)</para>
    /// </summary>
    public class HiddenOutputStaff : OutputStaff
    {
        public HiddenOutputStaff(SvgSystem svgSystem)
            : base(svgSystem, "", 1, 1, 1) // These default values will never be used.
        {
        }

        /// <summary>
        /// Writes out the (invisible) voices
        /// </summary>
        public override void WriteSVG(SvgWriter w, int systemNumber, int staffNumber, List<CarryMsgs> carryMsgsPerChannel)
        {
            w.SvgStartGroup("outputStaff");
            w.WriteAttributeString("score", "invisible", null, "1");

            base.WriteSVG(w, false, systemNumber, staffNumber, carryMsgsPerChannel);

            w.SvgEndGroup(); // outputStaff
        }
    }
}
