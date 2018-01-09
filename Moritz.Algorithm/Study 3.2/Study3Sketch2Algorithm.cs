using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;
using Moritz.Algorithm;
using Moritz.Palettes;
using Moritz.Symbols;
using Moritz.Globals;
using Moritz.Spec;
using System;

namespace Moritz.Algorithm.Study3Sketch2
{
    /// <summary>
    /// Algorithm for testing the new input staves.
    /// This may develope as composition progresses...
    /// </summary>
    public class Study3Sketch2Algorithm : CompositionAlgorithm
    {
        public Study3Sketch2Algorithm()
            : base()
        {
            CheckParameters();
        }

        public override IReadOnlyList<int> MidiChannelIndexPerOutputVoice { get { return new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7 }; } }
        public override IReadOnlyList<int> MidiChannelIndexPerInputVoice { get { return new List<int>() { 0 }; } }  
        public override int NumberOfBars { get { return 5; } }

        /// <summary>
        /// See CompositionAlgorithm.DoAlgorithm()
        /// </summary>
        public override List<Bar> DoAlgorithm(List<Krystal> krystals, List<Palette> palettes)
        {
            _krystals = krystals;
            _palettes = palettes;

			Seq bar1Seq = CreateBar1Seq();
			Seq bar2Seq = CreateBar2Seq();
			Seq bars345Seq = bar2Seq.Clone();

			/****************************************************************************************
			* This score is liable to be recompiled when other things change in the SVG-MIDI file format,
			* so its default version should be kept simple.
			* The default CC settings are set in the first InputChordDef in bar 1, but setting trkOptions
			* has therefore been commented out in the following functions. 
			* For information as to how to set TrkOptions, see the code in the commented-out sections of
			* the following functions, and the comment at the top of TrkOptions.cs.
			*****************************************************************************************/

			InputVoiceDef bar1InputVoiceDef = GetBar1InputVoiceDef(bar1Seq);
            SetTrackCCSettings(bar1InputVoiceDef);
			SetBar1PerformanceOptions(bar1InputVoiceDef);

            InputVoiceDef bar2InputVoiceDef = GetBar2InputVoiceDef(bar2Seq);
            SetBar2NoteOnNoteOffControls(bar2InputVoiceDef);

            InputVoiceDef bars345InputVoiceDef = GetBar345InputVoiceDef(bars345Seq);
            SetBar345PerformanceOptions(bars345InputVoiceDef);

			var approximateBarlineMsPositions = new List<double>();
			approximateBarlineMsPositions.Add(bar1InputVoiceDef.MsDuration);
			approximateBarlineMsPositions.Add(bar1InputVoiceDef.MsDuration + bar2InputVoiceDef.MsDuration);
			approximateBarlineMsPositions.Add(bar1InputVoiceDef.MsDuration + bar2InputVoiceDef.MsDuration + 5950);
			approximateBarlineMsPositions.Add(bar1InputVoiceDef.MsDuration + bar2InputVoiceDef.MsDuration + 10500);
			approximateBarlineMsPositions.Add(bar1InputVoiceDef.MsDuration + bar2InputVoiceDef.MsDuration + bars345InputVoiceDef.MsDuration);

			InputVoiceDef ivd = bar1InputVoiceDef;
            ivd.Concat(bar2InputVoiceDef);
            ivd.Concat(bars345InputVoiceDef);

			Seq mainSeq = bar1Seq;
			mainSeq.Concat(bar2Seq);
			mainSeq.Concat(bars345Seq);

			List<int> barlineMsPositions = GetBarlinePositions(mainSeq.Trks, new List<InputVoiceDef>() { ivd }, approximateBarlineMsPositions);

			List<InputVoiceDef> inputVoiceDefs = new List<InputVoiceDef>() { ivd };

			List<Bar> bars = GetBars(mainSeq, inputVoiceDefs, barlineMsPositions, null, null);

			return bars;
        }

