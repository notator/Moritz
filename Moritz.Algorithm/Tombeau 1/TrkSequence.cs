using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
    internal class TrkSequence
    {
        protected TrkSequence()
        {
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

        protected static List<Trk> GetTrksInChannel(List<Seq> seqs, int channel)
        {
            List<Trk> trks = new List<Trk>();
            foreach(Seq seq in seqs)
            {
                trks.Add(seq.Trks[channel]);
            }
            return trks;
        }

        protected List<List<Grp>> GrpLists = null;
        protected List<Trk> Trks = null;
    }
}