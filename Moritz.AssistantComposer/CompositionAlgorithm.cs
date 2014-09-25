using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;
using Moritz.Score;
using Moritz.Score.Midi;
using Moritz.Score.Notation;

namespace Moritz.AssistantComposer
{
    /// <summary>
    /// A CompositionAlgorithm is special to a particular composition.
    /// When called, the DoAlgorithm() function returns a list of VoiceDef lists,
    /// whereby each contained VoiceDef list is the definition of a bar (a bar is
    /// a place where a system can be broken).
    /// Algorithms don't control the page format, how many bars per system there
    /// are, or the shapes of the symbols. Those things are set for a particular
    /// score in an .mkss file using the Assistant Composer's main form.
    /// The VoiceDefs returned from DoAlgorithm() are converted to real Voices
    /// (containing real NoteObjects) later, using the options set an .mkss file. 
    /// </summary>
    public abstract class CompositionAlgorithm
    {
        protected CompositionAlgorithm(List<Krystal> krystals, List<Palette> palettes)
        {
            _krystals = krystals;
            _palettes = palettes;
            CheckParameters();
        }

        protected void CheckParameters()
        {
            Debug.Assert(NumberOfOutputVoices > 0, "CompositionAlgorithm: There must be at least one output voice!");
            Debug.Assert(NumberOfOutputVoices <= 16, "CompositionAlgorithm: There can not be more than 16 output voices.");

            Debug.Assert(NumberOfInputVoices >= 0, "CompositionAlgorithm: There can not be a negative number of input voices!");
            // I assume that a single performer will never need more than three staves @ two voices...
            // As far as I know, this restriction is not really necessary as far as the software is concerned, but it may help
            // reduce errors.
            Debug.Assert(NumberOfInputVoices <= 6, "CompositionAlgorithm: There should not be more than six input voices!");

            Debug.Assert(NumberOfBars > 0, "CompositionAlgorithm: There must be at least one bar!");            
        }

        /// <summary>
        /// Returns the number of outputVoices created by the algorithm.
        /// </summary>
        public abstract int NumberOfOutputVoices {get;}

        /// <summary>
        /// Returns the number of inputVoices created by the algorithm.
        /// </summary>
        public abstract int NumberOfInputVoices { get; }

        /// <summary>
        /// Returns the number of bars created by the algorithm.
        /// </summary>
        /// <returns></returns>
        public abstract int NumberOfBars { get; }

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
        /// The chord definitions in OutputVoice.UniqueDefs must be MidiChordDefs.
        /// The chord definitions in InputVoice.UniqueDefs must be InputChordDefs.
        /// 
        /// If one or more InputVoices are defined, then an InputControls object must be created, given
        /// default values, and assigned to this.InputControls (see below).
        /// </summary>
        public abstract List<List<Voice>> DoAlgorithm();

        /// <summary>
        /// Returns the position of the end of the last UniqueMidiDurationDef
        /// in the bar's first voice's UniqueMidiDurationDefs list.
        /// </summary>
        protected int GetEndMsPosition(List<Voice> bar)
        {
            Debug.Assert(bar != null && bar.Count > 0 && bar[0].UniqueDefs.Count > 0);
            List<IUniqueDef> lmdd = bar[0].UniqueDefs;
            IUniqueDef lastLmdd = lmdd[lmdd.Count - 1];
            int endMsPosition = lastLmdd.MsPosition + lastLmdd.MsDuration;
            return endMsPosition;
        }

