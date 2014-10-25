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
        /// constructs a 1-pitch chord pointing at a single sequence
        /// </summary>
        public InputChordDef(int msPosition, int msDuration, byte midiPitch, string lyric, byte seqVoiceID, byte seqLength, InputControls inputControls)
            : base(msDuration)
        {
            _msPosition = msPosition;
            _notatedMidiPitches = new List<byte>(){midiPitch};
            _lyric = lyric;
            List<SeqRef> seqRefs = new List<SeqRef>(){new SeqRef(midiPitch, seqVoiceID, seqLength, inputControls)}; // inputControls can be null
            _seqRefsPerMidiPitch.Add(seqRefs);
            _msDurationToNextBarline = null;
        }

        /// <summary>
        /// Constructs a multi-pitch chord, each pitch having a list of seqRefs.
        /// This constructor makes its own copy of the midiPitches but not the seqRefs.
        /// </summary>
        public InputChordDef(int msPosition, int msDuration, List<byte> notatedMidiPitches, List<List<SeqRef>> seqRefsPerMidiPitch, string lyric)
            : base(msDuration)
        {
            Debug.Assert(notatedMidiPitches.Count == seqRefsPerMidiPitch.Count);
            _msPosition = msPosition;
            _notatedMidiPitches = new List<byte>(notatedMidiPitches);
            _seqRefsPerMidiPitch = seqRefsPerMidiPitch;
            _lyric = lyric;
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
        /// Writes the logical content of this InputChordDef (see ChordSymbol and MidiChordDef)
        /// </summary>
        /// <param name="w"></param>
        public void WriteSvg(SvgWriter w)
        {
            w.WriteStartElement("score", "inputNotes", null);
            // N.B.: the lyric has been written as an attribute of the containing InputChordSymbol.

            for(int i = 0; i < _notatedMidiPitches.Count; ++i)
            {
                byte pitch = _notatedMidiPitches[i];
                w.WriteStartElement("inputNote");
                w.WriteAttributeString("notatedKey", pitch.ToString());
                
                List<SeqRef> seqRefs = this.SeqRefsPerMidiPitch[i];
                w.WriteStartElement("seqRefs");
                foreach(SeqRef seqRef in seqRefs)
                {
                    seqRef.WriteSvg(w);
                }
                w.WriteEndElement(); // score:seqRefs

                w.WriteEndElement(); // score:inputNote
            }

            w.WriteEndElement(); // score:inputNotes

        }

        public override IUniqueDef DeepClone()
        {
            throw new NotImplementedException("InputChordDef.DeepClone()");
        }

        public List<byte> SeqVoiceIDs
        {
            get
            {
                List<byte> seqVoiceIDs = new List<byte>();
                foreach(List<SeqRef> seqRefList in _seqRefsPerMidiPitch)
                {
                    foreach(SeqRef seqRef in seqRefList)
                    {
                        seqVoiceIDs.Add(seqRef.VoiceID);
                    }
                }
                return seqVoiceIDs;
            }
        }

        public int MsPosition { get { return _msPosition; } set { _msPosition = value; } }
        private int _msPosition = 0;

        public List<byte> NotatedMidiPitches { get { return _notatedMidiPitches; } set { _notatedMidiPitches = value; } }
        protected List<byte> _notatedMidiPitches = new List<byte>();

        public List<List<SeqRef>> SeqRefsPerMidiPitch { get { return _seqRefsPerMidiPitch; } }
        private List<List<SeqRef>> _seqRefsPerMidiPitch = new List<List<SeqRef>>();

        public string Lyric { get { return _lyric; } set { _lyric = value; } }
        private string _lyric;

        public int? MsDurationToNextBarline { get { return _msDurationToNextBarline; } set { _msDurationToNextBarline = value; } }
        private int? _msDurationToNextBarline = null;
    }
}
