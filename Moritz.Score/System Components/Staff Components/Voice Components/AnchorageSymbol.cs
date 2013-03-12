using System.Collections.Generic;

namespace Moritz.Score
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
        public ColorString ColorString = new ColorString("000000");

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

        public void AddDynamic(float velocityPercent, float currentVelocityPercent)
        {
            if(velocityPercent != currentVelocityPercent)
            {
                string dynamicString = "";
                #region get dynamicString and _dynamic
                float midiVelocity = velocityPercent * 1.27F;
                // note that cLicht has pppp and ffff, but these dynamics are not used here (in Study2)
                // These are the dynamicStrings for cLicht
                if(midiVelocity > 120F)
                {
                    dynamicString = "Ï";
                }
                else if(midiVelocity > 110.01F)
                {
                    dynamicString = "ƒ";
                }
                else if(midiVelocity > 85.01F)
                {
                    dynamicString = "f";
                }
                else if(midiVelocity > 60.01F)
                {
                    dynamicString = "F";
                }
                else if(midiVelocity > 40.01F)
                {
                    dynamicString = "P";
                }
                else if(midiVelocity > 32.01F)
                {
                    dynamicString = "p";
                }
                else if(midiVelocity > 20.01F)
                {
                    dynamicString = "π";
                }
                else
                {
                    dynamicString = "∏";
                }
                #endregion get dynamicString and _dynamic

                TextInfo dynamicTextInfo = new TextInfo(dynamicString, "CLicht", this.FontHeight * 0.75F, TextHorizAlign.left);

                Text dynamicText = new Text(this, dynamicTextInfo);
                this._drawObjects.Add(dynamicText);
            }
        }

        /// <summary>
        /// Adds a text to this anchorageSymbol's DrawObjects list.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="fontFamily">e.g. "Times New Roman", "Arial" etc.</param>
        /// <param name="fontHeight">Text height (capella's default text has height 12).</param>
        /// <param name="textHorizAlign">The horizontal alignment.</param>
        public void AddText(string text, string fontFamily, float fontHeight, TextHorizAlign textHorizAlign)
        {
            TextInfo textInfo = new TextInfo(text, fontFamily, fontHeight, textHorizAlign);
            Text textObject = new Text(this, textInfo);
            DrawObjects.Add(textObject);
        }
 
        /// <summary>
        /// If textToRemove is the Content of a Text DrawObject attached to this AnchorageSymbol
        /// or is the Content of a Text DrawObject contained in a DrawObjectGroup attached to this
        /// AnchorageSymbol, the Text is removed. Empty DrawObjectGroups are also removed.
        /// Otherwise, this function returns silently without doing anything.
        /// </summary>
        /// <param name="textToRemove"></param>
        public void RemoveText(string textToRemove)
        {
            List<Text> textsToRemove = new List<Text>();
            List<DrawObjectGroup> groupsToRemove = new List<DrawObjectGroup>();
            foreach(DrawObject drawObject in DrawObjects)
            {
                Text text = drawObject as Text;
                if(text != null && text.TextInfo.Text == textToRemove)
                    textsToRemove.Add(text);
                DrawObjectGroup group = drawObject as DrawObjectGroup;
                if(group != null)
                {
                    List<Text> innerTextsToRemove = new List<Text>();
                    foreach(DrawObject dObject in group.DrawObjects)
                    {
                        Text innerText = dObject as Text;
                        if(innerText.TextInfo.Text == textToRemove)
                        {
                            innerTextsToRemove.Add(innerText);
                        }
                    }
                    foreach(Text innerText in innerTextsToRemove)
                    {
                        group.DrawObjects.Remove(innerText);
                    }
                    if(group.DrawObjects.Count == 0)
                        groupsToRemove.Add(group);
                }
            }
            foreach(DrawObjectGroup group in groupsToRemove)
                DrawObjects.Remove(group);
            foreach(Text txt in textsToRemove)
                DrawObjects.Remove(txt);
        }
    }
}
