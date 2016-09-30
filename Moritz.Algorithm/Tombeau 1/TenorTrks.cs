using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
    internal class TenorTrks : TrkSequence
    {
        public TenorTrks(IReadOnlyList<Gamut> gamuts)
            : base()
        {
            List<TransformationParameters> transformationParametersList = GetTransformationParametersList(gamuts);

            GrpLists = GetAllTenorGrps(gamuts, transformationParametersList);
            Trks = GetTenorSeqTrks(GrpLists);
        }

        private List<TransformationParameters> GetTransformationParametersList(IReadOnlyList<Gamut> gamuts)
        {
            List<TransformationParameters> rList = new List<Tombeau1.TenorTrks.TransformationParameters>();

            Gamut gamut = gamuts[0];
            for(int i = 0; i < gamuts.Count; ++i)
            {
                TransformationParameters tps = new Tombeau1.TenorTrks.TransformationParameters();
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

        /// <summary>
        /// /*** available trk transformations ***/
        /// subT.Add();
        /// subT.AddRange();
        /// subT.AdjustChordMsDurations();
        /// subT.AdjustExpression();
        /// subT.AdjustVelocities();
        /// subT.AdjustVelocitiesHairpin();
        /// subT.AlignObjectAtIndex();
        /// subT.CreateAccel();
        /// subT.FindIndexAtMsPositionReFirstIUD();
        /// subT.Insert();
        /// subT.InsertRange();
        /// subT.Permute();
        /// subT.Remove();
        /// subT.RemoveAt();
        /// subT.RemoveBetweenMsPositions();
        /// subT.RemoveRange();
        /// subT.RemoveScorePitchWheelCommands();
        /// subT.Replace();
        /// subT.SetDurationsFromPitches();
        /// subT.SetPanGliss(0, subT.MsDuration, 0, 127);
        /// subT.SetPitchWheelDeviation();
        /// subT.SetPitchWheelSliders();
        /// subT.SetVelocitiesFromDurations();
        /// subT.SetVelocityPerAbsolutePitch();
        /// subT.TimeWarp();
        /// subT.Translate();
        /// subT.Transpose();
        /// subT.TransposeInGamut();
        /// </summary>
        /// <param name="tenorTemplates"></param>
        /// <returns></returns>
        private List<List<Grp>> GetAllTenorGrps(IReadOnlyList<Gamut> gamuts, List<TransformationParameters> transformationParametersList)
        {
            List<List<Grp>> allTenorGrps = new List<List<Grp>>();

            for(int i = 0; i < gamuts.Count; ++i)
            {
                TransformationParameters tps = transformationParametersList[i];
                Gamut gamut = gamuts[i];
                Grp tenorGrp = new Grp(gamut, gamut.BasePitch + (4 * 12), 6, 1000, tps.nChordsPerGrp);
                SetTenorGrp(tenorGrp, transformationParametersList[i]);

                List<Grp> tenorGrps = GetTrkGrps(tenorGrp, transformationParametersList[i]);

                allTenorGrps.Add(tenorGrps);
            }

            return allTenorGrps;
        }

        private void SetTenorGrp(Grp tenorGrp, TransformationParameters tps)
        {
            int nOrnamentChords = 3;
            List<byte> ornamentShape = new List<byte>() { 0, 127, 0 };
            for(int i = 0; i < tenorGrp.UniqueDefs.Count; ++i)
            {
                MidiChordDef mcd = tenorGrp.UniqueDefs[i] as MidiChordDef;
                if(mcd != null && i == 2)
                {
                    mcd.SetOrnament(tenorGrp.Gamut, ornamentShape, nOrnamentChords);
                }
            }

            tenorGrp.SetDurationsFromPitches(2000, 1000, true, 100);
            tenorGrp.SetDurationsFromPitches(2000, 600, true, tps.transformationPercent);
            tenorGrp.MsDuration = tps.grpMsDuration;
            tenorGrp.SetVelocitiesFromDurations(65, 127, 100);
            tenorGrp.SetVelocityPerAbsolutePitch(tps.velocityPerAbsolutePitch, tps.transformationPercent);
        }

        private List<Trk> GetTenorSeqTrks(List<List<Grp>> grpLists)
        {
            List<Trk> seqTrks = new List<Trk>();
            foreach(List<Grp> grps in grpLists)
            {
                // the Trk's midiChannel is set later.
                Trk trk0 = new Trk(0, 0, new List<IUniqueDef>(), grps[0].Gamut.Clone());
                foreach(Grp grp in grps)
                {
                    trk0.AddRange(grp);
                }
                seqTrks.Add(trk0);
            }
            return seqTrks;
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
    }       
}