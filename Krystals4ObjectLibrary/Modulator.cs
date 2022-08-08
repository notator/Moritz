using Moritz.Globals;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Krystals4ObjectLibrary
{
    /// <summary>
    /// A two dimensional array of unsigned integers used for modulating krystals.
    /// </summary>
    public sealed class Modulator : INamedComparable
    {
        #region constructors
        public Modulator(string filepath)
        {
            string NewModulatorFilename(string oldModulatorName)
            {
                string rval;
                string on = oldModulatorName;
                if(on.StartsWith("m"))
                {
                    int mInd = on.IndexOf("m"); // = 0
                    int xInd = on.IndexOf("x");
                    int bInd = on.IndexOf("(");
                    int iInd = on.IndexOf("-");
                    int dotInd = on.IndexOf(".");

                    string xStr = on.Substring(mInd + 1, xInd - mInd - 1);
                    string yStr = on.Substring(xInd + 1, bInd - xInd - 1);
                    string dStr = on.Substring(bInd + 1, iInd - bInd - 2);
                    string iStr = on.Substring(iInd + 1, dotInd - iInd - 1);

                    int.TryParse(xStr, out int xDim);
                    int.TryParse(yStr, out int yDim);
                    int.TryParse(dStr, out int maxValue);
                    int.TryParse(iStr, out int index);
                    rval = String.Format($"{xDim}.{yDim}.{maxValue}.{index}.kmod");
                }
                else rval = oldModulatorName;

                return rval;
            }

            _name = NewModulatorFilename(Path.GetFileName(filepath));

            filepath = K.ModulationOperatorsFolder + @"/" + _name;

            using(XmlReader r = XmlReader.Create(filepath))
            {
                _name = Path.GetFileName(filepath); ;
                K.ReadToXmlElementTag(r, "modulator"); // check that this is a modulator
                K.ReadToXmlElementTag(r, "array");
                for(int i = 0 ; i < 2 ; i++)
                {
                    r.MoveToAttribute(i);
                    switch(r.Name)
                    {
                        case "xdim":
                            _xDim = int.Parse(r.Value);
                            break;
                        case "ydim":
                            _yDim = int.Parse(r.Value);
                            break;
                    }
                }
                List<uint> valueList = K.GetUIntList(r.ReadString());
                if(valueList.Count != (_xDim * _yDim))
                {
                    string msg = "Error in file: incorrect dimensions for modulator array.";
                    throw new ApplicationException(msg);
                }
                _array = new int[_xDim, _yDim];
                int valueIndex = 0;
                for(int y = 0 ; y < _yDim ; y++)
                    for(int x = 0 ; x < _xDim ; x++)
                        _array[x, y] = (int) valueList[valueIndex++];
            }
        }
        public Modulator(int xDim, int yDim)
        {
            _xDim = xDim;
            _yDim = yDim;
            _array = new int[xDim, yDim];
        }
        #endregion constructors
        #region public functions
        /// <summary>
        /// Sets the modulator's Name, and saves it.
        /// If a modulator having identical content exists in the modulators directory,
        /// the user is given the option to
        ///    either overwrite the existing modulator (with a new date),
        ///    or abort the save.
        /// </summary>
        public void Save()
        {
            bool ModulatorIsUnique(out string name)
            {
                var isUnique = true;
                var nameRoot = String.Format("{0}.{1}.{2}.", _xDim, _yDim, MaxValue);

                IEnumerable<string> similarModulatorPaths = GetSimilarModulatorPaths(nameRoot);

                // default (with unique index)
                name = nameRoot + (similarModulatorPaths.Count() + 1).ToString() + K.ModulatorFilenameSuffix;

                foreach(var existingPath in similarModulatorPaths)
                {
                    var existingModulator = new Modulator(existingPath);
                    bool isIdentical = true;
                    bool checkNextModulator = false;
                    for(int x = 0;  x < _xDim; x++)
                    {
                        for(int y = 0; y < _yDim; y++)
                        {
                            if(existingModulator.Array[x,y] != Array[x,y])
                            {
                                isIdentical = false;
                                checkNextModulator = true;
                                break;
                            }
                            if(checkNextModulator)
                            {
                                break;
                            }
                        }
                        if(checkNextModulator)
                        {
                            break;
                        }
                    }
                    if(isIdentical)
                    {
                        isUnique = false;
                        name = existingModulator.Name;
                        break;
                    }
                }
                return isUnique;
            }

            DialogResult answer = DialogResult.Yes;
            if(ModulatorIsUnique(out _name) == false)
            {
                string msg = $"An identical modulator ({_name}) already existed. Save it again with a new date?";
                answer = MessageBox.Show(msg, "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            }

            if(answer == DialogResult.Yes)
            {
                var pathname = M.LocalMoritzModulationOperatorsFolder + "/" + Name;

                XmlWriterSettings settings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = ("\t"),
                    CloseOutput = true
                };

                using(XmlWriter w = XmlWriter.Create(pathname, settings))
                {
                    w.WriteStartDocument();
                    w.WriteComment("created: " + K.Now);

                    w.WriteStartElement("modulator");
                    w.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                    w.WriteAttributeString("xsi", "noNamespaceSchemaLocation", null, K.MoritzXmlSchemasFolder + @"/krystals.xsd");

                    w.WriteStartElement("array");
                    w.WriteAttributeString("xdim", _xDim.ToString());
                    w.WriteAttributeString("ydim", _yDim.ToString());
                    w.WriteString(MatrixAsString);
                    w.WriteEndElement(); // array
                    w.WriteEndElement(); // modulator    
                    w.Close();
                }
            }
        }

        private IEnumerable<string> GetSimilarModulatorPaths(string nameRoot)
        {
            var dirPath = M.LocalMoritzModulationOperatorsFolder;
            
            var searchString = nameRoot + "*" + K.ModulatorFilenameSuffix;
            var modulatorPaths = Directory.EnumerateFiles(dirPath, searchString);

            return modulatorPaths;
        }

        /// <summary>
        /// Returns 0 if both matrices are identical, otherwise compares the two names.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(object other)
        {
			if(!(other is Modulator otherModulator))
				throw new ArgumentException();

			bool modulatorsAreEquivalent = false;
            if(this.XDim == otherModulator.XDim
            && this.YDim == otherModulator.YDim)
            {
                modulatorsAreEquivalent = true;
                for(int x = 0 ; x < this.XDim ; x++)
                {
                    for(int y = 0 ; y < this.YDim ; y++)
                    {
                        if(this.Array[x, y] != otherModulator.Array[x, y])
                        {
                            modulatorsAreEquivalent = false;
                            break;
                        }
                    }
                    if(modulatorsAreEquivalent == false)
                        break;
                }
            }

            if(modulatorsAreEquivalent)
                return 0;
            else
                return this.Name.CompareTo(otherModulator.Name);
        }

        private bool MaxValueHasChanged()
        {
            string[] segs = _name.Split('(', ')');
            uint max = uint.Parse(segs[1]);
            if(max == MaxValue)
                return false;
            else
                return true;
        }

        public override string ToString()
        {
            return this.Name;
        }
        #endregion public functions
        #region properties
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        public int XDim
        {
            get { return _xDim; }
            set { _xDim = value; }
        }
        public int YDim
        {
            get { return _yDim; }
            set { _yDim = value; }
        }
        public int[,] Array
        {
            get { return _array; }
            set { _array = value; }
        }
        private string MatrixAsString
        {
            get
            {
                StringBuilder s = new StringBuilder();
                string introTabs = "\n\t\t\t\t";
                for(int y = 0 ; y < _yDim ; y++)
                {
                    s.Append(introTabs);
                    for(int x = 0 ; x < _xDim ; x++)
                    {
                        s.Append(_array[x, y].ToString());
                        s.Append("\t");
                    }
                }
                s.Append("\n\t\t\t");
                return s.ToString();
            }
        }
        private int MaxValue
        {
            get
            {
                int maxValue = 0;
                for(int y = 0 ; y < _yDim ; y++)
                    for(int x = 0 ; x < _xDim ; x++)
                        maxValue = maxValue < _array[x, y] ? _array[x, y] : maxValue;
                return maxValue;
            }
        }
        #endregion properties
        #region private variables
        private string _name = "";
        private int _xDim = 1;
        private int _yDim = 1;
        private int[,] _array = new int[1, 1];
        #endregion private variables
    }
}

