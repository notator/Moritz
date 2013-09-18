using System.Collections.Generic;
using System.Diagnostics;

using Moritz.Score;
using Moritz.Score.Midi;
using Krystals4ObjectLibrary;

namespace Moritz.AssistantComposer
{
    /// <summary>
    /// A CompositionAlgorithm adds the correct number of Systems, Staves and Voices to the score, whereby
    /// ONE BAR PER SYSTEM is created. (Algorithms are independent of the page format.)
    /// The Voices' MidiDurationDefs lists are set, but not the (graphic) NoteObjects.
    /// NoteObjects are created later, using a specialized Notator object. 
    /// </summary>
    public abstract class MidiCompositionAlgorithm
    {
        protected MidiCompositionAlgorithm(List<Krystal> krystals, List<PaletteDef> paletteDefs)
        {
            _krystals = krystals;
            _paletteDefs = paletteDefs;
            CheckMidiChannels();
        }

        protected void CheckMidiChannels()
        {
            List<byte> midiChannels = MidiChannels();
            Debug.Assert(midiChannels != null && midiChannels.Count > 0, "MidiCompositionAlgorithm: No midi channels!");
            for(int i = 0; i < midiChannels.Count; ++i)
            {
                Debug.Assert(midiChannels[i] >= 0, "MidiCompositionAlgorithm: Midi channel index must be >= 0!");
                if(i > 0)
                {
                    Debug.Assert(midiChannels[i] > midiChannels[i - 1], "MidiCompositionAlgorithm: Midi channels must be unique, and in ascending order.");
                }
            }
        }

        /// <summary>
        /// This function returns the list of midi channels used by the algorithm.
        /// The returned list must contain at least one channel, and all channel indices must be greater
        /// than 0. The midi channels do not have to be contiguous.
        /// The user decides the number of staves and the order of the midi channels
        /// from top to bottom in the notated score. A maximum of two voices per staff is possible.
        /// See also the DoAlgorithm() comment (below).
        /// </summary>
        public abstract List<byte> MidiChannels();

        /// <summary>
        /// All midi algorithms create a sequence of bars, each of which consists of a list of
        /// voices, in top to bottom order of a conceptual 'system'. Each bar has the same number of voices.
        /// Each voice in the 'system' has its own unique midi channel, so that voices at the same vertical
        /// position in consecutive bars contain the continuation of a particular midi channel.
        /// By convention, algorithms use midi channels having indices which increase from top to bottom in
        /// the 'system', with the top voice usually having midi channel index = 0. Midi channels may not
        /// occur twice in the same 'system'. The midi channels do not have to be contiguous.
        /// Each algorithm declares which midi channels it uses in the MidiChannels() function (see above).
        /// For an example, see Study2bAlgorithm.
        /// </summary>
        /// <returns>
        /// A list of sequential bars. Each bar contains all the voices in the bar, from top to bottom.
        /// </returns>
        public abstract List<List<Voice>> DoAlgorithm();
        /// <summary>
        /// Returns the number of bars created by the algorithm.
        /// </summary>
        /// <returns></returns>
        public abstract int NumberOfBars();

        /// <summary>
        /// Returns the position of the end of the last LocalizedMidiDurationDef
        /// in the bar's first voice's LocalizedMidiDurationDefs list.
        /// </summary>
        protected int GetEndMsPosition(List<Voice> bar)
        {
            Debug.Assert(bar != null && bar.Count > 0 && bar[0].LocalizedMidiDurationDefs.Count > 0);
            List<LocalizedMidiDurationDef> lmdd = bar[0].LocalizedMidiDurationDefs;
            LocalizedMidiDurationDef lastLmdd = lmdd[lmdd.Count - 1];
            int endMsPosition = lastLmdd.MsPosition + lastLmdd.MsDuration;
            return endMsPosition;
        }

