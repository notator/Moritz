using Moritz.Spec;

using System.Collections.Generic;
using System.Diagnostics;

namespace Moritz.Midi
{
    public class BasicMidiChord
    {
        public BasicMidiChord(int channel, MidiChord midiChord, BasicMidiChordDef bmcd, int realMsDuration)
        {
            MidiChord = midiChord;
            //MsPosition = bmcd.MsPosition;
            MsDuration = realMsDuration;

            if(bmcd.BankIndex != null)
            {
                BankControl = new BankControl(channel, (byte)bmcd.BankIndex);
            }
            if(bmcd.PatchIndex != null)
            {
                PatchControl = new PatchControl(channel, (byte)bmcd.PatchIndex);
            }

            ChordOn = new ChordOn(this);
            SetChordOn(channel, bmcd.Pitches, bmcd.Velocities);
            if(bmcd.HasChordOff)
            {
                ChordOff = new ChordOff(this);
                SetChordOff(channel, bmcd.Pitches);
            }
        }

        private void SetChordOn(int channel, List<byte> midiPitches, List<byte> midiVelocities)
        {
            for(int i = 0; i < midiPitches.Count; i++)
            {
                NoteOn noteOn = new NoteOn(channel, midiPitches[i], midiVelocities[i]);
                ChordOn.AddNote(noteOn);
            }
        }
        private void SetChordOff(int channel, List<byte> midiPitches)
        {
            for(int i = 0; i < midiPitches.Count; i++)
            {
                NoteOff noteOff = new NoteOff(channel, midiPitches[i], 64);
                ChordOff.AddNote(noteOff);
            }
        }

        /****************/
        #region Notes
        /// <summary>
        /// Gets the velocity of the lowest note.
        /// Sets the velocities of all notes in the chord.
        /// </summary>
        public byte Velocity
        {
            get { return (byte)ChordOn.Notes[0].Velocity; }
            set
            {
                Debug.Assert(value <= 127);
                foreach(NoteMessage note in ChordOn.Notes)
                    note.Velocity = value;
            }
        }
        public void AddNote(int channel, int pitch, int velocity)
        {
            ChordOn.AddNote(new NoteOn(channel, pitch, velocity));
            ChordOff.AddNote(new NoteOff(channel, pitch, 64));
        }
        public void ClearNotes()
        {
            ChordOn.ChannelMessages.Clear();
            ChordOff.ChannelMessages.Clear();
        }
        #endregion
        /****************/

        public BankControl BankControl = null;
        public PatchControl PatchControl = null;
        /// <summary>
        /// The ChordOn message which begins this BasicMidiChord.
        /// Contains NoteOns which are sent as ChannelMessages when the ChordOn is sent.
        /// </summary>
        public readonly ChordOn ChordOn = null;
        /// <summary>
        /// The ChordOff message which ends this BasicMidiChord.
        /// Contains NoteOffs which are sent as ChannelMessages when the ChordOff is sent.
        /// The ChordOff can be null.
        /// </summary>
        public readonly ChordOff ChordOff = null;


        public readonly MidiChord MidiChord;
        //public int MsPosition = 0;
        public int MsDuration = 0;

    }
}
