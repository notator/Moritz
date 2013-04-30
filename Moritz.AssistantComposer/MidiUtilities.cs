using System;
using System.Collections.Generic;
using System.Diagnostics;

using Multimedia.Midi;

using Moritz.Globals;
using Moritz.Score.Midi;

namespace Moritz.AssistantComposer
{
    internal class MidiUtilities
    {
        /// <summary>
        /// N.B. This function has not been completely tested to see that controls inside MidiChordDefs are returned correctly.
        /// This function was used in Song6Algorithm to analyse the singer's text (which I had recorded manually with the
        /// AssistantPerformer), but only the MomentDef.MsPosition and the MidiChordDef.BasicMidiChordDef info was relevant there.
        /// Any controls contained in the returned MidiChordDefs were irrelevant, and were ignored.
        /// </summary>
        public List<MomentDef> GetMomentDefs(Multimedia.Midi.Sequence sequence, out List<int> midiChannels)
        {
            SortedDictionary<int, MomentDef> allPosMomentDefs = new SortedDictionary<int,MomentDef>();
            Dictionary<int, SortedDictionary<int, MomentDef>> midiChannelMomentDefs = GetMidiChannelMomentDefs(sequence);

            midiChannels = new List<int>();
            foreach(int channel in midiChannelMomentDefs.Keys)
            {
                midiChannels.Add(channel);
            }

            foreach(int channel in midiChannelMomentDefs.Keys)
            {
                SortedDictionary<int, MomentDef> momentDefsDict = midiChannelMomentDefs[channel];
                foreach(int msPos in momentDefsDict.Keys)
                {
                    if(!allPosMomentDefs.ContainsKey(msPos))
                        allPosMomentDefs.Add(msPos, new MomentDef(msPos));
                    foreach(MidiChordDef midiChordDef in momentDefsDict[msPos].MidiChordDefs)
                    {
                        allPosMomentDefs[msPos].MidiChordDefs.Add(midiChordDef);
                    }
                }
            }

            List<MomentDef> returnMomentDefs = new List<MomentDef>();
            foreach(int msPos in allPosMomentDefs.Keys)
                returnMomentDefs.Add(allPosMomentDefs[msPos]);

            SetMomentWidths(returnMomentDefs);

            return returnMomentDefs;
        }
        /// <summary>
        /// In the returned Dictionary, the key is channel, value is [msPosition, MomentDef]
        /// Entries are constructed for this dictionary only for the channels which actually contain Chords.
        /// The smallest msPosition in the returned dictionary is always 0.
        /// (All the midiMoment.msPositions are adjusted if necessary, so that the first moment has MsPosition=0). 
        /// </summary>
        private Dictionary<int, SortedDictionary<int, MomentDef>> GetMidiChannelMomentDefs(Multimedia.Midi.Sequence sequence)
        {
            Dictionary<int, SortedDictionary<int, MomentDef>> midiChannelMomentDefs = null;
            if(sequence != null)
            {
                // WriteTrackNoteInfo(sequence); // (slow) test function for printing info to the Output window
                midiChannelMomentDefs = GetMidiChannelMomentDefsFromSequence(sequence);
            }
            if(midiChannelMomentDefs == null)
                Debug.Fail("Error converting midi sequence to List<MomentDef>");

            /// (The performance always begins at the first sounding Moment, so all the midiMoment.msPositions
            /// are adjusted if necessary, so that the first moment has MsPosition=0).
            int currentFirstPosition = GetFirstDefsPosition(midiChannelMomentDefs);
            Dictionary<int, SortedDictionary<int, MomentDef>> returnMidiChannelMoments =
                AdjustDefsToPositionZero(midiChannelMomentDefs, currentFirstPosition);

            return returnMidiChannelMoments;
        }

        private void SetMomentWidths(List<MomentDef> momentDefs)
        {
            if(momentDefs.Count > 1)
            {
                for(int index = 1; index < momentDefs.Count; ++index)
                {
                    momentDefs[index - 1].MsWidth = momentDefs[index].MsPosition - momentDefs[index - 1].MsPosition;
                }
            }
            int maxMidiChordDefWidth = int.MinValue;
            MomentDef lastMoment = momentDefs[momentDefs.Count - 1];
            foreach(MidiChordDef mcd in lastMoment.MidiChordDefs)
                maxMidiChordDefWidth = (maxMidiChordDefWidth > mcd.MsDuration) ? maxMidiChordDefWidth : mcd.MsDuration;

            momentDefs[momentDefs.Count - 1].MsWidth = maxMidiChordDefWidth;
        }