		/// <summary>
		/// This function returns null or a SortedDictionary per VoiceDef in each bar.
		/// The dictionary contains the index at which the clef will be inserted in the VoiceDef's IUniquedefs,
		/// and the clef ID string ("t", "t1", "b3" etc.).
		/// Clefs will be inserted in reverse order of the Sorted dictionary, so that the indices are those of
		/// the existing IUniqueDefs before which the clef will be inserted.
		/// The SortedDictionaries should not contain tne initial clefs per voicedef - those will be included
		/// automatically.
		/// Note that a CautionaryChordDef counts as an IUniqueDef at the beginning of a bar, and that clefs
		/// cannot be inserted in front of them.
		/// </summary>
		protected override List<List<SortedDictionary<int, string>>> GetClefChangesPerBar(int nBars)
		{
			return null;
			// test code...
			//VoiceDef voiceDef1 = bars[0][1];
			//voiceDef1.InsertClefDef(9, "b3");
			//voiceDef1.InsertClefDef(8, "b2");
			//voiceDef1.InsertClefDef(7, "b1");
			//voiceDef1.InsertClefDef(6, "b");
			//voiceDef1.InsertClefDef(5, "t3");
			//voiceDef1.InsertClefDef(4, "t2");
			//voiceDef1.InsertClefDef(3, "t1");
			//voiceDef1.InsertClefDef(2, "t");
		}

		/// <summary>
		/// This function returns null or a SortedDictionary per VoiceDef in each bar.
		/// The dictionary contains the index of the IUniqueDef in the barat which the clef will be inserted in the VoiceDef's IUniquedefs,
		/// and the clef ID string ("t", "t1", "b3" etc.).
		/// Clefs will be inserted in reverse order of the Sorted dictionary, so that the indices are those of
		/// the existing IUniqueDefs before which the clef will be inserted.
		/// The SortedDictionaries should not contain tne initial clefs per voicedef - those will be included
		/// automatically.
		/// Note that both Clefs and a CautionaryChordDef at the beginning of a bar count as IUniqueDefs for
		/// indexing purposes, and that lyrics cannot be attached to them.
		/// </summary>
		protected override List<List<SortedDictionary<int, string>>> GetLyricsPerBar(int nBars)
		{
			return null;
			// test code...
			//VoiceDef voiceDef0 = bars[0][0];
			//MidiChordDef mcd1 = voiceDef0[2] as MidiChordDef;
			//mcd1.Lyric = "lyric1";
			//MidiChordDef mcd2 = voiceDef0[3] as MidiChordDef;
			//mcd2.Lyric = "lyric2";
			//MidiChordDef mcd3 = voiceDef0[4] as MidiChordDef;
			//mcd3.Lyric = "lyric3";
		}

		private InputVoiceDef GetBar1InputVoiceDef(Seq bar1Seq)
        {
            InputVoiceDef ivd = new InputVoiceDef(0, 0, new List<IUniqueDef>());
            ivd.MsPositionReContainer = bar1Seq.AbsMsPosition;

            Trk leadTrk = null;
            foreach(Trk trk in bar1Seq.Trks)
            {
                if(trk.MidiChannel == 0)
                {
                    leadTrk = trk;
                    break;
                }
                
            }
            Debug.Assert(leadTrk != null);
            foreach(IUniqueDef tIud in leadTrk)
            {
                MidiRestDef tRestDef = tIud as MidiRestDef;
                MidiChordDef tmcd = tIud as MidiChordDef;
                if(tRestDef != null)
                {
                    InputRestDef iRestDef = new InputRestDef(tRestDef.MsPositionReFirstUD, tRestDef.MsDuration);
                    ivd.Add(iRestDef);
                }
                else if(tmcd != null)
                {
                    List<TrkRef> trkRefs = new List<TrkRef>();
                    TrkOptions trkOptions = new TrkOptions(new TrkOffControl(TrkOffOption.stopChord));
                    foreach(Trk trk in bar1Seq.Trks)
                    {
                        trkRefs.Add(new TrkRef((byte)trk.MidiChannel, bar1Seq.AbsMsPosition + tmcd.MsPositionReFirstUD, 1, trkOptions));
                    }
                    SeqRef seqRef = new SeqRef(trkRefs, null);
                    NoteOn noteOn = new NoteOn(seqRef);
                    List<InputNoteDef> inputNoteDefs = new List<InputNoteDef>();
                    foreach(byte notatedMidiPitch in tmcd.NotatedMidiPitches)
                    {                      
                        inputNoteDefs.Add(new InputNoteDef((byte)(notatedMidiPitch + 36), noteOn, null));
                    }
                    InputChordDef icd = new InputChordDef(tIud.MsPositionReFirstUD, tIud.MsDuration, inputNoteDefs, M.Dynamic.none, null);
                    ivd.Add(icd);
                }     
            }

            return ivd;
        }

