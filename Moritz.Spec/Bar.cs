using Krystals5ObjectLibrary;

using Moritz.Globals;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.XPath;

namespace Moritz.Spec
{
    public class Bar
    {
        /// <summary>
        /// This constructor creates the VoiceDefs field containing a list of VoiceDef objects.
        /// Each VoiceDef has a unique midi channel that is its index in the VoiceDefs list.
        /// Each VoiceDef contains a list of Trk. Each such Trk has the same channel, and is an alternative performance
        /// of the graphics that will eventually be associated with the VoiceDef's graphics. 
        /// </summary>
        /// <param name="absMsPosition">Must be greater than or equal to zero.</param>
        /// <param name="trks">There is one Trk per channel (A Trk's channel is the index in this list.).
        /// Each Trk must have a constructed UniqueDefs list which is either empty, or contains any combination of MidiChordDef or RestDef.
        /// Each trk.MsPositionReContainer must be 0. All trk.UniqueDef.MsPositionReFirstUD values must be set correctly.
        /// each MidiChordDef and RestDef has a MidiDef property containing a list of the alternative interpretations of the symbol. 
        public Bar(int absMsPosition, List<Trk> trks)
        {
            #region conditions
            Debug.Assert(absMsPosition >= 0);
            Debug.Assert(trks != null && trks.Count > 0 && trks.Count <= 16);
            foreach(var trk in trks)
            {
                trk.AssertConsistency();
            }
            #endregion conditions

            AbsMsPosition = absMsPosition;
            Trks = trks;

            AssertConsistency();
        }

        //public Bar Clone()
        //{
        //    var newTrks = new List<Trk>();

        //    foreach(var oldMidiVoiceDef in this.Trks)
        //    {
        //        List<Trk> newTrks = new List<Trk>();
        //        foreach(var oldTrk in oldMidiVoiceDef.Trks)
        //        {
        //            newTrks.Add((Trk)oldTrk.Clone());
        //        }
        //        newTrks.Add(new VoiceDef(newTrks));
        //    }

        //    Bar clone = new Bar(_absMsPosition, newTrks);

        //    return clone;
        //}

        #region old Bar

        public void Concat(Bar bar2)
        {
            Debug.Assert(Trks.Count == bar2.Trks.Count);

            for(int i = 0; i < Trks.Count; ++i)
            {
                    var trk1 = Trks[i];
                    var trk2 = bar2.Trks[i];    

                    trk1.AddRange(trk2);
                    trk1.RemoveDuplicateClefDefs();
                    trk1.AgglomerateRests();
            }

            AssertConsistency();
        }

        #region envelopes
        /// <summary>
        /// Warps the durations of a particular interpretation of the Bar without
        /// changing its overall MsDuration.
        /// See Envelope.TimeWarp() for a description of the arguments.
        /// </summary>
        /// <param name="envelope"></param>
        /// <param name="distortion"></param>
        public void TimeWarp(int interpretationIndex, Envelope envelope, double distortion)
        {
            AssertConsistency();
            for(int trkIndex = 0; trkIndex < Trks.Count; trkIndex++)
            {
                var trk = GetInterpretation(trkIndex, interpretationIndex);
                int originalMsDuration = trk.MsDuration;
                List<int> originalMsPositions = trk.GetMsPositions();
                Dictionary<int, int> warpDict = new Dictionary<int, int>();
                #region get warpDict
                List<int> newMsPositions = envelope.TimeWarp(originalMsPositions, distortion);
                for(int i = 0; i < newMsPositions.Count; ++i)
                {
                    warpDict.Add(originalMsPositions[i], newMsPositions[i]);
                }
                #endregion get warpDict
                List<IUniqueDef> iuds = trk.UniqueDefs;
                IUniqueDef iud;
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

                Debug.Assert(originalMsDuration == trk.MsDuration);
            }
            AssertConsistency();
        }

