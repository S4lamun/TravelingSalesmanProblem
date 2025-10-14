using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteligencjaObliczeniowa
{
    internal class NearestNeighbour
    {
        // TSP - travelling salesman problem
        public static (List<int> Path, double TotalDistance) NearestNeighbourTSP(double[,] distanceMatrix, int startNode = 0)
        {
            // Data Validation
            int n = distanceMatrix.GetLength(0);
            if (n != distanceMatrix.GetLength(1) || n == 0)
            {
                throw new ArgumentException("Macierz musi być kwadratowa i niepusta.");
            }
            if (startNode < 0 || startNode >= n)
            {
                throw new ArgumentOutOfRangeException(nameof(startNode), "Indeks miasta startowego jest poza zakresem macierzy.");
            }

            // 2. Initialization 
            var visited = new bool[n];
            var path = new List<int>();
            double totalDistance = 0;
            int currentNode = startNode;

            // 3. Start from initial spot (startNode)
            path.Add(currentNode);
            visited[currentNode] = true;

            // 4. Main loop
            // We continue until all cities are visited (n-1 times, since we start from one)
            while (path.Count < n)
            {
                int nextNode = -1;
                double minDistance = double.MaxValue;

                // Find the nearest unvisited city
                for (int nextCandidate = 0; nextCandidate < n; nextCandidate++)
                {
                    // We check only unvisited cities
                    if (!visited[nextCandidate])
                    {
                        double distance = distanceMatrix[currentNode, nextCandidate];

                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            nextNode = nextCandidate;
                        }
                    }
                }

                // Check if we found a next node
                if (nextNode != -1)
                {
                    // Add the distance to the total
                    totalDistance += minDistance;

                    // Go to the next city
                    currentNode = nextNode;
                    path.Add(currentNode);
                    visited[currentNode] = true;
                }
                else
                {
                    Console.WriteLine("Cannot find next unvisited city");
                    break;
                }
            }

            // 5. Back to start
            if (n > 0)
            {
                double returnDistance = distanceMatrix[currentNode, startNode];
                totalDistance += returnDistance;
                path.Add(startNode);
            }

            return (path, totalDistance);
        }
    }
}
