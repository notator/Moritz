using Moritz.Globals;

using System.Diagnostics;

namespace Moritz.Midi
{
    public abstract class NoteMessage
    {
        protected NoteMessage() { }

        protected NoteMessage(int channel, int pitch, int velocity)
        {
            Channel = channel;
            Pitch = pitch;
            Velocity = velocity;
        }

        public abstract NoteOn CloneNoteOn();
        public abstract NoteOff CloneNoteOff();

        public int Channel
        {
            get { return _channel; }
            set
            {
                M.Assert(value >= 0 || value <= 15);
                _channel = value;
            }
        }

        public int Pitch
        {
            get { return _pitch; }
            set
            {
                M.Assert(value >= 0 || value <= 127);
                _pitch = value;
            }
        }

        public int Velocity
        {
            get { return _velocity; }
            set
            {
                M.Assert(value >= 0 || value <= 127);
                _velocity = value;
            }
        }

        private int _channel = 0;
        private int _pitch = 0;
        private int _velocity = 0;
    }

    public class NoteOn : NoteMessage
    {
        public NoteOn() : base() { }

        public NoteOn(int channel, int pitch, int velocity)
            : base(channel, pitch, velocity)
        {
        }

        /// <summary>
        /// A duplicate of this NoteOn is created.
        /// </summary>
        /// <returns></returns>
        public override NoteOn CloneNoteOn()
        {
            NoteOn clone = new NoteOn(this.Channel, this.Pitch, this.Velocity);
            return clone;
        }

        /// <summary>
        /// This NoteOn is converted to a NoteOff having the same Channel and Pitch,
        /// The new NoteOff has Velocity 64 by default.
        /// For a description of MIDI note-off, see http://users.adelphia.net/~jgglatt/tech/midispec.htm 
        /// </summary>
        public override NoteOff CloneNoteOff()
        {
            // 64 is usually the default velocity for NoteOffs - see
            //
            NoteOff clone = new NoteOff(this.Channel, this.Pitch, 64);
            return clone;
        }

    }

    public class NoteOff : NoteMessage
    {
        public NoteOff() : base() { }

        public NoteOff(int channel, int pitch, int velocity)
            : base(channel, pitch, velocity)
        {
        }

        /// <summary>
        /// This NoteOff is converted to a NoteOn having the same Channel and Pitch,
        /// The new NoteOn has Velocity 64 by default.
        /// </summary>
        public override NoteOn CloneNoteOn()
        {
            NoteOn clone = new NoteOn(this.Channel, this.Pitch, 64);
            return clone;
        }

        /// <summary>
        /// A duplicate of this NoteOff is created.
        /// </summary>
        /// <returns></returns>
        public override NoteOff CloneNoteOff()
        {
            NoteOff clone = new NoteOff(this.Channel, this.Pitch, this.Velocity);
            return clone;
        }
    }
}
