using System.ComponentModel;
using System.Diagnostics;
using System.Collections.Generic;

namespace Moritz.Midi
{
    /// <summary>
    /// MidiControls which change the MIDI Expression value in their own time.
    /// Only one articulation is possible per chord because all articulations use the same midi controller (Expression).
    /// A default Articulation is set in the MidiSoundingDurationSymbol class's constructor. This sets the channel expression
    /// to the value of currentStaffState.Expression, and is overridden if the object has an explicit Articulation written
    /// in the score.
    /// Articulations change the channel's expression value without changing currentStaffState.Expression.
    /// currentStaffState.Expression is changed using the staff 'e' control.
    /// </summary>
    public abstract class MidiChordSlider : MidiControl
    {
        protected MidiChordSlider()
            : base()
        {
        }

        protected MidiChordSlider(ChordSliderType chordSliderType, List<byte> values, int channel, int noteDurationMilliseconds)
            : base()
        {
            Debug.Assert(values.Count > 0);
            if(values.Count == 1)
            {
                SetSingleMidiChordSlider(chordSliderType, channel, values[0]);
            }
            else // args.Count > 1
                if(values.Count > 1)
            {
                int msTimeBetweenPeaks = noteDurationMilliseconds / (values.Count - 1); // time between peaks
                int msPosition = 0;
                byte value = values[0]; // initial value

                for(int i = 1; i < values.Count; i++)
                {
                    int msPositionOfNextPeak = msTimeBetweenPeaks * i;
                    byte valueAtNextPeak = values[i]; // value at coming peak

                    SetMidiChordSlider(chordSliderType, channel, msPosition, msPositionOfNextPeak, value, valueAtNextPeak);

                    msPosition = msPositionOfNextPeak;
                    value = valueAtNextPeak;
                }
            }
        }


        /// <summary>
        /// Returns a value in range 0..127.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected byte MidiByte(double value)
        {
            double rval = (value > 127 ? 127 : value);
            rval = value < 0 ? 0 : value;
            return (byte)rval;
        }

        private void SetSingleMidiChordSlider(ChordSliderType chordSliderType, int channel, byte value)
        {
            MidiSliderTime midiSliderTime = null;
            switch(chordSliderType)
            {
                case ChordSliderType.Pitchwheel:
                    midiSliderTime =
                        new MidiSliderTime(new PitchWheel(channel, value, ControlContinuation.NoChange), 0);
                    break;
                case ChordSliderType.Pan:
                    midiSliderTime =
                        new MidiSliderTime(new Pan(channel, value, ControlContinuation.NoChange), 0);
                    break;
                case ChordSliderType.ModulationWheel:
                    midiSliderTime =
                        new MidiSliderTime(new ModulationWheel(channel, value, ControlContinuation.NoChange), 0);
                    break;
                case ChordSliderType.Expression:
                    midiSliderTime =
                        new MidiSliderTime(new Expression(channel, value, ControlContinuation.NoChange), 0);
                    break;
                default:
                    break;
            }
            if(midiSliderTime != null)
                MidiSliderTimes.Add(midiSliderTime);
        }