        /// <summary>
        /// In the returned Dictionary, the key is channel, value is [msPosition, MomentDef]
        /// Entries are constructed for this dictionary only for the channels which actually contain Chords.
        /// The smallest msPosition in the dictionary is always 0.
        /// </summary>
        private Dictionary<int, SortedDictionary<int, MomentDef>> GetMidiChannelMomentDefsFromSequence(Multimedia.Midi.Sequence sequence)
        {
            Dictionary<int, SortedDictionary<int, MomentDef>> returnDict = new Dictionary<int, SortedDictionary<int, MomentDef>>();
            foreach(Track track in sequence)
            {
                SortedDictionary<int, MomentDef> midiMomentDefsDict = new SortedDictionary<int, MomentDef>();

                Dictionary<int, List<NoteOn>> timedNoteOns = GetTimedNoteOns(track);
                Dictionary<int, List<NoteOff>> timedNoteOffs = GetTimedNoteOffs(track);
                Dictionary<int, List<MidiControlDef>> timedMidiControlDefs = GetTimedMidiControlDefs(track);

                if(timedNoteOns.Count > 0)
                {
                    Dictionary<int, List<MidiChordDef>> timedMidiChordDefs =
                        GetTimedMidiChordDefs(timedNoteOns, timedNoteOffs, timedMidiControlDefs);

                    Debug.Assert(timedMidiChordDefs.Count > 0);

                    foreach(int key in timedMidiChordDefs.Keys)
                    {
                        if(!midiMomentDefsDict.ContainsKey(key))
                        {
                            midiMomentDefsDict.Add(key, new MomentDef(key));
                        }
                        midiMomentDefsDict[key].MidiChordDefs.AddRange(timedMidiChordDefs[key]);
                    }

                    int channel = -1;
                    foreach(int pos in timedNoteOns.Keys)
                    {
                        channel = timedNoteOns[pos][0].Channel;
                        break;
                    }
                    Debug.Assert(!returnDict.ContainsKey(channel));
                    returnDict.Add(channel, midiMomentDefsDict);
                }
            }

            return returnDict;
        }

        private int GetFirstDefsPosition(Dictionary<int, SortedDictionary<int, MomentDef>> midiChannelMomentDefs)
        {
            int firstPosition = int.MaxValue;
            foreach(int channel in midiChannelMomentDefs.Keys)
            {
                SortedDictionary<int, MomentDef> mmDict = midiChannelMomentDefs[channel];
                foreach(KeyValuePair<int, MomentDef> kvp in mmDict)
                {
                    firstPosition = (firstPosition < kvp.Key) ? firstPosition : kvp.Key;
                    break;
                }
            }
            return firstPosition;
        }

        private Dictionary<int, SortedDictionary<int, MomentDef>>
            AdjustDefsToPositionZero(Dictionary<int, SortedDictionary<int, MomentDef>> midiChannelMoments, int currentFirstPosition)
        {
            Dictionary<int, SortedDictionary<int, MomentDef>> returnDict = new Dictionary<int, SortedDictionary<int, MomentDef>>();
            foreach(int channel in midiChannelMoments.Keys)
            {
                SortedDictionary<int, MomentDef> mmDict = midiChannelMoments[channel];
                SortedDictionary<int, MomentDef> newDict = new SortedDictionary<int, MomentDef>();
                foreach(KeyValuePair<int, MomentDef> kvp in mmDict)
                {
                    int newPosition = kvp.Key - currentFirstPosition;
                    MomentDef midiMoment = kvp.Value;
                    midiMoment.MsPosition = newPosition;
                    // The contained midiChordDefs do not have a position attribute, so it does not need to change.
                    newDict.Add(newPosition, midiMoment);
                }
                returnDict.Add(channel, newDict);
            }
            return returnDict;
        }


