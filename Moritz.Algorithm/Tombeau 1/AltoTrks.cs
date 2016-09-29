using System;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
    internal class AltoTrks : TrkSequence
    {
        public AltoTrks(List<Seq> seqs, List<Gamut> gamuts)
            : base()
        {
            List<Trk> tenorTrks = GetTrksInChannel(seqs, 2);
            List<Trk> sopranoTrks = GetTrksInChannel(seqs, 0);
            List<Trk> bassTrks = GetTrksInChannel(seqs, 3);
            List<AltoTemplate> altoTemplates = GetAltoTemplates(gamuts);

            Trks = GetAltoTrks(tenorTrks, sopranoTrks, bassTrks, altoTemplates);
        }

        /// <summary>
        /// Sets up standard templates that will be used in the composition.
        /// </summary>
        private List<AltoTemplate> GetAltoTemplates(List<Gamut> gamuts)
        {
            List<AltoTemplate> altoTemplates = new List<AltoTemplate>();

            for(int i = 0; i < gamuts.Count; ++i)
            {
                Gamut gamut = gamuts[i];
                altoTemplates.Add(new AltoTemplate(gamut));
            }
            return altoTemplates;
        }

        private List<Trk> GetAltoTrks(List<Trk> tenorTrks, List<Trk> sopranoTrks, List<Trk> bassTrks, List<AltoTemplate> altoTemplates)
        {
            List<Trk> returnTrks = new List<Trk>();

            //Debug.Assert(tenorTrks.Count == altoTemplates.Count);

            //int nTrks = altoTemplates.Count;

            //for(int i = 0; i < nTrks; ++i)
            //{
            //    Debug.Assert(tenorTrks[i].UniqueDefs.Count > 0);

            //    Trk trk0 = tenorTrks[i];
            //    Trk trkA = altoTemplates[0].Clone();
            //    Trk trkB = altoTemplates[i].Clone();

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