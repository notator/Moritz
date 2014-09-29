using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using System;
using System.Linq;
using System.IO;

using Multimedia.Midi;

using Moritz.Globals;
using Moritz.Symbols;
using Moritz.Midi;

namespace Moritz.Performer
{
    internal partial class AssistantPerformerRuntime : IMidiSink, IMidiSource
    {
        #region constructor
        public AssistantPerformerRuntime(MidiInputDevice midiInputDevice, MidiOutputDevice midiOutputDevice,
            SvgScore svgScore, MoritzPerformanceOptions moritzPerformanceOptions, bool isAssistantOnlyPerformance, 
            ReportPositionDelegate reportPosition,
            NotifyCompletionDelegate notifyCompletion,
            SaveSequenceAsMidiFileDelegate saveSequenceAsMidifile)
        {
            _midiInputDevice = midiInputDevice;
            _midiInputDevice.Connect(this);
            _midiOutputDevice = midiOutputDevice;
            this.Connect(_midiOutputDevice);

            ReportPosition = reportPosition;
            NotifyCompletion = notifyCompletion;
            SaveSequenceAsMidifile = saveSequenceAsMidifile; 
            _midiScore = new MidiScore(svgScore, moritzPerformanceOptions, isAssistantOnlyPerformance);
            GetMidiVariables();
        }

        private void GetMidiVariables()
        {
            if(_midiScore != null)
            {
                _performanceOptions = _midiScore.PerformanceOptions;
                _msPosTimeControlsDict = _midiScore.MsPosTimeControlsDict;
                _performersMoments = _midiScore.PerformersMoments;
                _assistantsMoments = _midiScore.AssistantsMoments;

                if(_performersMoments.Count > 0)
                    _performersFinalChordOffPosition =
                        _performersMoments.Last().MsPosition + _performersMoments.Last().MsWidth;

                if(_assistantsMoments.Count > 0)
                    _assistantsFinalChordOffPosition =
                        _assistantsMoments.Last().MsPosition + _assistantsMoments.Last().MsWidth;

                _finalChordOffPosition =
                    (_performersFinalChordOffPosition > _assistantsFinalChordOffPosition) ?
                    _performersFinalChordOffPosition : _assistantsFinalChordOffPosition;
            }
        }
        #endregion constructor

        #region IMidiSource
        public void Connect(IMidiSink midiOutputDevice)
        {
            SendChordMessage += new ChordMessageDelegate(midiOutputDevice.ProcessMessage);
            SendChannelMessage += new ChannelMessageDelegate(midiOutputDevice.ProcessMessage);
            SendSysExMessage += new SysExMessageDelegate(midiOutputDevice.ProcessMessage);
            SendSysCommonMessage += new SysCommonMessageDelegate(midiOutputDevice.ProcessMessage);
            SendSysRealtimeMessage += new SysRealtimeMessageDelegate(midiOutputDevice.ProcessMessage);
        }
        public void Disconnect(IMidiSink midiOutputDevice)
        {
            SendChordMessage -= new ChordMessageDelegate(midiOutputDevice.ProcessMessage);
            SendChannelMessage -= new ChannelMessageDelegate(midiOutputDevice.ProcessMessage);
            SendSysExMessage -= new SysExMessageDelegate(midiOutputDevice.ProcessMessage);
            SendSysCommonMessage -= new SysCommonMessageDelegate(midiOutputDevice.ProcessMessage);
            SendSysRealtimeMessage -= new SysRealtimeMessageDelegate(midiOutputDevice.ProcessMessage);
        }
        public bool IsRunning { get { return _isRunning; } }
        public void StopMidiStreaming()
        {
            if(_isRunning) // disconnect
            {
                if(_midiInputDevice != null)
                {
                    _midiInputDevice.StopMidiStreaming();
                    _midiInputDevice.Disconnect(this);
                }

                if(_midiOutputDevice != null)
                {
                    _midiOutputDevice.StopMidiStreaming();
                    Disconnect(_midiOutputDevice);
                }

                _isRunning = false;

                if(NotifyCompletion != null)
                    NotifyCompletion();
            }
        }
        public void StartMidiStreaming()
        {
            if(_isRunning == false) // connect
            {
                if(_midiInputDevice != null)
                {
                    _midiInputDevice.StartMidiStreaming();
                }
                if(_midiOutputDevice != null)
                {
                    string defaultMidiFilename = "";
                    if(_performanceOptions.SaveMidiFile)
                    {
                        defaultMidiFilename = Path.GetFileNameWithoutExtension(_performanceOptions.FileName) + ".mid";
                        //midiFilepath = AP.MoritzMidiFilesFolder + @"\" + defaultMidiFilename; 
                    }
                    _midiOutputDevice.StartMidiStreaming(this, SaveSequenceAsMidifile, defaultMidiFilename);
                }
                _isRunning = true;
            }
        }
        #endregion

