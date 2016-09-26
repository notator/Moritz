using System;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
    internal class SopranoTrks : TrkSequence
    {
        public SopranoTrks(List<Seq> seqs, IReadOnlyList<SopranoTemplate> sopranoTemplates, int channel)
            : base()
        {
            List<Trk> tenorTrks = GetTenorTrks(seqs);
            Trks = GetSopranoTrks(tenorTrks, sopranoTemplates, channel);
        }

        private List<Trk> GetTenorTrks(List<Seq> seqs)
        {
            int tenorChannel = 2;
            List<Trk> tenorTrks = new List<Trk>();
            foreach(Seq seq in seqs)
            {
                tenorTrks.Add(seq.Trks[tenorChannel]);
            }
            return tenorTrks;
        }

        private IReadOnlyList<Trk> GetSopranoTrks(List<Trk> trk0s, IReadOnlyList<SopranoTemplate> sopranoTemplates, int channel)
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