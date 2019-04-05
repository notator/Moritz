using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Krystals4ObjectLibrary;
using Moritz.Globals;

namespace Moritz.Spec
{
	public class FortePitchClassRecord
	{
		public FortePitchClassRecord(HashSet<int> bestPrimeForm, string intervalVectorStr, string zPairName)
		{
			PrimeForm = bestPrimeForm;

			List<int> intervalVector = new List<int>();
			for(int i = 0; i < intervalVectorStr.Length; ++i)
			{
				string c = intervalVectorStr[i].ToString();
				Int32.TryParse(c, out int number);
				intervalVector.Add(number);
			}

			IntervalVector = intervalVector;
			ZPairName = zPairName;
		}
		public readonly HashSet<int> PrimeForm;
		public readonly IReadOnlyList<int> IntervalVector;
		public readonly string ZPairName;
	}

	/// <summary>
	/// This class implements the "pitch class set" defined by Allen Forte in "The Structure of Atonal Music".
	/// In that book, a "pitch class set" must contain between 3 and 9 pitch classes.
	/// </summary>
	public class FortePitchClassSet
	{
		/// <summary>
		/// The argument is cloned before being used.
		/// The PrimeForm, PrimeRoot, PrimeInversionForm and ForteName attributes are set.
		/// The ForteName is found as containing the "best" of either PrimeForm or PrimeInversionForm.
		/// "best" is defined by Allen Forte's Algorithm, coded in the GetBestForm(form1, form2) function in this class. 
		/// </summary>
		/// <param name="pitches">A List of int or a HashSet of int containing any number of non-negative integers (the order is unimportant).</param>
		public FortePitchClassSet(ICollection<int> pitchesArg)
		{
			HashSet<int> basicPitchClassesSet = GetBasicPitchClassSet(pitchesArg);

			if(basicPitchClassesSet.Count < 3 || basicPitchClassesSet.Count > 9)
			{
				throw new ApplicationException("Cannot create a FortePitchClassSet containing less than 3 or more than 9 pitch classes.");
			}

			IReadOnlyList<int> bestRotation = GetBestRotation(basicPitchClassesSet);
			_rootPitchClass = bestRotation[0];

			// _primeForm is an IReadOnlyList, PrimeForm is a HashSet.
			_primeForm = GetPrimeForm(bestRotation);
			// _primeInversionForm is an IReadOnlyList, PrimeInversionForm is a HashSet.
			_primeInversionForm = GetPrimeInversionForm(_primeForm);

			FortePrimeForm = new HashSet<int>(GetBestForm(_primeForm, _primeInversionForm));

			ForteName = GetName(FortePrimeForm);
		}

		#region functions called by the constructor
		/// <summary> 
		/// Returns a HashSet of int containing the pitch classes present in the pitchesArg argument.
		/// The HashSet is constructed with the values in ascending order for debugging purposes.
		/// The argument can contain any number of non-negative ints.
		/// Duplicate pitch classes in the argument will be silently ignored.
		/// An exception will be thrown if the argument contains negative values or
		/// if the returned HashSet would have less than 3 or more than 9 values.
		/// </summary>
		/// <param name="pitchesArg">A List of int or a HashSet of int containing any number of non-negative integers (the order is unimportant).</param>
		private HashSet<int> GetBasicPitchClassSet(ICollection<int> pitchesArg)
		{
			var pitches = new List<int>(pitchesArg);

			#region precondition
			for(int i = 0; i < pitchesArg.Count; ++i)
			{
				if(pitches[i] < 0)
				{
					throw new ApplicationException();
				}
			}
			#endregion

			for(int i = 0; i < pitches.Count; ++i)
			{
				pitches[i] %= 12;
			}

			var pitchClassSet = new List<int>();
			foreach(int val in pitches)
			{
				if(!pitchClassSet.Contains(val))
				{
					pitchClassSet.Add(val);
				}
			}

			pitchClassSet.Sort();

			#region postcondition
			CheckBasicConsistency(pitchClassSet);
			#endregion

			return new HashSet<int>(pitchClassSet);
		}

