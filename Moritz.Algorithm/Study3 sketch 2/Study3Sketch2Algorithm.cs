using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;
using Moritz.Algorithm;
using Moritz.Palettes;
using Moritz.Symbols;
using Moritz.Spec;

namespace Moritz.Algorithm.Study3Sketch2
{
    /// <summary>
    /// Algorithm for testing the new input staves.
    /// This may develope as composition progresses...
    /// </summary>
    public class Study3Sketch2Algorithm : CompositionAlgorithm
    {
        /// <summary>
        /// This constructor can be called with both parameters null,
        /// just to get the overridden properties.
        /// </summary>
        public Study3Sketch2Algorithm()
            : base()
        {
        }

        public override List<int> MidiChannelIndexPerOutputVoice { get { return new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7 }; } }
        public override List<int> MasterVolumePerOutputVoice { get { return new List<int>() { 100, 100, 100, 100, 100, 100, 100, 100 }; } }
        public override int NumberOfInputVoices { get { return 1; } }  
        public override int NumberOfBars { get { return 5; } }

        /// <summary>
        /// See CompositionAlgorithm.DoAlgorithm()
        /// </summary>
        public override List<List<VoiceDef>> DoAlgorithm(List<Krystal> krystals, List<Palette> palettes)
        {
            _krystals = krystals;
            _palettes = palettes;

            List<List<VoiceDef>> bars = new List<List<VoiceDef>>();
            List<VoiceDef> bar1 = CreateBar1();
            bars.Add(bar1);
            int bar2StartMsPos = GetEndMsPosition(bar1);
            List<VoiceDef> bar2 = CreateBar2(bar2StartMsPos);
            bars.Add(bar2);
            int bar3StartMsPos = GetEndMsPosition(bar2);
            List<List<VoiceDef>> bars3to5 = CreateBars3to5(bar3StartMsPos);
            foreach(List<VoiceDef> bar in bars3to5)
            {
                bars.Add(bar);
            }
            Debug.Assert(bars.Count == NumberOfBars);

			/****************************************************************************************
			* This score is liable to be recompiled when other things change in the SVG-MIDI file format,
			* so its default version should be kept simple.
			* The default CC settings are set in the first InputChordDef in bar 1, but setting trkOptions
			* has therefore been commented out in the following functions. 
			* For information as to how to set TrkOptions, see the code in the commented-out sections of
			* the following functions, and the comment at the top of TrkOptions.cs.
			*****************************************************************************************/
			List<InputChordDef> inputChordSymbolsInBar1 = GetInputChordDefsInBar(bars[0]);
			SetPerformanceOptionsOnFirstInputChordDef(inputChordSymbolsInBar1);
			List<InputChordDef> inputChordSymbolsInBar2 = GetInputChordDefsInBar(bars[1]);
            SetBar2NoteOnNoteOffControls(inputChordSymbolsInBar2);
            List<InputChordDef> inputChordSymbolsInBar3 = GetInputChordDefsInBar(bars[2]);
            SetBar3PitchWheelToVolumeControls(inputChordSymbolsInBar3);
            List<InputChordDef> inputChordSymbolsInBar4 = GetInputChordDefsInBar(bars[3]);
            SetBar4FadeControls(inputChordSymbolsInBar4);
            List<InputChordDef> inputChordSymbolsInBar5 = GetInputChordDefsInBar(bars[4]);
            SetBar5SpeedControls(inputChordSymbolsInBar5);

            return bars;
        }

		private static void SetPerformanceOptionsOnFirstInputChordDef(List<InputChordDef> inputChordDefs)
		{
			InputChordDef inputChordDef1 = inputChordDefs[0]; // no need to check for null here.
			#region set ccSettings
			TrackCCSettings defaultCCSettings = new TrackCCSettings(null, new List<CCSetting>()
			{
				new PitchWheelPitchControl(5),
				new PressureControl(CControllerType.channelPressure),
				new ModWheelVolumeControl(20,127)
			});
			inputChordDef1.CCSettings = new CCSettings(defaultCCSettings, null);
			#endregion

			#region set chord level TrkOptions
			//inputChordDef1.TrkOptions = new TrkOptions(new List<TrkOption>()
			//{
			//	new VelocityScaledControl(3),
			//	new PedalControl(PedalOption.holdAll),
			//	new TrkOffControl(TrkOffOption.fade),
			//	new SpeedControl(1.5F)
			//});
			#endregion chordTrkOptions

			#region set note level TrkOptions
			//inputChordDef1.InputNoteDefs[0].TrkOptions = new TrkOptions(new VelocityScaledControl(2));
			#endregion

			#region set noteOn TrkOptions
			//NoteOn nOn = inputChordDef1.InputNoteDefs[0].NoteOn;
			//#region noteOnSeq
			//Seq nOnSeq = nOn.Seq;
			//nOnSeq.TrkOptions = new TrkOptions(new List<TrkOption>()
			//{
			//	new VelocityScaledControl(3),
			//	new PedalControl(PedalOption.holdAll),
			//	new TrkOffControl(TrkOffOption.undefined),
			//	new SpeedControl(2.1F)
			//});

			//for(int i = 0; i < nOnSeq.TrkRefs.Count; ++i)
			//{
			//	nOnSeq.TrkRefs[i].TrkOptions = new TrkOptions(new VelocityOverriddenControl((byte)(i + 4)));
			//}
			//#endregion
			#endregion set noteOn TrkOptions

			#region set noteOff TrkOptions
			//List<byte> newTrkOffs = new List<byte>() { 0,4,2,3,5 };
			//inputChordDef1.InputNoteDefs[0].NoteOff.TrkOffs = newTrkOffs;
			//inputChordDef1.InputNoteDefs[0].NoteOn.TrkOffs = newTrkOffs;
			#endregion set noteOff TrkOptions
		}

