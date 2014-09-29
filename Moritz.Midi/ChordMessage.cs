using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;

using Multimedia.Midi;

namespace Moritz.Midi
{
	/// <summary>
	/// A ChordMessage is a temporal event containing a collection of synchronous NoteMessages.
	/// ChordOn is a derived type, containing a List{NoteMessage} containing only NoteOn messages,
	/// ChordOff is a derived type, containing a List{NoteMessage} containing only NoteOff messages,
	/// 
	/// Moritz allows NoteOn messages to have zero velocity (necessary when applying factors to
	/// existing velocities) but generally uses real NoteOff messages.
	/// 
	/// Relation to notated (spatial) objects:
	/// A ChordSymbol, which contains a collection of notehead symbols etc., is associated with
	///      a ChordOn followed by a ChordOff at the end of the chord's duration. Note that
	///      ChordOffs may be suppressed or delayed if ChannelState.Sustain is true.
	///      See http://users.adelphia.net/~jgglatt/tech/midispec.htm for a description of the
	///      Hold Pedal Message.
	///      Note also that a ChordOff need not affect all the currently sounding pitches,
	///      and need not be sent at all if its messages have been removed.
	/// A RestSymbol, which contains no sub-symbols, has neither a ChordOn nor a ChordOff.
	/// </summary>
	public abstract class ChordMessage
	{
        public ChordMessage() { }
        public ChordMessage(BasicMidiChord container)
        {
            BasicMidiChord = container;
        }

		public abstract ChordOn CloneChordOn();
		public abstract ChordOff CloneChordOff();

		public abstract void AddNote(NoteMessage note);

		public void RemoveNote(NoteMessage note)
		{
			if(_notes.Count > 0)
				for(int i = _notes.Count - 1 ; i >= 0 ; i--)
				{
					if(note.Channel == _notes[i].Channel
						&& note.Pitch == _notes[i].Pitch
						&& note.Velocity == _notes[i].Velocity)
					{
						_notes.RemoveAt(i);
						break;
					}
				}
		}

		/// <summary>
		/// The notes of a ChordMessage must all be in one channel
		/// </summary>
		public int Channel
		{
			get
			{
				if(_notes.Count > 0)
					return _notes[0].Channel;
				else return 0;
			}
		}
		/// <summary>
		/// The notes of a ChordMessage usually have one velocity.
		/// This Property is the velocity of the first note in Notes
		/// </summary>
		public int Velocity
		{
			get
			{
				Debug.Assert(_notes.Count > 0);
				return _notes[0].Velocity;
			}
		}

        public List<NoteMessage> Notes { get { return _notes; } set { _notes = value; } }
		public abstract List<ChannelMessage> ChannelMessages { get; }
		protected List<ChannelMessage> GetChannelMessages(ChannelCommand noteOnOff)
		{
			List<ChannelMessage> messages = new List<ChannelMessage>();
			ChannelMessageBuilder builder = new ChannelMessageBuilder();
			builder.MidiChannel = this.Channel;

            //foreach(MidiControl midiControl in this.MidiControls)
            //{
            //    messages.AddRange(midiControl.ChannelMessages);
            //}
			builder.Command = noteOnOff;
			foreach(NoteMessage midiNote in _notes)
			{
				builder.Data1 = midiNote.Pitch;
				builder.Data2 = midiNote.Velocity;

				builder.Build();

				messages.Add(builder.Result);
			}
			return messages;
		}

		protected List<NoteMessage> _notes = new List<NoteMessage>();
		public static readonly int MaxMilliseconds = 12; // the maximum number of milliseconds between midi note-on or note-off events in the chord. 

        protected BasicMidiChord BasicMidiChord = null; // container
	}

	/// <summary>
	/// Moritz ensures that a ChordOn's _notes list contains only NoteOn messages, and that
	/// all such messages are in the same channel.
	/// </summary>
	public class ChordOn : ChordMessage
	{
        public ChordOn() { }
		public ChordOn(BasicMidiChord container)
            : base(container)
        { }

		public override ChordOn CloneChordOn()
		{
            ChordOn chordOn = new ChordOn(this.BasicMidiChord);

			foreach(NoteOn noteOn in this.Notes)
				chordOn.AddNote(noteOn.CloneNoteOn());

            chordOn.GetIdleOrnamentBW = GetIdleOrnamentBW;

			return chordOn;
		}
		public override ChordOff CloneChordOff()
		{
            ChordOff chordOff = new ChordOff(this.BasicMidiChord);

			foreach(NoteOn noteOn in this.Notes)
				chordOff.AddNote(noteOn.CloneNoteOff());

			return chordOff;
		}

		public override void AddNote(NoteMessage note)
		{
			// All notes in a ChordOn must have the same channel.
			Debug.Assert(note is NoteOn && ((_notes.Count == 0) || (note.Channel == _notes[0].Channel)));
			NoteOn newNote = new NoteOn();
			newNote.Channel = note.Channel;
			newNote.Pitch = note.Pitch;
			newNote.Velocity = note.Velocity;
			_notes.Add(newNote);
		}

		public override List<ChannelMessage> ChannelMessages
		{
			get { return GetChannelMessages(ChannelCommand.NoteOn); }
		}

        /// <summary>
        /// The backgroundWorker used by AssistantPerformer.
        /// </summary>
        internal delegate BackgroundWorker GetIdleOrnamentBWHandler();
        internal GetIdleOrnamentBWHandler GetIdleOrnamentBW = null;
	}
	/// <summary>
	/// Moritz ensures that a ChordOff's _notes list contains only NoteOff messages, and that all
	/// such messages are in the same channel.
	/// </summary>
	public class ChordOff : ChordMessage
	{
        public ChordOff() { }
        public ChordOff(BasicMidiChord container) : base(container) { } 

		public override ChordOn CloneChordOn()
		{
            ChordOn chordOn = new ChordOn(this.BasicMidiChord);

			foreach(NoteOff noteOff in this.Notes)
				chordOn.AddNote(noteOff.CloneNoteOn());

			return chordOn;
		}
		public override ChordOff CloneChordOff()
		{
            ChordOff chordOff = new ChordOff(this.BasicMidiChord);

			foreach(NoteOff noteOff in this.Notes)
				chordOff.AddNote(noteOff.CloneNoteOff());

			return chordOff;
		}
		public override void AddNote(NoteMessage note)
		{
			// All notes in a ChordOff must have the same channel.
			Debug.Assert(note is NoteOff && ((_notes.Count == 0) || (note.Channel == _notes[0].Channel)));
			NoteOff noteOff = new NoteOff();
			noteOff.Channel = note.Channel;
			noteOff.Pitch = note.Pitch;
			noteOff.Velocity = 64;
			_notes.Add(noteOff);
		}
        
        public override List<ChannelMessage> ChannelMessages
		{
			get { return GetChannelMessages(ChannelCommand.NoteOff); }
		}
	}
}
