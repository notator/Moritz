using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        /// Saves the krystal and any non-krystal associated files (e.g. expander or modulator).
        /// If overwrite is true, the file(s) are overwritten if they exist.
        /// </summary>
        /// <param name="overwrite"></param>
        public abstract void Save(bool overwrite);
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
		/// <summary>
		/// Finds an identical, already saved krystal
		/// </summary>
		/// <param name="nameIntro">one of "ck", "lk", "mk", "xk", "sk"</param>
		/// <returns></returns>
		protected string GetNameOfEquivalentSavedKrystal(string nameIntro)
        {
            Debug.Assert(_name == "" || _name == K.UntitledKrystalName);
            string newName = "";
            DirectoryInfo dir = new DirectoryInfo(K.KrystalsFolder);
            Krystal otherKrystal = null;
            foreach(FileInfo fileInfo in dir.GetFiles("*.krys"))
            {
                if(fileInfo.Name[0] == nameIntro[0])
                {
                    switch(fileInfo.Name[0])
                    {
                        case 'c':
                            otherKrystal = new ConstantKrystal(K.KrystalsFolder + @"\" + fileInfo.Name);
                            break;
                        case 'l':
                            otherKrystal = new LineKrystal(K.KrystalsFolder + @"\" + fileInfo.Name);
                            break;
                        case 'm':
                            otherKrystal = new ModulationKrystal(K.KrystalsFolder + @"\" + fileInfo.Name);
                            break;
                        case 'p':
                            otherKrystal = new PermutationKrystal(K.KrystalsFolder + @"\" + fileInfo.Name);
                            break;
                        case 's':
                            otherKrystal = new ShapedExpansionKrystal(K.KrystalsFolder + @"\" + fileInfo.Name);
                            break;
                        case 'x':
                            otherKrystal = new ExpansionKrystal(K.KrystalsFolder + @"\" + fileInfo.Name);
                            break;
                    }
                    if(this.CompareTo(otherKrystal) == 0)
                    {
                        newName = otherKrystal.Name;
                        break;
                    }
                }
            }
            return newName;
        }
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
        /// At level 1, length is always 1.
        /// At level krystal.Level, length is the total number of strands.
        /// At level krystal.Level + 1, length is the total number of strand values.
        /// An ArgumentOutOfRangeException is thrown if the argument is outside the range 1..krystal.Level + 1.
        /// </summary>
        /// <param name="level">Must be in range 1..krystal.Level+1</param>
        /// <returns>The number of elements at the given level.</returns>
        public int GetLength(uint level)
        {
            level--;
            if(level < 0 || level > _level)
                throw new ArgumentOutOfRangeException("Error in Krystal.GetLength().");
            int[] shapeArray = ShapeArray;
            return shapeArray[level];
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

        /// <summary>
        /// The total number of values in the returned list of int lists is equal to the number of values in
        /// the larger krystal. Each inner list corresponds to a value in this krystal, and contains that value
        /// repeated the appropriate number of times.
        /// Values in this krystal's strands are repeated according to the shape of the larger krystal.
        /// Restrictions:
        /// 1. the level of the largerKrystal must be greater than the level of this krystal.
        /// 2. the shape of this krystal must be contained in the shape of the larger krystal.
        /// </summary>
        /// <param name="largerKrystal"></param>
        /// <returns></returns>
        public List<List<int>> GetValuePerValue(Krystal largerKrystal)
        {
            Debug.Assert(largerKrystal.Shape.Contains(this.Shape));
            Debug.Assert(largerKrystal.Level > this.Level);

            List<List<int>> returnValue = new List<List<int>>();
            List<List<int>> largerValues = largerKrystal.GetValues(this.Level + 1);
            // each list in largerValues corresponds to a value in this krystal
            int thisStrandIndex = 0;
            int thisValueIndex = 0;
            uint thisValue = this.Strands[thisStrandIndex].Values[thisValueIndex];
            foreach(List<int> listInt in largerValues)
            {
                List<int> innerList = new List<int>();
                returnValue.Add(innerList);
                foreach(int i in listInt)
                    innerList.Add((int)thisValue);

                thisValueIndex++;
                if(thisValueIndex == this.Strands[thisStrandIndex].Values.Count)
                {
                    thisValueIndex = 0;
                    thisStrandIndex++;
                }
 
                if(thisStrandIndex < this.Strands.Count)
                    thisValue = this.Strands[thisStrandIndex].Values[thisValueIndex];
            }
            return returnValue;
        }
        private int[] ShapeArray
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
        #region protected variables
        protected string _name; // Used by status line: changed ONLY when writing a newly created krystal's XML
        protected uint _level; // the maximum level of any strand in the krystal
        protected uint _minValue; // the minimum value in the krystal
        protected uint _maxValue; // the maximum value in the krystal
        protected uint _numValues; // the number of values in the krystal
        protected List<Strand> _strands = new List<Strand>();
        #endregion protected variables
    }

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
        }
        #region overridden functions
        public override void Save(bool overwrite)
        {
            _name = String.Format("ck0({0}){1}", this.MaxValue, K.KrystalFilenameSuffix);
            if(File.Exists(K.KrystalsFolder + @"\" + _name) == false || overwrite)
            {
                XmlWriter w = base.BeginSaveKrystal(); // disposed of in EndSaveKrystal
                #region save heredity info (only that this is a constant...)
                w.WriteStartElement("constant");
                w.WriteEndElement(); // constant
                #endregion
                base.EndSaveKrystal(w); // saves the strands, closes the document, disposes of w
            }

            string msg = "Constant krystal saved as \r\n\r\n" +
                         "   " + _name;
            MessageBox.Show(msg, "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        public override void Rebuild()
        {
            // This function does nothing. Constant krystals are not dependent on other krystals!
            throw new Exception("The method or operation is deliberately not implemented.");
        }
        #endregion overridden functions
    }

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
        }
        #region overridden functions
        public override void Save(bool overwrite)
        {
            string pathname;
            bool alreadyExisted = true;
            if(_name != null && _name.Equals(K.UntitledKrystalName))
                _name = base.GetNameOfEquivalentSavedKrystal("lk");
            if(string.IsNullOrEmpty(_name))
            {
                alreadyExisted = false;
                int fileIndex = 1;
                do
                {
                    _name = String.Format("lk1({0})-{1}{2}",
                        _maxValue, fileIndex, K.KrystalFilenameSuffix);
                    pathname = K.KrystalsFolder + @"\" + _name;
                    fileIndex++;
                } while(File.Exists(pathname));
            }
            else
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
            StringBuilder msgSB = new StringBuilder("Line krystal:\r\n\r\n    ");
            foreach(uint value in this.Strands[0].Values)
            {
                msgSB.Append(value.ToString() + " ");
            }
            msgSB.Append("      \r\n\r\nsaved as:");
            msgSB.Append("\r\n\r\n    " + _name + "  ");
            if(alreadyExisted)
            {
                msgSB.Append("\r\n\r\n(This line krystal already existed.)");
            }
            MessageBox.Show(msgSB.ToString(), "Saved", MessageBoxButtons.OK, MessageBoxIcon.None);
        }
        public override void Rebuild()
        {
            // This function does nothing. Line krystals are not dependent on other krystals!
            throw new Exception("The method or operation is deliberately not implemented.");
        }
        #endregion overridden functions
    }

    /// <summary>
    /// Krystals used as inputs to expansions, modulations etc. have this class.
    /// It contains strands, and functions for dealing with them, but no information
    /// about the krystal's constructor.
    /// </summary>
    public abstract class InputKrystal : Krystal
    {
        public InputKrystal(string filepath)
            : base(filepath)
        {
            GetAbsoluteValues();
            GetMissingAbsoluteValues();
        }
        #region Enumerable Properties
        // In order to use IEnumerable, include a "using System.Collections" statement. 
        /// <summary>
        /// Each value in this krystal is returned with its level.
        /// The first value in each strand has the strand's level. All other values have this krystal's level + 1.
        /// </summary>
        public IEnumerable<LeveledValue> LeveledValues
        {
            get
            {
                LeveledValue leveledValue;
                int valueLevel = (int)this._level + 1;
                foreach(Strand strand in _strands)
                {
                    leveledValue.level = (int)strand.Level;
                    leveledValue.value = (int)strand.Values[0];
                    yield return leveledValue;
                    if(strand.Values.Count > 1)
                        for(int index = 1; index < strand.Values.Count; index++)
                        {
                            leveledValue.level = valueLevel;
                            leveledValue.value = (int)strand.Values[index];
                            yield return leveledValue;
                        }
                }
            }
        }
        #endregion Enumerable Properties
        #region public functions
        /// <summary>
        /// Dummy function. This should never be called.
        /// </summary>
        public override void Save(bool overwrite)
        {
            throw new ApplicationException("Input krystals cannot be saved (they have already been saved!)");
        }
        /// <summary>
        /// Dummy function. This should never be called.
        /// </summary>
        public override void Rebuild()
        {
            throw new Exception("Input krystals are never regenerated (they have been regenerated already!).");
        }
        /// <summary>
        /// Returns an array containing one value from this krystal per value in the master krystal.
        /// The master krystal may have the same or more levels than this krystal, but must have the same form
        /// as this krystal at this krystal's level.
        /// The master krystal may not have less levels than this krystal. If it has MORE levels, values
        /// from this krystal are repeated.
        /// </summary>
        /// <param name="masterKrystal"></param>
        public int[] AlignedValues(InputKrystal masterKrystal)
        {
            List<int> alignedValues = new List<int>();
            int mValueIndex = 0;
            int mStrandIndex = 0;
            uint masterLevel = 1;
            int strandIndex = 0;
            int valueIndex = 0;
            while(strandIndex < _strands.Count)
            {
                uint valueLevel = _strands[strandIndex].Level;
                valueIndex = 0;

                while(valueIndex < _strands[strandIndex].Values.Count
                    && mStrandIndex < masterKrystal.Strands.Count
                    && mValueIndex < masterKrystal.Strands[mStrandIndex].Values.Count)
                {
                    alignedValues.Add((int)_strands[strandIndex].Values[valueIndex]);

                    mValueIndex++;
                    if(mValueIndex == masterKrystal.Strands[mStrandIndex].Values.Count)
                    {
                        mValueIndex = 0;
                        mStrandIndex++;
                        if(mStrandIndex < masterKrystal.Strands.Count)
                            masterLevel = masterKrystal.Strands[mStrandIndex].Level;
                    }
                    else masterLevel = masterKrystal.Level + 1;

                    valueLevel = this._level + 1;

                    if(valueLevel == masterLevel || (mValueIndex == 0 && valueLevel > masterLevel))
                        valueIndex++;
                }
                strandIndex++;
            }
            return alignedValues.ToArray();
        }
        #endregion public functions
        #region properties
        public List<int> AbsoluteValues { get { return _absoluteValues; } }
        public List<int> MissingAbsoluteValues { get { return _missingAbsoluteValues; } }
        #endregion properties
        #region private functions
        /// <summary>
        /// Sets the AbsoluteValues property for this krystal. The absolute values are a list of the values
        /// which occur in the strands of this krystal (each value once).
        /// </summary>
        private void GetAbsoluteValues()
        {
            _absoluteValues = new List<int>();
            if(_maxValue == _minValue)
                _absoluteValues.Add((int)_minValue);
            else
            {
                for(int i = (int)_minValue; i <= (int)_maxValue; i++)
                {
                    bool found = false;
                    foreach(Strand s in _strands)
                    {
                        foreach(uint value in s.Values)
                        {
                            if(i == (int)value)
                            {
                                found = true;
                                break;
                            }
                        }
                        if(found == true)
                            break;
                    }
                    if(found)
                        _absoluteValues.Add(i);
                }
            }
        }
        /// <summary>
        /// Sets the MissingAbsoluteValues property for this krystal.
        /// The actual values in a krystal need not be the complete set of values between 1 and MaxValue.
        /// The missing values are the values between 1 and MaxValue which do not occur
        /// in this krystal's strands.
        /// </summary>
        private void GetMissingAbsoluteValues()
        {
            _missingAbsoluteValues.Clear();
            for(int i = 1; i < (int)MaxValue; i++)
                if(!_absoluteValues.Contains(i))
                    _missingAbsoluteValues.Add(i);
        }
        #endregion private functions
        #region private variables
        private List<int> _absoluteValues = new List<int>(); // set for the input values krystal of an expansion
        private List<int> _missingAbsoluteValues = new List<int>(); // set for the input values krystal of an expansion
        #endregion private variables
    }
}

