using System;
using System.Collections.Generic;
using System.Linq;

namespace TravelingSalesmanProblem
{
    public class AntColonyOptimization
    {
        // === 1. PARAMETRY ALGORYTMU ===
        private readonly double _alpha; // Waga feromonu (znaczenie śladu)
        private readonly double _beta;  // Waga heurystyki (znaczenie odległości)
        private readonly double _rho;   // Współczynnik parowania feromonu

        private readonly int _n; // Liczba miast
        private readonly int _m; // Liczba mrówek
        private readonly double[,] _distanceMatrix; // Macierz odległości
        private double[,] _pheromoneMatrix;        // Macierz feromonów

        // Stałe
        private const double Q = 100.0; // Ilość feromonu
        private const double InitialPheromone = 1.0;
        private const double MinPheromone = 0.001;

        // Generator liczb losowych
        private readonly Random _random;

        public AntColonyOptimization(double[,] distanceMatrix, int numAnts, double alpha = 1.0, double beta = 5.0, double rho = 0.5)
        {
            _distanceMatrix = distanceMatrix;
            _n = distanceMatrix.GetLength(0);
            _m = numAnts;

            _alpha = alpha;
            _beta = beta;
            _rho = rho;

            _random = new Random(Guid.NewGuid().GetHashCode());

            InitializePheromones();
        }

        private void InitializePheromones()
        {
            _pheromoneMatrix = new double[_n, _n];
            for (int i = 0; i < _n; i++)
            {
                for (int j = 0; j < _n; j++)
                {
                    if (i != j)
                    {
                        _pheromoneMatrix[i, j] = InitialPheromone;
                    }
                }
            }
        }

        // ----------------------------------------------------------------------
        // GŁÓWNA METODA ALGORYTMU
        // ----------------------------------------------------------------------

        public TSP Solve(int maxIterations)
        {
            TSP bestGlobalRoute = new TSP(new List<int>(), double.MaxValue);

            for (int iter = 0; iter < maxIterations; iter++)
            {
                List<TSP> antRoutes = new List<TSP>();

                // 1. FAZA KONSTRUKCJI ROZWIĄZAŃ
                for (int k = 0; k < _m; k++)
                {
                    antRoutes.Add(ConstructRoute());
                }

                // 2. AKTUALIZACJA NAJLEPSZEGO ROZWIĄZANIA
                TSP bestIterationRoute = antRoutes.OrderBy(r => r.Cost).First();
                if (bestIterationRoute.Cost < bestGlobalRoute.Cost)
                {
                    bestGlobalRoute = bestIterationRoute.Clone();
                    Console.WriteLine($"Iteration {iter + 1}: New best cost = {bestGlobalRoute.Cost:F2}");
                }

                // 3. FAZA AKTUALIZACJI FEROMONÓW
                EvaporatePheromones();
                DepositPheromones(antRoutes, bestGlobalRoute);
            }

            return bestGlobalRoute;
        }

        // ----------------------------------------------------------------------
        // METODY KONSTRUKCJI I RUCHU MRÓWKI
        // ----------------------------------------------------------------------

        private TSP ConstructRoute()
        {
            List<int> route = new List<int>();
            HashSet<int> visited = new HashSet<int>();

            // Losowy wybór miasta startowego
            int startCity = _random.Next(_n);
            int currentCity = startCity;

            route.Add(currentCity);
            visited.Add(currentCity);

            // Budowanie trasy
            while (visited.Count < _n)
            {
                int nextCity = SelectNextCity(currentCity, visited);
                route.Add(nextCity);
                visited.Add(nextCity);
                currentCity = nextCity;
            }

            // Powrót do miasta startowego
            route.Add(startCity);

            double cost = CalculateRouteCost(route);

            return new TSP(route, cost);
        }

