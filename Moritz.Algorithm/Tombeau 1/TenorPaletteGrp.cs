using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
    internal class TenorPaletteGrp : Grp
    {
        public TenorPaletteGrp(Gamut gamut, int domain)
            //rootOctave = 3;
            //pitchesPerChord = 6;
            //msDurationPerChord = 200; // dummy, durations are set from pitches below in the ctor
            //velocityFactor = 0.5; // dummy, velocities are set from absolute pitches below in the ctor
            : base(gamut, 3, 6, 200, domain, 0.5)
        {
            int minimumVelocity = 20;
            int maximumVelocity = 127;
            List<byte> velocityPerAbsolutePitch = gamut.GetVelocityPerAbsolutePitch(minimumVelocity, maximumVelocity, 0, true);

            SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch, (byte)minimumVelocity);

            int minMsDuration = 200;
            int maxMsDuration = 300;
            SetDurationsFromPitches(maxMsDuration, minMsDuration, true);
        }

       
    }       
}