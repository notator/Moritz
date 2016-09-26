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
        /// <summary>
        /// Sets up the standard templates that will be used in the composition.
        /// </summary>
        private void SetTemplates()
        {
            List<SopranoTemplate> sopranoTemplates = new List<SopranoTemplate>();
            List<AltoTemplate> altoTemplates = new List<AltoTemplate>();
            List<TenorTemplate> tenorTemplates = new List<TenorTemplate>();
            List<BassTemplate> bassTemplates = new List<BassTemplate>();

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
                Gamut gamut= new Gamut(rphi, rootPitchAbs, nPitchesPerOctave);

                //sopranoTemplates.Add(new SopranoTemplate(gamut, rootPitch));
                //altoTemplates.Add(new AltoTemplate(gamut, rootPitch));
                tenorTemplates.Add(new TenorTemplate(gamut, rootPitch -12));
                //bassTemplates.Add(new BassTemplate(gamut, rootPitch));
            }

            _sopranoTemplates = sopranoTemplates;
            _altoTemplates = altoTemplates;
            _tenorTemplates = tenorTemplates;
            _bassTemplates = bassTemplates;
        }

        private IReadOnlyList<SopranoTemplate> _sopranoTemplates;
        private IReadOnlyList<AltoTemplate> _altoTemplates;
        private IReadOnlyList<TenorTemplate> _tenorTemplates;
        private IReadOnlyList<BassTemplate> _bassTemplates;

    }
}
