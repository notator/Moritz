using System;
using System.Collections.Generic;
using System.Diagnostics;

using Moritz.Score;
using Moritz.Score.Midi;

namespace Moritz.AssistantPerformer
{
    public partial class MidiScore
    {
        #region SVG
        public MidiScore(SvgScore svgScore, MoritzPerformanceOptions performanceOptions, bool isAssistantOnlyPerformance)
        {
            _performanceOptions = performanceOptions;
            // Dictionary<channel, SortedDictionary<msPosition,MomentSymbol>>
            Dictionary<int, SortedDictionary<int, MidiMoment>> midiChannelMoments = GetMidiChannelMoments(svgScore);

            midiChannelMoments = SetChannelInitializationControls(midiChannelMoments);

            // SortedDictionary<msPosition, TimeControl>
            SortedDictionary<int, TimeControl> logPosTimeControlsDict = GetLogPosTimeControlsDict(midiChannelMoments);

            // Now allocate the performer's and assistant's midiMoments.
            // Both performer's and assistant's midiMoments are simply List<MidiMoment> in order of MsPosition
            SetPerformerAndAssistantMoments(midiChannelMoments, logPosTimeControlsDict, isAssistantOnlyPerformance);
        }

        /// <summary>
        /// In the returned Dictionary, the key is channel, value is [msPosition, MidiMoment]
        /// Entries are constructed for this dictionary only for the channels
        /// which will actually be played by the assistant or live performer.
        /// The smallest msPosition in the dictionary is always 0.
        /// (The performance always begins at the first sounding Moment,
        /// so all the midiMoment.msPositions are adjusted if necessary,
        /// so that the first moment has MsPosition=0). 
        /// </summary>
        private Dictionary<int, SortedDictionary<int, MidiMoment>> GetMidiChannelMoments(SvgScore svgScore)
        {
            var channelMoments = new Dictionary<int, SortedDictionary<int, MidiMoment>>();
            // The svgScore.Voices iterator returns the voices in the score from top to bottom.
            // Each voice is a single system wide.
            List<Voice> voices = new List<Voice>(svgScore.Voices);

            for(int i = 0; i < voices.Count; ++i )
            {
                int playerIndex = i % _performanceOptions.MoritzPlayers.Count; 
                if(_performanceOptions.MoritzPlayers[playerIndex] != MoritzPlayer.None)
                {
                    Voice voice = voices[i];
                    int channel = voice.MidiChannel;
                    ChannelState channelState = new ChannelState();
                    foreach(MidiChordDef uniqueMidiChordDef in voice.UniqueDefs)
                    {
                        if(uniqueMidiChordDef != null) // not interested in rests here
                        {
                            int msPosition = uniqueMidiChordDef.MsPosition;
                            int msDuration = uniqueMidiChordDef.MsDuration;
                            if(_performanceOptions.SpeedFactor != 1F)
                            {
                                msPosition = (int)(((float)uniqueMidiChordDef.MsPosition) / _performanceOptions.SpeedFactor);
                                msDuration = (int)(((float)uniqueMidiChordDef.MsDuration) / _performanceOptions.SpeedFactor);
                            }

                            // The following constructor only creates Midi controls if the channel state has to be changed.
                            // (It updates the channel state accordingly.)
                            MidiChord midiChord = new MidiChord(channel, uniqueMidiChordDef, msPosition, msDuration, channelState,
                                _performanceOptions.MinimumOrnamentChordMsDuration);

                            if(!channelMoments.ContainsKey(channel))
                            {
                                channelMoments.Add(channel, new SortedDictionary<int, MidiMoment>());
                            }
                            if(!channelMoments[channel].ContainsKey(midiChord.MsPosition))
                            {
                                channelMoments[channel].Add(midiChord.MsPosition, new MidiMoment(midiChord.MsPosition));
                            }

                            channelMoments[channel][midiChord.MsPosition].MidiChords.Add(midiChord);
                        }
                    }
                }
            }

            channelMoments = RemoveInitialRest(channelMoments);

            foreach(int channel in channelMoments.Keys)
                _midiChannels.Add(channel);

            return channelMoments;
        }

