using System;
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
        public override IReadOnlyList<int> MidiChannelIndexPerInputVoice { get { return null; } }
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

			List<int> barlineMsPositions = GetBarlinePositions(sequentialStaff1Bars);

			Trk mainTrk1 = GetMainTrk(sequentialStaff1Bars);
			Trk mainTrk2 = GetMainTrk(sequentialStaff2Bars);
			Trk mainTrk3 = GetMainTrk(sequentialStaff3Bars);

			List<Trk> trks = new List<Trk>()
				{
					mainTrk1,
					mainTrk2,
					mainTrk3
				};

			Seq mainSeq = new Seq(0, trks, MidiChannelIndexPerOutputVoice);

			List<Bar> bars = GetBars(mainSeq, null, barlineMsPositions, null, null);

			return bars;
        }

		/// <summary>
		/// Returns the concatenation of the argument Trks, with rests agglommerated.
		/// </summary>
		/// <param name="trkSequence"></param>
		/// <returns></returns>
		private Trk GetMainTrk(List<Trk> trkSequence)
		{
			Trk mainTrk = new Trk(trkSequence[0].MidiChannel);
			foreach(Trk trk in trkSequence)
			{
				mainTrk.AddRange(trk); // agglommerates rests
			}
			return mainTrk;
		}

		private List<int> GetBarlinePositions(List<Trk> sequentialStaff1Bars)
		{
			var barlinePositions = new List<int>();
			int msPos = 0;
			foreach(Trk trk in sequentialStaff1Bars)
			{
				msPos += trk.MsDuration;
				barlinePositions.Add(msPos);
			}
			return barlinePositions;
		}

		/// <summary>
		/// This function returns null or a SortedDictionary per VoiceDef in each bar.
		/// The dictionary contains the index at which the clef will be inserted in the VoiceDef's IUniquedefs,
		/// and the clef ID string ("t", "t1", "b3" etc.).
		/// Clefs will be inserted in reverse order of the Sorted dictionary, so that the indices are those of
		/// the existing IUniqueDefs before which the clef will be inserted.
		/// The SortedDictionaries should not contain tne initial clefs per voicedef - those will be included
		/// automatically.
		/// Note that a CautionaryChordDef counts as an IUniqueDef at the beginning of a bar, and that clefs
		/// cannot be inserted in front of them.
		/// </summary>
		protected override List<List<SortedDictionary<int, string>>> GetClefChangesPerBar(int nBars)
		{
			return null;
			// test code...
			//VoiceDef voiceDef1 = bars[0][1];
			//voiceDef1.InsertClefDef(9, "b3");
			//voiceDef1.InsertClefDef(8, "b2");
			//voiceDef1.InsertClefDef(7, "b1");
			//voiceDef1.InsertClefDef(6, "b");
			//voiceDef1.InsertClefDef(5, "t3");
			//voiceDef1.InsertClefDef(4, "t2");
			//voiceDef1.InsertClefDef(3, "t1");
			//voiceDef1.InsertClefDef(2, "t");
		}

		/// <summary>
		/// This function returns null or  a SortedDictionary per VoiceDef in each bar.
		/// The dictionary contains the index of the IUniqueDef in the barat which the clef will be inserted in the VoiceDef's IUniquedefs,
		/// and the clef ID string ("t", "t1", "b3" etc.).
		/// Clefs will be inserted in reverse order of the Sorted dictionary, so that the indices are those of
		/// the existing IUniqueDefs before which the clef will be inserted.
		/// The SortedDictionaries should not contain tne initial clefs per voicedef - those will be included
		/// automatically.
		/// Note that both Clefs and a CautionaryChordDef at the beginning of a bar count as IUniqueDefs for
		/// indexing purposes, and that lyrics cannot be attached to them.
		/// </summary>
		protected override List<List<SortedDictionary<int, string>>> GetLyricsPerBar(int nBars)
		{
			return null;
			// test code...
			//VoiceDef voiceDef0 = bars[0][0];
			//MidiChordDef mcd1 = voiceDef0[2] as MidiChordDef;
			//mcd1.Lyric = "lyric1";
			//MidiChordDef mcd2 = voiceDef0[3] as MidiChordDef;
			//mcd2.Lyric = "lyric2";
			//MidiChordDef mcd3 = voiceDef0[4] as MidiChordDef;
			//mcd3.Lyric = "lyric3";
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
