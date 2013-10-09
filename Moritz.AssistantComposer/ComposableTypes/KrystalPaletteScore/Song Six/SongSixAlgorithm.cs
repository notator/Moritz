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
            return new List<byte>() { 0, 1, 2, 3 };
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
            // The rootWind is the lowest wind. The winds are numbered from top to bottom in the score.
            VoiceDef rootWind = new VoiceDef(_paletteDefs[0], _krystals[2]);

            AlignClytemnestraToRootWind(clytemnestra, rootWind, tempInterludeMsDuration);

            // barlineMsPositions contains both the position of bar 1 (0ms) and the position of the final barline
            List<int> barlineMsPositions = GetBarlineMsPositions(clytemnestra, rootWind);
            Debug.Assert(barlineMsPositions.Count == NumberOfBars() + 1); // includes bar 1 (mPos=0) and the final barline.

            // the penultimate wind. If rootWind is wind 2, then this is wind 1.
            VoiceDef tenorWind = GetTenorWind(rootWind, barlineMsPositions);
            
            // Complete the winds and birds.

            VoiceDef control = GetControlVoiceDef(clytemnestra, tenorWind, rootWind);

            #region code for testing VoiceDef functions
            //bassWind.SetContour(11, new List<int>() { 1, 4, 1, 2 }, 1, 1);
            //bassWind.Translate(15, 4, 16);
            // TODO:
            // Cut, Copy, PasteAt (List<LocalMididurationDefs>) !!
            #endregion

            // Add each voiceDef to voiceDefs here, in top to bottom (=channelIndex) order in the score.
            List<VoiceDef> voiceDefs = new List<VoiceDef>() {control, clytemnestra, tenorWind, rootWind /* etc.*/};
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

        /// <summary>
        /// Aligns Clytemnestra's verses to MsPositions in the rootWind (the lowest wind)
        /// </summary>
        /// <param name="clytemnestra"></param>
        /// <param name="rootWind"></param>
        /// <param name="tempInterludeMsDuration"></param>
        private void AlignClytemnestraToRootWind(Clytemnestra clytemnestra, VoiceDef rootWind, int tempInterludeMsDuration)
        {
            List<int> verseMsPositions = new List<int>();
            verseMsPositions.Add(rootWind[8].MsPosition);
            verseMsPositions.Add(rootWind[20].MsPosition);
            verseMsPositions.Add(rootWind[33].MsPosition);
            verseMsPositions.Add(rootWind[49].MsPosition);
            verseMsPositions.Add(rootWind[70].MsPosition);

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
            clmdds[lastIndex].MsDuration = rootWind.EndMsPosition - clmdds[lastIndex].MsPosition;
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

        private VoiceDef GetTenorWind(VoiceDef bassWind, List<int> barMsPositions)
        {
            VoiceDef wind4 = GetBasicTenorWind(bassWind, barMsPositions);
            wind4.Transpose(7); // the basic pitch
            wind4.AlignObjectAtIndex(0, 10, 82, barMsPositions[6]);
            wind4.AlignObjectAtIndex(10, 16, 82, barMsPositions[20]);
            wind4.AlignObjectAtIndex(16, 57, 82, barMsPositions[82]);

            //// now create a gliss from bar 61 to 83            
            //int bar61MsPosition = barMsPositions[60];
            //int startGlissIndex = wind4.FirstIndexAtOrAfterMsPos(bar61MsPosition);
            //int bar83MsPosition = barMsPositions[82];
            //int endGlissIndex = wind4.FirstIndexAtOrAfterMsPos(bar83MsPosition) - 1; // 57
            //int glissInterval = 19;
            //wind4.StepwiseGliss(startGlissIndex, endGlissIndex, glissInterval);

            //// from bar 83 to the end is constant (19 semitones higher than before)

            //for(int index = endGlissIndex + 1; index < wind4.Count; ++index)
            //{
            //    wind4[index].Transpose(19);
            //}
            return wind4;
        }
        /// <summary>
        /// Returns a VoiceDef containing clones of the LocalMidiDurationDefs in the originalVoiceDef
        /// argument, rotated so that the original first LocalMidiDurationDef is positioned at bar 83.
        /// The LocalMidiDurationDefs before bar 83 are stretched to fit. 
        /// The LocalMidiDurationDefs after bar 83 are compressed to fit. 
        /// </summary>
        /// <param name="originalVoiceDef"></param>
        /// <returns></returns>
        private VoiceDef GetBasicTenorWind(VoiceDef originalVoiceDef, List<int> barlineMsPositions)
        {
            VoiceDef tempWind = originalVoiceDef.Clone();
            int finalBarlineMsPosition = barlineMsPositions[barlineMsPositions.Count - 1];
            int msDurationAfterSynch = finalBarlineMsPosition - barlineMsPositions[82];

            List<LocalMidiDurationDef> originalLmdds = tempWind.LocalMidiDurationDefs;
            List<LocalMidiDurationDef> originalStartLmdds = new List<LocalMidiDurationDef>();
            List<LocalMidiDurationDef> wind4Lmdds = new List<LocalMidiDurationDef>();
            int accumulatingMsDuration = 0;
            for(int i = 0; i < tempWind.Count; ++i)
            {
                if(accumulatingMsDuration <= msDurationAfterSynch)
                {
                    originalStartLmdds.Add(originalLmdds[i]);
                    accumulatingMsDuration += originalLmdds[i].MsDuration;
                }
                else
                {
                    wind4Lmdds.Add(originalLmdds[i]);
                }
            }
            wind4Lmdds.AddRange(originalStartLmdds);

            int msPosition = 0;
            foreach(LocalMidiDurationDef lmdd in wind4Lmdds)
            {
                lmdd.MsPosition = msPosition;
                msPosition += lmdd.MsDuration;
            }
            VoiceDef wind4 = new VoiceDef(wind4Lmdds);

            return wind4;
        }

        private VoiceDef GetControlVoiceDef(Clytemnestra clytemnestra, VoiceDef tenorWind, VoiceDef bassWind)
        {
            int finalRestMsDuration = 100;

            UniqueMidiChordDef umcd = new UniqueMidiChordDef(new List<byte>() { (byte) 67 }, new List<byte>() { (byte) 0 }, bassWind.EndMsPosition, false, new List<MidiControl>());
            LocalMidiDurationDef lmChordd = new LocalMidiDurationDef(umcd, 0, bassWind.EndMsPosition - finalRestMsDuration);

            LocalMidiDurationDef lmRestd = new LocalMidiDurationDef(finalRestMsDuration);
            lmRestd.MsPosition = lmChordd.MsDuration;

            List<LocalMidiDurationDef> controlLmdds = new List<LocalMidiDurationDef>();
            controlLmdds.Add(lmChordd);
            controlLmdds.Add(lmRestd);
            VoiceDef controlVoiceDef = new VoiceDef(controlLmdds);
            // here, controlVoiceDef should contains a single note followed by a single rest having the duration of the piece.

            return controlVoiceDef;
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
