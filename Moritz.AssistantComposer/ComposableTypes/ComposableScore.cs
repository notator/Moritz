using System;
using System.Collections.Generic;

using Krystals4ObjectLibrary;

using Moritz.Score;
using Moritz.Score.Midi;
using Moritz.Score.Notation;

namespace Moritz.AssistantComposer
{
    public class ComposableSvgScore : SvgScore
    {
        public ComposableSvgScore(string folder, string scoreTitleName, string keywords, string comment, PageFormat pageFormat)
            :base(folder, scoreTitleName, keywords, comment, pageFormat)
        {
        }

        /// <summary>
        /// The krystals and paletteDefs arguments can be null if the algorithm whose name is algorithmName does not use them.
        /// </summary>
        /// <param name="algorithmName"></param>
        /// <param name="krystals"></param>
        /// <param name="paletteDefs"></param>
        /// <returns></returns>
        protected MidiCompositionAlgorithm Algorithm(string algorithmName, List<Krystal> krystals, List<PaletteDef> paletteDefs)
        {
            MidiCompositionAlgorithm midiAlgorithm = null;
            switch(algorithmName)
            {
                case "Study 2c":
                    midiAlgorithm = new Study2cAlgorithm(krystals, paletteDefs);
                    break;
                case "Song Six":
                    midiAlgorithm = new SongSixAlgorithm(krystals, paletteDefs);
                    break;
                case "Study 3 sketch":
                    midiAlgorithm = new Study3SketchAlgorithm(krystals, paletteDefs);
                    break;
                case "paletteDemo":
                    midiAlgorithm = new PaletteDemoAlgorithm(paletteDefs);
                    break;
                default:
                    throw new ApplicationException("unknown algorithm");
            }
            return midiAlgorithm;
        }

        protected List<PaletteDef> GetPaletteDefs(List<Palette> palettes)
        {
            List<PaletteDef> paletteDefs = new List<PaletteDef>();
            for(int paletteIndex = 0; paletteIndex < palettes.Count; ++paletteIndex)
            {
                Palette palette = palettes[paletteIndex];
                List<MidiDurationDef> midiDurationDefs = new List<MidiDurationDef>();
                for(int valueIndex = 0; valueIndex < palette.Domain; ++valueIndex)
                {
                    MidiDurationDef mdd;
                    if(palette.BasicChordSettings.ChordDensities[valueIndex] == 0)
                    {
                        int msDuration = palette.BasicChordSettings.Durations[valueIndex];
                        string id = "palette" + (paletteIndex + 1).ToString() +
                            "_rest" + (valueIndex + 1).ToString();
                        mdd = new MidiRestDef(id, msDuration);
                    }
                    else
                    {
                        string id = "palette" + (paletteIndex + 1).ToString() +
                            "_chord" + (valueIndex + 1).ToString();
                        /// A PaletteMidiChordDef is a MidiChordDef which is saved in or retreived from a palette.
                        /// It can be 'used' in SVG files, but is usually converted to a UniqueMidiChordDef
                        /// (which is saved locally in an SVG file).
                        /// LocalMidiChordDefs are UniqueMidiChordDefs with MsPositon and msDuration attributes.
                        mdd = new PaletteMidiChordDef(id, palette, valueIndex);
                    }
                    midiDurationDefs.Add(mdd);
                }
                paletteDefs.Add(new PaletteDef(midiDurationDefs));
            }
            return paletteDefs;
        }

        /// <summary>
        /// Called by the derived class after setting _midiAlgorithm and Notator
        /// </summary>
        protected void CreateScore()
        {
            List<List<Voice>> voicesPerSystemPerBar = _midiAlgorithm.DoAlgorithm();

            Notator.CreateSystems(this, voicesPerSystemPerBar, _pageFormat.Gap);

            if(_pageFormat.ChordSymbolType != "none") // set by AudioButtonsControl
            {
                Notator.AddSymbolsToSystems(this.Systems);

                FinalizeSystemStructure(); // adds barlines, joins bars to create systems, etc.

                /// The systems do not yet contain Metrics info.
                /// The systems are given Metrics inside the following function then justified internally,
                /// both horizontally and vertically.
                Notator.CreateMetricsAndJustifySystems(this.Systems);

                CreatePages();
            }
        }

        protected List<PaletteDef> _paletteDefs = null;
        protected MidiCompositionAlgorithm _midiAlgorithm = null;
    }
}

