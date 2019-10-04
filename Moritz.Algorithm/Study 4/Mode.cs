using System;
using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;
using Moritz.Globals;
using Moritz.Palettes;
using Moritz.Spec;
using Moritz.Symbols;

namespace Moritz.Algorithm.Study4
{
	/// <summary>
	/// A Study4.Mode is an immutable class, containing:
	///    1. AbsolutePitchWeightDict: an IReadOnlyDictionary whose KeyValuePairs contain
	///       Key: an absolute pitch (in range [0..11] C=0, C#=1, D=2 etc.) and
	///       Value: a weight (in range [0..127]).
	///    2. AbsolutePitches: an IReadonlyList of absolute pitches (in range [0..11]) in order of weight.
	///    3. Gamut: a list of unique absolute pitch numbers in an ascending order scale (range [0..127]).
	///       Each absolute pitch in the Mode exists at all possible octaves above Mode.Gamut[0].
	///       (Each octave range in the gamut contains the same absolute pitches.)
	/// <para>Mode.Gamut[0] == Mode.AbsolutePitches[0] (range [0..11]).</para> 
	/// Not all absolute pitches need to be included in the Mode.
	/// The pitches' weights can be used, for example, to determine their relative velocities, durations etc.
	/// </summary>
	public class Mode
	{
		#region constructors
		
		/// <param name="absPitchWeightDict">Keys must be in range [0..11], Values must be in range [0..127], Count must be in range [1..12]</param>
		public Mode(Dictionary<int,int> absPitchWeightDict)
		{
			Debug.Assert(absPitchWeightDict.Count >= 1 && absPitchWeightDict.Count <= 12);

			foreach(var pitchWeight in absPitchWeightDict)
			{
				var pitch = pitchWeight.Key;
				var weight = pitchWeight.Value;
				Debug.Assert(pitch >= 0 && pitch <= 11);
				Debug.Assert(weight >= 0 && weight <= 127);
			}

			_absolutePitchWeightDict = new Dictionary<int, int>(absPitchWeightDict);
			_absolutePitchHierarchy = GetAbsPitchesSortedbyWeight(absPitchWeightDict);
			// Gamut is constructed lazily.
		}

		/// <summary>
		/// Pitches that have the same weight are returned in the unpredictable order returned by the Dictionary.
		/// </summary>
		private List<int> GetAbsPitchesSortedbyWeight(IReadOnlyDictionary<int, int> absPitchWeightDict)
		{
			List<int> weights = new List<int>();
			foreach(var kv in absPitchWeightDict)
			{
				weights.Add(kv.Value);
			}
			weights.Sort(); // small->large
			weights.Reverse(); // large->small

			List<int> absPitches = new List<int>();

			for(int i = 0; i < weights.Count; ++i)
			{
				int weight = weights[i];
				foreach(var kv in absPitchWeightDict)
				{
					if(kv.Value == weight && !absPitches.Contains(kv.Key))
					{
						absPitches.Add(kv.Key);
						break;
					}
				}
			}

			return absPitches;
		}

		/// <summary>
		/// Returns a list of Study4.Modes that begins with this Study4.Mode and moves towards
		/// the targetMode argument (which is not included in the returned list).
		/// The transformation is achieved by connecting the pitches in the pitchVectors using
		/// minimum (semitone) intervals per step.
		/// The targetMode can have different pitches, and/or a different pitchHierarchy and/or
		/// a different _number_ of pitches from this Study4.Mode.
		/// Each entry in the pitchVectors is an absolute pitch in this Study4.Mode, followed by an
		/// absolute pitch in the targetMode. Both duplicates and ommisions are allowed:
		///     1.  If not all of this Study4.Mode's pitches are present, there will be no corresponding
		///         pitchVector connecting the missing pitch to a pitch in the target.
		///     1a. If a start pitch is present that is not in this Study4.Mode, that start pitch's
		///         vector will start immediately after this Study4.Mode in the returned list, as if the
		///         start pitch has a weight of zero at the beginning of the vector.
		///     2.  If not all of the targetMode's pitches are present, there will be no corresponding
		///         pitchVector connecting to that missing pitch. (It will then appear suddenly, when
		///         the target is reached.)
		///     2a. If a target pitch is present that is not in the targetMode, the pitch vector will
		///         be contained as usual in the returned list, as if the final weight is zero.
		///     3.  It is possible both for multiple pitchVectors to start in a particular pitch in this
		///         Study4.Mode, and for multiple pitchVectors to end in a particular pitch in the target.
		/// The returned Study4.Modes may contain different numbers of pitches.
		/// 
		/// This function first constructs pitch sets using only the pitchVectors, initially ignoring
		/// both this and the target Study4.Mode. It then constructs the returned Study4.Modes, ordering
		/// the pitches in the returned Study4.Modes according to hierarchies calculated from the weights
		/// in the original Study4.Modes. Missing pitches at either end of a pitchVector have zero weight,
		/// so that pitch weights fade in and out gradually by default.
		/// 
		/// Precursors to this function can be found in earlier versions of Tombeau 1, and especially
		/// in my paper notebook from 25 Sept to 1 Oct 2019.
		/// </summary>
		/// <param name="targetMode"></param>
		/// <param name="pitchVectors">Each entry contains an absolute pitch in this Study4.Mode, and</param>
		/// <param name="steps">An integer greater than 1</param>
		/// <returns>A list of steps Study4.Modes, beginning with this Study4.Mode and not including the target.</returns>
		public List<Mode> GetModeVector(Mode targetMode, Dictionary<int,int> pitchVectors, int steps)
		{
			List<Mode> rval = new List<Mode>();

			return rval;
		}

		#endregion constructors

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

		public IReadOnlyDictionary<int, int> AbsolutePitchWeightDict { get { return _absolutePitchWeightDict; } }
		private readonly Dictionary<int, int> _absolutePitchWeightDict; // is set in ctor
		public IReadOnlyList<int> AbsolutePitchHierarchy { get { return _absolutePitchHierarchy; } }
		private readonly List<int> _absolutePitchHierarchy;  // is set in ctor
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

				List<int> sortedBasePitches = new List<int>(AbsolutePitchHierarchy);
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

				return _gamut as IReadOnlyList<int>;
			}
		}
		private List<int> _gamut = null; // will be lazily evaluated

	}
}
