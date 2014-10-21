using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;
using Moritz.Algorithm;
using Moritz.Palettes;
using Moritz.Spec;

namespace Moritz.Algorithm.Study2c3_1
{
    /// <summary>
    /// Algorithm for testing Song 6's palettes.
    /// This may develope as composition progresses...
    /// </summary>
    public class Study2c3_1Algorithm : CompositionAlgorithm
    {
        /// <summary>
        /// This constructor can be called with both parameters null,
        /// just to get the overridden properties.
        /// </summary>
        public Study2c3_1Algorithm(List<Krystal> krystals, List<Palette> palettes)
            : base(krystals, palettes)
        {
        }

        public override int NumberOfInputVoices { get { return 0; } }
        public override int NumberOfOutputVoices { get { return 3; } }
        public override int NumberOfBars { get { return 74; } }

        /// <summary>
        /// See CompositionAlgorithm.DoAlgorithm()
        /// </summary>
        public override List<List<VoiceDef>> DoAlgorithm()
        {
            List<VoiceDef> sequentialStaff1Bars = WriteTopStaff();
            List<VoiceDef> sequentialStaff2Bars = WriteLowerStaff(2, sequentialStaff1Bars);
            List<VoiceDef> sequentialStaff3Bars = WriteLowerStaff(3, sequentialStaff1Bars);
            Debug.Assert((sequentialStaff1Bars.Count == sequentialStaff2Bars.Count) 
                      && (sequentialStaff1Bars.Count == sequentialStaff3Bars.Count));

            List<List<VoiceDef>> bars = new List<List<VoiceDef>>();
            for(int barIndex = 0; barIndex < sequentialStaff1Bars.Count; ++barIndex)
            {
                List<VoiceDef> bar = new List<VoiceDef>();
                bar.Add(sequentialStaff1Bars[barIndex]);
                bar.Add(sequentialStaff2Bars[barIndex]);
                bar.Add(sequentialStaff3Bars[barIndex]);
                bars.Add(bar);
            }

            Debug.Assert(bars.Count == NumberOfBars);

            List<byte> masterVolumes = new List<byte>() { 100, 100, 100 };
            base.SetOutputVoiceMasterVolumes(bars[0], masterVolumes);

            return bars;
        }

        private List<VoiceDef> WriteTopStaff()
        {
            List<VoiceDef> consecutiveBars = new List<VoiceDef>();
            List<List<int>> dcValuesPerTopStaffBar = _krystals[0].GetValues(_krystals[0].Level);
            int msPosition = 0;
            for(int barIndex = 0; barIndex < dcValuesPerTopStaffBar.Count; barIndex++)
            {
                VoiceDef voice = new OutputVoiceDef();
                List<int> sequence = dcValuesPerTopStaffBar[barIndex];
                WriteDurationSymbolsForStrandInTopStaff(voice, barIndex, sequence, ref msPosition);
                consecutiveBars.Add(voice);
            }
            return consecutiveBars;
        }

        private void WriteDurationSymbolsForStrandInTopStaff(VoiceDef voice, int barIndex, List<int> originalStrandValues, ref int msPosition)
        {
            Palette palette = _palettes[0]; // top templateDefs
            for(int valueIndex = 0; valueIndex < originalStrandValues.Count; valueIndex++)
            {
                int value = originalStrandValues[valueIndex];
                IUniqueDef noteDef = palette.UniqueDurationDef(value - 1);
                noteDef.MsPosition = msPosition;
                msPosition += noteDef.MsDuration;
                voice.UniqueDefs.Add(noteDef);
            }
        }

        private List<VoiceDef> WriteLowerStaff(int staffNumber, List<VoiceDef> topStaffBars)
        {
            List<VoiceDef> consecutiveBars = new List<VoiceDef>();
            Krystal krystal = _krystals[staffNumber - 1];
            Palette palette = _palettes[staffNumber - 1];
            List<List<int>> strandValuesList = krystal.GetValues(krystal.Level);
            Debug.Assert(topStaffBars.Count == strandValuesList.Count);

            for(int barIndex = 0; barIndex < strandValuesList.Count; barIndex++)
            {
                VoiceDef topStaffVoice =  topStaffBars[barIndex];
                VoiceDef newVoice = new OutputVoiceDef();
                int currentMsPosition = topStaffVoice.UniqueDefs[0].MsPosition;

                List<int> lowerStaffValueSequence = strandValuesList[barIndex];
                List<int> lowerStaffMsDurations = LowerStaffMsDurations(topStaffVoice, lowerStaffValueSequence.Count);
                for(int valueIndex = 0; valueIndex < lowerStaffValueSequence.Count; valueIndex++)
                {
                    int value = lowerStaffValueSequence[valueIndex];
                    IUniqueDef noteDef = palette.UniqueDurationDef(value - 1);
                    noteDef.MsDuration = lowerStaffMsDurations[valueIndex];
                    noteDef.MsPosition = currentMsPosition;
                    currentMsPosition += noteDef.MsDuration; 
                    newVoice.UniqueDefs.Add(noteDef);
                }

                consecutiveBars.Add(newVoice);
            }
            return consecutiveBars;
        }

        private List<int> LowerStaffMsDurations(VoiceDef topStaffVoice, int numberOfDurationSymbolsToConstruct)
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
