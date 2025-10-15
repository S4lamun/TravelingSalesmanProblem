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

            // Nearest Neighbour Algorithm
            //NearestNeighbour.MakeNearestNeighbourAlgorithm(dataCities48, dataCities127.GetLength(0));
            //NearestNeighbour.MakeNearestNeighbourAlgorithm(dataCities76, dataCities127.GetLength(0));
            //NearestNeighbour.MakeNearestNeighbourAlgorithm(dataCities127, dataCities127.GetLength(0));

            // Hill Climbing Algorithm
            HillClimbingAlgorithm(dataCities48, 1000, "SWAP");
        }


        public static void HillClimbingAlgorithm(double[,] dataCities, int maxIterations, string method)
        {
            HillClimbingTsp tspSolver = new HillClimbingTsp(dataCities);

            Console.WriteLine("\n--- Rozwiązywanie problemu komiwojażera algorytmem Hill Climbing z różnymi ruchami ---\n");

            if (method == "SWAP")
            {
                HillClimbing bestSwapRoute = tspSolver.SolveTsp(tspSolver.ApplySwap, maxIterations);
                Console.WriteLine($"\nFinal Best Route (Swap): {bestSwapRoute}");
                Console.WriteLine(new string('-', 50));
            }
            else if (method == "INSERT")
            {
                HillClimbing bestSwapRoute = tspSolver.SolveTsp(tspSolver.ApplyInsert, maxIterations);
                Console.WriteLine($"\nFinal Best Route (Swap): {bestSwapRoute}");
                Console.WriteLine(new string('-', 50));
            }
            else if (method == "REVERSE")
            {
                HillClimbing bestSwapRoute = tspSolver.SolveTsp(tspSolver.ApplyReverse, maxIterations);
                Console.WriteLine($"\nFinal Best Route (Swap): {bestSwapRoute}");
                Console.WriteLine(new string('-', 50));
            }
            else
            {
                Console.WriteLine("Invalid method specified. Please choose SWAP, INSERT, or REVERSE.");
                return;
            }

            // 2. Insert
            HillClimbing bestInsertRoute = tspSolver.SolveTsp(tspSolver.ApplyInsert, maxIterations);
            Console.WriteLine($"\nFinal Best Route (Insert): {bestInsertRoute}");
            Console.WriteLine(new string('-', 50));

            // 3. Reverse (2-opt-like)
            HillClimbing bestReverseRoute = tspSolver.SolveTsp(tspSolver.ApplyReverse, maxIterations);
            Console.WriteLine($"\nFinal Best Route (Reverse): {bestReverseRoute}");
            Console.WriteLine(new string('-', 50));
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