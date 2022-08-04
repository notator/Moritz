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
    public sealed class ConstantKrystal : Krystal
    {
        /// <summary>
        /// Constructor used for loading a constant krystal from a file
        /// </summary>
        public ConstantKrystal(string filepath)
            : base(filepath)
        {
            string filename = Path.GetFileName(filepath);
            using(XmlReader r = XmlReader.Create(filepath))
            {
                K.ReadToXmlElementTag(r, "constant"); // check that this is a constant krystal (the other checks have been done in base())
            }
        }
        /// <summary>
        /// Constructor used when creating new constants
        /// </summary>
        /// <param name="originalName"></param>
        /// <param name="value"></param>
        public ConstantKrystal(string originalName, uint value)
            : base()
        {
            _name = originalName;
            _level = 0;
            _minValue = _maxValue = value;
            _numValues = 1;
			List<uint> valueList = new List<uint>
			{
				value
			};
			Strand strand = new Strand(0, valueList);
            _strands.Add(strand);

            _name = GetName(K.KrystalType.constant);
        }
        #region overridden functions
        public override void Save(bool overwrite)
        {
            if(!K.IsConstantKrystalFilename(_name))
            {
                _name = GetName(K.KrystalType.constant);
            }

            var pathname = K.KrystalsFolder + @"\" + _name;

            if(File.Exists(pathname) == false || overwrite)
            {
                XmlWriter w = base.BeginSaveKrystal(); // disposed of in EndSaveKrystal
                #region save heredity info (only that this is a constant...)
                w.WriteStartElement("constant");
                w.WriteEndElement(); // constant
                #endregion
                base.EndSaveKrystal(w); // saves the strands, closes the document, disposes of w
            }
            //string msg = "Constant krystal saved as \r\n\r\n" +
            //             "   " + _name;
            //MessageBox.Show(msg, "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        public override void Rebuild()
        {
            // This function does nothing. Constant krystals are not dependent on other krystals!
            throw new Exception("The method or operation is deliberately not implemented.");
        }
        #endregion overridden functions
    }
}

