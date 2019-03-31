using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Krystals4ObjectLibrary;
using Moritz.Globals;

namespace Moritz.Spec
{
	public class Mode
    {
		#region constructors
		/// <summary>
		/// A Mode is an immutable class, containing a list of absolute pitches (C=0, C#=1, D=2 etc.)
		/// in order of importance, whereby not all absolute pitches need to be included.
		/// Modes can used, for example, to determine the loudness of particular pitches.
		/// A Mode.Gamut is a list of absolute pitch numbers in an ascending order scale, whereby all the values
		/// are different and in range [0..127].
		/// <para>Mode.Gamut[0] == absolutePitchHierarchy[0] % 12 (restricted to range [0..11]).</para>
		/// <para>Each absolute pitch in the Mode exists at all possible octaves above Mode.Gamut[0].
		/// (So each octave range in the gamut contains the same absolute pitches.)</para>
		/// </summary>
		/// <param name="relativePitchHierarchyIndex">Will be treated % 22 (Mode.RelativePitchHierarchiesCount)</param>
		/// <param name="basePitch">Will be treated % 12</param>
		/// <param name="nPitchesPerOctave">Will be treated % 13</param>
		public Mode(int relativePitchHierarchyIndex, int basePitch, int nPitchesPerOctave)
        {
            relativePitchHierarchyIndex %= Mode.RelativePitchHierarchiesCount;
            basePitch %= 12;
            nPitchesPerOctave %= 13;

            SetAbsolutePitchHierarchy(relativePitchHierarchyIndex, basePitch);

			_gamut = null; // will be lazily evaluated
            
            RelativePitchHierarchyIndex = relativePitchHierarchyIndex;
            BasePitch = basePitch;
            NPitchesPerOctave = nPitchesPerOctave;
        }
		#endregion constructors

		#region public functions
		/// <summary>
		/// Modes are equal if their AbsolutePitchHierarchies are identical.
		/// </summary>
		public bool HasSameAbsolutePitchHierarchy(Mode otherMode)
		{
			bool equals = true;
			IReadOnlyList<int> absH = this.AbsolutePitchHierarchy;
			IReadOnlyList<int> otherAbsH = otherMode.AbsolutePitchHierarchy;
			int count = absH.Count;

			if(count != otherAbsH.Count)
			{
				equals = false;
			}
			for(int i = 0; i < count; i++)
			{
				if(absH[i] != otherAbsH[i])
				{
					equals = false;
					break;
				}
			}
			return equals;
		}

