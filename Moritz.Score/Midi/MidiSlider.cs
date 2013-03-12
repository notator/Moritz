using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading;

using Multimedia.Midi;

namespace Moritz.Score.Midi
{
	/// <summary>
	/// MidiSliders are MidiControls whose value can change gradually over time. In scores, each of these takes
	/// a byte argument (in the range 0..127) denoting the MSB values for the particular slider.
	/// Continuation characters: In addition to the msb argument, each slider text may end with a
	/// character which shows if and how the value subequently changes (until the next instance of the same
	/// slider:
	///   a) '-' the slider's value changes once per chord, in equal steps per chord
	///   b) '_' the slider's value changes once per chord, in equal steps per logical position
	///   c) '+' the slider's value changes continuously, in equal steps per chord
	///   d) '~' the slider's value changes continuously, in equal steps per logical position
	///   Examples:
	///         pan50~   pan23   pan80-  pan20
	///	The initial Instrument and Volume settings come from the capella file.
	///	All Sliders have a default value of 0, except
	///			volume 127 (alias v, vol)
	///			expression 64 (alias e, exp, expr)
	///			balance 64 (alias b, bal)
	///			pan 64 (alias p)
	///			pitchwheel 64 (alias pw)
	///	The default value is set when the control has no numeric argument. 
	/// </summary>
	public abstract class MidiSlider : MidiControl
	{
        //protected MidiSlider(int channel, ChannelCommand command, MidiControlSymbol midiControlSymbol)
        //    : base(channel, command, midiControlSymbol.MSB)
        //{
        //    byte initialMsb = midiControlSymbol.MSB;
        //    uint intFinalMsb = (uint)(initialMsb + (byte)(midiControlSymbol.ValueChangePerSemibreve * midiControlSymbol.LogicalWidth.ToFloat()));
        //    byte finalMsb = (byte)(intFinalMsb > 127 ? 127 : intFinalMsb); 
        //    ContinuousMidiMessages.Clear();
        //    for(byte msb = initialMsb; msb <= finalMsb; ++msb)
        //    {
        //        List<ChannelMessage> synchMessages = new List<ChannelMessage>();
        //        synchMessages.Add(BuildMessage(command, channel, 0, msb));
        //        ContinuousMidiMessages.Add(synchMessages);
        //    }
        //    Initialize(initialMsb, finalMsb, midiControlSymbol.Continuation);
        //}
        protected MidiSlider(int channel, ChannelCommand command, byte msb, ControlContinuation continuation)
            : base(channel, command, msb)
        {
            Initialize(msb, msb, continuation);
        }
        protected MidiSlider(int channel, ChannelCommand command, byte msb, byte lsb, ControlContinuation continuation)
            : base(channel, command, msb, lsb)
        {
            Initialize(msb, msb, continuation); // both msb! (intial==final)
        }
        protected MidiSlider(int channel, ControllerType controller, byte msb, ControlContinuation continuation)
			: base(channel, controller, msb)
		{
            Initialize(msb, msb, continuation);
		}
        //protected MidiSlider(int channel, ControllerType controller, MidiControlSymbol midiControlSymbol)
        //    : base(channel, controller, midiControlSymbol.MSB)
        //{
        //    byte initialMsb = midiControlSymbol.MSB;
        //    uint intFinalMsb = (uint)(initialMsb + (byte)(midiControlSymbol.ValueChangePerSemibreve * midiControlSymbol.LogicalWidth.ToFloat()));
        //    byte finalMsb = (byte)(intFinalMsb > 127 ? 127 : intFinalMsb);
        //    ContinuousMidiMessages.Clear();
        //    for(byte msb = initialMsb; msb <= finalMsb; ++msb)
        //    {
        //        List<ChannelMessage> synchMessages = new List<ChannelMessage>();
        //        synchMessages.Add(BuildMessage(ChannelCommand.Controller, channel, (int)controller, msb));
        //        ContinuousMidiMessages.Add(synchMessages);
        //    }
        //    Initialize(initialMsb, finalMsb, midiControlSymbol.Continuation);
        //}

