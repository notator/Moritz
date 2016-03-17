using System;
using System.Collections.Generic;

using Moritz.Globals;

namespace Moritz.Symbols
{
    /// <summary>
    /// AnchorageSymbols can have a list of attached DrawObjects.
    /// </summary>
    public abstract class AnchorageSymbol : NoteObject
    {
        public AnchorageSymbol(Voice voice)
            : base(voice)
        {
        }

        public AnchorageSymbol(Voice voice, float fontHeight)
            : base(voice, fontHeight)
        {
        }

        /// <summary>
        /// Returns the (positive) horizontal distance by which this anchorage symbol overlaps
        /// (any characters in) the previous noteObjectMoment (which contains symbols from both voices
        /// in a 2-voice staff). The result can be 0. If there is no overlap, the result is float.Minval.
        /// </summary>
        /// <param name="previousAS"></param>
        public virtual float OverlapWidth(NoteObjectMoment previousNOM)
        {
            float overlap = float.MinValue;
            float localOverlap = float.MinValue;
            foreach(AnchorageSymbol previousAS in previousNOM.AnchorageSymbols)
            {
                localOverlap = this.Metrics.OverlapWidth(previousAS);
                overlap = overlap > localOverlap ? overlap : localOverlap;
            }
            return overlap;
        }

        public List<DrawObject> DrawObjects { get { return _drawObjects; } set { _drawObjects = value; } }

        private List<DrawObject> _drawObjects = new List<DrawObject>();
      
        /// <summary>
        /// This field is set to true (while creating a MidiScore for performance) if a specific
        /// dynamic has been attached to this anchorageSymbol..
        /// </summary>
        public bool HasExplicitDynamic = false;
        /// <summary>
        /// Both rests, chords and the final barline have Velocity and ControlSymbols, so that hairpins etc. can be attached to them!
        /// </summary>
        public byte Velocity = 0;

        public void AddDynamic(byte midiVelocity, byte currentVelocity)
        {
            string newDynamicString = GetDynamicString(midiVelocity);
            string currentDynamicString = GetDynamicString(currentVelocity);

            if(String.Compare(newDynamicString, currentDynamicString) != 0)
            {
                DynamicText dynamicText = new DynamicText(this, newDynamicString, FontHeight);
                this._drawObjects.Add(dynamicText);
            }
        }

        private string GetDynamicString(byte midiVelocity)
        {
            string dynamicString = "";
            #region get dynamicString and _dynamic
            // note that cLicht has pppp and ffff, but these dynamics are not used here (in Study2)
            // These are the dynamicStrings for cLicht
            if(midiVelocity > M.MaxMidiVelocity["ff"]) // sqrt(0.875) * 127  (was 112 until March 2016)
            {
                dynamicString = "Ï";    // fff
            }
            else if(midiVelocity > M.MaxMidiVelocity["f"]) // sqrt(0.75) * 127 (was 96)
            {
                dynamicString = "ƒ";    // ff
            }
            else if(midiVelocity > M.MaxMidiVelocity["mf"]) // sqrt(0.625) * 127 (was 80)
            {
                dynamicString = "f";    // f
            }
            else if(midiVelocity > M.MaxMidiVelocity["mp"])  // sqrt(0.5) * 127 (was 64)
            {
                dynamicString = "F";    // mf
            }
            else if(midiVelocity > M.MaxMidiVelocity["p"])  // sqrt(0.375) * 127 (was 48)
            {
                dynamicString = "P";    // mp
            }
            else if(midiVelocity > M.MaxMidiVelocity["pp"])  // sqrt(0.25) * 127 (was 32)
            {
                dynamicString = "p";    // p
            }
            else if(midiVelocity > M.MaxMidiVelocity["ppp"])  // sqrt(0.125) * 127 (was 16)
            {
                dynamicString = "π";    // pp
            }
            else // > 0
            {
                dynamicString = "∏";    // ppp
            }
            #endregion get dynamicString and _dynamic
            return dynamicString;
        }
    }
}
