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

        public List<List<Grp>> Palette = null;

        /// <summary>
        /// This function is used while composing palettes.
        /// It simply copies them (the Grp lists) to the output Trks.
        /// </summary>
        protected List<Trk> PaletteToTrks(List<List<Grp>> palette)
        {
            List<Trk> seqTrks = new List<Trk>();
            foreach(List<Grp> grps in palette)
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
        /// This is where the composition is actually done.
        /// Returns the final sequence of Grps, possibly separated by RestDefs
        /// </summary>
        protected abstract List<Trk> GetTombeau1SeqTrks(List<List<Grp>> palette);
        protected List<Trk> Trks = null;
    }
}