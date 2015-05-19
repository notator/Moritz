using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Xml;
using System.Text;

using Krystals4ObjectLibrary;
using Moritz.Globals;
using Moritz.Xml;

namespace Moritz.Spec
{
    ///<summary>
    /// A InputChordDef can be saved and retrieved from voices in an SVG file.
    /// Each inputChord in an SVG file will be given an ID of the form "inputChord"+uniqueNumber, but
    /// Moritz does not actually use the ids, so they are not read into in UniqueInputChordDefs.
    ///</summary>
    public class InputChordDef : DurationDef, IUniqueSplittableChordDef
    {
        public InputChordDef()
            :base(0)
        {
        }

        /// <summary>
        /// constructs a 1-pitch chord whose list of trkRefs contains a single trkRef (having msOffset = 0).
        /// </summary>
        public InputChordDef(int msPosition, int msDuration, byte midiPitch, string lyric, byte trkMidiChannel, byte trkLength, InputControls inputControls)
            : base(msDuration)
        {
			_msPosition = msPosition;
			_lyric = lyric;
			_inputControls = inputControls;
			_msDurationToNextBarline = null;
			
			_inputNoteDefs = new List<InputNoteDef>();
			_inputNoteDefs.Add(new InputNoteDef(midiPitch, null, new List<TrkRef>() { new TrkRef(trkMidiChannel, trkLength, 0) }));
        }

        /// <summary>
        /// Constructs a multi-note chord, each note having a notated pitch, and a list of trkRefs.
        /// This constructor makes its own copy of the midiPitches but not of the trkRefs.
        /// </summary>
        public InputChordDef(int msPosition, int msDuration, List<byte> notatedMidiPitches, List<List<TrkRef>> trkRefsPerMidiPitch, string lyric)
            : base(msDuration)
        {

            _msPosition = msPosition;
			_lyric = lyric;
			_inputControls = null;
			_msDurationToNextBarline = null;

			_inputNoteDefs = new List<InputNoteDef>();
			Debug.Assert(notatedMidiPitches.Count == trkRefsPerMidiPitch.Count);
			for(int i = 0; i < notatedMidiPitches.Count; ++i)
			{
				_inputNoteDefs.Add(new InputNoteDef(notatedMidiPitches[i], null, trkRefsPerMidiPitch[i]));
			}
        }

        /// <summary>
        /// Transpose the notatedPitches by the number of semitones given in the argument.
        /// Negative interval values transpose down.
        /// It is not an error if Midi values would exceed the range 0..127.
        /// In this case, they are silently coerced to 0 or 127 respectively.
        /// </summary>
        public void Transpose(int interval)
        {
			for(int i = 0; i < _inputNoteDefs.Count; ++i)
            {
				_inputNoteDefs[i].NotatedMidiPitch = M.MidiValue(_inputNoteDefs[i].NotatedMidiPitch + interval);
            }
        }

        public override string ToString()
        {
            return ("MsPosition=" + MsPosition.ToString() + " MsDuration=" + MsDuration.ToString() + " InputChordDef");
        }

        /// <summary>
        /// Multiplies the MsDuration by the given factor.
        /// </summary>
        /// <param name="factor"></param>
        public void AdjustMsDuration(double factor)
        {
            MsDuration = (int)(_msDuration * factor);
        }

        /// <summary>
        /// Writes the logical content of this InputChordDef
        /// </summary>
        /// <param name="w"></param>
        public void WriteSvg(SvgWriter w)
        {
			// we are inside a score:inputChord element

			if(_inputControls != null)
			{
				_inputControls.WriteSvg(w);
			}

            w.WriteStartElement("score", "inputNotes", null);
			foreach(InputNoteDef ind in _inputNoteDefs)
			{
				ind.WriteSvg(w);
			}

            w.WriteEndElement(); // score:inputNotes
        }

        public override IUniqueDef DeepClone()
        {
            throw new NotImplementedException("InputChordDef.DeepClone()");
        }

        public int MsPosition { get { return _msPosition; } set { _msPosition = value; } }
        private int _msPosition = 0;

		public string Lyric { get { return _lyric; } set { _lyric = value; } }
		private string _lyric = null;

		public InputControls InputControls { get { return _inputControls; } set { _inputControls = value; } }
		private InputControls _inputControls = null;

		public List<InputNoteDef> InputNoteDefs { get {return _inputNoteDefs; }}
		private List<InputNoteDef> _inputNoteDefs = new List<InputNoteDef>();

		public List<byte> NotatedMidiPitches
		{
			get
			{
				List<byte> rList = new List<byte>();
				foreach(InputNoteDef ind in _inputNoteDefs)
				{
					rList.Add(ind.NotatedMidiPitch);
				}
				return rList;
			}
			/// This setter creates a new list of InputNoteheadDefs, without InputControls or TrkRefs
			set
			{
				List<byte> newNoteheads = value;
				_inputNoteDefs = new List<InputNoteDef>();
				int previousPitch = -1;
				foreach(byte midiPitch in newNoteheads)
				{
					Debug.Assert(midiPitch > previousPitch);
					previousPitch = midiPitch;
					_inputNoteDefs.Add(new InputNoteDef(midiPitch, null, null));
				}
			}
		}

        public int? MsDurationToNextBarline { get { return _msDurationToNextBarline; } set { _msDurationToNextBarline = value; } }
        private int? _msDurationToNextBarline = null;
    }
}
