using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
    internal class TenorTrks : TrkSequence
    {
        public TenorTrks(IReadOnlyList<TenorTemplate> template1s, int midiChannel)
            : base()
        {
            Trks = GetSeqTrk0s(template1s, midiChannel);
        }

        private IReadOnlyList<Trk> GetSeqTrk0s(IReadOnlyList<TenorTemplate> tenorTemplates, int midiChannel)
        {
            int nSubTrks = 5;
            int msDuration = 13000;

            List<Trk> seqTrks = new List<Trk>();
            seqTrks.Add(GetSeqTrk0(tenorTemplates[0], nSubTrks, msDuration, midiChannel));
            seqTrks.Add(GetSeqTrk0(tenorTemplates[1], nSubTrks, msDuration, midiChannel));
            seqTrks.Add(GetSeqTrk0(tenorTemplates[2], nSubTrks, msDuration, midiChannel));
            seqTrks.Add(GetSeqTrk0(tenorTemplates[3], nSubTrks, msDuration, midiChannel));
            seqTrks.Add(GetSeqTrk0(tenorTemplates[4], nSubTrks, msDuration, midiChannel));
            seqTrks.Add(GetSeqTrk0(tenorTemplates[5], nSubTrks, msDuration, midiChannel));
            seqTrks.Add(GetSeqTrk0(tenorTemplates[6], nSubTrks, msDuration, midiChannel));
            seqTrks.Add(GetSeqTrk0(tenorTemplates[7], nSubTrks, msDuration, midiChannel));
            seqTrks.Add(GetSeqTrk0(tenorTemplates[8], nSubTrks, msDuration, midiChannel));
            seqTrks.Add(GetSeqTrk0(tenorTemplates[9], nSubTrks, msDuration, midiChannel));
            seqTrks.Add(GetSeqTrk0(tenorTemplates[10], nSubTrks, msDuration, midiChannel));
            seqTrks.Add(GetSeqTrk0(tenorTemplates[11], nSubTrks, msDuration, midiChannel));
            seqTrks.Add(GetSeqTrk0(tenorTemplates[12], nSubTrks, msDuration, midiChannel));
            seqTrks.Add(GetSeqTrk0(tenorTemplates[13], nSubTrks, msDuration, midiChannel));
            seqTrks.Add(GetSeqTrk0(tenorTemplates[14], nSubTrks, msDuration, midiChannel));
            seqTrks.Add(GetSeqTrk0(tenorTemplates[15], nSubTrks, msDuration, midiChannel));
            seqTrks.Add(GetSeqTrk0(tenorTemplates[16], nSubTrks, msDuration, midiChannel));
            seqTrks.Add(GetSeqTrk0(tenorTemplates[17], nSubTrks, msDuration, midiChannel));
            seqTrks.Add(GetSeqTrk0(tenorTemplates[18], nSubTrks, msDuration, midiChannel));
            seqTrks.Add(GetSeqTrk0(tenorTemplates[19], nSubTrks, msDuration, midiChannel));
            seqTrks.Add(GetSeqTrk0(tenorTemplates[20], nSubTrks, msDuration, midiChannel));
            seqTrks.Add(GetSeqTrk0(tenorTemplates[21], nSubTrks, msDuration, midiChannel));

            return seqTrks;
        }

        /// <summary>
        /// The returned Trk has channel 0, and is the concatenation of nSubtrks + 1 versions of the original template1.
        /// </summary>
        /// <param name="template1"></param>
        /// <param name="nSubTrks"></param>
        private Trk GetSeqTrk0(TenorTemplate template1, int nSubTrks, int msDuration, int midiChannel)
        {
            List<int> relativeTranspositions = new List<int>() { 2, 1, 2, 2, 2, 1 };
            Debug.Assert(nSubTrks <= relativeTranspositions.Count);

            List<Trk> trk0SubTrks = new List<Trk>();
            trk0SubTrks.Add(template1.Clone());

            Trk subT = trk0SubTrks[0];
            /*** available transformations ***/
            //subT.Add();
            //subT.AddRange();
            //subT.AdjustChordMsDurations();
            //subT.AdjustExpression();
            //subT.AdjustVelocities();
            //subT.AdjustVelocitiesHairpin();
            //subT.AlignObjectAtIndex();
            //subT.CreateAccel();
            //subT.FindIndexAtMsPositionReFirstIUD();
            //subT.Insert();
            //subT.InsertRange();
            //subT.Permute();
            //subT.Remove();
            //subT.RemoveAt();
            //subT.RemoveBetweenMsPositions();
            //subT.RemoveRange();
            //subT.RemoveScorePitchWheelCommands();
            //subT.Replace();
            //subT.SetDurationsFromPitches();
            //subT.SetPanGliss(0, subT.MsDuration, 0, 127);
            //subT.SetPitchWheelDeviation();
            //subT.SetPitchWheelSliders();
            //subT.SetVelocitiesFromDurations();
            //subT.SetVelocityPerAbsolutePitch();
            //subT.TimeWarp();
            //subT.Translate();
            //subT.Transpose();
            //subT.TransposeInGamut();

            /*********************************/
            for(int i = 0; i < nSubTrks; ++i)
            {
                Trk subTrk = subT.Clone();
                subTrk.TransposeInGamut(relativeTranspositions[i]);

                if((i % 2) == 0)
                {
                    subTrk.Permute(1, 7);
                }
                trk0SubTrks.Add(subTrk);
                subT = subTrk;
            }

            Trk trk0 = new Trk(0, 0, new List<IUniqueDef>(), template1.Gamut.Clone());
            foreach(Trk subTrk in trk0SubTrks)
            {
                MidiChordDef mcd = (MidiChordDef)subTrk.UniqueDefs[subTrk.UniqueDefs.Count - 1];
                mcd.BeamContinues = false;

                trk0.AddRange(subTrk);
            }

            trk0.MsDuration = msDuration;
            trk0.MidiChannel = midiChannel;

            return trk0;
        }
    }
}