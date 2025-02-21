using Moritz.Globals;

using System;
using System.Text;
using System.Xml;

namespace Moritz.Xml
{
    public class SvgWriter : XmlWriterWrapper
    {
        public SvgWriter(string file, XmlWriterSettings settings)
            : base(XmlWriter.Create(file, settings))
        {
        }

        #region WriteSVG primitives
        /// <summary>
        /// Starts an SVG "g" element. End the group with WriteEndGroup().
        /// If the argument is not nullOrEmpty, is it written as the value of a class attribute.
        /// </summary>
        /// <param name="className">Can be null or empty or a class attribute</param
        public void SvgStartGroup(string className)
        {
            _w.WriteStartElement("g");

            if(!String.IsNullOrEmpty(className))
            {
                _w.WriteAttributeString("class", className);
            }
        }

        public void SvgEndGroup()
        {
            _w.WriteEndElement();

        }

        /// <summary>
        /// Starts an SVG "defs" element. End the group with WriteEndDefs().
        /// </summary>
        /// <param name="type">Can be null or empty or a type string</param>
        public void SvgStartDefs(string type)
        {
            _w.WriteStartElement("defs");
            if(!String.IsNullOrEmpty(type))
                _w.WriteAttributeString("class", type);
        }
        public void SvgEndDefs()
        {
            _w.WriteEndElement();
        }

        /// <summary>
        /// Writes an SVG "line" element
        /// </summary>
        /// <param name="styleName">the line's CSS style name</param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        public void SvgLine(CSSObjectClass cssClass, float x1, float y1, float x2, float y2)
        {
            _w.WriteStartElement("line");
            _w.WriteAttributeString("class", cssClass.ToString());
            _w.WriteAttributeString("x1", M.FloatToShortString(x1));
            _w.WriteAttributeString("y1", M.FloatToShortString(y1));
            _w.WriteAttributeString("x2", M.FloatToShortString(x2));
            _w.WriteAttributeString("y2", M.FloatToShortString(y2));
            _w.WriteEndElement(); //line
        }

        /// <summary>
        /// Writes an SVG "rect" element having a class that has a CSS definiton elsewhere.
        /// </summary>
        /// <param name="type">must be a valid string</param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void SvgRect(CSSObjectClass cssClass, float left, float top, float width, float height)
        {
            _w.WriteStartElement("rect");
            _w.WriteAttributeString("class", cssClass.ToString());
            _w.WriteAttributeString("x", M.FloatToShortString(left));
            _w.WriteAttributeString("y", M.FloatToShortString(top));
            _w.WriteAttributeString("width", M.FloatToShortString(width));
            _w.WriteAttributeString("height", M.FloatToShortString(height));
            _w.WriteEndElement(); // rect
        }

        /// <summary>
        /// Writes an SVG "circle" element having a class that has a CSS definiton elsewhere.
        /// </summary>
        /// <param name="type">Not written if null or empty</param>
        /// <param name="cx"></param>
        /// <param name="cy"></param>
        /// <param name="r"></param>
        public void SvgCircle(CSSObjectClass cssClass, float cx, float cy, float r)
        {
            WriteStartElement("circle");
            _w.WriteAttributeString("class", cssClass.ToString());
            WriteAttributeString("cx", M.FloatToShortString(cx));
            WriteAttributeString("cy", M.FloatToShortString(cy));
            WriteAttributeString("r", M.FloatToShortString(r));

            WriteEndElement(); // circle
        }

        /// <summary>
        /// Writes an SVG "ellipse" element having a class that has a CSS definiton elsewhere.
        /// </summary>
        /// <param name="type">Not written if null or empty</param>
        /// <param name="cx"></param>
        /// <param name="cy"></param>
        /// <param name="rx"></param>
        /// <param name="ry"></param>
        public void SvgEllipse(CSSObjectClass cssClass, float cx, float cy, float rx, float ry)
        {
            WriteStartElement("ellipse");
            WriteAttributeString("class", cssClass.ToString());
            WriteAttributeString("cx", M.FloatToShortString(cx));
            WriteAttributeString("cy", M.FloatToShortString(cy));
            WriteAttributeString("rx", M.FloatToShortString(rx));
            WriteAttributeString("ry", M.FloatToShortString(ry));

            WriteEndElement(); // ellipse
        }

