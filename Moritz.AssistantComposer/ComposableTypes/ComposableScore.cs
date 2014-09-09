using System;
using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;
using Moritz.Globals;
using Moritz.Krystals;
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
        protected MidiCompositionAlgorithm Algorithm(string algorithmName, List<Krystal> krystals, List<Palette> palettes)
        {
            MidiCompositionAlgorithm midiAlgorithm = null;
            switch(algorithmName)
            {
                case "Study 2c3.1":
                    midiAlgorithm = new Study2c3_1Algorithm(krystals, palettes);
                    break;
                case "Song Six":
                    midiAlgorithm = new SongSixAlgorithm(krystals, palettes);
                    break;
                case "Study 3 sketch 1":
                    midiAlgorithm = new Study3Sketch1Algorithm(krystals, palettes);
                    break;
                case "Study 3 sketch 2":
                    midiAlgorithm = new Study3Sketch2Algorithm(krystals, palettes);
                    break;
                case "paletteDemo":
                    midiAlgorithm = new PaletteDemoAlgorithm(palettes);
                    break;
                default:
                    throw new ApplicationException("unknown algorithm");
            }
            return midiAlgorithm;
        }

        private void CheckBars(List<List<Voice>> voicesPerSystemPerBar)
        {
            int error = 0;
            if(voicesPerSystemPerBar.Count == 0)
                error = 1;
            else
            {
                foreach(List<Voice> bar in voicesPerSystemPerBar)
                {
                    if(bar.Count == 0)
                    {
                        error = 2;
                        break;
                    }
                    if(!(bar[0] is OutputVoice))
                    {
                        error = 3;
                        break;
                    }
                    foreach(Voice voice in bar)
                    {
                        if(voice.UniqueDefs.Count == 0)
                        {
                            error = 4;
                            break;
                        }
                        if(voice.NoteObjects.Count != 0)
                        {
                            error = 5;
                            break;
                        }
                    }
                    if(error > 0)
                        break;
                }
            }
            if(error > 0)
            {
                throw new ApplicationException("ComposableScore.CheckBars(): Algorithm error: " + error.ToString());
            }
        }
        /// <summary>
        /// Called by the derived class after setting _midiAlgorithm and Notator
        /// </summary>
        protected void CreateScore()
        {
            List<List<Voice>> voicesPerSystemPerBar = _midiAlgorithm.DoAlgorithm();

            CheckBars(voicesPerSystemPerBar);

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

        protected MidiCompositionAlgorithm _midiAlgorithm = null;
    }
}

