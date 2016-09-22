using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
    internal class Trks0 : TrkSequence
    {
        public Trks0(IReadOnlyList<Tombeau1Template> tombeau1Templates, IReadOnlyList<IReadOnlyList<byte>> ornamentShapes, int channel)
            : base()
        {
            Trks = GetSeqTrk0s(tombeau1Templates, ornamentShapes, channel);
        }

        private IReadOnlyList<Trk> GetSeqTrk0s(IReadOnlyList<Tombeau1Template> tombeau1Templates, IReadOnlyList<IReadOnlyList<byte>> ornamentShapes, int channel)
        {
            int nSubTrks = 5;
            int msDuration = 13000;
            int nChordsPerOrnament = 5;

            List<Trk> seqTrks = new List<Trk>();
            seqTrks.Add(GetSeqTrk0(tombeau1Templates[0], channel, nSubTrks, msDuration, ornamentShapes[0], nChordsPerOrnament));
            seqTrks.Add(GetSeqTrk0(tombeau1Templates[1], channel, nSubTrks, msDuration, ornamentShapes[1], nChordsPerOrnament));
            seqTrks.Add(GetSeqTrk0(tombeau1Templates[2], channel, nSubTrks, msDuration, ornamentShapes[2], nChordsPerOrnament));
            seqTrks.Add(GetSeqTrk0(tombeau1Templates[3], channel, nSubTrks, msDuration, ornamentShapes[3], nChordsPerOrnament));
            seqTrks.Add(GetSeqTrk0(tombeau1Templates[4], channel, nSubTrks, msDuration, ornamentShapes[4], nChordsPerOrnament));
            seqTrks.Add(GetSeqTrk0(tombeau1Templates[5], channel, nSubTrks, msDuration, ornamentShapes[5], nChordsPerOrnament));
            seqTrks.Add(GetSeqTrk0(tombeau1Templates[6], channel, nSubTrks, msDuration, ornamentShapes[6], nChordsPerOrnament));
            seqTrks.Add(GetSeqTrk0(tombeau1Templates[7], channel, nSubTrks, msDuration, ornamentShapes[7], nChordsPerOrnament));
            seqTrks.Add(GetSeqTrk0(tombeau1Templates[8], channel, nSubTrks, msDuration, ornamentShapes[8], nChordsPerOrnament));
            seqTrks.Add(GetSeqTrk0(tombeau1Templates[9], channel, nSubTrks, msDuration, ornamentShapes[9], nChordsPerOrnament));
            seqTrks.Add(GetSeqTrk0(tombeau1Templates[10], channel, nSubTrks, msDuration, ornamentShapes[10], nChordsPerOrnament));
            seqTrks.Add(GetSeqTrk0(tombeau1Templates[11], channel, nSubTrks, msDuration, ornamentShapes[10], nChordsPerOrnament));
            seqTrks.Add(GetSeqTrk0(tombeau1Templates[12], channel, nSubTrks, msDuration, ornamentShapes[9], nChordsPerOrnament));
            seqTrks.Add(GetSeqTrk0(tombeau1Templates[13], channel, nSubTrks, msDuration, ornamentShapes[8], nChordsPerOrnament));
            seqTrks.Add(GetSeqTrk0(tombeau1Templates[14], channel, nSubTrks, msDuration, ornamentShapes[7], nChordsPerOrnament));
            seqTrks.Add(GetSeqTrk0(tombeau1Templates[15], channel, nSubTrks, msDuration, ornamentShapes[6], nChordsPerOrnament));
            seqTrks.Add(GetSeqTrk0(tombeau1Templates[16], channel, nSubTrks, msDuration, ornamentShapes[5], nChordsPerOrnament));
            seqTrks.Add(GetSeqTrk0(tombeau1Templates[17], channel, nSubTrks, msDuration, ornamentShapes[4], nChordsPerOrnament));
            seqTrks.Add(GetSeqTrk0(tombeau1Templates[18], channel, nSubTrks, msDuration, ornamentShapes[3], nChordsPerOrnament));
            seqTrks.Add(GetSeqTrk0(tombeau1Templates[19], channel, nSubTrks, msDuration, ornamentShapes[2], nChordsPerOrnament));
            seqTrks.Add(GetSeqTrk0(tombeau1Templates[20], channel, nSubTrks, msDuration, ornamentShapes[1], nChordsPerOrnament));
            seqTrks.Add(GetSeqTrk0(tombeau1Templates[21], channel, nSubTrks, msDuration, ornamentShapes[0], nChordsPerOrnament));

            return seqTrks;
        }

        /// <summary>
        /// Returns a new Trk that is the concatenation of (a clone of) the original tombeau1Template
        /// with nSubTrks Trks that are variations of the original tombeau1Template.
        /// The returned Trk has channel 0, and is the concatenation of nSubtrks + 1 versions of the original tombeau1Template.
        /// </summary>
        /// <param name="tombeau1Template"></param>
        /// <param name="nSubTrks"></param>
        private Trk GetSeqTrk0(Tombeau1Template tombeau1Template, int channel, int nSubTrks, int msDuration, IReadOnlyList<byte> ornamentShape, int nOrnamentChords)
        {
            List<int> relativeTranspositions = new List<int>() { 2, 1, 2, 2, 2, 1 };
            Debug.Assert(nSubTrks <= relativeTranspositions.Count);

            List<Trk> trk0SubTrks = new List<Trk>();
            trk0SubTrks.Add(tombeau1Template.Clone());

            Trk currentTrk = trk0SubTrks[0];
            for(int i = 0; i < nSubTrks; ++i)
            {
                Trk subTrk = currentTrk.Clone();
                subTrk.TransposeInGamut(relativeTranspositions[i]);

                if((i % 2) == 0)
                {
                    subTrk.Permute(1, 7);
                }
                trk0SubTrks.Add(subTrk);
                currentTrk = subTrk;
            }

            Trk trk0 = new Trk(0, 0, new List<IUniqueDef>(), tombeau1Template.Gamut.Clone());
            foreach(Trk subTrk in trk0SubTrks)
            {
                MidiChordDef mcd = (MidiChordDef)subTrk.UniqueDefs[subTrk.UniqueDefs.Count - 1];
                mcd.BeamContinues = false;

                trk0.AddRange(subTrk);
            }

            trk0.MsDuration = msDuration;

            return trk0;
        }
    }
}