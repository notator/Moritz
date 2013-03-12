using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;

namespace Moritz.Score
{
    /// <summary>
    /// A list of synchronous NoteObjects.
    /// All NoteObjectMomentSymbols contain at least one DurationSymbol. This is ensured by
    ///  1. having no constructor with zero arguments, and
    ///  2. no RemoveDurationSymbol() function.
    /// </summary>
    public class NoteObjectMoment
    {
        public NoteObjectMoment(DurationSymbol durationSymbol)
        {
            _msPosition = durationSymbol.MsPosition;
            AddNoteObject(durationSymbol);
        }

        public NoteObjectMoment(NoteObject noteObject, int msPosition)
        {
            _msPosition = msPosition;
            AddNoteObject(noteObject);
        }

        /// <summary>
        /// Returns the distance between the leftmost left edge and this moment's alignment point.
        /// </summary>
        /// <returns></returns>
        public float LeftEdgeToAlignment()
        {
            float maxLeftEdgeToAlignmentX = float.MinValue;
            foreach(NoteObject noteObject in _noteObjects)
            {
                float leftEdgeToAlignmentX = AlignmentX - noteObject.Metrics.Left;
                maxLeftEdgeToAlignmentX =
                    maxLeftEdgeToAlignmentX > leftEdgeToAlignmentX ? maxLeftEdgeToAlignmentX : leftEdgeToAlignmentX; 
            }
            return maxLeftEdgeToAlignmentX;
        }

        /// <summary>
        /// returns the first Barline in this NoteObjectMoment,
        /// or null if there is no barline.
        /// </summary>
        public Barline Barline
        {
            get
            {
                Barline barline = null;
                foreach(NoteObject noteObject in NoteObjects)
                {
                    if(noteObject is Barline)
                    {
                        barline = noteObject as Barline;
                        break;
                    }
                }
                return barline;
            }
        }

        /// <summary>
        /// returns a dictionary containing staff, rightEdge pairs.
        /// </summary>
        /// <returns></returns>
        public Dictionary<Staff, float> StaffRights()
        {
            Dictionary<Staff, float> dict = new Dictionary<Staff, float>();
            foreach(NoteObject noteObject in _noteObjects)
            {
                Staff staff = noteObject.Voice.Staff;
                if(dict.ContainsKey(staff))
                {
                    if(dict[staff] < noteObject.Metrics.Right)
                        dict[staff] = noteObject.Metrics.Right;
                }
                else
                {
                    dict.Add(staff, noteObject.Metrics.Right);
                }
            }
            return dict;
        }

        public void MoveToAlignmentX(float alignmentX)
        {
            float deltaX = alignmentX - AlignmentX;
            foreach(NoteObject noteObject in _noteObjects)
            {
                noteObject.Metrics.Move(deltaX, 0);
            }
            AlignmentX = alignmentX;
        }

        /// <summary>
        /// Aligns barline glyphs in this moment, moving an immediately preceding clef, but
        /// without moving the following duration symbol (which is aligned at this.AlignmentX).
        /// </summary>
        public void AlignBarlineGlyphs()
        {
            float minBarlineOriginX = float.MaxValue;
            foreach(NoteObject noteObject in _noteObjects)
            {
                Barline b = noteObject as Barline;
                if(b != null && b.Metrics != null && b.Metrics.OriginX < minBarlineOriginX)
                    minBarlineOriginX = b.Metrics.OriginX;
            }
            for(int index = 0; index < _noteObjects.Count; index++)
            {
                Barline barline = _noteObjects[index] as Barline;
                if(barline != null && barline.Metrics != null)
                {
                    Debug.Assert(AlignmentX == 0F);
                    if(index > 0)
                    {
                        ClefSign clef = _noteObjects[index-1] as ClefSign;
                        if(clef != null)
                            clef.Metrics.Move(minBarlineOriginX - barline.Metrics.OriginX, 0);
                    }
                    barline.Metrics.Move(minBarlineOriginX - barline.Metrics.OriginX, 0);
                }
            }
        }


        private void AddNoteObject(NoteObject noteObject)
        {
            DurationSymbol durationSymbol = noteObject as DurationSymbol;
            if(durationSymbol != null && _noteObjects.Count > 0)
            {
                Debug.Assert(durationSymbol.MsPosition == _msPosition);
                Debug.Assert(durationSymbol.Voice.Staff == _noteObjects[0].Voice.Staff);
            }

            _noteObjects.Add(noteObject);

        }

        public void ShowWarning_ControlsMustBeInTopVoice(DurationSymbol durationSymbol)
        {
            ShowVoiceWarning(durationSymbol, "control symbol");
        }
        public void ShowWarning_DynamicsMustBeInTopVoice(DurationSymbol durationSymbol)
        {
            ShowVoiceWarning(durationSymbol, "dynamic");
        }
        public void ShowVoiceWarning(DurationSymbol durationSymbol, string type)
        {
            int staffIndex = 0;
            int voiceIndex = 0;
            Staff staff = durationSymbol.Voice.Staff;
            foreach(Staff testStaff in staff.SVGSystem.Staves)
            {
                if(staff == testStaff)
                    break;
                staffIndex++;
            }
            foreach(Voice voice in staff.Voices)
            {
                if(voice == durationSymbol.Voice)
                    break;
                voiceIndex++;
            }
            string msg = "Found a " + type + " at\n" +
                        "    millisecond position " + durationSymbol.MsPosition + "\n" +
                        "    staff index " + staffIndex.ToString() + "\n" +
                        "    voice index " + voiceIndex.ToString() + "\n\n" +
                        "Controls which are not attached to the top voice\n" +
                        "in a staff will be ignored.";
            MessageBox.Show(msg, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        public void Add(NoteObject noteObject)
        {
            DurationSymbol durationSymbol = noteObject as DurationSymbol;
            if(durationSymbol != null && _msPosition != durationSymbol.MsPosition)
                throw new InvalidOperationException("Attempt to add a non-synchronous DurationSymbol to a MomentSymbol.");
            _noteObjects.Add(noteObject);
        }

        public IEnumerable DurationSymbols
        {
            get
            {
                foreach(DurationSymbol durationSymbol in _noteObjects)
                    yield return durationSymbol;
            }
        }

        public IEnumerable AnchorageSymbols
        {
            get
            {
                foreach(NoteObject noteObject in _noteObjects)
                {
                    AnchorageSymbol anchorageSymbol = noteObject as AnchorageSymbol;
                    if(anchorageSymbol != null)
                        yield return anchorageSymbol;
                }
            }
        }

        public IEnumerable ChordSymbols
        {
            get
            {
                foreach(ChordSymbol chordSymbol in _noteObjects)
                    yield return chordSymbol;
            }
        }

        public float AlignmentX = 0F;

        /// <summary>
        /// The logical position in milliseconds from the beginning of the score.
        /// </summary>
        public int MsPosition { get { return _msPosition; } }
        private int _msPosition = -1;

        public List<NoteObject> NoteObjects { get { return _noteObjects; } }
        private List<NoteObject> _noteObjects = new List<NoteObject>();
    }
}
