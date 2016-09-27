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
            internal int nSubTrks;
            internal int msDuration;
            internal int permuteAxisNumber;
            internal int permuteContourNumber;
            internal List<int> relativeTranspositions;
            internal List<byte> velocityPerAbsolutePitch;
            internal double transformationPercent;
        }

        public TenorTrks(IReadOnlyList<TenorTemplate> tenorTemplates)
            : base()
        {
            List<TransformationParameters> transformationParametersList = GetTransformationParametersList(tenorTemplates);
            Trks = GetTenorSeqTrks(tenorTemplates, transformationParametersList);
        }

        private List<TransformationParameters> GetTransformationParametersList(IReadOnlyList<TenorTemplate> tenorTemplates)
        {
            List<TransformationParameters> rList = new List<Tombeau1.TenorTrks.TransformationParameters>();
            List<byte> velocityPerAbsolutePitch = tenorTemplates[0].Gamut.GetVelocityPerAbsolutePitch(25, true);

            for(int i = 0; i < tenorTemplates.Count; ++i)
            {
                TransformationParameters tps = new Tombeau1.TenorTrks.TransformationParameters();
                tps.nSubTrks = 5;
                //tps.msDuration = 13000 + (i * 500);
                tps.msDuration = 13000;
                tps.permuteAxisNumber = 1;
                tps.permuteContourNumber = 7;
                tps.relativeTranspositions = new List<int>() { 2, 1, 2, 2, 2, 1 };
                tps.velocityPerAbsolutePitch = velocityPerAbsolutePitch;
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
        private IReadOnlyList<Trk> GetTenorSeqTrks(IReadOnlyList<TenorTemplate> tenorTemplates, List<TransformationParameters> transformationParametersList)
        {
            List<Trk> seqTrks = new List<Trk>();

            for(int i = 0; i < tenorTemplates.Count; ++i)
            {
                Trk trk = GetTenorSeqTrk(tenorTemplates[i], transformationParametersList[i]);
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
            int msDuration = tps.msDuration;

            Debug.Assert(nSubTrks <= relativeTranspositions.Count);

            List<Trk> trk0SubTrks = new List<Trk>();
            trk0SubTrks.Add(tenorTemplate.Clone());

            Trk subT = trk0SubTrks[0];

            //subT.SetDurationsFromPitches(1000, 595, true, 100);
            subT.SetDurationsFromPitches(2000, 500, true, tps.transformationPercent);
            subT.SetVelocitiesFromDurations(90, 127, 100);
            subT.SetVelocityPerAbsolutePitch(tps.velocityPerAbsolutePitch, tps.transformationPercent);

            for(int i = 0; i < nSubTrks; ++i)
            {
                Trk subTrk = subT.Clone();
                subTrk.TransposeInGamut(relativeTranspositions[i]);

                if((i % 2) == 0)
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

            trk0.MsDuration = msDuration;

            return trk0;
        }

        /// <summary>
        /// The returned Trk has channel 0, and is the concatenation of nSubtrks + 1 versions of the original template1.
        /// </summary>
        /// <param name="template1"></param>
        /// <param name="nSubTrks"></param>
        private Trk GetTenorSeqTrk(TenorTemplate template1, int nSubTrks, int msDuration)
        {
            List<int> relativeTranspositions = new List<int>() { 2, 1, 2, 2, 2, 1 };
            Debug.Assert(nSubTrks <= relativeTranspositions.Count);

            List<Trk> trk0SubTrks = new List<Trk>();
            trk0SubTrks.Add(template1.Clone());

            Trk subT = trk0SubTrks[0];

            for(int i = 0; i < nSubTrks; ++i)
            {
                Trk subTrk = subT.Clone();
                subTrk.TransposeInGamut(relativeTranspositions[i]);

                if((i % 2) == 0)
                {
                    subTrk.Permute(1, 7);
                }
                trk0SubTrks.Add(subTrk);
                subT = subTrk;
            }

            // the Trk's midiChannel is set later.
            Trk trk0 = new Trk(0, 0, new List<IUniqueDef>(), template1.Gamut.Clone());
            foreach(Trk subTrk in trk0SubTrks)
            {
                MidiChordDef mcd = (MidiChordDef)subTrk.UniqueDefs[subTrk.UniqueDefs.Count - 1];
                mcd.BeamContinues = false;

                trk0.AddRange(subTrk);
            }

            trk0.MsDuration = msDuration;

            return trk0;
        }
    }
}