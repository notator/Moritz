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
            return new List<byte>() { 0, 1, 2 };
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
        /// This means adding LocalMidiDurationDefs to each VoiceDef's LocalMidiDurationDefs list.
        /// The LocalMidiDurationDefs will later be transcribed into a particular notation by a Notator.
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
            VoiceDef bassWind = new VoiceDef(_paletteDefs[0], _krystals[2]);

            AlignClytemnestraToBassWind(clytemnestra, bassWind, tempInterludeMsDuration);

            VoiceDef control = bassWind.Clone();

            // barlineMsPositions contains both the position of bar 1 (0ms) and the position of the final barline
            List<int> barlineMsPositions = GetBarlineMsPositions(clytemnestra, bassWind);

            Debug.Assert(barlineMsPositions.Count == NumberOfBars() + 1); // includes bar 1 (mPos=0) and the final barline.
            
            // Complete the winds and birds.
            #region code for testing VoiceDef functions
            //bassWind.SetContour(11, new List<int>() { 1, 4, 1, 2 }, 1, 1);
            //bassWind.Translate(15, 4, 16);
            // TODO:
            // Cut, Copy, PasteAt (List<LocalMididurationDefs>) !!
            #endregion

            // Add each voiceDef to voiceDefs here, in top to bottom (=channelIndex) order in the score.
            List<VoiceDef> voiceDefs = new List<VoiceDef>() {control, clytemnestra, bassWind /* etc.*/};
            Debug.Assert(voiceDefs.Count == MidiChannels().Count);
            foreach(VoiceDef voiceDef in voiceDefs)
            {
                voiceDef.SetLyricsToIndex();
            }
            // this system contains one Voice per channel (not divided into bars)
            List<Voice> system = GetVoices(voiceDefs);
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
            clmdds[0].UniqueMidiDurationDef.MsDuration = clmdds[0].MsDuration;
            clytemnestra.MsPosition = 0; // sets all the positions
            for(int verse = 2; verse <= 5; ++verse)
            {
                int interludeIndex = clytemnestra.LocalMidiDurationDefs.FindIndex(
                    x => (x.UniqueMidiDurationDef is UniqueMidiRestDef 
                          && x.MsDuration == tempInterludeMsDuration));
                clmdds[interludeIndex].MsDuration = verseMsPositions[verse - 1] - clmdds[interludeIndex].MsPosition;
                clmdds[interludeIndex].UniqueMidiDurationDef.MsDuration = clmdds[interludeIndex].MsDuration;
                clytemnestra.MsPosition = 0; // sets all the positions
            }
            int lastIndex = clmdds.Count - 1;
            clmdds[lastIndex].MsDuration = bassWind.EndMsPosition - clmdds[lastIndex].MsPosition;
            clmdds[lastIndex].UniqueMidiDurationDef.MsDuration = clmdds[lastIndex].MsDuration;
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

        /// <summary>
        /// These barlines do not include the barlines at the beginning, middle or end of Clytemnestra's verses.
        /// </summary>
        /// <param name="bassWind"></param>
        /// <param name="barlineMsPositions"></param>
        /// <returns></returns>
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

        private List<Voice> GetVoices(List<VoiceDef> voiceDefs)
        {
            byte channelIndex = 0;
            List<Voice> voices = new List<Voice>();

            foreach(VoiceDef voiceDef in voiceDefs)
            {
                Voice voice = new Voice(null, channelIndex++);
                voice.LocalMidiDurationDefs = voiceDef.LocalMidiDurationDefs;
                voices.Add(voice);
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
