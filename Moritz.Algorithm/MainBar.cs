using System;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;

namespace Moritz.Algorithm
{
    public class MainBar : Bar
    {
		/// <summary>
		/// Used only by functions in this class.
		/// </summary>
        private MainBar()
			: base()
		{
		}

		/// <summary>
		/// A MainBar contains a list of voiceDefs, that can be of both kinds: Trks and InputVoiceDefs. A Seq can only contain Trks.
		/// As with all Bars, a MainBar does not contain barlines. They are implicit, at the beginning and end of the MainBar.
		/// This constructor uses its arguments' voiceDefs directly, so if the arguments need to be used again, pass clones.
		/// <para>MainBar consistency is identical to Bar consistency, with the further restrictions that AbsMsPosition must be 0
		/// and initalClefPerChannel may not be null.</para>
		/// <para>For further documentation about MainBar and Bar consistency, see their private AssertConsistency() functions.
		/// </summary>
		/// <param name="seq">Cannot be null, and must have Trks</param>
		/// <param name="inputVoiceDefs">This list can be null or empty</param>
		/// <param name="initialClefPerChannel">Cannot be null. Count must be seq.Trks.Count + inputVoiceDefs.Count</param>>
		public MainBar(Seq seq, IReadOnlyList<InputVoiceDef> inputVoiceDefs, List<string> initialClefPerChannel)
			: base(seq, inputVoiceDefs, initialClefPerChannel)
		{
			AssertConsistency(initialClefPerChannel);
		}

		private void AssertConsistency(List<string> initialClefPerChannel)
		{
			Debug.Assert(AbsMsPosition == 0);
			Debug.Assert(InitialClefPerChannel != null);
		}

		/// Converts this MainBar to a list of bars, consuming this bar's voiceDefs.
		/// Uses the argument barline msPositions as the EndBarlines of the returned bars (which don't contain barlines).
		/// An exception is thrown if:
		///    1) the first argument value is less than or equal to 0.
		///    2) the argument contains duplicate msPositions.
		///    3) the argument is not in ascending order.
		///    4) a Trk.MsPositionReContainer is not 0.
		///    5) an msPosition is not the endMsPosition of any IUniqueDef in the seq.
		public List<Bar> GetBars(List<int> barlineMsPositions)
		{
			CheckBarlineMsPositions(barlineMsPositions);

			List<int> barMsDurations = new List<int>();
			int startMsPos = 0;
			for(int i = 0; i < barlineMsPositions.Count; i++)
			{
				int endMsPos = barlineMsPositions[i];
				barMsDurations.Add(endMsPos - startMsPos);
				startMsPos = endMsPos;
			}

			List<Bar> bars = new List<Bar>();
			int totalDurationBeforePop = this.MsDuration;
			foreach(int barMsDuration in barMsDurations)
			{
				Bar poppedBar = PopBar(barMsDuration);
				Debug.Assert(poppedBar.MsDuration == barMsDuration);
				if(poppedBar != this)
				{
					Debug.Assert(poppedBar.MsDuration + this.MsDuration == totalDurationBeforePop);
				}
				else
				{
					Debug.Assert(poppedBar.MsDuration == totalDurationBeforePop);
				}
				totalDurationBeforePop = this.MsDuration;

				bars.Add(poppedBar);
			}

			return bars;
		}

