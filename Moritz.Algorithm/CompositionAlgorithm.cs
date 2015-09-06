using System;
using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;
using Moritz.Palettes;
using Moritz.Spec;

namespace Moritz.Algorithm
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
        protected CompositionAlgorithm()
        {
            CheckParameters();
        }

        protected void CheckParameters()
        {
            int channelCount = MidiChannelIndexPerOutputVoice.Count;
            if(channelCount < 1)
                throw new ApplicationException("CompositionAlgorithm: There must be at least one output voice!");
            if(channelCount > 16)
                throw new ApplicationException("CompositionAlgorithm: There can not be more than 16 output voices.");

            if(channelCount != MasterVolumePerOutputVoice.Count)
                throw new ApplicationException("CompositionAlgorithm: Wrong number of master volumes"); 

            for(int i = 0; i < channelCount; ++i)
            {
                int channelIndex = MidiChannelIndexPerOutputVoice[i];
                if(channelIndex < 0 || channelIndex > 15)
                    throw new ApplicationException("CompositionAlgorithm: midi channel out of range!");

                int masterVolume = MasterVolumePerOutputVoice[i];
                if(masterVolume < 0 || masterVolume > 127)
                    throw new ApplicationException("CompositionAlgorithm: master volume out of range!");

                if(i < channelCount - 1)
                {
                    for(int j = i + 1; j < channelCount; ++j)
                    {
                        if(channelIndex == MidiChannelIndexPerOutputVoice[j])
                            throw new ApplicationException("Output midi channels must be unique per output voice.");
                    }
                }
            }

            // Midi input devices are identified by their midi channel, so there may not be more than 16 of them.
            // InputVoices can share the same midi input channel (a device can play more than one voice), so there
            // is no upper limit to the number of InputVoices.
            // Input Voices having the same channel are agglomerated at load time by the Assistant Performer.
            if(NumberOfInputVoices < 0)
                throw new ApplicationException("CompositionAlgorithm: There can not be a negative number of input voices!");

            if(NumberOfBars == 0)
                throw new ApplicationException("CompositionAlgorithm: There must be at least one bar!");

        }

        /// <summary>
        /// Returns the midi channel of each output voice in top to bottom order in the original algorithm.
        /// The midi channels are usually in order, starting at 0, but this need not be the case.
        /// This is so that the standard midi percussion channel (channelIndex 9) can be used.
        /// These values are written once in the score (to each voice in the first system in the score).
        /// These values must be in range [ 0..15 ].
        /// A midi channel's voiceID (written into in the score, if there are input voices) is its position in this list.
        /// </summary>
        public abstract List<int> MidiChannelIndexPerOutputVoice { get; }

        /// <summary>
        /// Returns the master volume of each output voice in top to bottom order in the original algorithm.
        /// These values are written once in the score (to each voice in the first system in the score).
        /// These values must be in range [ 0..127 ].
        /// A midi channel's voiceID (written into in the score, if there are input voices) is its position in this list.
        /// According to Jeff Glatt, the Master Volume should be set to 90 by default.
        /// </summary>
        public abstract List<int> MasterVolumePerOutputVoice { get; }

        /// <summary>
        /// Returns the number of inputVoices created by the algorithm.
        /// </summary>
        public abstract int NumberOfInputVoices { get; }

        /// <summary>
        /// Returns the number of bars (=bar definitions) created by the algorithm.
        /// </summary>
        /// <returns></returns>
        public abstract int NumberOfBars { get; }

        /// <summary>
        /// The DoAlgorithm() function is special to a particular composition.
        /// This function returns a sequence of abstract bar definitions, devoid of layout information.
        /// Each bar definition is a list of voice definitions (VoiceDefs), The VoiceDefs are conceptually
        /// in the default top to bottom order of the voices in a final score. The actual order in which
        /// the voices are eventually printed is controlled using the Assistant Composer's layout options,
        /// Each bar definition in the sequence returned by this function contains the same number of
        /// VoiceDefs. VoiceDefs at the same index in each bar are continuations of the same overall voice
        /// definition, and may be concatenated to create multiple bars on a staff.
        /// Each VoiceDef returned by this function contains a list of UniqueDef objects (VoiceDef.UniqueDefs).
        /// When the Assistant Composer creates a real score, each of these UniqueDef objects is converted to
        /// a real NoteObject containing layout information (by a Notator), and the NoteObject then added to a
        /// concrete Voice.NoteObjects list. See Notator.AddSymbolsToSystems().
        /// ACHTUNG:
        /// The top (=first) VoiceDef in each bar definition must be a TrkDef.
        /// This can be followed by zero or more OutputVoices, followed by zero or more InputVoices.
        /// The chord definitions in TrkDef.UniqueDefs must be MidiChordDefs.
        /// The chord definitions in InputVoice.UniqueDefs must be InputChordDefs.
        /// Algorithms declare the number of output and input voices they construct by defining the
        /// MidiChannelIndexPerOutputVoice and NumberOfInputVoices properties (see above).
        /// For convenience in the Assistant Composer, the number of bars is also returned (in the
        /// NumberOfBars property).
        /// If one or more InputVoices are defined, then an TrkOptions object must be created, given
        /// default values, and assigned to this.TrkOptions (see below).
        /// 
        /// A note about voiceIDs and midi channels in scores:
        /// The Assistant Composer allocates the voiceIDs saved in the score automatically when the score is 
        /// created. Each VoiceID is its index in the original bars created by the algorithm. (The top-bottom 
        /// order of these voices in the final score is set using the Assistant Composer's layout options.)
        /// An algorithm associates each voice (voiceID) with a particular midi channel by setting the
        /// MidiChannelIndexPerOutputVoice property in the top to bottom order of the voices in the bars being
        /// created. This rigmarole allows algorithms to stipulate the standard midi percussion channel (channel
        /// index 9).
        /// An OutputVoice's midiChannel, voiceID (and masterVolume) are written only to each voice in the first
        /// system in the score. And the voiceID is only written if the score actually contains InputVoices.
        /// (VoiceIDs are only needed in score because these values are used as references by InputVoices.)
        /// Algorithms simply set the InputVoice references to OutputVoices (voiceIDs) by using their index
        /// in the default bar layout being created.
        /// </summary>
        public abstract List<List<VoiceDef>> DoAlgorithm(List<Krystal> krystals, List<Palette> palettes);

        /// <summary>
        /// Returns the position of the end of the last UniqueMidiDurationDef
        /// in the bar's first voice's UniqueMidiDurationDefs list.
        /// </summary>
        protected int GetEndMsPosition(List<VoiceDef> bar)
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
        protected List<List<VoiceDef>> SplitBar(List<VoiceDef> originalBar, int absoluteSplitPos)
        {
            List<List<VoiceDef>> twoBars = new List<List<VoiceDef>>();
            List<VoiceDef> firstBar = new List<VoiceDef>();
            List<VoiceDef> secondBar = new List<VoiceDef>();
            twoBars.Add(firstBar);
            twoBars.Add(secondBar);
            int originalBarStartPos = originalBar[0].UniqueDefs[0].MsPosition;
            int originalBarEndPos =
                originalBar[0].UniqueDefs[originalBar[0].UniqueDefs.Count - 1].MsPosition +
                originalBar[0].UniqueDefs[originalBar[0].UniqueDefs.Count - 1].MsDuration;

            VoiceDef firstBarVoice;
            VoiceDef secondBarVoice;
            foreach(VoiceDef voice in originalBar)
            {
                Trk outputVoice = voice as Trk;
                if(outputVoice != null)
                {
					firstBarVoice = new Trk(outputVoice.MidiChannel,new List<IUniqueDef>());
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

                            firstBarVoice.UniqueDefs.Add((IUniqueDef)uniqueChordDef);

                            CautionaryChordDef secondLmdd = new CautionaryChordDef((IUniqueChordDef)uniqueChordDef,
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
        protected void ReplaceConsecutiveRestsInBars(List<List<VoiceDef>> voicesPerStaffPerSystem)
        {
            foreach(List<VoiceDef> voicesPerStaff in voicesPerStaffPerSystem)
            {
                foreach(VoiceDef voice in voicesPerStaff)
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
        /// <param name="masterVolumes">A list with one value per TrkDef</param>
        protected void SetOutputVoiceChannelsAndMasterVolumes(List<VoiceDef> firstBar)
        {
            Debug.Assert(MasterVolumePerOutputVoice.Count == MidiChannelIndexPerOutputVoice.Count);
            for(int i = 0; i < MasterVolumePerOutputVoice.Count; ++i)
            {
                Trk oVoice = firstBar[i] as Trk;
                Debug.Assert(oVoice != null);
                Debug.Assert(MasterVolumePerOutputVoice[i] != 0);
                Debug.Assert(MasterVolumePerOutputVoice[i] >= 0 && MasterVolumePerOutputVoice[i] < 128);
                Debug.Assert(MidiChannelIndexPerOutputVoice[i] >= 0 && MidiChannelIndexPerOutputVoice[i] < 16);
                oVoice.MidiChannel = (byte)MidiChannelIndexPerOutputVoice[i];
                oVoice.MasterVolume = (byte) MasterVolumePerOutputVoice[i];
            }
        }

        protected List<Krystal> _krystals;
        protected List<Palette> _palettes;
    }
}
