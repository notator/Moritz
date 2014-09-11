using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Diagnostics;

using Multimedia.Midi;
using Moritz.Score.Midi;

namespace Moritz.AssistantPerformer
{
    public class PerformanceState
    {
        public PerformanceState(ChannelMessageDelegate sendChannelMessage, ChordMessageDelegate sendChordMessage,
            MoritzPerformanceOptions performanceOptions,
            SortedDictionary<int, TimeControl> msPosTimeControlsDict,
            List<int> midiChannels)
        {
            SendChannelMessage = sendChannelMessage;
            SendChordMessage = sendChordMessage;
            _assistantsDurationsType = performanceOptions.AssistantsDurationsType;
            _midiChannels = midiChannels;
            _keyboardSettings = performanceOptions.KeyboardSettings;
            _msPosTimeControlsDict = msPosTimeControlsDict;

            CreateNewWorkersAndLocks();
        }

        private void CreateNewWorkersAndLocks()
        {
            _chordOffsBWs.Clear();

            // Note that in .NET 3.5, the ThreadPool appears to contain 2000 worker threads by default!
            // The number of chordOffs backgroundWorkers created here usually suffices, but more are created
            // dynamically when required (see GetIdleChordOffsBackgroundWorker() below).
            // ChordOffsBackground workers send ChordOffs (for all channels) which have been added to a
            // pending chordOffs list. They are not, like the other background workers here, associated
            // with particular channels.
            for(int i = 0; i < 40; ++i)
            {
                BackgroundWorker chordOffsBW = new BackgroundWorker();
                chordOffsBW.DoWork += new DoWorkEventHandler(BackgroundSendFutureChordOffs);
                chordOffsBW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundSendFutureChordOffs_Completed);
                chordOffsBW.WorkerSupportsCancellation = true;
                _chordOffsBWs.Add(chordOffsBW);
            }

            for(int i = 0; i < _midiChannels.Count; i++)
            { 
                for(int j = 0; j < _initialNumberOfBasicMidiChordWorkersPerChannel; ++j)
                {
                    BackgroundWorker basicMidiChordsBW = new BackgroundWorker();
                    basicMidiChordsBW.WorkerSupportsCancellation = true;
                    basicMidiChordsBW.DoWork += new DoWorkEventHandler(BasicMidiChordsBW_DoWork);
                    basicMidiChordsBW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundWorker_RunWorkerCompleted);
                    _basicMidiChordsBWs.Add(basicMidiChordsBW);
                }

                BackgroundWorker modSliderBW = new BackgroundWorker();
                modSliderBW.WorkerSupportsCancellation = true;
                modSliderBW.DoWork += new DoWorkEventHandler(SliderBW_DoWork);
                modSliderBW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundWorker_RunWorkerCompleted);
                _modulationWheelSliderBWs.Add(modSliderBW);