        private Dictionary<int, SortedDictionary<int, MidiMoment>> RemoveInitialRest(
            Dictionary<int, SortedDictionary<int, MidiMoment>> channelMoments)
        {
            Dictionary<int, SortedDictionary<int, MidiMoment>> returnDict = channelMoments;

            bool hasInitialRest = true;
            foreach(int channel in channelMoments.Keys)
            {
                SortedDictionary<int, MidiMoment> dict = channelMoments[channel];
                if(dict.ContainsKey(0))
                    hasInitialRest = false;
            }

            if(hasInitialRest)
            {
                int initialRestMsDuration = int.MaxValue;
                foreach(int channel in channelMoments.Keys)
                {
                    SortedDictionary<int, MidiMoment> dict = channelMoments[channel];
                    foreach(int key in dict.Keys)
                    {
                        initialRestMsDuration = initialRestMsDuration < key ? initialRestMsDuration : key;
                        break; // only need the first key
                    }
                }

                returnDict = new Dictionary<int,SortedDictionary<int,MidiMoment>>();
                foreach(int channel in channelMoments.Keys)
                {
                    foreach(KeyValuePair<int, MidiMoment> kvp in channelMoments[channel])
                    {
                        MidiMoment midiMoment = kvp.Value; 
                        int newMsPosition = midiMoment.MsPosition - initialRestMsDuration;
                        foreach(MidiChord midiChord in midiMoment.MidiChords)
                        {
                            midiChord.MsPosition = newMsPosition;
                        }
                        midiMoment.MsPosition = newMsPosition;

                        if(! returnDict.ContainsKey(channel))
                            returnDict.Add(channel,new SortedDictionary<int, MidiMoment>());
                        returnDict[channel].Add(midiMoment.MsPosition,midiMoment);
                    }
                }
            }

            return returnDict;
        }

        /// <summary>
        /// in argument and return dict, key is channel, value is [msPosition, MidiMoment]
        /// </summary>
        /// <param name="systemMoments"></param>
        /// <returns></returns>
        private Dictionary<int, SortedDictionary<int, MidiMoment>> SetChannelInitializationControls(Dictionary<int, SortedDictionary<int, MidiMoment>> systemMoments)
        {
            Dictionary<int, SortedDictionary<int, MidiMoment>> returnDict = systemMoments;
            foreach(int channel in returnDict.Keys)
            {
                SortedDictionary<int, MidiMoment> midiMoments = returnDict[channel];
                foreach(int msPosition in midiMoments.Keys)
                {
                    Debug.Assert(midiMoments.ContainsKey(msPosition) && midiMoments[msPosition].MidiChords != null && midiMoments[msPosition].MidiChords.Count > 0);
                    MidiChord midiChord = midiMoments[msPosition].MidiChords[0];
                    SetMidiInitialization(midiChord);
                    break;
                }
            }
            return returnDict;
        }

        private void SetMidiInitialization(MidiChord midiChord)
        {
            int channel = midiChord.Channel;
            if(midiChord.ExpressionSlider == null)
                midiChord.ExpressionSlider = new MidiExpressionSlider(new List<byte>() { (byte)100 }, channel, 1);
            if(midiChord.ModulationWheelSlider == null)
                midiChord.ModulationWheelSlider = new MidiModulationWheelSlider(new List<byte>() { (byte)0 }, channel, 1);
            if(midiChord.PanSlider == null)
                midiChord.PanSlider = new MidiPanSlider(new List<byte>() { (byte)64 }, channel, 1);
            if(midiChord.PitchWheelDeviation == null)
                midiChord.PitchWheelDeviation = new PitchWheelDeviation(channel, 2);
            if(midiChord.PitchWheelSlider == null)
                midiChord.PitchWheelSlider = new MidiPitchWheelSlider(new List<byte>() { (byte)64 }, channel, 1);
        }

        private SortedDictionary<int, TimeControl> GetLogPosTimeControlsDict(Dictionary<int, SortedDictionary<int, MidiMoment>> systemMoments)
        {
            int endPosition = 0;
            SortedDictionary<int, TimeControl> logPosTimeControlsDict = new SortedDictionary<int, TimeControl>();
            List<int> channels = new List<int>(systemMoments.Keys);

            for(int i = 0; i < systemMoments.Keys.Count; ++i)
            {
                int channel = channels[i];
                SortedDictionary<int, MidiMoment> dict = systemMoments[channel];
                foreach(MidiMoment midiMoment in dict.Values)
                {
                    int msPos = midiMoment.MsPosition;
                    if(!logPosTimeControlsDict.ContainsKey(msPos))
                    {
                        logPosTimeControlsDict.Add(msPos, new TimeControl()); // 1 millisecond per semibreve
                    }
                    int maximumChordOffPosition = GetMaximumChordOffPosition(systemMoments[channel][msPos]);
                    endPosition = endPosition > maximumChordOffPosition ? endPosition : maximumChordOffPosition;
                }
            }
            // Add stop control after final chord
            logPosTimeControlsDict.Add(endPosition, new TimeControl());

            int prevPos = 0;
            foreach(int rPos in logPosTimeControlsDict.Keys)
            {
                if(rPos != prevPos)
                {
                    logPosTimeControlsDict[prevPos].MsWidth = rPos - prevPos;
                    logPosTimeControlsDict[prevPos].NotatedStartTime = prevPos;
                    logPosTimeControlsDict[prevPos].NotatedMsDuration = logPosTimeControlsDict[prevPos].MsWidth;
                }
                prevPos = rPos;
            }
            // the final stop control
            logPosTimeControlsDict[prevPos].MsWidth = 0;
            logPosTimeControlsDict[prevPos].NotatedStartTime = prevPos;
            logPosTimeControlsDict[prevPos].NotatedMsDuration = 0;

            return logPosTimeControlsDict;
        }
        private int GetMaximumChordOffPosition(MidiMoment systemMoment)
        {
            int maxChordOffPos = 0;
            foreach(MidiChord midiChord in systemMoment.MidiChords)
            {
                int chordOffPos = midiChord.MsPosition + midiChord.MsDuration;
                maxChordOffPos = maxChordOffPos > chordOffPos ? maxChordOffPos : chordOffPos;
            }
            return maxChordOffPos;
        }

