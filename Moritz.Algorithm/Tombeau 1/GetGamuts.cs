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
            int nTrkGamuts = 22;

            // These pitch hierarchies are in order of distance from the root heirarchy.
            List<int> relativePitchHierarchyIndices = new List<int>()
            { 0, 21, 1, 20, 2, 19, 3, 18, 4, 17, 5, 16, 6, 15, 7, 14, 8, 13, 9, 12, 10, 11 };
            List<int> rootPitches = new List<int>()
            {60, 67, 60, 67, 60, 67, 60, 67, 60, 67, 60, 67, 60, 67, 60, 67, 60, 67, 60, 67, 60, 67};
            List<int> pitchesPerOctave = new List<int>()
            { 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8 };

            Debug.Assert(nTrkGamuts == relativePitchHierarchyIndices.Count);
            Debug.Assert(nTrkGamuts == rootPitches.Count);
            Debug.Assert(nTrkGamuts == pitchesPerOctave.Count);

            for(int i = 0; i < nTrkGamuts; ++i)
            {
                int rphi = relativePitchHierarchyIndices[i];
                int rootPitch = rootPitches[i];
                int nPitchesPerOctave = pitchesPerOctave[i];

                int rootPitchAbs = rootPitch % 12;
                Gamut gamut = new Gamut(rphi, rootPitchAbs, nPitchesPerOctave);
                gamuts.Add(gamut);
            }

            return gamuts;
        }
    }
}
