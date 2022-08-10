using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;

namespace Krystals4ObjectLibrary
{
    public class Expansion
    {
        /// <summary>
        /// This constructor creates the strands from the strandNodeList and the expander.
        /// The expander has been checked to see that it satisfies the following conditions:
        ///     1. each gamete contains at least 1 point.
        ///     2. the smallest point value in each gamete is 1.
        ///     3. there are no duplicate values in a gamete.
        ///     4. the values in a gamete are contiguous (no gaps between the values).
        /// </summary>
        /// <param name="strandNodeList"></param>
        /// <param name="field"></param>
        public Expansion(List<StrandNode> strandNodeList, Expander expander)
        {
            Expand(strandNodeList, expander);
        }
        /// <summary>
        /// This constructor creates strands from the complete, checked inputs of its ExpansionKrystal argument.
        /// (No further checking is done.)
        /// </summary>
        /// <param name="ek">An expansion krystal with complete, checked inputs</param>
        public Expansion(ExpansionKrystal ek)
        {

            try
            {
                Expander expander = ek.Expander;
                expander.CalculateAbstractPointPositions(ek.DensityInputKrystal);
                List<StrandNode> strandNodeList = ek.StrandNodeList();
                Expand(strandNodeList, expander);

            }
            catch(ApplicationException ex)
            {
                throw ex;
            }
        }

        private void Expand(List<StrandNode> strandNodeList, Expander expander)
        {
            #region preparation
            List<PointGroup> fixedInputPointGroups = expander.InputGamete.FixedPointGroups;
            List<PointGroup> fixedOutputPointGroups = expander.OutputGamete.FixedPointGroups;
            List<Planet> inputPlanets = expander.InputGamete.Planets;
            List<Planet> outputPlanets = expander.OutputGamete.Planets;

            List<TrammelMark> trammel = new List<TrammelMark>();
            foreach(int value in expander.OutputGamete.Values)
            {
                TrammelMark tm = new TrammelMark(value);
                trammel.Add(tm);
            }

            uint[,] distances = new uint[expander.InputGamete.NumberOfValues, expander.OutputGamete.NumberOfValues];

            CalculateFixedDistances(distances, fixedInputPointGroups, fixedOutputPointGroups);

            #endregion preparation
            #region expand strands
            int momentIndex = 0;
            foreach(StrandNode strandNode in strandNodeList)
            {
                int level = strandNode.strandLevel;
                int density = strandNode.strandDensity;
                int iPointValue = strandNode.strandPoint;
                int iPointIndex = iPointValue - 1; // the input point's index in the distances array

                #region calculate distances for planets where necessary.
                PointF? iPointPosition = null;
                if(inputPlanets.Count > 0)
                    iPointPosition = InputPlanetPosition(momentIndex, iPointValue, inputPlanets);

                // iPointPosition == null if the current input point is not a planet
                // If the current input point is a planet, calculate distances to the fixed output points.
                if(iPointPosition != null && fixedOutputPointGroups.Count > 0)
                    CalculateInputPlanetDistances(distances, iPointIndex, (PointF) iPointPosition, fixedOutputPointGroups);

                if(outputPlanets.Count > 0)
                {   // If there are output planets, it is necessary to calculate their distances from the input point
                    if(iPointPosition == null) // the input point is not a planet, so its a fixed point
                        iPointPosition = FixedInputPointPosition(iPointValue, fixedInputPointGroups);
                    if(iPointPosition == null)
                        throw new ApplicationException("Error finding the position of an input point.");
                    // There are output planets, so calculate their distances from the (planet or fixed)input point.
                    CalculateOutputPlanetsDistances(momentIndex, distances, iPointIndex, (PointF) iPointPosition, outputPlanets);
                }
                #endregion calculate distances for planets where necessary

                foreach(TrammelMark tm in trammel)
                    tm.Distance = distances[iPointIndex, tm.DistancesIndex];

                Strand strand = ExpandStrand(level, density, trammel);
                _strands.Add(strand);

                momentIndex++;
            }
            #endregion expand strands
        }

        private void ContourStrands(List<StrandNode> contouredStrandNodeList, List<Strand> strands)
        {
            Debug.Assert(contouredStrandNodeList.Count ==  strands.Count);

            List<uint> tempList = new List<uint>();
            int[] contour;
            for(int strandIndex = 0 ; strandIndex < _strands.Count ; strandIndex++)
            {
				if(contouredStrandNodeList[strandIndex] is ContouredStrandNode contouredStrandNode
					&& contouredStrandNode.strandDensity > 1
					&& contouredStrandNode.strandDensity <= 7)
				{
					int density = contouredStrandNode.strandDensity;
					contour = K.Contour(density, contouredStrandNode.strandContour, contouredStrandNode.strandAxis);
					_strands[strandIndex].Values.Sort();
					tempList.Clear();
					for(int j = 0; j < density; j++)
					{
						tempList.Add(_strands[strandIndex].Values[contour[j] - 1]);
					}
					for(int j = 0; j < density; j++)
					{
						_strands[strandIndex].Values[j] = tempList[j];
					}
				}
			}
        }

