using System;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
    internal class Trks1 : ITrkArray
    {
        public Trks1(List<Seq> seqs, IReadOnlyList<Tombeau1Template> tombeau1Templates, IReadOnlyList<IReadOnlyList<byte>> ornamentShapes, int channel)
        {
            List<Trk> trk0s = GetTrk0s(seqs);
            Trks = GetTrk1s(trk0s, tombeau1Templates, ornamentShapes, channel);
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

        public Trk this[int index]    // Indexer declaration
        {
            get
            {
                Debug.Assert(index < Trks.Count); 
                return Trks[index];
            }
        }

        public int Count()
        {
            return Trks.Count;
        }

        private IReadOnlyList<Trk> GetTrk1s(List<Trk> trk0s, IReadOnlyList<Tombeau1Template> tombeau1Templates, IReadOnlyList<IReadOnlyList<byte>> ornamentShapes, int channel)
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
                List<byte> velocityPerAbsolutePitch = ((MidiChordDef)trk1[0]).Gamut.GetVelocityPerAbsolutePitch(25, true);
                trk1.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch, transformationPercents[i]);

                returnTrks.Add(trk1);

                //seqs[i].Trks[channel].AddRange(trk1);
            }

            ((MidiChordDef)returnTrks[0][0]).PanMsbs = new List<byte>() { 0 };

            return returnTrks;
        }

        private IReadOnlyList<Trk> Trks { get; }
    }
}