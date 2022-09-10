using Moritz.Globals;
using Moritz.Xml;

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Moritz.Spec
{
    ///<summary>
    /// A InputChordDef can be saved and retrieved from voices in an SVG file.
    ///</summary>
    public class InputChordDef : DurationDef, IUniqueSplittableChordDef
    {
        /// <summary>
        /// Constructs a multi-note chord, each inputNoteDef has a notated pitch and a SeqDef.
		/// The inputNoteDefs must be in order of their notated pitches (bottom to top). 
        /// </summary>
        public InputChordDef(int msPositionReFirstIUD, int msDuration, List<InputNoteDef> inputNoteDefs, M.Dynamic dynamic, TrkOptions trkOptions)
            : base(msDuration)
        {
            #region check notated pitches and trkRef positions
            int pitchBelow = -1;
            foreach(InputNoteDef ind in inputNoteDefs)
            {
                Debug.Assert(ind.NotatedMidiPitch > pitchBelow);
                pitchBelow = ind.NotatedMidiPitch;

                if(ind.NoteOn != null && ind.NoteOn.SeqRef != null)
                {
                    foreach(TrkRef trk in ind.NoteOn.SeqRef)
                    {
                        Debug.Assert(msPositionReFirstIUD <= trk.MsPosition);
                    }
                }
                if(ind.NoteOff != null && ind.NoteOff.SeqRef != null)
                {
                    int minSeqPos = msPositionReFirstIUD + msDuration;
                    foreach(TrkRef trk in ind.NoteOff.SeqRef)
                    {
                        Debug.Assert(minSeqPos <= trk.MsPosition);
                    }
                }
                // Note that there is no corresponding check for ind.NoteOnTrkOffs and ind.NoteOffTrkOffs
            }
            #endregion

            _msPositionReFirstIUD = msPositionReFirstIUD;
            _msDuration = msDuration;
            _inputNoteDefs = inputNoteDefs;
            _lyric = null;
            _dynamic = (dynamic == M.Dynamic.none) ? null : M.CLichtDynamicsCharacters[dynamic];
            _trkOptions = trkOptions;
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
            for(int i = 0; i < _inputNoteDefs.Count; ++i)
            {
                _inputNoteDefs[i].NotatedMidiPitch = M.MidiValue(_inputNoteDefs[i].NotatedMidiPitch + interval);
            }
        }

        public override string ToString()
        {
            return ("InputChordDef: MsPositionReFirstIUD=" + MsPositionReFirstUD.ToString() + " MsDuration=" + MsDuration.ToString());
        }

        /// <summary>
        /// Multiplies the MsDuration by the given factor.
        /// </summary>
        /// <param name="factor"></param>
        public void AdjustMsDuration(double factor)
        {
            MsDuration = (int)(_msDuration * factor);
            Debug.Assert(MsDuration > 0, "A UniqueDef's MsDuration may not be set to zero!");
        }

        /// <summary>
        /// Writes the logical content of this InputChordDef
        /// </summary>
        /// <param name="w"></param>
        public void WriteSVG(SvgWriter w)
        {
            // we are inside a score:inputChord element
            if(_ccSettings != null)
            {
                _ccSettings.WriteSVG(w);
            }

            if(_trkOptions != null)
            {
                _trkOptions.WriteSVG(w, true);
            }

            w.WriteStartElement("score", "inputNotes", null);

            foreach(InputNoteDef ind in _inputNoteDefs)
            {
                ind.WriteSVG(w);
            }
            w.WriteEndElement(); // score:inputNotes
        }

        public override object Clone()
        {
            throw new NotImplementedException("InputChordDef.DeepClone()");
        }

        public int MsPositionReFirstUD { get { return _msPositionReFirstIUD; } set { _msPositionReFirstIUD = value; } }
        private int _msPositionReFirstIUD = 0;

        public string Lyric { get { return _lyric; } set { _lyric = value; } }
        private string _lyric = null;

        public string Dynamic { get { return _dynamic; } set { _dynamic = value; } }
        private string _dynamic = null;

        public TrkOptions TrkOptions { get { return _trkOptions; } set { _trkOptions = value; } }
        private TrkOptions _trkOptions = null;

        public CCSettings CCSettings { get { return _ccSettings; } set { _ccSettings = value; } }
        private CCSettings _ccSettings = null;

        public List<InputNoteDef> InputNoteDefs { get { return _inputNoteDefs; } }
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
            // The new pitches must be in ascending order
            set
            {
                List<byte> newPitches = value;
                Debug.Assert(newPitches.Count == _inputNoteDefs.Count);
                int pitchBelow = -1;
                for(int i = 0; i < newPitches.Count; ++i)
                {
                    byte newPitch = newPitches[i];
                    Debug.Assert(newPitch > pitchBelow);
                    pitchBelow = newPitch;
                    _inputNoteDefs[i].NotatedMidiPitch = newPitch;
                }
            }
        }

        public List<byte> NotatedMidiVelocities
        {
            get
            {
                Debug.Assert(false, "Input Chords do not have notated Midi velocities.");
                return null;
            }
            set
            {
                Debug.Assert(false, "Input Chords do not have notated Midi velocities.");
            }
        }

        public int? MsDurationToNextBarline { get { return _msDurationToNextBarline; } set { _msDurationToNextBarline = value; } }
        private int? _msDurationToNextBarline = null;

        public bool BeamContinues { get { return _beamContinues; } set { _beamContinues = value; } }
        private bool _beamContinues = true;
    }
}