        /// <summary>
        /// None of the MidiChords returned by this function are ornaments.
        /// In other words, this function returns a MidiChord for each ChordOn in the midi file.
        /// A ChordOn contains a set of NoteOns having the same msPosition and a set of NoteOffs having the same position.
        /// </summary>
        /// <returns>
        /// A dictionary whose key is the onMsPosition of the list of chords
        /// </returns>
        public Dictionary<int, List<MidiChordDef>> GetTimedMidiChordDefs(
            Dictionary<int, List<NoteOn>> timedNoteOns,
            Dictionary<int, List<NoteOff>> timedNoteOffs,
            Dictionary<int, List<MidiControlDef>> timedMidiControlDefs)
        {
            Dictionary<int, List<MidiChordDef>> returnDict = new Dictionary<int, List<MidiChordDef>>();
            ChannelState channelState = new ChannelState();
            // All the notes in a MidiChord begin and end at the same time.
            // this track can have several simultaneously beginning MidiChords which end at different times.
            foreach(int onPos in timedNoteOns.Keys)
            {
                /// The noteOffsDict contains all the noteOffs corresponding to the input noteOns.
                /// The number of keys in this dictionary is the number of MidiChords which have to be constructed.
                /// If the key is int.MaxValue, the chord has no ChordOff.
                Dictionary<int, List<NoteOff>> noteOffsDict = GetNoteOffsDict(onPos, timedNoteOns[onPos], timedNoteOffs);

                /// The midiControls List contains a list of unique midiControls which happen before or synchronously with the onPos.
                /// MidiControls which happen before or synchronously with onPos are removed from the timedMidiControls dictionary,
                /// so that they are not used twice.

                //List<MidiControl> midiControls = GetMidiControls(onPos, timedMidiControls);
                List<MidiControlDef> midiControlDefs = null;
                if(timedMidiControlDefs.ContainsKey(onPos))
                    midiControlDefs = timedMidiControlDefs[onPos];

                foreach(int offPos in noteOffsDict.Keys)
                {
                    int channel = noteOffsDict[offPos][0].Channel;
                    bool hasChordOff = (offPos != int.MaxValue);
                    int msDuration = offPos - onPos;
                    List<byte> pitches = new List<byte>();
                    foreach(NoteOff noteOff in noteOffsDict[offPos])
                    {
                        pitches.Add((byte)noteOff.Pitch);
                    }
                    List<byte> velocities = new List<byte>();
                    foreach(NoteOn noteOn in timedNoteOns[onPos])
                    {
                        foreach(byte pitch in pitches)
                        {
                            if(noteOn.Pitch == pitch)
                            {
                                velocities.Add((byte)noteOn.Velocity);
                                break;
                            }
                        }
                    }
                    Debug.Assert(pitches.Count == velocities.Count);
                    MidiChordDef midiChordDef = new ComposableMidiChordDef(pitches, velocities, msDuration, hasChordOff, midiControlDefs);
                    if(!returnDict.ContainsKey(onPos))
                        returnDict.Add(onPos, new List<MidiChordDef>());
                    returnDict[onPos].Add(midiChordDef);
                    midiControlDefs.Clear();
                }
            }
            return returnDict;
        }

        /// <summary>
        /// The list of MidiControlDefs contains only the most recent of each type of MidiControlDef to happen before,
        /// or simultaneously with, the onPosition. The onPosition is the key in the returned dictionary.
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        public Dictionary<int, List<MidiControlDef>> GetTimedMidiControlDefs(Track track)
        {
            Dictionary<int, List<MidiControlDef>> returnDict = new Dictionary<int, List<MidiControlDef>>();
            IEnumerable<MidiEvent> ie = track.Iterator();
            IEnumerator<MidiEvent> midiEvents = ie.GetEnumerator();
            // Here, midiEvents.Current is before the first midiEvent. (According to Leslie Sanford.)
            MidiEvent midiEvent = null;
            KeyValuePair<int, List<MidiControlDef>> timedMidiControls = new KeyValuePair<int, List<MidiControlDef>>(int.MinValue, null);
            List<MidiControlDef> midiControlDefs = null;
            while(midiEvents.MoveNext())
            {
                //Console.WriteLine("Track: channelIndex={0}, Count={1}, Length={2}",
                //channelIndex, track.Count, track.Length);
                midiEvent = midiEvents.Current;
                ChannelMessage channelMessage = midiEvent.MidiMessage as ChannelMessage;
                int msPosition = midiEvent.AbsoluteTicks;
                if(timedMidiControls.Key != msPosition)
                {
                    timedMidiControls = new KeyValuePair<int, List<MidiControlDef>>(msPosition, new List<MidiControlDef>());
                    if(!returnDict.ContainsKey(msPosition))
                    {
                        returnDict.Add(timedMidiControls.Key, timedMidiControls.Value);
                        midiControlDefs = timedMidiControls.Value;
                    }
                }

                if(channelMessage != null)
                {
                    MidiControlDef midiControlDef = new MidiControlDef(channelMessage);
                    if(midiControlDef != null)
                    {
                        int removeAt = -1;
                        for(int i = 0; i < midiControlDefs.Count; ++i)
                        {
                            if(HaveTheSameMidiControlDefType(midiControlDef, midiControlDefs[i]))
                            {
                                removeAt = i;
                                break;
                            }
                        }
                        if(removeAt >= 0)
                        {
                            midiControlDefs.RemoveAt(removeAt);
                        }
                        midiControlDefs.Add(midiControlDef);
                    }
                }
            }
            if(!returnDict.ContainsKey(timedMidiControls.Key))
                returnDict.Add(timedMidiControls.Key, timedMidiControls.Value);
            return returnDict;
        }







