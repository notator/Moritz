using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Xml;

using Moritz.Globals;
using Moritz.Xml;

namespace Moritz.VoiceDef
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
        public InputChordDef(int msPosition, int msDuration, byte midiPitch, byte seqChannelIndex, byte seqLength, string lyric)
            : base(msDuration)
        {
            _msPosition = msPosition;
            _midiPitches = new List<byte>(){midiPitch};
            List<byte> seqChannelIndices = new List<byte>(){seqChannelIndex};
            List<byte> seqLengths = new List<byte>(){seqLength};
            SeqChannelIndicesPerMidiPitch.Add(midiPitch, seqChannelIndices);
            SeqLengthsPerMidiPitch.Add(midiPitch, seqLengths);
            _lyric = lyric;
            _msDurationToNextBarline = null;
        }

        /// <summary>
        /// Constructs a multi-pitch chord, each pitch pitch pointing at several sequences.
        /// This constructor makes its own copy of the midiPitches
        /// </summary>
        public InputChordDef(int msPosition, int msDuration, List<byte> midiPitches,
            List<List<byte>> seqChannelIndicesPerMidiPitch,
            List<List<byte>> seqLengthsPerMidiPitch,
            string lyric)
            : base(msDuration)
        {
            Debug.Assert(midiPitches.Count == seqChannelIndicesPerMidiPitch.Count);
            Debug.Assert(midiPitches.Count == seqLengthsPerMidiPitch.Count);

            _msPosition = msPosition;
            _midiPitches = new List<byte>(midiPitches);
            for(int pitchIndex = 0; pitchIndex < midiPitches.Count; ++pitchIndex)
            {
                SeqChannelIndicesPerMidiPitch.Add(midiPitches[pitchIndex], seqChannelIndicesPerMidiPitch[pitchIndex]);
                SeqLengthsPerMidiPitch.Add(midiPitches[pitchIndex], seqLengthsPerMidiPitch[pitchIndex]);
            }
            _lyric = lyric;
            _msDurationToNextBarline = null;
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
            w.WriteStartElement("score", "inputNotes", null);

            foreach(byte pitch in _midiPitches)
            {
                w.WriteStartElement("score", "inputNote", null);
                w.WriteAttributeString("midiPitch", pitch.ToString());
                List<byte> seqChannelIndices = SeqChannelIndicesPerMidiPitch[pitch];
                w.WriteAttributeString("seqChannels", M.ByteListToString(seqChannelIndices));
                List<byte> seqLengths = SeqLengthsPerMidiPitch[pitch];
                w.WriteAttributeString("seqLengths", M.ByteListToString(seqLengths));
                w.WriteEndElement(); // score:inputNote
            }

            w.WriteEndElement(); // score:inputNotes

        }

        public override IUniqueDef DeepClone()
        {
            throw new NotImplementedException("InputChordDef.DeepClone()");
        }

        /// <summary>
        /// All the channel indices of Seqs that can be started by this InputChordDef.
        /// The returned list is never null, but can be empty.
        /// </summary>
        public List<byte> SeqChannelIndices
        {
            get
            {
                List<byte> rval = new List<byte>();
                foreach(byte pitch in MidiPitches)
                {
                    List<byte> pitchOutputVoiceIndices = SeqChannelIndicesPerMidiPitch[pitch];
                    foreach(byte ovIndex in pitchOutputVoiceIndices)
                    {
                        rval.Add(ovIndex);
                    }
                }
                return rval;
            }
        }

        /// <summary>
        /// All the Seq lengths of Seqs that can be started by this InputChordDef.
        /// The returned list is never null, but can be empty.
        /// </summary>
        public List<byte> SeqLengths
        {
            get
            {
                List<byte> rval = new List<byte>();
                foreach(byte pitch in MidiPitches)
                {
                    List<byte> pitchSeqLengths = this.SeqLengthsPerMidiPitch[pitch];
                    foreach(byte len in pitchSeqLengths)
                    {
                        rval.Add(len);
                    }
                }
                return rval;
            }
        }

        /// <summary>
        /// Returns all the MidiChordDefs that are the beginnings of Seqs associated with this inputChordDef.
        /// An exception is thrown if this InputChordDef is not in the given bar.
        /// The list will be empty if there are no such MidiChordDefs.
        /// </summary>
        public List<MidiChordDef> SeqStarts(List<VoiceDef> bar)
        {
            CheckThisIsInBar(bar);

            List<MidiChordDef> rval = new List<MidiChordDef>();
            List<byte> outputVoiceIndices = this.SeqChannelIndices;

            foreach(byte outputVoiceIndex in outputVoiceIndices)
            {
                OutputVoiceDef ov = bar[outputVoiceIndex] as OutputVoiceDef;
                Debug.Assert(ov != null);
                foreach(MidiChordDef mcd in ov.MidiChordDefs)
                {
                    if(mcd.MsPosition == this.MsPosition)
                    {
                        rval.Add(mcd);
                    }
                }
            }

            return rval;
        }

        private void CheckThisIsInBar(List<VoiceDef> bar)
        {
            bool found = false;
            foreach(VoiceDef v in bar)
            {
                InputVoiceDef iv = v as InputVoiceDef;
                if(iv != null)
                {
                    foreach(InputChordDef icd in iv.InputChordDefs)
                    {
                        if(this == icd)
                        {
                            found = true;
                            break;
                        }
                    }
                }
                if(found)
                    break;
            }
            if(!found)
            {
                throw new ApplicationException("InputChordDef Error: inputChordDef not found in inputVoice.");
            }
        }

        public int MsPosition { get { return _msPosition; } set { _msPosition = value; } }
        private int _msPosition = 0;

        public List<byte> MidiPitches { get { return _midiPitches; } set { _midiPitches = value; } }
        protected List<byte> _midiPitches = new List<byte>();

        public string Lyric { get { return _lyric; } set { _lyric = value; } }
        protected string _lyric = null;

        public int? MsDurationToNextBarline { get { return _msDurationToNextBarline; } set { _msDurationToNextBarline = value; } }
        private int? _msDurationToNextBarline = null;

        public Dictionary<byte, List<byte>> SeqChannelIndicesPerMidiPitch = new Dictionary<byte, List<byte>>();
        public Dictionary<byte, List<byte>> SeqLengthsPerMidiPitch = new Dictionary<byte, List<byte>>();
    }
}
