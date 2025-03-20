
using Krystals5ObjectLibrary;

using Moritz.Globals;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Moritz.Spec
{
    /// <summary>
    /// <para>ChannelDef classes are IEnumerable (foreach loops can be used).</para>
    /// <para>They are also indexable (Trk trk = this[index])</para>
    /// </summary>
    public class ChannelDef : IEnumerable
    {
        #region constructorss
        /// <summary>
        /// The Trks must have the same number and types of contained UniqueDefs
        /// but they can have different durations and MIDI definitions 
        /// </summary>
        /// <param name="trks"></param>
        public ChannelDef(List<Trk> trks)
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
                        || (iuds0[j] is MidiRestDef && trkIUDs[j] is MidiRestDef));
                }
            }
            #endregion

            Trks = trks;

            foreach(Trk trk in trks)
            {
                trk.ChannelDefsContainer = (IChannelDefsContainer)this;
                trk.SetMsPositionsReFirstUD();
            }

            this._msPositionReContainer = 0;

            AssertConsistency();
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

        public IChannelDefsContainer Container = null;

        /// <summary>
        /// The msPosition of the first note or rest in the ChannelDef re the start of the containing Seq or Bar.
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
