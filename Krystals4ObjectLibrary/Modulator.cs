using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

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
            //_name = K.UntitledModulatorName;
            _name = ""; // is set when the modulator is saved
            _xDim = xDim;
            _yDim = yDim;
            _array = new int[xDim, yDim];
        }
        #endregion constructors
        #region public functions
        public void Save()
        {
            bool equivalentExists = false;
            string pathname = "";
            if(string.IsNullOrEmpty(_name)) // this is a new or newly edited modulator
            {
                DirectoryInfo dir = new DirectoryInfo(K.ModulationOperatorsFolder);
                foreach(FileInfo fileInfo in dir.GetFiles("m*.kmod"))
                {
                    Modulator otherModulator = new Modulator(K.ModulationOperatorsFolder + @"\" + fileInfo.Name);
                    if(this.CompareTo(otherModulator) == 0)
                    {
                        equivalentExists = true;
                        _name = otherModulator.Name;
                        pathname = K.ModulationOperatorsFolder + @"\" + _name;
                        break;
                    }
                }

                if(!equivalentExists) // generate a new name
                {
                    int fileIndex = 1;
                    do
                    {
                        _name = String.Format("m{0}x{1}({2})-{3}{4}",
                            _xDim, _yDim, MaxValue, fileIndex, K.ModulatorFilenameSuffix);
                        pathname = K.ModulationOperatorsFolder + @"\" + _name;
                        fileIndex++;
                    } while(File.Exists(pathname));
                }
            }
            else pathname = K.ModulationOperatorsFolder + @"\" + _name;

            if(MaxValueHasChanged()) // rename the current modulator
            {
                File.Delete(pathname);
                _name = "";
                Save(); // recursive call saves under a new name
            }
            else if(!equivalentExists)
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.IndentChars = ("\t");
                settings.CloseOutput = true;

                using(XmlWriter w = XmlWriter.Create(pathname, settings))
                {
                    w.WriteStartDocument();
                    w.WriteComment("created: " + K.Now);

                    w.WriteStartElement("modulator");
                    w.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                    w.WriteAttributeString("xsi", "noNamespaceSchemaLocation", null, K.MoritzXmlSchemasFolder + @"\krystals.xsd");

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

        /// <summary>
        /// Returns 0 if both matrices are identical, otherwise compares the two names.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(object other)
        {
            Modulator otherModulator = other as Modulator;
            if(otherModulator == null)
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
        private string _name;
        private int _xDim = 1;
        private int _yDim = 1;
        private int[,] _array = new int[1, 1];
        #endregion private variables
    }
}

