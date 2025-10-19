using System;
using System.Collections.Generic;
using System.Linq;

namespace TravelingSalesmanProblem
{
    public class HillClimbingTsp
    {
        private readonly double[,] _distanceMatrix;
        private readonly int _numCities;
        private readonly Random _random;

        public HillClimbingTsp(double[,] distanceMatrix)
        {
            _distanceMatrix = distanceMatrix;
            _numCities = distanceMatrix.GetLength(0);
            _random = new Random();
        }

        /// <summary>
        /// Calculates the total travel cost for a given route.
        /// </summary>
        /// <param name="route">The sequence of cities (0-indexed).</param>
        /// <returns>The total distance of the tour, including the return to the start city.</returns>
        private double CalculateCost(List<int> route)
        {
            double cost = 0;
            // Iterate through all segments of the route
            for (int i = 0; i < _numCities; i++)
            {
                int currentCity = route[i];
                // Use modulo operator to wrap around for the final segment (back to start)
                int nextCity = route[(i + 1) % _numCities];

                // Add the distance between the current city and the next city
                cost += _distanceMatrix[currentCity, nextCity];
            }
            return cost;
        }

        /// <summary>
        /// Generates a random initial solution (route) for the TSP.
        /// </summary>
        public TSP GenerateInitialRoute()
        {
            List<int> cities = Enumerable.Range(0, _numCities).ToList();

            // Randomizing city list using the Fisher-Yates shuffle
            for (int i = 0; i < _numCities; i++)
            {
                int j = _random.Next(i, _numCities);
                // Swap cities[i] and cities[j]
                (cities[i], cities[j]) = (cities[j], cities[i]);
            }

            double cost = CalculateCost(cities);
            return new TSP(cities, cost);
        }

        // --- Mutation Operators to Generate Neighbor Solutions ---

        /// <summary>
        /// Applies the Swap mutation operator: swaps two randomly chosen cities in the route.
        /// </summary>
        public TSP ApplySwap(TSP currentRoute)
        {
            var newRouteCities = currentRoute.Cities.ToList();

            // Randomly select two distinct city indices
            int i = _random.Next(_numCities);
            int j = _random.Next(_numCities);
            while (i == j)
            {
                j = _random.Next(_numCities);
            }

            // Perform the city swap
            (newRouteCities[i], newRouteCities[j]) = (newRouteCities[j], newRouteCities[i]);

            double newCost = CalculateCost(newRouteCities);
            return new TSP(newRouteCities, newCost);
        }

        /// <summary>
        /// Applies the Insert mutation operator: moves one randomly chosen city to a random new position.
        /// </summary>
        public TSP ApplyInsert(TSP currentRoute)
        {
            var newRouteCities = currentRoute.Cities.ToList();

            // Randomly select the source and destination indices
            int sourceIndex = _random.Next(_numCities);
            int destIndex = _random.Next(_numCities);

            // If indices are identical, return a clone of the original route (no change)
            if (sourceIndex == destIndex)
                return currentRoute.Clone();

            int cityToMove = newRouteCities[sourceIndex];
            newRouteCities.RemoveAt(sourceIndex); // Remove from the source position
            newRouteCities.Insert(destIndex, cityToMove); // Insert at the new position

            double newCost = CalculateCost(newRouteCities);
            return new TSP(newRouteCities, newCost);
        }

        /// <summary>
        /// Applies the Reverse (2-opt) operator: reverses the order of a sub-segment of the route.
        /// </summary>
        public TSP ApplyReverse(TSP currentRoute)
        {
            var newRouteCities = currentRoute.Cities.ToList();
            int numCities = newRouteCities.Count;

            // Select two distinct random indices
            int i = _random.Next(numCities);
            int j = _random.Next(numCities);

            while (i == j)
            {
                j = _random.Next(numCities);
            }

            // Ensure i is the smaller index (start of the segment)
            if (i > j)
            {
                (i, j) = (j, i); // Swap i and j
            }

            // Reverse the segment of the list starting from index 'i' 
            // with a length of 'j - i + 1' (to include element 'j')
            newRouteCities.Reverse(i, j - i + 1);

            double newCost = CalculateCost(newRouteCities);
            return new TSP(newRouteCities, newCost);
        }

        // --- Main Hill Climbing Algorithm ---

        /// <summary>
        /// Solves the TSP using the Hill Climbing heuristic.
        /// </summary>
        /// <param name="mutationOperator">A function (delegate) to generate a neighbor solution.</param>
        /// <param name="maxIterations">Maximum number of iterations allowed.</param>
        /// <param name="maxStagnation">Maximum iterations without finding a better solution.</param>
        public TSP SolveTsp(Func<TSP, TSP> mutationOperator, int maxIterations = 1000, int maxStagnation = 100)
        {
            // Start with a randomly generated initial route
            TSP currentBestRoute = GenerateInitialRoute();

            // Get the operator's name for logging purposes
            string methodName = mutationOperator.Method.Name.Replace("Apply", "");

            Console.WriteLine($"Starting Hill Climbing with {methodName}. Initial Cost: {currentBestRoute.Cost:F2}");

            int iteration = 0;
            int stagnationCounter = 0;

            while (iteration < maxIterations && stagnationCounter < maxStagnation)
            {
                // Generate a neighbor solution using the provided mutation operator
                TSP neighborRoute = mutationOperator(currentBestRoute);

                // Hill Climbing logic: accept the neighbor ONLY if it is a strictly better solution
                if (neighborRoute.Cost < currentBestRoute.Cost)
                {
                    currentBestRoute = neighborRoute;
                    stagnationCounter = 0; // Reset stagnation counter on improvement
                }
                else
                {
                    stagnationCounter++; // Increment if no improvement
                }

                iteration++;
            }

            Console.WriteLine($"Hill Climbing with {methodName} finished after {iteration} iterations.");
            return currentBestRoute;
        }
    }
}