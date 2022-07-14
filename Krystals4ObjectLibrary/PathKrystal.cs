using Moritz.Globals;

using System;
using System.Collections.Generic;
using System.Xml;

namespace Krystals4ObjectLibrary
{
    public abstract class PathKrystalBase : Krystal
    {
        public PathKrystalBase()
            : base()
        {
        }

        public PathKrystalBase(string filepath)
            : base(filepath)
        {
        }

        public override void Save(bool overwrite)
        {
            Save(overwrite);
        }
    }
    
    public class PathKrystal : PathKrystalBase
    {
        private string _svgInputFilename;
        private Field _field;
        private Trajectory _trajectory;

        /// <summary>
        /// constructor for loading a complete path krystal from a .krys file
        /// </summary>
        public PathKrystal(string filepath)
            : base(filepath)
        {

        }

        /// <summary>
        /// constructor for creating a path krystal from an (SVG) XmlDocument
        /// </summary>
        public PathKrystal(string svgInputFilename, XmlDocument svgDoc)
            : base()
        {
            this._svgInputFilename = svgInputFilename;

            XmlElement fieldPathElem = M.GetElementById(svgDoc, "path", "field");

            _field = new Field(fieldPathElem);

            XmlElement trajectoryPathElement = M.GetElementById(svgDoc, "path", "trajectory");

            _trajectory = new Trajectory(trajectoryPathElement);

            _strands = GetStrands(_field, _trajectory);
        }

        private List<Strand> GetStrands(Field field, Trajectory trajectory)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This version of Save() is used by PathKrystal.Rebuild() to overwrite krystals.
        /// </summary>
        public override void Save(bool overwriteKrystal)
        {
            if(overwriteKrystal)
            {
                XmlWriter w = base.BeginSaveKrystal(); // disposed of in EndSaveKrystal
                #region save heredity info
                w.WriteStartElement("path");
                w.WriteAttributeString("svg", this._svgInputFilename);
                w.WriteEndElement(); // path
                #endregion
                base.EndSaveKrystal(w); // saves the strands, closes the document, disposes of w
            }
        }
        /// <summary>
        /// Re-expands this krystal (using the existing input data), then saves it,
        /// overwriting the existing file.
        /// All the krystals in the krystals folder are rebuilt, when one of them has been changed.
        /// </summary>
        public override void Rebuild()
        {
            GetStrands(this._field, this._trajectory);
            this.Save(true);
        }
    }
}