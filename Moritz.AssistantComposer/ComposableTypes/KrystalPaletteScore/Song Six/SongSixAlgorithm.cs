using System.Collections.Generic;
using System.Diagnostics;
using System;

using Moritz.Score;
using Moritz.Score.Midi;
using Krystals4ObjectLibrary;

namespace Moritz.AssistantComposer
{
    /// <summary>
    /// The Algorithm for Song 6.
    /// This will develope as composition progresses...
    /// </summary>
    internal class SongSixAlgorithm : MidiCompositionAlgorithm
    {
        public SongSixAlgorithm(List<Krystal> krystals, List<PaletteDef> paletteDefs)
            : base(krystals, paletteDefs)
        {
        }

        /// <summary>
        /// The values are then checked for consistency in the base constructor.
        /// </summary>
        public override List<byte> MidiChannels()
        {
            return new List<byte>() { 0, 1 };
        }

        /// <summary>
        /// The number of bars produced by DoAlgorithm().
        /// </summary>
        /// <returns></returns>
        public override int NumberOfBars()
        {
            return 93;
        }

        /// <summary>
        /// Sets the midi content of the score, independent of its notation.
        /// This means adding MidiDurationDefs to each voice's MidiDurationDefs list.
        /// The MidiDurations will later be transcribed into a particular notation by a Notator.
        /// Notations are independent of the midi info.
        /// This DoAlgorithm() function is special to this composition.
        /// </summary>
        /// <returns>
        /// A list of sequential bars. Each bar contains all the voices in the bar, from top to bottom.
        /// </returns>
        public override List<List<Voice>> DoAlgorithm()
        {
            int tempInterludeMsDuration = int.MaxValue / 50;
            Clytemnestra clytemnestra = new Clytemnestra(tempInterludeMsDuration); 

            List<VoiceDef> winds = new List<VoiceDef>();

            VoiceDef bassWind = new VoiceDef(_paletteDefs[0], _krystals[2]);
            winds.Add(bassWind);

            AlignClytemnestraToBassWind(clytemnestra, bassWind, tempInterludeMsDuration);

            List<int> barlineMsPositions = GetBarlineMsPositions(clytemnestra, bassWind);
            // barlineMsPositions contains both the position of bar 1 (0ms) and the position of the final barline

            Debug.Assert(barlineMsPositions.Count == NumberOfBars() + 1); // includes bar 1 (mPos=0) and the final barline.
            
            //winds.CompleteTheWinds(barlineMsPositions);

            #region test code
            //code for testing VoiceDef.SetContour(...)
            //VoiceDef contouredPhrase = winds.VoiceDefs[0];
            //contouredPhrase.SetContour(11, new List<int>() { 1, 4, 1, 2 }, 1, 1);

            //code for testing Translate
            //VoiceDef translated = winds.VoiceDefs[0];
            //translated.Translate(15, 4, 16);
            #endregion

            //Birds birds = new Birds(clytemnestra, winds, _krystals, _paletteDefs, blockMsDurations);

            clytemnestra.AddIndexToLyrics();
            foreach(VoiceDef wind in winds)
            {
                wind.SetLyricsToIndex();
            }

            // system contains one Voice per channel (not divided into bars)

            List<Voice> system = GetVoices(/*birds,*/ clytemnestra, winds);

            List<List<Voice>> bars = GetBars(system, barlineMsPositions);

            return bars;
        }

