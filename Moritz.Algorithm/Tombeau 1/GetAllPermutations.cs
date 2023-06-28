using System;
using System.Collections.Generic;
using System.Diagnostics;

using Krystals5ObjectLibrary;
using Moritz.Globals;
using Moritz.Palettes;
using Moritz.Spec;
using Moritz.Symbols;

namespace Moritz.Algorithm.Tombeau1
{
	public partial class Tombeau1Algorithm : CompositionAlgorithm
	{
		/**********************************************
		 * Heap's algorithm:
		procedure generate(k : integer, A : array of any):
			if k = 1 then
				output(A)
			else
				// Generate permutations with kth unaltered
				// Initially k == length(A)
				generate(k - 1, A)

				// Generate permutations for kth swapped with each k-1 initial
				for i := 0; i<k-1; i += 1 do
					// Swap choice dependent on parity of k (even or odd)
					if k is even then

						swap(A[i], A[k - 1]) // zero-indexed, the kth is at k-1
					else
						swap(A[0], A[k - 1])

					end if
					generate(k - 1, A)


				end for
			end if
		******************************************************/

		/// <summary>
		/// Function derived from Heap's algorithm:  https://en.wikipedia.org/wiki/Heap%27s_algorithm
		/// </summary>
		/// <param name="k">Initial value must be A.Count</param>
		/// <param name="A">The integers to be permuted (e.g. {1,2,3,4,5} )</param>
		/// <param name="result">When this recursive function returns, the result will be here.</param>
		void GetAllPermutations(int k, List<int> A, List<List<int>> result)
		{
			void swap(List<int> array, int index1, int index2)
			{
				int temp = array[index1];
				array[index1] = array[index2];
				array[index2] = temp;
			}

			if(k == 1)
				result.Add(new List<int>(A));
			else
			{
				// Generate permutations with kth unaltered
				// Initially k == length(A)
				GetAllPermutations(k - 1, A, result);

				// Generate permutations for kth swapped with each k-1 initial
				for(int i = 0; i < k - 1; i++)
				{
					if(k % 2 == 0)
					{
						swap(A, i, k - 1);  // zero-indexed, the kth is at k-1
					}
					else
					{
						swap(A, 0, k - 1);  // zero-indexed, the kth is at k-1
					}
					GetAllPermutations(k - 1, A, result);
				}
			}
		}
	}
}