        /// <summary>
        /// A square bracket
        /// </summary>
        public void SvgCautionaryBracket(CSSObjectClass cssClass, bool isLeftBracket, float top, float right, float bottom, float left)
        {
            if(!isLeftBracket)
            {
                float temp = left;
                left = right;
                right = temp;
            }
            string leftStr = left.ToString("0.###", M.En_USNumberFormat);
            string topStr = top.ToString("0.###", M.En_USNumberFormat);
            string rightStr = right.ToString("0.###", M.En_USNumberFormat);
            string bottomStr = bottom.ToString("0.###", M.En_USNumberFormat);

            _w.WriteStartElement("path");
            _w.WriteAttributeString("class", cssClass.ToString());
            StringBuilder d = new StringBuilder();
            d.Append("M " + rightStr + "," + topStr + " ");
            d.Append("L " + leftStr + "," + topStr + " " +
                leftStr + "," + bottomStr + " " +
                rightStr + "," + bottomStr);
            _w.WriteAttributeString("d", d.ToString());
            _w.WriteEndElement(); // path
        }

        /// <summary>
        /// Draws a vertical parallelogram of class "beam" (with black fill and stroke) or "opaqueBeam".
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="topLeftY"></param>
        /// <param name="topRightY"></param>
        /// <param name="beamWidth">The vertical distance between the coordinates on the left and right sides.</param>
        /// <param name="isOpaque"></param>
        public void SvgBeam(float left, float right, float topLeftY, float topRightY, float beamWidth, bool isOpaque)
        {
            float bottomLeftY = topLeftY + beamWidth;
            float bottomRightY = topRightY + beamWidth;
            StringBuilder dSB = new StringBuilder();
            dSB.Append("M " + left.ToString("0.###", M.En_USNumberFormat) + " " + topLeftY.ToString("0.###", M.En_USNumberFormat) + " ");
            dSB.Append("L " + right.ToString("0.###", M.En_USNumberFormat) + " " + topRightY.ToString("0.###", M.En_USNumberFormat) + " ");
            dSB.Append(right.ToString("0.###", M.En_USNumberFormat) + " " + bottomRightY.ToString("0.###", M.En_USNumberFormat) + " ");
            dSB.Append(left.ToString("0.###", M.En_USNumberFormat) + " " + bottomLeftY.ToString("0.###", M.En_USNumberFormat) + " z");

            _w.WriteStartElement("path");

            CSSObjectClass beamClass;
            if(isOpaque)
            {
                beamClass = CSSObjectClass.opaqueBeam;
            }
            else
            {
                beamClass = CSSObjectClass.beam;
            }
            _w.WriteAttributeString("class", beamClass.ToString());
            _w.WriteAttributeString("d", dSB.ToString());
            _w.WriteEndElement(); // path
        }

        public void SvgText(CSSObjectClass cssClass, string text, float x, float y)
        {
            _w.WriteStartElement("text");
            _w.WriteAttributeString("class", cssClass.ToString());
            _w.WriteAttributeString("x", M.FloatToShortString(x));
            _w.WriteAttributeString("y", M.FloatToShortString(y));
            _w.WriteString(text);
            _w.WriteEndElement(); // text
        }

        // Currently used only by HeadMetrics (to write coloured noteheads).
        public void SvgText(CSSObjectClass cssObjectClass, CSSColorClass cssColorClass, string text, float x, float y)
        {
            string classesString = cssObjectClass.ToString();
            if(cssColorClass != CSSColorClass.none && cssColorClass != CSSColorClass.black)
            {
                classesString = string.Concat(classesString, " ", cssColorClass.ToString());
            };

            _w.WriteStartElement("text");
            if(!String.IsNullOrEmpty(classesString))
            {
                _w.WriteAttributeString("class", classesString);
            }
            _w.WriteAttributeString("x", M.FloatToShortString(x));
            _w.WriteAttributeString("y", M.FloatToShortString(y));
            _w.WriteString(text);
            _w.WriteEndElement(); // text
        }

