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
        }

        public override IReadOnlyList<int> MidiChannelIndexPerOutputVoice { get { return new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7 }; } }
        public override IReadOnlyList<int> MasterVolumePerOutputVoice { get { return new List<int>() { 100, 100, 100, 100, 100, 100, 100, 100 }; } }
        public override int NumberOfInputVoices { get { return 1; } }  
        public override int NumberOfBars { get { return 5; } }

        /// <summary>
        /// See CompositionAlgorithm.DoAlgorithm()
        /// </summary>
        public override List<List<VoiceDef>> DoAlgorithm(List<Krystal> krystals, List<Palette> palettes)
        {
            _krystals = krystals;
            _palettes = palettes;

            List<int> barlineMsPositions = new List<int>();

            Seq bar1Seq = CreateBar1Seq();
            barlineMsPositions.Add(bar1Seq.MsDuration);

            Seq bar2Seq = CreateBar2Seq();
            barlineMsPositions.Add(bar1Seq.MsDuration + bar2Seq.MsDuration);

            Seq bars345Seq = bar2Seq.Clone();
            barlineMsPositions.Add(bar1Seq.MsDuration + bar2Seq.MsDuration + 5950);
            barlineMsPositions.Add(bar1Seq.MsDuration + bar2Seq.MsDuration + 10500);
            barlineMsPositions.Add(bar1Seq.MsDuration + bar2Seq.MsDuration + bars345Seq.MsDuration);

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

            InputVoiceDef ivd = bar1InputVoiceDef;
            ivd.Concat(bar2InputVoiceDef);
            ivd.Concat(bars345InputVoiceDef);

            Seq mainSeq = bar1Seq;
            mainSeq.Concat(bar2Seq);
            mainSeq.Concat(bars345Seq);

            List<InputVoiceDef> inputVoiceDefs = new List<InputVoiceDef>() { ivd };

            MainBlock mainBlock = new MainBlock(InitialClefPerChannel, MidiChannelIndexPerOutputVoice, inputVoiceDefs.Count);
            Block block = new Block(mainSeq, inputVoiceDefs);
            mainBlock.Concat(block);

            mainBlock.AddBarlines(barlineMsPositions);
            mainBlock.AdjustBarlinePositionsForInputVoices();

            List<List<VoiceDef>> bars = mainBlock.ConvertToBars();

            return bars;
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
                RestDef tRestDef = tIud as RestDef;
                MidiChordDef tmcd = tIud as MidiChordDef;
                if(tRestDef != null)
                {
                    RestDef iRestDef = new RestDef(tRestDef.MsPositionReFirstUD, tRestDef.MsDuration);
                    ivd.Add(iRestDef);
                }
                else if(tmcd != null)
                {
                    List<TrkRef> trkRefs = new List<TrkRef>();
                    
                    foreach(Trk trk in bar1Seq.Trks)
                    {
                        trkRefs.Add(new TrkRef((byte)trk.MidiChannel, bar1Seq.AbsMsPosition + tmcd.MsPositionReFirstUD, 1, null));
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

            foreach(Trk trk in bar2Seq.Trks)
            {
                MidiChordDef firstMidiChordDef = null;
                foreach(IUniqueDef iud in trk.UniqueDefs)
                {
                    firstMidiChordDef = iud as MidiChordDef;
                    if(firstMidiChordDef != null)
                    {
                        List<TrkRef> trkRefs = new List<TrkRef>();
                        trkRefs.Add(new TrkRef((byte)trk.MidiChannel, bar2Seq.AbsMsPosition + firstMidiChordDef.MsPositionReFirstUD, 12, null));
                        SeqRef seqRef = new SeqRef(trkRefs, null);
                        NoteOn noteOn = new NoteOn(seqRef);
                        List<InputNoteDef> inputNoteDefs = new List<InputNoteDef>();
                        inputNoteDefs.Add(new InputNoteDef((byte)65, noteOn, null));
                        InputChordDef icd = new InputChordDef(firstMidiChordDef.MsPositionReFirstUD, 1500, inputNoteDefs, M.Dynamic.none, null);
                        ivd.Add(icd);
                        break;
                    }
                }
            }

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
                RestDef restDef = new RestDef(msPositionReFirstIUD + durationDef.MsDuration, bar1ChordMsSeparation - durationDef.MsDuration);
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
                    RestDef restDef = new RestDef(0, restMsDuration);
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
                    RestDef restDef = new RestDef(trkEndMsPosReBar, maxMsPosReBar - trkEndMsPosReBar);
                    trk.Add(restDef);
                }
            }

            Seq seq = new Seq(0, bar, MidiChannelIndexPerOutputVoice);

            return seq;
        }
    }
}
