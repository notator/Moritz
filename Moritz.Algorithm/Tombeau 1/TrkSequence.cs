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
            internal int nChordsPerGrp;
            internal int nGrpsPerGamut;
            internal int grpMsDuration;
            internal int permuteAxisNumber;
            internal int permuteContourNumber;
            internal List<int> transpositions;
            internal List<byte> velocityPerAbsolutePitch;
            internal double transformationPercent;
        }

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

        protected abstract List<Grp> GetTrkGrps(Grp grp, TransformationParameters tps);

        protected List<List<Grp>> GrpLists = null;
        protected List<Trk> Trks = null;
    }
}