using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
    internal class TenorTrks : TrkSequence
    {
        internal class TransformationParameters
        {
            internal int nChordsPerSubTrk;
            internal int nSubTrks;
            internal int msDuration;
            internal int permuteAxisNumber;
            internal int permuteContourNumber;
            internal List<int> relativeTranspositions;
            internal List<byte> velocityPerAbsolutePitch;
            internal double transformationPercent;
        }

        public TenorTrks(IReadOnlyList<Gamut> gamuts)
            : base()
        {
            List<TransformationParameters> transformationParametersList = GetTransformationParametersList(gamuts);
            Trks = GetTenorSeqTrks(gamuts, transformationParametersList);
        }

        private List<TransformationParameters> GetTransformationParametersList(IReadOnlyList<Gamut> gamuts)
        {
            List<TransformationParameters> rList = new List<Tombeau1.TenorTrks.TransformationParameters>();
            List<int> nSubTrksPerTrk = new List<int>() { 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6 };
            List<int> nChordsPerSubTrk = new List<int>() { 8, 7, 6, 5, 4, 3, 4, 5, 6, 7, 8, 8, 7, 6, 5, 4, 3, 4, 5, 6, 7, 8 };
            List<int> msDurations = new List<int>() { 13000, 11800, 10600, 9400, 8200, 7000, 8200, 9400, 10600, 11800, 13000, 13000, 11800, 10600, 9400, 8200, 7000, 8200, 9400, 10600, 11800, 13000};

            for(int i = 0; i < gamuts.Count; ++i)
            {
                Gamut gamut = gamuts[i];
                TransformationParameters tps = new Tombeau1.TenorTrks.TransformationParameters();
                tps.nChordsPerSubTrk = nChordsPerSubTrk[i];
                tps.nSubTrks = nSubTrksPerTrk[i];
                tps.msDuration = msDurations[i];
                tps.permuteAxisNumber = 1;
                tps.permuteContourNumber = 7;
                tps.relativeTranspositions = new List<int>() { 0, 2, 1, 2, 2, 2, 1 };
                tps.velocityPerAbsolutePitch = gamut.GetVelocityPerAbsolutePitch(70, true);
                tps.transformationPercent = (i < 2) ? 0 : (i - 2) * 5;
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
        private IReadOnlyList<Trk> GetTenorSeqTrks(IReadOnlyList<Gamut> gamuts, List<TransformationParameters> transformationParametersList)
        {
            List<Trk> seqTrks = new List<Trk>();

            for(int i = 0; i < gamuts.Count; ++i)
            {
                TransformationParameters tps = transformationParametersList[i];
                TenorTemplate tenorTemplate = new TenorTemplate(gamuts[i], tps.nChordsPerSubTrk);
                
                Trk trk = GetTenorSeqTrk(tenorTemplate, transformationParametersList[i]);

                seqTrks.Add(trk);
            }

            return seqTrks;
        }

        private Trk GetTenorSeqTrk(TenorTemplate tenorTemplate, TransformationParameters tps)
        {
            //List<int> relativeTranspositions = new List<int>() { 2, 1, 2, 2, 2, 1 };
            //Debug.Assert(nSubTrks <= relativeTranspositions.Count);
            List<int> relativeTranspositions = tps.relativeTranspositions;
            int nSubTrks = tps.nSubTrks;
            int permuteAxisNumber = tps.permuteAxisNumber;
            int permuteContourNumber = tps.permuteContourNumber;

            Debug.Assert(nSubTrks <= relativeTranspositions.Count);

            List<Trk> trk0SubTrks = new List<Trk>();
            Trk subT = tenorTemplate;

            
            subT.SetDurationsFromPitches(2000, 1000, true, 100);
            subT.SetDurationsFromPitches(2000, 400, true, tps.transformationPercent);
            subT.SetVelocitiesFromDurations(65, 127, 100);
            subT.SetVelocityPerAbsolutePitch(tps.velocityPerAbsolutePitch, tps.transformationPercent);

            for(int i = 0; i < nSubTrks; ++i)
            {
                Trk subTrk = subT.Clone();
                subTrk.TransposeInGamut(relativeTranspositions[i]);

                if((i % 2) == 1)
                {
                    subTrk.Permute(1, 7);
                }

                trk0SubTrks.Add(subTrk);
                subT = subTrk;
            }

            // the Trk's midiChannel is set later.
            Trk trk0 = new Trk(0, 0, new List<IUniqueDef>(), tenorTemplate.Gamut.Clone());
            foreach(Trk subTrk in trk0SubTrks)
            {
                MidiChordDef mcd = (MidiChordDef)subTrk.UniqueDefs[subTrk.UniqueDefs.Count - 1];
                mcd.BeamContinues = false;

                trk0.AddRange(subTrk);
            }

            trk0.MsDuration = tps.msDuration;

            return trk0;
        }
    }
}