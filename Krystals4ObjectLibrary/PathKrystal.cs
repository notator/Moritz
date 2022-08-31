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
    public class PathKrystal : Krystal
    {
        public readonly string SVGInputFilename;
        public string DensityInputKrystalName; // to be made readonly when the krystalNames have all been changed
        public readonly XmlDocument SvgDoc;
        public readonly InputKrystal DensityInputKrystal;
        private readonly Field _field;
        private readonly Trajectory _trajectory;

        /// <summary>
        /// Constructor for loading a complete path krystal from a file.
        /// This constructor reads the heredity info, and constructs the corresponding objects.
        /// The Krystal base class reads the strands.
        /// </summary>
        /// <param name="filepath"></param>
        public PathKrystal(string filepath, bool onlyLoadHeredityInfo = false)
            : base(filepath)
        {
            using(XmlReader r = XmlReader.Create(filepath))
            {
                K.ReadToXmlElementTag(r, "path"); // check that this is a path (the other checks have been done in base()
                for(int attr = 0; attr < r.AttributeCount; attr++)
                {
                    r.MoveToAttribute(attr);
                    switch(r.Name)
                    {
                        case "svg":
                            this.SVGInputFilename = r.Value;
                            break;
                        case "density":
                            this.DensityInputKrystalName = r.Value;
                            break;
                    }
                }
            }

            if(onlyLoadHeredityInfo == false)
            {
                string svgInputFilepath = K.KrystalsSVGFolder + @"\" + SVGInputFilename;
                string densityInputFilepath = K.KrystalsFolder + @"\" + DensityInputKrystalName;

                DensityInputKrystal = new DensityInputKrystal(densityInputFilepath);
                SvgDoc = new XmlDocument();
                SvgDoc.PreserveWhitespace = true;
                SvgDoc.Load(svgInputFilepath);
            }
        }
        /// <summary>
        /// constructor for creating a path krystal from an (SVG) XmlDocument and a krystal
        /// This constructor generates a unique name for the krystal.
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

            SvgDoc = new XmlDocument();
            SvgDoc.PreserveWhitespace = true;
            SvgDoc.Load(svgFilepath);

            XmlElement fieldPathElem = M.GetElementById(SvgDoc, "path", "field");

            _field = new Field(fieldPathElem);

            XmlElement trajectoryPathElement = M.GetElementById(SvgDoc, "path", "trajectory");
            DensityInputKrystal densityInputKrystal = new DensityInputKrystal(densityInputKrystalFilePath);

            _trajectory = new Trajectory(trajectoryPathElement, nEffectiveTrajectoryNodes, densityInputKrystal);

            List<List<uint>> expansionDistances = GetExpansionDistances(_field.Foci, _trajectory.StrandsInput);

            Strands = ExpandStrands(_trajectory.StrandsInput, _field.Values, expansionDistances);

            Level = (uint) _trajectory.Level;

            Name = GetUniqueName(K.KrystalType.path);
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

            NumValues = 0;
            MaxValue = uint.MinValue;
            MinValue = uint.MaxValue;

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

                NumValues += (uint) density;

                foreach(uint value in strand.Values)
                {
                    MaxValue = (MaxValue > value) ? MaxValue : value;
                    MinValue = (MinValue < value) ? MinValue : value;
                } 

                rval.Add(strand);
            }

            return rval;
        }

        /// <summary>
        /// If a krystal having identical content exists in the krystals directory,
        /// the user is given the option to
        ///    either overwrite the existing krystal (using that krystal's index),
        ///    or abort the save.
        /// This means that a given set of ancestors should always have the same index.
        /// Returns true if the file has been saved, otherwise false.
        /// </summary>
        public override bool Save()
        {
            var hasBeenSaved = false;
            var pathname = K.KrystalsFolder + @"\" + Name;
            DialogResult answer = DialogResult.Yes;
            if(File.Exists(pathname))
            {
                string msg = $"PathKrystal {Name} already exists.\nSave it again with a new date?";
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
                MessageBox.Show($"{Name} saved.", "Saved", MessageBoxButtons.OK);
                hasBeenSaved = true;
            }
            else
            {
                MessageBox.Show($"{Name} not saved.", "Save Aborted", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return hasBeenSaved;
        }
        /// <summary>
        /// Re-expands this krystal (using the existing input data), then saves it,
        /// overwriting the existing file.
        /// All the krystals in the krystals folder are rebuilt, when one of them has been changed.
        /// </summary>
        public override void Rebuild()
        {
            List<List<uint>> expansionDistances = GetExpansionDistances(_field.Foci, _trajectory.StrandsInput);

            Strands = ExpandStrands(_trajectory.StrandsInput, _field.Values, expansionDistances);

            this.Save();
        }
    }
}