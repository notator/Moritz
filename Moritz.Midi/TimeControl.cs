using System;
using System.Diagnostics;

namespace Moritz.Midi
{
    /// <summary>
    /// This class and the MsPosTimeControlsDict SortedDictionary use int values as keys for 
    /// both "logical position" and "logical width".
    /// In CapXML scores these values are calculated to the nearest millisecond from the (tuplet)
    /// durations notated in the score.
    /// In SVG scores, "logical position" and "logical width" are calculated directly from the
    /// millisecond durations notated in the score.
    ///   "logical position" is the time (in milliseconds) from the beginning of a performance.
    ///   "logical width" is a *duration* (in milliseconds) 
    /// </summary>
    public class TimeControl
    {
        public TimeControl()
        {
        }

        public override string ToString()
        {
            return String.Format("msDuration={0}, notatedDuration={1}, isGeneralPause={4}",
               _msDuration.ToString(), _notatedMsDuration.ToString("G5"), IsGeneralPause.ToString());
        }

        /// <summary>
        /// The logical distance to the following timeControl (in milliseconds).
        /// </summary>
        public int MsWidth
        {
            get
            {
                return _msDuration;
            }
            set
            {
                if(_msDuration == -1)
                {
                    _msDuration = value;
                }
                else
                {
                    Debug.Assert(false, "Cannot set TimeControl.MsWidth twice!");
                }
            }
        }

        /// <summary>
        /// The notated duration in milliseconds.
        /// This value is set using information in the score: the logicalWidth and the score's tempo indications.
        /// </summary>
        /// <param name="logicalWidth"></param>
        /// <returns></returns>
        public int NotatedMsDuration
        {
            get
            {
                return _notatedMsDuration;
            }
            set
            {
                if(_notatedMsDuration == -1L)
                    _notatedMsDuration = value;
                else
                {
                    Debug.Assert(false, "Cannot set TimeControl.NotatedDuration twice!");
                }

            }
        }
        /// <summary>
        /// The number of logical milliseconds from the beginning of the piece.
        /// This value is calculated from the MsDurations of the previous TimeControls in the score.
        /// It is used by the Assistant.
        /// </summary>
        public int NotatedStartTime = -1;
        /// <summary>
        /// This property is true, if there are no sounding chords during this timeControl's entire duration.
        /// </summary>
        public bool IsGeneralPause = false;

        private int _msDuration = -1;
        private int _notatedMsDuration = -1;
    }
}
