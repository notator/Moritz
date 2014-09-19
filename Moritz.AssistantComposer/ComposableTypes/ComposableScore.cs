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
                    if(! string.IsNullOrEmpty(errorString))
                        break;
                }
            }
            if(!string.IsNullOrEmpty(errorString))
            {
                throw new ApplicationException("\nComposableScore.CheckBars(): Algorithm error:\n" + errorString);
            }
        }
        /// <summary>
        /// Called by the derived class after setting _midiAlgorithm and Notator
        /// </summary>
        protected void CreateScore()
        {
            List<List<Voice>> voicesPerSystemPerBar = _midiAlgorithm.DoAlgorithm();

            CheckPerformanceOptions(_midiAlgorithm, voicesPerSystemPerBar);

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

                CreatePages(_midiAlgorithm.PerformanceControlDef);
            }
        }

        /// <summary>
        /// Throw an exception if _midiAlgorithm.PerformanceOptionsDef is null and there is an InputVoice. 
        /// </summary>
        /// <param name="_midiAlgorithm"></param>
        /// <param name="voicesPerSystemPerBar"></param>
        private void CheckPerformanceOptions(MidiCompositionAlgorithm midiAlgorithm, List<List<Voice>> voicesPerSystemPerBar)
        {
            bool hasAnInputVoice = HasAnInputVoice(voicesPerSystemPerBar);

            if(midiAlgorithm.PerformanceControlDef == null && hasAnInputVoice == true)
            {
                throw new ApplicationException("\nComposableScore:CheckPerformanceOptions(...)" +
                                               "\nIf the algorithm defines an InputVoice, then it must" +
                                               "\nalso define global PerformanceControl options.");
            }
            else if(midiAlgorithm.PerformanceControlDef != null && hasAnInputVoice == false)
            {
                throw new ApplicationException("\nComposableScore:CheckPerformanceOptions(...)" + 
                                               "\nThis algorithm does not define any InputVoices, so it does" +
                                               "\nnot need to define global PerformanceControl options.");
            }
        }
        #region private to CheckPerformanceOptions(...)
        private bool HasAnInputVoice(List<List<Voice>> voicesPerSystemPerBar)
        {
            bool rval = false;
            List<Voice> voices = voicesPerSystemPerBar[0];
            foreach(Voice voice in voices)
            {
                if(voice is InputVoice)
                {
                    rval = true;
                    break;
                }
            }
            return rval;
        }
        #endregion

        protected MidiCompositionAlgorithm _midiAlgorithm = null;
    }
}

