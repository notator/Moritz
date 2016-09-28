using System;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
    internal class BassTrks : TrkSequence
    {
        public BassTrks(List<Seq> seqs, IReadOnlyList<Gamut> gamuts)
            : base()
        {
            List<Trk> tenorTrks = GetTrksInChannel(seqs, 2);
            List<Trk> sopranoTrks = GetTrksInChannel(seqs, 0);
            IReadOnlyList<BassTemplate> bassTemplates = GetBassTemplates(gamuts);

            Trks = GetBassTrks(tenorTrks, sopranoTrks, bassTemplates);
        }

        /// <summary>
        /// Sets up standard templates that will be used in the composition.
        /// </summary>
        private IReadOnlyList<BassTemplate> GetBassTemplates(IReadOnlyList<Gamut> gamuts)
        {
            List<BassTemplate> bassTemplates = new List<BassTemplate>();

            for(int i = 0; i < gamuts.Count; ++i)
            {
                Gamut gamut = gamuts[i];
                bassTemplates.Add(new BassTemplate(gamut));
            }
            return bassTemplates;
        }

        private IReadOnlyList<Trk> GetBassTrks(List<Trk> tenorTrks, List<Trk> sopranoTrks, IReadOnlyList<BassTemplate> bassTemplates)
        {
            List<Trk> returnTrks = new List<Trk>();

            //Debug.Assert(trk0s.Count == bassTemplates.Count);

            //int nTrks = bassTemplates.Count;


            //for(int i = 0; i < nTrks; ++i)
            //{
            //    Debug.Assert(trk0s[i].UniqueDefs.Count > 0);

            //    Trk trk0 = trk0s[i];
            //    Trk trkA = bassTemplates[0].Clone();
            //    Trk trkB = bassTemplates[i].Clone();

            //    trkB.MsPositionReContainer = 6000;

            //    trkA.MsDuration = 3000;
            //    trkB.MsDuration = 3000;

            //    int indexToAlign = trkA.Count - 1;
            //    int msPositionReContainer = trk0.UniqueDefs[15].MsPositionReFirstUD;
            //    trkA.AlignObjectAtIndex(indexToAlign, msPositionReContainer);

            //    indexToAlign = trkB.Count - 1;
            //    msPositionReContainer = trk0.UniqueDefs[31].MsPositionReFirstUD;
            //    trkB.AlignObjectAtIndex(indexToAlign, msPositionReContainer);

            //    byte newPitch = 47;
            //    MidiChordDef mcd = new MidiChordDef(new List<byte>() { newPitch }, new List<byte>() { 127 }, 500, true);
            //    Trk trkB1 = new Trk(trkB.MidiChannel, 0, new List<IUniqueDef>() { mcd });

            //    trkB1.MsPositionReContainer = trkB.MsPositionReContainer + trkB.MsDuration + 500;

            //    trkB.Superimpose(trkB1);

            //    MidiChordDef trkmcd = trkB.ToMidiChordDef(2000);

            //    Gamut gamut = (trk0.Gamut.Contains(newPitch)) ? trk0.Gamut : null;

            //    Trk trkC = trkmcd.ToTrk(3000, trkB.MidiChannel, gamut);

            //    trkB.Insert(trkB.Count, trkmcd);
            //    trkB.AddRange(trkC);

            //    Trk trk1 = trkA.Superimpose(trkB);

            //    trk1.MsDuration = trk0.MsDuration;
            //    trk1.MsPositionReContainer = 0;

            //    returnTrks.Add(trk1);
            //}

            return returnTrks;
        }
    }
}