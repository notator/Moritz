using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;
using System.Collections.ObjectModel;

namespace Moritz.Spec
{
	public class Block
	{
        /// <summary>
        /// A Block contains a list of voiceDefs, that can be of both kinds: Trks and InputVoiceDefs. A Seq can only contains Trks.
        /// This constructor converts its argument to a Block so, if the arguments need to be preserved, pass clones.
        /// <para>The seq's Trks and the InputVoiceDefs are cast to VoiceDefs, and then padded at the beginning and end with rests
        /// so that they all start at the beginning of the Block and have the same duration.</para>
        /// <para>The Block's AbsMsPosition is set to the seq's AbsMsPosition.</para>
        /// <para>There is at least one MidiChordDef or InputChordDef at the start of the Block, and at least one
        /// MidiChordDef ends at its end.</para>
        /// <para>If an original seq.trk.UniqueDefs list is empty or contains a single restDef, the corresponding
        /// voiceDef will contain a single rest having the same duration as the other trks.</para>
        /// </summary>
        /// <param name="seq">cannot be null, and must have Trks</param>
        /// <param name="inputVoiceDefs">can be null</param>
        public Block(Seq seq, List<InputVoiceDef> inputVoiceDefs)
		{
            AbsMsPosition = seq.AbsMsPosition;

            foreach(Trk trk in seq.Trks)
            {
                _voiceDefs.Add(trk);
            }
            if(inputVoiceDefs != null)
            {
                foreach(InputVoiceDef ivd in inputVoiceDefs)
                {
                    _voiceDefs.Add(ivd);
                }
            }

            int blockMsDuration = MsDuration; // MsDuration is a property that looks at UniqueDefs in this block. 

			foreach(VoiceDef voiceDef in _voiceDefs)
			{
				if(voiceDef.UniqueDefs.Count > 0)
				{
                    IUniqueDef firstIUD = voiceDef.UniqueDefs[0];
					int startRestMsDuration = firstIUD.MsPositionReTrk;
					if(startRestMsDuration > 0)
					{
						voiceDef.UniqueDefs.Insert(0, new RestDef(0, startRestMsDuration));
					}

					int endOfTrkMsPositionReTrk = voiceDef.EndMsPositionReTrk;
					int endRestMsDuration = blockMsDuration - endOfTrkMsPositionReTrk;
					if(endRestMsDuration > 0)
					{
						voiceDef.UniqueDefs.Add(new RestDef(endOfTrkMsPositionReTrk, endRestMsDuration));
					}
					voiceDef.AgglomerateRests();
				}
				else
				{
					voiceDef.UniqueDefs.Add(new RestDef(0, blockMsDuration));
				}
			}

			AssertBlockConsistency();
		}

        /// <summary>
        /// A Block must fulfill the following criteria:
        /// The Trks may contain any combination of RestDef, MidiChordDef and ClefChangeDef.
        /// The InputVoiceDefs may contain any combination of RestDef, InputChordDef and ClefChangeDef.
        /// <para>1. All voiceDefs start at MsPositionReSeq=0 and have the same MsDuration.</para>
        /// <para>2. A restDef is never followed by another RestDef (RestDefs are agglomerated).</para>
        /// <para>3. No voiceDef may have a ClefChangeDef before the first MidiChordDef.</para>
        /// <para>4. There is at least one MidiChordDef or InputChordDef at the beginning of the block</para>
        /// <para>5. At least one MidiChordDef or InputChordDef ends at the end of the block</para>
        /// </summary>
        private void AssertBlockConsistency()
		{
			#region All voiceDefs must begin at MsPosition=0 and have the same MsDuration
			int blockMsDuration = MsDuration;
            foreach(VoiceDef voiceDef in _voiceDefs)
            {
                if(voiceDef.MsPositionReSeq > 0 || voiceDef.MsDuration != blockMsDuration)
                {
                    Debug.Assert(false, "All voiceDefs in a block must begin at MsPosition=0 and have the same MsDuration.");
                }
            }
            #endregion
            #region There is at least one MidiChordDef or InputChordDef at the beginning and end of the Sequence
            bool foundStartChordDef = false;
			bool foundEndChordDef = false;
            foreach(VoiceDef voiceDef in _voiceDefs)
            {
				if(voiceDef.UniqueDefs.Count > 0)
				{
                    IUniqueDef firstIUD = voiceDef.UniqueDefs[0];
					if(firstIUD is MidiChordDef || firstIUD is InputChordDef)
					{
						foundStartChordDef = true;
					}
                    IUniqueDef lastIUD = voiceDef.UniqueDefs[voiceDef.UniqueDefs.Count - 1];
					if(lastIUD is MidiChordDef || lastIUD is InputChordDef)
					{
						foundEndChordDef = true;
					}
				}
			}

			Debug.Assert((foundStartChordDef == true && foundEndChordDef == true),
						"A block must begin and end with at least one MidiChordDef or InputChordDef.");
			#endregion
		}

