using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Xml;

using Moritz.Globals;

namespace Moritz.Score.Notation
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

        public InputChordDef(XmlReader r, int msDuration)
            : base(msDuration)
        {
            // The reader is at the beginning of a "score:inputChord" element
            Debug.Assert(r.Name == "score:inputChord" && r.IsStartElement() && r.AttributeCount > 0);
            int nAttributes = r.AttributeCount;
            //for(int i = 0; i < nAttributes; i++)
            //{
                //r.MoveToAttribute(i);
                //switch(r.Name)
                //{
                //    case "id": // ids are ignored!
                //        break;
                //}
            //}
            //bool isStartElement = r.IsStartElement();
            //Debug.Assert(r.Name == "score.midiChord" && !(isStartElement));
        }

        /// <summary>
        /// Transpose by the number of semitones given in the argument.
        /// Negative interval values transpose down.
        /// It is not an error if Midi values would exceed the range 0..127.
        /// In this case, they are silently coerced to 0 or 127 respectively.
        /// </summary>
        public void Transpose(int interval)
        {
            for(int i = 0; i < _midiPitches.Count; ++i)
            {
                _midiPitches[i] = M.MidiValue(_midiPitches[i] + interval);
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
            w.WriteStartElement("score", "inputChord", null);

            // etc.

            w.WriteEndElement(); // score:inputChord
        }

        public override IUniqueDef DeepClone()
        {
            throw new NotImplementedException("InputChordDef.DeepClone()");
        }

        public int? MsDurationToNextBarline { get { return _msDurationToNextBarline; } set { _msDurationToNextBarline = value; } }
        private int? _msDurationToNextBarline = null;

        public int MsPosition { get { return _msPosition; } set { _msPosition = value; } }
        private int _msPosition = 0;

        public List<byte> MidiPitches { get { return _midiPitches; } set { _midiPitches = value; } }
        protected List<byte> _midiPitches = new List<byte>();

        public string Lyric { get { return _lyric; } set { _lyric = value; } }
        protected string _lyric = null;
    }
}
