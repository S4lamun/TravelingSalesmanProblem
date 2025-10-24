using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelingSalesmanProblem
{
    // Uproszczona Reprezentacja Ruchu dla Listy Tabu
    public struct TabuMove // Domyślnie internal, ale właściwości muszą być publiczne
    {
        public int Index1 { get; set; }  // MUSI BYĆ PUBLIC
        public int Index2 { get; set; }  // MUSI BYĆ PUBLIC
        public MoveTypeEnum MoveType { get; set; } // MUSI BYĆ PUBLIC
    }


    public class TabuSearch
    {
        private static Random random = new Random();

        // --- Metody Pomocnicze (MUSZĄ BYĆ PUBLICZNE, jeśli używane poza tą klasą) ---

        // Metoda CalculateCost musi być publiczna, jeśli jest wywoływana z Program.cs lub innych klas
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

        // Metoda GenerateInitialSolution musi być publiczna, jeśli jest wywoływana z SolveTSP lub innych klas
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

        // Metody Apply... nie muszą być publiczne, jeśli są używane tylko w TabuSearch.SolveTSP
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

        // --------------------------------------------------------------------------

        /// <summary>
        /// Sprawdza, czy dany potencjalny ruch jest tabu.
        /// </summary>
        private static bool IsMoveTabu(Queue<TabuMove> tabuList, TabuMove potentialMove)
        {
            // Logika jest poprawna. Używa PUBLICZNYCH właściwości Index1, Index2 i MoveType.
            foreach (var tabuMove in tabuList)
            {
                if (tabuMove.MoveType != potentialMove.MoveType)
                {
                    continue;
                }

                if (tabuMove.MoveType == MoveTypeEnum.Insert)
                {
                    if (tabuMove.Index1 == potentialMove.Index1 && tabuMove.Index2 == potentialMove.Index2)
                    {
                        return true;
                    }
                }
                else if (tabuMove.MoveType == MoveTypeEnum.Swap || tabuMove.MoveType == MoveTypeEnum.Revert)
                {
                    int tabuMin = Math.Min(tabuMove.Index1, tabuMove.Index2);
                    int tabuMax = Math.Max(tabuMove.Index1, tabuMove.Index2);
                    int currentMin = Math.Min(potentialMove.Index1, potentialMove.Index2);
                    int currentMax = Math.Max(potentialMove.Index1, potentialMove.Index2);

                    if (tabuMin == currentMin && tabuMax == currentMax)
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        /// <summary>
        /// Rozwiązuje problem komiwojażera (TSP) za pomocą algorytmu Tabu Search.
        /// MUSI BYĆ PUBLICZNA, aby można było ją wywołać z klasy Program.
        /// </summary>
        public static TSP SolveTSP(
            double[,] dataMatrix,
            MoveTypeEnum moveType,
            int tabuListLength,
            int maxIterationsWithoutImprovement,
            int maxIterations)
        {
            // ... (Reszta logiki SolveTSP) ...
            // (Logika jest poprawna i używa już zadeklarowanych publicznych/prywatnych statycznych metod)

            // 1. Inicjalizacja
            TSP currentSolution = GenerateInitialSolution(dataMatrix); // OK, jest public static
            TSP bestSolution = currentSolution.Clone();
            Queue<TabuMove> tabuList = new Queue<TabuMove>(tabuListLength);
            int n = dataMatrix.GetLength(0);
            int iterationsWithoutImprovement = 0;
            int totalIterations = 0;

            MoveTypeEnum fixedMoveType = (moveType == MoveTypeEnum.Random)
                ? (MoveTypeEnum)random.Next(1, 4)
                : moveType;
            if (fixedMoveType == MoveTypeEnum.Random)
            {
                fixedMoveType = MoveTypeEnum.Swap;
            }


            // 2. Główna pętla Tabu Search
            while (totalIterations < maxIterations && iterationsWithoutImprovement < maxIterationsWithoutImprovement)
            {
                TSP bestNeighbor = null;
                TabuMove bestNeighborMove = new TabuMove();
                double bestNeighborCost = double.MaxValue;

                // 3. Generowanie i ocenianie sąsiadów
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (i == j) continue;

                        TabuMove potentialMove = new TabuMove
                        {
                            Index1 = i, // OK, jest public
                            Index2 = j, // OK, jest public
                            MoveType = fixedMoveType // OK, jest public
                        };

                        bool isTabu = IsMoveTabu(tabuList, potentialMove);

                        TSP neighbor = currentSolution.Clone();
                        switch (fixedMoveType)
                        {
                            case MoveTypeEnum.Insert: ApplyInsert(neighbor.Cities, i, j); break; // OK, jest private static
                            case MoveTypeEnum.Swap: ApplySwap(neighbor.Cities, i, j); break;     // OK, jest private static
                            case MoveTypeEnum.Revert: ApplyReverse(neighbor.Cities, i, j); break; // OK, jest private static
                        }
                        neighbor.Cost = CalculateCost(neighbor.Cities, dataMatrix); // OK, jest public static

                        bool aspire = isTabu && (neighbor.Cost < bestSolution.Cost);

                        if (!isTabu || aspire)
                        {
                            if (neighbor.Cost < bestNeighborCost)
                            {
                                bestNeighbor = neighbor;
                                bestNeighborCost = neighbor.Cost;
                                bestNeighborMove = potentialMove;
                            }
                        }
                    }
                }

                if (bestNeighbor != null)
                {
                    currentSolution = bestNeighbor;

                    if (currentSolution.Cost < bestSolution.Cost)
                    {
                        bestSolution = currentSolution.Clone();
                        iterationsWithoutImprovement = 0;
                    }
                    else
                    {
                        iterationsWithoutImprovement++;
                    }

                    tabuList.Enqueue(bestNeighborMove);

                    if (tabuList.Count > tabuListLength)
                    {
                        tabuList.Dequeue();
                    }
                }
                else
                {
                    iterationsWithoutImprovement++;
                }

                totalIterations++;
            }

            return bestSolution;
        }
    }
}