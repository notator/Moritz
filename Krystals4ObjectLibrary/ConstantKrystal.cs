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
        /// Constructor used for loading a constant krystal from a file.
        /// The strand is loaded by the base class. Constant krystals have no heredity info.
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
        public ConstantKrystal(uint value)
            : base()
        {
            _level = 0;
            _minValue = _maxValue = value;
            _numValues = 1;
			List<uint> valueList = new List<uint>
			{
				value
			};
			Strand strand = new Strand(0, valueList);
            _strands.Add(strand);
        }
        #region overridden functions
        /// <summary>
        /// Sets the krystal's Name, and saves it.
        /// If a krystal having identical content exists in the krystals directory,
        /// the user is given the option to
        ///    either overwrite the existing krystal (with a new date),
        ///    or abort the save.
        /// </summary>
        public override void Save()
        {
            _name = String.Format($"{MaxValue}.0.1.{K.KrystalType.constant}{K.KrystalFilenameSuffix}");

            var pathname = K.KrystalsFolder + @"\" + _name;

            DialogResult answer = DialogResult.Yes;
            if(File.Exists(pathname))
            {
                string msg = $"Constant krystal {_name} already exists. Save it again with a new date?";
                answer = MessageBox.Show(msg, "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            }

            if(answer == DialogResult.Yes)
            {
                XmlWriter w = base.BeginSaveKrystal(); // disposed of in EndSaveKrystal
                #region save heredity info (only that this is a constant...)
                w.WriteStartElement("constant");
                w.WriteEndElement(); // constant
                #endregion
                base.EndSaveKrystal(w); // saves the strands, closes the document, disposes of w
            }
        }
        public override void Rebuild()
        {
            // This function does nothing. Constant krystals are not dependent on other krystals!
            throw new Exception("The method or operation is deliberately not implemented.");
        }
        #endregion overridden functions
    }
}

