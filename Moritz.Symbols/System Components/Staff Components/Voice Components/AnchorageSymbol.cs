using Moritz.Globals;

using System;
using System.Collections.Generic;

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

        public List<DrawObject> DrawObjects { get; set; } = new List<DrawObject>();

        /// <summary>
        /// This field is set to true (while creating a MidiScore for performance) if a specific
        /// dynamic has been attached to this anchorageSymbol..
        /// </summary>
        public bool HasExplicitDynamic = false;

        public void AddDynamic(int midiVelocity, int currentVelocity)
        {
            string newDynamicString = GetDynamicString(midiVelocity);
            string currentDynamicString = GetDynamicString(currentVelocity);

            if(String.Compare(newDynamicString, currentDynamicString) != 0)
            {
                DynamicText dynamicText = new DynamicText(this, newDynamicString, FontHeight);
                this.DrawObjects.Add(dynamicText);
            }
        }

        private string GetDynamicString(int midiVelocity)
        {
            string dynamicString = "";
            #region get dynamicString and _dynamic
            // note that cLicht has pppp and ffff, but these dynamics are not used here (in Study2)
            // These are the dynamicStrings for cLicht
            if(midiVelocity > M.MaxMidiVelocity[M.Dynamic.ff])
            {
                dynamicString = M.CLichtDynamicsCharacters[M.Dynamic.fff]; // "Ï"
            }
            else if(midiVelocity > M.MaxMidiVelocity[M.Dynamic.f])
            {
                dynamicString = M.CLichtDynamicsCharacters[M.Dynamic.ff]; // "ƒ"
            }
            else if(midiVelocity > M.MaxMidiVelocity[M.Dynamic.mf])
            {
                dynamicString = M.CLichtDynamicsCharacters[M.Dynamic.f]; // "f"
            }
            else if(midiVelocity > M.MaxMidiVelocity[M.Dynamic.mp])
            {
                dynamicString = M.CLichtDynamicsCharacters[M.Dynamic.mf]; // "F"
            }
            else if(midiVelocity > M.MaxMidiVelocity[M.Dynamic.p])
            {
                dynamicString = M.CLichtDynamicsCharacters[M.Dynamic.mp]; // "P"
            }
            else if(midiVelocity > M.MaxMidiVelocity[M.Dynamic.pp])
            {
                dynamicString = M.CLichtDynamicsCharacters[M.Dynamic.p]; // "p"
            }
            else if(midiVelocity > M.MaxMidiVelocity[M.Dynamic.ppp])
            {
                dynamicString = M.CLichtDynamicsCharacters[M.Dynamic.pp]; // "π"
            }
            else if(midiVelocity > M.MaxMidiVelocity[M.Dynamic.pppp])
            {
                dynamicString = M.CLichtDynamicsCharacters[M.Dynamic.ppp]; // "∏"
            }
            else // > 0 
            {
                dynamicString = M.CLichtDynamicsCharacters[M.Dynamic.pppp]; // "Ø"
            }
            #endregion get dynamicString and _dynamic
            return dynamicString;
        }
    }
}
