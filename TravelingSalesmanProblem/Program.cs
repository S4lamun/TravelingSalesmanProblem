using ClosedXML.Excel;
using System.Collections.Generic;
using System.Diagnostics; // Required for Stopwatch

namespace TravelingSalesmanProblem
{
    public class Program
    {
        // --- DATA DEFINITIONS ---
        // Make sure these paths are correct for your project structure
        static string _cities48 = @"..\..\..\Dane_TSP_48.xlsx";
        static string _cities76 = @"..\..\..\Dane_TSP_76.xlsx";
        static string _cities127 = @"..\..\..\Dane_TSP_127.xlsx";

        static double[,] dataCities48 = ExcelDataReader.ReadDataToMatrix(_cities48);
        static double[,] dataCities76 = ExcelDataReader.ReadDataToMatrix(_cities76);
        static double[,] dataCities127 = ExcelDataReader.ReadDataToMatrix(_cities127);

        public static void Main(string[] args)
        {
            // Nearest Neighbour Algorithm
            //NearestNeighbour.MakeNearestNeighbourAlgorithm(dataCities48, dataCities127.GetLength(0));
            //NearestNeighbour.MakeNearestNeighbourAlgorithm(dataCities76, dataCities127.GetLength(0));
            //NearestNeighbour.MakeNearestNeighbourAlgorithm(dataCities127, dataCities127.GetLength(0));

            // Run the test suite for Hill Climbing for all datasets
            TestHillCLimbingRafal();

            Console.WriteLine("\nPress any key to exit.");
            Console.ReadKey();
        }

        /// <summary>
        /// Runs the Hill Climbing algorithm for a given dataset and parameters.
        /// Now returns the best route and the execution time.
        /// </summary>
        public static (TSP bestRoute, TimeSpan duration) RunHillClimbingAlgorithm(double[,] dataCities, int maxIterations, string method, int maxStagnation)
        {
            HillClimbingTsp tspSolver = new HillClimbingTsp(dataCities);
            Stopwatch stopwatch = new Stopwatch();
            TSP bestRoute = null;

            Console.WriteLine($"\n--- Running Hill Climbing: Method={method}, Iterations={maxIterations}, Stagnation={maxStagnation} ---");

            stopwatch.Start(); // Start measuring time

            if (method == "SWAP")
            {
                bestRoute = tspSolver.SolveTsp(tspSolver.ApplySwap, maxIterations, maxStagnation);
            }
            else if (method == "INSERT")
            {
                bestRoute = tspSolver.SolveTsp(tspSolver.ApplyInsert, maxIterations, maxStagnation);
            }
            else if (method == "REVERSE")
            {
                bestRoute = tspSolver.SolveTsp(tspSolver.ApplyReverse, maxIterations, maxStagnation);
            }
            else
            {
                Console.WriteLine("Invalid method specified. Please choose SWAP, INSERT, or REVERSE.");
                stopwatch.Stop();
                return (null, stopwatch.Elapsed);
            }

            stopwatch.Stop(); // Stop measuring time

            Console.WriteLine($"Final Best Route ({method}): {bestRoute}");
            Console.WriteLine($"Execution Time: {stopwatch.Elapsed.TotalMilliseconds:F2} ms");
            Console.WriteLine(new string('-', 50));

            return (bestRoute, stopwatch.Elapsed);
        }

        /// <summary>
        /// A helper class to store results before writing to Excel.
        /// </summary>
        public class ResultData
        {
            public string Method { get; set; }
            public int MaxIterations { get; set; }
            public int MaxStagnation { get; set; }
            public double RouteCost { get; set; }
            public double ExecutionTimeMs { get; set; }
        }


        /// <summary>
        /// Runs a battery of tests for all datasets and saves all results to a single Excel file,
        /// with each dataset's results in a separate worksheet.
        /// </summary>
        public static void TestHillCLimbingRafal()
        {
            int[] maxIterations = { 1000, 5000, 10000, 20000 };
            int[] maxStagnation = { 250, 500, 1000, 2000 };
            string[] methods = { "SWAP", "INSERT", "REVERSE" };

            // A dictionary to hold datasets and their names for easy iteration
            var datasets = new Dictionary<string, double[,]>
            {
                { "48_Cities", dataCities48 },
                { "76_Cities", dataCities76 },
                { "127_Cities", dataCities127 }
            };

            string fileName = "HillClimbing_Results.xlsx";
            using (var workbook = new XLWorkbook())
            {
                foreach (var datasetEntry in datasets)
                {
                    string datasetName = datasetEntry.Key;
                    double[,] cityData = datasetEntry.Value;
                    var allResults = new List<ResultData>();
                    List<int> mockList = new List<int> { 1, 2, 3 }; // List for initialization


                    Console.WriteLine($"\n===== STARTING TESTS FOR {datasetName.Replace('_', ' ')} DATASET =====");

                    foreach (var method in methods) // methods
                    {
                        foreach (var iteration in maxIterations) // maxIterations
                        {
                            foreach (var stagnation in maxStagnation) // maxStagnation
                            {
                                TSP bestResultRoute = new TSP(mockList, double.MaxValue);
                                TimeSpan bestTime = TimeSpan.MaxValue;

                                for (int i=0; i<10; i++) // mutlistart
                                {
                                    var (resultRoute, duration) = RunHillClimbingAlgorithm(cityData, iteration, method, stagnation);
                                    if(bestResultRoute.Cost > resultRoute.Cost)
                                    {
                                        bestResultRoute = resultRoute;
                                        bestTime = duration;
                                    }
                                }
                                if (bestResultRoute != null)
                                {
                                    allResults.Add(new ResultData
                                    {
                                        Method = method,
                                        MaxIterations = iteration,
                                        MaxStagnation = stagnation,
                                        RouteCost = bestResultRoute.Cost,
                                        ExecutionTimeMs = bestTime.TotalMilliseconds
                                    });
                                }
                            }
                        }
                    }
                    Console.WriteLine($"\n===== FINISHED TESTS FOR {datasetName.Replace('_', ' ')} DATASET =====");


                    // --- Writing results for the current dataset to a new worksheet ---
                    var worksheet = workbook.Worksheets.Add($"HC_Results_{datasetName}");

                    // Set Headers
                    worksheet.Cell("A1").Value = "Method";
                    worksheet.Cell("B1").Value = "Max Iterations";
                    worksheet.Cell("C1").Value = "Max Stagnation";
                    worksheet.Cell("D1").Value = "Final Route Cost";
                    worksheet.Cell("E1").Value = "Execution Time (ms)";

                    // Style headers
                    var headerRange = worksheet.Range("A1:E1");
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                    // Write data from the list
                    for (int i = 0; i < allResults.Count; i++)
                    {
                        var result = allResults[i];
                        int currentRow = i + 2; // Data starts from row 2
                        worksheet.Cell(currentRow, 1).Value = result.Method;
                        worksheet.Cell(currentRow, 2).Value = result.MaxIterations;
                        worksheet.Cell(currentRow, 3).Value = result.MaxStagnation;
                        worksheet.Cell(currentRow, 4).Value = result.RouteCost;
                        worksheet.Cell(currentRow, 5).Value = result.ExecutionTimeMs;
                    }

                    // Adjust column widths to content
                    worksheet.Columns().AdjustToContents();
                }

                workbook.SaveAs(fileName);
            }

            Console.WriteLine($"\nAll results have been successfully saved to '{fileName}'");
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

