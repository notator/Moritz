using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
    internal abstract class TrkSequence
    {
        internal class TransformationParameters
        {
            internal Gamut gamut;

            internal int baseGrpRootPitch;
            internal int baseGrpNChords;
            internal int baseGrpNPitchesPerChord;
            internal int baseGrpMsDurationPerChord;

            internal int nGrpsPerPalette;
            internal int grpMsDuration;
            internal int permuteAxisNumber;
            internal int permuteContourNumber;
            internal List<int> transpositions;
            internal List<byte> velocityPerAbsolutePitch;
            internal double transformationPercent;
        }

        public Trk this[int index]
        {
            get
            {
                Debug.Assert(index < Trks.Count);
                return Trks[index];
            }
        }

        public int Count { get { return Trks.Count; } }

        /// <summary>
        /// Returns a new list of TransformationParameters in which only the gamut field has been set.
        /// The other fields must be set by derived classes.
        /// </summary>
        /// <param name="tombeau1Gamuts">The gamuts in use in Tombeau1</param>
        protected virtual List<TransformationParameters> GetTransformationParametersList(IReadOnlyList<Gamut> tombeau1Gamuts)
        {
            List<TransformationParameters> tpsList = new List<TransformationParameters>();

            for(int i = 0; i < tombeau1Gamuts.Count; ++i)
            {
                TransformationParameters tps = new TransformationParameters();
                tps.gamut = tombeau1Gamuts[i];
                
                tpsList.Add(tps);
            }
            return tpsList;
        }

        /// <summary>
        /// Each GrpList will be a palette of Grps having the same gamut.
        /// </summary>
        /// <param name="tenorTemplates"></param>
        /// <returns></returns>
        protected List<List<Grp>> GetGrpLists(List<TransformationParameters> tpsList)
        {
            List<List<Grp>> grpLists = new List<List<Grp>>();

            for(int i = 0; i < tpsList.Count; ++i)
            {
                List<Grp> grpList = GetGrpList(tpsList[i]);
                grpLists.Add(grpList);
            }

            return grpLists;
        }

        /// <summary>
        /// This function is used while composing Grp palettes.
        /// It simply copies them (the Grp lists) to the output Trks.
        /// </summary>
        protected List<Trk> GetGrpPalettes(List<List<Grp>> grpLists)
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

        /// <summary>
        /// Called by the above GetGrpLists() function.
        /// Creates a list of Grps having the same gamut.
        /// Note that SATB might each use a different, but related, gamut.
        /// For example, each having a different number of pitches per octave.
        /// </summary>
        protected abstract List<Grp> GetGrpList(TransformationParameters tps);

        /// <summary>
        /// returns a Grp that is the basis for a list of Grps having the same gamut.
        /// </summary>
        protected abstract Grp GetBaseGrp(TransformationParameters tps);

        /// <summary>
        /// This is where the composition is actually done.
        /// The final sequence of Grps, possibly separated by RestDefs
        /// </summary>
        protected abstract List<Trk> GetTombeau1SeqTrks(List<List<Grp>> grpLists);

        protected List<List<Grp>> GrpLists = null;
        protected List<Trk> Trks = null;
    }
}