using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System;
using System.Drawing;

using Moritz.Xml;
using Moritz.Globals;

namespace Moritz.Symbols
{
    /// <summary>
    /// Base class of all DrawObject classes.
    /// </summary>
    public abstract class DrawObject
    {
        public DrawObject()
        { }

        public DrawObject(object container)
        {
            Container = container;
        }

        public abstract void WriteSVG(SvgWriter w);

        public readonly object Container;

        /// <summary>
        /// Set when this drawObject is inside a Transposable drawObject
        /// </summary>
        public string EnharmonicNote
        {
            get { return _enharmonicNote; }
            set
            {
                // possible Values:
                // "C", "C#", "Db", "D", "D#", "Eb", "E", "E#", "Fb", "F", "F#" "Gb",
                // "G", "G#", "Ab", "A", "A#", "Bb", "B", "B#", "Cb"
                Debug.Assert(Regex.Matches(value, @"^[A-G][#b]?$") != null);
                _enharmonicNote = value;
            }
        }
        /// <summary>
        /// Set when this object is part of the gallery
        /// </summary>
        public string Name = "";
        /// <summary>
        /// Used by AssistantComposer
        /// </summary>
        public Metrics Metrics = null;

        private string _enharmonicNote = "";
    }

    #region primary DrawObject classes (each defines an XML element which is put inside a drawObj element)

    internal class Text : DrawObject
    {
        public Text(object container, TextInfo textInfo)
            : base(container)
        {
            _textInfo = textInfo;
        }

        public Text(object container, TextInfo textInfo, FrameInfo frameInfo)
            : this(container, textInfo)
        {
            _frameInfo = frameInfo;
        }

        public override void WriteSVG(SvgWriter w)
        {
            //w.SvgText(TextInfo, Metrics as TextMetrics); // does not work with DynamicMetrics
            if(Metrics != null)
                Metrics.WriteSVG(w);
            if(_frameInfo != null)
            {
                switch(_frameInfo.FrameType)
                {
                    case TextFrameType.none:
                    break;
                    case TextFrameType.rectangle:
                    w.SvgRect(null, Metrics.Left, Metrics.Top, Metrics.Right - Metrics.Left, Metrics.Bottom - Metrics.Top,
						_frameInfo.ColorString.String, _frameInfo.StrokeWidth, "none");
                    break;
                    case TextFrameType.ellipse:
					w.SvgEllipse(null, Metrics.Left, Metrics.Top, (Metrics.Right - Metrics.Left) / 2, (Metrics.Bottom - Metrics.Top) / 2,
						_frameInfo.ColorString.String, _frameInfo.StrokeWidth, "none");
					break;
                    case TextFrameType.circle:
                    w.SvgCircle(null, Metrics.Right - Metrics.Left, Metrics.Bottom - Metrics.Top,((Metrics.Right - Metrics.Left)/2),
						_frameInfo.ColorString.String, _frameInfo.StrokeWidth, "none");
                    break;
                }
            }
        }

        public override string ToString()
        {
            return "Text : " + TextInfo.Text;
        }

        // attributes
        public TextInfo TextInfo { get { return _textInfo; } }
        private TextInfo _textInfo = null;
        public FrameInfo FrameInfo { get { return _frameInfo; } }
        private FrameInfo _frameInfo = null;
    }
    public class Lyric : DrawObject
    {
        public Lyric(object container, TextInfo textInfo)
            : base(container)
        {
            _textInfo = textInfo;
        }

        public override void WriteSVG(SvgWriter w)
        {
            if(Metrics != null)
                Metrics.WriteSVG(w);
        }

        public override string ToString()
        {
            return "Lyric : " + TextInfo.Text;
        }

        // attributes
        public TextInfo TextInfo { get { return _textInfo; } }
        private TextInfo _textInfo = null;
    }
    #endregion primary DrawObject classes
    #region types containing a List<DrawObject>
    /// <summary>
    /// DrawObjects attached to a chord, a rest or the page.
    /// All measures given in draw objects are relative to the object attached to.
    /// For some horizontally spread draw objects the end is relative to another chord/rest
    /// </summary>
    internal class DrawObjectGroup : DrawObject
    {
        public DrawObjectGroup(List<DrawObject> drawObjects, object container)
            : base(container)
        {
            DrawObjects = drawObjects;
        }