		/// <summary>
		/// Returns true if the mode.List contains all the pitches in the argument. Otherwise false. 
		/// </summary>
		public bool ContainsAllPitches(MidiChordDef mcd)
        {
            foreach(BasicMidiChordDef bmcd in mcd.BasicDurationDefs)
            {
                for(int i = 0; i < bmcd.Pitches.Count; ++i)
                {
                    if(this.Gamut.Contains(bmcd.Pitches[i]) == false)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Returns -1 if pitch is not found.
        /// </summary>
        public int IndexInGamut(int pitch)
        {
			if(_gamut == null)
			{
				IReadOnlyList<int> gamut = Gamut; // evaluates _gamut
			}
			Debug.Assert(_gamut != null && _gamut.Count > 0);

            return _gamut.IndexOf(pitch);
        }

        /// <summary>
        /// The returned list contains a list of pitches, one pitch per envelope.Original.Count.
        /// Throws an exception if firstPitch is not in the mode.List.
        /// Pitches that would be lower or higher than any pitch in the mode are silently coerced to
        /// the lowest or highest values respectively.
        /// </summary>
        /// <param name="firstPitch">Will be the first pitch in the returned list.</param>
        internal List<int> PitchSequence(int firstPitch, Envelope envelope)
        {
            Debug.Assert(_gamut.Contains(firstPitch), $"{nameof(firstPitch)} is not in mode.");

            List<int> pitchSequence = new List<int>();
            if(envelope == null)
            {
                pitchSequence.Add(firstPitch);
            }
            else
            {
                List<int> envOriginal = envelope.Original; // clone
                int firstIndexInEnvelope = envOriginal[0];
                int indexOfFirstPitchInMode = IndexInGamut(firstPitch);
                int indexDiff = indexOfFirstPitchInMode - firstIndexInEnvelope;

                List<int> indices = envOriginal;
				int gamutCount = Gamut.Count;

				for(int i = 0; i < indices.Count; ++i)
                {
                    indices[i] += indexDiff;
                    indices[i] = (indices[i] < 0) ? 0 : indices[i];
                    indices[i] = (indices[i] >= gamutCount) ? gamutCount - 1 : indices[i];
                }

                foreach(int index in indices)
                {
                    pitchSequence.Add(_gamut[index]);
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
            Debug.Assert(Gamut.Contains(rootPitch));

            List<int> pitches = new List<int>() { rootPitch };

            if(nPitches > 1)
            {
                int absRootPitch = rootPitch % 12;
				var absolutePitchHierarchy = new List<int>(AbsolutePitchHierarchy);
				int rootIndex = absolutePitchHierarchy.IndexOf(absRootPitch);
                int maxIndex = rootIndex + nPitches;
                maxIndex = (maxIndex < NPitchesPerOctave) ? maxIndex : NPitchesPerOctave;
                for(int i = rootIndex + 1; i < maxIndex; ++i)
                {
                    int pitch = absolutePitchHierarchy[i];
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

		private static void AssertVelocityPerAbsolutePitchValidity(IReadOnlyList<byte> velocityPerAbsolutePitch)
		{
			Debug.Assert(velocityPerAbsolutePitch.Count == 12);
			
			foreach(byte velocity in velocityPerAbsolutePitch)
			{
				M.AssertIsVelocityValue(velocity);
			}
		}

		/// <summary>
		/// Returns a new DefaultVelocityPerAbsolutePitch list containing 12 velocity values in range [1..127].
		/// These can be applied to pitches in any Trk or MidiChordDef (using their SetVelocityPerAbsolutePitch(...) ) function,
		/// regardless of the mode from which they have been constructed.
		/// Pitches that are part of *this* mode in the returned list are given velocities in range [1..127].
		/// Other pitches are given velocity = 1.
		/// The returned Velocity values are:
		/// 1. in range [1..127].
		/// 2. in a linear progression per AbsolutePitchHierarchy value.
		/// 3. sorted, so that the velocity for C natural (absolute pitch 0) is at position 0, C# (absolute pitch 1) is at position 1, etc.
		/// </summary>
		public List<byte> GetDefaultVelocityPerAbsolutePitch()
		{
			const int maximumVelocity = 127;
			const int minimumVelocity = 1;
			List<double> velocities = new List<double>();
			if(NPitchesPerOctave == 1)
			{
				velocities.Add(maximumVelocity);
			}
			else // linear gradient
			{
				double velocityDiff = ((double)(maximumVelocity - minimumVelocity)) / (NPitchesPerOctave - 1);
				for(int i = 0; i < NPitchesPerOctave; ++i)
				{
					double vel = maximumVelocity - (i * velocityDiff);
					velocities.Add(vel);
				}
			}

			List<byte> velocityPerAbsPitch = new List<byte>();
			for(int i = 0; i < 12; ++i)
			{
				velocityPerAbsPitch.Add((byte)minimumVelocity); // default value 1
			}

			for(int absPitchIndex = 0; absPitchIndex < NPitchesPerOctave; ++absPitchIndex)
			{
				int absPitch = AbsolutePitchHierarchy[absPitchIndex];

				byte velocity = (byte)Math.Round(velocities[absPitchIndex]);
				velocity = M.VelocityValue(velocity);
				velocityPerAbsPitch[absPitch] = velocity;
			}
			return velocityPerAbsPitch;
		}

		/// <summary>
		/// Returns a new VelocityPerAbsolutePitch list containing 12 velocity values in range [1..127].
		/// If power greater than 0, weakens the higher velocities less than the lower velocities.
		/// If power less than 0, strengthens the lower velocities more than the higher velocities.
		/// If power equals 0, leaves the velocities unchanged.
		/// </summary>
		/// <param name="velocityPerAbsolutePitch">A list of 12 velocity values (range [1..127] in order of absolute pitch</param>
		/// <param name="power">In range [-1..1]. (1 gives maximum emphasis to high velocities.)</param>
		public static List<byte> SetVelocityPerAbsolutePitchGradient(IReadOnlyList<byte> velocityPerAbsolutePitch, double power)
		{
			AssertVelocityPerAbsolutePitchValidity(velocityPerAbsolutePitch);
			Debug.Assert(power >= -1 && power <= 1);

			List<byte> rval = new List<byte>();
			for(int i = 0; i < velocityPerAbsolutePitch.Count; i++)
			{
				byte velocity = velocityPerAbsolutePitch[i];
				double factor = Math.Pow(((double)velocity / 127), power);
				byte newVelocity = (byte)Math.Round(velocity * factor);
				newVelocity = M.VelocityValue(newVelocity);
				rval.Add(newVelocity);
			}

			AssertVelocityPerAbsolutePitchValidity(rval);

			return rval;
		}

		/// <summary>
		/// Returns a new VelocityPerAbsolutePitch list containing 12 velocity values in range [1..127].
		/// Spreads the current range of the velocityPerAbsolutePitch list over the range [minVelocity..maxVelocity].
		/// If the all the values in the input list	are the same, they are changed to the value of maxVelocity.
		/// </summary>
		public static List<byte> SetVelocityPerAbsolutePitchRange(IReadOnlyList<byte> velocityPerAbsolutePitch, byte minVelocity, byte maxVelocity)
		{
			AssertVelocityPerAbsolutePitchValidity(velocityPerAbsolutePitch);
			Debug.Assert(minVelocity <= maxVelocity);

			List<byte> rval = new List<byte>();
			byte minVel = byte.MaxValue;
			byte maxVel = byte.MinValue;
			foreach(byte value in velocityPerAbsolutePitch)
			{
				minVel = (minVel < value) ? minVel : value;
				maxVel = (maxVel > value) ? maxVel : value;
			}
			if(maxVel == minVel)
			{
				for(int i = 0; i < velocityPerAbsolutePitch.Count; i++)
				{
					rval.Add(maxVelocity);
				} 
			}
			double factor = ((double)maxVelocity - minVelocity) / (maxVel - minVel);
			for(int i = 0; i < velocityPerAbsolutePitch.Count; i++)
			{
				byte newVelocity = (byte)Math.Round(minVelocity + ((velocityPerAbsolutePitch[i] - minVel) * factor));
				newVelocity = M.VelocityValue(newVelocity);
				rval.Add(newVelocity);
			}
			AssertVelocityPerAbsolutePitchValidity(rval);
			return rval;
		}

		/// <summary>
		/// Returns 264 ModeProximity objects containing all the possible Modes
		/// with their proximities to this Mode. The returned list is sorted by proximity.
		/// </summary>
		/// <returns></returns>
		public List<ModeProximity> GetModeProximities()
		{
			var rval = new List<ModeProximity>();

			for(int modeRPHIndex = 0; modeRPHIndex < Mode.RelativePitchHierarchiesCount; ++modeRPHIndex)
			{
				for(int modeRPHBasePitch = 0; modeRPHBasePitch < 12; ++modeRPHBasePitch)
				{
					Mode mode2 = new Mode(modeRPHIndex, modeRPHBasePitch, this.NPitchesPerOctave);
					ModeProximity modeProximity = GetModeProximity(mode2);
					rval.Add(modeProximity);
				}
			}

			for(int i = 0; i < rval.Count; ++i)
			{
				rval.Sort((a, b) => a.Proximity.CompareTo(b.Proximity));
			}

			return rval;
		}

		public override string ToString()
		{
			const string nums = "0123456789AB";
			StringBuilder aph = new StringBuilder();
			foreach(int i in AbsolutePitchHierarchy)
			{
				aph.Append(nums[i]);
			}
		
			return $"AbsPitchHierarchy={aph.ToString()}, rphIndex={RelativePitchHierarchyIndex}, basePitch={BasePitch}, nPitchesPerOctave={NPitchesPerOctave}";
		}

		#endregion public functions

		#region public properties
		public static int RelativePitchHierarchiesCount { get { return RelativePitchHierarchies.Count; } }
        public readonly int RelativePitchHierarchyIndex;
        public readonly int BasePitch;
        public readonly int NPitchesPerOctave;

		public int MaxGamutPitch
		{
			get
			{
				if(_maxGamutPitch > 0)
				{
					return _maxGamutPitch;
				}
				// lazy evaluation
				int topBasePitch = BasePitch;
				while(topBasePitch < 115)
				{
					topBasePitch += 12;
				}
				List<int> topOctavePitches = new List<int>();
				List<int> relativePitchHierarchy = RelativePitchHierarchies[this.RelativePitchHierarchyIndex];
				for(int pitchAbove = 0; pitchAbove < NPitchesPerOctave; ++pitchAbove)
				{
					// topOctavePitches can be > 127.
					topOctavePitches.Add(topBasePitch + relativePitchHierarchy[pitchAbove]);
				}
				foreach(int pitch in topOctavePitches)
				{
					_maxGamutPitch = (_maxGamutPitch < pitch && pitch <= 127) ? pitch : _maxGamutPitch;
				}
				return _maxGamutPitch;
			}
		}
		private int _maxGamutPitch = 0;
		public IReadOnlyList<int> AbsolutePitchHierarchy { get; private set; }
        public IReadOnlyList<int> Gamut
		{
			get
			{
				if(_gamut != null)
				{
					return _gamut as IReadOnlyList<int>;
				}
				// lazy evaluation
				int rootPitch = AbsolutePitchHierarchy[0];

				List<int> sortedBasePitches = new List<int>();
				for(int i = 0; i < NPitchesPerOctave; ++i)
				{
					sortedBasePitches.Add(AbsolutePitchHierarchy[i]);
				}
				sortedBasePitches.Sort();

				_gamut = new List<int>();
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
						_gamut.Add(pitch);
					}

					if(rphIndex >= sortedBasePitches.Count)
					{
						rphIndex = 0;
						octave++;
					}
				}

				AssertGamutValidity();

				Debug.Assert(MaxGamutPitch == _gamut[_gamut.Count - 1]);

				return _gamut as IReadOnlyList<int>;
			}
		}

        private List<int> _gamut = null;

		#endregion public properties

		#region private helper functions
		private ModeProximity GetModeProximity(Mode otherMode)
		{
			Debug.Assert(this.NPitchesPerOctave == otherMode.NPitchesPerOctave);

			int proximity = GetProximity(otherMode);
			ModeProximity rval = new ModeProximity(otherMode, proximity);

			return rval;
		}

		private int GetProximity(Mode otherMode)
		{
			int proximity = 0;
			var thisAbsolutePitchHierarchy = new List<int>(this.AbsolutePitchHierarchy);
			var otherAbsolutePitchHierarchy = new List<int>(otherMode.AbsolutePitchHierarchy);
			// N.B. test ALL pitches in the hierarchies, not just the first NPitchesPerOctave in thisAbsolutePitchHierarchy.
			for(int index1 = 0; index1 < 12; ++index1)
			{
				int pitch = thisAbsolutePitchHierarchy[index1];
				int index2 = otherAbsolutePitchHierarchy.FindIndex(a => a == pitch);
				int minIndex = (index1 <= index2) ? index1 : index2;
				int maxIndex = (index1 > index2) ? index1 : index2;

				int p = (maxIndex - minIndex + 1) * (minIndex + maxIndex);

				proximity += p;
			}
			return proximity;
		}

		/// <summary>
		/// A pitchHierarchy.Count must be 12.
		/// Each value must be in range [0..11] and occur only once (no duplicates).
		/// </summary>
		private void AssertAbsolutePitchHierarchyValidity()
		{
			Debug.Assert(AbsolutePitchHierarchy.Count == 12);
			List<bool> presence = new List<bool>();
			for(int i = 0; i < 12; ++i)
			{
				presence.Add(false);
			}

			foreach(int value in AbsolutePitchHierarchy)
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

		/// <summary>
		/// Throws an exception if _gamut is invalid for any of the following reasons:
		/// 1. _gamut is null or empty.
		/// 2. All the values must be different, in ascending order, and in range [0..127].
		/// 3. Each absolute pitch exists at all possible octaves above the base pitch.
		/// </summary>
		private void AssertGamutValidity()
		{
			Debug.Assert(_gamut != null && _gamut.Count > 0, $"{nameof(_gamut)} is null or empty.");
			Debug.Assert(_gamut[0] % 12 == AbsolutePitchHierarchy[0], $"The lowest pitch in {nameof(_gamut)} must always be equal to {nameof(AbsolutePitchHierarchy)}[0].");

			for(int i = 1; i < _gamut.Count; ++i)
			{
				Debug.Assert(_gamut[i] >= 0 && _gamut[i] <= 127, $"{nameof(_gamut)}[{i}] is out of range.");
				Debug.Assert(_gamut[i] > _gamut[i - 1], $"{nameof(_gamut)} values must be in ascending order.");
			}

			#region check pitch consistency
			List<int> basePitches = new List<int>();
			int pitchIndex = 0;
			int octaveAboveBasePitch = _gamut[0] + 12;
			while(pitchIndex < _gamut.Count && _gamut[pitchIndex] < octaveAboveBasePitch)
			{
				basePitches.Add(_gamut[pitchIndex++]);
			}
			int pitchCount = 0;
			foreach(int pitch in basePitches)
			{
				int pitchOctave = pitch;
				while(pitchOctave < 128)
				{
					Debug.Assert(_gamut.Contains(pitchOctave), $"Missing pitch in {nameof(_gamut)}");
					pitchCount += 1;
					pitchOctave += 12;
				}
			}
			Debug.Assert(_gamut.Count == pitchCount, $"Unknown pitch in {nameof(_gamut)}.");
			#endregion check pitch consistency
		}
		#endregion private helper functions

		#region Pitch Hierarchies
		/// <summary>
		/// Sets AbsolutePitchHierarchy to contain the sums of absoluteValue(rootPitch) + RelativePitchHierarchies[index].
		/// If a value would be greater than 11, value = value - 12, so that all values are in range [0..11].
		/// </summary>
		/// <param name="relativePitchHierarchyIndex">In range [0..21]</param>
		/// <param name="rootPitch">In range [0..127]</param>
		private void SetAbsolutePitchHierarchy(int relativePitchHierarchyIndex, int rootPitch)
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

			AbsolutePitchHierarchy = absolutePitchHierarchy;

			#region condition
			AssertAbsolutePitchHierarchyValidity();
			#endregion condition
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

	/// <summary>
	/// This class implements the "pitch class set" defined by Allen Forte in "The Structure of Atonal Music".
	/// In that book, a "pitch class set" must contain between 3 and 9 pitch classes.
	/// </summary>
	public class PitchClassSet
	{
		/// <summary>
		/// The argument is cloned before being used.
		/// The NormalForm, PrimeForm, PrimeInversionForm and BestPrimeForm attributes are set.
		/// The BestPrimeForm is the "best" of either PrimeForm or PrimeInversionForm.
		/// "best" is defined by Allen Forte's Algorithm, coded in the GetBestForm(form1, form2) function in this class. 
		/// </summary>
		/// <param name="pitches">Any number of unordered, unrestricted integers.</param>
		public PitchClassSet(IReadOnlyList<int> pitches)
		{
			NormalForm = GetPitchClasses(pitches);
			PrimeForm = GetPrimeForm(NormalForm);
			PrimeInversionForm = GetPrimeInversionForm(PrimeForm);
			BestPrimeForm = GetBestForm(PrimeForm, PrimeInversionForm);
		}

		/// <summary> 
		/// Returns a HashSet containing the pitch classes present in the pitchesArg argument.
		/// The argument can contain any number of positive ints (including 0).
		/// Duplicate pitch classes in the argument will be silently ignored.
		/// An exception will be thrown if the argument contains negative values or
		/// if the returned list would have less than 3 or more than 9 values.
		/// </summary>
		/// <param name="pitchesArg">Any number of positive ints representing pitches. Duplicate pitch classes will be ignored.</param>
		private HashSet<int> GetPitchClasses(IReadOnlyList<int> pitchesArg)
		{
			#region precondition
			for(int i = 0; i < pitchesArg.Count; ++i)
			{
				if(pitchesArg[i] < 0)
				{
					throw new ApplicationException();
				}
			}
			#endregion

			var pitches = new List<int>(pitchesArg);

			for(int i = 0; i < pitches.Count; ++i)
			{
				pitches[i] %= 12;
			}

			var pitchClassSet = new HashSet<int>();
			foreach(int val in pitches)
			{
				if(!pitchClassSet.Contains(val))
				{
					pitchClassSet.Add(val);
				}
			}

			#region postcondition
			CheckNormalConsistency(pitchClassSet);
			#endregion

			return pitchClassSet;
		}

		/// <summary>
		/// Returns either the list1 or list2 depending on which is "best" according to Forte's algorithm (p.4).
		/// Both lists must be the same length and in ascending order.
		/// </summary>
		/// <param name="list1">Can be a rotated list of pitches containing values greater than 11</param>
		/// <param name="list2">Can be a rotated list of pitches containing values greater than 11</param>
		/// <returns></returns>
		private IReadOnlyList<int> GetBestForm(IReadOnlyList<int> list1, IReadOnlyList<int> list2)
		{
			int count = list1.Count;
			#region preconditions
			if(count != list2.Count)
			{
				throw new ApplicationException();
			}
			for(int i = 1; i < count; i++)
			{
				if((list1[i - 1] >= list1[i]) || (list2[i - 1] >= list2[i]))
				{
					throw new ApplicationException();
				}
			}
			#endregion

			IReadOnlyList<int> rval = list1; // default

			int firstForm1 = list1[0];
			int firstForm2 = list2[0];
			int diff = (list1[count-1] - firstForm1) - (list2[count-1] - firstForm2);
			if(diff == 0)
			{
				for(int i = 1; i < count; ++i)
				{
					diff = (list1[i] - firstForm1) - (list2[i] - firstForm2);
					if(diff < 0)
					{
						break;
					}
					else if(diff > 0)
					{
						rval = list2;
						break;
					}
				}

			}
			else if(diff > 0)
			{
				rval = list2;
			}

			return rval;
		}

		/// <summary>
		/// See "The Structure of Atonal Music" page 4.
		/// The returned list contains the "best" rotation of the argument's pitch classes.
		/// The values in the returned list are in ascending order, and may be in any octave.
		/// </summary>
		/// <param name="normalForm">An ordered list of between 3 and 9 unique pitch classes in range [0..11]</param>
		/// <returns>The "best" rotation (according to Forte's algorithm).</returns>
		private IReadOnlyList<int> GetBestRotation(HashSet<int> normalForm)
		{
			#region get rotations
			// pitch classes are in range [0..11]
			// absolute pitches are pitch classes + 12 per octave.
			// rotations contain absolute pitches in ascending order
			List<int> rotation1 = new List<int>(normalForm);
			rotation1.Sort();
			List<List<int>> rotations = new List<List<int>>();
			rotations.Add(rotation1);

			for(int i = 1; i < rotation1.Count; ++i)
			{
				var prevRotation = rotations[i - 1];
				var rotation = new List<int>();
				for(int j = 1; j < prevRotation.Count; ++j)
				{
					rotation.Add(prevRotation[j]);
				}
				int absPitch = prevRotation[0];
				while(absPitch < rotation[rotation.Count-1])
				{
					absPitch += 12;
				}
				rotation.Add(absPitch);
				rotations.Add(rotation);
			}
			#endregion get rotations

			IReadOnlyList<int> rval = rotations[0];
			for(int i = 1; i < rotations.Count; ++i)
			{
				rval = GetBestForm(rval, rotations[i]);
			}

			return rval;
		}

		/// <summary>
		/// The PrimeForm is created by finding the "best" rotation of the normalForm (according to Forte's algorithm for "best"),
		/// then subtracting the first value in bestRotation from each value, and adding 12 when/if the result is negative.
		/// The result is a list in which the first value is 0, the values are in ascending order, and the intervallic
		/// relations in bestRotation are preserved.
		/// </summary>
		private IReadOnlyList<int> GetPrimeForm(HashSet<int> normalForm)
		{
			IReadOnlyList<int> bestRotation = GetBestRotation(normalForm);

			var primeForm = new List<int>();
			int first = bestRotation[0];
			foreach(int val in bestRotation)
			{
				int baseVal = val - first;
				while(baseVal < 0)
				{
					baseVal += 12;
				}
				primeForm.Add(baseVal);
			}
			
			return primeForm;
		}

		private IReadOnlyList<int> GetPrimeInversionForm(IReadOnlyList<int> primeForm)
		{
			var inversion = new HashSet<int>();
			for(int i = 0; i < primeForm.Count; ++i)
			{
				inversion.Add((12 - primeForm[i]) % 12);
			}

			IReadOnlyList<int> primeInversion = GetPrimeForm(inversion);

			return primeInversion;
		}

		/// <summary>
		/// This function ensures that
		/// 1. the argument contains between 3 and 9 integers,
		/// 2. the values are in range [0..11] (in any order)
		/// </summary>
		private void CheckNormalConsistency(HashSet<int> normalForm)
		{
			if(normalForm == null || normalForm.Count < 3 || normalForm.Count > 9)
			{
				throw new ApplicationException("Allen Forte's pitch class sets contain between 3 and 9 pitch classes.");
			}
			foreach(int v in normalForm)
			{
				if(v < 0 || v > 11)
				{
					throw new ApplicationException();
				}
			}
		}

		/// <summary>
		/// This function is used when checking assignments to PrimeForm, PrimeInversion and BestPrime.
		/// It ensures that
		/// 1. the primeList contains between 3 and 9 integers,
		/// 2. the values are in range [0..11]
		/// 3. the values are in ascending order.
		/// 4. the first value in the primeList is 0
		/// </summary>
		private void CheckPrimeConsistency(IReadOnlyList<int> primeList)
		{
			if(primeList == null || primeList.Count < 3 || primeList.Count > 9)
			{
				throw new ApplicationException("Allen Forte's pitch class sets contain between 3 and 9 pitch classes.");
			}
			foreach(int v in primeList)
			{
				if(v < 0 || v > 11)
				{
					throw new ApplicationException();
				}
			}
			for(int i = 1; i < primeList.Count; i++)
			{
				if(primeList[i - 1] >= primeList[i])
				{
					throw new ApplicationException();
				}
			}
			if(primeList[0] != 0)
			{
				throw new ApplicationException();
			}
		}

		/// <summary>
		/// NormalForm contains a HashSet of between 3 and 9 integers in range [0..11] (C=0, C#=1 etc.)
		/// A HashSet is eqivalent to a mathematical "set", and has corresponding set functions.
		/// </summary>
		public HashSet<int> NormalForm
		{
			get
			{
				return new HashSet<int>(_normalForm);
			}
			private set
			{
				CheckNormalConsistency(value);
				_normalForm = value;
			}
		}
		private HashSet<int> _normalForm;

		/// <summary>
		/// PrimeForm is the "best" rotation of a NormalForm, transposed so that PrimeForm[0] is 0.
		/// Rotations are in ascending order, so PrimeForm is too.
		/// </summary>
		public IReadOnlyList<int> PrimeForm
		{
			get
			{
				return new List<int>(_primeForm);
			}
			private set
			{
				CheckPrimeConsistency(value);
				_primeForm = value;
			}
		}
		private IReadOnlyList<int> _primeForm;

		/// <summary>
		/// PrimeInversionForm is the "best" rotation of PrimeForm's inversion, transposed so that PrimeInversionForm[0] is 0.
		/// Rotations are in ascending order, so PrimeInversionForm is too.
		/// </summary>
		public IReadOnlyList<int> PrimeInversionForm
		{
			get { return new List<int>(_primeInversionForm); }
			private set
			{
				CheckPrimeConsistency(value);
				_primeInversionForm = value;
			}
		}
		private IReadOnlyList<int> _primeInversionForm;

		/// <summary>
		/// BestPrimeForm is the "best" of either PrimeForm or PrimeInvertedForm.
		/// BestPrimeForm is the pitch class set used in Forte's Appendix 1.
		/// </summary>
		public IReadOnlyList<int> BestPrimeForm
		{
			get { return new List<int>(_bestPrimeForm); }
			private set
			{
				CheckPrimeConsistency(value);
				_bestPrimeForm = value;
			}
		}
		private IReadOnlyList<int> _bestPrimeForm;
	}
}
