using System;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
    internal class Trks1 : TrkSequence
    {
        public Trks1(List<Seq> seqs, IReadOnlyList<Template1> template1s, int channel)
            : base()
        {
            List<Trk> trk0s = GetTrk0s(seqs);
            Trks = GetTrk1s(trk0s, template1s, channel);
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

        private IReadOnlyList<Trk> GetTrk1s(List<Trk> trk0s, IReadOnlyList<Template1> template1s, int channel)
        {
            int nTrks = trk0s.Count;

            // Trk0 msDurations are currently all 13000ms
            List<int> trk1MsDurations = new List<int>()
            { 11100, 11200, 11300, 11400, 11500, 11600, 11700, 11800, 11900, 12000, 12100, 12100,
              12000, 11900, 11800, 11700, 11600, 11500, 11400, 11300, 11200, 11100 };
            Debug.Assert(trk1MsDurations.Count == nTrks);
            List<int> transformationPercents = new List<int>()
            { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };
            Debug.Assert(transformationPercents.Count == nTrks);

            List<Trk> returnTrks = new List<Trk>();
            for(int i = 0; i < nTrks; ++i)
            {
                Debug.Assert(trk0s[i].UniqueDefs.Count > 0);

                Trk trk0 = trk0s[i];
                Trk trk1 = trk0.Clone();

                trk1.MidiChannel = channel;
                trk1.MsDuration = trk1MsDurations[i];
                trk1.TransposeInGamut(12);
                List<byte> velocityPerAbsolutePitch = trk0.Gamut.GetVelocityPerAbsolutePitch(25, true);
                trk1.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch, transformationPercents[i]);
                int indexToAlign = trk1.Count - 1;
                int msPositionReContainer = trk0.UniqueDefs[trk0.UniqueDefs.Count - 1].MsPositionReFirstUD;
                trk1.AlignObjectAtIndex(indexToAlign, msPositionReContainer);
                returnTrks.Add(trk1);
            }

            ((MidiChordDef)returnTrks[0][0]).PanMsbs = new List<byte>() { 0 };

            return returnTrks;
        }
    }
}