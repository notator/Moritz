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
            if(midiVelocity > M.MaxMidiVelocity["ff"])
            {
                dynamicString = "Ï";    // fff
            }
            else if(midiVelocity > M.MaxMidiVelocity["f"])
            {
                dynamicString = "ƒ";    // ff
            }
            else if(midiVelocity > M.MaxMidiVelocity["mf"])
            {
                dynamicString = "f";    // f
            }
            else if(midiVelocity > M.MaxMidiVelocity["mp"])
            {
                dynamicString = "F";    // mf
            }
            else if(midiVelocity > M.MaxMidiVelocity["p"])
            {
                dynamicString = "P";    // mp
            }
            else if(midiVelocity > M.MaxMidiVelocity["pp"])
            {
                dynamicString = "p";    // p
            }
            else if(midiVelocity > M.MaxMidiVelocity["ppp"])
            {
                dynamicString = "π";    // pp
            }
            else if(midiVelocity > M.MaxMidiVelocity["pppp"])
            {
                dynamicString = "∏";    // ppp
            }
            else // > 0 
            {
                dynamicString = "Ø";    // pppp
            }
            #endregion get dynamicString and _dynamic
            return dynamicString;
        }
    }
}
