using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;
using Moritz.Algorithm;
using Moritz.Palettes;
using Moritz.Spec;

namespace Moritz.Algorithm.Study2
{
    public class Study2Algorithm : CompositionAlgorithm
    {
        public Study2Algorithm()
            : base()
        {
            CheckParameters();
        }

        public override IReadOnlyList<int> MidiChannelIndexPerOutputVoice { get { return new List<int>() { 0, 1, 2 }; } }
        public override int NumberOfInputVoices { get { return 0; } }
        public override int NumberOfBars { get { return 74; } }

        /// <summary>
        /// See CompositionAlgorithm.DoAlgorithm()
        /// </summary>
        public override List<Bar> DoAlgorithm(List<Krystal> krystals, List<Palette> palettes)
        {
            _krystals = krystals;
            _palettes = palettes;

            List<Trk> sequentialStaff1Bars = WriteTopStaff();
            List<Trk> sequentialStaff2Bars = WriteLowerStaff(2, sequentialStaff1Bars);
            List<Trk> sequentialStaff3Bars = WriteLowerStaff(3, sequentialStaff1Bars);
            Debug.Assert((sequentialStaff1Bars.Count == sequentialStaff2Bars.Count) 
                      && (sequentialStaff1Bars.Count == sequentialStaff3Bars.Count));

            List<Bar> bars = new List<Bar>();
			int seqBarAbsMsPosition = 0;
            for(int barIndex = 0; barIndex < sequentialStaff1Bars.Count; ++barIndex)
            {
				List<Trk> trks = new List<Trk>()
				{
					sequentialStaff1Bars[barIndex],
					sequentialStaff2Bars[barIndex],
					sequentialStaff3Bars[barIndex]
				};
				Seq seq = new Seq(seqBarAbsMsPosition, trks, MidiChannelIndexPerOutputVoice);

				Bar bar = new Bar(seq);

				bars.Add(bar);

				seqBarAbsMsPosition += seq.MsDuration;
			}

			InsertClefChanges(bars);

            return bars;
        }

        protected override void InsertClefChanges(List<Bar> bars)
        {
            //VoiceDef voiceDef = bars[1][0];
            //voiceDef.InsertClefDef(1, "b");
        }

        private List<Trk> WriteTopStaff()
        {
            List<Trk> consecutiveBars = new List<Trk>();
            List<List<int>> dcValuesPerTopStaffBar = _krystals[0].GetValues(_krystals[0].Level);
            int msPosition = 0;
            for(int barIndex = 0; barIndex < dcValuesPerTopStaffBar.Count; barIndex++)
            {
                Trk trk = new Trk(0, 0, new List<IUniqueDef>());
                List<int> sequence = dcValuesPerTopStaffBar[barIndex];
                WriteDurationSymbolsForStrandInTopStaff(trk, barIndex, sequence, ref msPosition);
                consecutiveBars.Add(trk);
            }
            return consecutiveBars;
        }

        private void WriteDurationSymbolsForStrandInTopStaff(VoiceDef voice, int barIndex, List<int> originalStrandValues, ref int msPositionReFirstIUD)
        {
            Palette palette = _palettes[0]; // top templateDefs
            for(int valueIndex = 0; valueIndex < originalStrandValues.Count; valueIndex++)
            {
                int value = originalStrandValues[valueIndex];
                IUniqueDef noteDef = palette.UniqueDurationDef(value - 1);
                noteDef.MsPositionReFirstUD = msPositionReFirstIUD;
                msPositionReFirstIUD += noteDef.MsDuration;
                voice.UniqueDefs.Add(noteDef);
            }
        }

        private List<Trk> WriteLowerStaff(int staffNumber, List<Trk> topStaffBars)
        {
            List<Trk> consecutiveBars = new List<Trk>();
            Krystal krystal = _krystals[staffNumber - 1];
            Palette palette = _palettes[staffNumber - 1];
            List<List<int>> strandValuesList = krystal.GetValues(krystal.Level);
            Debug.Assert(topStaffBars.Count == strandValuesList.Count);

            for(int barIndex = 0; barIndex < strandValuesList.Count; barIndex++)
            {
                Trk topStaffTrk =  topStaffBars[barIndex];
				Trk newTrk = new Trk(staffNumber-1, 0, new List<IUniqueDef>());
                int currentMsPositionReFirstIUD = topStaffTrk.UniqueDefs[0].MsPositionReFirstUD;

                List<int> lowerStaffValueSequence = strandValuesList[barIndex];
                List<int> lowerStaffMsDurations = LowerStaffMsDurations(topStaffTrk, lowerStaffValueSequence.Count);
                for(int valueIndex = 0; valueIndex < lowerStaffValueSequence.Count; valueIndex++)
                {
                    int value = lowerStaffValueSequence[valueIndex];
                    IUniqueDef noteDef = palette.UniqueDurationDef(value - 1);
                    noteDef.MsDuration = lowerStaffMsDurations[valueIndex];
                    noteDef.MsPositionReFirstUD = currentMsPositionReFirstIUD;
                    currentMsPositionReFirstIUD += noteDef.MsDuration; 
                    newTrk.UniqueDefs.Add(noteDef);
                }

                consecutiveBars.Add(newTrk);
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
            List<int> actual1MsPositionsReFirstIUD = new List<int>();
            List<int> actual1MsDurations = new List<int>();
            foreach(IUniqueDef iumdd in topStaffVoice.UniqueDefs)
            {
                equal1MsPositions.Add(equal1MsPosition);
                equal1MsPosition += equal1MsDuration;

                actual1MsPositionsReFirstIUD.Add(iumdd.MsPositionReFirstUD);
                actual1MsDurations.Add(iumdd.MsDuration);
            }
            for(int i = 0; i < equal1MsPositions.Count; i++)
            {
                equal1MsPositions[i] += actual1MsPositionsReFirstIUD[0];
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
                equal2MsPositions[i] += actual1MsPositionsReFirstIUD[0];
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
                            actualStaff2MsPosition = actual1MsPositionsReFirstIUD[i] +
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
