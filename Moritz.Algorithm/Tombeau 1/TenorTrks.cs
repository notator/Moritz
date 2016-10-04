using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
    internal class TenorTrks : TrkSequence
    {
        public TenorTrks(IReadOnlyList<Gamut> tombeau1Gamuts)
            : base()
        {
            List<TransformationParameters> transformationParametersList = GetTransformationParametersList(tombeau1Gamuts);
            
            Debug.Assert(transformationParametersList.Count == tombeau1Gamuts.Count);

            GrpLists = GetGrpLists(transformationParametersList);

            Debug.Assert(GrpLists.Count == tombeau1Gamuts.Count);

            bool displayGrpPalettes = true;
            if(displayGrpPalettes)
            {
                Trks = GetGrpPalettes(GrpLists);
            }
            else
            {
                Trks = GetTombeau1SeqTrks(GrpLists);
            }
        }

        protected override List<TransformationParameters> GetTransformationParametersList(IReadOnlyList<Gamut> tombeau1Gamuts)
        {
            // tps.gamut is the same for SATB, and is therefore set in the base class.
            List<TransformationParameters> tpsList = base.GetTransformationParametersList(tombeau1Gamuts);
            Debug.Assert(tpsList.Count == tombeau1Gamuts.Count);

            //List<byte> tombeau1vps = tombeau1Gamuts[0].GetVelocityPerAbsolutePitch(10, 127, 3, true);

            for(int i = 0; i < tpsList.Count; ++i)
            {
                TransformationParameters tps = tpsList[i];

                tps.baseGrpRootPitch = tps.gamut.BasePitch + (4 * 12);
                tps.baseGrpNChords = 8;
                tps.baseGrpNPitchesPerChord = 6;
                tps.baseGrpMsDurationPerChord = 1000;

                tps.nGrpsPerPalette = 6;
                tps.grpMsDuration = 2167;
                tps.permuteAxisNumber = 1;
                tps.permuteContourNumber = 7;
                tps.transpositions = new List<int>() { 0, 2, 3, 5, 7, 9, 10 };
                //tps.velocityPerAbsolutePitch = tombeau1vps;
                tps.velocityPerAbsolutePitch = tps.gamut.GetVelocityPerAbsolutePitch(10, 127, 3, true);
                //tps.transformationPercent = (i < 2) ? 0 : (i - 2) * 5;
                tps.transformationPercent = 100;
                Debug.Assert(tps.transformationPercent <= 100);
            }
            return tpsList;
        }

        #region available trk transformations
        // subT.Add();
        // subT.AddRange();
        // subT.AdjustChordMsDurations();
        // subT.AdjustExpression();
        // subT.AdjustVelocities();
        // subT.AdjustVelocitiesHairpin();
        // subT.AlignObjectAtIndex();
        // subT.CreateAccel();
        // subT.FindIndexAtMsPositionReFirstIUD();
        // subT.Insert();
        // subT.InsertRange();
        // subT.Permute();
        // subT.Remove();
        // subT.RemoveAt();
        // subT.RemoveBetweenMsPositions();
        // subT.RemoveRange();
        // subT.RemoveScorePitchWheelCommands();
        // subT.Replace();
        // subT.SetDurationsFromPitches();
        // subT.SetPanGliss(0, subT.MsDuration, 0, 127);
        // subT.SetPitchWheelDeviation();
        // subT.SetPitchWheelSliders();
        // subT.SetVelocitiesFromDurations();
        // subT.SetVelocityPerAbsolutePitch();
        // subT.TimeWarp();
        // subT.Translate();
        // subT.Transpose();
        // subT.TransposeInGamut();
        #endregion available trk transformations

        /// <summary>
        /// Called by the above GetGrpLists() function.
        /// Creates a list of Grps having the same gamut.
        /// Note that SATB might each use a different, but related, gamut.
        /// For example, each having a different number of pitches per octave.
        /// </summary>
        protected override List<Grp> GetGrpList(TransformationParameters tps)
        {
            // N.B. SATB might each use a different, but related, gamut.
            // For example, each having a different number of pitches per octave.
            Gamut gamut = tps.gamut;

            Grp baseGrp = GetBaseGrp(tps);

            List<int> transpositions = tps.transpositions;
            int nGrpsPerGamut = tps.nGrpsPerPalette;
            int permuteAxisNumber = tps.permuteAxisNumber;
            int permuteContourNumber = tps.permuteContourNumber;

            Debug.Assert(nGrpsPerGamut <= transpositions.Count);

            List<Grp> grps = new List<Grp>();

            for(int i = 0; i < nGrpsPerGamut; ++i)
            {
                Grp localGrp = baseGrp.Clone();
                localGrp.TransposeInGamut(transpositions[i]);

                //if((i % 2) == 1)
                //{
                //    localGrp.Permute(1, 7);
                //}
                localGrp.Permute(1, i + 1);

                grps.Add(localGrp);
            }
            return (grps);
        }

        protected override Grp GetBaseGrp(TransformationParameters tps)
        {
            Grp baseGrp = new Grp(tps.gamut, tps.baseGrpRootPitch, tps.baseGrpNPitchesPerChord, tps.baseGrpMsDurationPerChord, tps.baseGrpNChords);

            int nOrnamentChords = 3;
            List<byte> ornamentShape = new List<byte>() { 0, 127, 0 };
            for(int i = 0; i < baseGrp.UniqueDefs.Count; ++i)
            {
                MidiChordDef mcd = baseGrp.UniqueDefs[i] as MidiChordDef;
                if(mcd != null && i == 2)
                {
                    mcd.SetOrnament(baseGrp.Gamut, ornamentShape, nOrnamentChords);
                }
            }

            baseGrp.SetDurationsFromPitches(2000, 1000, true, 100);
            baseGrp.SetDurationsFromPitches(2000, 600, true, tps.transformationPercent);
            baseGrp.MsDuration = tps.grpMsDuration;
            baseGrp.SetVelocitiesFromDurations(65, 127, 100);
            baseGrp.SetVelocityPerAbsolutePitch(tps.velocityPerAbsolutePitch, 30, tps.transformationPercent);

            return baseGrp;
        }

        /// <summary>
        /// This is where the composition is actually done.
        /// The final sequence of Grps, possibly separated by RestDefs
        /// </summary>
        protected override List<Trk> GetTombeau1SeqTrks(List<List<Grp>> grpLists)
        {
            List<Trk> seqTrks = new List<Trk>();
            return seqTrks;
        }
    }       
}