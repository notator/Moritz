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
	/// Every Palette contains a readonly list of objects that can only be retrieved by index.
	/// Every object returned by the indexer is a _clone_ of the object in the list.
	/// The readonly list must be set once by the derived constructor using the SetValues(values) function.
	
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class Palette<T> where T : ICloneable
	{
		public int Count
		{
			get => _values.Count;
		}

		/// <summary>
		/// This protected indexer returns a clone of the object in the private _values list.
		/// It must be called by a public accessor function in the derived class.
		/// </summary>
		protected T this[int index]
		{
			get => (T)_values[index].Clone();
		}

		/// <summary>
		/// This function must be called to set the private _values list. It can only be called once.
		/// </summary>
		protected void SetValues(List<T> values)
		{
			if(_values == null)
			{
				_values = values;
			}
			else
			{
				throw new ApplicationException("_values can only be set once.");
			}
		}



		private IReadOnlyList<T> _values = null;
	}

	public class TrkPalette : Palette<Trk>
	{
		public TrkPalette(List<Trk> trks)
		{
			SetValues(trks);
		}

		/// <summary>
		/// Gets a clone of the trk at index, sets its midi channel to midiChannel, and then returns the result.
		/// </summary>
		public Trk GetTrk(int index, int midiChannel)
		{
			Trk trk = this[index];
			trk.MidiChannel = midiChannel;

			return trk;
		}
	}
}
