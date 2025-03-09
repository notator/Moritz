using Krystals5ObjectLibrary;

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Moritz.Spec
{
    public class Bar : IChannelDefsContainer
    {
        public Bar()
        {
        }

        /// <summary>
        /// All constructors in this class are protected, so Bars can only be created by subclasses.
        /// <para>A Bar contains a list of ChannelDef. Each ChannelDef contains a list of Trk objects.
        /// Bars do not contain barlines. They are implicit, at the beginning and end of the Bar. 
        /// This constructor uses its arguments' channelDefs directly, so, if the arguments need to be used again, pass a clone.</para>
        /// <para>Seq.AssertConsistency and all inputVoiceDef.AssertConsistency functions must succeed (see their definitions).</para>
        /// <para>The Bar's AbsMsPosition is set to the seq's AbsMsPosition. If initialClefPerChannel is not null, the initial ClefDef is
        /// inserted at the beginning of each voice.</para>
        /// <para>When complete, this constructor calls the bar.AssertConsistency() function (see that its documentation).</para>
        /// </summary>
        /// <param name="seq">Cannot be null, and must have Trks</param>
        /// <param name="inputVoiceDefs">This list can be null or empty</param>
        protected Bar(Seq seq)
        {
            #region conditions
            seq.AssertConsistency();
            #endregion

            AbsMsPosition = seq.AbsMsPosition;

            int clefIndex = 0;
            int msDuration = seq.MsDuration;
            foreach(var channelDef in seq.ChannelDefs)
            {
                channelDef.Container = this;
                Debug.Assert(channelDef.MsDuration == msDuration); // cannot be 0 here.
                _voiceDefs.Add(channelDef);
                clefIndex++;
            }

            AssertConsistency();
        }

        public void Concat(Bar bar2)
        {
            Debug.Assert(_voiceDefs.Count == bar2._voiceDefs.Count);

            for(int i = 0; i < _voiceDefs.Count; ++i)
            {
                ChannelDef vd1 = _voiceDefs[i];
                ChannelDef vd2 = bar2._voiceDefs[i];

                vd1.Container = null;
                vd2.Container = null;
                vd1.AddRange(vd2);
                vd1.RemoveDuplicateClefDefs();
                vd1.AgglomerateRests();
                vd1.Container = this;
            }

            AssertConsistency();
        }


        /// <summary>
        /// Trk.AssertConsistency() is called on each Trk in each ChannelDef.
        /// Then the following checks ae also made:
        /// <para>All channelDefs have the same MsDuration.</para>
        /// <para>At least one Trk must start with a MidiChordDef, possibly preceded by a ClefDef.</para>
        /// </summary> 
        public virtual void AssertConsistency()
        {
            #region trk consistent in bar
            foreach(ChannelDef channelDef in _voiceDefs)
            {
                channelDef.AssertConsistency();
                foreach(Trk trk in channelDef.Trks)
                {
                    trk.AssertConsistency();
                }
            }
            #endregion

            int barMsDuration = MsDuration;


            #region All channelDefs have the same MsDuration.
            foreach(ChannelDef channelDef in _voiceDefs)
            {
                Debug.Assert(channelDef.MsDuration == barMsDuration, "All ChannelDefs in a bar must have the same duration.");
            }
            #endregion

            #region 3. At least one Trk must start with a MidiChordDef, possibly preceded by a ClefDef.
            IReadOnlyList<Trk> trks = Trks;
            bool startFound = false;
            foreach(Trk trk in trks)
            {
                List<IUniqueDef> iuds = trk.UniqueDefs;
                IUniqueDef firstIud = iuds[0];
                if(firstIud is MidiChordDef || (iuds.Count > 1 && firstIud is ClefDef && iuds[1] is MidiChordDef))
                {
                    startFound = true;
                    break;
                }
            }
            Debug.Assert(startFound, "MidiChordDef not found at start.");
            #endregion
        }

        #region envelopes
        /// <summary>
        /// This function does not change the MsDuration of the Bar.
        /// See Envelope.TimeWarp() for a description of the arguments.
        /// </summary>
        /// <param name="envelope"></param>
        /// <param name="distortion"></param>
        public void TimeWarp(Envelope envelope, double distortion)
        {
            AssertConsistency();
            int originalMsDuration = MsDuration;
            List<int> originalMsPositions = GetMsPositions();
            Dictionary<int, int> warpDict = new Dictionary<int, int>();
            #region get warpDict
            List<int> newMsPositions = envelope.TimeWarp(originalMsPositions, distortion);

            for(int i = 0; i < newMsPositions.Count; ++i)
            {
                warpDict.Add(originalMsPositions[i], newMsPositions[i]);
            }
            #endregion get warpDict

            foreach(ChannelDef channelDef in _voiceDefs)
            {
                List<IUniqueDef> iuds = channelDef.UniqueDefs;
                IUniqueDef iud = null;
                int msPos = 0;
                for(int i = 1; i < iuds.Count; ++i)
                {
                    iud = iuds[i - 1];
                    msPos = warpDict[iud.MsPositionReFirstUD];
                    iud.MsPositionReFirstUD = msPos;
                    iud.MsDuration = warpDict[iuds[i].MsPositionReFirstUD] - msPos;
                    msPos += iud.MsDuration;
                }
                iud = iuds[iuds.Count - 1];
                iud.MsPositionReFirstUD = msPos;
                iud.MsDuration = originalMsDuration - msPos;
            }

            Debug.Assert(originalMsDuration == MsDuration);

            AssertConsistency();
        }

        /// <summary>
        /// Returns a list containing the msPositions of all IUniqueDefs plus the endMsPosition of the final object.
        /// </summary>
        private List<int> GetMsPositions()
        {
            int originalMsDuration = MsDuration;

            List<int> originalMsPositions = new List<int>();
            foreach(ChannelDef channelDef in _voiceDefs)
            {
                foreach(IUniqueDef iud in channelDef)
                {
                    int msPos = iud.MsPositionReFirstUD;
                    if(!originalMsPositions.Contains(msPos))
                    {
                        originalMsPositions.Add(msPos);
                    }
                }
                originalMsPositions.Sort();
            }
            originalMsPositions.Add(originalMsDuration);
            return originalMsPositions;
        }

        public void SetPitchWheelSliders(Envelope envelope)
        {
            #region condition
            if(envelope.Domain != 127)
            {
                throw new ArgumentException($"{nameof(envelope.Domain)} must be 127.");
            }
            #endregion condition

            List<int> msPositions = GetMsPositions();
            Dictionary<int, int> pitchWheelValuesPerMsPosition = envelope.GetValuePerMsPosition(msPositions);

            foreach(ChannelDef channelDef in _voiceDefs)
            {
                foreach(var trk in channelDef.Trks)
                {
                    trk.SetPitchWheelSliders(pitchWheelValuesPerMsPosition);
                }
            }
        }

        #endregion envelopes

        public void SetMsPositionsReFirstUD()
        {
            foreach(ChannelDef channelDef in _voiceDefs)
            {
                int msPosition = 0;
                foreach(IUniqueDef iud in channelDef)
                {
                    iud.MsPositionReFirstUD = msPosition;
                    msPosition += iud.MsDuration;
                }
            }
        }

        /// <summary>
        /// Setting this value stretches or compresses the msDurations of all the channelDefs and their contained UniqueDefs.
        /// </summary>
        public int MsDuration
        {
            get
            {
                return _voiceDefs[0].MsDuration;
            }
            set
            {
                AssertConsistency();
                int currentDuration = MsDuration;
                double factor = ((double)value) / currentDuration;
                foreach(ChannelDef channelDef in _voiceDefs)
                {
                    channelDef.MsDuration = (int)Math.Round(channelDef.MsDuration * factor);
                }
                int roundingError = value - MsDuration;
                if(roundingError != 0)
                {
                    foreach(ChannelDef channelDef in _voiceDefs)
                    {
                        if((channelDef.EndMsPositionReFirstIUD + roundingError) == value)
                        {
                            channelDef.EndMsPositionReFirstIUD += roundingError;
                        }
                    }
                }
                foreach(ChannelDef channelDef in _voiceDefs)
                {
                    Debug.Assert(channelDef.MsDuration == value);
                }
            }
        }

        public int AbsMsPosition
        {
            get { return _absMsPosition; }
            set
            {
                Debug.Assert(value >= 0);
                _absMsPosition = value;
            }
        }

        private int _absMsPosition = 0;

        /// <summary>
        /// returns _all_ the trks in the Bar as a flat list.
        /// </summary>
        public IReadOnlyList<Trk> Trks
        {
            get
            {
                List<Trk> trks = new List<Trk>();
                foreach(ChannelDef channelDef in _voiceDefs)
                {
                    foreach(var trk in channelDef.Trks)
                    {
                        trks.Add(trk);
                    }
                }
                return trks.AsReadOnly();
            }
        }

        public override string ToString()
        {
            return $"AbsMsPosition={AbsMsPosition}, MsDuration={MsDuration}, nVoiceDefs={ChannelDefs.Count}";
        }

        public IReadOnlyList<ChannelDef> ChannelDefs
        {
            get => _voiceDefs;
            //set
            //{
            //    _voiceDefs = value;
            //    AssertConsistency();
            //}
        }
        private List<ChannelDef> _voiceDefs = new List<ChannelDef>();
    }
}
