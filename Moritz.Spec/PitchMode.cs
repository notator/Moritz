using System;
using static System.Diagnostics.Debug;
using System.Collections.Generic;

namespace Moritz.Spec
{
    public class PitchMode
    {
        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index">The index of the relativePitchHierarchy in the static list. (Range [0..21]</param>
        /// <param name="nPitchesPerOctave">The number of different pitches in each octave in the Mode. (Range [1..12])</param>
        /// <param name="basePitch">The basePitch of the PitchMode's Gamut. (Range [0..11])</param>
        /// 
        public PitchMode(int index, int nPitchesPerOctave, int basePitch)
        {
            #region conditions

            if(index < 0 || index >= relativePitchHierarchies.Count)
            {
                throw new ArgumentException($"{nameof(index)} out of range.");
            }

            if(nPitchesPerOctave < 1 || nPitchesPerOctave > relativePitchHierarchies[0].Count)
            {
                throw new ArgumentException($"{nameof(nPitchesPerOctave)} out of range.");
            }

            CheckBasePitch(basePitch);

            #endregion conditions

            List<int> rph = relativePitchHierarchies[index];
            for(int i = 0; i < nPitchesPerOctave; ++i )
            {
                _relativePitchHierarchy.Add(rph[i]);
            }

            SetGamut(basePitch);
        }

        private static void CheckBasePitch(int basePitch)
        {
            if(basePitch < 0 || basePitch > 11)
            {
                throw new ArgumentException($"{nameof(basePitch)} out of range.");
            }
        }

        /// <summary>
        /// Sets Gamut to all the available absolute pitches in an ascending order scale.
        /// Gamut[0] == basePitch. All the values are different. The final value is less than or equal to 127.
        /// To create a RelativeGamut that can be transposed, use Gamut(0).
        /// </summary>
        /// <param name="basePitch">In range [0..11]</param>
        private void SetGamut(int basePitch)
        {
            List<int> sortedBasePitches = SortedBasePitches(basePitch); // checks basePitch

            var gamut = new List<int>();
            int rphIndex = 0;
            int octave = 0;
            while(true)
            {
                int pitch = sortedBasePitches[rphIndex++] + (octave * 12);
                if(pitch > 127)
                {
                    break;
                }
                gamut.Add(pitch);

                if(rphIndex >= sortedBasePitches.Count)
                {
                    rphIndex = 0;
                    octave++;
                }
            }

            _gamut = new Gamut(gamut); // checks its argument
        }

        /// <summary>
        /// The pitches in the first octave above the base pitch (sorted into ascending order).
        /// The first value in the returned list is the basePitch.
        /// </summary>
        /// <param name="basePitch">In range [0..11]</param>
        private List<int> SortedBasePitches(int basePitch)
        {
            CheckBasePitch(basePitch);

            List<int> rval = RelativePitchHierarchy; // a clone
            rval.Sort();
            for(int i = 0; i < rval.Count; ++i)
            {
                rval[i] += basePitch;
            }

            return rval;
        }

        #endregion constructor

        /// <summary>
        ///  A clone of a private field.
        ///  A Debug.Assertion fails if the private field is changed and:
        ///  1. any value is < 0 or > 11.
        ///  2. the first value in the list is not 0.
        ///  3. the number of values in the list is < 1 or > 12.
        /// </summary>
        public List<int> RelativePitchHierarchy
        {
            get
            {
                List<int> rval = new List<int>();
                foreach(int val in _relativePitchHierarchy)
                {
                    Assert(val >= 0 && val <= 11);
                    rval.Add(val);
                }
                Assert(rval[0] == 0);
                Assert(rval.Count >= 1 && rval.Count <= 12);
                return rval;
            }
        }

        public Gamut Gamut { get { return _gamut; }}

        public int NumberOfAbsolutePitches { get { return _relativePitchHierarchy.Count; } }