        protected DrawObjectGroup(AnchorageSymbol anchorageSymbol, List<string> strings, float fontHeight, ColorString colorString, TextHorizAlign textHorizAlign)
        {
            if(strings.Count > 0)
            {
                for(int i = 0; i < strings.Count; i++)
                {
                    TextInfo textInfo = new TextInfo(strings[i], "Arial", fontHeight, colorString, textHorizAlign);
                    Text text = new Text(this, textInfo);
                    DrawObjects.Add(text);
                }
            }
        }

        public override void WriteSVG(SvgWriter w)
        {
            w.SvgStartGroup(Metrics.ObjectType, null);
            foreach(DrawObject drawObject in DrawObjects)
                drawObject.WriteSVG(w);
            w.SvgEndGroup();
        }

        #region Moritz helper property
        /// <summary>
        /// All the simple Moritz.Score.Text objects inside this DrawObjectGroup
        /// (even in nested DrawObjectGroups)
        /// </summary>
        public List<Text> NestedTexts
        {
            get
            {
                List<Text> returnValue = new List<Text>();
                foreach(DrawObject drawObject in DrawObjects)
                {
                    Text text = drawObject as Text;
                    if(text != null)
                    {
                        returnValue.Add(text);
                    }
                    else
                    {
                        DrawObjectGroup dog = drawObject as DrawObjectGroup;
                        if(dog != null)
                            returnValue.AddRange(dog.NestedTexts); // recursive call
                    }
                }
                return returnValue;
            }
        }
        /// <summary>
        /// All the simple Moritz.Score.DrawObjectGroup objects inside this DrawObjectGroup
        /// (even in nested DrawObjectGroups)
        /// </summary>
        public List<DrawObjectGroup> NestedGroups
        {
            get
            {
                List<DrawObjectGroup> returnValue = new List<DrawObjectGroup>();
                foreach(DrawObject drawObject in DrawObjects)
                {
                    DrawObjectGroup group = drawObject as DrawObjectGroup;
                    if(group != null)
                    {
                        returnValue.Add(group);
                        foreach(DrawObjectGroup nestedGroup in group.NestedGroups)
                            returnValue.Add(nestedGroup); // recursive call
                    }
                }
                return returnValue;
            }
        }
        #endregion

        public List<DrawObject> DrawObjects = new List<DrawObject>();
    }

    #region classes used by AssistantComposer to differentiate between DrawObjectGroups
    internal class StaffControlsGroup : DrawObjectGroup
    {
        public StaffControlsGroup(Barline firstBarline, 
            List<string> controlStrings, string volume,
            float fontHeight, ColorString colorString, TextHorizAlign textHorizAlign)
            : base(firstBarline, controlStrings, fontHeight, colorString, textHorizAlign)
        {
            if(!String.IsNullOrEmpty(volume))
            {
                TextInfo textInfo = new TextInfo("v" + volume, "Arial", fontHeight * 2,
                    new ColorString(Color.Black), textHorizAlign);
                Text text = new Text(this, textInfo);
                DrawObjects.Insert(0,text);
            }
        }
    }
    internal class OrnamentControlsGroup : DrawObjectGroup
    {
        public OrnamentControlsGroup(AnchorageSymbol anchorageSymbol, List<string> strings, float fontHeight, ColorString colorString, TextHorizAlign textHorizAlign)
            : base(anchorageSymbol, strings, fontHeight, colorString, textHorizAlign)
        {
        }
    }
    internal class DurationSymbolControlsGroup : DrawObjectGroup
    {
        public DurationSymbolControlsGroup(AnchorageSymbol anchorageSymbol, List<string> strings, float fontHeight, ColorString colorString, TextHorizAlign textHorizAlign)
            : base(anchorageSymbol, strings, fontHeight, colorString, textHorizAlign)
        {
        }
    }

    #endregion

    #endregion types containing a List<DrawObject>
}
