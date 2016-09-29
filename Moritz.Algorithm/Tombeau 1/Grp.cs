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

        public new Grp Clone()
        {
            List<IUniqueDef> clonedIUDs = GetUniqueDefsClone();
            Gamut gamut = null;
            if(this.Gamut != null)
            {
                gamut = this.Gamut.Clone();
            }
            Grp grp = new Grp(MidiChannel, MsPositionReContainer, clonedIUDs, gamut);
            grp.Container = this.Container;

            return grp;
        }

        public new void SetVelocityPerAbsolutePitch(List<byte> velocityPerAbsolutePitch, double percent = 100.0)
        {
            base.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch, percent);            
            SetBeamEnd(); // the final MidiChordDef may have been replaced by a RestDef
        }

        public new void Permute(int axisNumber, int contourNumber)
        {
            base.Permute(axisNumber, contourNumber);
            SetBeamEnd(); // the final MidiChordDef may have moved
        }

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
