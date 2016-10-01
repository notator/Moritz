using System;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
    internal class AltoTrks : TrkSequence
    {
        public AltoTrks(SopranoTrks sopranoTrks, TenorTrks tenorTrks, BassTrks bassTrks, List<Gamut> gamuts)
            : base()
        {
            List<TransformationParameters> transformationParametersList = GetTransformationParametersList(gamuts);

            GrpLists = GetAllAltoGrps(gamuts, transformationParametersList);

            Trks = GetAltoTrks(sopranoTrks, tenorTrks, bassTrks, GrpLists);
        }

        private List<TransformationParameters> GetTransformationParametersList(IReadOnlyList<Gamut> gamuts)
        {
            List<TransformationParameters> rList = new List<TransformationParameters>();

            Gamut gamut = gamuts[0];
            for(int i = 0; i < gamuts.Count; ++i)
            {
                TransformationParameters tps = new TransformationParameters();
                // TODO -- customise for altoGrps
                tps.nChordsPerGrp = 8;
                tps.nGrpsPerGamut = 6;
                tps.grpMsDuration = 2167;
                tps.permuteAxisNumber = 1;
                tps.permuteContourNumber = 7;
                tps.transpositions = new List<int>() { 0, 2, 3, 5, 7, 9, 10 };
                tps.velocityPerAbsolutePitch = gamut.GetVelocityPerAbsolutePitch(30, true);
                //tps.transformationPercent = (i < 2) ? 0 : (i - 2) * 5;
                tps.transformationPercent = 100;
                Debug.Assert(tps.transformationPercent <= 100);

                rList.Add(tps);
            }
            return rList;
        }

        #region GetAllAltoGrps
        private List<List<Grp>> GetAllAltoGrps(IReadOnlyList<Gamut> gamuts, List<TransformationParameters> transformationParametersList)
        {
            List<List<Grp>> allAltoGrps = new List<List<Grp>>();

            for(int i = 0; i < gamuts.Count; ++i)
            {
                TransformationParameters tps = transformationParametersList[i];
                Gamut gamut = gamuts[i];
                Grp altoGrp = new Grp(gamut, gamut.BasePitch + (5 * 12), 6, 1000, tps.nChordsPerGrp);
                SetAltoGrp(altoGrp, transformationParametersList[i]);

                List<Grp> tenorGrps = GetTrkGrps(altoGrp, transformationParametersList[i]);

                allAltoGrps.Add(tenorGrps);
            }

            return allAltoGrps;
        }
        private void SetAltoGrp(Grp altoGrp, TransformationParameters tps)
        {
            int nOrnamentChords = 5;
            List<byte> ornamentShape = new List<byte>() { 0, 127, 0 };

            for(int i = 0; i < altoGrp.UniqueDefs.Count; ++i)
            {
                MidiChordDef mcd = altoGrp.UniqueDefs[i] as MidiChordDef;
                if(mcd != null && i == 2)
                {
                    mcd.SetOrnament(altoGrp.Gamut, ornamentShape, nOrnamentChords);
                }
            }

            altoGrp.SetDurationsFromPitches(2000, 1000, true, 100);
            altoGrp.SetDurationsFromPitches(2000, 600, true, tps.transformationPercent);
            altoGrp.MsDuration = tps.grpMsDuration;
            altoGrp.SetVelocitiesFromDurations(65, 127, 100);
            altoGrp.SetVelocityPerAbsolutePitch(tps.velocityPerAbsolutePitch, 0, tps.transformationPercent);
        }
        protected override List<Grp> GetTrkGrps(Grp grp, TransformationParameters tps)
        {
            List<int> transpositions = tps.transpositions;
            int nGrpsPerGamut = tps.nGrpsPerGamut;
            int permuteAxisNumber = tps.permuteAxisNumber;
            int permuteContourNumber = tps.permuteContourNumber;

            Debug.Assert(nGrpsPerGamut <= transpositions.Count);

            List<Grp> grps = new List<Grp>();

            for(int i = 0; i < nGrpsPerGamut; ++i)
            {
                Grp localGrp = grp.Clone();
                localGrp.TransposeInGamut(transpositions[i]);

                if((i % 2) == 1)
                {
                    localGrp.Permute(1, 7);
                }

                grps.Add(localGrp);
            }
            return (grps);
        }
        #endregion GetAllAltoGrps

        private List<Trk> GetAltoTrks(SopranoTrks sopranoTrks, TenorTrks tenorTrks, BassTrks bassTrks, List<List<Grp>> grpLists)
        {
            List<Trk> altoTrks = new List<Trk>();
            int nTrks = tenorTrks.Count;

            // N.B. sopranoTrks, tenorTrks and bassTrks can be indexed to return a Trk! :-))

            /*********** TODO customise for alto ********************/

            // Trk0 msDurations are currently all 13000ms
            List<int> trk1MsDurations = new List<int>()
            { 11100, 11200, 11300, 11400, 11500, 11600, 11700, 11800, 11900, 12000, 12100, 12100,
              12000, 11900, 11800, 11700, 11600, 11500, 11400, 11300, 11200, 11100 };
            Debug.Assert(trk1MsDurations.Count == nTrks);
            List<int> transformationPercents = new List<int>()
            { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };
            Debug.Assert(transformationPercents.Count == nTrks);

            for(int i = 0; i < nTrks; ++i)
            {
                Debug.Assert(tenorTrks[i].UniqueDefs.Count > 0);

                Trk trk0 = tenorTrks[i];
                Trk trk1 = trk0.Clone();

                trk1.MsDuration = trk1MsDurations[i];
                trk1.TransposeInGamut(12);
                List<byte> velocityPerAbsolutePitch = trk0.Gamut.GetVelocityPerAbsolutePitch(25, true);
                trk1.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch, 0, transformationPercents[i]);
                int indexToAlign = trk1.Count - 1;
                int msPositionReContainer = trk0.UniqueDefs[trk0.UniqueDefs.Count - 1].MsPositionReFirstUD;
                trk1.AlignObjectAtIndex(indexToAlign, msPositionReContainer);
                altoTrks.Add(trk1);
            }

            ((MidiChordDef)altoTrks[0][0]).PanMsbs = new List<byte>() { 0 };

            return altoTrks;
        }
    }
}