        /// <summary>
        /// In the returned Dictionary, the key is channel, value is [msPosition, MidiMoment]
        /// Entries are constructed for this dictionary only for the channels which actually contain Chords which
        /// will be played by the assistant or live performer.
        /// The smallest msPosition in the dictionary is always 0.
        /// (The performance always begins at the first sounding Moment, so all the midiMoment.msPositions
        /// are adjusted if necessary, so that the first moment has MsPosition=0). 
        /// </summary>
        public Dictionary<int, SortedDictionary<int, MidiMoment>> GetMidiChannelMoments(Multimedia.Midi.Sequence sequence, out List<int> midiChannels)
        {
            Dictionary<int, SortedDictionary<int, MidiMoment>> midiChannelMoments = null;
            if(sequence != null)
            {
                // WriteTrackNoteInfo(sequence); // (slow) test function for printing info to the Output window
                midiChannelMoments = GetMidiChannelMomentsFromSequence(sequence);
            }

            midiChannels = new List<int>();
            foreach(int channel in midiChannelMoments.Keys)
            {
                midiChannels.Add(channel);
            }

            if(midiChannelMoments == null)
                Debug.Fail("Error converting midi sequence to List<MidiMoment>");

            /// (The performance always begins at the first sounding Moment, so all the midiMoment.msPositions
            /// are adjusted if necessary, so that the first moment has MsPosition=0).
            int currentFirstPosition = GetFirstPosition(midiChannelMoments);
            Dictionary<int, SortedDictionary<int, MidiMoment>> returnMidiChannelMoments =
                AdjustToPositionZero(midiChannelMoments, currentFirstPosition);

            return returnMidiChannelMoments;
        }

        /// <summary>
        /// In the returned Dictionary, the key is channel, value is [msPosition, MidiMoment]
        /// Entries are constructed for this dictionary only for the channels
        /// which actually contain Chords which will be played by the assistant or live performer.
        /// The smallest msPosition in the dictionary is always 0.
        /// </summary>
        private Dictionary<int, SortedDictionary<int, MidiMoment>> GetMidiChannelMomentsFromSequence(Multimedia.Midi.Sequence sequence)
        {
            Dictionary<int, SortedDictionary<int, MidiMoment>> returnDict = new Dictionary<int, SortedDictionary<int, MidiMoment>>();
            foreach(Track track in sequence)
            {
                SortedDictionary<int, MidiMoment> midiMomentsDict = new SortedDictionary<int, MidiMoment>();

                Dictionary<int, List<NoteOn>> timedNoteOns = GetTimedNoteOns(track);
                Dictionary<int, List<NoteOff>> timedNoteOffs = GetTimedNoteOffs(track);
                Dictionary<int, List<MidiControl>> timedMidiControls = GetTimedMidiControls(track);

                if(timedNoteOns.Count > 0)
                {
                    Dictionary<int, List<MidiChord>> timedMidiChords =
                        GetTimedMidiChords(timedNoteOns, timedNoteOffs, timedMidiControls);

                    Debug.Assert(timedMidiChords.Count > 0);

                    foreach(int key in timedMidiChords.Keys)
                    {
                        if(!midiMomentsDict.ContainsKey(key))
                        {
                            midiMomentsDict.Add(key, new MidiMoment(key));
                        }
                        midiMomentsDict[key].MidiChords.AddRange(timedMidiChords[key]);
                    }

                    int channel = -1;
                    foreach(int pos in timedNoteOns.Keys)
                    {
                        channel = timedNoteOns[pos][0].Channel;
                        break;
                    }
                    Debug.Assert(!returnDict.ContainsKey(channel));
                    returnDict.Add(channel, midiMomentsDict);
                }
            }

            return returnDict;
        }

