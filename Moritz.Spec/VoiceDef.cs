
using Krystals5ObjectLibrary;

using Moritz.Globals;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Moritz.Spec
{
    /// <summary>
    /// <para>VoiceDef classes are IEnumerable (foreach loops can be used).</para>
    /// <para>They are also indexable (Trk trk = this[index])</para>
    /// </summary>
    public class VoiceDef : IEnumerable
    {
        #region constructorss
        /// <summary>
        /// The Trks must have the same number and types of contained UniqueDefs
        /// but they can have different durations and MIDI definitions 
        /// </summary>
        /// <param name="trks"></param>
        public VoiceDef(List<Trk> trks)
        {
            #region conditions
            // The Trks must have the same number and types of contained UniqueDefs
            // but they can have different durations and MIDI definitions
            List<IUniqueDef> iuds0 = trks[0].UniqueDefs;
            int uidCount = iuds0.Count;
            for(int i = 1; i < trks.Count; i++)
            {
                List<IUniqueDef> trkIUDs = trks[i].UniqueDefs;
                Debug.Assert(trkIUDs.Count == uidCount);
                for(int j = 0; j < trkIUDs.Count; j++)
                {
                    Debug.Assert((iuds0[j] is MidiChordDef && trkIUDs[j] is MidiChordDef)
                        || (iuds0[j] is RestDef && trkIUDs[j] is RestDef));
                }
            }
            #endregion

            Trks = trks;

            foreach(Trk trk in trks)
            {
                trk.SetMsPositionsReFirstUD();
            }

            this._msPositionReContainer = 0;

            AssertConsistency();
        }

        /// <summary>
        /// Returns two VoiceDefs (each having the same voice index)
        /// Item1.Trks[0] contains the IUniqueDefs that begin within the poppedMsDuration.
        /// Item2.Trks[0] contains the remaining IUniqueDefs from the original voiceDef.Trks plus possible CautionaryChordDefs.
        /// The remaining Trks in Item1 and Item2 are parallel IUniqueDefs (that can have other durations).
        /// The popped IUniqueDefs are removed from the current voiceDef before returning it as Item2.
        /// MidiRestDefs and MidiChordDefs are split as necessary to fit the required Trk[0] duration.
        /// </summary>
        /// <param name="voiceDef"></param>
        /// <param name="poppedBarMsDuration"></param>
        /// <returns></returns>
        public Tuple<VoiceDef, VoiceDef> PopVoiceDef(int poppedMsDuration)
        {
            Tuple<Trk, Trk> trks = Trks[0].PopTrk(poppedMsDuration);

            Trk poppedTrk0 = trks.Item1;
            Trk remainingTrk0 = trks.Item2;

            List<Trk> poppedTrks = new List<Trk> { poppedTrk0 };
            List<Trk> remainingTrks = new List<Trk> { remainingTrk0 };

            // The rest of this function should be reprogrammed, first copying corresponding items from each lower track...

            int nUniqueDefs = poppedTrk0.UniqueDefs.Count;
            List<Trk> voiceTrks = Trks;

            for(int trkIndex = 1; trkIndex < voiceTrks.Count; ++trkIndex)
            {
                Trk poppedTrk = new Trk();
                List<IUniqueDef> originalUids = voiceTrks[trkIndex].UniqueDefs;
                for(int uidIndex = 0; uidIndex < nUniqueDefs; ++uidIndex)
                {
                    poppedTrk.UniqueDefs.Add(originalUids[0]);
                    originalUids.RemoveAt(0);
                }
                var msDurationOffset = originalUids[0].MsPositionReFirstUD;
                foreach(var iud in originalUids)
                {
                    iud.MsPositionReFirstUD -= msDurationOffset;
                }
                poppedTrks.Add(poppedTrk);
                remainingTrks.Add(new Trk(originalUids));
            }

            VoiceDef poppedVoiceDef = new VoiceDef(poppedTrks);
            VoiceDef remainingVoiceDef = new VoiceDef(remainingTrks);

            return new Tuple<VoiceDef, VoiceDef>(poppedVoiceDef, remainingVoiceDef);
        }

        #endregion constructors
        public virtual void AssertConsistency()
        {
            Debug.Assert(MsPositionReContainer >= 0);
            Debug.Assert(Trks != null && Trks.Count > 0);
            foreach(var trk in Trks)
            {
                trk.AssertConsistency();
            }
        }

        /// <summary>
        /// Inserts a ClefDef at the given index (which must be greater than 0).
        /// <para>If a ClefDef is defined directly before a rest, the resulting SmallClef will be placed before the
        /// following Chord or the bar's end barline.
        /// </para>
        /// <para>If the index is equal to or greater than the number of objects in the voiceDef, the ClefDef will be
        /// placed before the final barline.
        /// </para>
        /// <para>
        /// When changing clefs more than once in the same VoiceDef, it is easier to get the indices right if
        /// they are added backwards.
        /// </para>
        /// </summary>
        /// <param name="index">Must be greater than 0</param>
        /// <param name="clefType">One of the following strings: "t", "t1", "t2", "t3", "b", "b1", "b2", "b3"</param>
        public void InsertClefDef(int trkIndex, int uidIindex, string clefType)
        {
            #region check args
            Debug.Assert(trkIndex < Trks.Count);
            Debug.Assert(uidIindex > 0, "Cannot insert a clef before the first chord or rest in the bar!");

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

            ClefDef clefDef = new ClefDef(clefType, 0);
            Trks[trkIndex].Insert(uidIindex, clefDef);
        }

        public void Insert(int trkIndex, int uidIndex, IUniqueDef iUniqueDef)
        {
            Debug.Assert(!(iUniqueDef.MsDuration > 0), "Cannot Insert IUniqueDefs that have msDuration inside a Bar.");

            Trk trk = Trks[trkIndex];
            trk.Insert(uidIndex, iUniqueDef);

            trk.SetMsPositionsReFirstUD();

            AssertConsistency();
        }

        #region public indexer & enumerator
        /// <summary>
        /// Indexer. Allows individual Trks to be accessed using array notation on the Trks.
        /// Automatically resets the MsPositions of all UniqueDefs in the list.
        /// e.g. iumdd = trk[3].
        /// </summary>
        public Trk this[int i]
        {
            get
            {
                if(i < 0 || i >= Trks.Count)
                {
                    Debug.Assert(false, "Index out of range");
                }
                return Trks[i];
            }
            set
            {
                if(i < 0 || i >= Trks.Count)
                {
                    Debug.Assert(false, "Index out of range");
                }

                Trks[i] = value;
                Trks[i].SetMsPositionsReFirstUD();
                AssertConsistency();
            }
        }

        #region Enumerators
        public IEnumerator GetEnumerator()
        {
            return new MyEnumerator(Trks);
        }
		// Add the missing MoveNext() method to the MyEnumerator class
		private class MyEnumerator : IEnumerator
		{
			public List<Trk> Trks;
			int position = -1;
			//constructor
			public MyEnumerator(List<Trk> trks)
			{
				Trks = trks;
			}
			private IEnumerator GetEnumerator()
			{
				return (IEnumerator)this;
			}

			//IEnumerator
			public void Reset()
			{
				position = -1;
			}
			//IEnumerator
			public object Current
			{
				get
				{
					try
					{
						return Trks[position];
					}
					catch(IndexOutOfRangeException)
					{
						Debug.Assert(false);
						return null;
					}
				}
			}
			// Add the missing MoveNext() method
			public bool MoveNext()
			{
				position++;
				return (position < Trks.Count);
			}
		}
        #endregion
        #endregion public indexer & enumerator

        #region Properties

        public Bar Container = null;

        /// <summary>
        /// The msPosition of the first note or rest in the VoiceDef re the start of the containing Seq or Bar.
        /// The msPositions of the IUniqueDefs in the Trks are re the first IUniqueDef in the list,
        /// so the first IUniqueDef.MsPositionReFirstUID is always 0;
        /// </summary>
        private int _msPositionReContainer = 0;
        public virtual int MsPositionReContainer
        {
            get
            {
                return _msPositionReContainer;
            }
            set
            {
                Debug.Assert(value >= 0);
                _msPositionReContainer = value;
            }
        }

        public List<Trk> Trks = new List<Trk>();

        #endregion Properties
    }
}
