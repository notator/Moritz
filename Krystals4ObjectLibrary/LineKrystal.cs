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
        /// <summary>
        /// Sets the krystal's Name, and saves it (but not any of its ancestor files).
        /// If a krystal having identical content exists in the krystals directory,
        /// the user is given the option to
        ///    either overwrite the existing krystal (using that krystal's index),
        ///    or abort the save.
        /// This means that a given set of ancestors should always have the same index.
        /// </summary>
        public override void Save()
        {
            bool LineIsUnique(out string name)
            {
                var isUnique = true;
                name = GetName(K.KrystalType.line); // default name (with an index that is not used in the krystals folder)

                IEnumerable<string> similarKrystalPaths = GetSimilarKrystalPaths(name);

                var theseValues = Strands[0].Values;
                foreach(var existingPath in similarKrystalPaths)
                {
                    var existingKrystal = new LineKrystal(existingPath);
                    var existingValues = existingKrystal.Strands[0].Values;
                    if(theseValues.SequenceEqual(existingValues))
                    {
                        isUnique = false;
                        name = Path.GetFileName(existingPath);
                        break;
                    }
                }
                return isUnique;
            }

            DialogResult answer = DialogResult.Yes;
            if(LineIsUnique(out _name) == false)
            {
                string msg = $"Line krystal {_name} already existed. Save it again with a new date?";
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
            }
        }

        public override void Rebuild()
        {
            // This function does nothing. Line krystals are not dependent on other krystals!
            throw new Exception("The method or operation is deliberately not implemented.");
        }
        #endregion overridden functions
    }
}

