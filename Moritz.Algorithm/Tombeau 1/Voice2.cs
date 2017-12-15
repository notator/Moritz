using System;
using System.Collections.Generic;
using System.Diagnostics;
using Krystals4ObjectLibrary;
using Moritz.Globals;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
	internal class Voice2 : Tombeau1Voice
    {
		public Voice2(int midiChannel, Voice1 voice1, Envelope centredEnvelope, Envelope basedEnvelope)
			: base(midiChannel)
        {
			_modeSegments = Compose(voice1, centredEnvelope, basedEnvelope);
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

		private List<ModeSegment> Compose(Voice1 voice1, Envelope centredEnvelope, Envelope basedEnvelope)
		{
			var msValuesOfVoice1ModeGrpTrks = voice1.GetMsValuesOfModeGrpTrks();
			var msValuesOfVoice1IUniqueDefs = voice1.GetMsValuesOfIUniqueDefs();

			List<ModeSegment> voice2ModeSegments = new List<ModeSegment>();

			var v1ModeSegments = voice1.ModeSegments;
			for(int i = 0; i < v1ModeSegments.Count; i++)
			{
				var modeGrpTrks = GetModeGrpTrks(i, v1ModeSegments, msValuesOfVoice1ModeGrpTrks, msValuesOfVoice1IUniqueDefs);

				var modeSegment = new ModeSegment(MidiChannel, 0, modeGrpTrks);

				Debug.Assert(modeSegment.MsDuration == v1ModeSegments[i].MsDuration);

				voice2ModeSegments.Add(modeSegment);
			}

			SetModeSegmentMsPositionsReContainer(voice2ModeSegments);

			return voice2ModeSegments;
		}

		/// <summary>
		/// Returns the modeGrpTrks for the voice2 modeSegment having modeSegmentIndex.
		/// (There are v1ModeSegments.Count modeSegments in voice2 as well.)
		/// </summary>
		private IReadOnlyList<ModeGrpTrk> GetModeGrpTrks(int modeSegmentIndex, List<ModeSegment> v1ModeSegments,
										IReadOnlyList<IReadOnlyList<MsValues>> msValuesOfVoice1ModeGrpTrks,
										IReadOnlyList<IReadOnlyList<IReadOnlyList<MsValues>>> msValuesOfVoice1IUniqueDefs)
		{
			var modeGrpTrks = new List<ModeGrpTrk>();

			ModeGrpTrk v1ModeGrpTrk0 = v1ModeSegments[0].ModeGrpTrks[0];
			Mode mode = v1ModeGrpTrk0.Mode;
			List<IUniqueDef> iuds = new List<IUniqueDef>();
			
			foreach(var iud in v1ModeGrpTrk0.UniqueDefs)
			{
				iuds.Add((IUniqueDef)iud.Clone());
			}

			ModeGrpTrk v2ModeGrpTrk = new ModeGrpTrk(MidiChannel, 0, iuds, mode, 8);
			v2ModeGrpTrk.Shear(0, 36);
			v2ModeGrpTrk.CreateAccel(0, v2ModeGrpTrk.Count, 0.2);
			var velocityPerAbsolutePitch = mode.GetDefaultVelocityPerAbsolutePitch();
			velocityPerAbsolutePitch = Mode.SetVelocityPerAbsolutePitchRange(velocityPerAbsolutePitch, 10, 64);
			v2ModeGrpTrk.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch);

			int restDuration = v1ModeSegments[modeSegmentIndex].MsDuration - v2ModeGrpTrk.MsDuration;
			MidiRestDef midiRestDef = new MidiRestDef(0, restDuration);
			v2ModeGrpTrk.Add(midiRestDef);

			modeGrpTrks.Add(v2ModeGrpTrk);

			return modeGrpTrks;
		}

		internal void AdjustForThreeVoices()
		{
			throw new NotImplementedException();
		}

		internal void AdjustForFourVoices()
		{
			throw new NotImplementedException();
		}

		public override List<int> BarlineMsPositions()
		{
			var voice2BarlinePositions = new List<int>();
			return voice2BarlinePositions;
		}
	}
}