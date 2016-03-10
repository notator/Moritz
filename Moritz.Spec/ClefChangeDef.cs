using System;
using System.Diagnostics;

using Krystals4ObjectLibrary;
using Moritz.Globals;

namespace Moritz.Spec
{
    ///<summary>
    /// A ClefChangeDef is a IUniqueDef which can be created while programming a score.
    /// It must be either be added to a voice.UniqueDefs list immediately before a MidiChordDef or RestDef with which it
    /// shares the same MsPosition, or appended to the voice.UniqueDefs list (in which case it has the EndMsPositionReTrk of
    /// the voiceDef (which does not contain barlines).
    /// When converting definitions to symbols, the Notator uses this class to ensure that both voices in
    /// a two-voice staff contain the same ClefSigns (see Notator.AddSymbolsToSystems(List<SvgSystem> systems)).
    /// (The ClefSigns may have different visibility in the two voices.)
    ///</summary>
    public class ClefChangeDef : IUniqueDef
    {
		public ClefChangeDef(string clefType, int msPositionReTrk)
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
			MsPositionReTrk = msPositionReTrk;
		}

		#region IUniqueDef
		public override string ToString()
        {
            return ("MsPositionReTrk=" + MsPositionReTrk.ToString() + " clefChange: type=" + _clefType + " ClefChangeDef");
        }

        public void AdjustMsDuration(double factor) {}

        public IUniqueDef Clone()
        {
            ClefChangeDef deepClone = new ClefChangeDef(_clefType, MsPositionReTrk);
            return deepClone;
        }

        public int MsDuration { get { return 0; } set { throw new System.NotSupportedException(); } }
		private int _msPositionReTrk = -1;
		/// <summary>
		/// Care should be taken to ensure that ClefChangeDefs always have the same msPosition as the following MidiChordDef or RestDef
		/// </summary>
        public int MsPositionReTrk
        {
            get
            {
                Debug.Assert(_msPositionReTrk >= 0);
                return _msPositionReTrk;
            }
            set
			{
				Debug.Assert(value >= 0);
				_msPositionReTrk = value;
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
