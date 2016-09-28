using System;
using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;

using Moritz.Globals;
using Moritz.Palettes;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
	public partial class Tombeau1Algorithm : CompositionAlgorithm
	{
        private List<Gamut> GetGamuts()
        {
            List<Gamut> gamuts = new List<Gamut>();
            // The standard relative pitch heirarchies are in a circular matrix having 22 values.
            int nGamuts = 22;
            for(int i = 0; i < nGamuts; ++i)
            {
                int rphi = i;
                int basePitch = 0;
                int nPitchesPerOctave = 8;

                Gamut gamut = new Gamut(rphi, basePitch, nPitchesPerOctave);
                gamuts.Add(gamut);
            }

            return gamuts;
        }
    }
}