        /// <summary>
        /// Returns two bars. The first is the beginning of the argument bar up to absoluteSplitPos,
        /// The second is the end of the argument bar beginning at absoluteSplitPos.
        /// The final LocalizedMidiDurationDef in each voice.LocalizedMidiDurationDefs list is converted
        /// to a FinalLMDDInVoice object containing an MsDurationToBarline property.
        /// If a chord or rest overlaps a barline, an OverlapLmddAtStartOfBar object is created at the
        /// start of the voice.LocalizedMidiDurationDefs in the second bar. An OverlapLmddAtStartOfBar
        /// object is a kind of rest which is used while justifying systems, but is not displayed and
        /// does not affect performance.
        /// </summary>
        protected List<List<Voice>> SplitBar(List<Voice> originalBar, int absoluteSplitPos)
        {
            List<List<Voice>> twoBars = new List<List<Voice>>();
            List<Voice> firstBar = new List<Voice>();
            List<Voice> secondBar = new List<Voice>();
            twoBars.Add(firstBar);
            twoBars.Add(secondBar);
            int originalBarStartPos = originalBar[0].LocalizedMidiDurationDefs[0].MsPosition;
            int originalBarEndPos =
                originalBar[0].LocalizedMidiDurationDefs[originalBar[0].LocalizedMidiDurationDefs.Count - 1].MsPosition +
                originalBar[0].LocalizedMidiDurationDefs[originalBar[0].LocalizedMidiDurationDefs.Count - 1].MsDuration;
           

            foreach(Voice voice in originalBar)
            {
                Voice firstBarVoice = new Voice(voice.Staff, voice.MidiChannel);
                firstBar.Add(firstBarVoice);
                Voice secondBarVoice = new Voice(voice.Staff, voice.MidiChannel);
                secondBar.Add(secondBarVoice);
                foreach(LocalizedMidiDurationDef lmdd in voice.LocalizedMidiDurationDefs)
                {
                    int lmddMsDuration = (lmdd.MsDurationToNextBarline == null) ? lmdd.MsDuration : (int) lmdd.MsDurationToNextBarline;
                    int lmddEndPos = lmdd.MsPosition + lmddMsDuration;
                    if(lmdd.MsPosition >= absoluteSplitPos)
                    {
                        Debug.Assert(lmddEndPos <= originalBarEndPos);
                        secondBarVoice.LocalizedMidiDurationDefs.Add(lmdd);
                    }
                    else
                    if(lmddEndPos > absoluteSplitPos)
                    {
                        int durationAfterBarline = lmddEndPos - absoluteSplitPos;
                        if(lmdd.MidiChordDef == null)
                        {
                            // This is a rest. Split it.
                            LocalizedMidiDurationDef firstLmdd = new LocalizedMidiDurationDef(lmdd.MidiChordDef, lmdd.MsPosition, absoluteSplitPos - lmdd.MsPosition);
                            firstLmdd.MsDurationToNextBarline = absoluteSplitPos - lmdd.MsPosition;
                            firstBarVoice.LocalizedMidiDurationDefs.Add(firstLmdd);

                            LocalizedMidiDurationDef secondLmdd = new LocalizedMidiDurationDef(lmdd.MidiChordDef, absoluteSplitPos, durationAfterBarline);
                            secondBarVoice.LocalizedMidiDurationDefs.Add(secondLmdd);
                        }
                        else
                        {
                            // This is a chord. Set the position of the following barline, and
                            // Add an OverlapLmddAtStartOfBar at the beginning of the following bar.
                            lmdd.MsDurationToNextBarline = absoluteSplitPos - lmdd.MsPosition;
                            firstBarVoice.LocalizedMidiDurationDefs.Add(lmdd);

                            OverlapLmddAtStartOfBar secondLmdd = new OverlapLmddAtStartOfBar(absoluteSplitPos, durationAfterBarline, lmdd.MidiChordDef);
                            secondBarVoice.LocalizedMidiDurationDefs.Add(secondLmdd);
                        }
                    }
                    else
                    {
                        Debug.Assert(lmddEndPos <= absoluteSplitPos && lmdd.MsPosition >= originalBarStartPos);
                        firstBarVoice.LocalizedMidiDurationDefs.Add(lmdd);
                    }
                }
            }
            return twoBars;
        }


        /// <summary>
        /// There is currently still one bar per system.
        /// </summary>
        protected void ReplaceConsecutiveRestsInBars(List<List<Voice>> voicesPerStaffPerSystem)
        {
            foreach(List<Voice> voicesPerStaff in voicesPerStaffPerSystem)
            {
                foreach(Voice voice in voicesPerStaff)
                {
                    // contains lists of consecutive rest indices
                    List<List<int>> restsToReplace = new List<List<int>>();
                    #region find the consecutive rests
                    List<int> consecRestIndices = new List<int>();
                    for(int i = 0; i < voice.LocalizedMidiDurationDefs.Count - 1; i++)
                    {
                        MidiChordDef mcd1 = voice.LocalizedMidiDurationDefs[i].MidiChordDef;
                        MidiChordDef mcd2 = voice.LocalizedMidiDurationDefs[i + 1].MidiChordDef;
                        if(mcd1 == null && mcd2 == null)
                        {
                            if(!consecRestIndices.Contains(i))
                            {
                                consecRestIndices.Add(i);
                            }
                            consecRestIndices.Add(i + 1);
                        }
                        else
                        {
                            if(consecRestIndices != null && consecRestIndices.Count > 0)
                            {
                                restsToReplace.Add(consecRestIndices);
                                consecRestIndices = new List<int>();
                            }
                        }

                        if(i == voice.LocalizedMidiDurationDefs.Count - 2 && consecRestIndices.Count > 0)
                        {
                            restsToReplace.Add(consecRestIndices);
                        }
                    }
                    #endregion
                    #region replace the consecutive rests
                    if(restsToReplace.Count > 0)
                    {
                        for(int i = restsToReplace.Count - 1; i >= 0; i--)
                        {
                            List<int> indToReplace = restsToReplace[i];
                            int msDuration = 0;
                            int msPosition = voice.LocalizedMidiDurationDefs[indToReplace[0]].MsPosition;
                            for(int j = indToReplace.Count - 1; j >= 0; j--)
                            {
                                LocalizedMidiDurationDef lmdd = voice.LocalizedMidiDurationDefs[indToReplace[j]];
                                Debug.Assert(lmdd.MidiChordDef == null);
                                Debug.Assert(lmdd.MsDuration > 0);
                                msDuration += lmdd.MsDuration;
                                voice.LocalizedMidiDurationDefs.RemoveAt(indToReplace[j]);
                            }
                            LocalizedMidiDurationDef replacementLmdd = new LocalizedMidiDurationDef(msDuration);
                            replacementLmdd.MsPosition = msPosition;
                            voice.LocalizedMidiDurationDefs.Insert(indToReplace[0], replacementLmdd);
                        }
                    }
                    #endregion
                }
            }
        }

        protected List<Krystal> _krystals;
        protected List<PaletteDef> _paletteDefs;
    }
}
