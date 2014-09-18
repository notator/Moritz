using System.Collections.Generic;
using System.Diagnostics;

namespace Moritz.Score
{
    public class InputStaff : Staff
    {
        public InputStaff(SvgSystem svgSystem, string staffName, int numberOfStafflines, float gap, float stafflineStemStrokeWidth)
            : base(svgSystem, staffName, numberOfStafflines, gap, stafflineStemStrokeWidth)
        {
        }

        /// <summary>
        /// Writes out the stafflines, noteObjects, and possibly the performanceOptions for an InputStaff.
        /// </summary>
        public override void WriteSVG(SvgWriter w, int pageNumber, int systemNumber, int staffNumber)
        {
            w.SvgStartGroup("inputStaff", "p" + pageNumber.ToString() + "_sys" + systemNumber.ToString() + "_staff" + staffNumber.ToString());

            base.WriteSVG(w);

            if(PerformanceOptions != null)
            {
                PerformanceOptions.WriteSVG(w);
            }

            w.SvgEndGroup(); // InputStaff
        }

        public PerformanceOptions PerformanceOptions = null;
    }
}
