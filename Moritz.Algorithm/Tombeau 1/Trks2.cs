using System;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
    internal class Trks2 : TrkSequence
    {
        public Trks2(List<Seq> seqs, IReadOnlyList<Tombeau1Template> tombeau1Templates, IReadOnlyList<IReadOnlyList<byte>> ornamentShapes, int channel)
            : base()
        {
            List<Trk> trk0s = GetTrk0s(seqs);
            Trks = GetTrk2s(trk0s, tombeau1Templates, ornamentShapes, channel);
        }

        private List<Trk> GetTrk0s(List<Seq> seqs)
        {
            List<Trk> trk0s = new List<Trk>();
            foreach(Seq seq in seqs)
            {
                trk0s.Add(seq.Trks[0]);
            }
            return trk0s;
        }

        private IReadOnlyList<Trk> GetTrk2s(List<Trk> trk0s, IReadOnlyList<Tombeau1Template> tombeau1Templates, IReadOnlyList<IReadOnlyList<byte>> ornamentShapes, int channel)
        {
            Debug.Assert(trk0s.Count == tombeau1Templates.Count);

            int nTrks = tombeau1Templates.Count;

            List<Trk> returnTrks = new List<Trk>();
            for(int i = 0; i < nTrks; ++i)
            {
                Debug.Assert(trk0s[i].UniqueDefs.Count > 0);

                Trk trk0 = trk0s[i];
                Trk trkA = tombeau1Templates[0].Clone();
                Trk trkB = tombeau1Templates[i].Clone();

                trkB.MsPositionReContainer = 6000;

                trkA.MsDuration = 3000;
                trkB.MsDuration = 3000;

                int indexToAlign = trkA.Count - 1;
                int msPositionReContainer = trk0.UniqueDefs[15].MsPositionReFirstUD;
                trkA.AlignObjectAtIndex(indexToAlign, msPositionReContainer);

                indexToAlign = trkB.Count - 1;
                msPositionReContainer = trk0.UniqueDefs[31].MsPositionReFirstUD;
                trkB.AlignObjectAtIndex(indexToAlign, msPositionReContainer);

                MidiChordDef mcd = new MidiChordDef(new List<byte>() { 47 }, new List<byte>() { 127 }, 500, true);
                Trk trkB1 = new Trk(trkB.MidiChannel, 0, new List<IUniqueDef>() { mcd });

                trkB1.MsPositionReContainer = trkB.MsPositionReContainer + trkB.MsDuration + 500;

                trkB.Superimpose(trkB1);

                MidiChordDef trkmcd = trkB.ToMidiChordDef(2000);

                trkB.Insert(trkB.Count, trkmcd);

                Trk trk1 = trkA.Superimpose(trkB);

                trk1.MidiChannel = channel;

                returnTrks.Add(trk1);
            }

            return returnTrks;
        }
    }
}