        private void Initialize(byte initialMsb, byte finalMsb, ControlContinuation continuation)
        {
            _initialMsb = initialMsb;
            _finalMsb = finalMsb;
            _controlContinuation = continuation;
        }

        #region Continuous Sliders *****************************************
		private class SliderBWArgs
		{
			public int DurationMilliseconds;
			public ChannelMessageDelegate SendChannelMessage;
		}
		public void SliderBW_DoWork(object sender, DoWorkEventArgs e)
		{
            BackgroundWorker worker = sender as BackgroundWorker;
            Debug.Assert(worker != null);

            try
            {
                SliderBWArgs args = (SliderBWArgs)e.Argument;
                lock(worker)
                {
                    int nMessages = ContinuousMidiMessages.Count;
                    int sleepBetween = args.DurationMilliseconds / nMessages;
                    foreach(List<ChannelMessage> nowMessages in ContinuousMidiMessages)
                    {
                        foreach(ChannelMessage msg in nowMessages)
                        {
                            args.SendChannelMessage(msg);
                        }
                        if(worker.CancellationPending)
                            break;
                        Thread.Sleep(sleepBetween);
                    }
                }
            }
            catch(Exception ex)
            {
                e.Result = ex.Message; // SliderBW_Completed() called now...
            }
        }
		private void SliderBW_Completed(object sender, RunWorkerCompletedEventArgs e)
		{
            BackgroundWorker worker = sender as BackgroundWorker;
            if(worker != null)
            {
                worker.DoWork -= new DoWorkEventHandler(SliderBW_DoWork);
                worker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(SliderBW_Completed);
            }
            string exceptionErrorMessage = e.Result as string;
            if(!string.IsNullOrEmpty(exceptionErrorMessage))
                throw new PerformanceErrorException(exceptionErrorMessage);
		}
 		/// <summary>
		/// Starts a continuous slider, so that it sends its ContinuousMidiMessages spread out over durationMilliseconds.
		/// </summary>
		/// <param name="durationMilliseconds"></param>
		public void Start(BackgroundWorker sliderBW, int durationMilliseconds, ChannelMessageDelegate sendChannelMessage)
		{
			Debug.Assert(durationMilliseconds > 0);
            Debug.Assert(sliderBW != null);
            Debug.Assert(!sliderBW.IsBusy);

            if(ContinuousMidiMessages.Count > 0 && sliderBW != null)
            {
                SliderBWArgs args = new SliderBWArgs() 
                { 
                    DurationMilliseconds = durationMilliseconds, 
                    SendChannelMessage = sendChannelMessage 
                };

                sliderBW.DoWork += new DoWorkEventHandler(SliderBW_DoWork);
                sliderBW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(SliderBW_Completed);
                sliderBW.RunWorkerAsync(args);
            }
		}

		public byte FinalMsb { get { return _finalMsb; } set { _finalMsb = value; } }
		private byte _finalMsb = 0;

        #endregion Continuous Sliders *****************************************

        /// <summary>
        /// The messages in each inner list are sent at (conceptually) the same time. The sleep time between the
        /// sending of the inner messages depends on the milliseconds duration of the containing midiDurationClass.
        /// </summary>
        public List<List<ChannelMessage>> ContinuousMidiMessages = new List<List<ChannelMessage>>();

		public byte Msb { get { return _initialMsb; } }
		private byte _initialMsb = 0;
		public ControlContinuation ControlContinuation { get{return _controlContinuation;} }
		private ControlContinuation _controlContinuation = ControlContinuation.NoChange;
	}