        #region IMidiSink
        public void ProcessMessage(ChordMessage performersChord)
        {
            MidiMoment moment = null;
            if(SendChannelMessage != null)
            {
                ChordOn pOn = performersChord as ChordOn;
                if(pOn != null)
                {
                    moment = GetMoment();
                }

                ChordMessage assistedChord = null;
                ChordMessage solo_AssistantHearsFirst_Chord = null;
                ChordMessage solo_AssistantHearsNothing_Chord = null;
                SplitChord(performersChord, out assistedChord, out solo_AssistantHearsFirst_Chord, out solo_AssistantHearsNothing_Chord);

                if(assistedChord != null)
                {
                    if(_pendingSoloChordOff != null)
                    {
                        DoAssistedChord(moment, _pendingSoloChordOff);
                        _pendingSoloChordOff = null;
                    }
                    DoAssistedChord(moment, assistedChord);
                }
                else if(solo_AssistantHearsFirst_Chord != null)
                {
                    if(_pendingSoloChordOff != null)
                    {
                        this._performanceState.Send(solo_AssistantHearsFirst_Chord);
                    }
                    else
                    {
                        if(solo_AssistantHearsFirst_Chord is ChordOn)
                        {
                            DoAssistedChord(moment, solo_AssistantHearsFirst_Chord);
                            _pendingSoloChordOff = solo_AssistantHearsFirst_Chord.CloneChordOff();
                        }
                    }
                }
                else if(solo_AssistantHearsNothing_Chord != null)
                {
                    this._performanceState.Send(solo_AssistantHearsNothing_Chord);
                }
            }
        }
        public void ProcessMessage(ChannelMessage message)
        {
            // This node uses chord messages instead of noteOn and NoteOff! (see above)
            if(this.SendChannelMessage != null)
                this.SendChannelMessage(message);
        }
        public void ProcessMessage(SysExMessage message)
        {
            // other processing goes here
            if(this.SendSysExMessage != null)
                this.SendSysExMessage(message);
        }
        public void ProcessMessage(SysCommonMessage message)
        {
            // other processing goes here
            if(this.SendSysCommonMessage != null)
                this.SendSysCommonMessage(message);
        }
        public void ProcessMessage(SysRealtimeMessage message)
        {
            // other processing goes here
            if(this.SendSysRealtimeMessage != null)
                this.SendSysRealtimeMessage(message);
        }
        #endregion

        #region Stop/Go (public interface)
        public void Stop()
        {
            if(_assistant != null)
                _assistant.CancelPerformance();
            StopMidiStreaming();
        }
        public void Go()
        {
            StartMidiStreaming();
            StartExecution();
        }
        #endregion