		/// <summary>
		/// Creates a bar containing a list of voiceDefs containing the IUniqueDefs that
		/// begin within barDuration, and removes these IUniqueDefs from the current bar.
		/// MidiRestDefs and MidiChordDefs are split as necessary, so that when this
		/// function returns, both the popped bar and the current bar contain voiceDefs
		/// having the same msDuration. i.e.: Both the popped bar and the remaining bar "add up".
		/// </summary>
		/// <param name="barMsDuration"></param>
		/// <returns>The popped bar</returns>
		private MainBar PopBar(int barMsDuration)
		{
			Debug.Assert(barMsDuration > 0);

			if(barMsDuration == this.MsDuration)
			{
				return this;
			}

			MainBar poppedBar = new MainBar();
			MainBar remainingBar = new MainBar() ;
			int thisMsDuration = this.MsDuration;

			VoiceDef poppedBarVoice;
			VoiceDef remainingBarVoice;
			foreach(VoiceDef voiceDef in _voiceDefs)
			{
				Trk outputVoice = voiceDef as Trk;
				InputVoiceDef inputVoice = voiceDef as InputVoiceDef;
				if(outputVoice != null)
				{
					poppedBarVoice = new Trk(outputVoice.MidiChannel) { Container = poppedBar };
					poppedBar.VoiceDefs.Add(poppedBarVoice);
					remainingBarVoice = new Trk(outputVoice.MidiChannel) { Container = remainingBar };
					remainingBar.VoiceDefs.Add(remainingBarVoice);
				}
				else
				{
					poppedBarVoice = new InputVoiceDef(inputVoice.MidiChannel, 0, new List<IUniqueDef>()) { Container = poppedBar };
					poppedBar.VoiceDefs.Add(poppedBarVoice);
					remainingBarVoice = new InputVoiceDef(inputVoice.MidiChannel, 0, new List<IUniqueDef>()) { Container = remainingBar };
					remainingBar.VoiceDefs.Add(remainingBarVoice);
				}
				foreach(IUniqueDef iud in voiceDef.UniqueDefs)
				{
					int iudMsDuration = iud.MsDuration;
					int iudStartPos = iud.MsPositionReFirstUD;
					int iudEndPos = iudStartPos + iudMsDuration;

					if(iudStartPos >= barMsDuration)
					{
						if(iud is ClefDef && iudStartPos == barMsDuration)
						{
							poppedBarVoice.UniqueDefs.Add(iud);
						}
						else
						{
							remainingBarVoice.UniqueDefs.Add(iud);
						}
					}
					else if(iudEndPos > barMsDuration)
					{
						int durationBeforeBarline = barMsDuration - iudStartPos;
						int durationAfterBarline = iudEndPos - barMsDuration;
						if(iud is MidiRestDef)
						{
							// This is a rest. Split it.
							MidiRestDef firstRestHalf = new MidiRestDef(iudStartPos, durationBeforeBarline);
							poppedBarVoice.UniqueDefs.Add(firstRestHalf);

							MidiRestDef secondRestHalf = new MidiRestDef(barMsDuration, durationAfterBarline);
							remainingBarVoice.UniqueDefs.Add(secondRestHalf);
						}
						if(iud is InputRestDef)
						{
							// This is a rest. Split it.
							InputRestDef firstRestHalf = new InputRestDef(iudStartPos, durationBeforeBarline);
							poppedBarVoice.UniqueDefs.Add(firstRestHalf);

							InputRestDef secondRestHalf = new InputRestDef(barMsDuration, durationAfterBarline);
							remainingBarVoice.UniqueDefs.Add(secondRestHalf);
						}
						else if(iud is CautionaryChordDef)
						{
							Debug.Assert(false, "There shouldnt be any cautionary chords here.");
							// This is a cautionary chord. Set the position of the following barline, and
							// Add a CautionaryChordDef at the beginning of the following bar.
							iud.MsDuration = barMsDuration - iudStartPos;
							poppedBarVoice.UniqueDefs.Add(iud);

							Debug.Assert(remainingBarVoice.UniqueDefs.Count == 0);
							CautionaryChordDef secondLmdd = new CautionaryChordDef((IUniqueChordDef)iud, 0, durationAfterBarline);
							remainingBarVoice.UniqueDefs.Add(secondLmdd);
						}
						else if(iud is MidiChordDef || iud is InputChordDef)
						{
							IUniqueSplittableChordDef uniqueChordDef = iud as IUniqueSplittableChordDef;
							uniqueChordDef.MsDurationToNextBarline = durationBeforeBarline;
							poppedBarVoice.UniqueDefs.Add(uniqueChordDef);

							Debug.Assert(remainingBarVoice.UniqueDefs.Count == 0);
							CautionaryChordDef ccd = new CautionaryChordDef(uniqueChordDef, 0, durationAfterBarline);
							remainingBarVoice.UniqueDefs.Add(ccd);
						}
					}
					else
					{
						Debug.Assert(iudEndPos <= barMsDuration && iudStartPos >= 0);
						poppedBarVoice.UniqueDefs.Add(iud);
					}
				}
			}

			this._voiceDefs = remainingBar.VoiceDefs;
			poppedBar.AbsMsPosition = this.AbsMsPosition;
			this.AbsMsPosition += barMsDuration;

			SetMsPositionsReFirstUD();

			AssertConsistency();
			poppedBar.AssertConsistency();

			return poppedBar;
		}

