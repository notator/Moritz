using Moritz.Globals;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
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
        private readonly string _svgInputFilename;
        private readonly string _densityInputKrystalName;
        private readonly Field _field;
        private readonly Trajectory _trajectory;

        /// <summary>
        /// constructor for creating a path krystal from an (SVG) XmlDocument and a krystal
        /// </summary>
        public PathKrystal(string svgFilepath, string densityInputKrystalFilePath)
            : base()
        {
            _svgInputFilename = Path.GetFileName(svgFilepath);
            _densityInputKrystalName = Path.GetFileName(densityInputKrystalFilePath);

            char[] splitChar = { '.' };
            var svgInputFilenameComponents = _svgInputFilename.Split(splitChar);
            
            Debug.Assert(svgInputFilenameComponents[3] == "path" && svgInputFilenameComponents[4] == "svg" && _densityInputKrystalName.EndsWith(".krys"));

            int nEffectiveTrajectoryNodes = int.Parse(svgInputFilenameComponents[1]); // can be 1 (A constant: the first node in the trajectory path)

            XmlDocument svgDoc = new XmlDocument();
            svgDoc.PreserveWhitespace = true;
            svgDoc.Load(svgFilepath);

            XmlElement fieldPathElem = M.GetElementById(svgDoc, "path", "field");

            _field = new Field(fieldPathElem);

            XmlElement trajectoryPathElement = M.GetElementById(svgDoc, "path", "trajectory");
            DensityInputKrystal densityInputKrystal = new DensityInputKrystal(densityInputKrystalFilePath);

            _trajectory = new Trajectory(trajectoryPathElement, nEffectiveTrajectoryNodes, densityInputKrystal);

            List<List<uint>> expansionDistances = GetExpansionDistances(_field.Foci, _trajectory.StrandsInput);

            _strands = ExpandStrands(_trajectory.StrandsInput, _field.Values, expansionDistances);

            _level = (uint) _trajectory.Level;

            _name = GetName(K.KrystalType.path);
        }

        /// <summary>
        /// The "distance" used in an expansion is actually the square of the distance between a trajectory point and a focus.
        /// Also, the distance is a uint, not a float: distance = (uint)(float * scale)
        /// </summary>
        /// <param name="focusPoints"></param>
        /// <param name="strandsInput"></param>
        /// <returns></returns>
        private List<List<uint>> GetExpansionDistances(List<PointF> focusPoints, List<StrandArgs> strandsInput)
        {
            var rval = new List<List<uint>>();
            const int scale = 1; // expansions are created by using large ints, not floats.

            foreach(StrandArgs strandArgs in strandsInput)
            {
                var trajectoryPoint = strandArgs.TrajectoryPoint;
                var distances = new List<uint>();
                for(int focusIndex = 0; focusIndex < focusPoints.Count; focusIndex++)
                {
                    var focusPoint = focusPoints[focusIndex];
                    double distance = Math.Pow((double)(focusPoint.X - trajectoryPoint.X), 2) + Math.Pow((double)(focusPoint.Y - trajectoryPoint.Y), 2);
                    distances.Add((uint) Math.Round((distance * scale)));
                }
                rval.Add(distances);
            }
            return rval;
        }
        private List<Strand> ExpandStrands(List<StrandArgs> strandsInput, List<string> focusValues, List<List<uint>> expansionDistances)
        {
            Debug.Assert(strandsInput.Count == expansionDistances.Count);

            _numValues = 0;
            _maxValue = uint.MinValue;
            _minValue = uint.MaxValue;

            List<TrammelMark> trammel = new List<TrammelMark>();

            for(int i = 0; i < focusValues.Count; i++)
            {
                TrammelMark tm = new TrammelMark(int.Parse(focusValues[i]));
                trammel.Add(tm);
            }

            var rval = new List<Strand>();

            for(int strandIndex = 0; strandIndex < strandsInput.Count; strandIndex++)
            {
                var strandArgs = strandsInput[strandIndex];
                var level = strandArgs.Level;
                var density = strandArgs.Density;
                var distances = expansionDistances[strandIndex];

                Debug.Assert(distances.Count == trammel.Count);
                for(int tmIndex = 0; tmIndex < trammel.Count; tmIndex++)
                {
                    trammel[tmIndex].Distance = distances[tmIndex];
                }

                Strand strand = Expansion.ExpandStrand(level, density, trammel);

                _numValues += (uint) density;

                foreach(uint value in strand.Values)
                {
                    _maxValue = (_maxValue > value) ? _maxValue : value;
                    _minValue = (_minValue < value) ? _minValue : value;
                } 

                rval.Add(strand);
            }

            return rval;
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
                w.WriteAttributeString("density", this._densityInputKrystalName);
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
            List<List<uint>> expansionDistances = GetExpansionDistances(_field.Foci, _trajectory.StrandsInput);

            _strands = ExpandStrands(_trajectory.StrandsInput, _field.Values, expansionDistances);

            this.Save(true);
        }
    }
}