        private void AlignClytemnestraToBassWind(Clytemnestra clytemnestra, VoiceDef bassWind, int tempInterludeMsDuration)
        {
            List<int> verseMsPositions = new List<int>();
            verseMsPositions.Add(bassWind[8].MsPosition);
            verseMsPositions.Add(bassWind[20].MsPosition);
            verseMsPositions.Add(bassWind[33].MsPosition);
            verseMsPositions.Add(bassWind[49].MsPosition);
            verseMsPositions.Add(bassWind[70].MsPosition);

            List<LocalMidiDurationDef> clmdds = clytemnestra.LocalMidiDurationDefs;
            clmdds[0].MsDuration = verseMsPositions[0];
            clytemnestra.MsPosition = 0; // sets all the positions
            for(int verse = 2; verse <= 5; ++verse)
            {
                int interludeIndex = clytemnestra.LocalMidiDurationDefs.FindIndex(
                    x => (x.UniqueMidiDurationDef is UniqueMidiRestDef 
                          && x.MsDuration == tempInterludeMsDuration));
                clmdds[interludeIndex].MsDuration = verseMsPositions[verse - 1] - clmdds[interludeIndex].MsPosition;
                clytemnestra.MsPosition = 0; // sets all the positions
            }
            clmdds[clmdds.Count - 1].MsDuration = bassWind.EndMsPosition - clmdds[clmdds.Count - 1].MsPosition;
        }

        /// <summary>
        /// The returned barlineMsPositions contain both the position of bar 1 (0ms) and the position of the final barline.
        /// </summary>
        private List<int> GetBarlineMsPositions(Clytemnestra clytemnestra, VoiceDef bassWind)
        {
            List<int> barlineMsPositions = clytemnestra.BarLineMsPositions;
            barlineMsPositions = AddInterludeBarlinePositions(bassWind, barlineMsPositions);
            barlineMsPositions.Add(0);
            barlineMsPositions.Add(bassWind.EndMsPosition);
            barlineMsPositions.Sort();
            return barlineMsPositions;
        }

        private List<int> AddInterludeBarlinePositions(VoiceDef bassWind, List<int> barlineMsPositions)
        {
            List<int> newBarlineIndices = new List<int>() { 1, 3, 5, 15, 27, 40, 45, 63, 77 }; // by inspection of the score
            foreach(int index in newBarlineIndices)
            {
                barlineMsPositions.Add(bassWind.LocalMidiDurationDefs[index].MsPosition);
            }
            barlineMsPositions.Sort();

            return barlineMsPositions;
        }

        private List<Voice> GetVoices(/*Birds birds,*/ Clytemnestra clytemnestra, List<VoiceDef> winds)
        {
            byte channelIndex = 0;
            List<Voice> voices = new List<Voice>();

            //List<Voice> birdVoices = birds.GetVoices(channelIndex);
            //foreach(Voice voice in birdVoices)
            //{
            //    voices.Add(voice);
            //    channelIndex++;
            //}

            Voice clytemnestrasVoice = new Voice(null, channelIndex++);
            clytemnestrasVoice.LocalMidiDurationDefs = clytemnestra.LocalMidiDurationDefs;
            voices.Add(clytemnestrasVoice);

            foreach(VoiceDef windDef in winds)
            {
                Voice windVoice = new Voice(null, channelIndex++);
                windVoice.LocalMidiDurationDefs = windDef.LocalMidiDurationDefs;
                voices.Add(windVoice);
            }

            return voices;
        }

        private List<List<Voice>> GetBars(List<Voice> system, List<int> barlineMsPositions)
        {
            // barlineMsPositions contains both msPos=0 and the position of the final barline
            List<List<Voice>> bars = new List<List<Voice>>();
            bars = GetBarsFromBarlineMsPositions(system, barlineMsPositions);
            Debug.Assert(bars.Count == NumberOfBars());
            return bars;
        }

        /// <summary>
        /// Splits the voices (currently in a single bar) into bars
        /// barlineMsPositions contains both msPosition 0, and the position of the final barline.
        /// </summary>
        private List<List<Voice>> GetBarsFromBarlineMsPositions(List<Voice> voices, List<int> barLineMsPositions)
        {
            List<List<Voice>> bars = new List<List<Voice>>();
            List<List<Voice>> twoBars = null;

            for(int i = barLineMsPositions.Count - 2; i >= 1; --i)
            {
                twoBars = SplitBar(voices, barLineMsPositions[i]);
                bars.Insert(0, twoBars[1]);
                voices = twoBars[0];
            }
            bars.Insert(0, twoBars[0]);

            return bars;
        }
    }
}