        #region private implementation of ProcessMessage(performersChord)
        #region private functions
        private MidiMoment GetMoment()
        {
            MidiMoment moment = null;
            if(_performersMoments.Count > 0 && _momentIndex < _performersMoments.Count)
            {
                moment = _performersMoments[_momentIndex];
            }
            else if(_assistantsMoments.Count > 0 && _momentIndex < _assistantsMoments.Count)
            {
                moment = _assistantsMoments[_momentIndex];
            }
            return moment;
        }
        /// <summary>
        /// This is the usual case. The assistant hears and reacts to the performer's chord.
        /// </summary>
        /// <param name="assistedChord"></param>
        private void DoAssistedChord(MidiMoment moment, ChordMessage assistedChord)
        {
            if(_assistantIsPlayingAloneToEndOfPiece == false)
            {
                ChordOn performersChordOn = assistedChord as ChordOn;
                ChordOff performersChordOff = assistedChord as ChordOff;
                AssistantsDurationsType assistantsDurationsType = _performanceOptions.AssistantsDurationsType;

                if(performersChordOn != null)
                {
                    #region handle performer's ChordOn
                    if(_performersMoments.Count > 0)
                    {
                        SetPerformersMoment(moment, performersChordOn, _performanceOptions);

                        if(_momentIndex == _momentStartIndex)
                        {
                            _performanceState.Initialize(moment);
                            _momentStartIndex = 0; // for repeat
                        }
                        else
                            _performanceState.SetPerformedMoment(moment);

                        int thisMomentPosition = moment.MsPosition;
                        if(_momentIndex < _performersMoments.Count - 1)
                        {
                            _nextMomentMsPosition = _performersMoments[_momentIndex + 1].MsPosition;
                        }
                        else
                        {
                            _nextMomentMsPosition = moment.MsPosition + moment.MsWidth;
                        }

                        if(!(_momentIndex == _performersMoments.Count - 1
                        && _momentIndex > 0
                        && _msPosTimeControlsDict[_performersMoments[_momentIndex - 1].MsPosition].IsGeneralPause))
                        {
                            _performanceState.Perform(moment.MidiChords, thisMomentPosition);
                        }

                        if(_assistantsMoments.Count > 0)
                        {
                            int assistantsFromIndex = 0;
                            int assistantsMsPosition = 0;
                            _assistant.GetPosition(thisMomentPosition, _nextMomentMsPosition,
                                out assistantsFromIndex, out assistantsMsPosition);

                            if(assistantsMsPosition >= thisMomentPosition
                            && assistantsMsPosition < _nextMomentMsPosition)
                            {
                                _assistant.Perform(moment.StartTimeMilliseconds,
                                    thisMomentPosition, _nextMomentMsPosition, _performersFinalChordOffPosition,
                                    assistantsFromIndex, assistantsMsPosition);
                            }
                        }

                        IncrementPerformersIndex();
                    }
                    else
                    {   // _performersMoments.Count == 0. moment is an assistantsMoment
                        _performanceState.Initialize(moment);
                        _assistant.PerformAlone(moment.MsPosition, _momentIndex, UpdateAssistantsIndexDisplay);
                    }
                    #endregion handle performer's ChordOn
                }
                else if(performersChordOff != null && _performersMoments.Count > 0)
                {
                    #region handle performer's ChordOff
                    _performanceState.UpdateForPerformersChordOff(_nextMomentMsPosition);

                    int thisChordOffPosition;
                    if(_momentIndex < _performersMoments.Count)
                        thisChordOffPosition = _performersMoments[_momentIndex].MsPosition;
                    else
                        thisChordOffPosition = _performersFinalChordOffPosition;

                    SendPreviousMomentsOffControls();

                    if(_momentIndex > 0 && _momentIndex < _performersMoments.Count)
                    {
                        if(_msPosTimeControlsDict[_performersMoments[_momentIndex].MsPosition].IsGeneralPause)
                        {
                            if(_momentIndex == _performersMoments.Count - 1)
                            {
                                _performersMoments[_momentIndex].StartTimeMilliseconds = M.NowMilliseconds;
                                if(_assistantsMoments.Count > 0
                                && _msPosTimeControlsDict[_assistantsMoments[_assistantsMoments.Count - 1].MsPosition].IsGeneralPause)
                                {
                                    _assistantsMoments[_assistantsMoments.Count - 1].StartTimeMilliseconds =
                                        _performersMoments[_momentIndex].StartTimeMilliseconds;
                                }

                                //StopMidiStreaming();
                                CheckRepeat(_performanceState);
                            }
                            else
                                StartImmediately(); // send a dummy chord to ProcessMessage() to simulate performer sending a chordOn.
                        }
                    }

                    if(_performersMoments != null && _momentIndex == _performersMoments.Count)
                    {
                        // The performer has sent a chordOff, and _momentIndex has completed the _performersMoments.
                        // Now check if the assistant has any more moments to play, and if so play them.
                        if(_assistantsMoments != null && _assistantsMoments.Count > 0)
                        {
                            MidiMoment lastAssistantsMoment = _assistantsMoments[_assistantsMoments.Count - 1];
                            if(lastAssistantsMoment.MsPosition > _performersFinalChordOffPosition)
                            {
                                #region if the assistant has more moments to play, play them
                                int beginAtPosition = 0;
                                int beginAtAssistantsIndex = 0;
                                for(int momentIndex = 0; momentIndex < _assistantsMoments.Count; ++momentIndex)
                                {
                                    if(_assistantsMoments[momentIndex].MsPosition >= _performersFinalChordOffPosition)
                                    {
                                        if(_assistantsMoments[momentIndex].MsPosition > _performersFinalChordOffPosition)
                                        {
                                            Thread.Sleep(_assistantsMoments[momentIndex].MsPosition - _performersFinalChordOffPosition);
                                        }
                                        beginAtAssistantsIndex = momentIndex;
                                        beginAtPosition = _assistantsMoments[momentIndex].MsPosition;
                                        break;
                                    }
                                }

                                _assistantIsPlayingAloneToEndOfPiece = true;
                                _assistant.Perform(M.NowMilliseconds, beginAtPosition, lastAssistantsMoment.MsPosition + 100,
                                    _assistantsFinalChordOffPosition, beginAtAssistantsIndex, beginAtPosition);

                                //Thread.Sleep(_assistantsFinalChordOffPosition - beginAtPosition + 100);
                                while(_assistant.IsBusy)
                                    Thread.Sleep(0);
                 
                                thisChordOffPosition = _assistantsFinalChordOffPosition;
                                #endregion
                            }
                        }
                        StopMidiStreaming();
                    }

                    if(thisChordOffPosition == _finalChordOffPosition)
                        CheckRepeat(_performanceState);

                    #endregion handle performer's ChordOff
                }
            }
        }
        /// <summary>
        /// Starts an assistant-only performance by sending a dummy chord to ProcessMessage()
        /// </summary>
        private void StartImmediately()
        {
            ChordOn startChord = new ChordOn(null);
            startChord.Notes.Add(new NoteOn());
            startChord.Notes[0].Channel = 0;
            startChord.Notes[0].Pitch = 64;
            startChord.Notes[0].Velocity = 64;
            _performanceOptions.KeyboardSettings[0][startChord.Notes[0].Pitch] = KeyType.Assisted;

            ProcessMessage(startChord);
        }
        /// <summary>
        /// Set the starting time of the moment. 
        /// Also set the dynamics and pitches of the performer's Chords according to what the
        /// performer actually plays and the performance options set in the score.
        /// </summary>
        private void SetPerformersMoment(MidiMoment performersMoment, ChordMessage performersChordOn,
            MoritzPerformanceOptions performanceOptions)
        {
            performersMoment.StartTimeMilliseconds = M.NowMilliseconds;
            performersMoment.PerformedDelay = 0; // Performers moments never have a delay.

            foreach(MidiChord midiChord in performersMoment.MidiChords)
            {
                if(midiChord != null)
                {
                    SetPerformersMidiChord(midiChord, performersChordOn,
                        performanceOptions.PerformersPitchesType, performanceOptions.PerformersDynamicsType);
                }
            }
        }
        private void SetPerformersMidiChord(MidiChord midiChord,
            ChordMessage performersChordOn,
            PerformersPitchesType pitchAssistanceType, PerformersDynamicsType dynamicsAssistanceType)
        {
            ChannelMessageBuilder noteOnMessageBuilder = new ChannelMessageBuilder();
            noteOnMessageBuilder.Command = ChannelCommand.NoteOn;
            ChannelMessageBuilder noteOffMessageBuilder = new ChannelMessageBuilder();
            noteOffMessageBuilder.Command = ChannelCommand.NoteOff;

            switch(dynamicsAssistanceType)
            {
                case PerformersDynamicsType.AsNotated:
                    break;
                case PerformersDynamicsType.AsPerformed:
                    midiChord.Velocity = (byte)performersChordOn.Notes[0].Velocity;
                    break;
                case PerformersDynamicsType.Silent:
                    midiChord.Velocity = 0;
                    break;
            }

            switch(pitchAssistanceType)
            {
                case PerformersPitchesType.AsNotated:
                    break;
                case PerformersPitchesType.AsPerformed:
                    List<byte> midiPitches = new List<byte>();
                    foreach(NoteMessage noteMessage in performersChordOn.Notes)
                        midiPitches.Add((byte)noteMessage.Pitch);
                    midiChord.ResetMidiPitches(midiPitches);
                    break;
            }
        }
        private void IncrementPerformersIndex()
        {
            int msPosition = _performersMoments[_momentIndex].MsPosition;
            _momentIndex++;

            if(_momentIndex == _performersMoments.Count)
            {
                if(_performanceOptions.RepeatPerformance)
                {
                    _momentIndex = 0;
                    _midiScore.InitializeMidiScoreContext();
                    _performanceState.Initialize(_performersMoments[0]);
                }
                else if(_msPosTimeControlsDict[_performersMoments[_momentIndex - 1].MsPosition].IsGeneralPause)
                {
                    //Thread.Sleep(100); // let the assistant's thread finish
                    StopMidiStreaming();
                }
            }

            if(ReportPosition != null)
                ReportPosition(msPosition);
        }
        /// <summary>
        /// Called as a delegate by the assistant if it is playing alone, without a performer.
        /// </summary>
        /// <param name="assistantsIndex"></param>
        private void UpdateAssistantsIndexDisplay(int assistantsIndex)
        {
            if(ReportPosition != null)
            {
                ReportPosition(_assistantsMoments[assistantsIndex - 1].MsPosition);
            }
        }
        /// <summary>
        /// Moves NoteMessages from the performer's chord into the appropriate out ChordMessage.
        /// </summary>
        /// <remarks>
        /// Notes of KeyType.Silent are simply swallowed by (never returned from) this function.
        /// Only one of the output Chords can be returned as non-null:
        /// If there is at least one KeyType.Assisted note in the performer's chord, all notes are returned
        ///   in the assistedChord.
        /// Else if there is at least one KeyType.Solo_AssistantHearsFirst note in the performer's chord, all notes
        ///   are returned in the soloRestChord.
        /// Else if there is at least one KeyType.Solo_AssistantHearsNothing note in the performer's chord,
        ///   all notes are returned in the soloTieChord.
        /// Else all notes were of type KeyType.Silent, and all out ChordMessages are returned null.
        /// </remarks>
        private void SplitChord(ChordMessage performersChord,
            out ChordMessage assistedChord, out ChordMessage solo_AssistantHearsFirst_Chord, out ChordMessage solo_AssistantHearsNothing_Chord)
        {
            #region create NoteMessage lists
            List<NoteMessage> assistedNotes = new List<NoteMessage>();
            List<NoteMessage> solo_AssistantHearsFirst_Notes = new List<NoteMessage>();
            List<NoteMessage> solo_AssistantHearsNothing_Notes = new List<NoteMessage>();
            foreach(NoteMessage noteMessage in performersChord.Notes)
            {
                switch(_performanceState.KeyboardSettings[_performanceState.KeyboardSettingsIndex]
                    [noteMessage.Pitch])
                {
                    case KeyType.Assisted:
                    assistedNotes.Add(noteMessage);
                    break;
                    case KeyType.Solo_AssistantHearsFirst:
                    solo_AssistantHearsFirst_Notes.Add(noteMessage);
                    break;
                    case KeyType.Solo_AssistantHearsNothing:
                    solo_AssistantHearsNothing_Notes.Add(noteMessage);
                    break;
                }
            }
            #endregion create NoteMessage lists

            assistedChord = null;
            solo_AssistantHearsNothing_Chord = null;
            solo_AssistantHearsFirst_Chord = null;
            if(assistedNotes.Count > 0)
            {
                assistedChord = performersChord;
                assistedChord.Notes = assistedNotes;
                assistedChord.Notes.AddRange(solo_AssistantHearsFirst_Notes);
                assistedChord.Notes.AddRange(solo_AssistantHearsNothing_Notes);
            }
            else
            {
                ChordOn chordOn = performersChord as ChordOn;
                ChordOff chordOff = performersChord as ChordOff;
                if(chordOn != null)
                {
                    if(solo_AssistantHearsFirst_Notes.Count > 0)
                    {
                        solo_AssistantHearsFirst_Chord = chordOn.CloneChordOn();
                        solo_AssistantHearsFirst_Chord.Notes = solo_AssistantHearsFirst_Notes;
                        solo_AssistantHearsFirst_Chord.Notes.AddRange(solo_AssistantHearsNothing_Notes);
                    }
                    else if(solo_AssistantHearsNothing_Notes.Count > 0)
                    {
                        solo_AssistantHearsNothing_Chord = chordOn.CloneChordOn();
                        solo_AssistantHearsNothing_Chord.Notes = solo_AssistantHearsNothing_Notes;
                    }
                }
                else if(chordOff != null)
                {
                    if(solo_AssistantHearsFirst_Notes.Count > 0)
                    {
                        solo_AssistantHearsFirst_Chord = chordOff.CloneChordOff();
                        solo_AssistantHearsFirst_Chord.Notes = solo_AssistantHearsFirst_Notes;
                        solo_AssistantHearsFirst_Chord.Notes.AddRange(solo_AssistantHearsNothing_Notes);
                    }
                    else if(solo_AssistantHearsNothing_Notes.Count > 0)
                    {
                        solo_AssistantHearsNothing_Chord = chordOff.CloneChordOff();
                        solo_AssistantHearsNothing_Chord.Notes = solo_AssistantHearsNothing_Notes;
                    }
                }
            }
        }
        /// <summary>
        /// Called in the UI thread from AssistantsChordOns_Completed()
        /// when a performance reaches the end of the score.
        /// </summary>
        /// <param name="assistantsIndex"></param>
        private void CheckRepeat(PerformanceState performanceState)
        {
            if(_performanceOptions.RepeatPerformance)
            {
                // auto repeat
                if(_performersMoments == null || _performersMoments.Count == 0)
                    Thread.Sleep(1500);
                StartExecution();
            }
            else
            {
                Thread.Sleep(200); // prevents an audible glitch
                this.StopMidiStreaming();
            }
        }
        private void StartExecution()
        {
            try
            {
                _momentIndex = _performanceOptions.StartAtMoment - 1;
                _momentIndex = (_momentIndex < 0) ? 0 : _momentIndex;
                int startMsPosition = 0;
                if(_performersMoments.Count > 0)
                {
                    if(_momentIndex >= _performersMoments.Count)
                        _momentIndex = 0;
                    startMsPosition = _performersMoments[_momentIndex].MsPosition;
                }
                else if(_performersMoments.Count == 0)
                {
                    if(_momentIndex >= _assistantsMoments.Count)
                        _momentIndex = 0;
                    startMsPosition = _assistantsMoments[_momentIndex].MsPosition;
                }
                _momentStartIndex = _momentIndex;

                if(SendChannelMessage != null)
                {
                    //_performanceState = new PerformanceState(SendChannelMessage, SendChordMessage,
                    //    _loudnessStates, _performanceOptions, _midiScore.MidiSchedule, _isRecording);
                    _performanceState = new PerformanceState(SendChannelMessage, SendChordMessage,
                            _performanceOptions, _midiScore.MsPosTimeControlsDict, _midiScore.MidiChannels);

                    _performanceState.ResetChannels(); // sends AllNotesOff and AllControllersOff to each channel

                    _assistant = new Assistant(_assistantsMoments, _performanceState,
                        _midiScore.MsPosTimeControlsDict, CheckRepeat);
                }

                SetTrackInitializationData(startMsPosition);

                // If this is an assistant-only performance, start immediately.
                // Dont wait for midi input (keyboard).
                if(_performersMoments.Count == 0)
                {
                    StartImmediately();
                }
            }
            catch(ApplicationException aex)
            {
                throw (aex);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// If the performance does not start at the beginning of the piece, 
        /// this function adds chased MidiControls to the first MidiChord which is
        /// going to be played by the performer or assistant in each channel. 
        /// </summary>
        private void SetTrackInitializationData(int startMsPosition)
        {
            if(startMsPosition > 0)
            {
                for(int channel = 0; channel < _performanceOptions.MoritzPlayers.Count; channel++)
                {
                    if(_performanceOptions.MoritzPlayers[channel] != MoritzPlayer.None)
                    {
                        SetChasedControlsInFirstPerformedChord(_performersMoments, channel, startMsPosition);
                        SetChasedControlsInFirstPerformedChord(_assistantsMoments, channel, startMsPosition);
                    }
                }
            }
        }

        private void SetChasedControlsInFirstPerformedChord(List<MidiMoment> moments, int channel, int startMsPosition)
        {
            MidiChord startMidiChord = null;
            Volume firstVolume = new Volume(channel, M.DefaultVolume, ControlContinuation.NoChange);
            PitchWheelDeviation firstPitchWheelDeviation = new PitchWheelDeviation(channel, M.DefaultPitchWheelDeviation);
            MidiChordSlider firstPitchWheelSlider = null;
            MidiChordSlider firstPanSlider = null;
            MidiChordSlider firstModulationWheelSlider = null;
            MidiChordSlider firstExpressionSlider = null;

            BankControl firstBank = null;
            PatchControl firstPatch = null;

            foreach(MidiMoment moment in moments)
            {
                if(moment.MsPosition > startMsPosition)
                    break;
                foreach(MidiChord midiChord in moment.MidiChords)
                {
                    if(midiChord.Channel == channel)
                    {
                        startMidiChord = midiChord;

                        firstBank = (midiChord.Bank != null) ? midiChord.Bank : firstBank;
                        firstPatch = (midiChord.Patch != null) ? midiChord.Patch : firstPatch;
                        firstVolume = (midiChord.Volume != null) ? midiChord.Volume : firstVolume;
                        firstPitchWheelDeviation = (midiChord.PitchWheelDeviation != null) ? midiChord.PitchWheelDeviation : firstPitchWheelDeviation;
                        firstPitchWheelSlider = (midiChord.PitchWheelSlider != null) ? midiChord.PitchWheelSlider : firstPitchWheelSlider;
                        firstPanSlider = (midiChord.PanSlider != null) ? midiChord.PanSlider : firstPanSlider;
                        firstModulationWheelSlider = (midiChord.ModulationWheelSlider != null) ? midiChord.ModulationWheelSlider : firstModulationWheelSlider;
                        firstExpressionSlider = (midiChord.ExpressionSlider != null) ? midiChord.ExpressionSlider : firstExpressionSlider;

                        BasicMidiChord bmc = midiChord.BasicMidiChords[0];
                        firstBank = (bmc.BankControl != null) ? bmc.BankControl : firstBank;
                        firstPatch = (bmc.PatchControl != null) ? bmc.PatchControl : firstPatch;
                    }
                }
            }

            if(startMidiChord != null)
            {
                startMidiChord.Bank = firstBank;
                startMidiChord.Patch = firstPatch;
                startMidiChord.Volume = firstVolume;
                startMidiChord.PitchWheelDeviation = firstPitchWheelDeviation;
                startMidiChord.PitchWheelSlider = firstPitchWheelSlider;
                startMidiChord.PanSlider = firstPanSlider;
                startMidiChord.ModulationWheelSlider = firstModulationWheelSlider;
                startMidiChord.ExpressionSlider = firstExpressionSlider;

                startMidiChord.BasicMidiChords[0].BankControl = firstBank;
                startMidiChord.BasicMidiChords[0].PatchControl = firstPatch;
            }
        }
        /// <summary>
        /// If a MidiOffControl is attached to the right of its durationSymbol, its messages are put in the
        /// durationSymbol's OffMessages. This function (which is called when the performer issues a ChordOff)
        /// sends such messages. There is no need to keep track of such messages in the PerformanceState.
        /// </summary>
        private void SendPreviousMomentsOffControls()
        {
            MidiMoment previousMoment = null;
            if(_momentIndex > 0)
            {
                if(_performersMoments.Count > 0)
                {
                    previousMoment = _performersMoments[_momentIndex - 1];
                }
                else if(_assistantsMoments.Count > 0)
                {
                    previousMoment = _assistantsMoments[_momentIndex - 1];
                }

                foreach(MidiDurationSymbol midiDurationSymbol in previousMoment.MidiChords)
                {
                    if(SendChannelMessage != null && midiDurationSymbol.OffMessages.Count > 0)
                    {
                        foreach(ChannelMessage message in midiDurationSymbol.OffMessages)
                            SendChannelMessage(message);
                    }
                }
            }
        }
        #endregion private functions
        #region private variables
        private SortedDictionary<int, TimeControl> _msPosTimeControlsDict = null;
        private List<MidiMoment> _performersMoments = null; // only ever used when set to _midiScore.PerformersMoments
        private List<MidiMoment> _assistantsMoments = null; // only ever used when set to _midiScore.AssistantsMoments
        private int _performersFinalChordOffPosition = 0;
        private int _assistantsFinalChordOffPosition = 0;
        private int _finalChordOffPosition = 0;
        private bool _assistantIsPlayingAloneToEndOfPiece = false;
        /// <summary>
        /// currentIndex in the performed moments
        /// </summary>
        private int _momentIndex;
        /// <summary>
        /// Is set to the corresponding ChordOff when the performer has sent a Solo_AssistantRest KeyType chord.
        /// Is set to null when other chord types are sent.
        /// </summary>
        private ChordOff _pendingSoloChordOff = null;
        /// <summary>
        /// The _momentIndex with which the performance starts.
        /// </summary>
        private int _momentStartIndex;
        private Assistant _assistant = null;
        private PerformanceState _performanceState = null;
        private int _nextMomentMsPosition = 0;
        #endregion private variables
        #endregion

        private ChordMessageDelegate SendChordMessage;
        private ChannelMessageDelegate SendChannelMessage;
        private SysExMessageDelegate SendSysExMessage;
        private SysCommonMessageDelegate SendSysCommonMessage;
        private SysRealtimeMessageDelegate SendSysRealtimeMessage;

        private ReportPositionDelegate ReportPosition = null;
        private NotifyCompletionDelegate NotifyCompletion = null;
        private SaveSequenceAsMidiFileDelegate SaveSequenceAsMidifile = null;

        private bool _isRunning = false;
        private MidiInputDevice _midiInputDevice = null;
        private MidiOutputDevice _midiOutputDevice = null;
        private MidiScore _midiScore = null;
        private MoritzPerformanceOptions _performanceOptions = null;
    }
}
