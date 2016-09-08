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

            SetType1TemplateTrks();
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
        private void SetType1TemplateTrks()
        {
            List<Level1TemplateTrk> level1TemplateTrks = new List<Level1TemplateTrk>();

            int nPitchesPerOctave = 2;

            level1TemplateTrks.Add(new Level1TemplateTrk(0, 0, nPitchesPerOctave));
            level1TemplateTrks.Add(new Level1TemplateTrk(11, 0, nPitchesPerOctave++));
            level1TemplateTrks.Add(new Level1TemplateTrk(1, 0, nPitchesPerOctave));
            level1TemplateTrks.Add(new Level1TemplateTrk(12, 0, nPitchesPerOctave++));
            level1TemplateTrks.Add(new Level1TemplateTrk(2, 0, nPitchesPerOctave));
            level1TemplateTrks.Add(new Level1TemplateTrk(13, 0, nPitchesPerOctave++));
            level1TemplateTrks.Add(new Level1TemplateTrk(3, 0, nPitchesPerOctave));
            level1TemplateTrks.Add(new Level1TemplateTrk(14, 0, nPitchesPerOctave++));
            level1TemplateTrks.Add(new Level1TemplateTrk(4, 0, nPitchesPerOctave));
            level1TemplateTrks.Add(new Level1TemplateTrk(15, 0, nPitchesPerOctave++));
            level1TemplateTrks.Add(new Level1TemplateTrk(5, 0, nPitchesPerOctave));
            level1TemplateTrks.Add(new Level1TemplateTrk(16, 0, nPitchesPerOctave++));
            level1TemplateTrks.Add(new Level1TemplateTrk(6, 0, nPitchesPerOctave));
            level1TemplateTrks.Add(new Level1TemplateTrk(17, 0, nPitchesPerOctave++));
            level1TemplateTrks.Add(new Level1TemplateTrk(7, 0, nPitchesPerOctave));
            level1TemplateTrks.Add(new Level1TemplateTrk(18, 0, nPitchesPerOctave++));
            level1TemplateTrks.Add(new Level1TemplateTrk(8, 0, nPitchesPerOctave));
            level1TemplateTrks.Add(new Level1TemplateTrk(19, 0, nPitchesPerOctave++));
            level1TemplateTrks.Add(new Level1TemplateTrk(9, 0, nPitchesPerOctave));
            level1TemplateTrks.Add(new Level1TemplateTrk(20, 0, nPitchesPerOctave++));
            level1TemplateTrks.Add(new Level1TemplateTrk(10, 0, nPitchesPerOctave));
            level1TemplateTrks.Add(new Level1TemplateTrk(21, 0, nPitchesPerOctave));

            List<Trk> level2TemplateTrks = new List<Trk>();
            level2TemplateTrks.Add(new Level2TemplateTrk(level1TemplateTrks[0], 6, _ornamentShapes[0], 7));
            level2TemplateTrks.Add(new Level2TemplateTrk(level1TemplateTrks[1], 5, _ornamentShapes[1], 7));
            level2TemplateTrks.Add(new Level2TemplateTrk(level1TemplateTrks[2], 5, _ornamentShapes[2], 7));
            level2TemplateTrks.Add(new Level2TemplateTrk(level1TemplateTrks[3], 6, _ornamentShapes[3], 7));
            level2TemplateTrks.Add(new Level2TemplateTrk(level1TemplateTrks[4], 6, _ornamentShapes[4], 7));
            level2TemplateTrks.Add(new Level2TemplateTrk(level1TemplateTrks[5], 6, _ornamentShapes[5], 7));
            level2TemplateTrks.Add(new Level2TemplateTrk(level1TemplateTrks[6], 5, _ornamentShapes[6], 7));
            level2TemplateTrks.Add(new Level2TemplateTrk(level1TemplateTrks[7], 5, _ornamentShapes[7], 7));
            level2TemplateTrks.Add(new Level2TemplateTrk(level1TemplateTrks[8], 5, _ornamentShapes[6], 7));
            level2TemplateTrks.Add(new Level2TemplateTrk(level1TemplateTrks[9], 5, _ornamentShapes[5], 7));
            level2TemplateTrks.Add(new Level2TemplateTrk(level1TemplateTrks[10], 5, _ornamentShapes[4], 7));
            level2TemplateTrks.Add(new Level2TemplateTrk(level1TemplateTrks[11], 5, _ornamentShapes[3], 7));
            level2TemplateTrks.Add(new Level2TemplateTrk(level1TemplateTrks[12], 5, _ornamentShapes[2], 7));
            level2TemplateTrks.Add(new Level2TemplateTrk(level1TemplateTrks[13], 5, _ornamentShapes[1], 7));
            level2TemplateTrks.Add(new Level2TemplateTrk(level1TemplateTrks[14], 5, _ornamentShapes[0], 7));
            level2TemplateTrks.Add(new Level2TemplateTrk(level1TemplateTrks[15], 5, _ornamentShapes[1], 7));
            level2TemplateTrks.Add(new Level2TemplateTrk(level1TemplateTrks[16], 5, _ornamentShapes[2], 7));
            level2TemplateTrks.Add(new Level2TemplateTrk(level1TemplateTrks[17], 5, _ornamentShapes[3], 7));
            level2TemplateTrks.Add(new Level2TemplateTrk(level1TemplateTrks[18], 5, _ornamentShapes[4], 7));
            level2TemplateTrks.Add(new Level2TemplateTrk(level1TemplateTrks[19], 5, _ornamentShapes[5], 7));
            level2TemplateTrks.Add(new Level2TemplateTrk(level1TemplateTrks[20], 5, _ornamentShapes[6], 7));
            level2TemplateTrks.Add(new Level2TemplateTrk(level1TemplateTrks[21], 5, _ornamentShapes[7], 7));

            _level1TemplateTrks = level1TemplateTrks;
            _level2TemplateTrks = level2TemplateTrks;
        }

        


        #endregion SetType1TemplateTrks
        #endregion Init() helper functions
    }
}
