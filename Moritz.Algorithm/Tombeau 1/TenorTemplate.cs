using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;
using Moritz.Globals;

namespace Moritz.Algorithm.Tombeau1
{
    public class TenorTemplate : Trk
    {
        public TenorTemplate(Gamut gamut, int nChords)
            : base(0, 0, new List<IUniqueDef>(), gamut)
        {
            int rootPitch = gamut.BasePitch + (4 * 12);
            int nPitchesPerChord = 5;
            int nOrnamentChords = 5;

            int msDuration = 1000;
            int rootIndex = Gamut.IndexOf(rootPitch);
            Debug.Assert(rootIndex >= 0); 

            List<byte> ornamentShape = new List<byte>() { 0, 127, 0 };

            for(int i = 0; i < nChords; ++i)
            {
                MidiChordDef mcd = new MidiChordDef(msDuration, Gamut, Gamut[rootIndex + i], nPitchesPerChord, null);
                if(i == 2)
                {
                    mcd.SetOrnament(this.Gamut, ornamentShape, nOrnamentChords);
                }
                _uniqueDefs.Add(mcd);
            }
        }
    }
}
