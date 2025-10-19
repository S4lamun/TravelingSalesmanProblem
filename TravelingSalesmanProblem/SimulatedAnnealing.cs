using System;
using System.Collections.Generic;
using System.Linq;

namespace TravelingSalesmanProblem
{
    public class SimulatedAnnealing
    {
        private static Random random = new Random();
        public static double CalculateCost(List<int> route, double[,] dataMatrix)
        {
            double cost = 0;
            int n = route.Count;
            for (int i = 0; i < n; i++)
            {
                int from = route[i];
                int to = route[(i + 1) % n];
                cost += dataMatrix[from, to];
            }
            return cost;
        }

        public static TSP GenerateInitialSolution(double[,] dataMatrix)
        {
            int n = dataMatrix.GetLength(0);
            List<int> cities = Enumerable.Range(0, n).ToList();
            for (int i = 0; i < n - 1; i++)
            {
                int j = random.Next(i, n);
                int temp = cities[i];
                cities[i] = cities[j];
                cities[j] = temp;
            }
            double cost = CalculateCost(cities, dataMatrix);
            return new TSP(cities, cost);
        }

        private static void ApplyInsert(List<int> route, int i, int j)
        {
            if (i != j)
            {
                int city = route[i];
                route.RemoveAt(i);
                route.Insert(j, city);
            }
        }

        private static void ApplySwap(List<int> route, int i, int j)
        {
            int temp = route[i];
            route[i] = route[j];
            route[j] = temp;
        }

        private static void ApplyReverse(List<int> route, int i, int j)
        {
            if (i > j) (i, j) = (j, i);
            route.Reverse(i, j - i + 1);
        }

        public static TSP GetNeighborSolution(TSP currentSolution, double[,] dataMatrix, MoveTypeEnum moveType)
        {
            TSP neighbor = currentSolution.Clone();
            int n = neighbor.Cities.Count;

            // Wybierz losowo 2 różne indeksy i i j
            int i = random.Next(n);
            int j;
            do { j = random.Next(n); } while (i == j);

            switch (moveType)
            {
                case MoveTypeEnum.Insert:
                    ApplyInsert(neighbor.Cities, i, j);
                    break;
                case MoveTypeEnum.Swap:
                    ApplySwap(neighbor.Cities, i, j);
                    break;
                case MoveTypeEnum.Revert: // 2-Opt
                    ApplyReverse(neighbor.Cities, i, j);
                    break;
                case MoveTypeEnum.Random:
                    int randomMove = random.Next(3);
                    if (randomMove == 0) ApplyInsert(neighbor.Cities, i, j);
                    else if (randomMove == 1) ApplySwap(neighbor.Cities, i, j);
                    else ApplyReverse(neighbor.Cities, i, j);
                    break;
            }

            neighbor.Cost = CalculateCost(neighbor.Cities, dataMatrix);
            return neighbor;
        }

    // Main Alghorithm
        public static TSP SolveTSP(double[,] dataMatrix, double initialTemperature, double coolingRate, int solutionsPerTemperature,int iterations, MoveTypeEnum moveMethod) 
        {
            TSP currentSolution = GenerateInitialSolution(dataMatrix);
            TSP bestSolution = currentSolution.Clone();
            double T = initialTemperature;

            for (int k = 0; k < iterations && T > 0.0001; k++)
            {
                for (int i = 0; i < solutionsPerTemperature; i++)
                {
                    TSP neighborSolution = GetNeighborSolution(currentSolution, dataMatrix, moveMethod);

                    double dE = neighborSolution.Cost - currentSolution.Cost;

                    if (dE < 0)
                    {
                        currentSolution = neighborSolution;
                        if (currentSolution.Cost < bestSolution.Cost)
                        {
                            bestSolution = currentSolution.Clone();
                        }
                    }
                    else
                    {
                        double acceptanceProbability = Math.Exp(-dE / T);
                        if (random.NextDouble() < acceptanceProbability)
                        {
                            currentSolution = neighborSolution;
                        }
                    }
                }

                T *= coolingRate;
            }

            return bestSolution;
        }
    }
}