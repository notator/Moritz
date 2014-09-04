using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;
using Moritz.Globals;
using Moritz.Krystals;
using Moritz.Score;
using Moritz.Score.Midi;
using Moritz.Score.Notation;
using Moritz.AssistantPerformer;

namespace Moritz.AssistantComposer
{
    /// <summary>
    /// Algorithm for testing Song 6's palettes.
    /// This may develope as composition progresses...
    /// </summary>
    public class Study2c3_1Algorithm : MidiCompositionAlgorithm
    {
        /// <summary>
        /// This constructor can be called with both parameters null,
        /// just to get the overridden properties.
        /// </summary>
        public Study2c3_1Algorithm(List<Krystal> krystals, List<PaletteDef> paletteDefs)
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
        /// The DoAlgorithm() function is special to a particular composition.
        /// The function returns a sequence of bar definitions. Each bar is a list of Voices (conceptually from top to bottom
        /// in a system, though the actual order can be changed in the Assistant Composer's options).
        /// Each bar in the sequence has the same number of Voices. Voices at the same position in each bar are continuations
        /// of the same overall voice, and may be concatenated later. OutputVoices at the same position in each bar have the
        /// same midi channel.
        /// Midi channels:
        /// By convention, algorithms use midi channels having indices which increase from top to bottom in the
        /// system, starting at 0. Midi channels may not occur twice in the same system. Each algorithm declares which midi
        /// channels it uses in the MidiChannels() function (see above). For an example, see Study2bAlgorithm.
        /// Each 'bar definition' is actually contained in the UniqueDefs list in each Voice (i.e. Voice.UniqueDefs).
        /// The Voice.NoteObjects lists are still empty when DoAlgorithm() returns.
        /// The Voice.UniqueDefs will be converted to NoteObjects having a specific notation later (in Notator.AddSymbolsToSystems()).
        /// ACHTUNG:
        /// The top (=first) Voice in each bar must be an OutputVoice.
        /// This can be followed by zero or more OutputVoices, followed by zero or more InputVoices.
        /// The chord definitions in OutputVoice.UniqueDefs must be UniqueMidiChordDefs.
        /// The chord definitions in InputVoice.UniqueDefs must be UniqueInputChordDefs.
        /// </summary>
        public override List<List<Voice>> DoAlgorithm()
        {
            List<Voice> sequentialStaff1Bars = WriteTopStaff();
            List<Voice> sequentialStaff2Bars = WriteLowerStaff(2, sequentialStaff1Bars);
            List<Voice> sequentialStaff3Bars = WriteLowerStaff(3, sequentialStaff1Bars);
            Debug.Assert((sequentialStaff1Bars.Count == sequentialStaff2Bars.Count) 
                      && (sequentialStaff1Bars.Count == sequentialStaff3Bars.Count));

            List<List<Voice>> bars = new List<List<Voice>>();
            for(int barIndex = 0; barIndex < sequentialStaff1Bars.Count; ++barIndex)
            {
                List<Voice> bar = new List<Voice>();
                bar.Add(sequentialStaff1Bars[barIndex]);
                bar.Add(sequentialStaff2Bars[barIndex]);
                bar.Add(sequentialStaff3Bars[barIndex]);
                bars.Add(bar);
            }

            Debug.Assert(bars.Count == NumberOfBars());

            return bars;
        }

        /// <summary>
        /// The number of bars produced by DoAlgorithm().
        /// </summary>
        /// <returns></returns>
        public override int NumberOfBars()
        {
            // this value was determined experimentally when running the algorithm 
            return 74;
        }

        private List<Voice> WriteTopStaff()
        {
            List<Voice> consecutiveBars = new List<Voice>();
            List<List<int>> dcValuesPerTopStaffBar = _krystals[0].GetValues(_krystals[0].Level);
            int msPosition = 0;
            for(int barIndex = 0; barIndex < dcValuesPerTopStaffBar.Count; barIndex++)
            {
                Voice voice = new OutputVoice(null, this.MidiChannels()[0]);
                List<int> sequence = dcValuesPerTopStaffBar[barIndex];
                WriteDurationSymbolsForStrandInTopStaff(voice, barIndex, sequence, ref msPosition);
                consecutiveBars.Add(voice);
            }
            return consecutiveBars;
        }

        private void WriteDurationSymbolsForStrandInTopStaff(Voice voice, int barIndex, List<int> originalStrandValues, ref int msPosition)
        {
            PaletteDef paletteDef = this._paletteDefs[0]; // top paletteDef
            for(int valueIndex = 0; valueIndex < originalStrandValues.Count; valueIndex++)
            {
                int value = originalStrandValues[valueIndex];
                DurationDef midiDurationDef = paletteDef[value - 1];
                IUniqueDef noteDef = midiDurationDef.DeepClone();
                Debug.Assert(midiDurationDef.MsDuration > 0);
                Debug.Assert(noteDef.MsDuration == midiDurationDef.MsDuration);
                noteDef.MsPosition = msPosition;
                msPosition += noteDef.MsDuration;
                voice.UniqueDefs.Add(noteDef);
            }
        }

