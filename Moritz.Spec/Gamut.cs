using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Moritz.Spec
{
    /// <summary>
    /// A Gamut contains/is a list of absolute pitch numbers in an ascending order scale.
    /// All the values are different and in range [0..127].
    /// <para>Gamut.List[0] == absolutePitchHierarchy[0] % 12 (restricted to range [0..11]).</para>
    /// <para>Each absolute pitch exists at all possible octaves above Gamut.List[0].
    /// (So each octave range in the gamut contains the same absolute pitches.)</para>
    /// </summary>
    public class Gamut
    {
        #region constructors
        /// <summary>
        /// Gamuts are immutable!
        /// </summary>
        /// <param name="relativePitchHierarchyIndex">Will be treated % 22 (Gamut.RelativePitchHierarchiesCount)</param>
        /// <param name="basePitch">Will be treated % 12</param>
        /// <param name="nPitchesPerOctave">Will be treated % 13</param>
        public Gamut(int relativePitchHierarchyIndex, int basePitch, int nPitchesPerOctave)
        {
            relativePitchHierarchyIndex %= Gamut.RelativePitchHierarchiesCount;
            basePitch %= 12;
            nPitchesPerOctave %= 13;

            List<int> absolutePitchHierarchy = GetAbsolutePitchHierarchy(relativePitchHierarchyIndex, basePitch);
            #region condition
            ThrowExceptionIfPitchHierarchyIsInvalid(absolutePitchHierarchy);
            #endregion condition

            _absolutePitchHierarchy = new List<int>(absolutePitchHierarchy);
            _list = GetGamutList(_absolutePitchHierarchy, nPitchesPerOctave);
            
            RelativePitchHierarchyIndex = relativePitchHierarchyIndex;
            BasePitch = basePitch;
            NPitchesPerOctave = nPitchesPerOctave;
            MaxPitch = _list[_list.Count - 1];
        }

        #region private helper functions
        /// <summary>
        /// A pitchHierarchy.Count must be 12.
        /// Each value must be in range [0..11] and occur only once (no duplicates).
        /// </summary>
        /// <param name="pitchHierarchy"></param>
        private void ThrowExceptionIfPitchHierarchyIsInvalid(List<int> pitchHierarchy)
        {
            Debug.Assert(pitchHierarchy.Count == 12);
            List<bool> presence = new List<bool>();
            for(int i = 0; i < 12; ++i)
            {
                presence.Add(false);
            }

            foreach(int value in pitchHierarchy)
            {
                Debug.Assert(value >= 0 && value <= 11);
                Debug.Assert(presence[value] == false);
                presence[value] = true;
            }

            for(int i = 0; i < 12; ++i)
            {
                Debug.Assert(presence[i] == true);
            }
        }

        private List<int> GetGamutList(List<int> absolutePitchHierarchy, int nPitchesPerOctave)
        {
            int rootPitch = absolutePitchHierarchy[0];

            List<int> sortedBasePitches = new List<int>();
            for(int i = 0; i < nPitchesPerOctave; ++i)
            {
                sortedBasePitches.Add(absolutePitchHierarchy[i]);
            }
            sortedBasePitches.Sort();

            List<int> gamutList = new List<int>();
            int rphIndex = 0;
            int octave = 0;
            while(true)
            {
                int pitch = sortedBasePitches[rphIndex++] + (octave * 12);
                if(pitch > 127)
                {
                    break;
                }

                if(pitch >= rootPitch)
                {
                    gamutList.Add(pitch);
                }

                if(rphIndex >= sortedBasePitches.Count)
                {
                    rphIndex = 0;
                    octave++;
                }
            }

            ThrowExceptionIfGamutListIsInvalid(gamutList);

            return gamutList;
        }

        /// <summary>
        /// Throws an exception if the argument is invalid for any of the following reasons:
        /// 1. The argument may not be null or empty.
        /// 2. All the values must be different, in ascending order, and in range [0..127].
        /// 3. Each absolute pitch exists at all possible octaves in the gamut.
        /// (So each octave range in the gamut contains the same absolute pitches.)
        /// </summary>
        private void ThrowExceptionIfGamutListIsInvalid(List<int> gamutList)
        {
            if(gamutList == null || !gamutList.Any())
            {
                throw new ArgumentNullException($"The {nameof(gamutList)} argument is null or empty.");
            }
            if(gamutList[0] % 12 != AbsolutePitchHierarchy[0])
            {
                throw new ArgumentException($"The lowest pitch in a gamutList must always be equal to {nameof(AbsolutePitchHierarchy)}[0].");
            }
            for(int i = 1; i < gamutList.Count; ++i)
            {
                if(gamutList[i] < 0 || gamutList[i] > 127)
                {
                    throw new ArgumentException($"{nameof(gamutList)}[{i}] is out of range.");
                }
                if(gamutList[i] <= gamutList[i - 1])
                {
                    throw new ArgumentException($"{nameof(gamutList)} must be in ascending order.");
                }
            }

            #region check pitch consistency
            List<int> basePitches = new List<int>();
            int pitchIndex = 0;
            int octaveAboveBasePitch = gamutList[0] + 12;
            while(pitchIndex < gamutList.Count && gamutList[pitchIndex] < octaveAboveBasePitch)
            {
                basePitches.Add(gamutList[pitchIndex++]); 
            }
            int pitchCount = 0;
            foreach(int pitch in basePitches)
            {
                int pitchOctave = pitch;
                while(pitchOctave < 128)
                {
                    if(!gamutList.Contains(pitchOctave))
                    {
                        throw new Exception($"Missing pitch in gamut list.");
                    }
                    pitchCount += 1;
                    pitchOctave += 12;
                }
            }
            if(gamutList.Count > pitchCount)
            {
                throw new Exception($"Unknown pitch in gamut list.");
            }
            #endregion check pitch consistency
        }
        #endregion private helper functions

        #endregion constructors

        #region public functions
        /// <summary>
        /// returns the value in the gamut list at index i.
        /// </summary>
        public int this[int i]
        {
            get
            {
                return _list[i];
            }
        }

        public bool Contains(int pitch)
        {
            return _list.Contains(pitch);
        }

        /// <summary>
        /// Returns true if the gamut.List contains all the pitches in the argument. Otherwise false. 
        /// </summary>
        public bool ContainsAllPitches(MidiChordDef mcd)
        {
            foreach(BasicMidiChordDef bmcd in mcd.BasicMidiChordDefs)
            {
                for(int i = 0; i < bmcd.Pitches.Count; ++i)
                {
                    if(this.Contains(bmcd.Pitches[i]) == false)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Returns true if the gamut.List contains all the pitches in the argument. Otherwise false. 
        /// </summary>
        public bool ContainsAllPitches(List<IUniqueDef> iuds)
        {
            foreach(IUniqueDef iud in iuds)
            {
                MidiChordDef mcd = iud as MidiChordDef;
                if(mcd != null && ContainsAllPitches(mcd) == false)
                {
                    return false;                    
                }
            }
            return true;
        }

        /// <summary>
        /// Returns -1 if pitch is not found.
        /// </summary>
        public int IndexOf(int pitch)
        {
            return _list.IndexOf(pitch);
        }

        /// <summary>
        /// The returned list contains a list of pitches, one pitch per envelope.Original.Count.
        /// Throws an exception if firstPitch is not in the gamut.List.
        /// Pitches that would be lower or higher than any pitch in the gamut are silently coerced to
        /// the lowest or highest values respectively.
        /// </summary>
        /// <param name="firstPitch">Will be the first pitch in the returned list.</param>
        internal List<int> PitchSequence(int firstPitch, Envelope envelope)
        {
            Debug.Assert(_list.Contains(firstPitch), $"{nameof(firstPitch)} is not in gamut.");

            List<int> pitchSequence = new List<int>();
            if(envelope == null)
            {
                pitchSequence.Add(firstPitch);
            }
            else
            {
                List<int> envOriginal = envelope.Original; // clone
                int firstIndexInEnvelope = envOriginal[0];
                int indexOfFirstPitchInGamut = IndexOf(firstPitch);
                int indexDiff = indexOfFirstPitchInGamut - firstIndexInEnvelope;

                List<int> indices = envOriginal;
                for(int i = 0; i < indices.Count; ++i)
                {
                    indices[i] += indexDiff;
                    indices[i] = (indices[i] < 0) ? 0 : indices[i];
                    indices[i] = (indices[i] >= this.Count) ? this.Count - 1 : indices[i];
                }

                foreach(int index in indices)
                {
                    pitchSequence.Add(_list[index]);
                }
            }

            return pitchSequence;
        }

        /// <summary>
        /// Returns a list of pitch numbers in range [0..127] in ascending order.
        /// The first pitch in the returned list is always rootPitch.
        /// The other returned pitches are transposed to be in ascending order by adding 12 as necessary.
        /// The maximum number of pitches returned is either nPitches or the number of pitches beginning with and
        /// following the rootPitch in the absolutePitchHierarchy, whichever is smaller.
        /// The number of returned pitches can also be smaller than nPitches because pitches that would be higher
        /// than 127 are simply not added to the returned list.
        /// </summary>
        /// <param name="rootPitch">In range [0..127]</param>
        /// <param name="nPitches">In range [1..12]</param>
        /// <returns></returns>
        public List<byte> GetChord(int rootPitch, int nPitches)
        {
            Debug.Assert(nPitches > 0 && nPitches <= 12);
            Debug.Assert(_list.Contains(rootPitch));

            List<int> pitches = new List<int>();
            pitches.Add(rootPitch);

            if(nPitches > 1)
            {
                int absRootPitch = rootPitch % 12;
                int rootIndex = AbsolutePitchHierarchy.IndexOf(absRootPitch);
                int maxIndex = rootIndex + nPitches;
                maxIndex = (maxIndex < NPitchesPerOctave) ? maxIndex : NPitchesPerOctave;
                for(int i = rootIndex + 1; i < maxIndex; ++i)
                {
                    int pitch = AbsolutePitchHierarchy[i];
                    int lowerPitch = pitches[pitches.Count - 1];
                    int minimumInterval = MinimumInterval(lowerPitch);
                    while((pitch - lowerPitch) < minimumInterval)
                    {
                        pitch += 12;
                    }

                    if(pitch <= 127)
                    {
                        pitches.Add(pitch);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            List<byte> bytePitches = new List<byte>();
            foreach(int pitch in pitches)
            {
                bytePitches.Add((byte)pitch);
            }

            return bytePitches;
        }

        private int MinimumInterval(int lowerPitch)
        {
            int minimumInterval;

            if(lowerPitch < 6)
                minimumInterval = 15;
            else if(lowerPitch < 15)
                minimumInterval = 14;
            else if(lowerPitch < 23)
                minimumInterval = 13;
            else if(lowerPitch < 30)
                minimumInterval = 12;
            else if(lowerPitch < 36)
                minimumInterval = 11;
            else if(lowerPitch < 41)
                minimumInterval = 10;
            else if(lowerPitch < 45)
                minimumInterval = 9;
            else if(lowerPitch < 48)
                minimumInterval = 8;
            else if(lowerPitch < 50)
                minimumInterval = 7;
            else
            {
                switch(lowerPitch)
                {
                    case 50:
                        minimumInterval = 6;
                        break;
                    case 51:
                        minimumInterval = 5;
                        break;
                    case 52:
                        minimumInterval = 4;
                        break;
                    case 53:
                        minimumInterval = 3;
                        break;
                    case 54:
                        minimumInterval = 2;
                        break;
                    default:
                        minimumInterval = 1;
                        break;
                }
            }

            return minimumInterval;
        }

        /// <summary>
        /// The returned list contains 12 velocity values in range [1..127] that can be applied to pitches in any Trk or MidiChordDef
        /// (using their SetVelocityPerAbsolutePitch(...) ) function, regardless of the gamut from which they have been constructed.
        /// Pitches that are part of *this* gamut in the returned list are given velocities in range [minimumVelocity..maximumVelocity].
        /// Other pitches are given velocity = minimumVelocity.
        /// The returned values are in order of absolute pitch, with C natural (absolute pitch 0) at position 0, C# (absolute pitch 1) at
        /// position 1, etc.
        /// The velocities of the gamut's performing pitches are permuted according to a contour in a linear-negative field having
        /// domain NPitchesPerOctave. The contour is found using the loudestPitchIndex.
        /// The contour at loudestPitchIndex==0 (the default) is simply ascending (e.g. 1,2,3,4,5,6,7,8).
        /// The contour at loudestPitchIndex==(NValuesPerOctave-1) is simply descending (e.g. 9,8,7,6,5,4,3,2,1).
        /// So, in chords constructed from this gamut, loudestPitchIndex==0 results in a bottom-to-top (i.e. loud-quiet) velocity gradient with
        /// respect to the gamut's absolute pitch hierarchy, while loudestPitchIndex==(NValuesPerOctave-1) results in a top-to-bottom (i.e. loud-quiet)
        /// gradient with respect to the gamut's absolute pitch hierarchy.
        /// </summary>
        /// <param name="minimumVelocity">In range [1..127]. The minimum velocity to be given to any pitch.</param>
        /// <param name="maximumVelocity">In range [1..127]. The maximum velocity to be given to any pitch.</param>
        /// <param name="loudestPitchIndex">In range [0..(NPitchesPerOctave-1)].</param>
        /// <param name="isLinearGradient">If false, velocity values are scaled logarithmically between minimumVelocity and maximumVelocity.</param>
        public List<byte> GetVelocityPerAbsolutePitch(int minimumVelocity, int maximumVelocity, int loudestPitchIndex = 0, bool isLinearGradient = true)
        {
            Debug.Assert(minimumVelocity >= 1 && minimumVelocity <= 127);
            Debug.Assert(maximumVelocity >= 1 && maximumVelocity <= 127);
            Debug.Assert(minimumVelocity <= maximumVelocity);
            Debug.Assert(loudestPitchIndex >= 0 && loudestPitchIndex <= (NPitchesPerOctave-1));

            List<double> velocities = new List<double>();
            if(NPitchesPerOctave == 1)
            {
                velocities.Add(maximumVelocity);
            }
            else if(isLinearGradient)
            {
                double velocityDiff = ((double)(maximumVelocity - minimumVelocity)) / (NPitchesPerOctave - 1);
                for(int i = 0; i < NPitchesPerOctave; ++i)
                {
                    double vel = maximumVelocity - (i * velocityDiff);
                    velocities.Add(vel);
                }
            }
            else
            {
                double factor = ((double) Math.Pow(((double)minimumVelocity / maximumVelocity), (((double)1) / (NPitchesPerOctave - 1))));
                velocities.Add(maximumVelocity);
                for(int i = 1; i < NPitchesPerOctave; ++i)
                {
                    double vel = velocities[i - 1] * factor;
                    velocities.Add(vel);
                }
            }

            if(NPitchesPerOctave > 1)
            {
                velocities = ContourVelocities(velocities, loudestPitchIndex);
            }

            List<byte> velocityPerAbsPitch = new List<byte>();
            for(int i = 0; i < 12; ++i)
            {
                velocityPerAbsPitch.Add((byte)minimumVelocity); // default value
            }

            for(int absPitchIndex = 0; absPitchIndex < NPitchesPerOctave ; ++absPitchIndex)
            {
                int absPitch = AbsolutePitchHierarchy[absPitchIndex];

                byte velocity = (byte) Math.Round(velocities[absPitchIndex]);
                velocity = (velocity >= 1) ? velocity : (byte)1;
                velocity = (velocity <= 127) ? velocity : (byte)127;

                velocityPerAbsPitch[absPitch] = velocity;
            }
            return velocityPerAbsPitch;
        }

        /// <summary>
        /// Returns the values in the velocities list permuted linearly.
        /// The velocites list can be of any length. loudestPitchIndex is always in range [0..(NPitchesPerOctave-1)].
        /// If loudestPitchIndex==0, the list will be unchanged. If loudestPitchIndex==(NPitchesPerOctave-1), the list will be reversed.
        /// </summary>
        /// <param name="velocities">velocities.Count == NPitchesPerOctave.</param>
        /// <param name="contourNumber">in range 1..12</param>
        private List<double> ContourVelocities(List<double> velocities, int loudestPitchIndex)
        {
            Debug.Assert(velocities.Count == NPitchesPerOctave && NPitchesPerOctave > 1);
            Debug.Assert(loudestPitchIndex >= 0 && loudestPitchIndex <= (NPitchesPerOctave - 1));

            List<double> newVs = new List<double>();
            List<int> contour = GetLinearNegativeContour(velocities.Count, loudestPitchIndex);
            foreach(int val in contour)
            {
                newVs.Add(velocities[val - 1]);
            }
            return newVs;
        }

        /// <summary>
        /// Returns a contour having count values, whose first value is (contourIndex+1). 
        /// (as if from a linear-negative matrix containing count contours having domain=count).
        /// </summary>
        /// <param name="count">In range 2..12</param>
        /// <param name="contourIndex">In range 0..(count-1)</param>
        private List<int> GetLinearNegativeContour(int count, int contourIndex)
        {
            Debug.Assert(count >= 2 && count <= 12);
            Debug.Assert(contourIndex >= 0 && contourIndex <= (count - 1));

            int incrSign = 1; // results in a "linear-negative" contour (when contourIndex > 0, contour[1] < contour[0].
            int absIncr = 0;
            int val = contourIndex + 1;
            int incr = absIncr * incrSign; 
            int breakIndex = 0;
           
            List<int> contour = new List<int>();
            for(int i = 0; i < count; ++i)
            {
                contour.Add(val);
                incrSign *= -1;
                absIncr += 1;
                incr = absIncr * incrSign;
                val += incr;
                if(val > count || val < 1)
                {
                    breakIndex = i;
                    break;
                }

            }
            // increasing or decreasing end phase
            if(breakIndex < count - 1)
            {
                int endIncr = (val > count) ? -1 : 1;
                val -= incr;
                val += endIncr;
                while(++breakIndex < count)
                {
                    contour.Add(val);
                    val += endIncr;
                }
            }
            return contour;
        }

        #endregion public functions

        #region public properties
        public static int RelativePitchHierarchiesCount { get { return RelativePitchHierarchies.Count; } }
        public readonly int RelativePitchHierarchyIndex;
        public readonly int BasePitch;
        public readonly int NPitchesPerOctave;
        public readonly int MaxPitch;

        /// <summary>
        /// A clone of the private list.
        /// </summary>
        public List<int> AbsolutePitchHierarchy { get { return new List<int>(_absolutePitchHierarchy); } }
        private List<int> _absolutePitchHierarchy = new List<int>();

        /// <summary>
        /// A clone of the private list.
        /// </summary>
        public List<int> List { get { return new List<int>(_list); } }
        private List<int> _list;

        public int Count { get { return _list.Count; } }
        #endregion public interface

        #region Pitch Hierarchies
        /// <summary>
        /// Returns a list that contains the sums of absoluteValue(rootPitch) + RelativePitchHierarchies[index].
        /// If a value would be greater than 11, value = value - 12, so that all values are in range [0..11].
        /// </summary>
        /// <param name="relativePitchHierarchyIndex">In range [0..21]</param>
        /// <param name="rootPitch">In range [0..127]</param>
        public static List<int> GetAbsolutePitchHierarchy(int relativePitchHierarchyIndex, int rootPitch)
        {
            if(RelativePitchHierarchies.Count != 22)
            {
                throw new ArgumentException($"{nameof(RelativePitchHierarchies)} has changed!");
            }
            if(relativePitchHierarchyIndex < 0 || relativePitchHierarchyIndex >= RelativePitchHierarchies.Count)
            {
                throw new ArgumentException($"{nameof(relativePitchHierarchyIndex)} out of range.");
            }
            if(rootPitch < 0 || rootPitch > 127)
            {
                throw new ArgumentException($"{nameof(rootPitch)} out of range.");
            }

            List<int> absolutePitchHierarchy = new List<int>(RelativePitchHierarchies[relativePitchHierarchyIndex]); // checks index
            int absRootPitch = rootPitch % 12;

            for(int i = 0; i < absolutePitchHierarchy.Count; ++i)
            {
                absolutePitchHierarchy[i] += absRootPitch;
                absolutePitchHierarchy[i] = (absolutePitchHierarchy[i] > 11) ? absolutePitchHierarchy[i] - 12 : absolutePitchHierarchy[i];
            }
            return absolutePitchHierarchy;
        }

        /// <summary>
        /// This series of RelativePitchHierarchies is derived from the "most consonant" hierarchy at index 0:
        ///                    0, 7, 4, 10, 2, 5, 9, 11, 1, 3, 6, 8
        /// which has been deduced from the harmonic series as follows (decimals rounded to 3 figures):
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
        private static List<List<int>> RelativePitchHierarchies = new List<List<int>>()
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

        #endregion Pitch Hierarchies
    }
}
