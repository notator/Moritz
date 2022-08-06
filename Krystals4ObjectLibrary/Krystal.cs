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
    #region helper classes
    /// <summary>
    /// The struct returned by the Enumerable property Krystal.LeveledValues.
    /// Each value in the krystal is returned with its level.
    /// The first value in each strand has the strand's level.
    /// All other values have a level equal to the krystal's level + 1.
    /// </summary>
    public struct LeveledValue
    {
        public int level;
        public int value;
    }
    /// <summary>
    /// this class contains strand parameters, and is used to build the _strandNodeList for an expansion.
    /// The _strandNodeList is used as a parameter in the following constructors:
    ///     FieldEditor.Painter
    ///     FieldEditor.ExpansionTreeView
    ///     FieldEditor.Expansion
    /// </summary>
    public class StrandNode : TreeNode
    {
        public StrandNode(int moment, int level, int density, int point)
        {
            strandMoment = moment;
            strandLevel = level;
            strandDensity = density;
            strandPoint = point;
        }
        public int strandMoment;
        public int strandLevel;
        public int strandDensity;
        public int strandPoint;
    }
    public class ContouredStrandNode : StrandNode
    {
        public ContouredStrandNode(int moment, int level, int density, int point, int axis, int contour)
            : base(moment, level, density, point)
        {
            strandAxis = axis;
            strandContour = contour;
        }
        public int strandAxis;
        public int strandContour;
    }
    #endregion helper classes

    public interface INamedComparable : IComparable
    {
        string Name { get; }
    }

    public abstract class Krystal : INamedComparable
    {
        public Krystal() { }

        /// <summary>
        /// Checks that the argument file is a krystal definition, and loads the strands.
        /// Does not load heredity info.
        /// </summary>
        /// <param name="filepath"></param>
        public Krystal(string filepath)
        {
            string filename = Path.GetFileName(filepath);
            try
            {
                using(XmlReader r = XmlReader.Create(filepath))
                {
                    _name = Path.GetFileName(filepath);
                    _minValue = uint.MaxValue;
                    _maxValue = uint.MinValue;
                    K.ReadToXmlElementTag(r, "krystal"); // check that this is a krystal
                    // ignore the heredity info in the next element (<expansion> etc.)
                    K.ReadToXmlElementTag(r, "strands"); // check that there is a "strands" element
                    // get the strands and their related variables
                    K.ReadToXmlElementTag(r, "s");
                    while(r.Name == "s")
                    {
                        Strand strand = new Strand(r);
                        _level = (_level < strand.Level) ? strand.Level : _level;
                        foreach(uint value in strand.Values)
                        {
                            _minValue = (_minValue < value) ? _minValue : value;
                            _maxValue = (_maxValue < value) ? value : _maxValue;
                        }
                        _numValues += (uint)strand.Values.Count;
                        _strands.Add(strand);
                    }
                    // r.Name is the end tag "strands" here
                }
            }
            catch(Exception ex)
            {
                throw (ex);
            }
        }
        #region public functions
        /// <summary>
        /// Updates the strand-related Properties Strands, Level, MinValue, MaxValue and NumValues
        /// using the given list of strands.
        /// </summary>
        /// <param name="strands"></param>
        public void Update(List<Strand> strands)
        {
            _strands = strands;
            _level = 0;
            _minValue = uint.MaxValue;
            _maxValue = uint.MinValue;
            _numValues = 0;
            foreach(Strand strand in strands)
            {
                _numValues += (uint)strand.Values.Count;
                _level = (_level < strand.Level) ? strand.Level : _level;
                foreach(uint value in strand.Values)
                {
                    _minValue = (_minValue < value) ? _minValue : value;
                    _maxValue = (_maxValue < value) ? value : _maxValue;
                }
            }
        }
        /// <summary>
        /// Sets the krystal's Name, and saves it (but not any of its ancestor files).
        /// If a krystal having identical content exists in the krystals directory,
        /// the user is given the option to
        ///    either overwrite the existing krystal (using that krystal's index),
        ///    or abort the save.
        /// This means that a given set of ancestors should always have the same index.
        /// </summary>
        public abstract void Save();

        /// <summary>
        /// This function returns 0 if the shape and numeric content are the same,
        /// Otherwise it compares the two names.
        /// </summary>
        /// <param name="otherKrystal"></param>
        /// <returns></returns>
        public int CompareTo(object other)
        {
			if(!(other is Krystal otherKrystal))
				throw new ArgumentException();

			bool isEquivalent = false;
            if(this.Shape == otherKrystal.Shape
            && this.Strands.Count == otherKrystal.Strands.Count
            && this.Level == otherKrystal.Level
            && this.MaxValue == otherKrystal.MaxValue
            && this.MinValue == otherKrystal.MinValue
            && this.NumValues == otherKrystal.NumValues
            && this.MissingValues == otherKrystal.MissingValues)
            {
                isEquivalent = true;
                for(int i = 0; i < this.Strands.Count; i++)
                {
                    Strand thisStrand = this.Strands[i];
                    Strand otherStrand = otherKrystal.Strands[i];
                    if(thisStrand.Values.Count != otherStrand.Values.Count
                    || thisStrand.Level != otherStrand.Level)
                    {
                        isEquivalent = false;
                        break;
                    }
                    for(int j = 0; j < thisStrand.Values.Count; j++)
                    {
                        if(thisStrand.Values[j] != otherStrand.Values[j])
                        {
                            isEquivalent = false;
                            break;
                        }
                    }
                }
            }
            if(isEquivalent == true)
                return 0;
            else
                return this.Name.CompareTo(otherKrystal.Name);
        }

        /// <summary>
        /// Recreates this krystal using its existing inputs, without opening an editor.
        /// Used to preserve the integrity of a group of krystals, when one of them has changed.
        /// </summary>
        public abstract void Rebuild();
        public override string ToString()
        {
            return this._name;
        }
        /// <summary>
        /// This krystal can be permuted at the given level if it has less then 7 elements at that level.
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public bool IsPermutableAtLevel(uint level)
        {
            bool isPermutableAtLevel = true;
            int nElements = 0;
            if(level > this.Level || level < 1)
            {
                isPermutableAtLevel = false;
            }
            else if(level < this.Level)
            {
                foreach(Strand strand in this.Strands)
                {
                    if(strand.Level <= level)
                    {
                        nElements = 1;
                    }
                    else if(strand.Level == level + 1)
                    {
                        nElements++;
                        if(nElements > 7)
                        {
                            isPermutableAtLevel = false;
                            break;
                        }
                    }
                }
            }
            else // level == this.Level
            {
                foreach(Strand strand in this.Strands)
                {
                    if(strand.Values.Count > 7)
                    {
                        isPermutableAtLevel = false;
                        break;
                    }
                }
            }
            return isPermutableAtLevel;
        }

		/// <summary>
		/// Returns the flat krystal values, spread over the range [minEnvValue..maxEnvValue],
		/// as an Envelope.
		/// </summary>
		/// <param name="minEnvValue"></param>
		/// <param name="maxEnvValue"></param>
		/// <returns></returns>
		public Envelope ToEnvelope(int minEnvValue, int maxEnvValue)
		{
			List<int> substituteValues = new List<int>();
			double incr = ((double)(maxEnvValue - minEnvValue)) / (MaxValue - MinValue);
			for(int i = (int)MinValue; i < (MaxValue + 1); i++)
			{
				substituteValues.Add((int) Math.Round(minEnvValue + (incr * (i - 1))));
			}

			List<int> kValues = GetValues(1)[0];

			for(int i = 0; i < kValues.Count; i++)
			{
				kValues[i] = substituteValues[kValues[i] - 1];
			}

			Envelope envelope = new Envelope(kValues, maxEnvValue, maxEnvValue, kValues.Count);

			return envelope;
		}
		#endregion public functions
		#region protected functions
        protected XmlWriter BeginSaveKrystal()
        {
			XmlWriterSettings settings = new XmlWriterSettings
			{
				Indent = true,
				IndentChars = ("\t"),
				CloseOutput = true
			};
			string namePath = K.KrystalsFolder + @"\" + _name;
            XmlWriter w = XmlWriter.Create(namePath, settings);
            w.WriteStartDocument();
            w.WriteComment("created: " + K.Now);

            w.WriteStartElement("krystal"); // ended in EndSaveKrystal()
            w.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
            w.WriteAttributeString("xsi", "noNamespaceSchemaLocation", null, K.MoritzXmlSchemasFolder + @"/krystals.xsd");

            return w;
        }
        protected void EndSaveKrystal(XmlWriter w)
        {
            w.WriteStartElement("strands");
            foreach(Strand strand in _strands)
            {
                w.WriteStartElement("s");
                w.WriteAttributeString("l", strand.Level.ToString());
                w.WriteString(K.GetStringOfUnsignedInts(strand.Values));
                w.WriteEndElement();
            }
            w.WriteEndElement(); // ends the strands
            w.WriteEndElement(); // ends the krystal element
            w.Close(); // closes the stream and (I think) disposes of w            
        }
        #endregion protected functions
        #region public properties
        public string Name { get { return _name; } set { _name = value; } }
        public uint Level { get { return _level; } set { _level = value; } }
        public uint MinValue { get { return _minValue; } }
        public uint MaxValue { get { return _maxValue; } }
        public uint NumValues { get { return _numValues; } }
        public List<Strand> Strands { get { return _strands; } }
        /// <summary>
        /// A string of comma-separated values not contained in the krystal
        /// </summary>
        public string MissingValues
        {
            get
            {
                int[] nValues = new int[this._maxValue + 1];
                foreach(Strand strand in this._strands)
                    foreach(uint value in strand.Values)
                        nValues[value]++;

                StringBuilder sb = new StringBuilder();
                for(uint i = _minValue; i <= this._maxValue; i++)
                    if(nValues[i] == 0)
                    {
                        sb.Append(", ");
                        sb.Append(i.ToString());
                    }
                if(sb.Length > 0)
                    sb.Remove(0, 2);
                else
                    sb.Append("(none)");
                return sb.ToString();
            }
        }

        /// <summary>
        /// At level 1, the single int list contains all the strand values as a single list.
        /// At level krystal.Level, each internal int list contains the values from a single strand.
        /// At level krystal.Level + 1, each internal int list contains a single strand value.
        /// An ArgumentOutOfRangeException is thrown if the argument is outside the range 1..krystal.Level + 1.
        /// </summary>
        /// <param name="level">Must be in range 1..krystal.Level+1</param>
        /// <returns>The values in the krystal as a list of int lists.</returns>
        public List<List<int>> GetValues(uint level)
        {
            if(level < 1 || level > _level + 1)
                throw new ArgumentOutOfRangeException("Error in Krystal.GetValues().");

            List<List<int>> returnList = new List<List<int>>();
            int[] shapeArray = ShapeArray;
            int nInternalLists = shapeArray[level-1];
            for(int i = 0; i < nInternalLists; i++)
            {
                List<int> internalList = new List<int>();
                returnList.Add(internalList);
            }

            int returnListIndex = -1;
            foreach(Strand strand in this._strands)
            {
                if(level == _level + 1)
                {
                    foreach(uint value in strand.Values)
                    {
                        returnListIndex++;
                        returnList[returnListIndex].Add((int)value);
                    }
                }
                else
                {
                    if(strand.Level <= level)
                        returnListIndex++;

                    foreach(uint value in strand.Values)
                        returnList[returnListIndex].Add((int)value);
                }
            }

            return returnList;
        }

        public int[] ShapeArray
        {
            get
            {
                int[] shapeArray = new int[this._level + 1];
                foreach(Strand strand in this._strands)
                {
                    for(uint i = strand.Level; i <= this._level; i++)
                        shapeArray[i - 1] += 1;
                    shapeArray[this._level] += strand.Values.Count;
                }
                return shapeArray;
            }
        }
        /// <summary>
        /// A string of " : " separated values showing the final clock sizes
        /// </summary>
        public string Shape
        {
            get
            {
                string shapeString = "";
                if(this._level > 0)
                {
                    int[] shapeArray = ShapeArray;
                    StringBuilder shapeStrB = new StringBuilder();
                    for(int i = 0; i <= this._level; i++)
                    {
                        shapeStrB.Append(" : ");
                        shapeStrB.Append(shapeArray[i].ToString());
                    }
                    shapeStrB.Remove(0, 3);
                    shapeString = shapeStrB.ToString();
                }
                return shapeString;
            }
        }
        #endregion public properties

        /// <summary>
        /// returns the paths to all the krystals whose name is krystalName except for the index.
        /// </summary>
        /// <param name="krystalName"></param>
        /// <returns></returns>
        protected IEnumerable<string> GetSimilarKrystalPaths(string nameRoot, K.KrystalType krystalType)
        {
            var searchString = nameRoot + ".*." + krystalType.ToString() + ".krys";

            var similarKrystalPaths = Directory.EnumerateFiles(M.LocalMoritzKrystalsFolder, searchString);

            return similarKrystalPaths;
        }

        /// <summary>
        /// A Krystal's name root consists of its domain (=MaxValue) followed by a '.' character, followed by a shapeNameString followed by a '.' character.
        /// The shapeNameString contains one or more integers separated by '_' characters.
        /// The first int in the shapeNameString is the number of level 1 and level 2 strands, so:
        ///  "0" is a constant krystal -- containing one strand having level 0 and one value (=domain) (no level 1 or level 2 strands).
        ///  "1_[nValues]" is a line krystal -- containing one strand having level 1 and [nValues] values (no level 2 strands).
        ///  "7_[nValues)" is a level 2 krystal -- containing 7 strands (1 level 1 and 6 level 2 strands) and [nValues] values.
        ///  "7_28_[nValues]" is a level 3 krystal - containing 28 strands having level 1, 2, or 3, and [nValues] values.
        ///  "7_28_206_[nValues]" is a level 4 krystal - containing 206 strands having level 1, 2, 3 or 4, and [nValues] values. 
        /// </summary>
        /// <returns></returns>
        protected string GetNameRoot()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(MaxValue.ToString() + ".");

            if(Level == 0)
            {
                sb.Append("0");
            }
            else if(Level == 1)
            {
                sb.Append("1");
                sb.Append("_");
                sb.Append(Strands[0].Values.Count().ToString());
            }
            else
            {
                for(int i = 1; i < ShapeArray.Length; i++)
                {
                    sb.Append(ShapeArray[i].ToString() + '_');
                }
                sb.Remove(sb.Length - 1, 1);
            }

            sb.Append('.');

            return sb.ToString();
        }

        #region protected variables
        protected string _name = ""; // Used by status line: set ONLY by Save() -- i.e. when writing the newly created krystal's XML
        protected uint _level; // the maximum level of any strand in the krystal
        protected uint _minValue; // the minimum value in the krystal
        protected uint _maxValue; // the maximum value in the krystal
        protected uint _numValues; // the number of values in the krystal
        protected List<Strand> _strands = new List<Strand>();
        #endregion protected variables
    }
}

