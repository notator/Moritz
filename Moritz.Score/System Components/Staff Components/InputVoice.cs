using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using Moritz.Score.Notation;

namespace Moritz.Score
{
    public class InputVoice : Voice
    {
        public InputVoice(InputStaff inputStaff)
            : base(inputStaff)
        {
        }

        /// <summary>
        /// Writes out the noteObjects, and possibly the performanceOptions for an InputVoice.
        /// </summary>
        public override void WriteSVG(SvgWriter w)
        {
            w.SvgStartGroup("inputVoice", null);

            base.WriteSVG(w);

            w.SvgEndGroup(); // inputVoice
        }

        /// <summary>
        /// returns true if this inputVoice contains inputChordDef, otherwise false.
        /// </summary>
        /// <param name="inputChordDef"></param>
        /// <returns></returns>
        internal bool Contains(InputChordDef inputChordDef)
        {
            bool contains = false;
            foreach(IUniqueDef iud in this.UniqueDefs)
            {
                InputChordDef localICD = iud as InputChordDef;
                if(localICD != null && localICD == inputChordDef)
                {
                    contains = true;
                    break;
                }
            }
            return contains;
        }
    }
}
