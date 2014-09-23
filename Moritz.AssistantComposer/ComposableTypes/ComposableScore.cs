using System;
using System.Collections.Generic;

using Krystals4ObjectLibrary;
using Moritz.Score;
using Moritz.Score.Notation;
using Moritz.AssistantComposer.SongSix;

namespace Moritz.AssistantComposer
{
    public class ComposableSvgScore : SvgScore
    {
        public ComposableSvgScore(string folder, string scoreTitleName, string keywords, string comment, PageFormat pageFormat)
            : base(folder, scoreTitleName, keywords, comment, pageFormat)
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
            string errorString = null;
            if(voicesPerSystemPerBar.Count == 0)
                errorString = "The algorithm has not created any bars!";
            else
            {
                errorString = BasicCheck(voicesPerSystemPerBar);
            }
            if(string.IsNullOrEmpty(errorString))
            {
                errorString = CheckInputControlsDef(voicesPerSystemPerBar);
            }
            if(!string.IsNullOrEmpty(errorString))
            {
                throw new ApplicationException("\nComposableScore.CheckBars(): Algorithm error:\n" + errorString);
            }
        }
        #region private to CheckBars(...)
        private string BasicCheck(List<List<Voice>> voicesPerSystemPerBar)
        {
            string errorString = null;
            foreach(List<Voice> bar in voicesPerSystemPerBar)
            {
                if(bar.Count == 0)
                {
                    errorString = "One bar (at least) contains no voices.";
                    break;
                }
                if(!(bar[0] is OutputVoice))
                {
                    errorString = "The top (first) voice in every bar must be an output voice.";
                    break;
                }
                for(int voiceIndex = 0; voiceIndex < bar.Count; ++voiceIndex)
                {
                    Voice voice = bar[voiceIndex];
                    if(voice.UniqueDefs.Count == 0)
                    {
                        errorString = "A voice (voiceIndex=" + voiceIndex.ToString() + ") has an empty UniqueDefs list.";
                        break;
                    }
                    if(voice.NoteObjects.Count != 0)
                    {
                        errorString = "A voice (voiceIndex=" + voiceIndex.ToString() + ") has an empty NoteObjects list.";
                        break;
                    }
                }
                if(!string.IsNullOrEmpty(errorString))
                    break;
            }
            return errorString;
        }
        private string CheckInputControlsDef(List<List<Voice>> voicesPerSystemPerBar)
        {
            string errorString = null;
            List<OutputVoice> oVoices = new List<OutputVoice>();
            foreach(Voice voice in voicesPerSystemPerBar[0])
            {
                OutputVoice ov = voice as OutputVoice;
                if(ov != null)
                {
                    if(ov.MasterVolume == 0)
                    {
                        errorString = "\nEvery OutputVoice in the first bar of a score\n" +
                                      "must have a MasterVolume value greater than 0.";
                    }
                    oVoices.Add(ov);
                }
                else // voice is an InputVoice
                {
                    foreach(OutputVoice ov1 in oVoices)
                    {
                        if(ov1.InputControls == null)
                        {
                            errorString = "\nThis score contains InputVoice(s),\n" +
                                          "so every OutputVoice in the first bar must have an InputControls definition.";
                        }
                    }
                }
            }
            if(string.IsNullOrEmpty(errorString) && voicesPerSystemPerBar.Count > 1)
            {
                for(int bar = 1; bar < voicesPerSystemPerBar.Count; ++bar)
                {
                    foreach(Voice voice in voicesPerSystemPerBar[bar])
                    {
                        OutputVoice ov = voice as OutputVoice;
                        if(ov != null)
                        {
                            if(ov.MasterVolume != 0)
                            {
                                errorString = "\nNo OutputVoice except the first may ever have a MasterVolume.";
                            }
                            if(ov.InputControls != null)
                            {
                                errorString = "\nNo OutputVoice except the first may ever have an InputControls definition.";
                            }
                        }
                    }
                }
            }
            return errorString;
        }
        #endregion

        /// <summary>
        /// Called by the derived class after setting _midiAlgorithm and Notator
        /// </summary>
        protected void CreateScore()
        {
            List<List<Voice>> voicesPerSystemPerBar = _midiAlgorithm.DoAlgorithm();

            CheckBars(voicesPerSystemPerBar);

            Notator.CreateSystems(this, voicesPerSystemPerBar);

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