                BackgroundWorker panSliderBW = new BackgroundWorker();
                panSliderBW.WorkerSupportsCancellation = true;
                panSliderBW.DoWork += new DoWorkEventHandler(SliderBW_DoWork);
                panSliderBW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundWorker_RunWorkerCompleted);
                _panSliderBWs.Add(panSliderBW);

                BackgroundWorker pitchWheelSliderBW = new BackgroundWorker();
                pitchWheelSliderBW.WorkerSupportsCancellation = true;
                pitchWheelSliderBW.DoWork += new DoWorkEventHandler(SliderBW_DoWork);
                pitchWheelSliderBW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundWorker_RunWorkerCompleted);
                _pitchWheelSliderBWs.Add(pitchWheelSliderBW);

                BackgroundWorker expressionSliderBW = new BackgroundWorker();
                expressionSliderBW.WorkerSupportsCancellation = true;
                expressionSliderBW.DoWork += new DoWorkEventHandler(SliderBW_DoWork);
                expressionSliderBW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundWorker_RunWorkerCompleted);
                _expressionSliderBWs.Add(expressionSliderBW);
            }
        }

        /// <summary>
        /// Sends AllSoundOff and AllControllersOff messages to each of the channels currently in use.
        /// </summary>
        public void ResetChannels()
        {
            if(SendChannelMessage != null)
            {
                foreach(int channel in _midiChannels)
                {
                    AllSoundOff allSoundOff = new AllSoundOff(channel);
                    foreach(ChannelMessage channelMessage in allSoundOff.ChannelMessages)
                        SendChannelMessage(channelMessage);
                    AllControllersOff allControllersOff = new AllControllersOff(channel);
                    foreach(ChannelMessage channelMessage in allControllersOff.ChannelMessages)
                        SendChannelMessage(channelMessage);
                }
                Thread.Sleep(5); // avoid click
            }
        }

        void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            lock(sender)
            {
                BackgroundWorker worker = sender as BackgroundWorker;
                if(worker != null)
                {
                    if(!worker.CancellationPending)
                        worker.CancelAsync();
                    while(worker.IsBusy)
                        Thread.Sleep(0);
                }
            }
        }

        /// <summary>
        /// Called by the performer in the UI thread and by the assistant in a background thread.
        /// This function sends ChordOn events and updates the performance state for other pending messages.
        /// </summary>
        public void SendMidiDurationSymbolUpdateState(
            List<MidiChord> midiChords,
            int currentMsPosition)
        {
            if(currentMsPosition < _currentMsPosition)
                return;
            else
                _currentMsPosition = currentMsPosition;

            if(midiChords.Count > 0)
            {
                foreach(MidiChord midiChord in midiChords)
                    SendMidiChordInBackgroundUpdateState(midiChord);
            }
        }

        #region threads
        /// <summary>
        /// Sends the midiChord' basicMidiChords and its associated sliders in background threads, and
        /// immediately updates the state for the pending chordOffs.
        /// </summary>
        public void SendMidiChordInBackgroundUpdateState(MidiChord midiChord)
        {
            SendMidiChordInBackground(midiChord);
            
            if(midiChord.ChordOff != null)
                UpdatePendingChordOffs(midiChord);
        }

        private void SendMidiChordInBackground(MidiChord midiChord)
        {
            Debug.Assert(SendChannelMessage != null);

            #region send Bank, Patch, Volume and/or PitchWheelDeviation
            if(midiChord.Bank != null)
            {
                foreach(ChannelMessage channelMessage in midiChord.Bank.ChannelMessages)
                    SendChannelMessage(channelMessage);
            }
            if(midiChord.Patch != null)
            {
                foreach(ChannelMessage channelMessage in midiChord.Patch.ChannelMessages)
                    SendChannelMessage(channelMessage);
            }
            if(midiChord.Volume != null)
            {
                foreach(ChannelMessage channelMessage in midiChord.Volume.ChannelMessages)
                    SendChannelMessage(channelMessage);
            }

            if(midiChord.PitchWheelDeviation != null)
            {
                foreach(ChannelMessage channelMessage in midiChord.PitchWheelDeviation.ChannelMessages)
                    SendChannelMessage(channelMessage);
            }
            #endregion

            midiChord.BasicMidiChordsBackgroundWorker = RunBasicMidiChordBW(midiChord);

            #region sliders
            if(midiChord.ModulationWheelSlider != null)
            {
                RunBackgroundWorker(_modulationWheelSliderBWs, SliderBW_DoWork, BackgroundWorker_RunWorkerCompleted,
                                                    midiChord.ModulationWheelSlider);
            }
            if(midiChord.PanSlider != null)
            {
                RunBackgroundWorker(_panSliderBWs, SliderBW_DoWork, BackgroundWorker_RunWorkerCompleted,
                                                    midiChord.PanSlider);
            }
            if(midiChord.PitchWheelSlider != null)
            {
                RunBackgroundWorker(_pitchWheelSliderBWs, SliderBW_DoWork, BackgroundWorker_RunWorkerCompleted,
                                                    midiChord.PitchWheelSlider);
            }
            if(midiChord.ExpressionSlider != null)
            {
                RunBackgroundWorker(_expressionSliderBWs, SliderBW_DoWork, BackgroundWorker_RunWorkerCompleted,
                                                    midiChord.ExpressionSlider);
            }
            #endregion
        }

        private BackgroundWorker RunBasicMidiChordBW(MidiChord midiChord)
        {
            BackgroundWorker worker = 
                RunBackgroundWorker(_basicMidiChordsBWs, 
                BasicMidiChordsBW_DoWork, BackgroundWorker_RunWorkerCompleted, midiChord);
            return worker;
        }

        private BackgroundWorker RunBackgroundWorker(List<BackgroundWorker> workers,
            DoWorkEventHandler doWorkEventHandler,
            RunWorkerCompletedEventHandler runWorkerCompletedEventHandler,
            object runArg)
        {
            BackgroundWorker worker = null;
            lock(workers)
            {
                foreach(BackgroundWorker backgroundWorker in workers)
                {
                    if(!backgroundWorker.IsBusy)
                    {
                        worker = backgroundWorker;
                        break;
                    }
                }
                if(worker == null)
                {
                    worker = new BackgroundWorker();
                    worker.DoWork += new DoWorkEventHandler(doWorkEventHandler);
                    worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(runWorkerCompletedEventHandler);
                    worker.WorkerSupportsCancellation = true;
                    _chordOffsBWs.Add(worker);
                }
                worker.RunWorkerAsync(runArg);
            }
            return worker;
        }

        public void BasicMidiChordsBW_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            Debug.Assert(worker != null);

            MidiChord midiChord = e.Argument as MidiChord;
            //BackgroundWorker basicMidiChordsBW = midiChord.BasicMidiChordsBackgroundWorker;

            Debug.Assert(SendChannelMessage != null);
            Debug.Assert(SendChordMessage != null);
            lock(midiChord.BasicMidiChords)
            {
                foreach(BasicMidiChord bmc in midiChord.BasicMidiChords)
                {
                    bmc.SendBankAndPatch(SendChannelMessage);
                    bmc.Send(SendChordMessage); // sends in this thread
                }
                e.Result = midiChord;
            }
        }

        public void SliderBW_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            MidiChordSlider midiChordSlider = e.Argument as MidiChordSlider;

            Debug.Assert(worker != null);
            Debug.Assert(midiChordSlider != null);
            Debug.Assert(SendChordMessage != null);
            Debug.Assert(SendChannelMessage != null);

            foreach(MidiSliderTime sliderTime in midiChordSlider.MidiSliderTimes)
            {
                foreach(ChannelMessage channelMessage in sliderTime.MidiSlider.ChannelMessages)
                {
                    SendChannelMessage(channelMessage);
                }
                Thread.Sleep(sliderTime.MsDuration);
            }

            e.Result = midiChordSlider;
        }

        #endregion threads

        private void UpdatePendingChordOffs(MidiChord midiChord)
        {
            Debug.Assert(midiChord.ChordOff != null);

            int offPosition = midiChord.MsPosition + midiChord.MsDuration;

            lock(PendingChordOffs)
            {
                if(PendingChordOffs.ContainsKey(offPosition))
                {
                    PendingChordOffs[offPosition].Add(midiChord.ChordOff);
                }
                else
                {
                    List<ChordOff> chordOffs = new List<ChordOff>();
                    chordOffs.Add(midiChord.ChordOff);
                    PendingChordOffs.Add(offPosition, chordOffs);
                }
                Monitor.PulseAll(PendingChordOffs);
            }
        }

        public void SendPendingChannelMessages(int currentMsPosition)
        {
            lock(PendingChannelMessages)
            {
                if(PendingChannelMessages.ContainsKey(currentMsPosition))
                {
                    List<int> channelMessageKeysToRemove = new List<int>();
                    foreach(int position in PendingChannelMessages.Keys)
                    {
                        if(position > currentMsPosition)
                            break;

                        channelMessageKeysToRemove.Add(position);
                        List<ChannelMessage> offMessages = PendingChannelMessages[position];

                        foreach(ChannelMessage channelMessage in offMessages)
                        {
                            SendChannelMessage(channelMessage);
                        }
                    }
                    foreach(int position in channelMessageKeysToRemove)
                        PendingChannelMessages.Remove(position);
                }
                Monitor.PulseAll(PendingChannelMessages);
            }
        }

        public void PushOffMessages(MidiDurationSymbol midiDurationSymbol)
        {
            int offMsPosition = midiDurationSymbol.MsPosition + midiDurationSymbol.MsDuration;
            if(midiDurationSymbol.OffMessages.Count > 0)
            {
                lock(PendingChannelMessages)
                {
                    if(PendingChannelMessages.ContainsKey(offMsPosition))
                    {
                        PendingChannelMessages[offMsPosition].AddRange(midiDurationSymbol.OffMessages);
                    }
                    else
                    {
                        List<ChannelMessage> messages = new List<ChannelMessage>(midiDurationSymbol.OffMessages);
                        PendingChannelMessages.Add(offMsPosition, messages);
                    }
                    Monitor.PulseAll(PendingChannelMessages);
                }
            }
        }

        /// <summary>
        /// Sends ChordMessages due up to the currentMsPosition. Does NOT set _currentPosition = currentPosition.
        /// </summary>
        /// <param name="currentPosition"></param>
        public void SendPendingMessagesUpdateState(int currentMsPosition)
        {
            List<int> backgroundWorkerKeysToRemove = new List<int>();
            List<int> chordOffKeysToRemove = new List<int>();
            List<int> midiOrnamentsToRemove = new List<int>();

            #region cancel pending background sliders
            lock(PendingBackgroundSliders)
            {
                foreach(int msPosition in PendingBackgroundSliders.Keys)
                {
                    if(msPosition > currentMsPosition)
                        break;

                    backgroundWorkerKeysToRemove.Add(msPosition);
                    List<BackgroundWorker> offWorkers = PendingBackgroundSliders[msPosition];

                    foreach(BackgroundWorker bw in offWorkers)
                    {
                        if(bw.IsBusy && !bw.CancellationPending)
                            bw.CancelAsync();
                    }
                }
                foreach(int msPosition in backgroundWorkerKeysToRemove)
                    PendingBackgroundSliders.Remove(msPosition);
                Monitor.PulseAll(PendingBackgroundSliders);
            }
            #endregion cancel pending background sliders
            #region send pending chord offs
            lock(PendingChordOffs)
            {
                foreach(int msPosition in PendingChordOffs.Keys)
                {
                    if(msPosition > currentMsPosition)
                        break;

                    chordOffKeysToRemove.Add(msPosition);
                    List<ChordOff> offMessages = PendingChordOffs[msPosition];
                    foreach(ChordOff chordOff in offMessages)
                        SendChordMessage(chordOff);
                }
                foreach(int msPosition in chordOffKeysToRemove)
                    PendingChordOffs.Remove(msPosition);
                Monitor.PulseAll(PendingChordOffs);
            }
            #endregion send pending chord offs
            #region cancel currently running ornaments
            lock(PendingMidiChords)
            {
                List<int> ornamentKeysToRemove = new List<int>();
                foreach(int msPosition in PendingMidiChords.Keys)
                {
                    if(msPosition > currentMsPosition)
                        break;

                    foreach(MidiChord midiChord in PendingMidiChords[msPosition])
                    {
                        midiChord.Cancel();
                    }
                    ornamentKeysToRemove.Add(msPosition);
                }
                foreach(int msPosition in ornamentKeysToRemove)
                    PendingMidiChords.Remove(msPosition);
                Monitor.PulseAll(PendingMidiChords);
            }
            #endregion cancel currently running ornaments
        }

        /// <summary>
        /// Called by the performer in the UI thread.
        /// Perform the midi duration symbols at thisMomentMsPosition.
        /// </summary>
        public void Perform(List<MidiChord> performersMidiChords, int thisMomentMsPosition)
        {
            SendMidiDurationSymbolUpdateState(performersMidiChords, thisMomentMsPosition);
        }

        /// <summary>
        /// Called when the performer sends a ChordOff event.
        /// </summary>
        public void UpdateForPerformersChordOff(int chordOffMsPosition)
        {
            SendPendingMessagesUpdateState(chordOffMsPosition);
        }

        private class ChordOffsWorkerArgs
        {
            public int ThisChordOnMsPosition;
            public int NextChordOnMsPosition;
        }

        /// <summary>
        /// Called by the assistant.
        /// </summary>
        public void SendAssistantsFutureChordOffs(
            int assistantsChordOnMsPosition, int nextAssistantsChordOnMsPosition)
        {
            ChordOffsWorkerArgs args = new ChordOffsWorkerArgs();
            args.ThisChordOnMsPosition = assistantsChordOnMsPosition;
            args.NextChordOnMsPosition = nextAssistantsChordOnMsPosition;
            RunBackgroundWorker(_chordOffsBWs, BackgroundSendFutureChordOffs, BackgroundSendFutureChordOffs_Completed, args);
        }

        private void BackgroundSendFutureChordOffs(object sender, DoWorkEventArgs e)
        {
            ChordOffsWorkerArgs args = (ChordOffsWorkerArgs)e.Argument;
            int thisChordOnMsPosition = args.ThisChordOnMsPosition;
            int nextChordOnMsPosition = args.NextChordOnMsPosition;
            int currentMsPosition = thisChordOnMsPosition;

            if(nextChordOnMsPosition > currentMsPosition)
            {
                int nextOffsMsPosition = 0;
                while(currentMsPosition < nextChordOnMsPosition
                && (PendingChordOffs.Count > 0 || PendingChannelMessages.Count > 0
                                                                || PendingBackgroundSliders.Count > 0))
                {
                    nextOffsMsPosition = this.NextChordOffsMsPosition;
                    if(nextOffsMsPosition >= currentMsPosition && nextOffsMsPosition < nextChordOnMsPosition)
                    {
                        Sleep(currentMsPosition, nextOffsMsPosition);
                        SendPendingMessagesUpdateState(nextOffsMsPosition);
                    }
                    else break;
                }
            }
        }

        private void BackgroundSendFutureChordOffs_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            Exception ex = e.Error;
            if(ex != null)
            {
                ApplicationException appEx =
                    new ApplicationException("PerformanceState threw an exception while sending ChordOffs and updating itself.", ex);
                throw (appEx);
            }
        }

        public int Sleep(int currentMsPosition, int nextMsPosition)
        {
            int sleepDuration = 0;

            Debug.Assert(_msPosTimeControlsDict != null);

            if(!_msPosTimeControlsDict.ContainsKey(currentMsPosition)
            || !_msPosTimeControlsDict.ContainsKey(nextMsPosition))
                sleepDuration = 0;
            else if(_assistantsDurationsType == AssistantsDurationsType.SymbolsAbsolute)
            {
                // sleepDuration is notated duration
                sleepDuration = _msPosTimeControlsDict[currentMsPosition].NotatedMsDuration;
                //_assistantsSpaceTimeDict[nextMsPosition] - _assistantsSpaceTimeDict[currentMsPosition];
            }
            else
            {
                // sleepDuration is relative to performer's currentMsPerSemibreve
                sleepDuration = (int)(_liveMsPerSemibreve * ((float)(nextMsPosition - currentMsPosition)));
            }

            Thread.Sleep((int)sleepDuration);

            return sleepDuration;
        }

        /// <summary>
        /// Sends a ChordOn or ChordOff message, logging chord data (velocity, duration etc.) if the state is set to record.
        /// </summary>
        /// <param name="chordMessage"></param>
        public void Send(ChordMessage chordMessage)
        {
            if(SendChordMessage != null && chordMessage != null)
                SendChordMessage(chordMessage);
        }

        #region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(true);
        }
        protected virtual void Dispose(bool disposing)
        {
            if(!_disposed)
            {
                if(disposing)
                {
                    foreach(BackgroundWorker backgroundWorker in _chordOffsBWs)
                    {
                        backgroundWorker.Dispose();
                    }
                    foreach(BackgroundWorker backgroundWorker in _basicMidiChordsBWs)
                    {
                        backgroundWorker.Dispose();
                    }
                    foreach(BackgroundWorker sliderBW in _modulationWheelSliderBWs)
                    {
                        sliderBW.Dispose();
                    }
                    foreach(BackgroundWorker sliderBW in _panSliderBWs)
                    {
                        sliderBW.Dispose();
                    }
                    foreach(BackgroundWorker sliderBW in _pitchWheelSliderBWs)
                    {
                        sliderBW.Dispose();
                    }
                    foreach(BackgroundWorker sliderBW in _expressionSliderBWs)
                    {
                        sliderBW.Dispose();
                    }
                    
                    _chordOffsBWs.Clear();
                }
            }
            _disposed = true;
        }
        private bool _disposed = false;
        #endregion Dispose

        /// <summary>
        /// Key is the MsPosition at which the channel messages should be sent.
        /// Always lock this dictionary before accessing it.
        /// </summary>
        private SortedDictionary<int, List<ChordOff>> PendingChordOffs = new SortedDictionary<int, List<ChordOff>>();
        /// <summary>
        /// Key is the MsPosition at which the PendingMidiChord ends.
        /// Always lock this dictionary before accessing it.
        /// </summary>
        public SortedDictionary<int, List<MidiChord>> PendingMidiChords = new SortedDictionary<int, List<MidiChord>>();
        /// <summary>
        /// Key is the MsPosition at which the channel messages should be sent.
        /// Always lock this dictionary before accessing it.
        /// </summary>
        private SortedDictionary<int, List<ChannelMessage>> PendingChannelMessages = new SortedDictionary<int, List<ChannelMessage>>();
        /// <summary>
        /// Key is the MsPosition at which the pending background worker ought to stop (the OffPosition
        /// of a duration symbol).
        /// Always lock this dictionary before accessing it.
        /// </summary>
        public SortedDictionary<int, List<BackgroundWorker>> PendingBackgroundSliders = new SortedDictionary<int, List<BackgroundWorker>>();

        /// <summary>
        /// This property must be checked at the end of an assisted performance to prevent the SaveMidiFile 
        /// dialog appearing before all the notes have stopped playing.
        /// </summary>
        public bool IsBusy
        {
            get
            {
                lock(PendingBackgroundSliders)
                {
                    foreach(int key in PendingBackgroundSliders.Keys)
                    {
                        foreach(BackgroundWorker bw in PendingBackgroundSliders[key])
                        {
                            if(bw.IsBusy)
                                return true;
                        }
                    }
                }
                lock(_basicMidiChordsBWs)
                {
                    foreach(BackgroundWorker bw in _basicMidiChordsBWs)
                        if(bw.IsBusy)
                            return true;
                }
                lock(_modulationWheelSliderBWs)
                {
                    foreach(BackgroundWorker bw in _modulationWheelSliderBWs)
                        if(bw.IsBusy)
                            return true;
                }
                lock(_panSliderBWs)
                {
                    foreach(BackgroundWorker bw in _panSliderBWs)
                        if(bw.IsBusy)
                            return true;
                }
                lock(_pitchWheelSliderBWs)
                {
                    foreach(BackgroundWorker bw in _pitchWheelSliderBWs)
                        if(bw.IsBusy)
                            return true;
                }
                lock(_expressionSliderBWs)
                {
                    foreach(BackgroundWorker bw in _expressionSliderBWs)
                        if(bw.IsBusy)
                            return true;
                }
                lock(_chordOffsBWs)
                {
                    foreach(BackgroundWorker bw in _chordOffsBWs)
                        if(bw.IsBusy)
                            return true;
                }

                return false;
            }

        }
        /// <summary>
        /// If there are no pending ChordOffs, ChannelMessages or BackgroundWorkers this function returns -1/1
        /// </summary>
        public int NextChordOffsMsPosition
        {
            get
            {
                int nextOffsMsPosition = 0;
                lock(PendingChannelMessages)
                {
                    if(PendingChannelMessages.Keys.Count > 0)
                    {
                        foreach(int position in PendingChannelMessages.Keys)
                        {
                            nextOffsMsPosition = position;
                            break;
                        }
                    }
                    Monitor.PulseAll(PendingChannelMessages);
                }
                lock(PendingChordOffs)
                {
                    if(PendingChordOffs.Keys.Count > 0)
                    {
                        foreach(int msPosition in PendingChordOffs.Keys)
                        {
                            if(nextOffsMsPosition > 0)
                            {
                                nextOffsMsPosition = (nextOffsMsPosition < msPosition) ? nextOffsMsPosition : msPosition;
                            }
                            else
                            {
                                nextOffsMsPosition = msPosition;
                            }
                            break;
                        }
                    }
                    Monitor.PulseAll(PendingChordOffs);
                }
                lock(PendingBackgroundSliders)
                {
                    if(PendingBackgroundSliders.Count > 0)
                    {
                        foreach(int msPosition in PendingBackgroundSliders.Keys)
                        {
                            if(nextOffsMsPosition > 0)
                            {
                                nextOffsMsPosition = (nextOffsMsPosition < msPosition) ? nextOffsMsPosition : msPosition;
                            }
                            else
                            {
                                nextOffsMsPosition = msPosition;
                            }
                            break;
                        }
                    }
                    Monitor.PulseAll(PendingBackgroundSliders);
                }

                if(nextOffsMsPosition == 0)
                    nextOffsMsPosition = -1;

                return nextOffsMsPosition;
            }
        }

        public int NoteID(NoteMessage note)
        {
            return ((note.Channel << 8) + note.Pitch);
        }

        /// <summary>
        /// Called when a new performance is begun.
        /// </summary>
        public void Initialize(MidiMoment firstMoment)
        {
            _currentMsPosition = firstMoment.MsPosition;
            if(_assistantsDurationsType == AssistantsDurationsType.SymbolsRelative)
            {
                _liveMsPerSemibreve = 1F;
            }
            _previousPerformedMoment = firstMoment;
        }

        /// <summary>
        /// Called for each performed moment.
        /// </summary>
        public void SetPerformedMoment(MidiMoment moment)
        {
            if(_assistantsDurationsType == AssistantsDurationsType.SymbolsRelative)
            {
                _liveMsPerSemibreve = (moment.StartTimeMilliseconds - _previousPerformedMoment.StartTimeMilliseconds)
                                            / (float) _previousPerformedMoment.MsWidth;
            }
            _previousPerformedMoment = moment;
            _currentMsPosition = moment.MsPosition;
        }

        /// <summary>
        /// Called for each new assistant's moment.
        /// </summary>
        public void SetAssistantsMoment(MidiMoment thisAssistantsMoment)
        {
            _currentMsPosition = thisAssistantsMoment.MsPosition;
        }

        public bool OrnamentIsPerformingInBackground
        {
            get
            {
                return (PendingMidiChords.Count > 0);
            }
        }

        public AssistantsDurationsType AssistantsDurationsType { get { return _assistantsDurationsType; } }

        public int CurrentMsPosition { get { return _currentMsPosition; } }
        public List<List<KeyType>> KeyboardSettings { get { return _keyboardSettings; } }
        public int KeyboardSettingsIndex { get { return _keyboardSettingsIndex; } set { _keyboardSettingsIndex = value; } }

        public ChordMessageDelegate SendChordMessage;
        public ChannelMessageDelegate SendChannelMessage;
        //private MidiLoudness[] _loudnessStates = null;
        private readonly AssistantsDurationsType _assistantsDurationsType = AssistantsDurationsType.SymbolsAbsolute;
        private readonly List<int> _midiChannels;
        private List<List<KeyType>> _keyboardSettings = null;
        public int _keyboardSettingsIndex = 0;
        private MidiMoment _previousPerformedMoment;
        /// <summary>
        /// _liveMsPerSemibreve is used by the Assistant when the AssistantsDurationsType set to SymbolsRelative,
        /// and in Ornaments when their DurationsType is set to FitToPerformersTempo.
        /// The performer always overrides all notated tempo markings.
        /// If the AssistantsDurationsType is SymbolsRelative:
        ///     1. the assistant treats all its notated tempo markings relative to _liveMsPerSemibreve
        ///     2. _liveMsPerSemibreve is updated (while the assistant is performing alone) using the assistant's notated
        ///        tempi relatively (e.g. if the assistant's notated tempo doubles, the assistant doubles the value of
        ///        _liveMsPerSemibreve).
        /// </summary>
        private float _liveMsPerSemibreve = 1F;
        private int _currentMsPosition;
        public SortedDictionary<int, TimeControl> MsPosTimeControlsDict
        {
            get { return _msPosTimeControlsDict; } 
        }
        private SortedDictionary<int, TimeControl> _msPosTimeControlsDict = null;

        // there is one List<BackgroundWorker> per channel and one BackgroundWorker per voice (currently maximum 6)
        private const int _initialNumberOfBasicMidiChordWorkersPerChannel = 6;
        public List<BackgroundWorker> _basicMidiChordsBWs = new List<BackgroundWorker>();
        // there is one ModSliderBW per channel
        public List<BackgroundWorker> _modulationWheelSliderBWs = new List<BackgroundWorker>();
        // there is one PanSliderBW per channel
        public List<BackgroundWorker> _panSliderBWs = new List<BackgroundWorker>();
        // there is one PitchwheelSliderBW per channel
        public List<BackgroundWorker> _pitchWheelSliderBWs = new List<BackgroundWorker>();
        // there is one ExpressionSliderBW per channel
        public List<BackgroundWorker> _expressionSliderBWs = new List<BackgroundWorker>();

        private List<BackgroundWorker> _chordOffsBWs = new List<BackgroundWorker>();
    }
}
