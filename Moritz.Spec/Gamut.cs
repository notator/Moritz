using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Diagnostics.Debug;

namespace Moritz.Spec
{
    /// <summary>
    /// A Gamut contains/is a list of absolute pitch numbers in an ascending order scale.
    /// All the values are different and in range [0..127].
    /// <para>Gamut.List[0] == basePitch (restricted to range [0..11]).</para>
    /// <para>Each absolute pitch exists at all possible octaves in the gamut.
    /// (So each octave range in the gamut contains the same absolute pitches.)</para>
    /// <para>Pitches can be added to, or removed from, a gamut by calling
    /// AddOctaves(...) or RemoveOctaves(...).</para>
    /// </summary>
    public class Gamut
    {
        #region constructors

        /// <summary>
        /// A Gamut contains/is a list of absolute pitch numbers in an ascending order scale.
        /// All the values are different and in range [0..127].
        /// <para>Gamut.List[0] == basePitch (restricted to range [0..11]).</para>
        /// <para>Each absolute pitch exists at all possible octaves in the gamut.
        /// (So each octave range in the gamut contains the same absolute pitches.)</para>
        /// <para>Pitches can be added to, or removed from, a gamut by calling
        /// AddOctaves(...) or RemoveOctaves(...).</para>
        /// </summary>
        /// <param name="index">The index in the static relativePitchHierarchies list. (Range [0..21]</param>
        /// <param name="nPitchesPerOctave">The number of different pitches in each octave. (Range [1..12])</param>
        /// <param name="basePitch">The Gamut's lowest pitch. (Range [0..11])</param>
        public Gamut(int index, int nPitchesPerOctave, int basePitch)
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

            if(basePitch < 0 || basePitch > 11)
            {
                throw new ArgumentException($"{nameof(basePitch)} out of range.");
            }

            ThrowExceptionIfRelativePitchHierarchyIsInvalid(relativePitchHierarchies[index], index);

            #endregion conditions

            List<int> rph = relativePitchHierarchies[index];
            List<int> sortedBasePitches = SortedBasePitches(rph, basePitch, nPitchesPerOctave);

            _list = new List<int>();
            int rphIndex = 0;
            int octave = 0;
            while(true)
            {
                int pitch = sortedBasePitches[rphIndex++] + (octave * 12);
                if(pitch > 127)
                {
                    break;
                }
                _list.Add(pitch);

                if(rphIndex >= sortedBasePitches.Count)
                {
                    rphIndex = 0;
                    octave++;
                }
            }

            ThrowExceptionIfGamutListIsInvalid(_list);
        }

        /// <summary>
        /// This private constructor is used by Clone.
        /// </summary>
        /// <param name="list">A valid gamut list (see class summary)</param>
        private Gamut(List<int> list)
        {
            #region conditions
            ThrowExceptionIfGamutListIsInvalid(list);
            #endregion conditions

            _list = new List<int>(list);
        }

