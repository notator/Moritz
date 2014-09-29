using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using Moritz.Xml;
using Moritz.VoiceDef;

namespace Moritz.Symbols
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

        public IEnumerable<InputChordDef> InputChordDefs
        {
            get
            {
                foreach(IUniqueDef iud in this.UniqueDefs)
                {
                    InputChordDef inputChordDef = iud as InputChordDef;
                    if(inputChordDef != null)
                        yield return inputChordDef;
                }
            }
        }

        /// <summary>
        /// returns true if this inputVoice contains inputChordDef, otherwise false.
        /// </summary>
        /// <param name="inputChordDef"></param>
        /// <returns></returns>
        internal bool Contains(InputChordDef inputChordDef)
        {
            bool contains = false;
            foreach(InputChordDef localChordDef in InputChordDefs)
            {
                if(localChordDef == inputChordDef)
                {
                    contains = true;
                    break;
                }
            }
            return contains;
        }
    }
}