	#region long (2-byte) sliders
	public class PitchWheel : MidiSlider
	{
        /// <summary>
        /// Achtung: this slider needs its lsb to be set!
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="lsb"></param>
        /// <param name="continuation"></param>
        public PitchWheel(int channel, byte lsb, ControlContinuation continuation)
            : base(channel, ChannelCommand.PitchWheel, 0, lsb, continuation)
        {
        }
    }
	public class ModulationWheel : MidiSlider
	{
        //public ModulationWheel(int channel, MidiControlSymbol midiControlSymbol)
        //    : base(channel, ControllerType.ModulationWheel, midiControlSymbol)
        //{
        //}
		public ModulationWheel(int channel, byte msb, ControlContinuation continuation)
			: base(channel, ControllerType.ModulationWheel, msb, continuation)
		{
		}
	}
	public class BreathController : MidiSlider
	{
        //public BreathController(int channel, MidiControlSymbol midiControlSymbol)
        //    : base(channel, ControllerType.BreathControl, midiControlSymbol)
        //{
        //}
		public BreathController(int channel, byte msb, ControlContinuation continuation)
			: base(channel, ControllerType.BreathControl, msb, continuation)
		{
		}
	}
	public class FootPedal : MidiSlider
	{
        //public FootPedal(int channel, MidiControlSymbol midiControlSymbol)
        //    : base(channel, ControllerType.FootPedal, midiControlSymbol)
        //{
        //}
        public FootPedal(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.FootPedal, msb, continuation)
        {
        }
    }
	public class PortamentoTime : MidiSlider
	{
        //public PortamentoTime(int channel, MidiControlSymbol midiControlSymbol)
        //    : base(channel, ControllerType.PortamentoTime, midiControlSymbol)
        //{
        //}
        public PortamentoTime(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.PortamentoTime, msb, continuation)
        {
        }
    }
	public class Volume : MidiSlider
	{
        //public Volume(int channel, MidiControlSymbol midiControlSymbol)
        //    : base(channel, ControllerType.Volume, midiControlSymbol)
        //{
        //    //  ControllerType.VolumeFine is ignored
        //}
        public Volume(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.Volume, msb, continuation)
        {
            //  ControllerType.VolumeFine is ignored
        }
    }
	public class Balance : MidiSlider
	{
        //public Balance(int channel, MidiControlSymbol midiControlSymbol)
        //    : base(channel, ControllerType.Balance, midiControlSymbol)
        //{
        //    //  ControllerType.BalanceFine is ignored.
        //}
        public Balance(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.Balance, msb, continuation)
        {
             //  ControllerType.BalanceFine is ignored.
        }
    }
	public class Pan : MidiSlider
	{
        //public Pan(int channel, MidiControlSymbol midiControlSymbol)
        //    : base(channel, ControllerType.Pan, midiControlSymbol)
        //{
        //    //  ControllerType.PanFine is ignored.
        //}
        public Pan(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.Pan, msb, continuation)
        {
            //  ControllerType.PanFine is ignored.
        }
    }
	public class Expression : MidiSlider
	{
        //public Expression(int channel, MidiControlSymbol midiControlSymbol)
        //    : base(channel, ControllerType.Expression, midiControlSymbol)
        //{
        //    //  ControllerType.ExpressionFine is ignored
        //}
        public Expression(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.Expression, msb, continuation)
        {
            //  ControllerType.ExpressionFine is ignored
        }
    }
	public class EffectControl1 : MidiSlider
	{
        //public EffectControl1(int channel, MidiControlSymbol midiControlSymbol)
        //    : base(channel, ControllerType.EffectControl1, midiControlSymbol)
        //{
        //}
        public EffectControl1(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.EffectControl1, msb, continuation)
        {
        }
    }
	public class EffectControl2 : MidiSlider
	{
        //public EffectControl2(int channel, MidiControlSymbol midiControlSymbol)
        //    : base(channel, ControllerType.EffectControl2, midiControlSymbol)
        //{
        //}
        public EffectControl2(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.EffectControl2, msb, continuation)
        {
        }
    }
	#endregion long (2-byte) sliders
	#region short (1-byte) sliders
	public class ChannelPressure : MidiSlider
	{
        //public ChannelPressure(int channel, MidiControlSymbol midiControlSymbol)
        //    : base(channel, ChannelCommand.ChannelPressure, midiControlSymbol)
        //{
        //}
        public ChannelPressure(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ChannelCommand.ChannelPressure, msb, continuation)
        {
        }
    }
	public class GeneralPurposeSlider1 : MidiSlider
	{
        //public GeneralPurposeSlider1(int channel, MidiControlSymbol midiControlSymbol)
        //    : base(channel, ControllerType.GeneralPurposeSlider1, midiControlSymbol)
        //{
        //}
        public GeneralPurposeSlider1(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.GeneralPurposeSlider1, msb, continuation)
        {
        }
    }
	public class GeneralPurposeSlider2 : MidiSlider
	{
        //public GeneralPurposeSlider2(int channel, MidiControlSymbol midiControlSymbol)
        //    : base(channel, ControllerType.GeneralPurposeSlider2, midiControlSymbol)
        //{
        //}
        public GeneralPurposeSlider2(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.GeneralPurposeSlider2, msb, continuation)
        {
        }
    }
	public class GeneralPurposeSlider3 : MidiSlider
	{
        //public GeneralPurposeSlider3(int channel, MidiControlSymbol midiControlSymbol)
        //    : base(channel, ControllerType.GeneralPurposeSlider3, midiControlSymbol)
        //{
        //}
        public GeneralPurposeSlider3(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.GeneralPurposeSlider3, msb, continuation)
        {
        }
    }
	public class GeneralPurposeSlider4 : MidiSlider
	{
        //public GeneralPurposeSlider4(int channel, MidiControlSymbol midiControlSymbol)
        //    : base(channel, ControllerType.GeneralPurposeSlider4, midiControlSymbol)
        //{
        //}
        public GeneralPurposeSlider4(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.GeneralPurposeSlider4, msb, continuation)
        {
        }
    }
	public class SoundVariation : MidiSlider
	{
        //public SoundVariation(int channel, MidiControlSymbol midiControlSymbol)
        //    : base(channel, ControllerType.SoundVariation, midiControlSymbol)
        //{
        //}
        public SoundVariation(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.SoundVariation, msb, continuation)
        {
        }
    }
	public class SoundTimbre : MidiSlider
	{
        //public SoundTimbre(int channel, MidiControlSymbol midiControlSymbol)
        //    : base(channel, ControllerType.SoundTimbre, midiControlSymbol)
        //{
        //}
        public SoundTimbre(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.SoundTimbre, msb, continuation)
        {
        }

	}
	public class SoundReleaseTime : MidiSlider
	{
        //public SoundReleaseTime(int channel, MidiControlSymbol midiControlSymbol)
        //    : base(channel, ControllerType.SoundReleaseTime, midiControlSymbol)
        //{
        //}
        public SoundReleaseTime(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.SoundReleaseTime, msb, continuation)
        {
        }
    }
	public class SoundAttackTime : MidiSlider
	{
        //public SoundAttackTime(int channel, MidiControlSymbol midiControlSymbol)
        //    : base(channel, ControllerType.SoundAttackTime, midiControlSymbol)
        //{
        //}
        public SoundAttackTime(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.SoundAttackTime, msb, continuation)
        {
        }
    }
	public class SoundBrightness : MidiSlider
	{
        //public SoundBrightness(int channel, MidiControlSymbol midiControlSymbol)
        //    : base(channel, ControllerType.SoundBrightness, midiControlSymbol)
        //{
        //}
        public SoundBrightness(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.SoundBrightness, msb, continuation)
        {
        }
    }
	public class SoundControl6 : MidiSlider
	{
        //public SoundControl6(int channel, MidiControlSymbol midiControlSymbol)
        //    : base(channel, ControllerType.SoundControl6, midiControlSymbol)
        //{
        //}
        public SoundControl6(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.SoundControl6, msb, continuation)
        {
        }
    }
	public class SoundControl7 : MidiSlider
	{
        //public SoundControl7(int channel, MidiControlSymbol midiControlSymbol)
        //    : base(channel, ControllerType.SoundControl7, midiControlSymbol)
        //{
        //}
        public SoundControl7(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.SoundControl7, msb, continuation)
        {
        }
    }
	public class SoundControl8 : MidiSlider
	{
        //public SoundControl8(int channel, MidiControlSymbol midiControlSymbol)
        //    : base(channel, ControllerType.SoundControl8, midiControlSymbol)
        //{
        //}
        public SoundControl8(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.SoundControl8, msb, continuation)
        {
        }
    }
	public class SoundControl9 : MidiSlider
	{
        //public SoundControl9(int channel, MidiControlSymbol midiControlSymbol)
        //    : base(channel, ControllerType.SoundControl9, midiControlSymbol)
        //{
        //}
        public SoundControl9(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.SoundControl9, msb, continuation)
        {
        }
    }
	public class SoundControl10 : MidiSlider
	{
        //public SoundControl10(int channel, MidiControlSymbol midiControlSymbol)
        //    : base(channel, ControllerType.SoundControl10, midiControlSymbol)
        //{
        //}
        public SoundControl10(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.SoundControl10, msb, continuation)
        {
        }
    }
	public class EffectsLevel : MidiSlider
	{
        //public EffectsLevel(int channel, MidiControlSymbol midiControlSymbol)
        //    : base(channel, ControllerType.EffectsLevel, midiControlSymbol)
        //{
        //}
        public EffectsLevel(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.EffectsLevel, msb, continuation)
        {
        }
    }
	public class TremoloLevel : MidiSlider
	{
        //public TremoloLevel(int channel, MidiControlSymbol midiControlSymbol)
        //    : base(channel, ControllerType.TremeloLevel, midiControlSymbol)
        //{
        //}
        public TremoloLevel(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.TremeloLevel, msb, continuation)
        {
        }
    }
	public class ChorusLevel : MidiSlider
	{
        //public ChorusLevel(int channel, MidiControlSymbol midiControlSymbol)
        //    : base(channel, ControllerType.ChorusLevel, midiControlSymbol)
        //{
        //}
        public ChorusLevel(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.ChorusLevel, msb, continuation)
        {
        }
    }
	public class CelesteLevel : MidiSlider
	{
        //public CelesteLevel(int channel, MidiControlSymbol midiControlSymbol)
        //    : base(channel, ControllerType.CelesteLevel, midiControlSymbol)
        //{
        //}
        public CelesteLevel(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.CelesteLevel, msb, continuation)
        {
        }
    }
    public class PhaserLevel : MidiSlider
    {
        //public PhaserLevel(int channel, MidiControlSymbol midiControlSymbol)
        //    : base(channel, ControllerType.PhaserLevel, midiControlSymbol)
        //{
        //}
        public PhaserLevel(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.PhaserLevel, msb, continuation)
        {
        }
    }
    #endregion short (1-byte) sliders
    #region MidiSlider which does not send MidiMessages
    public class VerticalVelocityFactor : MidiSlider
    {
        //public VerticalVelocityFactor(int channel, MidiControlSymbol midiControlSymbol)
        //    : base(channel, ControllerType.SoundControl9, midiControlSymbol)
        //    // The controller type is a dummy (Midi messages are never sent by this MidiSlider)
        //{
        //    this.ChannelMessages.Clear();
        //}
        public VerticalVelocityFactor(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.SoundControl9, msb, continuation)
            // The controller type is a dummy (Midi messages are never sent by this MidiSlider)
        {
            this.ChannelMessages.Clear();
        }
    }
    #endregion MidiSlider which does not send MidiMessages


}