		/// <summary>
		/// A Block may not have a ClefChangeDef before the first MidiChordDef if the Block is at MsPosition 0.
		/// </summary>
		private void AssertBlockClefChangeConsistency()
		{
			if(AbsMsPosition == 0)
			{
                foreach(VoiceDef voiceDef in _voiceDefs)
                {
					foreach(IUniqueDef iud in voiceDef.UniqueDefs)
					{
						if(iud is MidiChordDef || iud is InputChordDef || iud is ClefChangeDef)
						{
                            Debug.Assert((iud is MidiChordDef || iud is InputChordDef),
								"A Block may not have a ClefChangeDef before the first ChordDef if the Block is at MsPosition 0.");
							break;
						}
					}
				}
			}
		}

		/// <summary>
		/// The argument warp is a list of doubles, in ascending order, beginning with 0 and ending with 1.
		/// The doubles represent moments in the original duration that will be separated from each other
		/// by equal durations when the function returns. The MsDuration of the Seq is not changed.
		/// </summary>
		public void WarpDurations(List<double> warp)
		{
			AssertBlockConsistency();
			int sequenceMsDuration = MsDuration;
            foreach(VoiceDef voiceDef in _voiceDefs)
            {
                voiceDef.WarpDurations(warp);
			}
			Debug.Assert(sequenceMsDuration == MsDuration);
			AssertBlockConsistency();
		}

        /// <summary>
        /// Creates a "bar" which is a list of voiceDefs containing IUniqueDefs that begin before barlineEndMsPosition,
        /// and removes these IUniqueDefs from the current block.
        /// </summary>
        /// <param name="barlineAbsEndMsPosition"></param>
        /// <returns>The removed bar</returns>
        public List<VoiceDef> GetBar(int barlineAbsEndMsPosition)
        {
            Debug.Assert(AbsMsPosition < barlineAbsEndMsPosition);

            List<VoiceDef> firstBar = new List<VoiceDef>();
            List<VoiceDef> secondBar = new List<VoiceDef>();
            int currentBlockAbsEndPos = this.AbsMsPosition + this.MsDuration;

            bool isLastBar = (currentBlockAbsEndPos == barlineAbsEndMsPosition);

            VoiceDef firstBarVoice;
            VoiceDef secondBarVoice;
            foreach(VoiceDef voiceDef in _voiceDefs)
            {
                Trk outputVoice = voiceDef as Trk;
                if(outputVoice != null)
                {
                    firstBarVoice = new Trk(outputVoice.MidiChannel, new List<IUniqueDef>());
                    firstBar.Add(firstBarVoice);
                    secondBarVoice = new Trk(outputVoice.MidiChannel, new List<IUniqueDef>());
                    secondBar.Add(secondBarVoice);
                }
                else
                {
                    firstBarVoice = new InputVoiceDef();
                    firstBar.Add(firstBarVoice);
                    secondBarVoice = new InputVoiceDef();
                    secondBar.Add(secondBarVoice);
                }
                foreach(IUniqueDef iud in voiceDef.UniqueDefs)
                {
                    Debug.Assert((iud is MidiChordDef || iud is RestDef || iud is InputChordDef),
                         "Illegal IUniqueDef type in voiceDef.UniqueDefs");

                    int iudMsDuration = iud.MsDuration;
                    int iudAbsStartPos = this.AbsMsPosition + iud.MsPositionReTrk;
                    int iudAbsEndPos = iudAbsStartPos + iudMsDuration;

                    if(iudAbsStartPos >= barlineAbsEndMsPosition)
                    {
                        Debug.Assert(iudAbsEndPos <= currentBlockAbsEndPos);
                        secondBarVoice.UniqueDefs.Add(iud);
                    }
                    else if(iudAbsEndPos > barlineAbsEndMsPosition)
                    {
                        int durationAfterBarline = iudAbsEndPos - barlineAbsEndMsPosition;
                        if(iud is RestDef)
                        {
                            // This is a rest. Split it.
                            RestDef firstRestHalf = new RestDef(iudAbsStartPos, barlineAbsEndMsPosition - iudAbsStartPos);
                            firstBarVoice.UniqueDefs.Add(firstRestHalf);

                            RestDef secondRestHalf = new RestDef(barlineAbsEndMsPosition, durationAfterBarline);
                            secondBarVoice.UniqueDefs.Add(secondRestHalf);
                        }
                        else if(iud is MidiChordDef || iud is InputChordDef)
                        {
                            IUniqueSplittableChordDef uniqueChordDef = iud as IUniqueSplittableChordDef;
                            uniqueChordDef.MsDurationToNextBarline = barlineAbsEndMsPosition - iudAbsStartPos;

                            firstBarVoice.UniqueDefs.Add(uniqueChordDef);
                        }
                    }
                    else
                    {
                        Debug.Assert(iudAbsEndPos <= barlineAbsEndMsPosition && iudAbsStartPos >= AbsMsPosition);
                        firstBarVoice.UniqueDefs.Add(iud);
                    }
                }
            }

            this.AbsMsPosition = barlineAbsEndMsPosition;

            this._voiceDefs = secondBar;
            SetMsPositions();

            if(!isLastBar)
            {
                // _voiceDefs is not empty
                AssertBlockConsistency();
            }

            return firstBar;
        }

