using System;
using System.Collections.Generic;
using System.Xml;
using System.Diagnostics;

using Moritz.Globals;

namespace Moritz.Score.Notation
{
    public class InputChordDef : DurationDef
    {
        /// <summary>
        /// Constructor used while constructing derived types.
        /// The derived types are responsible for initializing all the fields correctly.
        /// </summary>
        public InputChordDef()
            : base(0)
        {
        }

        #region Constructor used when reading an SVG file
        /// <summary>
        /// Contains values retrieved from an SVG file score:inputChord element
        /// Note that InputChordDefs do not have msPosition and msDuration attributes.
        /// These attributes are provided by embedding a clone of the InputChordDef in a UniqueInputChordDef.
        /// </summary>
        public InputChordDef(XmlReader r, string localID, int msDuration)
            : base(msDuration)
        {
            // The reader is at the beginning of a "score:inputChord" element having an ID attribute
            Debug.Assert(r.Name == "score:inputChord" && r.IsStartElement() && r.AttributeCount > 0);
            int nAttributes = r.AttributeCount;
            for(int i = 0; i < nAttributes; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "id":
                        if(localID != null)
                            ID = localID; // this is the local id in the score
                        else
                            ID = r.Value; // this is the id in the palleteDefs
                        break;
                    //case "repeat":
                    //    // repeat is false if this attribute is not present
                    //    byte rmVal = byte.Parse(r.Value);
                    //    if(rmVal == 0)
                    //        _repeat = false;
                    //    else
                    //        _repeat = true;
                    //    break;
                    //case "hasChordOff":
                    //    // hasChordOff is true if this attribute is not present
                    //    byte hcoVal = byte.Parse(r.Value);
                    //    if(hcoVal == 0)
                    //        _hasChordOff = false;
                    //    else
                    //        _hasChordOff = true;
                    //    break;
                    //case "bank":
                    //    _bank = byte.Parse(r.Value);
                    //    break;
                    //case "patch":
                    //    _patch = byte.Parse(r.Value);
                    //    break;
                    //case "volume":
                    //    _volume = byte.Parse(r.Value);
                    //    break;
                    //case "pitchWheelDeviation":
                    //    _pitchWheelDeviation = byte.Parse(r.Value);
                    //    break;
                    //case "minBasicChordMsDuration":
                    //    this._minimumBasicMidiChordMsDuration = int.Parse(r.Value);
                    //    break;
                }
            }
        }

        #endregion

        public void SetDuration(int msDuration)
        {
            MsDuration = msDuration;
        }

        public override IUniqueDef DeepClone()
        {
            UniqueInputChordDef uicd = new UniqueInputChordDef(this); // a deep clone with a special id string.
            uicd.MsPosition = 0;
            uicd.MsDuration = this.MsDuration;
            return uicd;
        }

        private static int _uniqueChordID = 0;
        public static int UniqueChordID { get { return ++_uniqueChordID; } }

        public string ID;

        public List<byte> MidiPitches { get { return _midiPitches; } set { _midiPitches = value; } }
        protected List<byte> _midiPitches = new List<byte>();

        public string Lyric { get { return _lyric; } set { _lyric = value; } }
        protected string _lyric = null;
    }
}
