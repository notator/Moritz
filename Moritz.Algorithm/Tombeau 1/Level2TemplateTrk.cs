using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;
using Moritz.Globals;

namespace Moritz.Algorithm.Tombeau1
{
    public class Level2TemplateTrk : Tombeau1Trk
    {
        /// <summary>
        /// Returns a new Trk that is the concatenation of (a clone of) the original level1TemplateTrk
        /// with nSubTrks Trks that are variations of the original level1TemplateTrk.
        /// The returned Level2TemplateTrk is the concatenation of nSubtrks + 1 versions of the original level1TemplateTrk (including the original).
        /// </summary>
        /// <param name="midiChannel"></param>
        /// <param name="level1TemplateTrk"></param>
        /// <param name="nSubTrks"></param>
        public Level2TemplateTrk(Level1TemplateTrk level1TemplateTrk, int nSubTrks, IReadOnlyList<byte> ornamentShape, int nOrnamentChords)
            :base()
        {
            List<int> relativeTranspositions = new List<int>() { 2, 1, 2, 2, 2, 1 };
            Debug.Assert(nSubTrks <= relativeTranspositions.Count);

            List<Level1TemplateTrk> level1Trks = new List<Level1TemplateTrk>();
            level1Trks.Add(level1TemplateTrk.Clone());

            //SetOrnament(trk.UniqueDefs[2] as MidiChordDef, _envelopeShapes[0], 7);
            SetOrnament(level1Trks[0].UniqueDefs[2] as MidiChordDef, ornamentShape, nOrnamentChords);

            Level1TemplateTrk currentTrk = level1Trks[0];
            for(int i = 0; i < nSubTrks; ++i)
            {
                Level1TemplateTrk subTrk = currentTrk.Clone();
                subTrk.TransposeInGamut(relativeTranspositions[i]);

                if((i % 2) == 0)
                {
                    subTrk.Permute(1, 7);
                }
                level1Trks.Add(subTrk);
                currentTrk = subTrk;
            }

            foreach(Level1TemplateTrk subTrk in level1Trks)
            {
                UniqueDefs.AddRange(subTrk.UniqueDefs);
            }

            SetDurationsFromPitches(1000, 2000, false);

            MidiChordDef lastTrk0MidiChordDef = (MidiChordDef)UniqueDefs[UniqueDefs.Count - 1];
            lastTrk0MidiChordDef.BeamContinues = false;
        }

        private void SetOrnament(MidiChordDef midiChordDef, IReadOnlyList<byte> ornamentShape, int nOrnamentChords)
        {
            int nPitchesPerOctave = midiChordDef.Gamut.NPitchesPerOctave;
            Envelope ornamentEnvelope = new Envelope(ornamentShape, 127, nPitchesPerOctave, nOrnamentChords);
            midiChordDef.SetOrnament(ornamentEnvelope);
        }

        /// <summary>
        /// TemplateTrks are all constructed with MidiChannel=0 and msPositionReContainer=0.
        /// This constructor is used by Level2TemplateTrk.Clone()
        /// </summary>
        private Level2TemplateTrk(int midiChannel, int msPositionReContainer, List<IUniqueDef> iuds)
            : base(midiChannel, msPositionReContainer, iuds)
        {
        }

        /// <summary>
        /// Returns a deep clone of this Level1TemplateTrk.
        /// </summary>
        public new Level2TemplateTrk Clone()
        {
            List<IUniqueDef> clonedIUDs = GetUniqueDefsClone();
            Level2TemplateTrk clone = new Level2TemplateTrk(MidiChannel, MsPositionReContainer, clonedIUDs);
            clone.Container = this.Container;

            return clone;
        }
    }
}
