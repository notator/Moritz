﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;

using Moritz.Globals;
using Moritz.Palettes;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
    public class Tombeau1Factory
    {
        /// <summary>
        /// Sets up the standard MidiChordDefs, Trks etc. that will be used in the composition.
        /// </summary>
        public Tombeau1Factory(List<Palette> paletteList, IReadOnlyList<int> midiChannelIndexPerOutputVoice)
        {
            MidiChannelIndexPerOutputVoice = midiChannelIndexPerOutputVoice;

            SetPaletteMidiChordDefs(paletteList);
            SetPitchWheelTestMidiChordDefs();
            SetOrnamentTestMidiChordDefs();

            SetType1TemplateTrks();
            // maybe define other template trk types, and add them here.
        }

        #region constructor helper functions
        private void SetPaletteMidiChordDefs(List<Palette> paletteList)
        {
            List<List<MidiChordDef>> paletteMidiChordDefs = new List<List<MidiChordDef>>();
            foreach(Palette palette in paletteList)
            {
                _paletteMidiChordDefs.Add(GetPaletteMidiChordDefs(palette));
            }
        }
        /// <summary>
        /// A list of the MidiChordDefs defined in the palette.
        /// </summary>
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

        private void SetPitchWheelTestMidiChordDefs()
        {
            foreach(List<List<byte>> envList in _envelopeShapes)
            {
                List<MidiChordDef> pwmcds = GetPitchWheelTestMidiChordDefs(envList);
                _pitchWheelTestMidiChordDefs.Add(pwmcds);
            }
        }
        private List<MidiChordDef> GetPitchWheelTestMidiChordDefs(List<List<byte>> envList)
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

        private void SetOrnamentTestMidiChordDefs()
        {
            int relativePitchHierarchyIndex = 0;
            int absHierarchyRoot = 0;
            foreach(List<List<byte>> envList in _envelopeShapes)
            {
                List<List<byte>> constList = _envelopeShapes[6];
                List<MidiChordDef> omcds = GetOrnamentTestMidiChordDefs(constList, relativePitchHierarchyIndex++, absHierarchyRoot++);
                _ornamentTestMidiChordDefs.Add(omcds);
            }
        }
        private List<MidiChordDef> GetOrnamentTestMidiChordDefs(List<List<byte>> envList, int relativePitchHierarchyIndex, int absHierarchyRoot)
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

        private void SetType1TemplateTrks()
        {
            Trk templateTrk0 = GetType1TemplateTrk(4, 0, 9, new List<byte>() { 0, 127 }, 7);
            _type1TemplateTrks.Add(templateTrk0);
            Trk templateTrk1 = GetType1TemplateTrk(6, 6, 9, new List<byte>() { 0, 127 }, 7);
            // maybe add more type1 template trks here.
            _type1TemplateTrks.Add(templateTrk1);
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
            Envelope ornamentEnvelope = new Envelope(ornamentShape, 127, nPitchesPerOctave, nOrnamentChords);
            MidiChordDef mcd3 = new MidiChordDef(msDuration * 2, gamut, rootNotatedPitch, nPitchesPerChord + 3, ornamentEnvelope);
            mcd3.TransposeInGamut(2);
            iuds.Add(mcd3);
            MidiChordDef mcd4 = new MidiChordDef(msDuration, gamut, rootNotatedPitch, nPitchesPerChord + 4, null);
            mcd4.TransposeInGamut(3);
            iuds.Add(mcd4);

            Trk trk0 = new Trk(0, 0, iuds);

            return trk0;
        }
        #endregion constructor helper functions

        public void AddType1Block(List<Block> blockList, int blockMsDuration, int type1TemplateTrkIndex, int trk0InitialDelay)
        {
            Debug.Assert(blockList != null && blockMsDuration > 0 && type1TemplateTrkIndex >= 0 && trk0InitialDelay >= 0);

            Type1Block type1Block = new Type1Block(this, blockMsDuration, type1TemplateTrkIndex, trk0InitialDelay);
            blockList.Add(type1Block);
        }

        public IReadOnlyList<int> MidiChannelIndexPerOutputVoice;
        public IReadOnlyList<IReadOnlyList<MidiChordDef>> PaletteMidiChordDefs { get { return _paletteMidiChordDefs.AsReadOnly(); } }
        public IReadOnlyList<IReadOnlyList<MidiChordDef>> PitchWheelTestMidiChordDefs { get { return _pitchWheelTestMidiChordDefs.AsReadOnly(); } }
        public IReadOnlyList<IReadOnlyList<MidiChordDef>> OrnamentTestMidiChordDefs { get { return _ornamentTestMidiChordDefs.AsReadOnly(); } }
        public IReadOnlyList<IReadOnlyList<IReadOnlyList<byte>>> EnvelopeShapes { get { return _envelopeShapes.AsReadOnly(); } }
        public IReadOnlyList<IReadOnlyList<int>> DurationModi { get { return _durationModi.AsReadOnly(); } }
        public IReadOnlyList<Trk> Type1TemplateTrks { get { return _type1TemplateTrks.AsReadOnly(); } }


        private List<List<MidiChordDef>> _paletteMidiChordDefs = new List<List<MidiChordDef>>();
        private List<List<MidiChordDef>> _pitchWheelTestMidiChordDefs = new List<List<MidiChordDef>>();
        private List<List<MidiChordDef>> _ornamentTestMidiChordDefs = new List<List<MidiChordDef>>();
        private List<List<List<byte>>> _envelopeShapes = new List<List<List<byte>>>()
            {
                EnvelopesShapes2, EnvelopeShapes3, EnvelopeShapes4, EnvelopeShapes5, EnvelopeShapes6, EnvelopeShapes7, EnvelopeShapesLong
            };
        private List<List<int>> _durationModi = new List<List<int>>()
            {
                Durations1, Durations2, Durations3, Durations4, Durations5, Durations6, Durations7, Durations8, Durations9, Durations10, Durations11, Durations12
            };
        private List<Trk> _type1TemplateTrks = new List<Trk>();

        #region envelopes
        private static List<List<byte>> EnvelopesShapes2 = new List<List<byte>>()
            {
                { new List<byte>() {64, 0} },
                { new List<byte>() {64, 18} },
                { new List<byte>() {64, 36} },
                { new List<byte>() {64, 54} },
                { new List<byte>() {64, 72} },
                { new List<byte>() {64, 91} },
                { new List<byte>() {64, 109} },
                { new List<byte>() {64, 127} }
            };
        private static List<List<byte>> EnvelopeShapes3 = new List<List<byte>>()
            {
                { new List<byte>() {64, 0, 64} },
                { new List<byte>() {64, 18, 64} },
                { new List<byte>() {64, 36, 64} },
                { new List<byte>() {64, 54, 64} },
                { new List<byte>() {64, 72, 64} },
                { new List<byte>() {64, 91, 64} },
                { new List<byte>() {64, 109, 64} },
                { new List<byte>() {64, 127, 64} }
            };
        private static List<List<byte>> EnvelopeShapes4 = new List<List<byte>>()
            {
                { new List<byte>() {64, 0, 64, 64} },
                { new List<byte>() {64, 22, 64, 64} },
                { new List<byte>() {64, 22, 96, 64} },
                { new List<byte>() {64, 64, 0, 64} },
                { new List<byte>() {64, 64, 22, 64} },
                { new List<byte>() {64, 64, 80, 64} },
                { new List<byte>() {64, 80, 64, 64} },
                { new List<byte>() {64, 96, 22, 64 } }
            };
        private static List<List<byte>> EnvelopeShapes5 = new List<List<byte>>()
            {
                { new List<byte>() {64, 50, 72, 50, 64} },
                { new List<byte>() {64, 64, 0, 64, 64} },
                { new List<byte>() {64, 64, 64, 80, 64} },
                { new List<byte>() {64, 64, 64, 106, 64} },
                { new List<byte>() {64, 64, 127, 64, 64} },
                { new List<byte>() {64, 70, 35, 105, 64} },
                { new List<byte>() {64, 72, 50, 70, 64} },
                { new List<byte>() {64, 80, 64, 64, 64} },
                { new List<byte>() {64, 105, 35, 70, 64} },
                { new List<byte>() {64, 106, 64, 64, 64} }
            };
        private static List<List<byte>> EnvelopeShapes6 = new List<List<byte>>()
            {
                { new List<byte>() {64, 22, 43, 64, 64, 64} },
                { new List<byte>() {64, 30, 78, 64, 40, 64} },
                { new List<byte>() {64, 40, 64, 78, 30, 64} },
                { new List<byte>() {64, 43, 106, 64, 64, 64} },
                { new List<byte>() {64, 64, 64, 43, 22, 64} },
                { new List<byte>() {64, 64, 64, 64, 106, 64} },
                { new List<byte>() {64, 64, 64, 64, 127, 64} },
                { new List<byte>() {64, 64, 64, 106, 43, 64} },
                { new List<byte>() {64, 106, 64, 64, 64, 64} },
                { new List<byte>() {64, 127, 127, 22, 64, 64} }
            };
        private static List<List<byte>> EnvelopeShapes7 = new List<List<byte>>()
            {
                { new List<byte>() {64, 0, 0, 106, 106, 64, 64} },
                { new List<byte>() {64, 28, 68, 48, 108, 88, 64} },
                { new List<byte>() {64, 40, 20, 80, 60, 100, 64} },
                { new List<byte>() {64, 55, 50, 75, 50, 64, 64} },
                { new List<byte>() {64, 64, 64, 64, 64, 32, 64} },
                { new List<byte>() {64, 64, 50, 75, 50, 55, 64} },
                { new List<byte>() {64, 73, 78, 53, 78, 64, 64} },
                { new List<byte>() {64, 85, 64, 106, 64, 127, 64} },
                { new List<byte>() {64, 88, 108, 48, 68, 28, 64} },
                { new List<byte>() {64, 100, 60, 80, 20, 40, 64} },
                { new List<byte>() {64, 127, 127, 64, 64, 64, 64} }
            };
        private static List<List<byte>> EnvelopeShapesLong = new List<List<byte>>()
            {
                { new List<byte>() {64, 0, 64, 96, 127, 30, 0, 64} },
                { new List<byte>() {64, 64, 64, 127, 64, 106, 43, 64} },
                { new List<byte>() {64, 64, 43, 43, 64, 64, 85, 22, 64} },
                { new List<byte>() {64, 64, 64, 0, 64, 127, 0, 64, 64} },
                { new List<byte>() {64, 80, 64, 92, 64, 64, 64, 98, 64} },
                { new List<byte>() {64, 98, 64, 64, 64, 92, 64, 80, 64} },
                { new List<byte>() {64, 64, 64, 0, 64, 127, 0, 64, 127, 0, 64, 64} },
                { new List<byte>() {64, 64, 64, 64, 64, 64, 64, 64, 64, 100, 50, 100} },
                { new List<byte>() {64, 64, 64, 64, 64, 64, 64, 64, 64, 127, 43, 127, 64} },
                { new List<byte>() {64, 127, 43, 127, 64, 64, 64, 64, 64, 64, 64, 64, 64} },
                { new List<byte>() {64, 64, 64, 64, 64, 64, 64, 127, 43, 127, 64, 127, 43, 127, 64} },
                { new List<byte>() {64, 127, 43, 127, 43, 127, 64, 64, 64, 64, 64, 64, 64, 64, 64} },
                { new List<byte>() {64, 64, 64, 0, 64, 127, 0, 64, 127, 64, 0, 64, 127, 64, 0, 64} },
                { new List<byte>() {64, 127, 64, 64, 0, 64, 127, 0, 64, 127, 64, 0, 64, 127, 64, 0, 64} },
                { new List<byte>() {64, 127, 43, 127, 43, 127, 64, 127, 43, 127, 43, 127, 64, 64, 64, 64, 64, 64, 64, 64, 64} }
            };
        #endregion envelopes

        #region duration modi
        private static List<int> Durations1 = new List<int>()
            {   1000 };
        private static List<int> Durations2 = new List<int>()
            {   1000, 707 }; // 1 / ( 2^(1 / 2) )
        private static List<int> Durations3 = new List<int>()
            {   1000, 794, 630 }; // 1 / ( 2^(1 / 3) )
        private static List<int> Durations4 = new List<int>()
            {   1000, 841, 707, 595 }; // 1 / ( 2^(1 / 4) )
        private static List<int> Durations5 = new List<int>()
            {   1000, 871, 758, 660, 574 }; // 1 / ( 2^(1 / 5) )
        private static List<int> Durations6 = new List<int>()
            {   1000, 891, 794, 707, 630, 561 }; // 1 / ( 2^(1 / 6) )
        private static List<int> Durations7 = new List<int>()
            {   1000, 906, 820, 743, 673, 610, 552 }; // 1 / ( 2^(1 / 7) )
        private static List<int> Durations8 = new List<int>()
            {   1000, 917, 841, 771, 707, 648, 595, 545}; // 1 / ( 2^(1 / 8) )
        private static List<int> Durations9 = new List<int>()
            {   1000, 926, 857, 794, 735, 680, 630, 583, 540}; // 1 / ( 2^(1 / 9) )
        private static List<int> Durations10 = new List<int>()
            {   1000, 933, 871, 812, 758, 707, 660, 616, 574, 536}; // 1 / ( 2^(1 / 10) )
        private static List<int> Durations11 = new List<int>()
            {   1000, 939, 882, 828, 777, 730, 685, 643, 604, 567, 533 }; // 1 / ( 2^(1 / 11) )
        private static List<int> Durations12 = new List<int>()
            {   1000, 944, 891, 841, 794, 749, 707, 667, 630, 595, 561, 530 }; // 1 / ( 2^(1 / 12) )

        #endregion duration modi
    }
}