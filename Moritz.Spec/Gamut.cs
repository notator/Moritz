using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using Moritz.Globals;

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
        #region constructor
        /// <summary>
        /// Gamuts are immutable!
        /// </summary>
        public Gamut(List<int> absolutePitchHierarchy, int nPitchesPerOctave)
        {
            #region condition
            ThrowExceptionIfPitchHierarchyIsInvalid(absolutePitchHierarchy);
            #endregion condition

            _absolutePitchHierarchy = new List<int>(absolutePitchHierarchy);
            _nPitchesPerOctave = nPitchesPerOctave;

            _list = GetGamutList(_absolutePitchHierarchy, _nPitchesPerOctave);
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

        #endregion constructor

        /// <summary>
        /// Returns the conjugate Gamut.
        /// This is the Gamut whose AbsolutePitchHierachy is inverted re this Gamut
        /// </summary>
        /// <returns></returns>
        internal Gamut Conjugate()
        {
            List<int> conjugateAbsolutePitchHierachy = GetConjugateAbsolutePitchHierarchy();
            Gamut conjugateGamut = new Gamut(conjugateAbsolutePitchHierachy, NPitchesPerOctave);
            return conjugateGamut; 
        }
        private List<int> GetConjugateAbsolutePitchHierarchy()
        {
            List<int> pitchHierarchy = AbsolutePitchHierarchy; // a clone
            int rootPitch = pitchHierarchy[0];
            for(int i = 0; i < pitchHierarchy.Count; ++i)
            {
                pitchHierarchy[i] -= rootPitch;
            }
            // pitchHierachy[0] is now 0.
            // invert the pitchHierarchy
            for(int i = 0; i < pitchHierarchy.Count; ++i)
            {
                pitchHierarchy[i] *= -1;
            }
            // reset the rootPitch
            for(int i = 0; i < pitchHierarchy.Count; ++i)
            {
                pitchHierarchy[i] += rootPitch;
            }
            // normalize the pitchHierachy
            for(int i = 0; i < pitchHierarchy.Count; ++i)
            {
                pitchHierarchy[i] = (pitchHierarchy[i] < 0) ? pitchHierarchy[i] + 12 : pitchHierarchy[i];
                pitchHierarchy[i] = (pitchHierarchy[i] > 11) ? pitchHierarchy[i] - 12 : pitchHierarchy[i];
            }
            ThrowExceptionIfPitchHierarchyIsInvalid(pitchHierarchy);

            return pitchHierarchy;
        }

        #region public functions
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
        internal bool ContainsAllPitches(List<byte> pitches)
        {
            bool containsAllPitches = true;
            foreach(byte pitch in pitches)
            {
                if(_list.Contains(pitch) == false)
                {
                    containsAllPitches = false;
                    break;
                }
            }
            return containsAllPitches;
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
                    while(pitch <= pitches[pitches.Count - 1])
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

        /// <summary>
        /// The returned list contains 12 velocity values in range [0..127].
        /// Pitches in the gamut are given velocities in range [1..127]. Other pitches are given velocity = 1.
        /// The returned values are in order of absolute pitch, with C natural (absolute pitch 0) at position 0,
        /// C# (absolute pitch 1) at position 1, etc.
        /// The pitch at AbsolutePitchHierarchy[0] is given velocity=127.
        /// The pitch at AbsolutePitchHierarchy[NPitchesPerOctave - 1] is given minimumVelocity.
        /// The velocities in between are scaled linearly or logarithmically, depending on the value of the second argument.
        /// </summary>
        /// <param name="minimumVelocity">In range [1..127]. The velocity to be given to the pitch at AbsolutePitchHierarchy[NPitchesPerOctave - 1]</param>
        public List<byte> GetVelocityPerAbsolutePitch(int minimumVelocity, bool isLinearGradient)
        {
            Debug.Assert(minimumVelocity > 0 && minimumVelocity < 128);

            List<double> velocities = new List<double>();
            if(NPitchesPerOctave == 1)
            {
                velocities.Add(127);
            }
            else if(isLinearGradient)
            {
                double velocityDiff = ((double)(127 - minimumVelocity)) / (NPitchesPerOctave - 1);
                for(int i = 0; i < NPitchesPerOctave; ++i)
                {
                    double vel = 127 - (i * velocityDiff);
                    velocities.Add(vel);
                }
            }
            else
            {
                double factor = ((double) Math.Pow(((double)minimumVelocity / 127), (((double)1) / (NPitchesPerOctave - 1))));
                velocities.Add(127);
                for(int i = 1; i < NPitchesPerOctave; ++i)
                {
                    double vel = velocities[i - 1] * factor;
                    velocities.Add(vel);
                }
            }

            List<byte> velocityPerAbsPitch = new List<byte>();
            for(int i = 0; i < 12; ++i)
            {
                velocityPerAbsPitch.Add(1); // default value
            }

            for(int absPitchIndex = 0; absPitchIndex < NPitchesPerOctave ; ++absPitchIndex)
            {
                int absPitch = AbsolutePitchHierarchy[absPitchIndex];

                byte velocity = (byte) Math.Round(velocities[absPitchIndex]);
                velocity = (velocity > 1) ? velocity : (byte)1;
                velocity = (velocity <= 127) ? velocity : (byte)127;

                velocityPerAbsPitch[absPitch] = velocity;
            }
            return velocityPerAbsPitch;
        }

        #endregion public functions

        #region public properties
        public int NPitchesPerOctave { get { return _nPitchesPerOctave; } }
        public int _nPitchesPerOctave;

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
    }
}
