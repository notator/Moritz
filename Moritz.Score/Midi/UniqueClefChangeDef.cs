using System;
using System.Diagnostics;
using System.Collections.Generic;

using Multimedia.Midi;
using Moritz.Globals;

namespace Moritz.Score.Midi
{
    ///<summary>
    /// A UniqueClefChangeDef is a IUniqueMidiDurationDef which can be created while programming a score.
    /// It must be added to a voice.UniqueMidiDurationDefs list immediately before a UniqueMidiChordDef or
    /// UniqueMidiRestDef. It shares the same MsPosition.
    /// When converting definitions to symbols, the Notator uses this class to ensure that both voices in
    /// a two-voice staff contain the same ClefSigns (see Notator.AddSymbolsToSystems(List<SvgSystem> systems)).
    /// (The ClefSigns may have different visibility in the two voices.)
    ///</summary>
    public class UniqueClefChangeDef : IUniqueMidiDurationDef
    {
        /// <summary>
        /// The clefType must be one of the following strings "t", "t1", "t2", "t3", "b", "b1", "b2", "b3"
        /// The followingUniqueMidiChordOrRestDef must be a UniqueMidiChordDef or UniqueMidiRestDef.
        /// </summary>
        public UniqueClefChangeDef(string clefType, IUniqueMidiDurationDef followingUniqueMidiChordOrRestDef)
            :base()
        {
            #region check args
            if(String.Equals(clefType, "t") == false
            && String.Equals(clefType, "t1") == false
            && String.Equals(clefType, "t2") == false
            && String.Equals(clefType, "t3") == false
            && String.Equals(clefType, "b") == false
            && String.Equals(clefType, "b1") == false
            && String.Equals(clefType, "b2") == false
            && String.Equals(clefType, "b3") == false)
            {
                Debug.Assert(false, "Unknown clef type.");
            }

            if(!(followingUniqueMidiChordOrRestDef is UniqueMidiChordDef) && !(followingUniqueMidiChordOrRestDef is UniqueMidiRestDef))
            {
                Debug.Assert(false, "Clef change must be followed by a chord or rest.");
            }
            #endregion

            _id = "clefChange" + UniqueClefChangeIDNumber.ToString();
            _clefType = clefType;
            _followingUniqueMidiChordOrRestDef = followingUniqueMidiChordOrRestDef;
        }

        #region IUniqueMidiDurationDef
        public override string ToString()
        {
            return ("MsPosition=" + MsPosition.ToString() + " clefChange: type=" + _clefType);
        }
        public void Transpose(int interval) { }
        public void AdjustDuration(double factor){ }
        public void AdjustVelocities(double factor) { }
        public void AdjustExpression(double factor) { }
        public void AdjustModulationWheel(double factor) { }
        public void AdjustPitchWheel(double factor) { }
        public byte MidiVelocity { get { return 0; } set { } }
        public int? PitchWheelDeviation { get { return null; } set { } }
        public List<byte> PanMsbs { get { return new List<byte>(); } set { } }

        /// <summary>
        /// This field is set/used by chords when they cross a barline.
        /// Rests and Clefs never cross barlines, so they should never attempt to set this property.
        /// </summary>
        public int? MsDurationToNextBarline
        {
            get { return null; }
            set
            {
                Debug.Assert(false, "Clefs never cross barlines, so this field should never be set."); 
            }
        }
        public int MsDuration { get { return 0; } set { } }
        public int MsPosition
        {
            get
            {
                Debug.Assert(_followingUniqueMidiChordOrRestDef != null);
                return _followingUniqueMidiChordOrRestDef.MsPosition;
            }
            set { }
        }
        #endregion

        private IUniqueMidiDurationDef _followingUniqueMidiChordOrRestDef;
        public string ID { get { return _id; } }
        private string _id;

        /// <summary>
        /// One of the following strings "t", "t1", "t2", "t3", "b", "b1", "b2", "b3"
        /// </summary>
        public string ClefType { get { return _clefType; } }
        private string _clefType;

        private int UniqueClefChangeIDNumber { get { return ++_uniqueClefChangeIDNumber; } }
        private static int _uniqueClefChangeIDNumber = 0;
    }
}
