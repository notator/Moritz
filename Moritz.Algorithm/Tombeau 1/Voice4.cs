using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;
using Krystals4ObjectLibrary;

namespace Moritz.Algorithm.Tombeau1
{
    internal class Voice4 : Tombeau1Voice
    {
        public Voice4(int midiChannel, Voice1 voice1, Voice2 voice2, Voice3 voice3, Envelope centredEnvelope, Envelope basedEnvelope)
			: base(midiChannel)
        {
			_modeSegments = Compose(voice1, voice2, voice3, centredEnvelope, basedEnvelope);
		}

		#region available Trk and ModeGrpTrk transformations
		// Add();
		// AddRange();
		// AdjustChordMsDurations();
		// AdjustExpression();
		// AdjustVelocities();
		// AdjustVelocitiesHairpin();
		// AlignObjectAtIndex();
		// CreateAccel();
		// FindIndexAtMsPositionReFirstIUD();
		// Insert();
		// InsertRange();
		// Permute();
		// Remove();
		// RemoveAt();
		// RemoveBetweenMsPositions();
		// RemoveRange();
		// RemoveScorePitchWheelCommands();
		// Replace();
		// SetDurationsFromPitches();
		// SetPanGliss(0, subT.MsDuration, 0, 127);
		// SetPitchWheelDeviation();
		// SetPitchWheelSliders();
		// SetVelocitiesFromDurations();
		// SetVelocityPerAbsolutePitch();
		// TimeWarp();
		// Translate();
		// Transpose();
		// TransposeStepsInModeGamut();
		// TransposeToRootInModeGamut();
		#endregion available Trk and ModeGrpTrk transformations

		private List<ModeSegment> Compose(Voice1 voice1, Voice2 voice2, Voice3 voice3, Envelope centredEnvelope, Envelope basedEnvelope)
		{
			List<ModeSegment> rval = new List<ModeSegment>();
			int unicode = 97; // 'a';

			foreach(ModeSegment ms in voice2.ModeSegments)
			{
				string idChar = ((char)unicode++).ToString();

				ModeGrpTrk newMgt = null;
				foreach(ModeGrpTrk mgt in ms.ModeGrpTrks)
				{
					int restDuration = 0;
					int ornamentDuration = 0;
					List<IUniqueDef> mcds = new List<IUniqueDef>();
					foreach(var iud in mgt.UniqueDefs)
					{
						if(iud is MidiChordDef)
						{
							mcds.Add(iud); // no need to clone here - will be cloned in new MidiChordDef.
							ornamentDuration += iud.MsDuration;
						}
						else
						{
							restDuration = iud.MsDuration;
							break;
						}
					}

					MidiChordDef ornamentDef = new MidiChordDef(ornamentDuration, mcds, idChar); // clones mcds
					MidiRestDef restDef = new MidiRestDef(ornamentDuration, restDuration);

					List<IUniqueDef> ornamentPlusRest = new List<IUniqueDef>() { ornamentDef, restDef };

					newMgt = new ModeGrpTrk(MidiChannel, 0, ornamentPlusRest, mgt.Mode, mgt.RootOctave);

					Debug.Assert(mgt.MsDuration == newMgt.MsDuration);
				}
				var modeSegment = new ModeSegment(MidiChannel, 0, new List<ModeGrpTrk>() { newMgt });
				rval.Add(modeSegment);
			}

			SetModeSegmentMsPositionsReContainer(rval);

			return rval;

		}
	}       
}