        private int SelectNextCity(int currentCity, HashSet<int> visited)
        {
            List<int> unvisitedCities = Enumerable.Range(0, _n)
                                                .Where(city => !visited.Contains(city))
                                                .ToList();

            if (unvisitedCities.Count == 0)
                return -1;

            // Obliczanie prawdopodobieństw dla wszystkich nieodwiedzonych miast
            double[] probabilities = new double[unvisitedCities.Count];
            double totalProbability = 0.0;

            for (int i = 0; i < unvisitedCities.Count; i++)
            {
                int city = unvisitedCities[i];
                probabilities[i] = CalculateCityAttractiveness(currentCity, city);
                totalProbability += probabilities[i];
            }

            // Normalizacja prawdopodobieństw
            for (int i = 0; i < unvisitedCities.Count; i++)
            {
                probabilities[i] /= totalProbability;
            }

            // Selekcja za pomocą ruletki
            double r = _random.NextDouble();
            double cumulative = 0.0;

            for (int i = 0; i < unvisitedCities.Count; i++)
            {
                cumulative += probabilities[i];
                if (r <= cumulative)
                {
                    return unvisitedCities[i];
                }
            }

            // Awaryjnie zwróć ostatnie miasto
            return unvisitedCities.Last();
        }

        private double CalculateCityAttractiveness(int i, int j)
        {
            if (i == j) return 0.0;

            double heuristic = 1.0 / (_distanceMatrix[i, j] + double.Epsilon);
            return Math.Pow(_pheromoneMatrix[i, j], _alpha) * Math.Pow(heuristic, _beta);
        }

        private double CalculateRouteCost(List<int> route)
        {
            double cost = 0;
            for (int i = 0; i < route.Count - 1; i++)
            {
                int cityA = route[i];
                int cityB = route[i + 1];
                cost += _distanceMatrix[cityA, cityB];
            }
            return cost;
        }

        // ----------------------------------------------------------------------
        // METODY AKTUALIZACJI FEROMONÓW
        // ----------------------------------------------------------------------

        private void EvaporatePheromones()
        {
            for (int i = 0; i < _n; i++)
            {
                for (int j = 0; j < _n; j++)
                {
                    if (i != j)
                    {
                        _pheromoneMatrix[i, j] *= (1.0 - _rho);
                        // Utrzymanie minimalnego poziomu feromonu
                        if (_pheromoneMatrix[i, j] < MinPheromone)
                            _pheromoneMatrix[i, j] = MinPheromone;
                    }
                }
            }
        }

        private void DepositPheromones(List<TSP> antRoutes, TSP bestGlobalRoute)
        {
            // Elitarny system: wszystkie mrówki dodają feromon, ale najlepsza dodaje więcej

            // 1. Feromon od wszystkich mrówek
            foreach (var route in antRoutes)
            {
                double deltaTau = Q / route.Cost;

                for (int i = 0; i < route.Cities.Count - 1; i++)
                {
                    int cityA = route.Cities[i];
                    int cityB = route.Cities[i + 1];

                    _pheromoneMatrix[cityA, cityB] += deltaTau;
                    _pheromoneMatrix[cityB, cityA] += deltaTau;
                }
            }

            // 2. Dodatkowy feromon od najlepszej globalnej trasy (elitaryzm)
            double eliteDeltaTau = 2.0 * Q / bestGlobalRoute.Cost;
            for (int i = 0; i < bestGlobalRoute.Cities.Count - 1; i++)
            {
                int cityA = bestGlobalRoute.Cities[i];
                int cityB = bestGlobalRoute.Cities[i + 1];
                _pheromoneMatrix[cityA, cityB] += eliteDeltaTau;
                _pheromoneMatrix[cityB, cityA] += eliteDeltaTau;
            }
        }

        // Metoda pomocnicza do diagnostyki
        public void PrintPheromoneMatrix()
        {
            Console.WriteLine("Pheromone Matrix:");
            for (int i = 0; i < _n; i++)
            {
                for (int j = 0; j < _n; j++)
                {
                    Console.Write($"{_pheromoneMatrix[i, j]:F3}\t");
                }
                Console.WriteLine();
            }
        }
    }
}