        /// <summary>
        /// This series of relativePitchHierarchies is derived from the "most consonant" hierarchy at index 0:
        ///                    0, 7, 4, 10, 2, 5, 9, 11, 1, 3, 6, 8
        /// which is deduced from the harmonic series as follows (decimals rounded to 3 figures):
        /// 
        ///              absolute   equal              harmonic:     absolute         closest
        ///              pitch:  temperament                         harmonic    equal temperament
        ///                        factor:                           factor:       absolute pitch:
        ///                0:       1.000       |          1   ->   1/1  = 1.000  ->     0:
        ///                1:       1.059       |          3   ->   3/2  = 1.500  ->     7:
        ///                2:       1.122       |          5   ->   5/4  = 1.250  ->     4:
        ///                3:       1.189       |          7   ->   7/4  = 1.750  ->     10:
        ///                4:       1.260       |          9   ->   9/8  = 1.125  ->     2:
        ///                5:       1.335       |         11   ->  11/8  = 1.375  ->     5:
        ///                6:       1.414       |         13   ->  13/8  = 1.625  ->     9:
        ///                7:       1.498       |         15   ->  15/8  = 1.875  ->     11:
        ///                8:       1.587       |         17   ->  17/16 = 1.063  ->     1:
        ///                9:       1.682       |         19   ->  19/16 = 1.187  ->     3:
        ///                10:      1.782       |         21   ->  21/16 = 1.313  ->     
        ///                11:      1.888       |         23   ->  23/16 = 1.438  ->     6:
        ///                                     |         25   ->  25/16 = 1.563  ->     8:
        /// </summary>
        private static List<List<int>> relativePitchHierarchies = new List<List<int>>()
        {
            new List<int>(){ 0,  7,  4, 10,  2,  5,  9, 11,  1,  3,  6,  8 }, //  0
            new List<int>(){ 0,  4,  7,  2, 10,  9,  5,  1, 11,  6,  3,  8 }, //  1
            new List<int>(){ 0,  4,  2,  7,  9, 10,  1,  5,  6, 11,  8,  3 }, //  2
            new List<int>(){ 0,  2,  4,  9,  7,  1, 10,  6,  5,  8, 11,  3 }, //  3
            new List<int>(){ 0,  2,  9,  4,  1,  7,  6, 10,  8,  5,  3, 11 }, //  4
            new List<int>(){ 0,  9,  2,  1,  4,  6,  7,  8, 10,  3,  5, 11 }, //  5
            new List<int>(){ 0,  9,  1,  2,  6,  4,  8,  7,  3, 10, 11,  5 }, //  6
            new List<int>(){ 0,  1,  9,  6,  2,  8,  4,  3,  7, 11, 10,  5 }, //  7
            new List<int>(){ 0,  1,  6,  9,  8,  2,  3,  4, 11,  7,  5, 10 }, //  8
            new List<int>(){ 0,  6,  1,  8,  9,  3,  2, 11,  4,  5,  7, 10 }, //  9
            new List<int>(){ 0,  6,  8,  1,  3,  9, 11,  2,  5,  4, 10,  7 }, // 10
            new List<int>(){ 0,  8,  6,  3,  1, 11,  9,  5,  2, 10,  4,  7 }, // 11
            new List<int>(){ 0,  8,  3,  6, 11,  1,  5,  9, 10,  2,  7,  4 }, // 12
            new List<int>(){ 0,  3,  8, 11,  6,  5,  1, 10,  9,  7,  2,  4 }, // 13
            new List<int>(){ 0,  3, 11,  8,  5,  6, 10,  1,  7,  9,  4,  2 }, // 14
            new List<int>(){ 0, 11,  3,  5,  8, 10,  6,  7,  1,  4,  9,  2 }, // 15
            new List<int>(){ 0, 11,  5,  3, 10,  8,  7,  6,  4,  1,  2,  9 }, // 16
            new List<int>(){ 0,  5, 11, 10,  3,  7,  8,  4,  6,  2,  1,  9 }, // 17
            new List<int>(){ 0,  5, 10, 11,  7,  3,  4,  8,  2,  6,  9,  1 }, // 18
            new List<int>(){ 0, 10,  5,  7, 11,  4,  3,  2,  8,  9,  6,  1 }, // 19
            new List<int>(){ 0, 10,  7,  5,  4, 11,  2,  3,  9,  8,  1,  6 }, // 20
            new List<int>(){ 0,  7, 10,  4,  5,  2, 11,  9,  3,  1,  8,  6 }, // 21 
        };

        /// <summary>
        /// One of the above lists, truncated to nPitches values.
        /// </summary>
        private List<int> _relativePitchHierarchy = new List<int>();

        private Gamut _gamut = null;

    }
}
