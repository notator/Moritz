using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Diagnostics;

using Moritz.Globals;
using Moritz.Midi;

namespace Moritz.Performer
{
    public class Assistant
    {
        class WorkerArgs
        {
            public int PerformersStartTimeMilliseconds;
            public int PerformersCurrentMsPosition;
            public int PerformersNextMsPosition;
            public int PerformersFinalChordOffMsPosition;
            public int AssistantsFromIndex;
            public int AssistantsMsPosition;
            public bool AssistantIsPerformingAlone;
            public ReportPositionDelegate ReportPosition;
        }
        public Assistant(List<MidiMoment> moments, 
            PerformanceState state,
            SortedDictionary<int, TimeControl> msPosTimeControlsDict,
            //StopMidiStreaming stopMidiStreaming,
            CheckRepeatDelegate checkRepeat)
        {
            _moments = moments;
            _state = state;
            _msPosTimeControlsDict = msPosTimeControlsDict;

            CheckRepeat = checkRepeat;

            _assistantsChordOnsBW.DoWork += new DoWorkEventHandler(SendAssistantsMessages);
            _assistantsChordOnsBW.WorkerSupportsCancellation = true;
            _assistantsChordOnsBW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(SendAssistantsMessages_Completed);
        }

        public void ResetChannels()
        {
            Debug.Assert(_state != null);
            _state.ResetChannels(); // sends allSoundsOff and allControllersOff to each channel;
        }

        /// <summary>
        /// Called by an active performer to start a new thread to perform the assistant's moments from the
        /// currentMsPosition up to, but not including, the live performers's nextMsPosition.
        /// </summary>
        public void Perform(int performersStartTimeMilliseconds,
            int currentMsPosition, int nextMsPosition,
            int performersFinalChordOffMsPosition,
            int assistantsFromIndex, int assistantsMsPosition)
        {
            Perform(performersStartTimeMilliseconds,
                currentMsPosition, nextMsPosition,
                performersFinalChordOffMsPosition,
                assistantsFromIndex, assistantsMsPosition,
                false, null);
        }

        /// <summary>
        /// Starts a new thread to perform the assistant's moments from the currentMsPosition up to, but not
        /// including, the live player's nextMsPosition.
        /// </summary>
        public void Perform(int performersStartTimeMilliseconds,
            int currentMsPosition, int nextMsPosition,
            int performersFinalChordOffMsPosition,
            int assistantsFromIndex, int assistantsMsPosition,
            bool assistantIsPerformingAlone, ReportPositionDelegate reportPosition)
        {
            if(_moments.Count > 0)
            {
                WorkerArgs args = new WorkerArgs();
                args.PerformersStartTimeMilliseconds
                    = performersStartTimeMilliseconds;
                args.PerformersCurrentMsPosition = currentMsPosition;
                args.PerformersNextMsPosition = nextMsPosition;
                args.PerformersFinalChordOffMsPosition = performersFinalChordOffMsPosition;
                args.AssistantsFromIndex = assistantsFromIndex;
                args.AssistantsMsPosition = assistantsMsPosition;
                args.AssistantIsPerformingAlone = assistantIsPerformingAlone;
                args.ReportPosition = reportPosition;

                if(_assistantsChordOnsBW != null
                    && _assistantsChordOnsBW.IsBusy
                    && !_assistantsChordOnsBW.CancellationPending)
                {
                    _assistantsChordOnsBW.CancelAsync();
                }

                while(_assistantsChordOnsBW.IsBusy || _assistantsChordOnsBW.CancellationPending)
                    Thread.Sleep(1);

                _assistantsChordOnsBW.RunWorkerAsync(args);
            }
        }

        /// <summary>
        /// Used when the Performer has nothing to play, this function performs all the assistant's moments,
        /// from the beginAtMoment to the end of the piece using the msPositions and tempi set in the score.
        /// </summary>
        public void PerformAlone(int beginAtMsPosition, int beginAtIndex, ReportPositionDelegate reportPosition)
        {
            MidiMoment lastMoment = _moments[_moments.Count - 1];
            int finalPosition = lastMoment.MsPosition + (lastMoment.MsWidth * 2);

            Perform(M.NowMilliseconds, beginAtMsPosition, finalPosition, 0,
                beginAtIndex, _moments[beginAtIndex].MsPosition,
                true, reportPosition);
        }