        /// <summary>
        /// Converts the channelMidiMoments to two lists of MidiMoments,
        /// one for the Performer and one for the Assistant.
        /// </summary>
        private void SetPerformerAndAssistantMoments(
            Dictionary<int, SortedDictionary<int, MidiMoment>> channelMidiMoments,
            SortedDictionary<int, TimeControl> msPosTimeControlsDict,
            bool isAssistantOnlyPerformance)
        {
            _assistantsMoments.Clear();
            _performersMoments.Clear();

            int finalStopPos = 0;
            foreach(int logPos in msPosTimeControlsDict.Keys)
            {
                finalStopPos = logPos;
            }
            SortedDictionary<int, List<MidiMoment>> performersMidiMoments = new SortedDictionary<int, List<MidiMoment>>();
            SortedDictionary<int, List<MidiMoment>> assistantsMidiMoments = new SortedDictionary<int, List<MidiMoment>>();

            List<int> channels = new List<int>(channelMidiMoments.Keys);
            List<MoritzPlayer> realPlayers = new List<MoritzPlayer>();
            foreach(MoritzPlayer player in _performanceOptions.MoritzPlayers)
            {
                if(player != MoritzPlayer.None)
                {
                    if(isAssistantOnlyPerformance)
                        realPlayers.Add(MoritzPlayer.Assistant);
                    else
                        realPlayers.Add(player);
                }
            }
            for(int i = 0; i < channelMidiMoments.Keys.Count; ++i)
            {
                int channelIndex = channels[i];
                foreach(int pos in msPosTimeControlsDict.Keys) // each logPos in the piece (in order)
                {
                    if(pos == finalStopPos)
                        break;

                    MidiMoment midiMoment = null;
                    foreach(MidiMoment moment in channelMidiMoments[channelIndex].Values)
                    {
                        if(pos == moment.MsPosition)
                        {
                            midiMoment = moment;
                            break;
                        }
                    }

                    if(midiMoment != null)
                    {
                        if(realPlayers[i] == MoritzPlayer.Assistant)
                        {
                            if(!assistantsMidiMoments.ContainsKey(pos))
                                assistantsMidiMoments.Add(pos, new List<MidiMoment>());
                            assistantsMidiMoments[pos].Add(midiMoment);
                        }
                        else
                        {
                            if(!performersMidiMoments.ContainsKey(pos))
                                performersMidiMoments.Add(pos, new List<MidiMoment>());
                            performersMidiMoments[pos].Add(midiMoment);
                        }
                    }
                }
            }

            _performersMoments = GetSingleMomentsList(performersMidiMoments);
            _assistantsMoments = GetSingleMomentsList(assistantsMidiMoments);

            SetMomentsMsWidths(_performersMoments);
            SetMomentsMsWidths(_assistantsMoments);

            SetChordContainers(_performersMoments);
            SetChordContainers(_assistantsMoments);

            _msPosTimeControlsDict = msPosTimeControlsDict;
        }

