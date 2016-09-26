using System;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
    internal class Trks3 : TrkSequence
    {
        public Trks3(List<Seq> seqs, IReadOnlyList<Template1> template1s, int channel)
            : base()
        {
            List<Trk> trk0s = GetTrk0s(seqs);
            Trks = GetTrk3s(trk0s, template1s, channel);
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

        private IReadOnlyList<Trk> GetTrk3s(List<Trk> trk0s, IReadOnlyList<Template1> template1s, int channel)
        {
            Debug.Assert(trk0s.Count == template1s.Count);

            int nTrks = template1s.Count;

            List<Trk> returnTrks = new List<Trk>();
            for(int i = 0; i < nTrks; ++i)
            {
                Debug.Assert(trk0s[i].UniqueDefs.Count > 0);

                Trk trk0 = trk0s[i];
                Trk trkA = template1s[0].Clone();
                Trk trkB = template1s[i].Clone();

                trkB.MsPositionReContainer = 6000;

                trkA.MsDuration = 3000;
                trkB.MsDuration = 3000;

                int indexToAlign = trkA.Count - 1;
                int msPositionReContainer = trk0.UniqueDefs[15].MsPositionReFirstUD;
                trkA.AlignObjectAtIndex(indexToAlign, msPositionReContainer);

                indexToAlign = trkB.Count - 1;
                msPositionReContainer = trk0.UniqueDefs[31].MsPositionReFirstUD;
                trkB.AlignObjectAtIndex(indexToAlign, msPositionReContainer);

                byte newPitch = 47;
                MidiChordDef mcd = new MidiChordDef(new List<byte>() { newPitch }, new List<byte>() { 127 }, 500, true);
                Trk trkB1 = new Trk(trkB.MidiChannel, 0, new List<IUniqueDef>() { mcd });

                trkB1.MsPositionReContainer = trkB.MsPositionReContainer + trkB.MsDuration + 500;

                trkB.Superimpose(trkB1);

                MidiChordDef trkmcd = trkB.ToMidiChordDef(2000);

                Gamut gamut = (trk0.Gamut.Contains(newPitch)) ? trk0.Gamut : null;

                Trk trkC = trkmcd.ToTrk(3000, trkB.MidiChannel, gamut);

                trkB.Insert(trkB.Count, trkmcd);
                trkB.AddRange(trkC);


                Trk trk1 = trkA.Superimpose(trkB);

                trk1.MidiChannel = channel;
                trk1.MsDuration = trk0.MsDuration;
                trk1.MsPositionReContainer = 0;
                

                returnTrks.Add(trk1);
            }

            return returnTrks;
        }
    }
}