        private List<Voice> WriteLowerStaff(int staffNumber, List<Voice> topStaffBars)
        {
            List<Voice> consecutiveBars = new List<Voice>();
            Krystal krystal = _krystals[staffNumber - 1];
            PaletteDef paletteDef = _paletteDefs[staffNumber - 1];
            List<List<int>> strandValuesList = krystal.GetValues(krystal.Level);
            Debug.Assert(topStaffBars.Count == strandValuesList.Count);

            for(int barIndex = 0; barIndex < strandValuesList.Count; barIndex++)
            {
                Voice topStaffVoice =  topStaffBars[barIndex];
                Voice newVoice = new OutputVoice(null, MidiChannels()[staffNumber - 1]);
                int currentMsPosition = topStaffVoice.UniqueDefs[0].MsPosition;

                List<int> lowerStaffValueSequence = strandValuesList[barIndex];
                List<int> lowerStaffMsDurations = LowerStaffMsDurations(topStaffVoice, lowerStaffValueSequence.Count);
                for(int valueIndex = 0; valueIndex < lowerStaffValueSequence.Count; valueIndex++)
                {
                    int value = lowerStaffValueSequence[valueIndex];
                    DurationDef midiDurationDef = paletteDef[value - 1];
                    midiDurationDef.MsDuration = lowerStaffMsDurations[valueIndex];
                    IUniqueDef noteDef = midiDurationDef.DeepClone();
                    noteDef.MsPosition = currentMsPosition;
                    currentMsPosition += midiDurationDef.MsDuration; 
                    newVoice.UniqueDefs.Add(noteDef);
                }

                consecutiveBars.Add(newVoice);
            }
            return consecutiveBars;
        }

        private List<int> LowerStaffMsDurations(Voice topStaffVoice, int numberOfDurationSymbolsToConstruct)
        {
            #region get topStaffVoice durations and positions
            int voiceMsDuration = 0;
            int numberOfTopDurations = 0;
            foreach(IUniqueDef iumdd in topStaffVoice.UniqueDefs)
            {
                voiceMsDuration += iumdd.MsDuration;
                numberOfTopDurations++;
            }
            Debug.Assert(numberOfTopDurations > 0);

            int equal1MsDuration = voiceMsDuration / numberOfTopDurations;
            List<int> equal1MsPositions = new List<int>();
            int equal1MsPosition = 0;
            List<int> actual1MsPositions = new List<int>();
            List<int> actual1MsDurations = new List<int>();
            foreach(IUniqueDef iumdd in topStaffVoice.UniqueDefs)
            {
                equal1MsPositions.Add(equal1MsPosition);
                equal1MsPosition += equal1MsDuration;

                actual1MsPositions.Add(iumdd.MsPosition);
                actual1MsDurations.Add(iumdd.MsDuration);
            }
            for(int i = 0; i < equal1MsPositions.Count; i++)
            {
                equal1MsPositions[i] += actual1MsPositions[0];
            }
            int followingBarlinePosition = equal1MsPositions[0] + voiceMsDuration;
            #endregion

            #region get the (hypothetical) equal durations and positions in newVoice
            Debug.Assert(numberOfDurationSymbolsToConstruct > 0);

            int equal2MsDuration = voiceMsDuration / numberOfDurationSymbolsToConstruct;
            List<int> equal2MsPositions = new List<int>();
            int equal2MsPosition = 0;
            for(int i = 0; i < numberOfDurationSymbolsToConstruct; i++)
            {
                equal2MsPositions.Add(equal2MsPosition);
                equal2MsPosition += equal2MsDuration;
            }
            for(int i = 0; i < equal2MsPositions.Count; i++)
            {
                equal2MsPositions[i] += actual1MsPositions[0];
            }
            #endregion

            #region get newVoice MsPositions
            List<int> actualStaff2MsPositions = new List<int>();
            int actualStaff2MsPosition = 0;
            if(equal1MsPositions.Count == 1)
            {
                actualStaff2MsPositions = equal2MsPositions;
            }
            else
            {
                foreach(int e2MsPosition in equal2MsPositions)
                {
                    int upperLimit = 0;
                    for(int i = 0; i < equal1MsPositions.Count; i++)
                    {
                        if(i == (equal1MsPositions.Count - 1))
                            upperLimit = followingBarlinePosition;
                        else upperLimit = equal1MsPositions[i + 1];
                        if(e2MsPosition < upperLimit && e2MsPosition >= equal1MsPositions[i])
                        {
                            actualStaff2MsPosition = actual1MsPositions[i] +
                                (((e2MsPosition - equal1MsPositions[i]) * actual1MsDurations[i]) / equal1MsDuration);
                            break;
                        }
                    }
                    actualStaff2MsPositions.Add(actualStaff2MsPosition);
                }
            }
            Debug.Assert(actualStaff2MsPositions.Count == numberOfDurationSymbolsToConstruct);
            #endregion

            #region set newVoice MsDurations
            List<int> lowerStaffMsDurations = new List<int>();
            for(int i = 1; i < numberOfDurationSymbolsToConstruct; i++)
            {
                lowerStaffMsDurations.Add(actualStaff2MsPositions[i] - actualStaff2MsPositions[i - 1]);
            }
            lowerStaffMsDurations.Add(followingBarlinePosition - actualStaff2MsPositions[numberOfDurationSymbolsToConstruct - 1]);
            #endregion
            return lowerStaffMsDurations;
        }

    }
}
