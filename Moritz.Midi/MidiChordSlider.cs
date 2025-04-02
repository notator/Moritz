using Moritz.Globals;

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace Moritz.Midi
{
    /// <summary>
    /// MidiControls which change the MIDI Expression value in their own time.
    /// </summary>
    public abstract class MidiChordSlider : SanfordMessage
    {
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

        private void SetSingleMidiChordSlider(ChordSliderType chordSliderType, int channel, byte value)
        {
            MidiSliderTime midiSliderTime = null;
            switch(chordSliderType)
            {
                case ChordSliderType.Pitchwheel:
                    midiSliderTime =
                        new MidiSliderTime(new SSPitchWheelMsg(channel, value), 0);
                    break;
                case ChordSliderType.Pan:
                    midiSliderTime =
                        new MidiSliderTime(new SSPanMsg(channel, value), 0);
                    break;
                case ChordSliderType.ModulationWheel:
                    midiSliderTime =
                        new MidiSliderTime(new SSModulationWheelMsg(channel, value), 0);
                    break;
                case ChordSliderType.Expression:
                    midiSliderTime =
                        new MidiSliderTime(new SSExpressionMsg(channel, value), 0);
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
                            new MidiSliderTime(new SSPitchWheelMsg(channel, (byte)floatCurrentValue), sleepTime);
                        break;
                    case ChordSliderType.Pan:
                        midiSliderTime =
                            new MidiSliderTime(new SSPanMsg(channel, (byte)floatCurrentValue), sleepTime);
                        break;
                    case ChordSliderType.ModulationWheel:
                        midiSliderTime =
                            new MidiSliderTime(new SSModulationWheelMsg(channel, (byte)floatCurrentValue), sleepTime);
                        break;
                    case ChordSliderType.Expression:
                        midiSliderTime =
                            new MidiSliderTime(new SSExpressionMsg(channel, (byte)floatCurrentValue), sleepTime);
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

        public List<MidiSliderTime> MidiSliderTimes = new List<MidiSliderTime>();
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
        public MidiSliderTime(SanfordSlider midiSlider, int msDuration)
        {
            MidiSlider = midiSlider;
            MsDuration = msDuration;
        }
        public SanfordSlider MidiSlider;
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