		/// <summary>
		/// See "The Structure of Atonal Music" page 4.
		/// The returned list contains the "best" rotation of the argument's pitch classes.
		/// The values in the returned list are in ascending order, and may be in any octave.
		/// </summary>
		/// <param name="normalForm">A HashSet containing between 3 and 9 unique pitch classes in range [0..11]</param>
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
				while(absPitch < rotation[rotation.Count - 1])
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
		/// Returns either list1 or list2 depending on which is "best" according to Forte's algorithm (p.4).
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
			int diff = (list1[count - 1] - firstForm1) - (list2[count - 1] - firstForm2);
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
		/// The PrimeForm is created by subtracting the first value in bestRotation from each value, and adding 12
		/// when/if the result is negative. The result is a list in which the first value is 0, the values are in
		/// range [0..11], in ascending order, and the intervallic relations in bestRotation are preserved.
		/// </summary>
		private IReadOnlyList<int> GetPrimeForm(IReadOnlyList<int> bestRotation)
		{
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

			CheckPrimeConsistency(primeForm);

			return primeForm;
		}

		/// <summary>
		/// The primeForm of the bestRotation of the inversion of the argument.
		/// </summary>
		/// <param name="primeForm"></param>
		/// <returns></returns>
		private IReadOnlyList<int> GetPrimeInversionForm(IReadOnlyList<int> primeForm)
		{
			var inversion = new HashSet<int>();
			for(int i = 0; i < primeForm.Count; ++i)
			{
				inversion.Add((12 - primeForm[i]) % 12);
			}

			IReadOnlyList<int> bestRotation = GetBestRotation(inversion);
			IReadOnlyList<int> primeInversion = GetPrimeForm(bestRotation);

			return primeInversion;
		}

		/// <summary>
		/// The name (=key) of the FortePitchClassRec having the given primeForm in the FortePitchClassSets Dictionary. 
		/// </summary>
		private string GetName(HashSet<int> bestPrimeForm)
		{
			string name = "";
			foreach(KeyValuePair<string, FortePitchClassRecord> record in FortePitchClassSets)
			{
				if(record.Value.PrimeForm.SetEquals(bestPrimeForm))
				{
					name = record.Key;
					break;
				}
			}
			if(string.IsNullOrEmpty(name))
			{
				throw new ApplicationException("FortePitchClassSet not found in the FortePitchClassSets Dictionary.");
			}
			return name;
		}

		/// <summary>
		/// This function ensures that
		/// 1. the values are in range [0..11]
		/// 2. the values are in ascending order.
		/// </summary>
		private void CheckBasicConsistency(IReadOnlyList<int> normalForm)
		{
			foreach(int v in normalForm)
			{
				if(v < 0 || v > 11)
				{
					throw new ApplicationException();
				}
			}
			for(int i = 1; i < normalForm.Count; i++)
			{
				if(normalForm[i - 1] >= normalForm[i])
				{
					throw new ApplicationException();
				}
			}
		}

		/// <summary>
		/// This function is used when checking assignments to PrimeForm, PrimeInversion and BestPrime.
		/// It ensures that
		/// 1. the values are in range [0..11]
		/// 2. the values are in ascending order.
		/// 3. the first value in the primeList is 0
		/// </summary>
		private void CheckPrimeConsistency(IReadOnlyList<int> primeList)
		{
			CheckBasicConsistency(primeList);

			if(primeList[0] != 0)
			{
				throw new ApplicationException();
			}
		}

		#endregion

		#region set by the constructor
		/// <summary>
		/// The returned pitch classes are in normal order, i.e. in range 0..11 in ascending order.
		/// The first value is always 0.
		/// These values are independent of the transposition of the pitch classes used to construct this FortePitchClassSet,
		/// </summary>
		public HashSet<int> PrimeForm { get { return new HashSet<int>(_primeForm); } }
		/// <summary>
		/// The "best" Rotation of the inversion of the PrimeForm, transposed so that PrimeInversionForm[0] is 0.
		/// Rotations are in ascending order, so PrimeInversionForm is too.
		/// </summary>
		public HashSet<int> PrimeInversionForm { get { return new HashSet<int>(_primeInversionForm); } }
		public HashSet<int> FortePrimeForm { get; private set; }
		public string ForteName { get; private set; }
		public IReadOnlyList<int> IntervalVector
		{
			get
			{
				FortePitchClassRecord pcr = FortePitchClassSets[ForteName];
				return pcr.IntervalVector;
			}
		}
		public string ZPairName
		{
			get
			{
				FortePitchClassRecord pcr = FortePitchClassSets[ForteName];
				return pcr.ZPairName;
			}
		}