		private static void SetBar2NoteOnNoteOffControls(List<InputChordDef> bar2InputChordDefs)
        {
			//bar2InputChordDefs[0].InputNoteDefs[0].TrkOptions = new TrkOptions(new TrkOffControl(TrkOffOption.stopChord));
			//bar2InputChordDefs[1].InputNoteDefs[0].TrkOptions = new TrkOptions(new TrkOffControl(TrkOffOption.fade));
        }

        private static void SetBar3PitchWheelToVolumeControls(List<InputChordDef> bar3InputChordDefs)
        {
			//	foreach(InputChordDef inputChordDef in bar3InputChordDefs)
			//	{
			//		inputChordDef.InputNoteDefs[0].TrkOptions = new TrkOptions(new List<TrkOption>()
			//		{
			//			new TrkOffControl(TrkOffOption.fade),
			//		});
			//	}
        }

        private static void SetBar4FadeControls(List<InputChordDef> bar4InputChordDefs)
        {
			//  foreach(InputChordDef inputChordDef in bar4InputChordDefs)
			//  {
			//		inputChordDef.InputNoteDefs[0].TrkOptions = new TrkOptions(new List<TrkOption>()
			//		{
			//			new TrkOffControl(TrkOffOption.stopChord),	
			//		});
			//	}
        }

        private static void SetBar5SpeedControls(List<InputChordDef> bar5InputChordDefs)
        {
			//foreach(InputChordDef inputChordDef in bar5InputChordDefs)
			//{
			//	inputChordDef.InputNoteDefs[0].TrkOptions = new TrkOptions(new SpeedControl(2.2F));
			//}
        }

        #region CreateBar1()
        List<VoiceDef> CreateBar1()
		{
			List<VoiceDef> bar = new List<VoiceDef>();

			byte channel = 0;
			foreach(Palette palette in _palettes)
			{
				Trk trkDef = new Trk(channel, new List<IUniqueDef>());
				bar.Add(trkDef);
				WriteVoiceMidiDurationDefs1(trkDef, palette);
				++channel;
			}

			InputVoiceDef inputVoiceDef = new InputVoiceDef();
			VoiceDef bottomOutputVoice = bar[0];

			for(int i = 0; i < bottomOutputVoice.UniqueDefs.Count; ++i)
			{
				IUniqueDef iud = bottomOutputVoice.UniqueDefs[i];
				MidiChordDef mcd = iud as MidiChordDef;
				RestDef rd = iud as RestDef;
				if(mcd != null)
				{
					List<TrkRef> trkRefs = new List<TrkRef>();
					for(byte j = 0; j < _palettes.Count; ++j)
					{
						VoiceDef voiceDef = bar[j];
						MidiChordDef trkMCD = voiceDef[i] as MidiChordDef;
						TrkRef trkRef = new TrkRef(j, trkMCD.MsPosition, 1, null);
						trkRefs.Add(trkRef);
					};

					SeqRef seqRef = new SeqRef(trkRefs, null);

					NoteOn noteOn = new NoteOn(seqRef);
					NoteOff noteOff = new NoteOff(noteOn);

					byte notatedMidiPitch = (byte)(mcd.NotatedMidiPitches[0] + 36);
					InputNoteDef inputNoteDef = new InputNoteDef(notatedMidiPitch, noteOn, noteOff, null);

					List<InputNoteDef> inputNoteDefs = new List<InputNoteDef>() { inputNoteDef };

					// The InputChordDef's msPosition must be <= the msPosition of any of the contained trkRefs
					InputChordDef icd = new InputChordDef(mcd.MsPosition, mcd.MsDuration, inputNoteDefs, null);

					inputVoiceDef.UniqueDefs.Add(icd);
				}
				else if(rd != null)
				{
					RestDef newRest = new RestDef(rd.MsPosition, rd.MsDuration);
					inputVoiceDef.UniqueDefs.Add(newRest);
				}
			}

			bar.Add(inputVoiceDef);

			return bar;
		}