        /// <summary>
        /// Returns a flat list containing the unique msPositions of the IUniqueDefs of the main
        /// interpretation of in all Trks in the Bar, plus the endMsPosition of the final object.
        /// </summary>
        private List<int> GetAllMsPositions()
        {
            List<int> originalMsPositions = new List<int>();
            int originalMsDuration = MsDuration;

            foreach(var trk in Trks)
            {                                 
                foreach(IUniqueDef iud in trk.UniqueDefs)
                {
                    int msPos = iud.MsPositionReFirstUD;
                    if(!originalMsPositions.Contains(msPos))
                    {
                        originalMsPositions.Add(msPos);
                    }
                }
                originalMsPositions.Add(originalMsDuration);
            }
            originalMsPositions.Sort();
            return originalMsPositions;
        }

        #endregion envelopes

        public void SetMsPositionsReFirstUD()
        {
            foreach(Trk trk in Trks)
            {
                    int msPosition = 0;
                    foreach(IUniqueDef iud in trk.UniqueDefs)
                    {
                        iud.MsPositionReFirstUD = msPosition;
                        msPosition += iud.MsDuration;
                    }
            }
        }

        public override string ToString()
        {
            return $"AbsMsPosition={AbsMsPosition}, nTrks={Trks.Count}, nInterpretations={Trks[0].InterpretationsCount}";
        }

        public int AbsMsPosition { get; private set; }
        public IReadOnlyList<Trk> Trks { get; private set; } = new List<Trk>();

        #endregion old Bar

        #region copied from Seq (now deleted)

        /// <summary>
        /// AbsMsPosition is greater than or equal to 0.
        /// There is at least one Trk in Trks.
        /// Trk.AssertConsistency() is called on all Trks.
        /// All Trk.InterpretationCount values are the same.
        /// All Interpretations having the same index in any Trk in this Bar have the same msDuration.
        /// Interpretation consistency across Trks:
        /// The overall sequence of events (DurationDefs) must be identical in all interpretations.
        /// </summary>
        public virtual void AssertConsistency()
        {
            Debug.Assert(AbsMsPosition >= 0);
            Debug.Assert(Trks.Count > 0);
        }

        protected Trk GetInterpretation(int trkIndex, int interpIndex)
        {
            Trk returnTrk = new Trk();
            var mainUniqueDefs = Trks[trkIndex].UniqueDefs;
            for(int i = 0; i < mainUniqueDefs.Count; ++i)
            {
                var uniqueDef = mainUniqueDefs[i];
                if(uniqueDef is MidiChordDef mcd)
                {               
                    returnTrk.Add(mcd.MidiDefs[interpIndex]);
                }
                else if(uniqueDef is RestDef restDef)
                {
                    returnTrk.Add(restDef.MidiDefs[interpIndex]);
                }
            }
            return returnTrk;
        }

        public int MsDuration { get => Trks[0].MsDuration; }

        #region Envelopes
        /// <summary>
        /// This function does not change the MsDuration of the Seq.
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

            foreach(Trk trk in Trks)
            {
                List<IUniqueDef> iuds = trk.UniqueDefs;
                IUniqueDef iud;
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
        /// returns a list containing the msPositions of all the IUniqueDefs in the default interpretation of all Trks
        /// plus the endMsPosition of the final object.
        /// </summary>
        /// <returns></returns>
        private List<int> GetMsPositions()
        {
            int originalMsDuration = MsDuration;

            List<int> originalMsPositions = new List<int>();
            foreach(Trk trk in Trks)
            {
                foreach(IUniqueDef iud in trk)
                {
                    int msPos = iud.MsPositionReFirstUD;
                    if(!originalMsPositions.Contains(msPos))
                    {
                        originalMsPositions.Add(msPos);
                    }
                }
            }
            originalMsPositions.Sort();
            originalMsPositions.Add(originalMsDuration);

            return originalMsPositions;
        }

        #endregion Envelopes
        #endregion
    }
}
