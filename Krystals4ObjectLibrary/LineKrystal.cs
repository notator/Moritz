using Moritz.Globals;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Krystals4ObjectLibrary
{
    public sealed class LineKrystal : Krystal
    {
        /// <summary>
        /// Constructor used for loading a line krystal from a file
        /// </summary>
        public LineKrystal(string filepath)
            : base(filepath)
        {
            string filename = Path.GetFileName(filepath);
            using(XmlReader r = XmlReader.Create(filepath))
            {
                K.ReadToXmlElementTag(r, "line"); // check that this is a line krystal (the other checks have been done in base()
            }
        }
        /// <summary>
        /// Constructor used when creating new line krystals
        /// </summary>
        /// <param name="originalName"></param>
        /// <param name="values"></param>
        public LineKrystal(string originalName, string values)
            : base()
        {
            _name = originalName;
            _level = 1;
            List<uint> valueList = K.GetUIntList(values);
            _numValues = (uint)valueList.Count;
            _minValue = uint.MaxValue;
            _maxValue = uint.MinValue;
            foreach(uint value in valueList)
            {
                _minValue = _minValue < value ? _minValue : value;
                _maxValue = _maxValue > value ? _maxValue : value;
            }
            Strand strand = new Strand(1, valueList);
            _strands.Add(strand);

            _name = GetName(K.KrystalType.line);
        }
        #region overridden functions
        public override void Save(bool overwrite)
        {
            string pathname;
            if(!K.IsLineKrystalFilename(_name))
            {
                _name = GetName(K.KrystalType.line);
            }
            pathname = K.KrystalsFolder + @"\" + _name;

            if(File.Exists(pathname) == false || overwrite)
            {
                XmlWriter w = base.BeginSaveKrystal(); // disposed of in EndSaveKrystal
                #region save heredity info (only that this is a line)
                w.WriteStartElement("line");
                w.WriteEndElement(); // line
                #endregion
                base.EndSaveKrystal(w); // saves the strands, closes the document, disposes of w
            }
            //StringBuilder msgSB = new StringBuilder("Line krystal:\r\n\r\n    ");
            //foreach(uint value in this.Strands[0].Values)
            //{
            //    msgSB.Append(value.ToString() + " ");
            //}
            //msgSB.Append("      \r\n\r\nsaved as:");
            //msgSB.Append("\r\n\r\n    " + _name + "  ");
            //if(alreadyExisted)
            //{
            //    msgSB.Append("\r\n\r\n(This line krystal already existed.)");
            //}
            //MessageBox.Show(msgSB.ToString(), "Saved", MessageBoxButtons.OK, MessageBoxIcon.None);
        }
        public override void Rebuild()
        {
            // This function does nothing. Line krystals are not dependent on other krystals!
            throw new Exception("The method or operation is deliberately not implemented.");
        }
        #endregion overridden functions
    }
}

