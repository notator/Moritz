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
            List<Trk> type1TemplateTrks = new List<Trk>();

            Trk templateTrk0 = GetType1TemplateTrk(4, 0, 9, new List<byte>() { 0, 127 }, 7);
            type1TemplateTrks.Add(templateTrk0);
            Trk templateTrk1 = GetType1TemplateTrk(6, 6, 9, new List<byte>() { 0, 127 }, 7);
            // maybe add more type1 template trks here.
            type1TemplateTrks.Add(templateTrk1);

            _type1TemplateTrks = type1TemplateTrks;
        }

        private Trk GetType1TemplateTrk(int relativePitchHierarchyIndex, int rootPitch, int nPitchesPerOctave, IReadOnlyList<byte> ornamentShape, int nOrnamentChords)
        {
            List<int> absolutePitchHierarchy = M.GetAbsolutePitchHierarchy(relativePitchHierarchyIndex, rootPitch);
            Gamut gamut = new Gamut(absolutePitchHierarchy, nPitchesPerOctave);

            List<IUniqueDef> iuds = new List<IUniqueDef>();
            int rootNotatedPitch = gamut[gamut.Count / 2];
            int nPitchesPerChord = 1;
            int msDuration = 1000;

            MidiChordDef mcd1 = new MidiChordDef(msDuration, gamut, rootNotatedPitch, nPitchesPerChord + 1, null);
            iuds.Add(mcd1);

            MidiChordDef mcd2 = new MidiChordDef(msDuration, gamut, rootNotatedPitch, nPitchesPerChord + 2, null);
            mcd2.TransposeInGamut(1);
            iuds.Add(mcd2);

            //Envelope ornamentEnvelope = new Envelope(ornamentShape, 127, nPitchesPerOctave, nOrnamentChords);
            //MidiChordDef mcd3 = new MidiChordDef(msDuration * 2, gamut, rootNotatedPitch, nPitchesPerChord + 3, ornamentEnvelope);
            MidiChordDef mcd3 = new MidiChordDef(msDuration, gamut, rootNotatedPitch, nPitchesPerChord + 3, null);
            mcd3.TransposeInGamut(2);
            iuds.Add(mcd3);

            MidiChordDef mcd4 = new MidiChordDef(msDuration, gamut, rootNotatedPitch, nPitchesPerChord + 4, null);
            mcd4.TransposeInGamut(3);
            iuds.Add(mcd4);

            Trk trk0 = new Trk(0, 0, iuds);

            return trk0;
        }
        #endregion SetType1TemplateTrks
        #endregion Init() helper functions
    }
}
