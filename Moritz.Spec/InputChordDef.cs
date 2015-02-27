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
            _notatedMidiPitches = new List<byte>(){midiPitch};
			_lyric = lyric;
			_inputControls = null;
			_inputControlsPerMidiPitch = null;
            List<TrkRef> trkRefs = new List<TrkRef>(){new TrkRef(midiPitch, trkMidiChannel, trkLength, 0, inputControls)}; // inputControls can be null
            _trkRefsPerMidiPitch.Add(trkRefs);
            _msDurationToNextBarline = null;
        }

        /// <summary>
        /// Constructs a multi-pitch chord, each pitch having a list of trkRefs.
        /// This constructor makes its own copy of the midiPitches but not of the trkRefs.
        /// </summary>
        public InputChordDef(int msPosition, int msDuration, List<byte> notatedMidiPitches, List<List<TrkRef>> trkRefsPerMidiPitch, string lyric)
            : base(msDuration)
        {
            Debug.Assert(notatedMidiPitches.Count == trkRefsPerMidiPitch.Count);
            _msPosition = msPosition;
            _notatedMidiPitches = new List<byte>(notatedMidiPitches);
			_lyric = lyric;
			_inputControls = null;
			_inputControlsPerMidiPitch = null;
            _trkRefsPerMidiPitch = trkRefsPerMidiPitch;
            _msDurationToNextBarline = null;
        }

        /// <summary>
        /// Transpose the notatedPitches by the number of semitones given in the argument.
        /// Negative interval values transpose down.
        /// It is not an error if Midi values would exceed the range 0..127.
        /// In this case, they are silently coerced to 0 or 127 respectively.
        /// </summary>
        public void Transpose(int interval)
        {
            for(int i = 0; i < _notatedMidiPitches.Count; ++i)
            {
                _notatedMidiPitches[i] = M.MidiValue(_notatedMidiPitches[i] + interval);
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
            for(int i = 0; i < _notatedMidiPitches.Count; ++i)
            {
                byte pitch = _notatedMidiPitches[i];
                w.WriteStartElement("inputNote");
                w.WriteAttributeString("notatedKey", pitch.ToString());
				if(_inputControlsPerMidiPitch != null && _inputControlsPerMidiPitch[i] != null)
				{
					_inputControlsPerMidiPitch[i].WriteSvg(w);
				}
                List<TrkRef> trkRefs = this.TrkRefsPerMidiPitch[i];
                w.WriteStartElement("trkRefs");
                foreach(TrkRef trkRef in trkRefs)
                {
                    trkRef.WriteSvg(w);
                }
                w.WriteEndElement(); // score:trkRefs
                w.WriteEndElement(); // score:inputNote
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

		public List<byte> NotatedMidiPitches { get { return _notatedMidiPitches; } set { _notatedMidiPitches = value; } }
		protected List<byte> _notatedMidiPitches = new List<byte>();

		public List<InputControls> InputControlsPerMidiPitch
		{ 
			get	{ return _inputControlsPerMidiPitch; }
			set
			{ 
				if(value.Count != _notatedMidiPitches.Count)
				{
					throw new ApplicationException("This list must have one member per notated midi pitch (the member can be null).");
				}
				_inputControlsPerMidiPitch = value;
			}
		}
		private List<InputControls> _inputControlsPerMidiPitch = null;

        public List<List<TrkRef>> TrkRefsPerMidiPitch { get { return _trkRefsPerMidiPitch; } }
        private List<List<TrkRef>> _trkRefsPerMidiPitch = new List<List<TrkRef>>();

        public int? MsDurationToNextBarline { get { return _msDurationToNextBarline; } set { _msDurationToNextBarline = value; } }
        private int? _msDurationToNextBarline = null;
    }
}