        private void SetMsPositions()
        {
            foreach(VoiceDef voiceDef in _voiceDefs)
            {
                int msPosition = 0;
                foreach(IUniqueDef iud in voiceDef)
                {
                    iud.MsPositionReTrk = msPosition;
                    msPosition += iud.MsDuration;
                }
            }
        }

        /// <summary>
        /// The duration between the beginning of the first UniqueDef in the block and the end of the last UniqueDef in the block.
        /// Setting this value stretches or compresses the msDurations of all the voiceDefs and their contained UniqueDefs.
        /// </summary>
        public int MsDuration
        {
            get
            {
                int msDuration = 0;
                foreach(VoiceDef voiceDef in _voiceDefs)
                {
                    if(voiceDef.UniqueDefs.Count > 0)
                    {
                        IUniqueDef lastIUD = voiceDef.UniqueDefs[voiceDef.UniqueDefs.Count - 1];
                        int endMsPosReTrk = lastIUD.MsPositionReTrk + lastIUD.MsDuration;
                        msDuration = (msDuration < endMsPosReTrk) ? endMsPosReTrk : msDuration;
                    }
                }
                return msDuration;
            }
            set
            {
                Debug.Assert(_voiceDefs.Count > 0);
                AssertBlockConsistency(); // all Trks and InputVoiceDefs have MsPositionReSeq == 0, and are the same length.
                int currentDuration = MsDuration;
                double factor = ((double)value) / currentDuration;
                foreach(VoiceDef voiceDef in _voiceDefs)
                {
                    voiceDef.MsDuration = (int)Math.Round(voiceDef.MsDuration * factor);
                    voiceDef.MsPositionReSeq = (int)Math.Round(voiceDef.MsPositionReSeq * factor);
                }
                int roundingError = value - MsDuration;
                if(roundingError != 0)
                {
                    foreach(VoiceDef voiceDef in _voiceDefs)
                    {
                        if((voiceDef.EndMsPositionReTrk + roundingError) == value)
                        {
                            voiceDef.EndMsPositionReTrk += roundingError;
                        }
                    }
                }
                Debug.Assert(MsDuration == value);
            }
        }

        public int AbsMsPosition = 0;

        public List<Trk> Trks
        {
            get
            {
                List<Trk> trks = new List<Trk>();
                foreach(VoiceDef voiceDef in _voiceDefs)
                {
                    Trk trk = voiceDef as Trk;
                    if(trk != null)
                    {
                        trks.Add(trk);
                    }
                }
                return trks;
            }
        }

        private List<VoiceDef> _voiceDefs = new List<VoiceDef>();
	}
}