        /// <summary>
        /// Creates MidiSliderTime objects and adds them to MidiSliderTimes list so as to create a slide from 
        /// currentValue to endValue from msStartPosition up to (but not including) msEndPosition. 
        /// </summary>
        protected void SetMidiChordSlider(ChordSliderType chordSliderType, int channel, int currentMsPosition, int endMsPosition, 
            byte currentValue, byte endValue)
        {
            Debug.Assert(endMsPosition >= currentMsPosition);

            float slideValueDelta = 0f;
            float slideDuration = endMsPosition - currentMsPosition;
            float floatCurrentValue = (float)currentValue;
            float floatEndValue = (float)endValue;

            if(slideDuration != 0)
                slideValueDelta = (floatEndValue - floatCurrentValue) / (slideDuration / _defaultSleepTime);

            do
            {
                int sleepTime = endMsPosition - currentMsPosition;
                sleepTime = (sleepTime > _defaultSleepTime) ? _defaultSleepTime : sleepTime;
                MidiSliderTime midiSliderTime = null;
                switch(chordSliderType)
                {
                    case ChordSliderType.Pitchwheel:
                        midiSliderTime =
                            new MidiSliderTime(new PitchWheel(channel, (byte)floatCurrentValue, ControlContinuation.NoChange), sleepTime);
                        break;
                    case ChordSliderType.Pan:
                        midiSliderTime =
                            new MidiSliderTime(new Pan(channel, (byte)floatCurrentValue, ControlContinuation.NoChange), sleepTime);
                        break;
                    case ChordSliderType.ModulationWheel:
                        midiSliderTime =
                            new MidiSliderTime(new ModulationWheel(channel, (byte)floatCurrentValue, ControlContinuation.NoChange), sleepTime);
                        break;
                    case ChordSliderType.Expression:
                        midiSliderTime = 
                            new MidiSliderTime(new Expression(channel, (byte)floatCurrentValue, ControlContinuation.NoChange), sleepTime);
                        break;
                    default:
                        break;
                }
                if(midiSliderTime == null)
                    break;
                MidiSliderTimes.Add(midiSliderTime);
                currentMsPosition += sleepTime;
                floatCurrentValue += slideValueDelta;
            } while(currentMsPosition < endMsPosition);
        }

        protected readonly int _defaultSleepTime = 30;

        public BackgroundWorker BackgroundWorker = null;
        public List<MidiSliderTime> MidiSliderTimes = new List<MidiSliderTime>();

    }