        /// <summary>
        /// Writes an SVG "use" element, overriding its x- and y-coordinates.
        /// </summary>
        /// <param name="cssClass">Currently either CSSClass.clef or CSSClass.flag</param>
        /// <param name="cssClass">Currently either CSSClass.clef or CSSClass.flag</param>
        /// <param name="y">This element's y-coordinate.</param>
        /// <param name="idOfObjectToUse">(Do not include the leading '#'. It will be inserted automatically.)</param>
        public void SvgUseXY(CSSObjectClass cssClass, string idOfObjectToUse, float x, float y)
        {
            _w.WriteStartElement("use");
            _w.WriteAttributeString("class", cssClass.ToString());
            _w.WriteAttributeString("href", "#" + idOfObjectToUse);
            _w.WriteAttributeString("x", M.FloatToShortString(x));
            _w.WriteAttributeString("y", M.FloatToShortString(y));
            _w.WriteEndElement();
        }
        #endregion

        /// <summary>
        /// for example:
        /// [g id="Right5Flags"]
        ///     [path d="M 0,0    0,0.12096 0.31809,0.2467 Q 0.299,0.20 0.31809,0.1257" /]
        ///     [path d="M 0,0.25 0,0.37096 0.31809,0.4967 Q 0.299,0.45 0.31809,0.3757" /]
        ///     [path d="M 0,0.5  0,0.62096 0.31809,0.7467 Q 0.299,0.70 0.31809,0.6257" /]
        ///     [path d="M 0,0.75 0,0.87096 0.31809,0.9967 Q 0.299,0.95 0.31809,0.8757" /]
        ///     [path d="M 0,1    0,1.12096 0.31809,1.2467 Q 0.299,1.20 0.31809,1.1257" /]
        /// [/g]
        /// </summary>
        public void WriteFlagBlock(StringBuilder type, int nFlags, bool rightFlag, float fontHeight)
        {
            string id = type.ToString();
            string x1 = "0";
            string x2 = "0";
            string x3 = M.FloatToShortString(0.31809F * fontHeight);
            string x4 = M.FloatToShortString(0.299F * fontHeight);
            string x5 = M.FloatToShortString(0.31809F * fontHeight);

            float sign = rightFlag ? 1F : -1F;
            float y1 = 0F;
            float y2 = sign * 0.12096F * fontHeight;
            float y3 = sign * 0.2467F * fontHeight;
            float y4 = sign * 0.2F * fontHeight;
            float y5 = sign * 0.1257F * fontHeight;
            float flagOffset = sign * 0.25F * fontHeight;

            _w.WriteStartElement("g");
            _w.WriteAttributeString("id", id);

            for(float flagIndex = 0; flagIndex < nFlags; flagIndex++)
            {
                float offset = flagIndex * flagOffset;
                _w.WriteStartElement("path");
                StringBuilder dAttributeSB = new StringBuilder();
                dAttributeSB.Append("M ");
                dAttributeSB.Append(x1 + "," + M.FloatToShortString(y1 + offset) + " ");
                dAttributeSB.Append(x2 + "," + M.FloatToShortString(y2 + offset) + " ");
                dAttributeSB.Append(x3 + "," + M.FloatToShortString(y3 + offset) + " Q ");
                dAttributeSB.Append(x4 + "," + M.FloatToShortString(y4 + offset) + " ");
                dAttributeSB.Append(x5 + "," + M.FloatToShortString(y5 + offset));
                _w.WriteAttributeString("d", dAttributeSB.ToString());
                _w.WriteEndElement();
            }

            _w.WriteEndElement();
        }
    }
}
