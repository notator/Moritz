using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel; // BackgroundWorker
using System.Threading;
using System.Diagnostics;

using Multimedia;
using Multimedia.Midi;
using Moritz.Score.Midi;

namespace Moritz.AssistantPerformer
{
    internal class MidiInputDevice : IMidiSource
    {
        #region constructors
        public MidiInputDevice(InputDevice multimediaMidiInputDevice)
        {
            _multimediaMidiInputDevice = multimediaMidiInputDevice;
            _notesCollector.DoWork += new DoWorkEventHandler(NotesCollector_DoWork);
            _notesCollector.RunWorkerCompleted += new RunWorkerCompletedEventHandler(NotesCollector_RunWorkerCompleted);
            _notesCollector.RunWorkerAsync();
        }
        #endregion constructors
        #region IMidiSource
        private ChordMessageDelegate SendChordMessage;
        private ChannelMessageDelegate SendChannelMessage;
        private SysExMessageDelegate SendSysExMessage;
        private SysCommonMessageDelegate SendSysCommonMessage;
        private SysRealtimeMessageDelegate SendSysRealtimeMessage;

        public void Connect(IMidiSink assistantPerformerRuntime)
        {
            SendChordMessage += new ChordMessageDelegate(assistantPerformerRuntime.ProcessMessage);
            SendChannelMessage += new ChannelMessageDelegate(assistantPerformerRuntime.ProcessMessage);
            SendSysExMessage += new SysExMessageDelegate(assistantPerformerRuntime.ProcessMessage);
            SendSysCommonMessage += new SysCommonMessageDelegate(assistantPerformerRuntime.ProcessMessage);
            SendSysRealtimeMessage += new SysRealtimeMessageDelegate(assistantPerformerRuntime.ProcessMessage);
        }
        public virtual void Disconnect(IMidiSink assistantPerformerRuntime)
        {
            SendChordMessage -= new ChordMessageDelegate(assistantPerformerRuntime.ProcessMessage);
            SendChannelMessage -= new ChannelMessageDelegate(assistantPerformerRuntime.ProcessMessage);
            SendSysExMessage -= new SysExMessageDelegate(assistantPerformerRuntime.ProcessMessage);
            SendSysCommonMessage -= new SysCommonMessageDelegate(assistantPerformerRuntime.ProcessMessage);
            SendSysRealtimeMessage -= new SysRealtimeMessageDelegate(assistantPerformerRuntime.ProcessMessage);
        }
        public bool IsRunning { get { return _isRunning; } }
        public void StopMidiStreaming()
        {
            if(_multimediaMidiInputDevice != null && _isRunning) // disconnect
            {
                _isRunning = false;
                _multimediaMidiInputDevice.StopRecording();
                DisconnectDevice();
            }
        }
        private void DisconnectDevice()
        {
            try
            {
                _multimediaMidiInputDevice.Disconnect((Sink<ChannelMessage>)ProcessMessage);
                _multimediaMidiInputDevice.Disconnect((Sink<SysExMessage>)ProcessMessage);
                _multimediaMidiInputDevice.Disconnect((Sink<SysCommonMessage>)ProcessMessage);
                _multimediaMidiInputDevice.Disconnect((Sink<SysRealtimeMessage>)ProcessMessage);

            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Device disconnection failed", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        public void StartMidiStreaming()
        {
            if(_multimediaMidiInputDevice != null && _isRunning == false) // connect
            {
                if(ConnectDevice())
                {
                    _isRunning = true;
                    _multimediaMidiInputDevice.StartRecording();
                }
            }
        }
        private bool ConnectDevice()
        {
            try
            {
                _multimediaMidiInputDevice.Connect((Sink<ChannelMessage>)ProcessMessage);
                _multimediaMidiInputDevice.Connect((Sink<SysExMessage>)ProcessMessage);
                _multimediaMidiInputDevice.Connect((Sink<SysCommonMessage>)ProcessMessage);
                _multimediaMidiInputDevice.Connect((Sink<SysRealtimeMessage>)ProcessMessage);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Device connection failed", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
            return true;
        }

        #endregion

        #region assemble chords
        /// <summary>
        /// Called from ProcessMessage(ChannelMessage) by MIDI input nodes and Midi file input nodes.
        /// Converts incoming Channelmessages to Chords. The _chordCollector sends ChordMessages.
        /// </summary>
        /// <param name="message"></param>
        /// <summary>
        /// The delegate called when the _chordCollector.RunWorkerAsync is Started.
        /// It simply waits until the chord's MidiEvent list is complete then sends the Chord.
        /// This ChordIndexerNode then sends the chord's index and note-on messages.
        /// </summary>
        private void NotesCollector_DoWork(object obj, DoWorkEventArgs e)
        {
            Thread.Sleep(ChordMessage.MaxMilliseconds); // give ProcessMessage time to add more messages to _timedMessages}
        }
        private void NotesCollector_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            while(_notes.Count > 0)
            {
                ChordOff chordOff = new ChordOff(null);
                ChordOn chordOn = new ChordOn(null);

                int channel = _notes[_notes.Count - 1].Channel;
                for(int i = _notes.Count - 1; i >= 0; i--)
                {
                    NoteOff noteOff = _notes[i] as NoteOff;
                    if(noteOff != null && noteOff.Channel == channel)
                    {
                        chordOff.AddNote(noteOff);
                        _notes.RemoveAt(i);
                    }
                    else
                    {
                        NoteOn noteOn = _notes[i] as NoteOn;
                        if(noteOn != null && noteOn.Channel == channel)
                        {
                            chordOn.AddNote(noteOn);
                            _notes.RemoveAt(i);
                        }
                    }
                }

                if(chordOff.Notes.Count > 0)
                {
                    // changed 02.04.09 for homophonic input.
                    // _previousChordMessage added to this class.
                    // The following if() clause previously read just
                    //     chordOff.Send(SendChordMessage, true);
                    if(_previousChordMessage != null && _previousChordMessage is ChordOn)
                    {
                        int pitch = chordOff.Notes[0].Pitch;
                        foreach(NoteMessage prevNoteMessage in _previousChordMessage.Notes)
                        {
                            // only send a chordOff if it matches the previously sent ChordOn
                            if(pitch == prevNoteMessage.Pitch)
                            {
                                chordOff = _previousChordMessage.CloneChordOff();
                                SendChordMessage(chordOff);
                                _previousChordMessage = chordOff;
                                break;
                            }
                        }
                    }
                }
                if(chordOn.Notes.Count > 0)
                {
                    if(_previousChordMessage != null && _previousChordMessage is ChordOn)
                    {
                        ChordOff cOff = _previousChordMessage.CloneChordOff();
                        if(cOff != null)
                            SendChordMessage(cOff);
                    }
                    if(SendChordMessage != null)
                        SendChordMessage(chordOn);

                    _previousChordMessage = chordOn;
                }
            }

            if(e.Error != null)
            {
                Debug.Assert(false);
                throw (e.Error);
            }
        }
        /// <summary>
        /// This MidiInputDevice first collects note-ons and note-offs into the _notes object, 
        /// according to the time limit Chord.MaxMilliseconds.
        /// In NotesCollector_RunWorkerCompleted (see above), note-offs in _notes are moved to a new ChordOff,
        /// note-ons are moved to a new ChordOn, and these two chords are sent as separate messages.
        /// </summary>
        /// <param name="message"></param>
        protected void ProcessChordMessage(ChannelMessage message)
        {
            if(message.Command == ChannelCommand.NoteOn || message.Command == ChannelCommand.NoteOff)
            {
                if(!_notesCollector.IsBusy)
                {
                    _notes.Clear();
                    _notesCollector.RunWorkerAsync();
                }

                if(message.Command == ChannelCommand.NoteOff)
                {
                    _notes.Add(new NoteOff(message.MidiChannel, message.Data1, message.Data2));
                }
                else if(message.Data2 == 0) // a MIDI note-on with zero velocity
                {
                    _notes.Add(new NoteOff(message.MidiChannel, message.Data1, 64));
                }
                else
                {
                    _notes.Add(new NoteOn(message.MidiChannel, message.Data1, message.Data2));
                }
            }
            else
            {
                if(this.SendChannelMessage != null)
                    this.SendChannelMessage((ChannelMessage)message);
            }
        }
        #endregion
        /// <summary>
        /// _multimediaMidiInputDevice never calls ProcessMessage(ChordMessage chord)
        /// </summary>
        /// <param name="chord"></param>
        public void ProcessMessage(ChordMessage chord)
        {
        }
        public void ProcessMessage(ChannelMessage message)
        {
            switch(message.Command)
            {
                case ChannelCommand.ChannelPressure:
                    break;
                case ChannelCommand.Controller:
                    break;
                case ChannelCommand.NoteOff:
                    ProcessChordMessage(message);
                    break;
                case ChannelCommand.NoteOn:
                    ProcessChordMessage(message);
                    break;
                case ChannelCommand.PitchWheel:
                    break;
                case ChannelCommand.PolyPressure:
                    break;
                case ChannelCommand.ProgramChange:
                    break;
                default:
                    break;
            }
        }
        public void ProcessMessage(SysExMessage message)
        {
            if(SendSysExMessage != null)
                this.SendSysExMessage((SysExMessage) message);
        }
        public void ProcessMessage(SysCommonMessage message)
        {
            if(SendSysCommonMessage != null)
                this.SendSysCommonMessage((SysCommonMessage) message);
        }
        public void ProcessMessage(SysRealtimeMessage message)
        {
            if(SendSysRealtimeMessage != null)
                this.SendSysRealtimeMessage((SysRealtimeMessage) message);
        }

        private bool _isRunning = false;
        private Multimedia.Midi.InputDevice _multimediaMidiInputDevice = null;
        private ChordMessage _previousChordMessage = null;
        private List<NoteMessage> _notes = new List<NoteMessage>();
        private BackgroundWorker _notesCollector = new BackgroundWorker();
    }
}
