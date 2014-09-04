using System;
using System.Diagnostics;
using System.Collections.Generic;

using Multimedia.Midi;
using Moritz.Globals;
using Moritz.Score.Midi;

namespace Moritz.Score.Notation
{
    ///<summary>
    /// A UniqueClefChangeDef is a IUniqueMidiDurationDef which can be created while programming a score.
    /// It must be added to a voice.UniqueMidiDurationDefs list immediately before a MidiChordDef or
    /// UniqueMidiRestDef. It shares the same MsPosition.
    /// When converting definitions to symbols, the Notator uses this class to ensure that both voices in
    /// a two-voice staff contain the same ClefSigns (see Notator.AddSymbolsToSystems(List<SvgSystem> systems)).
    /// (The ClefSigns may have different visibility in the two voices.)
    ///</summary>
    public class UniqueClefChangeDef : IUniqueDef
    {
        /// <summary>
        /// The clefType must be one of the following strings "t", "t1", "t2", "t3", "b", "b1", "b2", "b3"
        /// The followingUniqueMidiChordOrRestDef must be a MidiChordDef or UniqueMidiRestDef.
        /// </summary>
        public UniqueClefChangeDef(string clefType, IUniqueDef followingUniqueChordOrRestDef)
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

            if(!(followingUniqueChordOrRestDef is MidiChordDef) 
            && !(followingUniqueChordOrRestDef is InputChordDef)
            && !(followingUniqueChordOrRestDef is RestDef))
            {
                Debug.Assert(false, "Clef change must be followed by a chord or rest.");
            }
            #endregion

            _id = "clefChange" + UniqueClefChangeIDNumber.ToString();
            _clefType = clefType;
            _followingChordOrRestDef = followingUniqueChordOrRestDef;
        }

        #region IUniqueDef
        public override string ToString()
        {
            return ("MsPosition=" + MsPosition.ToString() + " clefChange: type=" + _clefType + " UniqueClefChangeDef");
        }

        public void AdjustDuration(double factor) {}

        public int MsDuration { get { return 0; } set { } }
        public int MsPosition
        {
            get
            {
                Debug.Assert(_followingChordOrRestDef != null);
                return _followingChordOrRestDef.MsPosition;
            }
            set { }
        }
        #endregion IUniqueDef

        private IUniqueDef _followingChordOrRestDef;
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
