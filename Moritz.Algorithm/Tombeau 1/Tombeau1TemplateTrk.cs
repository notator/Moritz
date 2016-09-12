using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;
using Moritz.Globals;

namespace Moritz.Algorithm.Tombeau1
{
    public class Tombeau1TemplateTrk : Tombeau1Trk
    {
        /// <summary>
        /// Returns a new Trk that is the concatenation of (a clone of) the original level1TemplateTrk
        /// with nSubTrks Trks that are variations of the original level1TemplateTrk.
        /// The returned Level2TemplateTrk is the concatenation of nSubtrks + 1 versions of the original level1TemplateTrk (including the original).
        /// </summary>
        /// <param name="level1TemplateTrk"></param>
        /// <param name="nSubTrks"></param>
        public Tombeau1TemplateTrk(Level1TemplateTrk level1TemplateTrk, int nSubTrks, IReadOnlyList<byte> ornamentShape, int nOrnamentChords)
            : base(0, 0, 100, new List<IUniqueDef>())
        {
            List<int> relativeTranspositions = new List<int>() { 2, 1, 2, 2, 2, 1 };
            Debug.Assert(nSubTrks <= relativeTranspositions.Count);

            List<Level1TemplateTrk> level1Trks = new List<Level1TemplateTrk>();
            level1Trks.Add(level1TemplateTrk.Clone());

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
                MidiChordDef mcd = (MidiChordDef)subTrk.UniqueDefs[subTrk.UniqueDefs.Count - 1];
                mcd.BeamContinues = false;

                UniqueDefs.AddRange(subTrk.UniqueDefs);
            }
        }

        /// <summary>
        /// TemplateTrks are initialized with MidiChannel=0 and msPositionReContainer=0, transformationPercent = 100.
        /// This constructor is used by Level2TemplateTrk.Clone()
        /// </summary>
        private Tombeau1TemplateTrk(int midiChannel, int msPositionReContainer, double transformationPercent, List<IUniqueDef> iuds)
            : base(midiChannel, msPositionReContainer, transformationPercent, iuds)
        {
        }

        /// <summary>
        /// Returns a deep clone of this Level1TemplateTrk.
        /// </summary>
        public new Tombeau1TemplateTrk Clone()
        {
            List<IUniqueDef> clonedIUDs = GetUniqueDefsClone();
            Tombeau1TemplateTrk clone = new Tombeau1TemplateTrk(MidiChannel, MsPositionReContainer, TransformationPercent, clonedIUDs);
            clone.Container = this.Container;

            return clone;
        }
    }
}