		/// <summary>
		/// An exception is thrown if:
		///    1) the first argument value is less than or equal to 0.
		///    2) the argument contains duplicate msPositions.
		///    3) the argument is not in ascending order.
		///    4) a VoiceDef.MsPositionReContainer is not 0.
		///    5) an msPosition is not the endMsPosition of any IUniqueDef in the bar.
		/// </summary>
		private void CheckBarlineMsPositions(IReadOnlyList<int> barlineMsPositionsReThisBar)
		{
			Debug.Assert(barlineMsPositionsReThisBar[0] > 0, "The first msPosition must be greater than 0.");

			int msDuration = this.MsDuration;
			for(int i = 0; i < barlineMsPositionsReThisBar.Count; ++i)
			{
				int msPosition = barlineMsPositionsReThisBar[i];
				Debug.Assert(msPosition <= this.MsDuration);
				for(int j = i + 1; j < barlineMsPositionsReThisBar.Count; ++j)
				{
					Debug.Assert(msPosition != barlineMsPositionsReThisBar[j], "Error: Duplicate barline msPositions.");
				}
			}

			int currentMsPos = -1;
			foreach(int msPosition in barlineMsPositionsReThisBar)
			{
				Debug.Assert(msPosition > currentMsPos, "Value out of order.");
				currentMsPos = msPosition;
				bool found = false;
				foreach(VoiceDef voiceDef in VoiceDefs)
				{
					Debug.Assert(voiceDef.MsPositionReContainer == 0);
					foreach(IUniqueDef iud in voiceDef.UniqueDefs)
					{
						if(msPosition == (iud.MsPositionReFirstUD + iud.MsDuration))
						{
							found = true;
							break;
						}
					}
					if(found)
					{ 
						break;
					}
				}
				Debug.Assert(found == true, "Error: barline must be at the endMsPosition of at least one IUniqueDef.");
			}
		}

		public void AddInputVoice(InputVoiceDef ivd)
        {
            Debug.Assert((ivd.MsPositionReContainer + ivd.MsDuration) <= MsDuration);
            Debug.Assert(ivd.MidiChannel >= 0 && ivd.MidiChannel <= 3);
            #region check for an existing InputVoiceDef having the same MidiChannel
            foreach(VoiceDef voiceDef in _voiceDefs)
            {
                if(voiceDef is InputVoiceDef existingInputVoiceDef)
                {
                    Debug.Assert(existingInputVoiceDef.MidiChannel != ivd.MidiChannel, "Error: An InputVoiceDef with the same MidiChannel already exists.");
                }
            }
            #endregion

            int startPos = ivd.MsPositionReContainer;
            ivd.MsPositionReContainer = 0;
            foreach(IUniqueDef iud in ivd.UniqueDefs)
            {
                iud.MsPositionReFirstUD += startPos;
            }

            _voiceDefs.Add(ivd);

            AssertConsistency();
        }
	}
}
