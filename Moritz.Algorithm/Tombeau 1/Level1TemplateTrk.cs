using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;
using Moritz.Globals;

namespace Moritz.Algorithm.Tombeau1
{
    public class Tombeau1Trk : Trk
    {
        protected Tombeau1Trk()
            : base(0, 0, new List<IUniqueDef>())
        {
        }

        protected Tombeau1Trk(int midiChannel, int msPositionReContainer, List<IUniqueDef> iuds)
            : base(midiChannel, msPositionReContainer, iuds)
            {

            }
    }

    public class Level1TemplateTrk : Tombeau1Trk
    {
        /// <summary>
        /// TemplateTrks are all constructed with MidiChannel=0 and msPositionReContainer=0.
        /// </summary>
        public Level1TemplateTrk(int relativePitchHierarchyIndex, int rootPitch, int nPitchesPerOctave)
            : base()
        {
            List<int> absolutePitchHierarchy = M.GetAbsolutePitchHierarchy(relativePitchHierarchyIndex, rootPitch);
            Gamut gamut = new Gamut(absolutePitchHierarchy, nPitchesPerOctave);

            Debug.Assert(_uniqueDefs != null && _uniqueDefs.Count == 0);

            int rootNotatedPitch = gamut[gamut.Count / 2];
            int nPitchesPerChord = 1;

            List<int> durations4 = new List<int>() { 1000, 841, 707, 595 }; // (1000 / n( 2^(1 / 4) )  for n = 1..4
            int msDuration = durations4[3];
            MidiChordDef mcd1 = new MidiChordDef(msDuration, gamut, rootNotatedPitch, nPitchesPerChord + 1, null);
            _uniqueDefs.Add(mcd1);

            msDuration = durations4[1];
            MidiChordDef mcd2 = new MidiChordDef(msDuration, gamut, rootNotatedPitch, nPitchesPerChord + 2, null);
            mcd2.TransposeInGamut(1);
            _uniqueDefs.Add(mcd2);

            msDuration = durations4[0];
            MidiChordDef mcd3 = new MidiChordDef(msDuration, gamut, rootNotatedPitch, nPitchesPerChord + 3, null);
            mcd3.TransposeInGamut(2);
            _uniqueDefs.Add(mcd3);

            msDuration = durations4[2];
            MidiChordDef mcd4 = new MidiChordDef(msDuration, gamut, rootNotatedPitch, nPitchesPerChord + 4, null);
            mcd4.TransposeInGamut(3);
            _uniqueDefs.Add(mcd4);
        }

        /// <summary>
        /// TemplateTrks are all constructed with MidiChannel=0 and msPositionReContainer=0.
        /// This constructor is used by Level1TemplateTrk.Clone()
        /// </summary>
        private Level1TemplateTrk(int midiChannel, int msPositionReContainer, List<IUniqueDef> iuds)
            : base(midiChannel, msPositionReContainer, iuds)
        {
        }

        /// <summary>
        /// Returns a deep clone of this Level1TemplateTrk.
        /// </summary>
        public new Level1TemplateTrk Clone()
        {
            List<IUniqueDef> clonedIUDs = GetUniqueDefsClone();
            Level1TemplateTrk clone = new Level1TemplateTrk(MidiChannel, MsPositionReContainer, clonedIUDs);
            clone.Container = this.Container;

            return clone;
        }
    }
}