        private Dictionary<int, List<NoteOn>> GetTimedNoteOns(Track track)
        {
            Dictionary<int, List<NoteOn>> returnDict = new Dictionary<int, List<NoteOn>>();
            IEnumerable<MidiEvent> ie = track.Iterator();
            IEnumerator<MidiEvent> midiEvents = ie.GetEnumerator();
            // Here, midiEvents.Current is before the first midiEvent. (According to Leslie Sanford.)
            MidiEvent midiEvent = null;
            KeyValuePair<int, List<NoteOn>> timedNoteOns = new KeyValuePair<int, List<NoteOn>>(int.MinValue, null);
            int previousPosition = int.MinValue;
            while(midiEvents.MoveNext())
            {
                //Console.WriteLine("Track: channelIndex={0}, Count={1}, Length={2}",
                //channelIndex, track.Count, track.Length);
                midiEvent = midiEvents.Current;
                ChannelMessage channelMessage = midiEvent.MidiMessage as ChannelMessage;
                int msPosition = midiEvent.AbsoluteTicks;

                if(channelMessage != null && channelMessage.Command == ChannelCommand.NoteOn)
                {
                    int channel = channelMessage.MidiChannel;
                    NoteOn noteOn = new NoteOn(channel, channelMessage.Data1, channelMessage.Data2);
                    List<NoteOn> noteOns = CollectNoteOnMessages(noteOn, msPosition, ref timedNoteOns);
                    if(noteOns != null) // is null when previousPosition < msPosition
                    {
                        Debug.Assert(previousPosition < msPosition);
                        returnDict.Add(previousPosition, noteOns);
                    }
                    previousPosition = msPosition;
                }
            }
            if(timedNoteOns.Value != null)
                returnDict.Add(timedNoteOns.Key, timedNoteOns.Value);

            return returnDict;
        }
        private List<NoteOn> CollectNoteOnMessages(NoteOn noteOn, int msPosition, ref KeyValuePair<int, List<NoteOn>> timedNoteOns)
        {
            List<NoteOn> returnList = null;

            if(timedNoteOns.Value == null)
            {
                timedNoteOns = new KeyValuePair<int, List<NoteOn>>(msPosition, new List<NoteOn>());
            }

            if(msPosition == timedNoteOns.Key)
            {
                timedNoteOns.Value.Add(noteOn);
            }
            else //if(msPosition != timedNoteOnMessages.Key)
            {
                returnList = timedNoteOns.Value;
                timedNoteOns =
                    new KeyValuePair<int, List<NoteOn>>(msPosition, new List<NoteOn>() { noteOn });
            }
            return returnList;
        }

