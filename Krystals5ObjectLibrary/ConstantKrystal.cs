using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace Krystals5ObjectLibrary
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

            Name = String.Format($"{this.MaxValue}.0.1.{K.KrystalType.constant}{K.KrystalFilenameSuffix}");
        }
        /// <summary>
        /// Constructor used when creating new constants.
        /// This constructor generates a unique name for the krystal.
        /// </summary>
        /// <param name="originalName"></param>
        /// <param name="value"></param>
        public ConstantKrystal(uint value)
            : base()
        {
            Name = String.Format($"{value}.0.1.{K.KrystalType.constant}{K.KrystalFilenameSuffix}");
            Level = 0;
            MinValue = MaxValue = value;
            NumValues = 1;
            List<uint> valueList = new List<uint>
            {
                value
            };
            Strand strand = new Strand(0, valueList);
            Strands = new List<Strand>() { strand };
        }
        #region overridden functions
        /// <summary>
        /// If a krystal with the same name already exists in the krystals directory,
        /// the user is given the option to
        ///    either overwrite the existing krystal (with a new date),
        ///    or abort the save.
        /// </summary>
        public override bool Save()
        {
            var hasBeenSaved = false;
            var pathname = K.KrystalsFolder + @"\" + Name;

            DialogResult answer = DialogResult.Yes;
            if(File.Exists(pathname))
            {
                string msg = $"Constant krystal {Name} already exists.\nSave it again with a new date?";
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

                MessageBox.Show($"{Name} saved.", "Saved", MessageBoxButtons.OK);
                hasBeenSaved = true;
            }
            else
            {
                MessageBox.Show($"{Name} not saved.", "Save Aborted", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return hasBeenSaved;
        }
        public override void Rebuild()
        {
            // This function does nothing. Constant krystals are not dependent on other krystals!
            throw new Exception("The method or operation is deliberately not implemented.");
        }
        #endregion overridden functions
    }
}

