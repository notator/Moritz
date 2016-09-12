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

            SetTemplateTrks();
            // maybe define other template trk types, and add them here.
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
        #region SetType1TemplateTrks
        private void SetTemplateTrks()
        {
            List<Level1TemplateTrk> level1TemplateTrks = GetLevel1TemplateTrks();
            SetTemplateTrks(level1TemplateTrks);
        }

        private List<Level1TemplateTrk> GetLevel1TemplateTrks()
        {
            int rootPitch = 0;
            int nPitchesPerOctave = 8;
            int nChordsPerOrnament = 5;

            Debug.Assert(_ornamentShapes.Count > 10);

            // The standard relative pitch heirarchies are in a circular matrix having 22 values.
            // These pitch hierarchies are in order of distance from the root heirarchy.
            Level1TemplateTrk level1tt0 = new Level1TemplateTrk(0, rootPitch, nPitchesPerOctave, _ornamentShapes[0], nChordsPerOrnament);
            Level1TemplateTrk level1tt1 = new Level1TemplateTrk(21, rootPitch, nPitchesPerOctave, _ornamentShapes[1], nChordsPerOrnament);
            Level1TemplateTrk level1tt2 = new Level1TemplateTrk(1, rootPitch, nPitchesPerOctave, _ornamentShapes[2], nChordsPerOrnament);
            Level1TemplateTrk level1tt3 = new Level1TemplateTrk(20, rootPitch, nPitchesPerOctave, _ornamentShapes[3], nChordsPerOrnament);
            Level1TemplateTrk level1tt4 = new Level1TemplateTrk(2, rootPitch, nPitchesPerOctave, _ornamentShapes[4], nChordsPerOrnament);
            Level1TemplateTrk level1tt5 = new Level1TemplateTrk(19, rootPitch, nPitchesPerOctave, _ornamentShapes[5], nChordsPerOrnament);
            Level1TemplateTrk level1tt6 = new Level1TemplateTrk(3, rootPitch, nPitchesPerOctave, _ornamentShapes[6], nChordsPerOrnament);
            Level1TemplateTrk level1tt7 = new Level1TemplateTrk(18, rootPitch, nPitchesPerOctave, _ornamentShapes[7], nChordsPerOrnament);
            Level1TemplateTrk level1tt8 = new Level1TemplateTrk(4, rootPitch, nPitchesPerOctave, _ornamentShapes[8], nChordsPerOrnament);
            Level1TemplateTrk level1tt9 = new Level1TemplateTrk(17, rootPitch, nPitchesPerOctave, _ornamentShapes[9], nChordsPerOrnament);
            Level1TemplateTrk level1tt10 = new Level1TemplateTrk(5, rootPitch, nPitchesPerOctave, _ornamentShapes[10], nChordsPerOrnament);
            Level1TemplateTrk level1tt11 = new Level1TemplateTrk(16, rootPitch, nPitchesPerOctave, _ornamentShapes[10], nChordsPerOrnament);
            Level1TemplateTrk level1tt12 = new Level1TemplateTrk(6, rootPitch, nPitchesPerOctave, _ornamentShapes[9], nChordsPerOrnament);
            Level1TemplateTrk level1tt13 = new Level1TemplateTrk(15, rootPitch, nPitchesPerOctave, _ornamentShapes[8], nChordsPerOrnament);
            Level1TemplateTrk level1tt14 = new Level1TemplateTrk(7, rootPitch, nPitchesPerOctave, _ornamentShapes[7], nChordsPerOrnament);
            Level1TemplateTrk level1tt15 = new Level1TemplateTrk(14, rootPitch, nPitchesPerOctave, _ornamentShapes[6], nChordsPerOrnament);
            Level1TemplateTrk level1tt16 = new Level1TemplateTrk(8, rootPitch, nPitchesPerOctave, _ornamentShapes[5], nChordsPerOrnament);
            Level1TemplateTrk level1tt17 = new Level1TemplateTrk(13, rootPitch, nPitchesPerOctave, _ornamentShapes[4], nChordsPerOrnament);
            Level1TemplateTrk level1tt18 = new Level1TemplateTrk(9, rootPitch, nPitchesPerOctave, _ornamentShapes[3], nChordsPerOrnament);
            Level1TemplateTrk level1tt19 = new Level1TemplateTrk(12, rootPitch, nPitchesPerOctave, _ornamentShapes[2], nChordsPerOrnament);
            Level1TemplateTrk level1tt20 = new Level1TemplateTrk(10, rootPitch, nPitchesPerOctave, _ornamentShapes[1], nChordsPerOrnament);
            Level1TemplateTrk level1tt21 = new Level1TemplateTrk(11, rootPitch, nPitchesPerOctave, _ornamentShapes[0], nChordsPerOrnament);

            var level1TemplateTrks = new List<Level1TemplateTrk>()
            { level1tt0, level1tt1, level1tt2, level1tt3, level1tt4, level1tt5, level1tt6, level1tt7, level1tt8, level1tt9,
              level1tt10, level1tt11, level1tt12, level1tt13, level1tt14, level1tt15, level1tt16, level1tt17, level1tt18, level1tt19,
              level1tt20, level1tt21 };

            return level1TemplateTrks;
        }

        private void SetTemplateTrks(IReadOnlyList<Level1TemplateTrk> level1TemplateTrks)
        {
            int nSubTrks = 5;
            int nChordsPerOrnament = 5;

            List<Tombeau1TemplateTrk> templateTrks = new List<Tombeau1TemplateTrk>();
            templateTrks.Add(new Tombeau1TemplateTrk(level1TemplateTrks[0], nSubTrks, _ornamentShapes[0], nChordsPerOrnament));
            templateTrks.Add(new Tombeau1TemplateTrk(level1TemplateTrks[1], nSubTrks, _ornamentShapes[1], nChordsPerOrnament));
            templateTrks.Add(new Tombeau1TemplateTrk(level1TemplateTrks[2], nSubTrks, _ornamentShapes[2], nChordsPerOrnament));
            templateTrks.Add(new Tombeau1TemplateTrk(level1TemplateTrks[3], nSubTrks, _ornamentShapes[3], nChordsPerOrnament));
            templateTrks.Add(new Tombeau1TemplateTrk(level1TemplateTrks[4], nSubTrks, _ornamentShapes[4], nChordsPerOrnament));
            templateTrks.Add(new Tombeau1TemplateTrk(level1TemplateTrks[5], nSubTrks, _ornamentShapes[5], nChordsPerOrnament));
            templateTrks.Add(new Tombeau1TemplateTrk(level1TemplateTrks[6], nSubTrks, _ornamentShapes[6], nChordsPerOrnament));
            templateTrks.Add(new Tombeau1TemplateTrk(level1TemplateTrks[7], nSubTrks, _ornamentShapes[7], nChordsPerOrnament));
            templateTrks.Add(new Tombeau1TemplateTrk(level1TemplateTrks[8], nSubTrks, _ornamentShapes[8], nChordsPerOrnament));
            templateTrks.Add(new Tombeau1TemplateTrk(level1TemplateTrks[9], nSubTrks, _ornamentShapes[9], nChordsPerOrnament));
            templateTrks.Add(new Tombeau1TemplateTrk(level1TemplateTrks[10], nSubTrks, _ornamentShapes[10], nChordsPerOrnament));
            templateTrks.Add(new Tombeau1TemplateTrk(level1TemplateTrks[11], nSubTrks, _ornamentShapes[10], nChordsPerOrnament));
            templateTrks.Add(new Tombeau1TemplateTrk(level1TemplateTrks[12], nSubTrks, _ornamentShapes[9], nChordsPerOrnament));
            templateTrks.Add(new Tombeau1TemplateTrk(level1TemplateTrks[13], nSubTrks, _ornamentShapes[8], nChordsPerOrnament));
            templateTrks.Add(new Tombeau1TemplateTrk(level1TemplateTrks[14], nSubTrks, _ornamentShapes[7], nChordsPerOrnament));
            templateTrks.Add(new Tombeau1TemplateTrk(level1TemplateTrks[15], nSubTrks, _ornamentShapes[6], nChordsPerOrnament));
            templateTrks.Add(new Tombeau1TemplateTrk(level1TemplateTrks[16], nSubTrks, _ornamentShapes[5], nChordsPerOrnament));
            templateTrks.Add(new Tombeau1TemplateTrk(level1TemplateTrks[17], nSubTrks, _ornamentShapes[4], nChordsPerOrnament));
            templateTrks.Add(new Tombeau1TemplateTrk(level1TemplateTrks[18], nSubTrks, _ornamentShapes[3], nChordsPerOrnament));
            templateTrks.Add(new Tombeau1TemplateTrk(level1TemplateTrks[19], nSubTrks, _ornamentShapes[2], nChordsPerOrnament));
            templateTrks.Add(new Tombeau1TemplateTrk(level1TemplateTrks[20], nSubTrks, _ornamentShapes[1], nChordsPerOrnament));
            templateTrks.Add(new Tombeau1TemplateTrk(level1TemplateTrks[21], nSubTrks, _ornamentShapes[0], nChordsPerOrnament));

            _templateTrks = templateTrks;
        }

        #endregion SetType1TemplateTrks
        #endregion Init() helper functions
    }
}
