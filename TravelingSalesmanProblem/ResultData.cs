using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelingSalesmanProblem
{
    public class ResultData
    {
        public string Method { get; set; }
        public int? MaxIterations { get; set; }

        // Hill Climbing (HC)
        public int? MaxStagnation { get; set; }

        // Simulated Annealing (SA)
        public int? SolutionsPerTemperature { get; set; }
        public double? InitialTemperature { get; set; }
        public double? CoolingRate { get; set; }

        // Tabu Search (TS)
        public int? TabuListLength { get; set;}

        // Ant Colony Optimization (ACO)
        public int? NumAnts { get; set; }   // (M)
        public double? Alpha { get; set; }  // (α)
        public double? Beta { get; set; }   // (β)
        public double? Rho { get; set; }    // (ρ)

        // Scores
        public double? RouteCost { get; set; }
        public double? ExecutionTimeMs { get; set; }
    }
}
