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
        /// <summary>
        /// Sets up standard templates that will be used in the composition.
        /// </summary>
        private IReadOnlyList<SopranoTemplate> GetSopranoTemplates(List<Gamut> gamuts)
        {
            List<SopranoTemplate> sopranoTemplates = new List<SopranoTemplate>();

            for(int i = 0; i < gamuts.Count; ++i)
            {
                Gamut gamut = gamuts[i];
                sopranoTemplates.Add(new SopranoTemplate(gamut));
            }
            return sopranoTemplates;
        }
        /// <summary>
        /// Sets up standard templates that will be used in the composition.
        /// </summary>
        private IReadOnlyList<AltoTemplate> GetAltoTemplates(List<Gamut> gamuts)
        {
            List<AltoTemplate> altoTemplates = new List<AltoTemplate>();

            for(int i = 0; i < gamuts.Count; ++i)
            {
                Gamut gamut = gamuts[i];
                altoTemplates.Add(new AltoTemplate(gamut));
            }
            return altoTemplates;
        }
        /// <summary>
        /// Sets up standard templates that will be used in the composition.
        /// </summary>
        private IReadOnlyList<TenorTemplate> GetTenorTemplates(List<Gamut> gamuts)
        {
            List<TenorTemplate> tenorTemplates = new List<TenorTemplate>();

            for(int i = 0; i < gamuts.Count; ++i)
            {
                Gamut gamut = gamuts[i];
                tenorTemplates.Add(new TenorTemplate(gamut));
            }
            return tenorTemplates;
        }
        /// <summary>
        /// Sets up standard templates that will be used in the composition.
        /// </summary>
        private IReadOnlyList<BassTemplate> GetBassTemplates(List<Gamut> gamuts)
        {
            List<BassTemplate> bassTemplates = new List<BassTemplate>();

            for(int i = 0; i < gamuts.Count; ++i)
            {
                Gamut gamut = gamuts[i];
                bassTemplates.Add(new BassTemplate(gamut));
            }
            return bassTemplates;
        }
    }
}