        private InputVoiceDef GetBar2InputVoiceDef(Seq bar2Seq)
        {
            InputVoiceDef ivd = new InputVoiceDef(0, 0, new List<IUniqueDef>());
            ivd.MsPositionReContainer = bar2Seq.AbsMsPosition;

            int inputChordDefMsDurations = 0;
            foreach(Trk trk in bar2Seq.Trks)
            {
                MidiChordDef firstMidiChordDef = null;
                TrkOptions trkOptions = new TrkOptions(new TrkOffControl(TrkOffOption.stopChord));
                foreach(IUniqueDef iud in trk.UniqueDefs)
                {
                    firstMidiChordDef = iud as MidiChordDef;
                    if(firstMidiChordDef != null)
                    {
                        List<TrkRef> trkRefs = new List<TrkRef>();
                        trkRefs.Add(new TrkRef((byte)trk.MidiChannel, bar2Seq.AbsMsPosition + firstMidiChordDef.MsPositionReFirstUD, 12, trkOptions));
                        SeqRef seqRef = new SeqRef(trkRefs, null);
                        NoteOn noteOn = new NoteOn(seqRef);
                        List<InputNoteDef> inputNoteDefs = new List<InputNoteDef>();
                        inputNoteDefs.Add(new InputNoteDef((byte)65, noteOn, null));
                        InputChordDef icd = new InputChordDef(firstMidiChordDef.MsPositionReFirstUD, 1500, inputNoteDefs, M.Dynamic.none, null);
                        ivd.Add(icd);
                        inputChordDefMsDurations += icd.MsDuration;
                        break;
                    }
                }
            }
            int finalRestMsDuration = bar2Seq.MsDuration - inputChordDefMsDurations;
            if(finalRestMsDuration > 0)
            {
                InputRestDef iRestDef = new InputRestDef(ivd.MsDuration, finalRestMsDuration);
                ivd.Add(iRestDef);
            }
            Debug.Assert(ivd.MsDuration == bar2Seq.MsDuration);

            return ivd;
        }

        private InputVoiceDef GetBar345InputVoiceDef(Seq bars345Seq)
        {
            InputVoiceDef ivd = GetBar2InputVoiceDef(bars345Seq);
            return ivd;
        }

        private static void SetTrackCCSettings(InputVoiceDef inputVoiceDef)
        {
            InputChordDef inputChordDef1 = (InputChordDef)inputVoiceDef[0]; // no need to check for null here.
            #region set ccSettings
            TrackCCSettings defaultCCSettings = new TrackCCSettings(null, new List<CCSetting>()
            {
                new PitchWheelPitchControl(5),
                new PressureControl(CControllerType.channelPressure),
                new ModWheelVolumeControl(20,127)
            });
            inputChordDef1.CCSettings = new CCSettings(defaultCCSettings, null);
            #endregion
        }

        private static void SetBar1PerformanceOptions(InputVoiceDef inputVoiceDef)
        {
            InputChordDef inputChordDef1 = (InputChordDef)inputVoiceDef[0]; // no need to check for null here.

            #region set voiceDef level trkOptions
            //foreach(InputChordDef inputChordDef in inputVoiceDef.InputChordDefs)
            //{
            //    inputChordDef.TrkOptions = new TrkOptions(new List<TrkOption>()
            //        {
            //            new TrkOffControl(TrkOffOption.fade),
            //        });
            //}
            #endregion

            #region set chord level TrkOptions
            //inputChordDef1.TrkOptions = new TrkOptions(new List<TrkOption>()
            //{
            //    new VelocityScaledControl(3),
            //    new PedalControl(PedalOption.holdAll),
            //    new TrkOffControl(TrkOffOption.fade),
            //    new SpeedControl(1.5F)
            //});
            #endregion chordTrkOptions

            #region set note level TrkOptions
            //inputChordDef1.InputNoteDefs[0].TrkOptions = new TrkOptions(new VelocityScaledControl(2));
            #endregion

            #region set noteOn TrkOptions
            //NoteOn nOn = inputChordDef1.InputNoteDefs[0].NoteOn;
            //nOn.TrkOptions = new TrkOptions(new List<TrkOption>()
            //{
            //    new VelocityScaledControl(3),
            //    new PedalControl(PedalOption.holdAll),
            //    new TrkOffControl(TrkOffOption.undefined),
            //    new SpeedControl(2.1F)
            //});
            #endregion set noteOn TrkOptions

            #region set noteOff TrkOptions
            //List<int> newTrkOffs = new List<int>() { 0, 4, 2, 3, 5 };
            //inputChordDef1.InputNoteDefs[0].NoteOff.TrkOffs = newTrkOffs;
            //inputChordDef1.InputNoteDefs[0].NoteOn.TrkOffs = newTrkOffs;
            #endregion set noteOff TrkOptions
        }

