using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;
using Moritz.Globals;

namespace Moritz.Algorithm.Tombeau1
{
    public class AltoTemplate : Trk
    {
        public AltoTemplate(Gamut gamut)
            : base(0, 0, new List<IUniqueDef>(), gamut)
        {
            int rootPitch = gamut.BasePitch + (5 * 12);
            int nPitchesPerChord = 5;
            int nOrnamentChords = 5;

            int msDuration = 1000; // dummy -- overridden by SetDurationsFromPitches() below
            int rootIndex = Gamut.IndexOf(rootPitch);
            Debug.Assert(rootIndex >= 0); 

            List<byte> ornamentShape = new List<byte>() { 0, 127, 0 };

            for(int i = 0; i < gamut.NPitchesPerOctave; ++i)
            {
                MidiChordDef mcd = new MidiChordDef(msDuration, Gamut, Gamut[rootIndex + i], nPitchesPerChord, null);
                if(i == 2)
                {
                    mcd.SetOrnament(this.Gamut, ornamentShape, nOrnamentChords);
                }
                _uniqueDefs.Add(mcd);
            }

            // 1000, 841, 707, 595 is (1000 / n( 2^(1 / 4) )  for n = 1..4
            // The actual durations are set such that MsDuration stays at 4000ms.
            SetDurationsFromPitches(1000, 595, true);
            SetVelocitiesFromDurations(75, 127);
        }
    }
}
