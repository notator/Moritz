using System.Collections.Generic;
using System.Diagnostics;

using Moritz.Score;
using Moritz.Score.Midi;
using Krystals4ObjectLibrary;

namespace Moritz.AssistantComposer
{
    /// <summary>
    /// Algorithm for testing Song 6's palettes.
    /// This may develope as composition progresses...
    /// </summary>
    public class Study2cAlgorithm : MidiCompositionAlgorithm
    {
        /// <summary>
        /// This constructor can be called with both parameters null,
        /// just to get the overridden properties.
        /// </summary>
        public Study2cAlgorithm(List<Krystal> krystals, List<PaletteDef> paletteDefs)
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
                Voice voice = new Voice(null, this.MidiChannels()[0]);
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
                MidiDurationDef midiDurationDef = paletteDef[value - 1];
                LocalizedMidiDurationDef noteDef = new LocalizedMidiDurationDef(midiDurationDef);
                Debug.Assert(midiDurationDef.MsDuration > 0);
                Debug.Assert(noteDef.MsDuration == midiDurationDef.MsDuration);
                noteDef.MsPosition = msPosition;
                msPosition += noteDef.MsDuration;
                voice.LocalizedMidiDurationDefs.Add(noteDef);
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
                Voice newVoice = new Voice(null, MidiChannels()[staffNumber - 1]);
                int currentMsPosition = topStaffVoice.LocalizedMidiDurationDefs[0].MsPosition;

                List<int> lowerStaffValueSequence = strandValuesList[barIndex];
                List<int> lowerStaffMsDurations = LowerStaffMsDurations(topStaffVoice, lowerStaffValueSequence.Count);
                for(int valueIndex = 0; valueIndex < lowerStaffValueSequence.Count; valueIndex++)
                {
                    int value = lowerStaffValueSequence[valueIndex];
                    MidiDurationDef midiDurationDef = paletteDef[value - 1];
                    midiDurationDef.MsDuration = lowerStaffMsDurations[valueIndex];
                    LocalizedMidiDurationDef noteDef = new LocalizedMidiDurationDef(midiDurationDef);
                    noteDef.MsPosition = currentMsPosition;
                    currentMsPosition += midiDurationDef.MsDuration; 
                    newVoice.LocalizedMidiDurationDefs.Add(noteDef);
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
            foreach(LocalizedMidiDurationDef lmdd in topStaffVoice.LocalizedMidiDurationDefs)
            {
                voiceMsDuration += lmdd.MsDuration;
                numberOfTopDurations++;
            }
            Debug.Assert(numberOfTopDurations > 0);

            int equal1MsDuration = voiceMsDuration / numberOfTopDurations;
            List<int> equal1MsPositions = new List<int>();
            int equal1MsPosition = 0;
            List<int> actual1MsPositions = new List<int>();
            List<int> actual1MsDurations = new List<int>();
            foreach(LocalizedMidiDurationDef lmdd in topStaffVoice.LocalizedMidiDurationDefs)
            {
                equal1MsPositions.Add(equal1MsPosition);
                equal1MsPosition += equal1MsDuration;

                actual1MsPositions.Add(lmdd.MsPosition);
                actual1MsDurations.Add(lmdd.MsDuration);
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
