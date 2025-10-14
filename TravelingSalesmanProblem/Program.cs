using ClosedXML.Excel;

namespace TravelingSalesmanProblem
{
    public class Program
    {
        static string _cities48 = @"..\..\..\Dane_TSP_48.xlsx";
        static string _cities76 = @"..\..\..\Dane_TSP_76.xlsx";
        static string _cities127 = @"..\..\..\Dane_TSP_127.xlsx";
        public static void Main(string[] args)
        {   
            double[,] dataCities48 = ExcelDataReader.ReadDataToMatrix(_cities48);
            double[,] dataCities76 = ExcelDataReader.ReadDataToMatrix(_cities76);
            double[,] dataCities127 = ExcelDataReader.ReadDataToMatrix(_cities127);

          MakeNearestNeighbourAlgorithm(dataCities76, dataCities76.GetLength(0));
        }

        public static void MakeNearestNeighbourAlgorithm(double[,] data, int citiesCount)
        {
            List<int> bestPath = new();
            double bestDistance = double.MaxValue;

            // foreach starting city 
            for (int i = 0; i < citiesCount; i++)
            {
                List<int> currentPath = new List<int>();
                double currentTotalDistance = 0;
                (currentPath, currentTotalDistance) = NearestNeighbour.NearestNeighbourTSP(data, i);
                if (currentTotalDistance<bestDistance)
                {
                    bestPath = currentPath;
                    bestDistance = currentTotalDistance;
                }
            }
            Console.WriteLine("Best path found:");
            foreach (var city in bestPath)
            {
                Console.Write($"{city} -> ");
            }
            Console.WriteLine("Best (lowest) distance: " + bestDistance);
        }

        public static void PrintData(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Console.Write($"{matrix[i, j],8:F2} ");
                }
                Console.WriteLine();
            }
        }
    }
}