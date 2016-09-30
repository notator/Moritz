using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;
using Moritz.Globals;

namespace Moritz.Algorithm.Tombeau1
{
    public class Grp : Trk
    {
        #region constructors
        public Grp(Gamut gamut, int rootPitch, int nPitchesPerChord, int msDurationPerChord, int nChords)
            : base(0, 0, new List<IUniqueDef>(), gamut)
        {
            int rootIndex = Gamut.IndexOf(rootPitch);
            Debug.Assert(rootIndex >= 0); 

            for(int i = 0; i < nChords; ++i)
            {
                MidiChordDef mcd = new MidiChordDef(msDurationPerChord, Gamut, Gamut[rootIndex + i], nPitchesPerChord, null);
                _uniqueDefs.Add(mcd);
            }

            SetBeamEnd();
        }

        /// <summary>
        /// Used by Clone.
        /// </summary>
        public Grp(int midiChannel, int msPositionReContainer, List<IUniqueDef> clonedIUDs, Gamut clonedGamut)
            : base(midiChannel, msPositionReContainer, clonedIUDs, clonedGamut)
        {
            SetBeamEnd();
        }
        #endregion constructors

        /// <summary>
        /// A deep clone of the Grp.
        /// </summary>
        public new Grp Clone()
        {
            Trk clonedTrk = base.Clone();
            Grp grp = new Grp(clonedTrk.MidiChannel, clonedTrk.MsPositionReContainer, clonedTrk.UniqueDefs, clonedTrk.Gamut);
            grp.Container = this.Container;

            return grp;
        }
        /***********************/
        #region Trk functions that change the sequence or number of MidiChordDefs
        #region Add, Remove, Insert, Replace objects in the Trk
        /// <summary>
        /// Appends the new MidiChordDef, RestDef, CautionaryChordDef or ClefChangeDef to the end of the list.
        /// Automatically sets the iUniqueDef's msPosition.
        /// Used by Block.PopBar(...), so accepts a CautionaryChordDef argument.
        /// </summary>
        public override void Add(IUniqueDef iUniqueDef)
        {
            base.Add(iUniqueDef);
            SetBeamEnd();
        }
        /// <summary>
        /// Adds the argument's UniqueDefs to the end of this Trk.
        /// Sets the MsPositions of the appended UniqueDefs.
        /// </summary>
        public override void AddRange(VoiceDef voiceDef)
        {
            base.AddRange(voiceDef);
            SetBeamEnd();
        }
        /// <summary>
        /// Inserts the iUniqueDef in the list at the given index, and then
        /// resets the positions of all the uniqueDefs in the list.
        /// </summary>
        public override void Insert(int index, IUniqueDef iUniqueDef)
        {
            base.Insert(index, iUniqueDef);
            SetBeamEnd();
        }
        /// <summary>
        /// Inserts the trk's UniqueDefs in the list at the given index, and then
        /// resets the positions of all the uniqueDefs in the list.
        /// </summary>
        public new void InsertRange(int index, Trk trk)
        {
            base.InsertRange(index, trk);
            SetBeamEnd();
        }
        /// <summary>
        /// Removes the iUniqueDef at index from the list, and then inserts the replacement at the same index.
        /// </summary>
        public new void Replace(int index, IUniqueDef replacementIUnique)
        {
            base.Replace(index, replacementIUnique);
            SetBeamEnd();
        }
        /// <summary> 
        /// This function attempts to add all the non-RestDef UniqueDefs in trk2 to the calling Trk
        /// at the positions given by their MsPositionReFirstIUD added to trk2.MsPositionReContainer,
        /// whereby trk2.MsPositionReContainer is used with respect to the calling Trk's container.
        /// Before doing the superimposition, the calling Trk is given leading and trailing RestDefs
        /// so that trk2's uniqueDefs can be added at their original positions without any problem.
        /// These leading and trailing RestDefs are however removed before the function returns.
        /// The superimposed uniqueDefs will be placed at their original positions if they fit inside
        /// a RestDef in the original Trk. A Debug.Assert() fails if this is not the case.
        /// To insert single uniqueDefs between existing uniqueDefs, use the function
        ///     Insert(index, iudToInsert).
        /// trk2's UniqueDefs are not cloned.
        /// </summary>
        /// <returns>this</returns>
        public new Trk Superimpose(Trk trk2)
        {
            Trk trk = base.Superimpose(trk2);
            SetBeamEnd();

            return trk;
        }
        #endregion Add, Remove, Insert, Replace objects in the Trk

        public new void Permute(int axisNumber, int contourNumber)
        {
            base.Permute(axisNumber, contourNumber);
            SetBeamEnd(); // the final MidiChordDef may have moved
        }

        #endregion Trk functions that change the sequence or number of MidiChordDefs

        /***********************/
        #region Trk functions that change the velocities in MidiChordDefs

        public new void SetVelocityPerAbsolutePitch(List<byte> velocityPerAbsolutePitch, double percent = 100.0)
        {
            base.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch, percent);
            SetBeamEnd(); // the final MidiChordDef may have been replaced by a RestDef
        }

        #endregion Trk functions that change the velocities in MidiChordDefs
        /***********************/

        /// <summary>
        /// Sets BeamContinues to false on the final MidiChordDef, and true on all the others.
        /// </summary>
        private void SetBeamEnd()
        {
            bool isFinalChord = true;
            for(int i = _uniqueDefs.Count - 1; i >= 0; --i)
            {
                MidiChordDef mcd = _uniqueDefs[i] as MidiChordDef;
                if(mcd != null)
                {
                    if(isFinalChord)
                    {
                        mcd.BeamContinues = false;
                        isFinalChord = false;
                    }
                    else
                    {
                        mcd.BeamContinues = true;
                    }
                }
            }
        }
    }
}
