using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Xml;

using Multimedia.Midi;
using Moritz.Globals;

namespace Moritz.Score.Notation
{
    ///<summary>
    /// A UniqueInputChordDef is a deep clone of an InputChordDef.
    //</summary>
    public class UniqueInputChordDef : InputChordDef, IUniqueSplittableChordDef, IUniqueCloneDef
    {
        public UniqueInputChordDef()
            :base()
        {
            ID = "localChord" + UniqueChordID.ToString();
        }

        public UniqueInputChordDef(XmlReader r, string localID, int msDuration)
            : base()
        {
            // The reader is at the beginning of a "score:inputChord" element having an ID attribute
            Debug.Assert(r.Name == "score:inputChord" && r.IsStartElement() && r.AttributeCount > 0);
            int nAttributes = r.AttributeCount;
            //for(int i = 0; i < nAttributes; i++)
            //{
                //r.MoveToAttribute(i);
                //switch(r.Name)
                //{
                //    case "pitchWheelDeviation":
                //        _pitchWheelDeviation = byte.Parse(r.Value);
                //        break;
                //    case "minBasicChordMsDuration":
                //        this._minimumBasicMidiChordMsDuration = int.Parse(r.Value);
                //        break;
                //}
            //}
            //bool isStartElement = r.IsStartElement();
            //Debug.Assert(r.Name == "score.midiChord" && !(isStartElement));
        }

        /// <summary>
        /// A deep clone of the argument. This class is saved as an individual chordDef in SVG files,
        /// so it allows ALL its attributes to be set, even after construction.
        /// The argument may not be null.
        /// </summary>
        /// <param name="midiChordDef"></param>
        public UniqueInputChordDef(InputChordDef icd)
            :this()
        {
            Debug.Assert(icd != null);
            throw new NotImplementedException();
        }

        #region IUniqueSplittableChordDef

        public int? MsDurationToNextBarline { get { return _msDurationToNextBarline; } set { _msDurationToNextBarline = value; } }
        private int? _msDurationToNextBarline = null;

        #region IUniqueChordDef
        /// <summary>
        /// Transpose by the number of semitones given in the argument.
        /// Negative interval values transpose down.
        /// It is not an error if Midi values would exceed the range 0..127.
        /// In this case, they are silently coerced to 0 or 127 respectively.
        /// </summary>
        public void Transpose(int interval)
        {
            int value;
            for(int i = 0; i < _midiPitches.Count; ++i)
            {
                value = _midiPitches[i] + interval;
                value = (value > 127) ? 127 : value;
                value = (value < 0) ? 0 : value;
                _midiPitches[i] = (byte) value;
            }
        }

        // MidiPitches is defined in InputChordDef
        //public List<byte> MidiPitches { get { return _midiPitches; } set { _midiPitches = value; } }
        //private List<byte> _midiPitches = null;

        #region IUniqueDef
        public override string ToString()
        {
            return ("MsPosition=" + MsPosition.ToString() + " MsDuration=" + MsDuration.ToString() + " UniqueInputChordDef");
        }

        /// <summary>
        /// Multiplies the MsDuration by the given factor.
        /// </summary>
        /// <param name="factor"></param>
        public void AdjustDuration(double factor)
        {
            MsDuration = (int)(_msDuration * factor);
        }

        /// <summary>
        /// Writes the logical content of this UniqueInputChordDef (see ChordSymbol and UniqueMidiChordDef)
        /// </summary>
        /// <param name="w"></param>
        public void WriteSvg(SvgWriter w, string idNumber)
        {
            w.WriteStartElement("score", "inputChord", null);

            if(!String.IsNullOrEmpty(idNumber))
                w.WriteAttributeString("id", "inputChord" + idNumber);

            // etc.

            w.WriteEndElement(); // score:midiChord
        }

        public int MsDuration { get { return _msDuration; } set { _msDuration = value; } }
        private int _msDuration = 0;
        public int MsPosition { get { return _msPosition; } set { _msPosition = value; } }
        private int _msPosition = 0;

        #endregion IUniqueDef
        #endregion IUniqueChordDef
        #endregion IUniqueSplittableChordDef

    }
}
