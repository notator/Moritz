using System.Collections.Generic;
using System.Xml;

namespace Krystals5ObjectLibrary
{
    /// <summary>
    /// A strand is a krystal component, and is a list of unsigned integers with a level attribute.
    /// </summary>
    ///[DebuggerDisplay("{ToString()}")] // doesnt help
    public class Strand
    {
        #region constructors
        //public Strand() { } // for constructing an empty list of strands
        public Strand(XmlReader r)
        {
            // The reader is currently positioned at the start of a strand element.
            // r.Name is currently "s"
            r.MoveToFirstAttribute();
            Level = uint.Parse(r.Value);
            Values = K.GetUIntList(r.ReadString());
            // r.Name is the end tag "s" here
            K.ReadToXmlElementTag(r, "s", "strands");
            // r.Name is either the start tag "s" of a new strand
            // or the end tag "strands"
        }
        public Strand(uint level)
        {
            Level = level;
            Values = new List<uint>();
        }
        public Strand(uint level, List<uint> values)
        {
            Level = level;
            Values = values;
        }
        #endregion constructors
        #region Properties
        public override string ToString()
        {
            // Thread.Sleep(1000); // Vary this to see different results -- (doesnt help either way)

            return "Level=" + Level.ToString() + " nValues=" + Values.Count.ToString();

            //return "Level=" + Level.ToString();
        }

        public uint Level { get; set; }
        public List<uint> Values { get; }

        #endregion Properties
    }
}