        #region public properties
        public List<Strand> Strands
        {
            get { return _strands; }
        }
        #endregion public Properties
        #region private functions
        /// <summary>
        /// This function is called once when beginning to expand the field. It calculates the distances between
        /// the fixed input and fixed output points, putting the result in the 'distances' array.
        /// </summary>
        /// <param name="distances">A two dimensional buffer which will contain the (squared) distances between the points.</param>
        /// <param name="inputPGs">The list of fixed input point groups</param>
        /// <param name="outputPGs">The list of fixed output point groups</param>
        private void CalculateFixedDistances(uint[,] distances, List<PointGroup> inputPGs, List<PointGroup> outputPGs)
        {
            foreach(PointGroup ipg in inputPGs)
                for(int iindex = 0 ; iindex < ipg.Count ; iindex++)
                {
                    int iPointValue = (int) ipg.Value[iindex];
                    int iPointIndex = iPointValue - 1;
                    PointF iPointPosition = ipg.WindowsPixelCoordinates[iindex];
                    foreach(PointGroup opg in outputPGs)
                        for(int oindex = 0 ; oindex < opg.Count ; oindex++)
                        {
                            int oPointValue = (int) opg.Value[oindex];
                            int oPointIndex = oPointValue - 1;
                            PointF oPointPosition = opg.WindowsPixelCoordinates[oindex];
                            distances[iPointIndex, oPointIndex] = SquaredDistance(iPointPosition, oPointPosition);
                        }
                }
        }
        /// <summary>
        /// Returns the position of the planet with the given value if found, or null if there is no such planet.
        /// </summary>
        /// <param name="momentIndex">The 0-based moment index</param>
        /// <param name="planetValue">The value of the planet whose position should be returned.</param>
        /// <param name="inputPlanets">the list of input planets</param>
        /// <returns></returns>
        /// <summary>
        /// Returns the position of the planet with the given value if found, or null if there is no such planet.
        /// </summary>
        /// <param name="momentIndex">The 0-based moment index</param>
        /// <param name="planetValue">The value of the planet whose position should be returned.</param>
        /// <param name="inputPlanets">the list of input planets</param>
        /// <returns></returns>
        private PointF? InputPlanetPosition(int momentIndex, int planetValue, List<Planet> inputPlanets)
        {
            Planet planet = null;
            PointF? position = null;
            foreach(Planet p in inputPlanets)
            {
                if(p.Value == planetValue)
                {
                    planet = p;
                    break;
                }
            }
            if(planet != null)
            {
                //int indexInSubpath = momentIndex;
                foreach(PointGroup pg in planet.Subpaths)
                {
                    if(momentIndex >= pg.Count)
                        momentIndex -= (int) pg.Count;
                    else
                    {
                        position = pg.WindowsPixelCoordinates[momentIndex];
                        break;
                    }
                }
            }

            ///////////////////////////////////////
            //{
            //    //int indexInSubpath = momentIndex;
            //    for (int i = 0; i < planet.Subpaths.Count; i++)
            //    {
            //        if (momentIndex >= planet.Subpaths[i].Count)
            //            momentIndex -= (int)planet.Subpaths[i].Count;
            //        else
            //        {
            //            position = planet.Subpaths[i].WindowsPixelCoordinates[momentIndex];
            //            break;
            //        }
            //    }
            //}
            /////////////////////////////////////
            //{
            //    int indexInSubpath = momentIndex;
            //    for (int i = 0; i < planet.Subpaths.Count; i++)
            //    {
            //        if (indexInSubpath >= planet.Subpaths[i].Count)
            //            indexInSubpath -= (int)planet.Subpaths[i].Count;
            //        else
            //        {
            //            position = planet.Subpaths[i].WindowsPixelCoordinates[indexInSubpath];
            //            break;
            //        }
            //    }
            //}
            return position;
        }
        /// <summary>
        /// This function is called if the input value is that of a planet, when beginning to expand each strand.
        /// It calculates the distances between the input planet and the fixed output points, putting the result
        /// in the 'distances' array.
        /// </summary>
        /// <param name="distances">A two dimensional buffer which will contain the (squared) distances between the points.</param>
        /// <param name="iPlanetPosition">The current position of the input planet</param>
        /// <param name="iPlanetValue">The value of the input planet</param>
        /// <param name="fixedOutputPointGroups">The list of fixed output point groups</param>
        private void CalculateInputPlanetDistances(uint[,] distances,
                                                    int iPlanetIndex, PointF iPlanetPosition,
                                                    List<PointGroup> fixedOutputPointGroups)
        {
            foreach(PointGroup opg in fixedOutputPointGroups)
                for(int oindex = 0 ; oindex < opg.Count ; oindex++)
                {
                    int oPointIndex = (int) opg.Value[oindex] - 1;
                    PointF oPointPosition = opg.WindowsPixelCoordinates[oindex];
                    distances[iPlanetIndex, oPointIndex]
                        = SquaredDistance(iPlanetPosition, oPointPosition);
                }
        }
        /// <summary>
        /// Called when beginning to expand a strand if the input point is fixed and there are output planets.
        /// </summary>
        /// <param name="iPointValue">The value of the point whose position is required.</param>
        /// <param name="fixedInputPointGroups">The list of fixed input point groups</param>
        /// <returns>The position of the fixed point with the given value.</returns>
        private PointF? FixedInputPointPosition(int iPointValue, List<PointGroup> fixedInputPointGroups)
        {
            PointF? rPointF = null;
            bool found = false;
            foreach(PointGroup pg in fixedInputPointGroups)
            {
                for(int i = 0 ; i < pg.Count ; i++)
                {
                    if(iPointValue == pg.Value[i])
                    {
                        rPointF = pg.WindowsPixelCoordinates[i];
                        found = true;
                        break;
                    }
                }
                if(found) break;
            }
            return (rPointF);
        }
        /// <summary>
        /// This function is called if there are any output planets, when beginning to expand each strand.
        /// It calculates the distances between the input point having value 'pointValue' (the point may be
        /// either a fixed point or a planet) and the output planets.
        /// </summary>
        /// <param name="momentIndex">The current moment index (base 0)</param>
        /// <param name="distances">A two dimensional buffer which will contain the (squared) distances between the points.</param>
        /// <param name="pointValue">The value of the input point</param>
        /// <param name="outputPlanets">The list of output planets</param>
        private void CalculateOutputPlanetsDistances(int momentIndex, uint[,] distances,
                                                    int iPointIndex, PointF iPointPosition,
                                                    List<Planet> outputPlanets)
        {
            PointF? oPosition = null;
            foreach(Planet oPlanet in outputPlanets)
            {
                foreach(PointGroup opg in oPlanet.Subpaths)
                {
                    if(momentIndex >= opg.WindowsPixelCoordinates.Length)
                        momentIndex -= opg.WindowsPixelCoordinates.Length;
                    else
                    {
                        oPosition = opg.WindowsPixelCoordinates[momentIndex];
                        break;
                    }
                }

                if(oPosition == null)
                    throw new ApplicationException("Error finding the position of an output planet.");

                distances[iPointIndex, (int) oPlanet.Value - 1]
                        = SquaredDistance(iPointPosition, (PointF) oPosition);
            }
        }
        /// <summary>
        /// Returns the distance between the two points, to the power of two, as an unsigned integer.
        /// The distances are rounded (down and up) to the nearest integral value.
        /// </summary>
        /// <param name="inP">the input ('expansion') point</param>
        /// <param name="outP">the output point (or 'focus')</param>
        /// <returns></returns>
        private uint SquaredDistance(PointF inP, PointF outP)
        {
            float x = inP.X - outP.X;
            float y = inP.Y - outP.Y;
            float result = ((x * x) + (y * y));
            uint uintResult = (uint) result;
            float diff = result - uintResult;
            if(diff >= 0.5f)
                uintResult++;
            return uintResult;
        }
        public static Strand ExpandStrand(int level, int density, List<TrammelMark> trammel)
        {
            Strand strand = new Strand((uint) level);
            SortedList<uint, TrammelMark> expansion = new SortedList<uint, TrammelMark>();

            // First add each trammel mark's distance to its current position...
            for(int i = 0 ; i < trammel.Count ; i++)
            {
                trammel[i].Position += trammel[i].Distance;
                uint positionKey = trammel[i].PositionKey;
                while(expansion.ContainsKey(positionKey))
                    positionKey += 1; // resolves positions which would otherwise be identical
                expansion.Add(positionKey, trammel[i]);
            }

            // Now construct the strand.
            for(int i = 0 ; i < density ; i++)
            {
                TrammelMark tm = expansion.Values[0];
                strand.Values.Add((uint) tm.Value);
                expansion.RemoveAt(0);
                tm.Position += tm.Distance;
                uint positionKey = tm.PositionKey;
                while(expansion.ContainsKey(positionKey))
                    positionKey += 1; // resolves positions which would otherwise be identical
                expansion.Add(positionKey, tm);
            }

            // The strand is complete.
            // Now reset the trammel so that the smallest trammel mark position is 0.
            // First, remove unused distances...
            expansion.Clear();
            for(int i = 0 ; i < trammel.Count ; i++)
            {
                trammel[i].Position -= trammel[i].Distance;
                uint positionKey = trammel[i].PositionKey;
                while(expansion.ContainsKey(positionKey))
                    positionKey += 1; // resolves positions which would otherwise be identical
                expansion.Add(positionKey, trammel[i]);
            }
            // Second, subtract the first trammel mark position from all trammel marks.
            uint minPos = expansion.Values[0].Position;
            foreach(TrammelMark tm in trammel)
                tm.Position -= minPos;

            return strand;
        }
        #endregion private functions
        #region private variables
        //private Dictionary<int, int> _inputIndexDict = new Dictionary<int, int>();
        //private Dictionary<int, int> _outputIndexDict = new Dictionary<int, int>();
        private List<Strand> _strands = new List<Strand>();
        #endregion private variables
    }
}