        private Dictionary<int, List<NoteOff>> GetTimedNoteOffs(Track track)
        {
            Dictionary<int, List<NoteOff>> returnDict = new Dictionary<int, List<NoteOff>>();
            IEnumerable<MidiEvent> ie = track.Iterator();
            IEnumerator<MidiEvent> midiEvents = ie.GetEnumerator();
            // Here, midiEvents.Current is before the first midiEvent. (According to Leslie Sanford.)
            MidiEvent midiEvent = null;
            KeyValuePair<int, List<NoteOff>> timedNoteOffs = new KeyValuePair<int, List<NoteOff>>(int.MinValue, null);
            int previousPosition = int.MinValue;
            while(midiEvents.MoveNext())
            {
                //Console.WriteLine("Track: channelIndex={0}, Count={1}, Length={2}",
                //channelIndex, track.Count, track.Length);
                midiEvent = midiEvents.Current;
                ChannelMessage channelMessage = midiEvent.MidiMessage as ChannelMessage;
                int msPosition = midiEvent.AbsoluteTicks;

                if(channelMessage != null && channelMessage.Command == ChannelCommand.NoteOff)
                {
                    int channel = channelMessage.MidiChannel;
                    NoteOff noteOff = new NoteOff(channel, channelMessage.Data1, channelMessage.Data2);
                    List<NoteOff> noteOffs = CollectNoteOffMessages(noteOff, msPosition, ref timedNoteOffs);
                    if(noteOffs != null)
                    {
                        Debug.Assert(previousPosition < msPosition);
                        returnDict.Add(previousPosition, noteOffs);
                    }
                    previousPosition = msPosition;
                }
            }
            if(timedNoteOffs.Value != null)
                returnDict.Add(timedNoteOffs.Key, timedNoteOffs.Value);

            return returnDict;
        }
        private List<NoteOff> CollectNoteOffMessages(NoteOff noteOff, int msPosition, ref KeyValuePair<int, List<NoteOff>> timedNoteOffs)
        {
            List<NoteOff> returnList = null;

            if(timedNoteOffs.Value == null)
            {
                timedNoteOffs = new KeyValuePair<int, List<NoteOff>>(msPosition, new List<NoteOff>());
            }

            if(msPosition == timedNoteOffs.Key)
            {
                timedNoteOffs.Value.Add(noteOff);
            }
            else //if(msPosition != timedNoteOffMessages.Key)
            {
                returnList = timedNoteOffs.Value;
                timedNoteOffs =
                    new KeyValuePair<int, List<NoteOff>>(msPosition, new List<NoteOff>() { noteOff });
            }
            return returnList;
        }
        /// <summary>
        /// The list of MidiControls contains only the most recent of each type of MidiControl to happen before,
        /// or simultaneously with, the onPosition. The onPosition is the key in the returned dictionary.
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        private Dictionary<int, List<MidiControl>> GetTimedMidiControls(Track track)
        {
            Dictionary<int, List<MidiControl>> returnDict = new Dictionary<int, List<MidiControl>>();
            IEnumerable<MidiEvent> ie = track.Iterator();
            IEnumerator<MidiEvent> midiEvents = ie.GetEnumerator();
            // Here, midiEvents.Current is before the first midiEvent. (According to Leslie Sanford.)
            MidiEvent midiEvent = null;
            KeyValuePair<int, List<MidiControl>> timedMidiControls = new KeyValuePair<int, List<MidiControl>>(int.MinValue, null);
            List<MidiControl> midiControls = null;
            while(midiEvents.MoveNext())
            {
                //Console.WriteLine("Track: channelIndex={0}, Count={1}, Length={2}",
                //channelIndex, track.Count, track.Length);
                midiEvent = midiEvents.Current;
                ChannelMessage channelMessage = midiEvent.MidiMessage as ChannelMessage;
                int msPosition = midiEvent.AbsoluteTicks;
                if(timedMidiControls.Key != msPosition)
                {
                    timedMidiControls = new KeyValuePair<int, List<MidiControl>>(msPosition, new List<MidiControl>());
                    if(!returnDict.ContainsKey(msPosition))
                    {
                        returnDict.Add(timedMidiControls.Key, timedMidiControls.Value);
                        midiControls = timedMidiControls.Value;
                    }
                }

                if(channelMessage != null)
                {
                    MidiControl midiControl = GetMidiControl(channelMessage);
                    if(midiControl != null)
                    {
                        int removeAt = -1;
                        for(int i = 0; i < midiControls.Count; ++i)
                        {
                            if(HaveTheSameMidiControlType(midiControl, midiControls[i]))
                            {
                                removeAt = i;
                                break;
                            }
                        }
                        if(removeAt >= 0)
                        {
                            midiControls.RemoveAt(removeAt);
                        }
                        midiControls.Add(midiControl);
                    }
                }
            }
            if(!returnDict.ContainsKey(timedMidiControls.Key))
                returnDict.Add(timedMidiControls.Key, timedMidiControls.Value);
            return returnDict;
        }
        /// <summary>
        /// The returned midiControl will be null if the control is not supported by Moritz.
        /// </summary>
        /// <param name="channelMessage"></param>
        /// <returns></returns>
        private MidiControl GetMidiControl(ChannelMessage channelMessage)
        {
            int channel = channelMessage.MidiChannel;
            byte value = (byte)channelMessage.Data2;
            MidiControl midiControl = null;
            switch(channelMessage.Command)
            {
                case ChannelCommand.Controller:
                {
                    ControllerType controllerType = (ControllerType)channelMessage.Data1;
                    switch(controllerType)
                    {
                        case ControllerType.AllSoundOff:
                        {
                            midiControl = new AllSoundOff(channel);
                            break;
                        }
                        case ControllerType.AllNotesOff:
                        {
                            midiControl = new AllNotesOff(channel);
                            break;
                        }
                        case ControllerType.AllControllersOff:
                        {
                            midiControl = new AllControllersOff(channel);
                            break;
                        }
                        case ControllerType.Balance:
                        {
                            midiControl = new Balance(channel, value, ControlContinuation.NoChange);
                            break;
                        }
                        case ControllerType.BankSelect:
                        {
                            midiControl = new BankControl(channel, value);
                            break;
                        }
                        case ControllerType.RegisteredParameterCoarse: // standard Midi...
                        {
                            midiControl = new PitchWheelDeviation(channel, value);
                            break;
                        }
                        case ControllerType.Expression:
                        {
                            midiControl = new Expression(channel, value, ControlContinuation.NoChange);
                            break;
                        }
                        case ControllerType.ModulationWheel:
                        {
                            midiControl = new ModulationWheel(channel, value, ControlContinuation.NoChange);
                            break;
                        }
                        case ControllerType.Pan:
                        {
                            midiControl = new Pan(channel, value, ControlContinuation.NoChange);
                            break;
                        }
                        case ControllerType.Volume:
                        {
                            midiControl = new Volume(channel, value, ControlContinuation.NoChange);
                            break;
                        }
                    }
                    break;
                }
                case ChannelCommand.PitchWheel:
                {
                    // Achtung: PitchWheel uses channelmessage.Data2 (not Data1) 15.10.2011!
                    midiControl = new PitchWheel(channel, value, ControlContinuation.NoChange);
                    break;
                }
                case ChannelCommand.ProgramChange:
                {
                    midiControl = new PatchControl(channel, (byte)channelMessage.Data1);
                    break;
                }
            }

            return midiControl;
        }
        private bool HaveTheSameMidiControlDefType(MidiControlDef mc1, MidiControlDef mc2)
        {
            bool argsHaveTheSameType = false;
            if(mc1.ControllerType == mc2.ControllerType 
            && mc1.ChannelCommand == mc2.ChannelCommand)
            {
                argsHaveTheSameType = true;
            }
            return argsHaveTheSameType;
        }
        private bool HaveTheSameMidiControlType(MidiControl mc1, MidiControl mc2)
        {
            bool argsHaveTheSameType = false;
            if((mc1 is AllSoundOff && mc2 is AllSoundOff)
            || (mc1 is AllNotesOff && mc2 is AllNotesOff)
            || (mc1 is AllControllersOff && mc2 is AllControllersOff)
            || (mc1 is Balance && mc2 is Balance)
            || (mc1 is BankControl && mc2 is BankControl)
            || (mc1 is PitchWheelDeviation && mc2 is PitchWheelDeviation)
            || (mc1 is Expression && mc2 is Expression)
            || (mc1 is ModulationWheel && mc2 is ModulationWheel)
            || (mc1 is Pan && mc2 is Pan)
            || (mc1 is Volume && mc2 is Volume)
            || (mc1 is PitchWheel && mc2 is PitchWheel)
            || (mc1 is PatchControl && mc2 is PatchControl))
            {
                argsHaveTheSameType = true;
            }
            return argsHaveTheSameType;
        }
        /// <summary>
        /// None of the MidiChords returned by this function are ornaments.
        /// In other words, this function returns a MidiChord for each ChordOn in the midi file.
        /// A ChordOn contains a set of NoteOns having the same msPosition and a set of NoteOffs having the same position.
        /// </summary>
        /// <returns>
        /// A dictionary whose key is the onMsPosition of the list of chords
        /// </returns>
        private Dictionary<int, List<MidiChord>> GetTimedMidiChords(
            Dictionary<int, List<NoteOn>> timedNoteOns,
            Dictionary<int, List<NoteOff>> timedNoteOffs,
            Dictionary<int, List<MidiControl>> timedMidiControls)
        {
            Dictionary<int, List<MidiChord>> returnDict = new Dictionary<int, List<MidiChord>>();
            ChannelState channelState = new ChannelState();
            // All the notes in a MidiChord begin and end at the same time.
            // this track can have several simultaneously beginning MidiChords which end at different times.
            foreach(int onPos in timedNoteOns.Keys)
            {
                /// The noteOffsDict contains all the noteOffs corresponding to the input noteOns.
                /// The number of keys in this dictionary is the number of MidiChords which have to be constructed.
                /// If the key is int.MaxValue, the chord has no ChordOff.
                Dictionary<int, List<NoteOff>> noteOffsDict = GetNoteOffsDict(onPos, timedNoteOns[onPos], timedNoteOffs);

                /// The midiControls List contains a list of unique midiControls which happen before or synchronously with the onPos.
                /// MidiControls which happen before or synchronously with onPos are removed from the timedMidiControls dictionary,
                /// so that they are not used twice.

                //List<MidiControl> midiControls = GetMidiControls(onPos, timedMidiControls);
                List<MidiControl> midiControls = null;
                if(timedMidiControls.ContainsKey(onPos))
                    midiControls = timedMidiControls[onPos];

                foreach(int offPos in noteOffsDict.Keys)
                {
                    int channel = noteOffsDict[offPos][0].Channel;
                    bool hasChordOff = (offPos != int.MaxValue);
                    int msDuration = offPos - onPos;
                    List<byte> pitches = new List<byte>();
                    foreach(NoteOff noteOff in noteOffsDict[offPos])
                    {
                        pitches.Add((byte)noteOff.Pitch);
                    }
                    List<byte> velocities = new List<byte>();
                    foreach(NoteOn noteOn in timedNoteOns[onPos])
                    {
                        foreach(byte pitch in pitches)
                        {
                            if(noteOn.Pitch == pitch)
                            {
                                velocities.Add((byte)noteOn.Velocity);
                                break;
                            }
                        }
                    }
                    Debug.Assert(pitches.Count == velocities.Count);
                    MidiChordDef midiChordDef = new ComposableMidiChordDef(pitches, velocities, msDuration, hasChordOff, midiControls);
                    midiControls.Clear();
                    MidiChord midiChord = new MidiChord(channel, midiChordDef, onPos, midiChordDef.MsDuration, channelState, M.DefaultMinimumBasicMidiChordMsDuration);
                    if(!returnDict.ContainsKey(onPos))
                        returnDict.Add(onPos, new List<MidiChord>());
                    returnDict[onPos].Add(midiChord);
                }
            }
            return returnDict;
        }
        /// <summary>
        /// Returns a dictionary containing all the noteOffs corresponding to the input noteOns.
        /// If there is no noteOff corresponding to a noteOn, a noteOff is added to the dictionary at key int.MaxValue.
        /// The number of keys in this dictionary is the number of MidiChords to be constructed.
        /// </summary>
        private Dictionary<int, List<NoteOff>> GetNoteOffsDict(int onPos, List<NoteOn> noteOns, Dictionary<int, List<NoteOff>> timedNoteOffs)
        {
            Dictionary<int, List<NoteOff>> noteOffsDict = new Dictionary<int, List<NoteOff>>();
            foreach(NoteOn noteOn in noteOns)
            {
                bool noteOffFound = false;
                foreach(int offPos in timedNoteOffs.Keys)
                {
                    if(offPos > onPos)
                    {
                        foreach(NoteOff noteOff in timedNoteOffs[offPos])
                        {
                            if(noteOn.Pitch == noteOff.Pitch)
                            {
                                if(!noteOffsDict.ContainsKey(offPos))
                                    noteOffsDict.Add(offPos, new List<NoteOff>());
                                noteOffsDict[offPos].Add(noteOff);
                                noteOffFound = true;
                                break;
                            }
                        }
                    }
                    if(noteOffFound)
                        break;
                }
                if(!noteOffFound)
                {
                    if(!noteOffsDict.ContainsKey(int.MaxValue))
                        noteOffsDict.Add(int.MaxValue, new List<NoteOff>());
                    noteOffsDict[int.MaxValue].Add(new NoteOff(noteOn.Channel, noteOn.Pitch, noteOn.Velocity));
                }
            }
            return noteOffsDict;
        }

