using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
     internal class Tombeau1Voice : Trk
    {
		public Tombeau1Voice(int midiChannel)
			: base(midiChannel)
		{
		}

		public IReadOnlyList<IReadOnlyList<MsValues>> MsValuesOfComposedGamutTrks
		{
			get
			{
				Debug.Assert(_composedModeSegments != null && _composedModeSegments.Count > 0);

				var msPlaces = new List<List<MsValues>>();
				int msPos = 0;
				for(int i = 0; i < _composedModeSegments.Count; ++i)
				{
					var msPlacesPerGamut = new List<MsValues>();
					msPlaces.Add(msPlacesPerGamut);

					ModeSegment modeSegment = _composedModeSegments[i];
					for(int j = 0; j < modeSegment.Count; j++)
					{

						msPlacesPerGamut.Add(new MsValues(msPos, modeSegment[j].MsDuration));
						msPos += modeSegment[j].MsDuration;
					}
				}
				return msPlaces;
			}
		}

		public IReadOnlyList<IReadOnlyList<IReadOnlyList<MsValues>>> MsValuesOfComposedIUniqueDefs
		{
			get
			{
				Debug.Assert(_composedModeSegments != null && _composedModeSegments.Count > 0);

				int msPos = 0;
				var rVal = new List<List<List<MsValues>>>();
				for(int i = 0; i < _composedModeSegments.Count; ++i)
				{
					var modeSegmentList = new List<List<MsValues>>();
					rVal.Add(modeSegmentList);

					ModeSegment modeSegment = _composedModeSegments[i];
					for(int j = 0; j < modeSegment.Count; j++)
					{
						var iUniqueDefsList = new List<MsValues>();
						modeSegmentList.Add(iUniqueDefsList);

						GamutTrk pGamutTrk = modeSegment[j];
						for(int k = 0; k < pGamutTrk.Count; k++)
						{
							iUniqueDefsList.Add(new MsValues(msPos, pGamutTrk[k].MsDuration));
							msPos += pGamutTrk[k].MsDuration;
						}
					}
				}
				return rVal;
			}
		}

		internal List<ModeSegment> ComposedModeSegments { get => _composedModeSegments; }
		protected List<ModeSegment> _composedModeSegments = new List<ModeSegment>();
	}
}