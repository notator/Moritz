using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
    internal abstract class TrkSequence
    {
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
        /// Each GrpList will be a palette of Grps having the same gamut.
        /// </summary>
        /// <param name="tenorTemplates"></param>
        /// <returns></returns>
        protected List<List<Grp>> GetGrpLists()
        {
            List<List<Grp>> grpLists = new List<List<Grp>>();

            for(int i = 0; i < Gamut.RelativePitchHierarchiesCount; ++i)
            {
                List<Grp> grpList = GetGrpList(i);
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
        /// Called by the GetGrpLists() function.
        /// Creates a list of Grps having the same relativePitchHierarchyIndex.
        /// </summary>
        protected abstract List<Grp> GetGrpList(int relativePitchHierarchyIndex);

        /// <summary>
        /// This is where the composition is actually done.
        /// The final sequence of Grps, possibly separated by RestDefs
        /// </summary>
        protected abstract List<Trk> GetTombeau1SeqTrks(List<List<Grp>> grpLists);

        protected List<List<Grp>> GrpLists = null;
        protected List<Trk> Trks = null;
    }
}