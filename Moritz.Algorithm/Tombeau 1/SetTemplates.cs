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
            int rootPitch = 60;
            int nPitchesPerOctave = 8;
            int nChordsPerOrnament = 5;

            Debug.Assert(_ornamentShapes.Count > 10);

            // The standard relative pitch heirarchies are in a circular matrix having 22 values.
            // These pitch hierarchies are in order of distance from the root heirarchy.
            Tombeau1Template t1template0 = new Tombeau1Template(0, rootPitch, nPitchesPerOctave, _ornamentShapes[0], nChordsPerOrnament);
            Tombeau1Template t1template1 = new Tombeau1Template(21, rootPitch, nPitchesPerOctave, _ornamentShapes[1], nChordsPerOrnament);
            Tombeau1Template t1template2 = new Tombeau1Template(1, rootPitch, nPitchesPerOctave, _ornamentShapes[2], nChordsPerOrnament);
            Tombeau1Template t1template3 = new Tombeau1Template(20, rootPitch, nPitchesPerOctave, _ornamentShapes[3], nChordsPerOrnament);
            Tombeau1Template t1template4 = new Tombeau1Template(2, rootPitch, nPitchesPerOctave, _ornamentShapes[4], nChordsPerOrnament);
            Tombeau1Template t1template5 = new Tombeau1Template(19, rootPitch, nPitchesPerOctave, _ornamentShapes[5], nChordsPerOrnament);
            Tombeau1Template t1template6 = new Tombeau1Template(3, rootPitch, nPitchesPerOctave, _ornamentShapes[6], nChordsPerOrnament);
            Tombeau1Template t1template7 = new Tombeau1Template(18, rootPitch, nPitchesPerOctave, _ornamentShapes[7], nChordsPerOrnament);
            Tombeau1Template t1template8 = new Tombeau1Template(4, rootPitch, nPitchesPerOctave, _ornamentShapes[8], nChordsPerOrnament);
            Tombeau1Template t1template9 = new Tombeau1Template(17, rootPitch, nPitchesPerOctave, _ornamentShapes[9], nChordsPerOrnament);
            Tombeau1Template t1template10 = new Tombeau1Template(5, rootPitch, nPitchesPerOctave, _ornamentShapes[10], nChordsPerOrnament);
            Tombeau1Template t1template11 = new Tombeau1Template(16, rootPitch, nPitchesPerOctave, _ornamentShapes[10], nChordsPerOrnament);
            Tombeau1Template t1template12 = new Tombeau1Template(6, rootPitch, nPitchesPerOctave, _ornamentShapes[9], nChordsPerOrnament);
            Tombeau1Template t1template13 = new Tombeau1Template(15, rootPitch, nPitchesPerOctave, _ornamentShapes[8], nChordsPerOrnament);
            Tombeau1Template t1template14 = new Tombeau1Template(7, rootPitch, nPitchesPerOctave, _ornamentShapes[7], nChordsPerOrnament);
            Tombeau1Template t1template15 = new Tombeau1Template(14, rootPitch, nPitchesPerOctave, _ornamentShapes[6], nChordsPerOrnament);
            Tombeau1Template t1template16 = new Tombeau1Template(8, rootPitch, nPitchesPerOctave, _ornamentShapes[5], nChordsPerOrnament);
            Tombeau1Template t1template17 = new Tombeau1Template(13, rootPitch, nPitchesPerOctave, _ornamentShapes[4], nChordsPerOrnament);
            Tombeau1Template t1template18 = new Tombeau1Template(9, rootPitch, nPitchesPerOctave, _ornamentShapes[3], nChordsPerOrnament);
            Tombeau1Template t1template19 = new Tombeau1Template(12, rootPitch, nPitchesPerOctave, _ornamentShapes[2], nChordsPerOrnament);
            Tombeau1Template t1template20 = new Tombeau1Template(10, rootPitch, nPitchesPerOctave, _ornamentShapes[1], nChordsPerOrnament);
            Tombeau1Template t1template21 = new Tombeau1Template(11, rootPitch, nPitchesPerOctave, _ornamentShapes[0], nChordsPerOrnament);

            _templates = new List<Tombeau1Template>()
            { t1template0, t1template1, t1template2, t1template3, t1template4, t1template5, t1template6, t1template7, t1template8, t1template9,
              t1template10, t1template11, t1template12, t1template13, t1template14, t1template15, t1template16, t1template17, t1template18, t1template19,
              t1template20, t1template21 };
        }

        private IReadOnlyList<Tombeau1Template> _templates;
    }
}
