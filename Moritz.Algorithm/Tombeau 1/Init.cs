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
        private void Init(List<string> paletteNames)
        {
            List<Palette> palettes = new List<Palette>();
            foreach(string paletteName in paletteNames)
            {
                Palette palette = GetPaletteByName(paletteName);
                palettes.Add(palette);
            }

            SetPaletteMidiChordDefs(palettes);
            SetPitchWheelTestMidiChordDefs();
            SetOrnamentTestMidiChordDefs();

            SetTombeau1Templates();
            // maybe define other template types, and add them here.
        }

        #region Init() helper functions
        #region SetPaletteMidiChordDefs
        private void SetPaletteMidiChordDefs(List<Palette> paletteList)
        {
            List<List<MidiChordDef>> paletteMidiChordDefs = new List<List<MidiChordDef>>();
            foreach(Palette palette in paletteList)
            {
                paletteMidiChordDefs.Add(GetPaletteMidiChordDefs(palette));
            }

            _paletteMidiChordDefs = paletteMidiChordDefs;
        }
        private List<MidiChordDef> GetPaletteMidiChordDefs(Palette palette)
        {
            List<MidiChordDef> midiChordDefs = new List<MidiChordDef>();
            for(int i = 0; i < palette.Count; ++i)
            {
                IUniqueDef iud = palette.UniqueDurationDef(i);
                Debug.Assert(iud is MidiChordDef);
                midiChordDefs.Add(iud as MidiChordDef);
            }
            return midiChordDefs;
        }
        #endregion SetPaletteMidiChordDefs
        #region SetPitchWheelTestMidiChordDefs
        private void SetPitchWheelTestMidiChordDefs()
        {
            IReadOnlyList<IReadOnlyList<IReadOnlyList<byte>>> EnvelopeShapes = new List<IReadOnlyList<IReadOnlyList<byte>>>()
            {
                _sliderShapes2, _sliderShapes3, _sliderShapes4, _sliderShapes5, _sliderShapes6, _sliderShapes7, _sliderShapesLong
            };
            List<List<MidiChordDef>> pitchWheelTestMidiChordDefs = new List<List<MidiChordDef>>();
            foreach(IReadOnlyList<IReadOnlyList<byte>> envList in EnvelopeShapes)
            {
                List<MidiChordDef> pwmcds = GetPitchWheelTestMidiChordDefs(envList);
                pitchWheelTestMidiChordDefs.Add(pwmcds);
            }

            _pitchWheelTestMidiChordDefs = pitchWheelTestMidiChordDefs;
        }
        private List<MidiChordDef> GetPitchWheelTestMidiChordDefs(IReadOnlyList<IReadOnlyList<byte>> envList)
        {
            List<MidiChordDef> rval = new List<MidiChordDef>();
            foreach(List<byte> envelope in envList)
            {
                MidiChordDef mcd = new MidiChordDef(new List<byte>() { 60 }, new List<byte>() { 127 }, 1000, true);
                mcd.SetPitchWheelEnvelope(new Envelope(envelope, 127, 127, envelope.Count));
                rval.Add(mcd);
            }
            return rval;
        }
        #endregion SetPitchWheelTestMidiChordDefs
        #region SetOrnamentTestMidiChordDefs
        private void SetOrnamentTestMidiChordDefs()
        {
            IReadOnlyList<IReadOnlyList<IReadOnlyList<byte>>> EnvelopeShapes = new List<IReadOnlyList<IReadOnlyList<byte>>>()
            {
                _sliderShapes2, _sliderShapes3, _sliderShapes4, _sliderShapes5, _sliderShapes6, _sliderShapes7, _sliderShapesLong
            };
            List<List<MidiChordDef>> ornamentTestMidiChordDefs = new List<List<MidiChordDef>>();
            int relativePitchHierarchyIndex = 0;
            int absHierarchyRoot = 0;
            foreach(IReadOnlyList<IReadOnlyList<byte>> envList in EnvelopeShapes)
            {
                IReadOnlyList<IReadOnlyList<byte>> localEnvList = EnvelopeShapes[6];
                List<MidiChordDef> omcds = GetOrnamentTestMidiChordDefs(localEnvList, relativePitchHierarchyIndex++, absHierarchyRoot++);
                ornamentTestMidiChordDefs.Add(omcds);
            }

            _ornamentTestMidiChordDefs = ornamentTestMidiChordDefs;
        }

        private List<MidiChordDef> GetOrnamentTestMidiChordDefs(IReadOnlyList<IReadOnlyList<byte>> envList, int relativePitchHierarchyIndex, int absHierarchyRoot)
        {
            List<MidiChordDef> rval = new List<MidiChordDef>();

            List<int> absolutePitchHierarchy = M.GetAbsolutePitchHierarchy(relativePitchHierarchyIndex, absHierarchyRoot);
            Gamut gamut = new Gamut(absolutePitchHierarchy, 8);

            foreach(List<byte> envelope in envList)
            {
                Envelope ornamentEnvelope = new Envelope(envelope, 127, 8, 6);
                Envelope timeWarpEnvelope = new Envelope(new List<byte>() { 127, 0, 127 }, 127, 127, 6);

                int msDuration = 1000;
                int firstPitch = 60;
                while(!gamut.Contains(firstPitch))
                {
                    firstPitch++;
                }
                MidiChordDef mcd = new MidiChordDef(msDuration, gamut, firstPitch, 1, ornamentEnvelope);

                mcd.TimeWarp(timeWarpEnvelope, 16);

                //mcd.Transpose(gamut, 3);

                rval.Add(mcd);
            }
            return rval;
        }
        #endregion SetOrnamentTestMidiChordDefs
        #region SetTombeau1Templates
        private void SetTombeau1Templates()
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

            _tombeau1Templates = new List<Tombeau1Template>()
            { t1template0, t1template1, t1template2, t1template3, t1template4, t1template5, t1template6, t1template7, t1template8, t1template9,
              t1template10, t1template11, t1template12, t1template13, t1template14, t1template15, t1template16, t1template17, t1template18, t1template19,
              t1template20, t1template21 };
        }
        #endregion SetTombeau1Templates
        #endregion Init() helper functions
    }
}