        #region private helper functions
        /// <summary>
        ///  Throws an exception if the relativePitchHierarchy is invalid for any of the following reasons:
        ///  1. the number of values in the list is not 12.
        ///  2. the first value in the list is not 0.
        ///  3. any value is < 0 or > 11.
        ///  4. all values must be different.
        /// </summary>
        private void ThrowExceptionIfRelativePitchHierarchyIsInvalid(List<int> relativePitchHierarchy, int index)
        {
            if(relativePitchHierarchy.Count != 12)
            {
                throw new ArgumentException($"All lists in the static {nameof(relativePitchHierarchies)} must have 12 values.");
            }
            if(relativePitchHierarchy[0] != 0)
            {
                throw new ArgumentException($"All lists in the static {nameof(relativePitchHierarchies)} must begin with the value 0.");
            }

            string errorSource;
            if(index >= 0)
            {
                errorSource = $"static {nameof(relativePitchHierarchies)}[{index}].";
            }
            else
            {
                errorSource = $"{nameof(relativePitchHierarchy)}.";
            }

            for(int i = 0; i < relativePitchHierarchy.Count; ++i)
            {
                int value = relativePitchHierarchy[i];
                if(value < 0 || value > 11)
                {
                    throw new ArgumentException($"Illegal value in {errorSource}");
                }
                for(int j = i + 1; j < relativePitchHierarchy.Count; ++j)
                {
                    int value2 = relativePitchHierarchy[j];
                    if(value == value2)
                    {
                        throw new ArgumentException($"Duplicate values in {errorSource}");
                    }
                }
            }
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
            if(gamutList[0] < 0 || gamutList[0] > 127)
            {
                throw new ArgumentException($"{nameof(gamutList)}[0] is out of range.");
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
            int octaveAboveBasePitch = _list[0] + 12;
            while(pitchIndex < _list.Count && _list[pitchIndex] < octaveAboveBasePitch)
            {
                basePitches.Add(_list[pitchIndex++]); 
            }
            int pitchCount = 0;
            foreach(int pitch in basePitches)
            {
                int pitchOctave = pitch;
                while(pitchOctave < 128)
                {
                    if(!_list.Contains(pitchOctave))
                    {
                        throw new Exception($"Missing pitch in gamut list.");
                    }
                    pitchCount += 1;
                    pitchOctave += 12;
                }
            }
            if(_list.Count > pitchCount)
            {
                throw new Exception($"Unknown pitch in gamut list.");
            }
            #endregion check pitch consistency
        }

        /// <summary>
        /// The limited number of pitches in the first octave above the base pitch (in ascending order).
        /// The first value in the returned list is the basePitch.
        /// </summary>
        /// <param name="basePitch">In range [0..11]</param>
        private List<int> SortedBasePitches(List<int> relativePitchHierarchy, int basePitch, int nPitchesPerOctave)
        {
            List<int> sortedBasePitches = new List<int>();
            for(int i = 0; i < nPitchesPerOctave; ++i)
            {
                sortedBasePitches.Add(relativePitchHierarchy[i]);
            }
            sortedBasePitches.Sort();
            for(int i = 0; i < sortedBasePitches.Count; ++i)
            {
                sortedBasePitches[i] += basePitch;
            }

            return sortedBasePitches;
        }
        #endregion private helper functions

        #endregion constructors

        #region public interface
        public Gamut Clone()
        {
            return new Gamut(_list);
        }

        /// <summary>
        /// The returned list contains a list of pitches, one pitch per envelope.Original.Count.
        /// Throws an exception if firstPitch is not in the gamut.
        /// </summary>
        /// <param name="firstPitch">Will be the first pitch in the returned list.</param>
        /// <param name="envelope">envelope.Original.Count will be the length of the returned list.</param>
        /// <returns></returns>
        public List<int> PitchSequence(int firstPitch, Envelope envelope)
        {
            #region conditions
            if(!_list.Contains(firstPitch))
            {
                throw new ArgumentException($"{nameof(firstPitch)} must exist in gamut.List.");
            }
            #endregion conditions
            int firstIndexInEnvelope = envelope.Original[0]; // clone
            int indexOfFirstPitchInGamut = _list.IndexOf(firstPitch);
            int indexDiff = indexOfFirstPitchInGamut - firstIndexInEnvelope;

            List<int> indices = envelope.Original; // clone
            for(int i = 0; i < indices.Count; ++i)
            {
                indices[i] += indexDiff;
                indices[i] = (indices[i] < 0) ? 0 : indices[i];
                indices[i] = (indices[i] >= _list.Count) ? _list.Count - 1 : indices[i];
            }

            List<int> pitches = new List<int>();
            foreach(int index in indices)
            {
                pitches.Add(_list[index]);
            }

            return pitches;
        }

        /// <summary>
        /// Adds all the pitches that are pitchArg or octaves thereof.
        /// An exception is thrown if an attempt is made to add a pitch that already exists. 
        /// </summary>
        /// <param name="pitchArg">in range [0..127]</param>
        public void AddOctaves(int pitchArg)
        {
            int pitch = pitchArg % 12;
            #region conditions
            if(pitchArg < 0 || pitchArg > 127)
            {
                throw new ArgumentNullException($"{nameof(pitchArg)} must be in range [0..127].");
            }
            if(_list.Contains(pitch))
            {
                throw new ArgumentException($"{nameof(_list)} already contains pitch {pitch}.");
            }
            #endregion conditions

            List<int> newPitches = new List<int>();
            while(pitch <= 127)
            {
                newPitches.Add(pitch);
                pitch += 12;
            }

            InsertPitches(newPitches);
        }
        /// <summary>
        /// The pitchList can contain values in range [0..127] in any order.
        /// An exception is thrown if a value is already present in the Gamut, otherwise it will be inserted
        /// in the Gamut's private list at the correct position.
        /// </summary>
        /// <param name="pitchList"></param>
        private void InsertPitches(List<int> pitchList)
        {
            #region conditions
            foreach(int pitch in pitchList)
            {
                if(pitch < 0 || pitch > 127)
                {
                    throw new ArgumentNullException($"{nameof(pitchList)} can only contain values in the range [0..127].");
                }
            }
            #endregion conditions
            int plIndex = 0;
            while( plIndex < pitchList.Count)
            {
                int newPitch = pitchList[plIndex];
                if(_list.Contains(newPitch))
                {
                    throw new ArgumentException($"{nameof(_list)} already contains pitch {newPitch}.");
                }
                else
                {
                    if(newPitch > _list[_list.Count - 1])
                    {
                        _list.Add(newPitch);
                        plIndex++;
                    }
                    else
                    {
                        for(int i = 0; i < _list.Count; ++i)
                        {
                            if(_list[i] > newPitch)
                            {
                                _list.Insert(i, newPitch);
                                plIndex++;
                                break;
                            }
                        }
                    }
                }
            }

            ThrowExceptionIfGamutListIsInvalid(_list);
        }

        /// <summary>
        /// Removes all the pitches that are pitchArg or octaves thereof.
        /// An exception is thrown if an attempt is made to remove a non-existent pitch. 
        /// </summary>
        /// <param name="pitchArg">in range [0..127]</param>
        public void RemoveOctaves(int pitchArg)
        {
            int pitch = pitchArg % 12;
            #region conditions
            if(pitchArg < 0 || pitchArg > 127)
            {
                throw new ArgumentNullException($"{nameof(pitchArg)} must be in range [0..127].");
            }
            #endregion conditions

            while(pitch <= 127)
            {
                if(!(_list.Contains(pitch)))
                {
                    throw new ArgumentNullException($"Attempt to remove a non-existent pitch ({pitch}) from {nameof(_list)}.");
                }
                _list.Remove(pitch);
                pitch += 12;
            }
        }
        

        public int this[int i]
        {
            get
            {
                return _list[i];
            }
        }
        /// <summary>
        /// Returns -1 if pitch is not found.
        /// </summary>
        public int IndexOf(int pitch)
        {
            return _list.IndexOf(pitch);
        }
        public int Count
        {
            get
            {
                return _list.Count;
            }
        }
        /// <summary>
        /// A clone of the private list.
        /// </summary>
        public List<int> List { get { return new List<int>(_list); } }
        #endregion public interface

        #region private property
        private List<int> _list;
        #endregion  private property

        #region static relativePitchHierarchies 
        /// <summary>
        /// This series of relativePitchHierarchies is derived from the "most consonant" hierarchy at index 0:
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
        #endregion static relativePitchHierarchies
    }
}