        private int GetFirstPosition(Dictionary<int, SortedDictionary<int, MidiMoment>> midiChannelMoments)
        {
            int firstPosition = int.MaxValue;
            foreach(int channel in midiChannelMoments.Keys)
            {
                SortedDictionary<int, MidiMoment> mmDict = midiChannelMoments[channel];
                foreach(KeyValuePair<int, MidiMoment> kvp in mmDict)
                {
                    firstPosition = (firstPosition < kvp.Key) ? firstPosition : kvp.Key;
                    break;
                }
            }
            return firstPosition;
        }

        private Dictionary<int, SortedDictionary<int, MidiMoment>>
            AdjustToPositionZero(Dictionary<int, SortedDictionary<int, MidiMoment>> midiChannelMoments, int currentFirstPosition)
        {
            Dictionary<int, SortedDictionary<int, MidiMoment>> returnDict = new Dictionary<int, SortedDictionary<int, MidiMoment>>();
            foreach(int channel in midiChannelMoments.Keys)
            {
                SortedDictionary<int, MidiMoment> mmDict = midiChannelMoments[channel];
                SortedDictionary<int, MidiMoment> newDict = new SortedDictionary<int,MidiMoment>();
                foreach(KeyValuePair<int, MidiMoment> kvp in mmDict)
                {
                    int newPosition = kvp.Key - currentFirstPosition;
                    MidiMoment midiMoment = kvp.Value;
                    midiMoment.MsPosition = newPosition;
                    foreach(MidiChord midiChord in midiMoment.MidiChords)
                    {
                        midiChord.MsPosition = newPosition;
                    }
                    newDict.Add(newPosition, midiMoment);
                }
                returnDict.Add(channel, newDict);
            }
            return returnDict;
        }

