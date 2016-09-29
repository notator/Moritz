using System;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
    internal class SopranoTrks : TrkSequence
    {
        public SopranoTrks(List<Seq> seqs, List<Gamut> gamuts)
            : base()
        {
            List<Trk> tenorTrks = GetTrksInChannel(seqs, 2);
            List<SopranoTemplate> sopranoTemplates = GetSopranoTemplates(gamuts);

            Trks = GetSopranoTrks(tenorTrks, sopranoTemplates);
        }

        /// <summary>
        /// Sets up standard templates that will be used in the composition.
        /// </summary>
        private List<SopranoTemplate> GetSopranoTemplates(List<Gamut> gamuts)
        {
            List<SopranoTemplate> sopranoTemplates = new List<SopranoTemplate>();

            for(int i = 0; i < gamuts.Count; ++i)
            {
                Gamut gamut = gamuts[i];
                sopranoTemplates.Add(new SopranoTemplate(gamut));
            }
            return sopranoTemplates;
        }

        private List<Trk> GetSopranoTrks(List<Trk> tenorTrks, List<SopranoTemplate> sopranoTemplates)
        {
            List<Trk> returnTrks = new List<Trk>();

            int nTrks = tenorTrks.Count;

            /*********** TODO ********************/

            //// Trk0 msDurations are currently all 13000ms
            //List<int> trk1MsDurations = new List<int>()
            //{ 11100, 11200, 11300, 11400, 11500, 11600, 11700, 11800, 11900, 12000, 12100, 12100,
            //  12000, 11900, 11800, 11700, 11600, 11500, 11400, 11300, 11200, 11100 };
            //Debug.Assert(trk1MsDurations.Count == nTrks);
            //List<int> transformationPercents = new List<int>()
            //{ 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };
            //Debug.Assert(transformationPercents.Count == nTrks);

            //for(int i = 0; i < nTrks; ++i)
            //{
            //    Debug.Assert(tenorTrks[i].UniqueDefs.Count > 0);

            //    Trk trk0 = tenorTrks[i];
            //    Trk trk1 = trk0.Clone();

            //    trk1.MsDuration = trk1MsDurations[i];
            //    trk1.TransposeInGamut(12);
            //    List<byte> velocityPerAbsolutePitch = trk0.Gamut.GetVelocityPerAbsolutePitch(25, true);
            //    trk1.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch, transformationPercents[i]);
            //    int indexToAlign = trk1.Count - 1;
            //    int msPositionReContainer = trk0.UniqueDefs[trk0.UniqueDefs.Count - 1].MsPositionReFirstUD;
            //    trk1.AlignObjectAtIndex(indexToAlign, msPositionReContainer);
            //    returnTrks.Add(trk1);
            //}

            //((MidiChordDef)returnTrks[0][0]).PanMsbs = new List<byte>() { 0 };

            return returnTrks;
        }
    }
}