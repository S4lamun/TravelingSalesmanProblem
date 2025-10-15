using System;
using System.Collections.Generic;
using System.Linq;

namespace TravelingSalesmanProblem
{
    public class HillClimbing
    {
        public List<int> Cities { get; set; } // Cities in the route
        public double Cost { get; set; }      // Cost of the route

        public HillClimbing(List<int> cities, double cost)
        {
            Cities = cities;
            Cost = cost;
        }

        public HillClimbing Clone()
        {
            return new HillClimbing(new List<int>(Cities), Cost);
        }

        public override string ToString()
        {
            return $"Route: {string.Join(" -> ", Cities)} (Cost: {Cost:F2})";
        }
    }

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

        // 1. Function of total Cost
        public double CalculateCost(List<int> route)
        {
            double cost = 0;
            for (int i = 0; i < _numCities; i++)
            {
                int currentCity = route[i];
                int nextCity = route[(i + 1) % _numCities];

                cost += _distanceMatrix[currentCity, nextCity];
            }
            return cost;
        }

        // 2. Generating intial solve
        public HillClimbing GenerateInitialRoute()
        {
            List<int> cities = Enumerable.Range(0, _numCities).ToList();

            // Randomizing city list
            for (int i = 0; i < _numCities; i++)
            {
                int j = _random.Next(i, _numCities);
                (cities[i], cities[j]) = (cities[j], cities[i]); // Swap
            }

            double cost = CalculateCost(cities);
            return new HillClimbing(cities, cost);
        }

        // 3. Different mutation operators to generate neighboring solutions

        // a) Swap 
        // Randomly select two different positions and swap the cities at these positions.
        public HillClimbing ApplySwap(HillClimbing currentRoute)
        {
            var newRouteCities = currentRoute.Cities.ToList();

            // Randomizing two cities
            int i = _random.Next(_numCities);
            int j = _random.Next(_numCities);
            while (i == j)
            {
                j = _random.Next(_numCities);
            }

            // City swap on theirs positions
            (newRouteCities[i], newRouteCities[j]) = (newRouteCities[j], newRouteCities[i]);

            double newCost = CalculateCost(newRouteCities);
            return new HillClimbing(newRouteCities, newCost);
        }

        // b) Insert 
        // Random select one city and insert it into a different position.
        public HillClimbing ApplyInsert(HillClimbing currentRoute)
        {
            var newRouteCities = currentRoute.Cities.ToList();

            // Randomizing city to tranfer (sourceIndex) and new position (destIndex)
            int sourceIndex = _random.Next(_numCities);
            int destIndex = _random.Next(_numCities);

            if (sourceIndex == destIndex)
                return currentRoute.Clone(); 

            int cityToMove = newRouteCities[sourceIndex];
            newRouteCities.RemoveAt(sourceIndex); // Deleting on source position
            newRouteCities.Insert(destIndex, cityToMove); // Inserting on new position

            double newCost = CalculateCost(newRouteCities);
            return new HillClimbing(newRouteCities, newCost);
        }

        // c) Reverse (2-opt)
        // 2-opt: Randomly select two different positions and reverse the order of cities between these positions.
        public HillClimbing ApplyReverse(HillClimbing currentRoute)
        {
            var newRouteCities = currentRoute.Cities.ToList();

            // Randomly select two cities 
            int i = _random.Next(_numCities);
            int j = _random.Next(_numCities);

            // Upewniamy się, że i jest mniejsze niż j
            if (i > j)
            {
                (i, j) = (j, i); // Swap
            }
            if (i == j) 
                return currentRoute.Clone();

            // Reversing route from i to j
            newRouteCities.Reverse(i, j - i + 1);

            double newCost = CalculateCost(newRouteCities);
            return new HillClimbing(newRouteCities, newCost);
        }

        // 4. Main Hill Climbing algorithm
        public HillClimbing SolveTsp(Func<HillClimbing, HillClimbing> mutationOperator, int maxIterations = 1000, int maxStagnation=100)
        {
            // Generating intial Route
            HillClimbing currentBestRoute = GenerateInitialRoute();
            Console.WriteLine($"Starting Hill Climbing with {mutationOperator.Method.Name}. Initial Cost: {currentBestRoute.Cost:F2}");

            int iteration = 0;
            int stagnationCounter = 0; 

            while (iteration < maxIterations && stagnationCounter < maxStagnation)
            {
                // Generating neighbour solution
                HillClimbing neighborRoute = mutationOperator(currentBestRoute);

                // Hill Climbing alghorithm - accepting only better solutions
                if (neighborRoute.Cost < currentBestRoute.Cost) 
                {
                    currentBestRoute = neighborRoute;
                    stagnationCounter = 0; 
                }
                else
                {
                    stagnationCounter++; 
                }

                iteration++;
            }

            Console.WriteLine($"Hill Climbing with {mutationOperator.Method.Name} finished after {iteration} iterations.");
            return currentBestRoute;
        }
    }
}
