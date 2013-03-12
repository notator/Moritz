using System;
using System.Xml;

namespace Moritz.Score
{
    /// <summary>
    /// Adapted from http://www.tkachenko.com/blog/archives/000585.html
    /// </summary>
    public class XmlWriterWrapper : XmlWriter
    {
        protected XmlWriter _w;

        public XmlWriterWrapper(XmlWriter xmlWriter)
        {
            this._w = xmlWriter;
        }

        public override void Close()
        {
            this._w.Close();
        }

        protected override void Dispose(bool disposing)
        {
            ((IDisposable)this._w).Dispose();
        }

        public override void Flush()
        {
            this._w.Flush();
        }

        public override string LookupPrefix(string ns)
        {
            return this._w.LookupPrefix(ns);
        }

        public override void WriteBase64(byte[] buffer, int index, int count)
        {
            this._w.WriteBase64(buffer, index, count);
        }

        public override void WriteCData(string text)
        {
            this._w.WriteCData(text);
        }

        public override void WriteCharEntity(char ch)
        {
            this._w.WriteCharEntity(ch);
        }

        public override void WriteChars(char[] buffer, int index, int count)
        {
            this._w.WriteChars(buffer, index, count);
        }

        public override void WriteComment(string text)
        {
            this._w.WriteComment(text);
        }

        public override void WriteDocType(string name, string pubid, string sysid, string subset)
        {
            this._w.WriteDocType(name, pubid, sysid, subset);
        }

        public override void WriteEndAttribute()
        {
            this._w.WriteEndAttribute();
        }

        public override void WriteEndDocument()
        {
            this._w.WriteEndDocument();
        }

        public override void WriteEndElement()
        {
            this._w.WriteEndElement();
        }

        public override void WriteEntityRef(string name)
        {
            this._w.WriteEntityRef(name);
        }

        public override void WriteFullEndElement()
        {
            this._w.WriteFullEndElement();
        }

        public override void WriteProcessingInstruction(string name, string text)
        {
            this._w.WriteProcessingInstruction(name, text);
        }

        public override void WriteRaw(string data)
        {
            this._w.WriteRaw(data);
        }

        public override void WriteRaw(char[] buffer, int index, int count)
        {
            this._w.WriteRaw(buffer, index, count);
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns)
        {
            this._w.WriteStartAttribute(prefix, localName, ns);
        }

        public override void WriteStartDocument()
        {
            this._w.WriteStartDocument();
        }

        public override void WriteStartDocument(bool standalone)
        {
            this._w.WriteStartDocument(standalone);
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            this._w.WriteStartElement(prefix, localName, ns);
        }

        public override void WriteString(string text)
        {
            this._w.WriteString(text);
        }

        public override void WriteSurrogateCharEntity(char lowChar, char highChar)
        {
            this._w.WriteSurrogateCharEntity(lowChar, highChar);
        }

        public override void WriteValue(bool value)
        {
            this._w.WriteValue(value);
        }

        public override void WriteValue(DateTime value)
        {
            this._w.WriteValue(value);
        }

        public override void WriteValue(decimal value)
        {
            this._w.WriteValue(value);
        }

        public override void WriteValue(double value)
        {
            this._w.WriteValue(value);
        }

        public override void WriteValue(int value)
        {
            this._w.WriteValue(value);
        }

        public override void WriteValue(long value)
        {
            this._w.WriteValue(value);
        }

        public override void WriteValue(object value)
        {
            this._w.WriteValue(value);
        }

        public override void WriteValue(float value)
        {
            this._w.WriteValue(value);
        }

        public override void WriteValue(string value)
        {
            this._w.WriteValue(value);
        }

        public override void WriteWhitespace(string ws)
        {
            this._w.WriteWhitespace(ws);
        }


        public override XmlWriterSettings Settings
        {
            get
            {
                return this._w.Settings;
            }
        }

        public override System.Xml.WriteState WriteState
        {
            get
            {
                return this._w.WriteState;
            }
        }

        public override string XmlLang
        {
            get
            {
                return this._w.XmlLang;
            }
        }

        public override System.Xml.XmlSpace XmlSpace
        {
            get
            {
                return this._w.XmlSpace;
            }
        }
    }
}