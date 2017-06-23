using System;
using System.Xml;
using System.Text;
using System.Diagnostics;

using Moritz.Globals;

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
		/// <param name="type">the line's class</param>
		/// <param name="x1"></param>
		/// <param name="y1"></param>
		/// <param name="x2"></param>
		/// <param name="y2"></param>
		public void SvgLine(string type, float x1, float y1, float x2, float y2)
		{
			_w.WriteStartElement("line");
            Debug.Assert(!String.IsNullOrEmpty(type));
		    _w.WriteAttributeString("class", type);
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
        public void SvgRect(string type, float left, float top, float width, float height)
        {
            _w.WriteStartElement("rect");
            Debug.Assert(!String.IsNullOrEmpty(type));
            _w.WriteAttributeString("class", type);
            _w.WriteAttributeString("x", M.FloatToShortString(left));
            _w.WriteAttributeString("y", M.FloatToShortString(top));
            _w.WriteAttributeString("width", M.FloatToShortString(width));
            _w.WriteAttributeString("height", M.FloatToShortString(height));
            _w.WriteEndElement(); // rect
        }

        /// <summary>
        /// Writes an SVG "rect" element. This function is deprecated. Use the one above with CSS instead.
        /// </summary>
        /// <param name="type">Not written if null or empty</param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="strokeColor">Not written if null or empty</param>
        /// <param name="strokeWidth">Must be >= 0</param>
        /// <param name="fillColor">Not written if null or empty</param>
        public void SvgRect(string type, float left, float top, float width, float height,
            string strokeColor, float strokeWidth, string fillColor)
        {
            Debug.Assert(strokeWidth > 0F);
            _w.WriteStartElement("rect");
            if(!String.IsNullOrEmpty(type))
                _w.WriteAttributeString("class", type);
            _w.WriteAttributeString("x", M.FloatToShortString(left));
            _w.WriteAttributeString("y", M.FloatToShortString(top));
            _w.WriteAttributeString("width", M.FloatToShortString(width));
            _w.WriteAttributeString("height", M.FloatToShortString(height));

            StringBuilder styleSB = getStyleStringBuilder(strokeColor, strokeWidth, fillColor, null);
            if(styleSB.Length > 0)
                WriteAttributeString("style", styleSB.ToString());

            _w.WriteEndElement(); // rect
        }

        /// <summary>
        /// Writes an SVG "circle" element. Deprecated. See SvgRect usage.
        /// </summary>
        /// <param name="type">Not written if null or empty</param>
        /// <param name="cx"></param>
        /// <param name="cy"></param>
        /// <param name="r"></param>
        /// <param name="strokeColor">Not written if null or empty</param>
        /// <param name="strokeWidth">Must be >= 0</param>
        /// <param name="fillColorOrNull">Not written if null or empty</param>
        public void SvgCircle(string type, float cx, float cy, float r,
			string strokeColor, float strokeWidth, string fillColorOrNull)
		{
			WriteStartElement("circle");
			if(!String.IsNullOrEmpty(type))
				_w.WriteAttributeString("class", type);
			WriteAttributeString("cx", M.FloatToShortString(cx));
			WriteAttributeString("cy", M.FloatToShortString(cy));
			WriteAttributeString("r", M.FloatToShortString(r));

			StringBuilder styleSB = getStyleStringBuilder(strokeColor, strokeWidth, fillColorOrNull, null);
			if(styleSB.Length > 0)
				WriteAttributeString("style", styleSB.ToString());

			WriteEndElement(); // circle
		}

        /// <summary>
        /// Writes an SVG "ellipse" element. Deprecated. See SvgRect usage.
        /// </summary>
        /// <param name="type">Not written if null or empty</param>
        /// <param name="cx"></param>
        /// <param name="cy"></param>
        /// <param name="rx"></param>
        /// <param name="ry"></param>
        /// <param name="strokeColour">Not written if null or empty</param>
        /// <param name="strokeWidth">Must be >= 0</param>
        /// <param name="fillColor">Not written if null or empty</param>
        /// <remarks>Note that Inkscape may turn this into a circle if rx == ry.</remarks>
        public void SvgEllipse(string type, float cx, float cy, float rx, float ry,
			string strokeColor, float strokeWidth, string fillColor)
		{
			WriteStartElement("ellipse");
			if(!String.IsNullOrEmpty(type))
				_w.WriteAttributeString("class", type);
			WriteAttributeString("cx", M.FloatToShortString(cx));
			WriteAttributeString("cy", M.FloatToShortString(cy));
			WriteAttributeString("rx", M.FloatToShortString(rx));
			WriteAttributeString("ry", M.FloatToShortString(ry));

			StringBuilder styleSB = getStyleStringBuilder(strokeColor, strokeWidth, fillColor, null);
			if(styleSB.Length > 0)
				WriteAttributeString("style", styleSB.ToString());

			WriteEndElement(); // ellipse
		}

		/// <summary>
		/// Returns a Stringbuilder containing a style string.
        /// This function is deprecated. Use a defined class with CSS instead.
		/// </summary>
		/// <param name="strokeColorOrNull">If null, stroke is not set. '#000000' is black</param>
		/// <param name="strokeWidth">Must be >= 0</param>
		/// <param name="fillColorOrNull">If null, fill is not set.  '#000000' is black</param>
		/// <param name="linecapStringOrNull">If null, lineCap is not set. Other values are: 'butt', 'round' and 'square'</param>
		/// <returns>A style string</returns>
		private StringBuilder getStyleStringBuilder(string strokeColorOrNull, float strokeWidth, string fillColorOrNull, string linecapStringOrNull)
		{
			StringBuilder rval = new StringBuilder();
			string strokeWidthString = strokeWidth.ToString("0.###", M.En_USNumberFormat);

			Debug.Assert(strokeWidth >= 0);
			Debug.Assert(linecapStringOrNull == null
				|| (string.Compare(linecapStringOrNull.ToString(), "butt") == 0)
				|| (string.Compare(linecapStringOrNull.ToString(), "round") == 0)
				|| (string.Compare(linecapStringOrNull.ToString(), "square") == 0));

			if(!String.IsNullOrEmpty(strokeColorOrNull))
				rval.Append("stroke:" + strokeColorOrNull + "; ");

			rval.Append("stroke-width:" + strokeWidthString + "px; ");

			if(!String.IsNullOrEmpty(fillColorOrNull))
				rval.Append("fill:" + fillColorOrNull + "; ");
			if(!String.IsNullOrEmpty(linecapStringOrNull))
				rval.Append("stroke-linecap:" + linecapStringOrNull + "; ");

			if(rval.Length > 0)
				rval.Remove(rval.Length - 2, 2);

			return rval;
		}

        /// <summary>
        /// A square bracket
        /// </summary>
        public void SvgCautionaryBracket(bool isLeftBracket, float top, float right, float bottom, float left)
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
            _w.WriteAttributeString("class", "cautionaryBracket");
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

            if(isOpaque)
            {
                _w.WriteAttributeString("class", "opaqueBeam");
                //_w.WriteAttributeString("fill", "white");
                //_w.WriteAttributeString("opacity", opacity.ToString("0.###", M.En_USNumberFormat));
            }
            else
            {
                _w.WriteAttributeString("class", "beam");
                //_w.WriteAttributeString("stroke", "black");
                //_w.WriteAttributeString("stroke-width", strokeWidth.ToString("0.###", M.En_USNumberFormat) + "px");
                //_w.WriteAttributeString("fill", "black");
            }
            _w.WriteAttributeString("d", dSB.ToString());
            _w.WriteEndElement(); // path
        }

        public void SvgText(string type, TextInfo textInfo, float x, float y)
        {
            _w.WriteStartElement("text");
			if(!String.IsNullOrEmpty(type))
			{
				_w.WriteAttributeString("class", type);
			}
            _w.WriteAttributeString("x", M.FloatToShortString(x));
            _w.WriteAttributeString("y", M.FloatToShortString(y));
            if(String.IsNullOrEmpty(type))
            {
                switch(textInfo.TextHorizAlign)
                {
                    case TextHorizAlign.left:
                        break;
                    case TextHorizAlign.center:
                        _w.WriteAttributeString("text-anchor", "middle");
                        break;
                    case TextHorizAlign.right:
                        _w.WriteAttributeString("text-anchor", "end");
                        break;
                }

                _w.WriteAttributeString("font-size", M.FloatToShortString(textInfo.FontHeight));
                _w.WriteAttributeString("font-family", textInfo.FontFamily);
                if(textInfo.ColorString != null
                    && !String.IsNullOrEmpty(textInfo.ColorString.String)
                    && !textInfo.ColorString.IsBlack)
                    _w.WriteAttributeString("fill", textInfo.ColorString.String);
            }
            _w.WriteString(textInfo.Text);
            _w.WriteEndElement(); // text
        }

        /// <summary>
        /// Writes an SVG "use" element, overriding its y-coordinate.
        /// </summary>
        /// <param name="type">Can be null or empty or an id String</param>
        /// <param name="y">This element's y-coordinate.</param>
        /// <param name="objectToUse">(Do not include the leading '#'. It will be inserted automatically.)</param>
        public void SvgUseXY(string type, string objectToUse, float x, float y, float fontSize)
        {
            _w.WriteStartElement("use");
            if(!String.IsNullOrEmpty(type))
                _w.WriteAttributeString("class", type);
            _w.WriteAttributeString("xlink", "href", null, "#" + objectToUse);
            //_w.WriteAttributeString("x", M.FloatToAttributeString(x) + "px");
            //_w.WriteAttributeString("y", M.FloatToAttributeString(y) + "px");
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
        public void WriteFlagBlock(int nFlags, bool rightFlag, float fontHeight)
        {
            StringBuilder type = new StringBuilder();
            if(rightFlag)
                type.Append("Right");
            else
                type.Append("Left");
            type.Append(nFlags);
            if(nFlags == 1)
                type.Append("Flag");
            else
                type.Append("Flags");

            string id = type.ToString();

            _w.WriteStartElement("g");

            if(!String.IsNullOrEmpty(id))
                _w.WriteAttributeString("id", id);

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
