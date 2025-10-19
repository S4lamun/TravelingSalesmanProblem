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

        // 1. Funkcja obliczająca całkowity koszt (przeniesiona z TSP, aby miała dostęp do macierzy)
        private double CalculateCost(List<int> route)
        {
            double cost = 0;
            for (int i = 0; i < _numCities; i++)
            {
                int currentCity = route[i];
                int nextCity = route[(i + 1) % _numCities];

                // Zakładamy, że macierz jest symetryczna (distanceMatrix[i, j] == distanceMatrix[j, i])
                cost += _distanceMatrix[currentCity, nextCity];
            }
            return cost;
        }

        // 2. Generowanie początkowego rozwiązania (przeniesiona z TSP)
        public TSP GenerateInitialRoute()
        {
            List<int> cities = Enumerable.Range(0, _numCities).ToList();

            // Randomizing city list (Fisher-Yates shuffle)
            for (int i = 0; i < _numCities; i++)
            {
                int j = _random.Next(i, _numCities);
                // Użycie krotki do zamiany (Swap)
                (cities[i], cities[j]) = (cities[j], cities[i]);
            }

            double cost = CalculateCost(cities);
            return new TSP(cities, cost);
        }

        // 3. Różne operatory mutacji do generowania sąsiednich rozwiązań

        // a) Swap (Zamiana)
        public TSP ApplySwap(TSP currentRoute)
        {
            var newRouteCities = currentRoute.Cities.ToList();

            // Randomly select two cities
            int i = _random.Next(_numCities);
            int j = _random.Next(_numCities);
            while (i == j)
            {
                j = _random.Next(_numCities);
            }

            // City swap on theirs positions
            (newRouteCities[i], newRouteCities[j]) = (newRouteCities[j], newRouteCities[i]);

            double newCost = CalculateCost(newRouteCities);
            return new TSP(newRouteCities, newCost);
        }

        // b) Insert (Wstawienie)
        public TSP ApplyInsert(TSP currentRoute)
        {
            var newRouteCities = currentRoute.Cities.ToList();

            // Randomizing city to tranfer (sourceIndex) and new position (destIndex)
            int sourceIndex = _random.Next(_numCities);
            int destIndex = _random.Next(_numCities);

            if (sourceIndex == destIndex)
                return currentRoute.Clone(); // Wracamy do oryginału, jeśli indeksy są identyczne

            int cityToMove = newRouteCities[sourceIndex];
            newRouteCities.RemoveAt(sourceIndex); // Usuwamy z pozycji źródłowej
            newRouteCities.Insert(destIndex, cityToMove); // Wstawiamy na nowej pozycji

            double newCost = CalculateCost(newRouteCities);
            return new TSP(newRouteCities, newCost);
        }

        // c) Reverse (2-opt)
        public TSP ApplyReverse(TSP currentRoute)
        {
            var newRouteCities = currentRoute.Cities.ToList();
            int numCities = newRouteCities.Count;

            // Losujemy dwa różne indeksy
            int i = _random.Next(numCities);
            int j = _random.Next(numCities);

            // Upewniamy się, że i != j
            while (i == j)
            {
                j = _random.Next(numCities);
            }

            // Upewniamy się, że i jest zawsze mniejsze niż j
            if (i > j)
            {
                (i, j) = (j, i); // Zamiana
            }

            // i jest teraz mniejsze niż j

            // Odwracamy segment listy ZACZYNAJĄC od indeksu 'i'
            // o długości 'j - i + 1' (aby objąć element 'j' włącznie)
            newRouteCities.Reverse(i, j - i + 1);

            double newCost = CalculateCost(newRouteCities);
            return new TSP(newRouteCities, newCost);
        }

        // 4. Główny algorytm Hill Climbing (logika bez zmian)
        public TSP SolveTsp(Func<TSP, TSP> mutationOperator, int maxIterations = 1000, int maxStagnation = 100)
        {
            // Generating intial Route (teraz wywołuje metodę tej klasy)
            TSP currentBestRoute = GenerateInitialRoute();

            // Pobieramy nazwę metody dla celów diagnostycznych
            string methodName = mutationOperator.Method.Name.Replace("Apply", "");

            Console.WriteLine($"Starting Hill Climbing with {methodName}. Initial Cost: {currentBestRoute.Cost:F2}");

            int iteration = 0;
            int stagnationCounter = 0;

            while (iteration < maxIterations && stagnationCounter < maxStagnation)
            {
                // Generating neighbour solution
                // Uwaga: mutationOperator to delegat, który MUSI być związany z instancją klasy HillClimbingTsp
                // (np. tspSolver.ApplySwap), aby mieć dostęp do macierzy odległości.
                TSP neighborRoute = mutationOperator(currentBestRoute);

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

            Console.WriteLine($"Hill Climbing with {methodName} finished after {iteration} iterations.");
            return currentBestRoute;
        }
    }
}