		#region private variables set by the constructor 
		/// <summary>
		/// The transposition of the original pitches used to construct this FortePitchClassSet re the PrimeForm.
		/// </summary>
		private readonly int _rootPitchClass;
		/// <summary>
		/// Used by public HashSet PrimeForm.
		/// </summary>
		private readonly IReadOnlyList<int> _primeForm;
		/// <summary>
		/// Used by public HashSet PrimeInversionForm.
		/// </summary>
		private readonly IReadOnlyList<int> _primeInversionForm;
		#endregion

		#endregion

		#region public functions
		/// <summary> 
		/// Returns the PrimeForm of this FortePitchClassSet transposed by the argument.
		/// The transposition is relative to the original pitches used to construct this FortePitchClassSet.
		/// The returned pitch classes are in normal order, i.e. in range 0..11 in ascending order.
		/// </summary>
		/// <param name="transposition">Any positive or negative int.</param>
		public HashSet<int> PitchClasses(int transposition)
		{
			return _pitchClasses(PrimeForm, transposition);
		}
		/// <summary> 
		/// Returns the PrimeInversionForm transposed by the argument.
		/// The transposition is relative to the original pitches used to construct this FortePitchClassSet.
		/// The returned pitch classes are in normal order, i.e. in range 0..11 in ascending order.
		/// </summary>
		/// <param name="transposition">Any positive or negative int.</param>
		public HashSet<int> InvertedPitchClasses(int transposition)
		{
			return _pitchClasses(PrimeInversionForm, transposition);
		}
		/// <summary>
		/// Code used by both of the above.
		/// The returned pitch classes are in normal order, i.e. in range 0..11 in ascending order.
		/// </summary>
		/// <param name="transposition"></param>
		private HashSet<int> _pitchClasses(HashSet<int> primeForm, int transposition)
		{
			List<int> pitchClasses = new List<int>();

			int transpRePrimeForm = _rootPitchClass + transposition;

			foreach(int primePitchClass in primeForm)
			{
				int pitchClass = (primePitchClass + transpRePrimeForm) % 12;
				while(pitchClass < 0)
				{
					pitchClass += 12;
				}
				pitchClasses.Add(pitchClass);
			}
			pitchClasses.Sort();

			return new HashSet<int>(pitchClasses);
		}
		#endregion

