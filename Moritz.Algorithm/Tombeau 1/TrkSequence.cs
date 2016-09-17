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

        protected IReadOnlyList<Trk> Trks = null;
    }
}