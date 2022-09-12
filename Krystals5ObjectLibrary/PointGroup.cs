using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml;

namespace Krystals5ObjectLibrary
{
    /// <summary>
    /// A component of the fixedPoints list and planets in a gamete.
    /// This is a set of parameters describing a group of points and their expansion values.
    /// </summary>
    public sealed class PointGroup
    {
        #region constructors
        public PointGroup()
        {
        }
        public PointGroup(string valueString)
        {
            this._value = K.GetUIntList(valueString);
        }
        public PointGroup(XmlReader r)
        {
            // at the start of this constructor, r.Name == "pointGroup" (start tag)
            K.DisplayColor c = K.DisplayColor.black;
            Type displayColorType = c.GetType();
            Enum s = K.PointGroupShape.circle;
            Type pointGroupShapeType = s.GetType();

            bool colorRead, countRead, shapeRead; // compulsory attributes
            colorRead = countRead = shapeRead = false;
            while(r.MoveToNextAttribute())
            {
                switch(r.Name)
                {
                    case "color":
                        _color = (K.DisplayColor)Enum.Parse(displayColorType, r.Value);
                        colorRead = true;
                        break;
                    case "count":
                        _count = uint.Parse(r.Value);
                        countRead = true;
                        break;
                    case "shape":
                        _shape = (K.PointGroupShape)Enum.Parse(pointGroupShapeType, r.Value);
                        shapeRead = true;
                        break;
                }
            }
            CheckAttributesRead(colorRead, countRead, shapeRead); // throw exception if attribute missing
            bool valueRead, toRead, fromRead; // compulsory point group parameters (the others are optional)
            valueRead = toRead = fromRead = false;
            do
            {
                do
                {
                    r.Read();
                } while(r.Name != "value" && r.Name != "from" && r.Name != "to"
                    && r.Name != "rotate" && r.Name != "translate"
                    && r.Name != "pointGroup");
                switch(r.Name)
                {
                    case "value":
                        _value = K.GetUIntList(r.ReadString());
                        valueRead = true;
                        break;
                    case "from":
                        while(r.MoveToNextAttribute())
                        {
                            switch(r.Name)
                            {
                                case "radius": _fromRadius = K.StringToFloat(r.Value); break;
                                case "angle": _fromAngle = K.StringToFloat(r.Value); break;
                            }
                        }
                        fromRead = true;
                        break;
                    case "to":
                        while(r.MoveToNextAttribute())
                        {
                            switch(r.Name)
                            {
                                case "radius": _toRadius = K.StringToFloat(r.Value); break;
                                case "angle": _toAngle = K.StringToFloat(r.Value); break;
                            }
                        }
                        toRead = true;
                        break;
                    case "rotate":
                        r.MoveToFirstAttribute();
                        if(r.Name == "angle") _rotateAngle = K.StringToFloat(r.Value); break;
                    case "translate":
                        while(r.MoveToNextAttribute())
                        {
                            switch(r.Name)
                            {
                                case "radius": _translateRadius = K.StringToFloat(r.Value); break;
                                case "angle": _translateAngle = K.StringToFloat(r.Value); break;
                            }
                        }
                        break;
                    case "pointGroup":
                        break;
                }
            } while(r.Name == "value" || r.Name == "radius" || r.Name == "angle");
            CheckParametersRead(valueRead, toRead, fromRead); // throw exception if parameter missing
            // r.Name == "pointGroup" here (the closing tag of this pointGroup)
            K.ReadToXmlElementTag(r, "pointGroup", "planet", "fixedPoints");
            // r.Name is "pointGroup", "planet" or "fixedPoints" here
            // Start tags: "pointGroup"
            // End tags: "planet", "fixedPoints"
            _visible = true;
        }
        #endregion constructors
        #region public functions
        /// <summary>
        /// Writes the XML for this point group while saving the expansion krystal.
        /// This is the XML code inside either the "fixedPoints" or "planet" elements.
        /// If there are no points, nothing is written!
        /// </summary>
        /// <param name="w"></param>
        public void Save(XmlWriter w)
        {
            if(this.Count > 0)
            {
                w.WriteStartElement("pointGroup");
                w.WriteAttributeString("color", _color.ToString());
                w.WriteAttributeString("count", _count.ToString());
                w.WriteAttributeString("shape", _shape.ToString());
                w.WriteStartElement("value");
                w.WriteString(K.GetStringOfUnsignedInts(_value));
                w.WriteEndElement(); // value
                w.WriteStartElement("from");
                w.WriteAttributeString("angle", K.FloatToAttributeString(_fromAngle));
                w.WriteAttributeString("radius", K.FloatToAttributeString(_fromRadius));
                w.WriteEndElement(); // from
                w.WriteStartElement("to");
                w.WriteAttributeString("angle", K.FloatToAttributeString(_toAngle));
                w.WriteAttributeString("radius", K.FloatToAttributeString(_toRadius));
                w.WriteEndElement(); // to
                if(_rotateAngle != 0f)
                {
                    w.WriteStartElement("rotate");
                    w.WriteAttributeString("angle", K.FloatToAttributeString(_rotateAngle));
                    w.WriteEndElement(); // rotate
                }
                if(_translateAngle != 0f || _translateRadius != 0f)
                {
                    w.WriteStartElement("translate");
                    w.WriteAttributeString("radius", K.FloatToAttributeString(_translateRadius));
                    w.WriteAttributeString("angle", K.FloatToAttributeString(_translateAngle));
                    w.WriteEndElement(); // translate               
                }
                w.WriteEndElement(); // pointGroup
            }
        }
        /// <summary>
        /// Used, for example, when calculating the coordinates for the various sections of a planet,
        /// and when calculating the points through which a background planet line passes.
        /// Note that the WindowsPixelCoordinates field IS NOT CURRENTLY CLONED, BUT IS SET TO NULL.
        /// </summary>
        /// <returns>The cloned point group.</returns>
        public PointGroup Clone()
        {
            PointGroup p = new PointGroup
            {
                Shape = _shape,
                Count = _count,
                StartMoment = _startMoment, // not stored in XML
                Color = _color
            };
            foreach(uint v in _value)
                p.Value.Add(v);
            p.FromRadius = _fromRadius;
            p.FromAngle = _fromAngle;
            p.ToRadius = _toRadius;
            p.ToAngle = _toAngle;
            p.RotateAngle = _rotateAngle;
            p.TranslateRadius = _translateRadius;
            p.TranslateAngle = _translateAngle;
            p.Visible = _visible; // not stored in XML
            p.WindowsPixelCoordinates = null; // not stored in XML. NOTE THAT THIS FIELD IS NOT CURRENTLY CLONED!
            return p;
        }
        /// <summary>
        /// Used while calculating the positions of points in planets.
        /// </summary>
        /// <param name="densityInputKrystal"></param>
        /// <param name="fieldPanelCentreX"></param>
        /// <param name="fieldPanelCentreY"></param>
        /// <param name="scale"></param>
        /// <param name="finalGroup"></param>
        public void GetWindowsPlanetPixelCoordinates(DensityInputKrystal densityInputKrystal, float fieldPanelCentreX, float fieldPanelCentreY,
                                                     float scale, bool finalGroup)
        {
            PointGroup p = this.Clone();
            if(!finalGroup)
                p.Count++;

            #region like next function
            List<PointF> userCartesianCoordinates = new List<PointF>();
            List<PointR> userRadialCoordinates = new List<PointR>();
            float[] relPos = densityInputKrystal.RelativePlanetPointPositions; // simple alias
            float relDistance = 1;
            int firstPointIndex = (int)p.StartMoment - 1;
            if(p.Count > 0)
                relDistance = relPos[firstPointIndex + p.Count - 1] - relPos[firstPointIndex];

            switch(p.Shape)
            {
                #region circle
                case K.PointGroupShape.circle:
                    float angleDistanceFactor = 360.0f / relDistance;
                    for(int i = 0; i < p.Count; i++)
                    {
                        float a = p.FromAngle + (angleDistanceFactor * (relPos[i + firstPointIndex] - relPos[firstPointIndex]));
                        a += p.RotateAngle;
                        PointR rp = new PointR(p.FromRadius, a);
                        rp.Shift(p.TranslateRadius, p.TranslateAngle);
                        userRadialCoordinates.Add(rp);
                    }
                    break;
                #endregion circle
                #region spiral
                case K.PointGroupShape.spiral:
                    float distanceFactorR = (p.ToRadius - p.FromRadius) / relDistance;
                    float distanceFactorA = (p.ToAngle - p.FromAngle) / relDistance;
                    for(int i = 0; i < p.Count; i++)
                    {
                        float r = p.FromRadius + (distanceFactorR * (relPos[i + firstPointIndex] - relPos[firstPointIndex]));
                        float a = p.FromAngle + (distanceFactorA * (relPos[i + firstPointIndex] - relPos[firstPointIndex]));
                        a += p.RotateAngle;
                        PointR rp = new PointR(r, a);
                        rp.Shift(p.TranslateRadius, p.TranslateAngle);
                        userRadialCoordinates.Add(rp);
                    }
                    break;
                #endregion spiral
                #region straight line
                case K.PointGroupShape.straightLine:
                    PointR startPR = new PointR(p.FromRadius, p.FromAngle + p.RotateAngle);
                    startPR.Shift(p.TranslateRadius, p.TranslateAngle);
                    PointR endPR = new PointR(p.ToRadius, p.ToAngle + p.RotateAngle);
                    endPR.Shift(p.TranslateRadius, p.TranslateAngle);
                    float startX = startPR.X;
                    float startY = startPR.Y;
                    float endX = endPR.X;
                    float endY = endPR.Y;
                    float distanceFactorX = (endX - startX) / relDistance;
                    float distanceFactorY = (endY - startY) / relDistance;
                    for(int i = 0; i < p.Count; i++)
                    {
                        float x = startX + (distanceFactorX * (relPos[i + firstPointIndex] - relPos[firstPointIndex]));
                        float y = startY + (distanceFactorY * (relPos[i + firstPointIndex] - relPos[firstPointIndex]));
                        userCartesianCoordinates.Add(new PointF(x, y));
                    }
                    break;
                    #endregion
            }
            #region convert from user to windows coordnates
            List<PointF> windowsPixelCoordinates = new List<PointF>();
            if(userRadialCoordinates.Count > 0) // circular and spiral point groups
                foreach(PointR rp in userRadialCoordinates)
                {
                    PointF fp = new PointF(fieldPanelCentreX + (rp.X * scale), fieldPanelCentreY - (rp.Y * scale));
                    windowsPixelCoordinates.Add(fp);
                }
            else if(userCartesianCoordinates.Count > 0) // straight line point groups
                foreach(PointF cp in userCartesianCoordinates)
                {
                    PointF fp = new PointF(fieldPanelCentreX + (cp.X * scale), fieldPanelCentreY - (cp.Y * scale));
                    windowsPixelCoordinates.Add(fp);
                }
            // if p.Count == 0, windowsPixelCoordinates is empty.
            this.WindowsPixelCoordinates = windowsPixelCoordinates.ToArray();
            #endregion  convert from user to windows coordnates
            #endregion
        }
        /// <summary>
        /// Used while calculating the positions of fixed points.
        /// </summary>
        /// <param name="fieldPanelCentreX"></param>
        /// <param name="fieldPanelCentreY"></param>
        /// <param name="scale"></param>
        public void GetFixedPointWindowsPixelCoordinates(float fieldPanelCentreX, float fieldPanelCentreY,
                                                         float scale)
        {
            List<PointF> windowsPixelCoordinates = new List<PointF>();
            List<PointF> userCartesianCoordinates = new List<PointF>();
            List<PointR> userRadialCoordinates = new List<PointR>();
            float angleDelta = 0;
            float radiusDelta = 0;
            switch(this.Shape)
            {
                case K.PointGroupShape.circle:
                    angleDelta = 360.0f / this.Count;
                    for(int i = 0; i < this.Count; i++)
                    {
                        PointR rp = new PointR(this.FromRadius, this.FromAngle + (angleDelta * i) + this.RotateAngle);
                        rp.Shift(this.TranslateRadius, this.TranslateAngle);
                        userRadialCoordinates.Add(rp);
                    }
                    break;
                case K.PointGroupShape.spiral:
                    if(this.Count > 1)
                    {
                        angleDelta = (this.ToAngle - this.FromAngle) / (this.Count - 1);
                        radiusDelta = (this.ToRadius - this.FromRadius) / (this.Count - 1);
                    }
                    for(int i = 0; i < this.Count; i++)
                    {
                        PointR rp = new PointR(this.FromRadius + (radiusDelta * i),
                                                this.FromAngle + (angleDelta * i) + this.RotateAngle);
                        rp.Shift(this.TranslateRadius, this.TranslateAngle);
                        userRadialCoordinates.Add(rp);
                    }
                    break;
                case K.PointGroupShape.straightLine:
                    PointR startPR = new PointR(this.FromRadius, this.FromAngle + this.RotateAngle);
                    startPR.Shift(this.TranslateRadius, this.TranslateAngle);
                    PointR endPR = new PointR(this.ToRadius, this.ToAngle + this.RotateAngle);
                    endPR.Shift(this.TranslateRadius, this.TranslateAngle);
                    float startX = startPR.X;
                    float startY = startPR.Y;
                    float endX = endPR.X;
                    float endY = endPR.Y;
                    float xDelta = 0;
                    float yDelta = 0;
                    if(this.Count > 1)
                    {
                        xDelta = (endX - startX) / (this.Count - 1);
                        yDelta = (endY - startY) / (this.Count - 1);
                    }
                    for(int i = 0; i < this.Count; i++)
                    {
                        PointF rp = new PointF(startX + (xDelta * i), startY + (yDelta * i));
                        userCartesianCoordinates.Add(rp);
                    }
                    break;
            }
            if(userRadialCoordinates.Count > 0) // circular and spiral point groups
                foreach(PointR rp in userRadialCoordinates)
                {
                    PointF fp = new PointF(fieldPanelCentreX + (rp.X * scale), fieldPanelCentreY - (rp.Y * scale));
                    windowsPixelCoordinates.Add(fp);
                }
            else // straight line point groups
                foreach(PointF cp in userCartesianCoordinates)
                {
                    PointF fp = new PointF(fieldPanelCentreX + (cp.X * scale), fieldPanelCentreY - (cp.Y * scale));
                    windowsPixelCoordinates.Add(fp);
                }

            this.WindowsPixelCoordinates = windowsPixelCoordinates.ToArray();
        }
        /// <summary>
        /// Use this function when expanding outside the expander editor
        /// </summary>
        public void GetFixedPointWindowsPixelCoordinates()
        {
            GetFixedPointWindowsPixelCoordinates(0, 0, 100);
        }
        #endregion public functions
        #region Properties
        public K.PointGroupShape Shape
        {
            get { return _shape; }
            set { _shape = value; }
        }
        public uint Count
        {
            get { return _count; }
            set { _count = value; }
        }
        public uint StartMoment
        {
            get { return _startMoment; }
            set { _startMoment = value; }
        }
        public K.DisplayColor Color
        {
            get { return _color; }
            set { _color = value; }
        }
        public List<uint> Value
        {
            get { return this._value; }
            set { this._value = value; }
        }
        public float FromRadius
        {
            get { return _fromRadius; }
            set { _fromRadius = value; }
        }
        public float FromAngle
        {
            get { return _fromAngle; }
            set { _fromAngle = value; }
        }
        public float ToRadius
        {
            get { return _toRadius; }
            set { _toRadius = value; }
        }
        public float ToAngle
        {
            get { return _toAngle; }
            set { _toAngle = value; }
        }
        public float RotateAngle
        {
            get { return _rotateAngle; }
            set { _rotateAngle = value; }
        }
        public float TranslateRadius
        {
            get { return _translateRadius; }
            set { _translateRadius = value; }
        }
        public float TranslateAngle
        {
            get { return _translateAngle; }
            set { _translateAngle = value; }
        }
        public bool Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }
        public PointF[] WindowsPixelCoordinates
        {
            get { return _windowsPixelCoordinates; }
            set { _windowsPixelCoordinates = value; }
        }
        #endregion Properties
        #region private functions
        /// <summary>
        /// Throws an exception and diagnostic message if one or more of its parameters is false  
        /// </summary>
        /// <param name="colorRead"></param>
        /// <param name="countRead"></param>
        /// <param name="shapeRead"></param>
        private void CheckAttributesRead(bool colorRead, bool countRead, bool shapeRead)
        {
            StringBuilder msg = new StringBuilder();
            if(!colorRead) msg.Append("\ncolor");
            if(!countRead) msg.Append("\ncount");
            if(!shapeRead) msg.Append("\nshape");
            if(msg.Length > 0)
            {
                msg.Insert(0, "XML error:\nThe following compulsory attribute(s) were missing in a pointGroup element:\n");
                throw new ApplicationException(msg.ToString());
            }
        }
        /// <summary>
        /// Throws an exception and diagnostic message if one or more of its parameters is false 
        /// </summary>
        /// <param name="valueRead"></param>
        /// <param name="toRead"></param>
        /// <param name="fromRead"></param>
        private void CheckParametersRead(bool valueRead, bool toRead, bool fromRead)
        {
            StringBuilder msg = new StringBuilder();
            if(!valueRead) msg.Append("\n<value>");
            if(!toRead) msg.Append("\n<to>");
            if(!fromRead) msg.Append("\n<from>");
            if(msg.Length > 0)
            {
                msg.Insert(0, "XML error:\nThe following compulsory element(s) were missing inside a pointGroup element:\n");
                throw new ApplicationException(msg.ToString());
            }
        }
        #endregion private functions
        #region private variables
        private K.PointGroupShape _shape = K.PointGroupShape.spiral;
        private uint _count;
        private K.DisplayColor _color = K.DisplayColor.black;
        private List<uint> _value = new List<uint>();
        private float _fromRadius;
        private float _fromAngle;
        private float _toRadius;
        private float _toAngle;
        private float _rotateAngle;
        private float _translateRadius;
        private float _translateAngle;

        // the following values are not stored in XML
        // In files, the count of each point group can be used to find the following point group's startMoment.
        private uint _startMoment;
        // Visibility is automatically turned on when expanding.
        private bool _visible = true;
        // The _windowsPixelCoordinates are recalculated each time the expander is drawn.
        private PointF[] _windowsPixelCoordinates; // one PointF per point in this point group
        #endregion private variables
    }
}

