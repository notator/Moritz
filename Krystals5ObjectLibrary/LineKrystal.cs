using Moritz.Globals;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Krystals5ObjectLibrary
{
    public sealed class LineKrystal : Krystal
    {
        /// <summary>
        /// Constructor used for loading a line krystal from a file.
        /// The Krystal base class reads the strand. Line krystals have no heredity info.
        /// </summary>
        public LineKrystal(string filepath)
            : base(filepath)
        {
            string filename = Path.GetFileName(filepath);
            using(XmlReader r = XmlReader.Create(filepath))
            {
                K.ReadToXmlElementTag(r, "line"); // check that this is a line krystal (the other checks have been done in base()
            }

            //Name = UniqueLineKrystalName();
        }
        /// <summary>
        /// Constructor used when creating new line krystals.
        /// This constructor generates a unique name for the krystal.
        /// </summary>
        public LineKrystal(List<uint> valueList)
            : base()
        {
            Level = 1;
            NumValues = (uint)valueList.Count;
            MinValue = uint.MaxValue;
            MaxValue = uint.MinValue;
            foreach(uint value in valueList)
            {
                MinValue = MinValue < value ? MinValue : value;
                MaxValue = MaxValue > value ? MaxValue : value;
            }
            Strand strand = new Strand(1, valueList);
            Strands = new List<Strand>() { strand };

            Name = GetUniqueName(K.KrystalType.line);
        }

        #region overridden functions

        /// <summary>
        /// If a krystal having identical content exists in the krystals directory,
        /// the user is given the option to
        ///    either overwrite the existing krystal (using that krystal's index),
        ///    or abort the save.
        /// This means that a given set of ancestors should always have the same index.
        /// </summary>
        public override bool Save()
        {
            bool hasBeenSaved = false;

            var pathname = K.KrystalsFolder + @"\" + Name;
            DialogResult answer = DialogResult.Yes;
            if(File.Exists(pathname))
            {
                string msg = $"Line krystal {Name} already exists.\nSave it again with a new date?";
                answer = MessageBox.Show(msg, "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            }

            if(answer == DialogResult.Yes)
            {
                XmlWriter w = base.BeginSaveKrystal(); // disposed of in EndSaveKrystal
                #region save heredity info (only that this is a line)
                w.WriteStartElement("line");
                w.WriteEndElement(); // line
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
            // This function does nothing. Line krystals are not dependent on other krystals!
            throw new Exception("The method or operation is deliberately not implemented.");
        }
        #endregion overridden functions
    }
}

