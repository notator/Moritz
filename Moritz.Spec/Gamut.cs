﻿using System;
using System.Collections.Generic;
using System.Linq;

using Moritz.Globals;

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
        /// <param name="index">The index in the static RelativePitchHierarchies list. (Range [0..21]</param>
        /// <param name="nPitchesPerOctave">The number of different pitches in each octave. (Range [1..12])</param>
        /// <param name="basePitch">The Gamut's lowest pitch. (Range [0..11])</param>
        public Gamut(int index, int nPitchesPerOctave, int basePitch)
        {
            List<int> relativePitchHierarchy = M.GetRelativePitchHierarchy(index); // checks index

            #region conditions
            if(nPitchesPerOctave < 1 || nPitchesPerOctave > relativePitchHierarchy.Count)
            {
                throw new ArgumentException($"{nameof(nPitchesPerOctave)} out of range.");
            }

            if(basePitch < 0 || basePitch > 11)
            {
                throw new ArgumentException($"{nameof(basePitch)} out of range.");
            }
            #endregion conditions

            List<int> sortedBasePitches = SortedBasePitches(relativePitchHierarchy, basePitch, nPitchesPerOctave);

            SetGamutList(sortedBasePitches);
        }

        public Gamut(List<int> absolutePitchHierarchy, int nPitchesPerOctave)
        {
            List<int> sortedBasePitches = new List<int>();
            for(int i = 0; i < nPitchesPerOctave; ++i)
            {
                sortedBasePitches.Add(absolutePitchHierarchy[i]);
            }
            sortedBasePitches.Sort();

            SetGamutList(sortedBasePitches);
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
        private void SetGamutList(List<int> sortedBasePitches)
        {
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
    }
}