        /// <summary>
        /// Performs a single moment in the assistant's _moments list.
        /// Note that ordinary MidiChords are sent in the main user thread,
        /// but ornaments are sent in a background thread.
        /// This is so that the caller can introduce pauses between the moment performances.
        /// </summary>
        /// <param name="assistantsMomentIndex"></param>
        public void PerformOneMoment(int assistantsMomentIndex)
        {
            int beginAtPosition = _moments[assistantsMomentIndex].MsPosition;
            if(_moments.Count > 0)
            {
                WorkerArgs args = new WorkerArgs();
                args.PerformersStartTimeMilliseconds = beginAtPosition;
                args.PerformersCurrentMsPosition = args.PerformersStartTimeMilliseconds;
                args.PerformersNextMsPosition = args.PerformersStartTimeMilliseconds + 1;
                args.PerformersFinalChordOffMsPosition = args.PerformersStartTimeMilliseconds + 1;
                args.AssistantsFromIndex = assistantsMomentIndex;
                args.AssistantsMsPosition = beginAtPosition;
                args.AssistantIsPerformingAlone = true;
                args.ReportPosition = null;

                DoWorkEventArgs doWorkEventArgs = new DoWorkEventArgs(args);
                SendAssistantsMessages(this, doWorkEventArgs);
            }
        }

        /// <summary>
        /// Called asynchronously by the Perform() functions above.
        /// Sends the assistant's chordOn messages, adds other pending messages to the performance state.
        /// Sends the assistant's chords whose MsPosition is between (including) args.PerformersCurrentMsPosition
        /// and (but not including) args.PerformersNextMsPosition.
        /// </summary>
        private void SendAssistantsMessages(object sender, DoWorkEventArgs e)
        {
            try
            {
                bool reachedEndOfPiece = false;
                e.Result = reachedEndOfPiece;
                int finalChordOffPositionInPiece = _msPosTimeControlsDict.Last().Key;

                #region get arguments
                WorkerArgs args = (WorkerArgs)e.Argument;
                int performersStartTimeMilliseconds = args.PerformersStartTimeMilliseconds;
                int performersCurrentPosition = args.PerformersCurrentMsPosition;
                int performersNextPos = args.PerformersNextMsPosition;
                int performersFinalChordOffPosition = args.PerformersFinalChordOffMsPosition; // Zero, if not playing 
                int assistantsIndex = args.AssistantsFromIndex;
                int assistantsCurrentPosition = args.AssistantsMsPosition;
                bool assistantIsPerformingAlone = args.AssistantIsPerformingAlone;
                ReportPositionDelegate ReportPosition = args.ReportPosition;
                #endregion

                ///////////// temp Debug ////////////
                //Rational stopPos = new Rational(3, 1);
                /////////////
                int nextAssistantsPosition = 0;
                MidiMoment assistantsMoment = _moments[assistantsIndex];
                if(performersCurrentPosition < assistantsCurrentPosition) // never true for an assistant only performance.
                {
                    assistantsMoment.PerformedDelay = _state.Sleep(performersCurrentPosition, assistantsCurrentPosition);
                }
                else
                    assistantsMoment.PerformedDelay = 0;

                while(assistantsIndex < _moments.Count
                    && assistantsCurrentPosition < performersNextPos
                    && assistantsCurrentPosition >= _state.CurrentMsPosition
                    && _assistantsChordOnsBW.CancellationPending == false)
                {
                    assistantsMoment = _moments[assistantsIndex];

                    assistantsMoment.StartTimeMilliseconds = M.NowMilliseconds; // for the output recording

                    _state.SendPendingMessagesUpdateState(assistantsCurrentPosition);

                    if(assistantsIndex == _moments.Count - 1 &&
                        _msPosTimeControlsDict[assistantsMoment.MsPosition].IsGeneralPause)
                    {
                        if(finalChordOffPositionInPiece > performersFinalChordOffPosition
                        && finalChordOffPositionInPiece == (assistantsMoment.MsPosition + assistantsMoment.MsWidth))
                        {
                            reachedEndOfPiece = true;
                            e.Result = reachedEndOfPiece;
                        }
                        assistantsIndex++;
                        break;
                    }

                    if(_assistantsChordOnsBW.CancellationPending == false)
                    {
                        _state.SendMidiDurationSymbolUpdateState(assistantsMoment.MidiChords, assistantsCurrentPosition);
                    }

                    // the assistant is performing alone here
                    if(ReportPosition != null)
                    {
                        ReportPosition(assistantsIndex + 1);
                    }
                    if(assistantsIndex < _moments.Count - 1)
                    {
                        nextAssistantsPosition = _moments[assistantsIndex + 1].MsPosition;
                    }
                    else
                    {
                        MidiMoment lastAssistantsMoment = _moments[_moments.Count - 1];
                        nextAssistantsPosition = lastAssistantsMoment.MsPosition + lastAssistantsMoment.MsWidth;
                    }

                    if(!assistantIsPerformingAlone && nextAssistantsPosition <= performersNextPos)
                    {
                        if(!(nextAssistantsPosition == performersNextPos
                            && _state.AssistantsDurationsType == AssistantsDurationsType.SymbolsRelative))
                            _state.SendAssistantsFutureChordOffs(assistantsCurrentPosition, nextAssistantsPosition);
                    }

                    if(assistantsCurrentPosition < nextAssistantsPosition
                    && (performersStartTimeMilliseconds == 0L || nextAssistantsPosition < performersNextPos))
                    {
                        if(assistantsIndex > 0)
                            _state.SetAssistantsMoment(_moments[assistantsIndex]);
                        _state.Sleep(assistantsCurrentPosition, nextAssistantsPosition);
                    }

                    /////////// temp Debug (see stopPos above) /////////////
                    //if(assistantsCurrentPosition == stopPos)
                    //    stopPos = nextAssistantsPosition;
                    ////////////////////////////////////////////////////////

                    if(_assistantsChordOnsBW.CancellationPending)
                    {
                        _state.SendAssistantsFutureChordOffs(assistantsCurrentPosition, nextAssistantsPosition);
                    }

                    assistantsCurrentPosition = nextAssistantsPosition;

                    assistantsIndex++;

                }

                while(_state.OrnamentIsPerformingInBackground)
                {
                    int endPosition = _moments[_moments.Count - 1].MsPosition
                        + _moments[_moments.Count - 1].MsWidth;
                    _state.SendPendingMessagesUpdateState(endPosition);
                    Thread.Sleep(200);
                }

                // if finalChordOffPositionInPiece == performersFinalChordOffPosition
                // the end of the piece is reached only when the performer sends the chordOff.
                if(finalChordOffPositionInPiece > performersFinalChordOffPosition
                    && finalChordOffPositionInPiece <= assistantsCurrentPosition)
                {
                    _state.SendAssistantsFutureChordOffs(assistantsCurrentPosition, performersNextPos);
                    reachedEndOfPiece = true;
                    e.Result = reachedEndOfPiece;
                }
            }
            catch // This swallows any exceptions thrown when the user cancels a performance early.
            {
            }
        }