    /// <summary>
    /// The DefaultArticulation is sent whenever the MidiDurationSymbol has no other defined Articulation.
    /// It sets the current Expression value to the value of currentStaffState.Expression.
    /// currentStaffState.Expression is set by the staff 'e' command, never by articulations.
    /// </summary>
    public class DefaultArticulation : MidiChordSlider
    {
        public DefaultArticulation(int channel, byte channelExpressionState)
            : base()
        {
            SetMidiChordSlider(ChordSliderType.Expression, channel, 0, 0, channelExpressionState, 0);
        }
    }
    /// <summary>
    /// A capella3 dot character (char(46)) (Augmentation dots are different.)
    /// A Staccato shortens the sounding part of the note, and makes a diminuendo of the
    /// expression value (to zero) while the note is sounding.
    /// </summary>
    public class Staccato : MidiChordSlider
    {
        public Staccato(int channel, byte channelExpressionState, int noteDurationMilliseconds)
            : base()
        {
            int staccatoDuration = noteDurationMilliseconds / 2;
            staccatoDuration = (staccatoDuration > 900) ? 900 : staccatoDuration;
            int curentMsPosition = 0;
            byte currentExpression = channelExpressionState;
            SetMidiChordSlider(ChordSliderType.Expression, channel, curentMsPosition, staccatoDuration, currentExpression, 0);
        }
    }
    /// <summary>
    /// A solid wedge - either capella3 char(201) or char(206) (wedge above or below).
    /// A HardStaccato shortens the sounding part of the note (as in an ordinary staccato),
    /// but does not diminuendo while sounding.
    /// </summary>
    public class HardStaccato : MidiChordSlider
    {
        public HardStaccato(int channel, byte channelExpressionState, int noteDurationMilliseconds)
            : base()
        {
            int hardStaccatoDuration = noteDurationMilliseconds / 2;
            hardStaccatoDuration = (hardStaccatoDuration > 900) ? 900 : hardStaccatoDuration;

            // set expression to 100 for hardStaccatoDuration
            MidiSliderTime eT = new MidiSliderTime(
                new Expression(channel, 127, ControlContinuation.NoChange), hardStaccatoDuration);
            MidiSliderTimes.Add(eT);

            // set expression to zero
            MidiSliderTime eTz = new MidiSliderTime(
                new Expression(channel, 0, ControlContinuation.NoChange), 0);
            MidiSliderTimes.Add(eTz);
        }
    }
    /// <summary>
    /// A capella3 tenuto symbol (char(200))
    /// A Tenuto makes a small crescendo from the original expression state, and shortens
    /// the sounding part of the note slightly, to separate it from the following note.
    /// </summary>
    public class Tenuto : MidiChordSlider
    {
        public Tenuto(int channel, byte channelExpressionState, int noteDurationMilliseconds)
            : base()
        {
            int tenutoDuration = 0;
            if(noteDurationMilliseconds > 1000)
                tenutoDuration = noteDurationMilliseconds - 200;
            else
                tenutoDuration = noteDurationMilliseconds * 8 / 10;

            int crescDuration = tenutoDuration / 2;

            int msPosition = 0;
            byte expression = channelExpressionState; // initial value for tenuto
            byte maxExpression = MidiByte((int)(expression * 1.3f));

            SetMidiChordSlider(ChordSliderType.Expression, channel, msPosition, crescDuration, expression, maxExpression);

            // fix expression for the rest of the tenuto
            MidiSliderTime eTe = new MidiSliderTime(
                new Expression(channel, maxExpression, ControlContinuation.NoChange), tenutoDuration - crescDuration);
            MidiSliderTimes.Add(eTe);

            // set expression to zero
            MidiSliderTime eTz = new MidiSliderTime(
                new Expression(channel, 0, ControlContinuation.NoChange), 0);
            MidiSliderTimes.Add(eTz);
        }
    }
    /// <summary>
    /// A cappela3 accent (char(202))
    /// An Accent begins the note at 100% expression, and makes a short diminuendo to the original expression state
    /// </summary>
    public class Accent : MidiChordSlider
    {
        public Accent(int channel, byte channelExpressionState, int noteDurationMilliseconds)
            : base()
        {
            int diminuendoTime = noteDurationMilliseconds / 2;
            diminuendoTime = (diminuendoTime > 600) ? 600 : diminuendoTime;

            int msPosition = 0;
            byte expression = 127; // initial value for accent

            SetMidiChordSlider(ChordSliderType.Expression, channel, msPosition, diminuendoTime, expression, channelExpressionState);
        }
    }
    /// <summary>
    /// A cappela3 strong accent (char(203))
    /// A StrongAccent begins the note at 100% expression, waits for a short time,
    /// then diminuendos in two stages to the current channelExpressionState.
    /// </summary>
    public class StrongAccent : MidiChordSlider
    {
        public StrongAccent(int channel, byte channelExpressionState, int noteDurationMilliseconds)
            : base()
        {
            int accentDuration = noteDurationMilliseconds / 3;
            accentDuration = (accentDuration > 900) ? 900 : accentDuration;

            int loudDuration = accentDuration / 2;

            // set expression to maximum for loudDuration
            byte expression = 127; // initial value for accent
            MidiSliderTime eTz = new MidiSliderTime(
                new Expression(channel, expression, ControlContinuation.NoChange), loudDuration);
            MidiSliderTimes.Add(eTz);

            int msPosition = loudDuration;
            int dimEndTime = msPosition * 2;
            byte sustainExpr = MidiByte((channelExpressionState + 127) / 2);

            SetMidiChordSlider(ChordSliderType.Expression, channel, msPosition, dimEndTime, expression, sustainExpr);
            SetMidiChordSlider(ChordSliderType.Expression, channel, dimEndTime, noteDurationMilliseconds, sustainExpr, channelExpressionState);
        }
    }
    /// <summary>
    /// A cappela3 sforzato character (char(115))
    /// Sforzato is like a StrongAccent, except that the Accent's initial diminuendo is replaced by
    /// a crescendo-diminuendo of the same length, and the continuation of the note
    /// begins at a higher expression value than the expression state. 
    /// </summary>
    public class Sforzato : MidiChordSlider
    {
        public Sforzato(int channel, byte channelExpressionState, int noteDurationMilliseconds)
            : base()
        {
            int msPosition = 0;
            byte expression = 127; // maximum value            
            int crescTime = noteDurationMilliseconds / 4;
            crescTime = (crescTime > 450) ? 450 : crescTime;
            SetMidiChordSlider(ChordSliderType.Expression, channel, msPosition, crescTime, expression, 127);

            int crescDimTime = crescTime * 2;
            byte sustainExpr = MidiByte((channelExpressionState + 100) / 2);
            SetMidiChordSlider(ChordSliderType.Expression, channel, crescTime, crescDimTime, 127, sustainExpr);
            SetMidiChordSlider(ChordSliderType.Expression, channel, crescDimTime, noteDurationMilliseconds, sustainExpr, channelExpressionState);
        }
    }
    /// <summary>
    /// A capella3 forzato character (char(123))
    /// A Forzato is like an Accent, except that (as in a Sforzato) the continuation of the note
    /// begins at a higher expression value than the expression state. 
    /// </summary>
    public class Forzato : MidiChordSlider
    {
        public Forzato(int channel, byte channelExpressionState, int noteDurationMilliseconds)
            : base()
        {

            int msPosition = 0;
            byte expression = 127; // initial value for accent
            byte sustainExpr = MidiByte((channelExpressionState + 100) / 2);
            int diminuendoTime = noteDurationMilliseconds / 3;
            diminuendoTime = (diminuendoTime > 750) ? 750 : diminuendoTime;

            SetMidiChordSlider(ChordSliderType.Expression, channel, msPosition, diminuendoTime, expression, sustainExpr);
            SetMidiChordSlider(ChordSliderType.Expression, channel, diminuendoTime, noteDurationMilliseconds, sustainExpr, channelExpressionState);
        }
    }
    /******************************************************************************/
    /******************************************************************************/
    /// <summary>
    /// An MidiExpressionSlider corresponds to an ExpressionControl in the score,
    /// which is a capital 'E' character followed by an argument list. 
    /// The values are spread evenly in time across the chord, with the first
    /// value at the very beginning and the final value at the very end.
    /// Intermediate sliding values are interpolated.
    /// </summary>
    public class MidiExpressionSlider : MidiChordSlider
    {
        public MidiExpressionSlider(List<byte> values, int channel, int noteDurationMilliseconds)
            : base(ChordSliderType.Expression, values, channel, noteDurationMilliseconds)
        {
        }
    }
    /******************************************************************************/
    public class MidiPitchWheelSlider : MidiChordSlider
    {
        public MidiPitchWheelSlider(List<byte> values, int channel, int noteDurationMilliseconds)
            : base(ChordSliderType.Pitchwheel, values, channel, noteDurationMilliseconds)
        {
        }
    }
    /******************************************************************************/
    public class MidiPanSlider : MidiChordSlider
    {
        public MidiPanSlider(List<byte> values, int channel, int noteDurationMilliseconds)
            : base(ChordSliderType.Pan, values, channel, noteDurationMilliseconds)
        {
        }
    }
    /******************************************************************************/
    public class MidiModulationWheelSlider : MidiChordSlider
    {
        public MidiModulationWheelSlider(List<byte> values, int channel, int noteDurationMilliseconds)
            : base(ChordSliderType.ModulationWheel, values, channel, noteDurationMilliseconds)
        {
        }
    }
    /******************************************************************************/
    /******************************************************************************/
    public class MidiSliderTime
    {
        public MidiSliderTime(MidiSlider midiSlider, int msDuration)
        {
            MidiSlider = midiSlider;
            MsDuration = msDuration;
        }
        public MidiSlider MidiSlider;
        public int MsDuration; // the duration to sleep after sending the Expression's messages
    }

    public enum ChordSliderType
    {
        unknownType,
        Pitchwheel,
        Pan,
        ModulationWheel,
        Expression
    }
    /*****************************************************************************/

}
