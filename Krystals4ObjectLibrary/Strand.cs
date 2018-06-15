using System.Collections.Generic;
using System.Xml;

namespace Krystals4ObjectLibrary
{
    /// <summary>
    /// A strand is a krystal component, and is a list of unsigned integers with a level attribute.
    /// </summary>
	public class Strand
    {
        #region constructors
		public Strand() { } // for constructing an empty list of strands
        public Strand(XmlReader r)
        {
            // The reader is currently positioned at the start of a strand element.
            // r.Name is currently "s"
            r.MoveToFirstAttribute();
            _level = uint.Parse(r.Value);
            _values = K.GetUIntList(r.ReadString());
            // r.Name is the end tag "s" here
            K.ReadToXmlElementTag(r, "s", "strands");
            // r.Name is either the start tag "s" of a new strand
            // or the end tag "strands"
        }
        public Strand(uint level)
        {
            this._level = level;
        }
        public Strand(uint level, List<uint> values)
        {
            this._level = level;
            this._values = values;
		}
		#endregion constructors
		#region Properties
		public uint Level
        {
            get { return _level; }
            set { _level = value; }
        }
        public List<uint> Values
        {
            get { return _values; }
        }
        #endregion Properties
        #region private variables
        private uint _level;
        private readonly List<uint> _values = new List<uint>();
        #endregion private variables
    }
}