		/// <summary>
		/// This static dictionary contains information copied from Appendix 1 of Allen Forte's book.
		/// I have added the names of the Z-related FortePitchClassSets, so that they are easy to find when linking.
		/// </summary>
		public static Dictionary<string, FortePitchClassRecord> FortePitchClassSets = new Dictionary<string, FortePitchClassRecord>()
		{
			{ "3_1(12)",	new FortePitchClassRecord( new HashSet<int>(){0,1,2}, "210000", "")},
			{ "3_2",		new FortePitchClassRecord( new HashSet<int>(){0,1,3}, "111000", "")},
			{ "3_3",		new FortePitchClassRecord( new HashSet<int>(){0,1,4}, "101100", "")},
			{ "3_4",		new FortePitchClassRecord( new HashSet<int>(){0,1,5}, "100110", "")},
			{ "3_5",		new FortePitchClassRecord( new HashSet<int>(){0,1,6}, "100011", "")},
			{ "3_6(12)",	new FortePitchClassRecord( new HashSet<int>(){0,2,4}, "020100", "")},
			{ "3_7",		new FortePitchClassRecord( new HashSet<int>(){0,2,5}, "011010", "")},
			{ "3_8",		new FortePitchClassRecord( new HashSet<int>(){0,2,6}, "010101", "")},
			{ "3_9(12)",	new FortePitchClassRecord( new HashSet<int>(){0,2,7}, "010020", "")},
			{ "3_10(12)",	new FortePitchClassRecord( new HashSet<int>(){0,3,6}, "002001", "")},
			{ "3_11",       new FortePitchClassRecord( new HashSet<int>(){0,3,7}, "001110", "")},
			{ "3_12(4)",    new FortePitchClassRecord( new HashSet<int>(){0,4,8}, "000300", "")},

			//--------------------------------------------------------------------------

			{ "4_1(12)",    new FortePitchClassRecord( new HashSet<int>(){0,1,2,3}, "321000", "") },
			{ "4_2",		new FortePitchClassRecord( new HashSet<int>(){0,1,2,4}, "221100", "") },
			{ "4_3(12)",    new FortePitchClassRecord( new HashSet<int>(){0,1,3,4}, "212100", "") },
			{ "4_4",		new FortePitchClassRecord( new HashSet<int>(){0,1,2,5}, "211110", "") },
			{ "4_5",		new FortePitchClassRecord( new HashSet<int>(){0,1,2,6}, "210111", "") },
			{ "4_6(12)",    new FortePitchClassRecord( new HashSet<int>(){0,1,2,7}, "210021", "") },
			{ "4_7(12)",    new FortePitchClassRecord( new HashSet<int>(){0,1,4,5}, "201210", "") },
			{ "4_8(12)",    new FortePitchClassRecord( new HashSet<int>(){0,1,5,6}, "200121", "") },
			{ "4_9(6)",		new FortePitchClassRecord( new HashSet<int>(){0,1,6,7}, "200022", "") },
			{ "4_10(12)",	new FortePitchClassRecord( new HashSet<int>(){0,2,3,5}, "122010", "") },
			{ "4_11",		new FortePitchClassRecord( new HashSet<int>(){0,1,3,5}, "121110", "") },
			{ "4_12",		new FortePitchClassRecord( new HashSet<int>(){0,2,3,6}, "112101", "") },
			{ "4_13",		new FortePitchClassRecord( new HashSet<int>(){0,1,3,6}, "112011", "") },
			{ "4_14",		new FortePitchClassRecord( new HashSet<int>(){0,2,3,7}, "111120", "") },
			{ "4_Z15",		new FortePitchClassRecord( new HashSet<int>(){0,1,4,6}, "111111", "4_Z29") },
			{ "4_16",		new FortePitchClassRecord( new HashSet<int>(){0,1,5,7}, "110121", "") },
			{ "4_17(12)",	new FortePitchClassRecord( new HashSet<int>(){0,3,4,7}, "102210", "") },
			{ "4_18",		new FortePitchClassRecord( new HashSet<int>(){0,1,4,7}, "102111", "") },
			{ "4_19",		new FortePitchClassRecord( new HashSet<int>(){0,1,4,8}, "101310", "") },
			{ "4_20(12)",	new FortePitchClassRecord( new HashSet<int>(){0,1,5,8}, "101220", "") },
			{ "4_21(12)",	new FortePitchClassRecord( new HashSet<int>(){0,2,4,6}, "030201", "") },
			{ "4_22",		new FortePitchClassRecord( new HashSet<int>(){0,2,4,7}, "021120", "") },
			{ "4_23(12)",	new FortePitchClassRecord( new HashSet<int>(){0,2,5,7}, "021030", "") },
			{ "4_24(12)",	new FortePitchClassRecord( new HashSet<int>(){0,2,4,8}, "020301", "") },
			{ "4_25(6)",	new FortePitchClassRecord( new HashSet<int>(){0,2,6,8}, "020202", "") },
			{ "4_26(12)",	new FortePitchClassRecord( new HashSet<int>(){0,3,5,8}, "012120", "") },
			{ "4_27",		new FortePitchClassRecord( new HashSet<int>(){0,2,5,8}, "012111", "") },
			{ "4_28(3)",	new FortePitchClassRecord( new HashSet<int>(){0,3,6,9}, "004002", "") },
			{ "4_Z29",      new FortePitchClassRecord( new HashSet<int>(){0,1,3,7}, "111111", "4_Z15") },

			//--------------------------------------------------------------------------

			{ "5_1(12)",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,4}, "432100", "") },
			{ "5_2",		new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,5}, "332110", "") },
			{ "5_3",		new FortePitchClassRecord( new HashSet<int>(){0,1,2,4,5}, "322210", "") },
			{ "5_4",		new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,6}, "322111", "") },
			{ "5_5",		new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,7}, "321121", "") },
			{ "5_6",		new FortePitchClassRecord( new HashSet<int>(){0,1,2,5,6}, "311221", "") },
			{ "5_7",		new FortePitchClassRecord( new HashSet<int>(){0,1,2,6,7}, "310132", "") },
			{ "5_8(12)",	new FortePitchClassRecord( new HashSet<int>(){0,2,3,4,6}, "232201", "") },
			{ "5_9",		new FortePitchClassRecord( new HashSet<int>(){0,1,2,4,6}, "231211", "") },
			{ "5_10",		new FortePitchClassRecord( new HashSet<int>(){0,1,3,4,6}, "223111", "") },
			{ "5_11",		new FortePitchClassRecord( new HashSet<int>(){0,2,3,4,7}, "222220", "") },
			{ "5_Z12(12)",	new FortePitchClassRecord( new HashSet<int>(){0,1,3,5,6}, "222121", "5_Z36") },
			{ "5_13",		new FortePitchClassRecord( new HashSet<int>(){0,1,2,4,8}, "221311", "") },
			{ "5_14",		new FortePitchClassRecord( new HashSet<int>(){0,1,2,5,7}, "221131", "") },
			{ "5_15(12)",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,6,8}, "220222", "") },
			{ "5_16",		new FortePitchClassRecord( new HashSet<int>(){0,1,3,4,7}, "213211", "") },
			{ "5_Z17(12)",	new FortePitchClassRecord( new HashSet<int>(){0,1,3,4,8}, "212320", "5_Z37(12)") },
			{ "5_Z18",		new FortePitchClassRecord( new HashSet<int>(){0,1,4,5,7}, "212221", "5_Z38") },
			{ "5_19",		new FortePitchClassRecord( new HashSet<int>(){0,1,3,6,7}, "212122", "") },
			{ "5_20",		new FortePitchClassRecord( new HashSet<int>(){0,1,3,7,8}, "211231", "") },
			{ "5_21",		new FortePitchClassRecord( new HashSet<int>(){0,1,4,5,8}, "202420", "") },
			{ "5_22(12)",	new FortePitchClassRecord( new HashSet<int>(){0,1,4,7,8}, "202321", "") },
			{ "5_23",		new FortePitchClassRecord( new HashSet<int>(){0,2,3,5,7}, "132130", "") },
			{ "5_24",		new FortePitchClassRecord( new HashSet<int>(){0,1,3,5,7}, "131221", "") },
			{ "5_25",		new FortePitchClassRecord( new HashSet<int>(){0,2,3,5,8}, "123121", "") },
			{ "5_26",		new FortePitchClassRecord( new HashSet<int>(){0,2,4,5,8}, "122311", "") },
			{ "5_27",		new FortePitchClassRecord( new HashSet<int>(){0,1,3,5,8}, "122230", "") },
			{ "5_28",		new FortePitchClassRecord( new HashSet<int>(){0,2,3,6,8}, "122212", "") },
			{ "5_29",		new FortePitchClassRecord( new HashSet<int>(){0,1,3,6,8}, "122131", "") },
			{ "5_30",		new FortePitchClassRecord( new HashSet<int>(){0,1,4,6,8}, "121321", "") },
			{ "5_31",		new FortePitchClassRecord( new HashSet<int>(){0,1,3,6,9}, "114112", "") },
			{ "5_32",		new FortePitchClassRecord( new HashSet<int>(){0,1,4,6,9}, "113221", "") },
			{ "5_33(12)",	new FortePitchClassRecord( new HashSet<int>(){0,2,4,6,8}, "040402", "") },
			{ "5_34(12)",	new FortePitchClassRecord( new HashSet<int>(){0,2,4,6,9}, "032221", "") },
			{ "5_35(12)",	new FortePitchClassRecord( new HashSet<int>(){0,2,4,7,9}, "032140", "") },
			{ "5_Z36",		new FortePitchClassRecord( new HashSet<int>(){0,1,2,4,7}, "222121", "5_Z12(12)") },
			{ "5_Z37(12)",	new FortePitchClassRecord( new HashSet<int>(){0,3,4,5,8}, "212320", "5_Z17(12)") },
			{ "5_Z38",      new FortePitchClassRecord( new HashSet<int>(){0,1,2,5,8}, "212221", "5_Z18") },

			//--------------------------------------------------------------------------

			{ "6_1(12)",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,4,5}, "543210", "") },
			{ "6_2",		new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,4,6}, "443211", "") },
			{ "6_Z3",		new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,5,6}, "433221", "6_Z36") },
			{ "6_Z4(12)",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,4,5,6}, "432321", "6_Z37(12)") },
			{ "6_5",		new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,6,7}, "422232", "") },
			{ "6_Z6(12)",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,5,6,7}, "421242", "6_Z38(12)") },
			{ "6_7(6)",		new FortePitchClassRecord( new HashSet<int>(){0,1,2,6,7,8}, "420243", "") },
			{ "6_8(12)",	new FortePitchClassRecord( new HashSet<int>(){0,2,3,4,5,7}, "343230", "") },
			{ "6_9",		new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,5,7}, "342231", "") },
			{ "6_Z10",		new FortePitchClassRecord( new HashSet<int>(){0,1,3,4,5,7}, "333321", "6_Z39") },
			{ "6_Z11",		new FortePitchClassRecord( new HashSet<int>(){0,1,2,4,5,7}, "333231", "6_Z40") },
			{ "6_Z12",		new FortePitchClassRecord( new HashSet<int>(){0,1,2,4,6,7}, "332232", "6_Z41") },
			{ "6_Z13(12)",	new FortePitchClassRecord( new HashSet<int>(){0,1,3,4,6,7}, "324222", "6_Z42(12)") },
			{ "6_14",		new FortePitchClassRecord( new HashSet<int>(){0,1,3,4,5,8}, "323430", "") },
			{ "6_15",		new FortePitchClassRecord( new HashSet<int>(){0,1,2,4,5,8}, "323421", "") },
			{ "6_16",		new FortePitchClassRecord( new HashSet<int>(){0,1,4,5,6,8}, "322431", "") },
			{ "6_Z17",		new FortePitchClassRecord( new HashSet<int>(){0,1,2,4,7,8}, "322332", "6_Z43") },
			{ "6_18",		new FortePitchClassRecord( new HashSet<int>(){0,1,2,5,7,8}, "322242", "") },
			{ "6_Z19",		new FortePitchClassRecord( new HashSet<int>(){0,1,3,4,7,8}, "313431", "6_Z44") },
			{ "6_20(4)",	new FortePitchClassRecord( new HashSet<int>(){0,1,4,5,8,9}, "303630", "") },
			{ "6_21",		new FortePitchClassRecord( new HashSet<int>(){0,2,3,4,6,8}, "242412", "") },
			{ "6_22",		new FortePitchClassRecord( new HashSet<int>(){0,1,2,4,6,8}, "241422", "") },
			{ "6_Z23(12)",	new FortePitchClassRecord( new HashSet<int>(){0,2,3,5,6,8}, "234222", "6_Z45(12)") },
			{ "6_Z24",		new FortePitchClassRecord( new HashSet<int>(){0,1,3,4,6,8}, "233331", "6_Z46") },
			{ "6_Z25",		new FortePitchClassRecord( new HashSet<int>(){0,1,3,5,6,8}, "233241", "6_Z47") },
			{ "6_Z26(12)",	new FortePitchClassRecord( new HashSet<int>(){0,1,3,5,7,8}, "232341", "6_Z48(12)") },
			{ "6_27",		new FortePitchClassRecord( new HashSet<int>(){0,1,3,4,6,9}, "225222", "") },
			{ "6_Z28(12)",	new FortePitchClassRecord( new HashSet<int>(){0,1,3,5,6,9}, "224322", "6_Z49(12)") },
			{ "6_Z29(12)",	new FortePitchClassRecord( new HashSet<int>(){0,1,3,6,8,9}, "224232", "6_Z50(12)") },
			{ "6_30(12)",	new FortePitchClassRecord( new HashSet<int>(){0,1,3,6,7,9}, "224223", "") },
			{ "6_31",		new FortePitchClassRecord( new HashSet<int>(){0,1,3,5,8,9}, "223431", "") },
			{ "6_32(12)",	new FortePitchClassRecord( new HashSet<int>(){0,2,4,5,7,9}, "143250", "") },
			{ "6_33",		new FortePitchClassRecord( new HashSet<int>(){0,2,3,5,7,9}, "143241", "") },
			{ "6_34",		new FortePitchClassRecord( new HashSet<int>(){0,1,3,5,7,9}, "142422", "") },
			{ "6_35(2)",	new FortePitchClassRecord( new HashSet<int>(){0,2,4,6,8,10},"060603", "") },
			// The following all have Z-relations
			{ "6_Z36",		new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,4,7}, "433221", "6_Z3") },
			{ "6_Z37(12)",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,4,8}, "432321", "6_Z4(12)") },
			{ "6_Z38(12)",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,7,8}, "421242", "6_Z6(12)") },
			{ "6_Z39",		new FortePitchClassRecord( new HashSet<int>(){0,2,3,4,5,8}, "333321", "6_Z10") },
			{ "6_Z40",		new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,5,8}, "333231", "6_Z11") },
			{ "6_Z41",		new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,6,8}, "332232", "6_Z12") },
			{ "6_Z42(12)",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,6,9}, "324222", "6_Z13(12)") },
			{ "6_Z43",		new FortePitchClassRecord( new HashSet<int>(){0,1,2,5,6,8}, "322332", "6_Z17") },
			{ "6_Z44",		new FortePitchClassRecord( new HashSet<int>(){0,1,2,5,6,9}, "313431", "6_Z19") },
			{ "6_Z45(12)",	new FortePitchClassRecord( new HashSet<int>(){0,2,3,4,6,9}, "234222", "6_Z23(12)") },
			{ "6_Z46",		new FortePitchClassRecord( new HashSet<int>(){0,1,2,4,6,9}, "233331", "6_Z24") },
			{ "6_Z47",		new FortePitchClassRecord( new HashSet<int>(){0,1,2,4,7,9}, "233241", "6_Z25") },
			{ "6_Z48(12)",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,5,7,9}, "232341", "6_Z26(12)") },
			{ "6_Z49(12)",	new FortePitchClassRecord( new HashSet<int>(){0,1,3,4,7,9}, "224322", "6_Z28(12)") },
			{ "6_Z50(12)",	new FortePitchClassRecord( new HashSet<int>(){0,1,4,6,7,9}, "224232", "6_Z29(12)") },

			//--------------------------------------------------------------------------

			{ "7_1",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,4,5,6}, "654321", "") },
			{ "7_2",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,4,5,7}, "554331", "") },
			{ "7_3",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,4,5,8}, "544431", "") },
			{ "7_4",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,4,6,7}, "544332", "") },
			{ "7_5",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,5,6,7}, "543342", "") },
			{ "7_6",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,4,7,8}, "533442", "") },
			{ "7_7",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,6,7,8}, "532353", "") },
			{ "7_8",	new FortePitchClassRecord( new HashSet<int>(){0,2,3,4,5,6,8}, "454422", "") },
			{ "7_9",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,4,6,8}, "453432", "") },
			{ "7_10",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,4,6,9}, "445332", "") },
			{ "7_11",	new FortePitchClassRecord( new HashSet<int>(){0,1,3,4,5,6,8}, "444441", "") },
			{ "7_Z12",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,4,7,9}, "444342", "7_Z36") },
			{ "7_13",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,4,5,6,8}, "443532", "") },
			{ "7_14",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,5,7,8}, "443352", "") },
			{ "7_15",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,4,6,7,8}, "442443", "") },
			{ "7_16",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,5,6,9}, "435432", "") },
			{ "7_Z17",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,4,5,6,9}, "434541", "7_Z37") },
			{ "7_Z18",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,5,8,9}, "434442", "7_Z38") },
			{ "7_19",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,6,7,9}, "434343", "") },																  
			{ "7_20",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,4,7,8,9}, "433452", "") },
			{ "7_21",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,4,5,8,9}, "424641", "") },
			{ "7_22",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,5,6,8,9}, "424542", "") },
			{ "7_23",	new FortePitchClassRecord( new HashSet<int>(){0,2,3,4,5,7,9}, "354351", "") },
			{ "7_24",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,5,7,9}, "353442", "") },
			{ "7_25",	new FortePitchClassRecord( new HashSet<int>(){0,2,3,4,6,7,9}, "345342", "") },
			{ "7_26",	new FortePitchClassRecord( new HashSet<int>(){0,1,3,4,5,7,9}, "344532", "") },
			{ "7_27",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,4,5,7,9}, "344451", "") },
			{ "7_28",	new FortePitchClassRecord( new HashSet<int>(){0,1,3,5,6,7,9}, "344433", "") },
			{ "7_29",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,4,6,7,9}, "344352", "") },																  
			{ "7_30",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,4,6,8,9}, "343542", "") },
			{ "7_31",	new FortePitchClassRecord( new HashSet<int>(){0,1,3,4,6,7,9}, "336333", "") },
			{ "7_32",	new FortePitchClassRecord( new HashSet<int>(){0,1,3,4,6,8,9}, "335442", "") },
			{ "7_33",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,4,6,8,10},"262623", "") },
			{ "7_34",	new FortePitchClassRecord( new HashSet<int>(){0,1,3,4,6,8,10},"254442", "") },
			{ "7_35",	new FortePitchClassRecord( new HashSet<int>(){0,1,3,5,6,8,10},"254361", "") },
			{ "7_Z36",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,5,6,8}, "444342", "7_Z12") },
			{ "7_Z37",	new FortePitchClassRecord( new HashSet<int>(){0,1,3,4,5,7,8}, "434541", "7_Z17") },
			{ "7_Z38",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,4,5,7,8}, "434442", "7_Z18") },

			//--------------------------------------------------------------------------

			{ "8_1",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,4,5,6,7}, "765442", "") },
			{ "8_2",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,4,5,6,8}, "665542", "") },
			{ "8_3",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,4,5,6,9}, "656542", "") },
			{ "8_4",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,4,5,7,8}, "655552", "") },
			{ "8_5",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,4,6,7,8}, "654553", "") },
			{ "8_6",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,5,6,7,8}, "654463", "") },
			{ "8_7",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,4,5,8,9}, "645652", "") },
			{ "8_8",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,4,7,8,9}, "644563", "") },
			{ "8_9",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,6,7,8,9}, "644464", "") },
			{ "8_10",	new FortePitchClassRecord( new HashSet<int>(){0,2,3,4,5,6,7,9}, "566452", "") },
			{ "8_11",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,4,5,7,9}, "565552", "") },
			{ "8_12",	new FortePitchClassRecord( new HashSet<int>(){0,1,3,4,5,6,7,9}, "556543", "") },
			{ "8_13",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,4,6,7,9}, "556453", "") },
			{ "8_14",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,4,5,6,7,9}, "555562", "") },
			{ "8_Z15",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,4,6,8,9}, "555553", "8_Z29") },
			{ "8_16",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,5,7,8,9}, "554563", "") },
			{ "8_17",	new FortePitchClassRecord( new HashSet<int>(){0,1,3,4,5,6,8,9}, "546652", "") },
			{ "8_18",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,5,6,8,9}, "546553", "") },
			{ "8_19",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,4,5,6,8,9}, "545752", "") },
			{ "8_20",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,4,5,7,8,9}, "545662", "") },
			{ "8_21",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,4,6,8,10},"474643", "") },
			{ "8_22",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,5,6,8,10},"465562", "") },
			{ "8_23",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,5,7,8,10},"465472", "") },
			{ "8_24",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,4,5,6,8,10},"464743", "") },
			{ "8_25",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,4,6,7,8,10},"464644", "") },
			{ "8_26",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,4,5,7,9,10},"456562", "") },
			{ "8_27",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,4,5,7,8,10},"456553", "") },
			{ "8_28",	new FortePitchClassRecord( new HashSet<int>(){0,1,3,4,6,7,9,10},"448444", "") },
			{ "8_Z29",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,5,6,7,9}, "555553", "8_Z15") },				

			//--------------------------------------------------------------------------

			{ "9_1",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,4,5,6,7,8},	"876663", "") },
			{ "9_2",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,4,5,6,7,9},	"777663", "") },
			{ "9_3",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,4,5,6,8,9},	"767763", "") },
			{ "9_4",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,4,5,7,8,9},	"766773", "") },
			{ "9_5",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,4,6,7,8,9},	"766674", "") },
			{ "9_6",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,4,5,6,8,10},	"686763", "") },
			{ "9_7",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,4,5,7,8,10},	"677673", "") },
			{ "9_8",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,4,6,7,8,10},	"676764", "") },
			{ "9_9",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,5,6,7,8,10},	"676683", "") },
			{ "9_10",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,4,6,7,9,10},	"668664", "") },
			{ "9_11",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,3,5,6,7,9,10},	"667773", "") },
			{ "9_12",	new FortePitchClassRecord( new HashSet<int>(){0,1,2,4,5,6,8,9,10},	"666963", "") }
		};
	}
}