        private static void SetBar2NoteOnNoteOffControls(InputVoiceDef inputVoiceDef)
        {
            //InputChordDef inputChordDef0 = (InputChordDef)inputVoiceDef[0]; // no need to check for null here.
            //InputChordDef inputChordDef1 = (InputChordDef)inputVoiceDef[1]; // no need to check for null here.

            //inputChordDef0.TrkOptions = new TrkOptions(new TrkOffControl(TrkOffOption.stopChord));
            //inputChordDef1.TrkOptions = new TrkOptions(new TrkOffControl(TrkOffOption.fade));
        }

        private static void SetBar345PerformanceOptions(InputVoiceDef inputVoiceDef)
        {
            //// actually only bar 3
            //foreach(InputChordDef inputChordDef in inputVoiceDef.InputChordDefs)
            //{
            //    inputChordDef.InputNoteDefs[0].TrkOptions = new TrkOptions(new List<TrkOption>()
            //        {
            //            new TrkOffControl(TrkOffOption.fade),
            //        });
            //}
            //// actually only bar 4
            //foreach(InputChordDef inputChordDef in inputVoiceDef.InputChordDefs)
            //{
            //    inputChordDef.InputNoteDefs[0].TrkOptions = new TrkOptions(new List<TrkOption>()
            //        {
            //            new TrkOffControl(TrkOffOption.stopChord),
            //        });
            //}
            //// actually only bar 5
            //foreach(InputChordDef inputChordDef in inputVoiceDef.InputChordDefs)
            //{
            //    inputChordDef.InputNoteDefs[0].TrkOptions = new TrkOptions(new SpeedControl(2.2F));
            //}
        }

        #region CreateBar1Seq()
        private Seq CreateBar1Seq()
        {
            List<Trk> bar = new List<Trk>();

            byte channel = 0;
            int endBarlineMsPos = 0;
            foreach(Palette palette in _palettes)
            {
                Trk trk = new Trk(channel, 0, new List<IUniqueDef>());
                bar.Add(trk);
                WriteVoiceMidiDurationDefs1(trk, palette);
                endBarlineMsPos = (endBarlineMsPos > trk.MsDuration) ? endBarlineMsPos : trk.MsDuration;
                channel++;
            }

            //bar[0].InsertClefDef(5, "t");

            Seq seq = new Seq(0, bar, MidiChannelIndexPerOutputVoice);

            return seq;
        }

        private void WriteVoiceMidiDurationDefs1(Trk trk, Palette palette)
        {
            trk.MsPositionReContainer = 0;

            int msPositionReFirstIUD = 0;
            int bar1ChordMsSeparation = 1500;
            for(int i = 0; i < palette.Count; ++i)
            {
                IUniqueDef durationDef = palette.UniqueDurationDef(i);
                durationDef.MsPositionReFirstUD = msPositionReFirstIUD;
                MidiRestDef restDef = new MidiRestDef(msPositionReFirstIUD + durationDef.MsDuration, bar1ChordMsSeparation - durationDef.MsDuration);
                msPositionReFirstIUD += bar1ChordMsSeparation;
                trk.UniqueDefs.Add(durationDef);
                trk.UniqueDefs.Add(restDef);
            }
        }
        #endregion CreateBar1Seq()

        /// <summary>
        /// This function creates only one bar, using Trk objects. 
        /// </summary>
        private Seq CreateBar2Seq()
        {
            List<Trk> bar = new List<Trk>();

            byte channel = 0;
            foreach(Palette palette in _palettes)
            {
                Trk trk = palette.NewTrk(channel);
                trk.MsPositionReContainer = 0;
                trk.MsDuration = 6000; // stretches or compresses the trk duration to 6000ms
                bar.Add(trk);
                ++channel;
            }

            int maxMsPosReBar = 0;
            // insert rests at the start of the Trks
            int restMsDuration = 0;
            foreach(Trk trk in bar)
            {
                if(restMsDuration > 0)
                {
                    MidiRestDef restDef = new MidiRestDef(0, restMsDuration);
                    trk.Insert(0, restDef);
                }
                restMsDuration += 1500;
                maxMsPosReBar = trk.EndMsPositionReFirstIUD;
            }

            // add the final rest in the bar
            foreach(Trk trk in bar)
            {
                int trkEndMsPosReBar = trk.EndMsPositionReFirstIUD;
                if(maxMsPosReBar > trkEndMsPosReBar)
                {
                    MidiRestDef restDef = new MidiRestDef(trkEndMsPosReBar, maxMsPosReBar - trkEndMsPosReBar);
                    trk.Add(restDef);
                }
            }

            Seq seq = new Seq(0, bar, MidiChannelIndexPerOutputVoice);

            return seq;
        }
    }
}