        private List<MidiMoment> GetSingleMomentsList(SortedDictionary<int, List<MidiMoment>> momentsListDict)
        {
            List<MidiMoment> midiMoments = new List<MidiMoment>();
            MidiMoment midiMoment = null;
            foreach(int pos in momentsListDict.Keys)
            {
                List<MidiMoment> momentsList = momentsListDict[pos];
                if(momentsList.Count == 1)
                {
                    midiMoment = momentsList[0];
                }
                else
                {
                    midiMoment = new MidiMoment(pos);
                    foreach(MidiMoment mm in momentsList)
                    {
                        foreach(MidiChord midiChord in mm.MidiChords)
                        {
                            midiMoment.MidiChords.Add(midiChord);
                        }
                    }
                }
                midiMoments.Add(midiMoment);
            }
            return midiMoments;
        }
        private void SetMomentsMsWidths(List<MidiMoment> moments)
        {
            if(moments.Count > 1)
            {
                for(int i = 1; i < moments.Count; i++)
                {
                    moments[i - 1].MsWidth = moments[i].MsPosition - moments[i - 1].MsPosition;
                }
            }

            if(moments.Count > 0)
            {
                // The msWidth of the last moment is the msDuration of its widest
                // contained durationSymbol (chord or rest).
                // If there is only one moment, the last moment is also the first.
                MidiMoment lastMoment = moments[moments.Count - 1];
                lastMoment.MsWidth = 0;
                foreach(MidiChord midiChord in lastMoment.MidiChords)
                {
                    if(midiChord.MsDuration > lastMoment.MsWidth)
                        lastMoment.MsWidth = midiChord.MsDuration;
                }
            }
        }
        /// <summary>
        /// When logging the delay between the onset of a moment and one of its contained chords,
        /// the chord needs access to the moment.
        /// </summary>
        /// <param name="moments"></param>
        private void SetChordContainers(List<MidiMoment> moments)
        {
            foreach(MidiMoment moment in moments)
            {
                foreach(MidiChord midiChord in moment.MidiChords)
                {
                    if(midiChord != null)
                    {
                        midiChord.Container = moment;
                    }
                }
            }
        }

        #endregion

        #region private variables
        private MoritzPerformanceOptions _performanceOptions;
        private SortedDictionary<int, TimeControl> _msPosTimeControlsDict = null;
        private List<MidiMoment> _performersMoments = new List<MidiMoment>();
        private List<MidiMoment> _assistantsMoments = new List<MidiMoment>();
        /// <summary>
        /// The channels which are actually used in this score
        /// </summary>
        private List<int> _midiChannels = new List<int>();
        #endregion
        #region public interface
        /// <summary>
        /// This function is used to set the default sounds/channels for when the score starts playing.
        /// It is called both when performance starts and when automatically repeating a performance.
        /// Uses _capxScore.Layout.StaffLayouts to set the default channel for each staff.
        /// For SVG scores, this function currently does nothing.
        /// </summary>
        public void InitializeMidiScoreContext()
        {
            #region set MIDI channels (capella only)
            //if(_capxScore != null)
            //{
            //    if(_capxScore.Layout.StaffLayouts.Count > M.MaximumNumberOfMidiChannels)
            //    {
            //        throw new ApplicationException(
            //            "Unfortunately, this score can't be performed because it\n" +
            //            "has more than " + M.MaximumNumberOfMidiChannels.ToString() + " staves per system.\n" +
            //            "(Moritz currently allocates one MIDI channel per staff,\n" +
            //            "and there are only " + M.MaximumNumberOfMidiChannels.ToString() + " MIDI channels.)");
            //    }

            //    List<int> availableMidiChannels = new List<int>();
            //    for(int i = 0; i < M.MaximumNumberOfMidiChannels; i++)
            //        availableMidiChannels.Add(i);

            //    foreach(Moritz.CapXMLScore.StaffLayout staffLayout in _capxScore.Layout.StaffLayouts)
            //    {
            //        if(!(availableMidiChannels.Count == 0))
            //        {
            //            if(!availableMidiChannels.Contains(staffLayout.Sounds[0].PreferredChannel))
            //            {
            //                staffLayout.Sounds[0].PreferredChannel = availableMidiChannels[0];
            //            }
            //            availableMidiChannels.Remove(staffLayout.Sounds[0].PreferredChannel);
            //        }
            //    }
            //}
            #endregion
        }
        /// <summary>
        /// This function clears performanceOptions, the performersMoments and the assistantsMoments.
        /// (It also does some internal cleanup for capXML scores.)
        /// Called when performance stops.
        /// </summary>
        public void Clear()
        {
            #region capella
            //if(_capxScore != null)
            //{
            //    foreach(List<Moritz.CapXMLScore.Staff> instrumentPart in _instrumentParts)
            //        instrumentPart.Clear();
            //    _instrumentParts.Clear();
            //    _capxScore = null;
            //}
            #endregion capella

            _performanceOptions = null;
            _performersMoments.Clear();
            _assistantsMoments.Clear();
        }

        public MoritzPerformanceOptions PerformanceOptions { get { return _performanceOptions; } }
        /// <summary>
        /// See the comment to TimeControl for an interesting explanation
        /// of why the types in LogPosTimeControlsDict are the way they are.
        /// </summary>
        public SortedDictionary<int, TimeControl> MsPosTimeControlsDict { get { return _msPosTimeControlsDict; } }
        public List<MidiMoment> PerformersMoments { get { return _performersMoments; } }
        public List<MidiMoment> AssistantsMoments { get { return _assistantsMoments; } }
        public List<int> MidiChannels { get { return _midiChannels; } }
        #endregion
    }

    internal class ErrorInScoreException : ApplicationException
    {
        public ErrorInScoreException(string message)
            : base(message)
        { }
    }
}
