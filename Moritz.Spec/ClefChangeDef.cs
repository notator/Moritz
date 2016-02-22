using System;
using System.Diagnostics;

using Krystals4ObjectLibrary;
using Moritz.Globals;

namespace Moritz.Spec
{
    ///<summary>
    /// A ClefChangeDef is a IUniqueDef which can be created while programming a score.
    /// It must be added to a voice.UniqueDefs list immediately before a MidiChordDef or
    /// RestDef. It shares the same MsPosition.
    /// When converting definitions to symbols, the Notator uses this class to ensure that both voices in
    /// a two-voice staff contain the same ClefSigns (see Notator.AddSymbolsToSystems(List<SvgSystem> systems)).
    /// (The ClefSigns may have different visibility in the two voices.)
    ///</summary>
    public class ClefChangeDef : IUniqueDef
    {
        /// <summary>
        /// The clefType must be one of the following strings "t", "t1", "t2", "t3", "b", "b1", "b2", "b3"
        /// The following IUniqueDef must be a MidiChordDef or RestDef.
        /// </summary>
        public ClefChangeDef(string clefType, IUniqueDef followingUniqueChordOrRestDef)
            :this(clefType, followingUniqueChordOrRestDef.MsPosition)
        {
            #region check args
            if(!(followingUniqueChordOrRestDef is MidiChordDef) 
            && !(followingUniqueChordOrRestDef is InputChordDef)
            && !(followingUniqueChordOrRestDef is RestDef))
            {
                Debug.Assert(false, "Clef change must be followed by a chord or rest.");
            }
            #endregion
        }

		private ClefChangeDef(string clefType, int msPosition)
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
			#endregion

			_id = "clefChange" + UniqueClefChangeIDNumber.ToString();
			_clefType = clefType;
			MsPosition = msPosition;
		}

		#region IUniqueDef
		public override string ToString()
        {
            return ("MsPosition=" + MsPosition.ToString() + " clefChange: type=" + _clefType + " ClefChangeDef");
        }

        public void AdjustMsDuration(double factor) {}

        public IUniqueDef Clone()
        {
            ClefChangeDef deepClone = new ClefChangeDef(_clefType, MsPosition);
            return deepClone;
        }

        public int MsDuration { get { return 0; } set { throw new System.NotSupportedException(); } }
		private int _msPosition = -1;
		/// <summary>
		/// Care should be taken to ensure that ClefChangeDefs always have the same msPosition as the following MidiChordDef or RestDef
		/// </summary>
        public int MsPosition
        {
            get
            {
                Debug.Assert(_msPosition >= 0);
                return _msPosition;
            }
            set
			{
				Debug.Assert(value >= 0);
				_msPosition = value;
			}
        }
        #endregion IUniqueDef

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
