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
            List<Template1> template1s = new List<Template1>();
            //List<Template2> template2s = new List<Template2>();

            // The standard relative pitch heirarchies are in a circular matrix having 22 values.
            // These pitch hierarchies are in order of distance from the root heirarchy.
            List<int> relativePitchHierarchyIndices = new List<int>()
            { 0, 21, 1, 20, 2, 19, 3, 18, 4, 17, 5, 16, 6, 15, 7, 14, 8, 13, 9, 12, 10, 11 };

            int rootPitch = 60;
            int nPitchesPerOctave = 8;
            foreach(int rphi in relativePitchHierarchyIndices)
            {
                int rootPitchAbs = rootPitch % 12;
                Gamut gamut= new Gamut(rphi, rootPitchAbs, nPitchesPerOctave);

                template1s.Add(new Template1(gamut, rootPitch)); 
                //template2s.Add(new Template2(gamut, rootPitch));
            }

            _template1s = template1s;
            //_template2s = template2s;

        }

        private IReadOnlyList<Template1> _template1s;
        //private IReadOnlyList<Template2> _template2s;
    }
}