        /// <summary>
        /// This is just a demo function, showing how to retrieve timing information for noteOn and noteOff messages
        /// in each track in a MIDI file (_sequence). It does nothing but write the information to the console.
        /// </summary>
        private void WriteTrackNoteInfo(Multimedia.Midi.Sequence sequence)
        {
            Console.WriteLine("Sequence: Division={0}, Count={1}", sequence.Division, sequence.Count);
            int channelIndex = 0;
            foreach(Track track in sequence)
            {
                Console.WriteLine("Track: channelIndex={0}, Count={1}, Length={2}",
                    channelIndex++, track.Count, track.Length);
                IEnumerator<MidiEvent> midiEvents = track.Iterator().GetEnumerator();
                // Here, midiEvents.Current is before the first midiEvent. (According to Leslie Sanford.)
                MidiEvent midiEvent = null;
                while(midiEvents.MoveNext())
                {
                    midiEvent = midiEvents.Current;
                    ChannelMessage channelMessage = midiEvent.MidiMessage as ChannelMessage;
                    if(channelMessage != null)
                    {
                        //Note that midiEvent.DeltaTicks is the time to wait until playing *this* midiEvent.
                        if(channelMessage.Command == ChannelCommand.NoteOn)
                        {
                            Console.WriteLine("    NoteOn:  AbsoluteTicks={0}, Pitch={1}, Velocity={2}",
                                midiEvent.AbsoluteTicks, channelMessage.Data1, channelMessage.Data2);
                        }
                        else if(channelMessage.Command == ChannelCommand.NoteOff)
                        {
                            Console.WriteLine("        NoteOff: AbsoluteTicks={0}, Pitch={1}, Velocity={2}",
                                midiEvent.AbsoluteTicks, channelMessage.Data1, channelMessage.Data2);
                        }
                    }
                }
            }
        }
    }
}
