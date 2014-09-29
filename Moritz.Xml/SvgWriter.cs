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
        /// </summary>
        /// <param name="type">Can be null or empty or a class attribute</param>
        /// <param name="id">Can be null or empty or an id attribute</param>
        public void SvgStartGroup(string type, string id)
        {
            _w.WriteStartElement("g");

            if(!String.IsNullOrEmpty(type))
                _w.WriteAttributeString("class", type);

            if(!String.IsNullOrEmpty(id))
                _w.WriteAttributeString("id", id);
        }

        public void SvgEndGroup()
        {
            _w.WriteEndElement();

        }

        /// <summary>
        /// Starts an SVG "defs" element. End the group with WriteEndDefs().
        /// </summary>
        /// <param name="id">Can be null or empty or an id-string</param>
        public void SvgStartDefs(string id)
        {
            _w.WriteStartElement("defs");
            if(!String.IsNullOrEmpty(id))
                _w.WriteAttributeString("id", id);
        }
        public void SvgEndDefs()
        {
            _w.WriteEndElement();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Not written if null or empty</param>
        /// <param name="cx"></param>
        /// <param name="cy"></param>
        /// <param name="rx"></param>
        /// <param name="ry"></param>
        /// <param name="strokeColour"></param>
        /// <param name="strokeWidth">Not written if null or empty</param>
        public void SvgEllipse(string id, float cx, float cy, float rx, float ry, 
            string strokeColor, float strokeWidth, string fillColor,
            string transformString)
        {
            WriteStartElement("ellipse");
            if(!String.IsNullOrEmpty(id))
                _w.WriteAttributeString("id", id);
            WriteAttributeString("cx", cx.ToString(M.En_USNumberFormat));
            WriteAttributeString("cy", cy.ToString(M.En_USNumberFormat));
            WriteAttributeString("rx", rx.ToString(M.En_USNumberFormat));
            WriteAttributeString("ry", ry.ToString(M.En_USNumberFormat));
            if(!String.IsNullOrEmpty(strokeColor)) 
                WriteAttributeString("stroke", strokeColor);
            WriteAttributeString("stroke-width", strokeWidth.ToString(M.En_USNumberFormat));
            if(!String.IsNullOrEmpty(fillColor))
                _w.WriteAttributeString("fill", fillColor);
            if(! String.IsNullOrEmpty(transformString))
                WriteAttributeString("transform", transformString);
            WriteEndElement(); // ellipse
        }


        /// <summary>
        /// Writes an SVG "line" element.
        /// </summary>
        /// <param name="type">Not written if null or empty</param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="strokeColor">Not written if null or empty</param>
        /// <param name="strokeWidth">Not written if null</param>
        public void SvgLine(string type, float x1, float y1, float x2, float y2,
            string strokeColor, float strokeWidth,
            string transformString,
            string stroke_linecapString)
        {
            _w.WriteStartElement("line");
            if(!String.IsNullOrEmpty(type))
                _w.WriteAttributeString("class", type);
            _w.WriteAttributeString("x1", x1.ToString(M.En_USNumberFormat));
            _w.WriteAttributeString("y1", y1.ToString(M.En_USNumberFormat));
            _w.WriteAttributeString("x2", x2.ToString(M.En_USNumberFormat));
            _w.WriteAttributeString("y2", y2.ToString(M.En_USNumberFormat));
            if(!String.IsNullOrEmpty(strokeColor))
                _w.WriteAttributeString("stroke", strokeColor);
            _w.WriteAttributeString("stroke-width", strokeWidth.ToString(M.En_USNumberFormat));
            if(!String.IsNullOrEmpty(transformString))
                _w.WriteAttributeString("transform", transformString);
            if(!String.IsNullOrEmpty(stroke_linecapString))
                _w.WriteAttributeString("stroke-linecap", stroke_linecapString);
            _w.WriteEndElement(); //line
        }

        /// <summary>
        /// A square bracket
        /// </summary>
        public void SvgCautionaryBracket(string id, bool isLeftBracket, float top, float right, float bottom, float left, float strokeWidth)
        {
            if(!isLeftBracket)
            {
                float temp = left;
                left = right;
                right = temp;
                //temp = top;
                //top = bottom;
                //bottom = temp;
            }
            string leftStr = left.ToString(M.En_USNumberFormat);
            string topStr = top.ToString(M.En_USNumberFormat);
            string rightStr = right.ToString(M.En_USNumberFormat);
            string bottomStr = bottom.ToString(M.En_USNumberFormat);

            _w.WriteStartElement("path");
            if(!String.IsNullOrEmpty(id))
                _w.WriteAttributeString("id", id);
            _w.WriteAttributeString("stroke", "black");
            _w.WriteAttributeString("fill", "none");
            _w.WriteAttributeString("stroke-width", strokeWidth.ToString(M.En_USNumberFormat));
            _w.WriteAttributeString("stroke-linecap", "butt");
            StringBuilder d = new StringBuilder();
            d.Append("M " + rightStr + "," + topStr + " ");
            d.Append("L " + leftStr + "," + topStr + " " +
                leftStr + "," + bottomStr + " " +
                rightStr + "," + bottomStr);
            _w.WriteAttributeString("d", d.ToString());
            _w.WriteEndElement(); // path
        }
        #endregion

    }
}
