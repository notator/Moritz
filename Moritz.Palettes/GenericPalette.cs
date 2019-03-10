using System;
using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;
using Moritz.Globals;
using Moritz.Midi;
using Moritz.Symbols;
using Moritz.Spec;

namespace Moritz.Palettes
{
	/// <summary>
	/// Every Palette contains an indexable readonly list of objects.
	/// Every object returned from the readonly list by the indexer is a _clone_ of the object in the list.
	/// The readonly list must be set by the derived constructor.
	/// (An ordinary List can be assigned to an IReadOnlyList.)
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class Palette<T> where T : ICloneable
	{
		public T this[int index]
		{
			get => (T)_values[index].Clone();
		}

		public int Count
		{
			get => _values.Count;
		}

		protected IReadOnlyList<T> _values;
	}

	public class TrkPalette : Palette<Trk>
	{
		public TrkPalette(List<Trk> trks)
		{
			_values = trks;
		}
	}
}
