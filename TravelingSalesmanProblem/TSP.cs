using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelingSalesmanProblem
{
    public class TSP
    {
        public List<int> Cities { get; set; } // Cities in the route
        public double Cost { get; set; }      // Cost of the route

        public TSP(List<int> cities, double cost)
        {
            Cities = cities;
            Cost = cost;
        }

        public TSP Clone()
        {
            return new TSP(new List<int>(Cities), Cost);
        }

        public override string ToString()
        {
            return $"Route: {string.Join(" -> ", Cities)} (Cost: {Cost:F2})";
        }

    }
}
