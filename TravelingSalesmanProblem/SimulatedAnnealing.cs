using System;
using System.Collections.Generic;
using System.Linq;

namespace TravelingSalesmanProblem
{
    // Note: Assuming TSP class and MoveTypeEnum exist elsewhere and are accessible.

    public class SimulatedAnnealing
    {
        private static Random random = new Random();

        /// <summary>
        /// Calculates the total travel cost for a given route using the distance matrix.
        /// </summary>
        public static double CalculateCost(List<int> route, double[,] dataMatrix)
        {
            double cost = 0;
            int n = route.Count;
            // Iterate through all cities, including the final return to the start city
            for (int i = 0; i < n; i++)
            {
                int from = route[i];
                int to = route[(i + 1) % n]; // Wraps around to the first city (index 0)
                cost += dataMatrix[from, to];
            }
            return cost;
        }

        /// <summary>
        /// Generates a random initial route (solution) using the Fisher-Yates shuffle.
        /// </summary>
        public static TSP GenerateInitialSolution(double[,] dataMatrix)
        {
            int n = dataMatrix.GetLength(0);
            List<int> cities = Enumerable.Range(0, n).ToList();

            // Perform Fisher-Yates shuffle to randomize the route
            for (int i = 0; i < n - 1; i++)
            {
                int j = random.Next(i, n);
                // Swap cities[i] and cities[j]
                int temp = cities[i];
                cities[i] = cities[j];
                cities[j] = temp;
            }

            double cost = CalculateCost(cities, dataMatrix);
            return new TSP(cities, cost);
        }

        // --- Move Operators for Generating Neighbors ---

        /// <summary>
        /// Moves the city at index 'i' to the position 'j' (Insert operation).
        /// </summary>
        private static void ApplyInsert(List<int> route, int i, int j)
        {
            if (i != j)
            {
                int city = route[i];
                route.RemoveAt(i);
                route.Insert(j, city);
            }
        }

        /// <summary>
        /// Swaps the cities at indices 'i' and 'j'.
        /// </summary>
        private static void ApplySwap(List<int> route, int i, int j)
        {
            int temp = route[i];
            route[i] = route[j];
            route[j] = temp;
        }

        /// <summary>
        /// Reverses the segment of the route between indices 'i' and 'j' (2-Opt operation).
        /// </summary>
        private static void ApplyReverse(List<int> route, int i, int j)
        {
            // Ensure i is the start index and j is the end index (i <= j)
            if (i > j) (i, j) = (j, i);
            // Reverse the sublist segment
            route.Reverse(i, j - i + 1);
        }

        /// <summary>
        /// Generates a neighboring solution by applying a specified or random move type.
        /// </summary>
        public static TSP GetNeighborSolution(TSP currentSolution, double[,] dataMatrix, MoveTypeEnum moveType)
        {
            // Start by cloning the current solution to create the neighbor
            TSP neighbor = currentSolution.Clone();
            int n = neighbor.Cities.Count;

            // Randomly select two distinct indices i and j
            int i = random.Next(n);
            int j;
            do { j = random.Next(n); } while (i == j);

            // Apply the chosen move operator
            switch (moveType)
            {
                case MoveTypeEnum.Insert:
                    ApplyInsert(neighbor.Cities, i, j);
                    break;
                case MoveTypeEnum.Swap:
                    ApplySwap(neighbor.Cities, i, j);
                    break;
                case MoveTypeEnum.Revert: // Corresponds to the 2-Opt move
                    ApplyReverse(neighbor.Cities, i, j);
                    break;
                case MoveTypeEnum.Random:
                    // Randomly select and apply one of the three move types
                    int randomMove = random.Next(3);
                    if (randomMove == 0) ApplyInsert(neighbor.Cities, i, j);
                    else if (randomMove == 1) ApplySwap(neighbor.Cities, i, j);
                    else ApplyReverse(neighbor.Cities, i, j);
                    break;
            }

            // Calculate and update the cost for the new neighbor solution
            neighbor.Cost = CalculateCost(neighbor.Cities, dataMatrix);
            return neighbor;
        }

        // --- Main Simulated Annealing Algorithm ---

        /// <summary>
        /// Solves the TSP using the Simulated Annealing metaheuristic.
        /// </summary>
        public static TSP SolveTSP(double[,] dataMatrix, double initialTemperature, double coolingRate, int solutionsPerTemperature, int iterations, MoveTypeEnum moveMethod)
        {
            // 1. Initialization
            TSP currentSolution = GenerateInitialSolution(dataMatrix);
            TSP bestSolution = currentSolution.Clone();
            double T = initialTemperature;

            // 2. Main Loop: Iterate until maximum iterations are reached or temperature is effectively zero
            for (int k = 0; k < iterations && T > 0.0001; k++)
            {
                // 3. Inner Loop: Generate multiple neighbor solutions at the current temperature
                for (int i = 0; i < solutionsPerTemperature; i++)
                {
                    TSP neighborSolution = GetNeighborSolution(currentSolution, dataMatrix, moveMethod);

                    // Calculate the change in energy (cost)
                    double dE = neighborSolution.Cost - currentSolution.Cost;

                    // 4. Acceptance Criteria
                    if (dE < 0) // Case 1: Better solution found (always accept)
                    {
                        currentSolution = neighborSolution;
                        // Update overall best solution found so far
                        if (currentSolution.Cost < bestSolution.Cost)
                        {
                            bestSolution = currentSolution.Clone();
                        }
                    }
                    else // Case 2: Worse solution found (accept with a probability)
                    {
                        // Calculate the acceptance probability: P = e^(-dE / T)
                        double acceptanceProbability = Math.Exp(-dE / T);

                        // Accept the worse solution if a random number is less than P
                        if (random.NextDouble() < acceptanceProbability)
                        {
                            currentSolution = neighborSolution;
                        }
                    }
                }

                // 5. Cooling Schedule: Decrease the temperature
                T *= coolingRate;
            }

            return bestSolution;
        }
    }
}