		private void WriteVoiceMidiDurationDefs1(Trk trkDef, Palette palette)
        {
            int bar1ChordMsSeparation = 1500;
            int msPosition = 0;

            for(int i = 0; i < palette.Count;++i)
            {
                IUniqueDef noteDef = palette.UniqueDurationDef(i);
                noteDef.MsPosition = msPosition;
                RestDef restDef = new RestDef(msPosition + noteDef.MsDuration, bar1ChordMsSeparation - noteDef.MsDuration);
                msPosition += bar1ChordMsSeparation;
                trkDef.UniqueDefs.Add(noteDef);
                trkDef.UniqueDefs.Add(restDef);
            }
        }
        #endregion CreateBar1()

        #region CreateBar2()
        /// <summary>
        /// This function creates only one bar, but with VoiceDef objects. 
        /// </summary>
        List<VoiceDef> CreateBar2(int bar2StartMsPos)
        {
            List<VoiceDef> bar = new List<VoiceDef>();

            byte channel = 0;
            List<Trk> trkDefs = new List<Trk>();
            foreach(Palette palette in _palettes)
            {
                bar.Add(new Trk(channel, new List<IUniqueDef>()));
				Trk trkDef = palette.NewTrkDef(channel);
                trkDef.MsDuration = 6000;
                trkDefs.Add(trkDef);
                ++channel;
            }

            int msPosition = bar2StartMsPos;
            int maxBarMsPos = 0;
            int startMsDifference = 1500;
            for(int i = 0; i < trkDefs.Count; ++i)
            {
                int maxMsPos = WriteVoiceMidiDurationDefsInBar2(bar[i], trkDefs[i], msPosition, bar2StartMsPos);
                maxBarMsPos = maxBarMsPos > maxMsPos ? maxBarMsPos : maxMsPos;
                msPosition += startMsDifference;
            }

            // now add the final rest in the bar
            for(int i = 0; i < trkDefs.Count; ++i)
            {
                int mdsdEndPos = trkDefs[i].EndMsPosition;
                if(maxBarMsPos > mdsdEndPos)
                {
                    RestDef rest2Def = new RestDef(mdsdEndPos, maxBarMsPos - mdsdEndPos);
                    bar[i].UniqueDefs.Add(rest2Def);
                }
            }

            InputVoiceDef inputVoiceDef = new InputVoiceDef(maxBarMsPos - bar2StartMsPos);
            inputVoiceDef.MsPosition = bar2StartMsPos; 
            int msPos = bar2StartMsPos;
            for(int i = 0; i < bar.Count; ++i)
            {
				TrkRef trk =  new TrkRef((byte)i, msPos, 12, null);
				List<TrkRef> trks = new List<TrkRef>() { trk };
				SeqRef seqRef = new SeqRef(trks, null);

				NoteOn noteOn = new NoteOn(seqRef);

				InputNoteDef inputNoteDef = new InputNoteDef(64, noteOn, null);

				List<InputNoteDef> inputNoteDefs = new List<InputNoteDef>(){inputNoteDef};
				InputChordDef inputChordDef = new InputChordDef(msPos, startMsDifference, inputNoteDefs, null);
                inputVoiceDef.InsertInRest(inputChordDef);
                msPos += startMsDifference;
            }

            bar.Add(inputVoiceDef);
            return bar;
        }

        /// <summary>
        /// Writes the first rest (if any) and the VoiceDef to the voice.
        /// Returns the endMsPos of the VoiceDef. 
        /// </summary>
        private int WriteVoiceMidiDurationDefsInBar2(VoiceDef voice, Trk trkDef, int msPosition, int bar2StartMsPos)
        {
            if(msPosition > bar2StartMsPos)
            {
                RestDef rest1Def = new RestDef(bar2StartMsPos, msPosition - bar2StartMsPos);
                voice.UniqueDefs.Add(rest1Def);
            }

            trkDef.MsPosition = msPosition;
            foreach(IUniqueDef iu in trkDef)
            {
                voice.UniqueDefs.Add(iu);
            }

            return trkDef.EndMsPosition;
        }
        #endregion CreateBar2()

        #region CreateBars3to5()
        /// <summary>
        /// This function creates three bars, identical to bar2 with two internal barlines.
        /// The VoiceDef objects cross barlines. 
        /// </summary>
        List<List<VoiceDef>> CreateBars3to5(int bar3StartMsPos)
        {
            List<List<VoiceDef>> bars = new List<List<VoiceDef>>();
            List<VoiceDef> threeBars = CreateBar2(bar3StartMsPos);

            //int bar4StartPos = bar3StartMsPos + 6000;
            int bar4StartPos = bar3StartMsPos + 5950;
            List<List<VoiceDef>> bars3And4Plus5 = SplitBar(threeBars, bar4StartPos);
            int bar5StartPos = bar3StartMsPos + 10500;
            List<List<VoiceDef>> bars4and5 = SplitBar(bars3And4Plus5[1], bar5StartPos);

            bars.Add(bars3And4Plus5[0]); // bar 3
            bars.Add(bars4and5[0]); // bar 4
            bars.Add(bars4and5[1]); // bar 5
            return bars;
        }

        #endregion CreateBars3to5()
    }
}