        /// <summary>
        /// Returns two bars. The first is the beginning of the argument bar up to absoluteSplitPos,
        /// The second is the end of the argument bar beginning at absoluteSplitPos.
        /// The final UniqueMidiDurationDef in each voice.UniqueMidiDurationDefs list is converted
        /// to a FinalLMDDInVoice object containing an MsDurationToBarline property.
        /// If a chord or rest overlaps a barline, a LocalizedCautionaryChordDef object is created at the
        /// start of the voice.UniqueMidiDurationDefs in the second bar. A LocalizedCautionaryChordDef
        /// object is a kind of chord which is used while justifying systems, but is not displayed and
        /// does not affect performance.
        /// ClefChangeDefs are placed at the end of the first bar, not at the start of the second bar.
        /// </summary>
        protected List<List<Voice>> SplitBar(List<Voice> originalBar, int absoluteSplitPos)
        {
            List<List<Voice>> twoBars = new List<List<Voice>>();
            List<Voice> firstBar = new List<Voice>();
            List<Voice> secondBar = new List<Voice>();
            twoBars.Add(firstBar);
            twoBars.Add(secondBar);
            int originalBarStartPos = originalBar[0].UniqueDefs[0].MsPosition;
            int originalBarEndPos =
                originalBar[0].UniqueDefs[originalBar[0].UniqueDefs.Count - 1].MsPosition +
                originalBar[0].UniqueDefs[originalBar[0].UniqueDefs.Count - 1].MsDuration;

            Voice firstBarVoice;
            Voice secondBarVoice;
            foreach(Voice voice in originalBar)
            {
                OutputVoice outputVoice = voice as OutputVoice;
                if(outputVoice != null)
                {
                    firstBarVoice = new OutputVoice((OutputStaff)voice.Staff, outputVoice.MidiChannel);
                    firstBar.Add(firstBarVoice);
                    secondBarVoice = new OutputVoice((OutputStaff)voice.Staff, outputVoice.MidiChannel);
                    secondBar.Add(secondBarVoice);
                }
                else
                {
                    firstBarVoice = new InputVoice((InputStaff)voice.Staff);
                    firstBar.Add(firstBarVoice);
                    secondBarVoice = new InputVoice((InputStaff)voice.Staff);
                    secondBar.Add(secondBarVoice);
                }
                foreach(IUniqueDef iUnique in voice.UniqueDefs)
                {
                    int udMsDuration = iUnique.MsDuration;
                    IUniqueSplittableChordDef uniqueChordDef = iUnique as IUniqueSplittableChordDef;
                    if(uniqueChordDef != null)
                    {
                        udMsDuration = (uniqueChordDef.MsDurationToNextBarline == null) ? iUnique.MsDuration : (int)uniqueChordDef.MsDurationToNextBarline;
                    }

                    int udEndPos = iUnique.MsPosition + udMsDuration;
                    
                    if(iUnique.MsPosition >= absoluteSplitPos)
                    {
                        if(iUnique.MsPosition == absoluteSplitPos && iUnique is ClefChangeDef)
                        {
                            firstBarVoice.UniqueDefs.Add(iUnique);
                        }
                        else
                        {
                            Debug.Assert(udEndPos <= originalBarEndPos);
                            secondBarVoice.UniqueDefs.Add(iUnique);
                        }
                    }
                    else if(udEndPos > absoluteSplitPos)
                    {
                        int durationAfterBarline = udEndPos - absoluteSplitPos;
                        if(iUnique is RestDef)
                        {
                            // This is a rest. Split it.
                            RestDef firstRestHalf = new RestDef(iUnique.MsPosition, absoluteSplitPos - iUnique.MsPosition);
                            firstBarVoice.UniqueDefs.Add(firstRestHalf);

                            RestDef secondRestHalf = new RestDef(absoluteSplitPos, durationAfterBarline);
                            secondBarVoice.UniqueDefs.Add(secondRestHalf);
                        }
                        else if(iUnique is CautionaryChordDef)
                        {
                            // This is a cautionary chord. Set the position of the following barline, and
                            // Add an LocalizedCautionaryChordDef at the beginning of the following bar.
                            iUnique.MsDuration = absoluteSplitPos - iUnique.MsPosition;
                            firstBarVoice.UniqueDefs.Add(iUnique);

                            CautionaryChordDef secondLmdd = new CautionaryChordDef((IUniqueChordDef)iUnique, absoluteSplitPos, durationAfterBarline);
                            secondBarVoice.UniqueDefs.Add(secondLmdd);
                        }
                        else
                        {
                            // This is a MidiChordDef or a InputChordDef. 
                            // Set the position of the following barline, and add a CautionaryChordDef at the beginning
                            // of the following bar.
                            if(uniqueChordDef != null)
                            {
                                uniqueChordDef.MsDurationToNextBarline = absoluteSplitPos - iUnique.MsPosition;
                            }

                            firstBarVoice.UniqueDefs.Add((IUniqueDef) uniqueChordDef);

                            CautionaryChordDef secondLmdd = new CautionaryChordDef((IUniqueChordDef) uniqueChordDef, 
                                absoluteSplitPos, durationAfterBarline);
                            secondBarVoice.UniqueDefs.Add(secondLmdd);
                        }
                    }
                    else
                    {
                        Debug.Assert(udEndPos <= absoluteSplitPos && iUnique.MsPosition >= originalBarStartPos);
                        firstBarVoice.UniqueDefs.Add(iUnique);
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
                    for(int i = 0; i < voice.UniqueDefs.Count - 1; i++)
                    {
                        MidiChordDef mcd1 = voice.UniqueDefs[i] as MidiChordDef;
                        MidiChordDef mcd2 = voice.UniqueDefs[i + 1] as MidiChordDef;
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

                        if(i == voice.UniqueDefs.Count - 2 && consecRestIndices.Count > 0)
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
                            int msPosition = voice.UniqueDefs[indToReplace[0]].MsPosition;
                            for(int j = indToReplace.Count - 1; j >= 0; j--)
                            {
                                IUniqueDef iumdd = voice.UniqueDefs[indToReplace[j]];
                                Debug.Assert(iumdd.MsDuration > 0);
                                msDuration += iumdd.MsDuration;
                                voice.UniqueDefs.RemoveAt(indToReplace[j]);
                            }
                            RestDef replacementLmdd = new RestDef(msPosition, msDuration);
                            voice.UniqueDefs.Insert(indToReplace[0], replacementLmdd);
                        }
                    }
                    #endregion
                }
            }
        }

        /// <summary>
        /// This function should be called for all scores when the bars are complete.
        /// </summary>
        /// <param name="firstBar"></param>
        /// <param name="masterVolumes">A list with one value per OutputVoice</param>
        protected void SetOutputVoiceMasterVolumes(List<Voice> firstBar, List<byte> masterVolumes)
        {
            Debug.Assert(masterVolumes.Count == NumberOfOutputVoices);
            for(int i = 0; i < masterVolumes.Count; ++i)
            {
                OutputVoice oVoice = firstBar[i] as OutputVoice;
                Debug.Assert(oVoice != null);
                Debug.Assert(masterVolumes[i] != 0);
                oVoice.MasterVolume = masterVolumes[i];
            }
        }

        /// <summary>
        /// This function should be called when the bars are complete, if the score contains InputVoices.
        /// </summary>
        /// <param name="firstBar"></param>
        /// <param name="inputControlsList">A list with one value per OutputVoice</param>
        protected void SetOutputVoiceInputControls(List<Voice> bar1, List<InputControls> inputControlsList)
        {
            Debug.Assert(inputControlsList.Count == NumberOfOutputVoices); // == number of OutputVoices.
            for(int i = 0; i < NumberOfOutputVoices; ++i)
            {
                OutputVoice oVoice = bar1[i] as OutputVoice;
                Debug.Assert(oVoice != null);
                Debug.Assert(inputControlsList[i] != null);
                oVoice.InputControls = inputControlsList[i];
            }
        }

        protected List<Krystal> _krystals;
        protected List<Palette> _palettes;
    }
}
