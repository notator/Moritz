using Moritz.Globals;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
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
    }
    
    public class PathKrystal : PathKrystalBase
    {
        public readonly string SVGInputFilename;
        public string DensityInputKrystalName; // to be made readonly when the krystalNames have all been changed
        private readonly Field _field;
        private readonly Trajectory _trajectory;

        /// <summary>
        /// constructor for creating a path krystal from an (SVG) XmlDocument and a krystal
        /// </summary>
        public PathKrystal(string svgFilepath, string densityInputKrystalFilePath)
            : base()
        {
            SVGInputFilename = Path.GetFileName(svgFilepath);
            DensityInputKrystalName = Path.GetFileName(densityInputKrystalFilePath);

            char[] splitChar = { '.' };
            var svgInputFilenameComponents = SVGInputFilename.Split(splitChar);
            
            Debug.Assert(svgInputFilenameComponents[3] == "path" && svgInputFilenameComponents[4] == "svg" && DensityInputKrystalName.EndsWith(".krys"));

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

        public PathKrystal(string filepath) : base(filepath)
        {
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
        /// Sets the krystal's Name, and saves it (but not any of its ancestor files).
        /// If a krystal having identical content exists in the krystals directory,
        /// the user is given the option to
        ///    either overwrite the existing krystal (using that krystal's index),
        ///    or abort the save.
        /// This means that a given set of ancestors should always have the same index.
        /// </summary>
        public override void Save()
        {
            bool PathKrystalIsUnique(out string name)
            {
                var isUnique = true;
                name = GetName(K.KrystalType.mod); // default name (with an index that is not used in the krystals folder)

                var pathKrystalPaths = Directory.EnumerateFiles(M.LocalMoritzKrystalsFolder, "*.path.krys");
                foreach(var existingPath in pathKrystalPaths)
                {
                    var existingKrystal = new PathKrystal(existingPath);
                    if( existingKrystal.DensityInputKrystalName == this.DensityInputKrystalName
                    && existingKrystal.SVGInputFilename == this.SVGInputFilename)
                    {
                        isUnique = false;
                        name = Path.GetFileName(existingPath);
                        break;
                    }
                }
                return isUnique;
            }

            DialogResult answer = DialogResult.Yes;
            if(PathKrystalIsUnique(out _name) == false)
            {
                string msg = $"PathKrystal {_name} already existed. Save it again with a new date?";
                answer = MessageBox.Show(msg, "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            }

            if(answer == DialogResult.Yes)
            {
                XmlWriter w = base.BeginSaveKrystal(); // disposed of in EndSaveKrystal
                #region save heredity info
                w.WriteStartElement("path");
                w.WriteAttributeString("svg", this.SVGInputFilename);
                w.WriteAttributeString("density", this.DensityInputKrystalName);
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

            this.Save();
        }
    }
}