        /// <summary>
        /// Called in the UI Thread. Argument contains an Exception if the ornamentWorker has raised one.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendAssistantsMessages_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            Exception ex = e.Error;
            if(ex != null)
            {
                ApplicationException appEx =
                    new ApplicationException("Assistant threw an exception in its background thread", ex);
                throw (appEx);
            }
            bool reachedEndOfPiece = (bool)e.Result;
            if(reachedEndOfPiece && !e.Cancelled && CheckRepeat != null)
            {
                while(_state.IsBusy)
                    Thread.Sleep(0);
                CheckRepeat(_state);
            }
        }

        public bool IsBusy
        {
            get
            {
                return (_assistantsChordOnsBW.IsBusy || _state.IsBusy);
            }
        }

        public void CancelPerformance()
        {
            if(_assistantsChordOnsBW != null && _assistantsChordOnsBW.IsBusy)
            {
                _assistantsChordOnsBW.CancelAsync();
                Thread.Sleep(100);
            }
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
                    _assistantsChordOnsBW.Dispose();
                }
            }
            _disposed = true;
        }
        private bool _disposed = false;
        #endregion Dispose

        public void GetPosition(int performersCurrentMsPosition, int nextPerformersMsPosition,
                                    out int assistantsFromIndex, out int assistantsFromMsPosition)
        {
            if(performersCurrentMsPosition > _moments[_moments.Count - 1].MsPosition)
            {
                assistantsFromIndex = _moments.Count;
                assistantsFromMsPosition = performersCurrentMsPosition - 1;
                return;
            }

            assistantsFromIndex = int.MaxValue;
            for(int i = 0; i < _moments.Count; i++)
            {
                if(_moments[i].MsPosition >= performersCurrentMsPosition)
                {
                    assistantsFromIndex = i;
                    break;
                }
            }

            if(assistantsFromIndex < _moments.Count)
            {
                assistantsFromMsPosition = _moments[assistantsFromIndex].MsPosition;
            }
            else assistantsFromMsPosition = nextPerformersMsPosition + 1;
        }

        private List<MidiMoment> _moments = null;
        /// <summary>
        /// Use this object to access the performance state.
        /// </summary>
        private PerformanceState _state;
        /// <summary>
        /// The background worker which performs the assistant's moments.
        /// </summary>
        private BackgroundWorker _assistantsChordOnsBW = new BackgroundWorker();
        private SortedDictionary<int, TimeControl> _msPosTimeControlsDict;
        /// <summary>
        /// Delegate called at the end of a performance.
        /// </summary>
        private CheckRepeatDelegate CheckRepeat = null;
    }
}
