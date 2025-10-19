using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace TravelingSalesmanProblem
{
    public class Program
    {
        static string _cities48 = @"..\..\..\Dane_TSP_48.xlsx";
        static string _cities76 = @"..\..\..\Dane_TSP_76.xlsx";
        static string _cities127 = @"..\..\..\Dane_TSP_127.xlsx";

        static double[,] dataCities48;
        static double[,] dataCities76;
        static double[,] dataCities127;

        static Program() 
        {
            dataCities48 = ExcelDataReader.ReadDataToMatrix(_cities48);
            dataCities76 = ExcelDataReader.ReadDataToMatrix(_cities76);
            dataCities127 = ExcelDataReader.ReadDataToMatrix(_cities127);
        }

        public static void Main(string[] args)
        {
            TestSimulatedAnnealing();

            // TestHillCLimbingRafal(); 

            Console.WriteLine("\nPress any key to exit.");
            Console.ReadKey();
        }

        #region HillClimbingAlgorithm
        public static (TSP bestRoute, TimeSpan duration) RunHillClimbingAlgorithm(double[,] dataCities, int maxIterations, string method, int maxStagnation)
        {
            HillClimbingTsp tspSolver = new HillClimbingTsp(dataCities);
            Stopwatch stopwatch = new Stopwatch();
            TSP bestRoute = null; 

            Console.WriteLine($"\n--- Running Hill Climbing: Method={method}, Iterations={maxIterations}, Stagnation={maxStagnation} ---");

            Func<TSP, TSP> mutationOperator;
            if (method == "SWAP")
            {
                mutationOperator = tspSolver.ApplySwap;
            }
            else if (method == "INSERT")
            {
                mutationOperator = tspSolver.ApplyInsert;
            }
            else if (method == "REVERSE")
            {
                mutationOperator = tspSolver.ApplyReverse;
            }
            else
            {
                throw new ArgumentException($"Nieznana metoda dla Hill Climbing: {method}");
            }

            stopwatch.Start();

            bestRoute = tspSolver.SolveTsp(mutationOperator, maxIterations, maxStagnation);

            stopwatch.Stop();

            if (bestRoute == null)
            {
                Console.WriteLine($"\n--- Błąd: Algorytm HC nie zwrócił trasy dla metody {method} ---");
                bestRoute = tspSolver.GenerateInitialRoute();
            }

            Console.WriteLine($"Final Best Route ({method}): {bestRoute.Cost:F2}");
            Console.WriteLine($"Execution Time: {stopwatch.Elapsed.TotalMilliseconds:F2} ms");
            Console.WriteLine(new string('-', 50));

            return (bestRoute, stopwatch.Elapsed);
        }


        public static void TestHillCLimbing()
        {
            int[] maxIterations = { 1000, 5000, 10000, 20000 };
            int[] maxStagnation = { 250, 500, 1000, 2000 };
            string[] methods = { "SWAP", "INSERT", "REVERSE" };

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
                    List<int> mockList = Enumerable.Range(0, cityData.GetLength(0)).ToList();

                    Console.WriteLine($"\n===== STARTING HC TESTS FOR {datasetName.Replace('_', ' ')} DATASET =====");

                    foreach (var method in methods)
                    {
                        foreach (var iteration in maxIterations)
                        {
                            foreach (var stagnation in maxStagnation)
                            {
                                TSP bestResultRoute = new TSP(mockList, double.MaxValue);
                                double totalTimeMs = 0;
                                const int MultiStartCount = 10;

                                for (int i = 0; i < MultiStartCount; i++) // Multi-start
                                {
                                    var (resultRoute, duration) = RunHillClimbingAlgorithm(cityData, iteration, method, stagnation);
                                    totalTimeMs += duration.TotalMilliseconds;

                                    if (resultRoute != null && bestResultRoute.Cost > resultRoute.Cost)
                                    {
                                        bestResultRoute = resultRoute;
                                    }
                                }

                                double averageTimeMs = totalTimeMs / MultiStartCount;

                                if (bestResultRoute != null)
                                {
                                    allResults.Add(new ResultData
                                    {
                                        Method = method,
                                        MaxIterations = iteration,
                                        MaxStagnation = stagnation, 
                                        RouteCost = bestResultRoute.Cost,
                                        ExecutionTimeMs = averageTimeMs
                                    });
                                }
                            }
                        }
                    }
                    Console.WriteLine($"\n===== FINISHED HC TESTS FOR {datasetName.Replace('_', ' ')} DATASET =====");


                    var worksheet = workbook.Worksheets.Add($"HC_Results_{datasetName}");

                    worksheet.Cell(1, 1).InsertTable(allResults.Select(r => new
                    {
                        r.Method,
                        MaxIterations = r.MaxIterations,
                        MaxStagnation = r.MaxStagnation,
                        FinalRouteCost = r.RouteCost,
                        ExecutionTimeMs = r.ExecutionTimeMs
                    }));

                    var headerRange = worksheet.Range(1, 1, 1, 5);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                    worksheet.Columns().AdjustToContents();
                }

                workbook.SaveAs(fileName);
            }

            Console.WriteLine($"\nAll HC results have been successfully saved to '{fileName}'");
        }
        #endregion

        #region SimulatedAnnealingAlgorithm
        public static (TSP bestRoute, TimeSpan duration) RunSimulatedAnnealingAlgorithm(double[,] dataCities, double initialTemperature, double coolingRate,
            int solutionsPerTemperature, int maxIterations, MoveTypeEnum moveMethod) 
        {
            Stopwatch stopwatch = new Stopwatch();

            Console.WriteLine($"\n--- Running SA: T0={initialTemperature}, alpha={coolingRate:F4}, Sols/T={solutionsPerTemperature}, Iter={maxIterations}, Move={moveMethod} ---");

            stopwatch.Start();

            TSP bestRoute = SimulatedAnnealing.SolveTSP(dataCities, initialTemperature, coolingRate,
                solutionsPerTemperature, maxIterations, moveMethod);

            stopwatch.Stop();

            Console.WriteLine($"Final Best Route (SA): {bestRoute.Cost:F2}");
            Console.WriteLine($"Execution Time: {stopwatch.Elapsed.TotalMilliseconds:F2} ms");

            return (bestRoute, stopwatch.Elapsed);
        }


        public static void TestSimulatedAnnealing()
        {
            double[] initialTemperatures = { 1000.0, 500.0, 200.0 };
            double[] coolingRates = { 0.99, 0.999, 0.95 };
            int[] solutionsPerTemperature = { 10, 50, 100 };
            int maxIterations = 1000;
            const int MultiStartCount = 5;

            var moveMethodMap = new Dictionary<string, MoveTypeEnum>
            {
                { "SWAP", MoveTypeEnum.Swap },
                { "INSERT", MoveTypeEnum.Insert },
                { "REVERSE", MoveTypeEnum.Revert } 
            };
            string[] methods = moveMethodMap.Keys.ToArray();

            var datasets = new Dictionary<string, double[,]>
            {
                { "48_Cities", dataCities48 },
                { "76_Cities", dataCities76 },
                { "127_Cities", dataCities127 }
            };

            string fileName = "SimulatedAnnealing_Results.xlsx";
            using (var workbook = new XLWorkbook())
            {
                foreach (var datasetEntry in datasets)
                {
                    string datasetName = datasetEntry.Key;
                    double[,] cityData = datasetEntry.Value;
                    var allResults = new List<ResultData>();
                    List<int> mockList = Enumerable.Range(0, cityData.GetLength(0)).ToList();

                    Console.WriteLine($"\n===== STARTING SA TESTS FOR {datasetName.Replace('_', ' ')} DATASET =====");

                    foreach (var temp in initialTemperatures)
                    {
                        foreach (var alpha in coolingRates)
                        {
                            foreach (var solsPerT in solutionsPerTemperature)
                            {
                                foreach (var method in methods)
                                {
                                    TSP bestResultRoute = new TSP(mockList, double.MaxValue);
                                    double totalTimeMs = 0;

                                    for (int i = 0; i < MultiStartCount; i++)
                                    {
                                        var (resultRoute, duration) = RunSimulatedAnnealingAlgorithm(
                                            cityData,
                                            temp,
                                            alpha,
                                            solsPerT,
                                            maxIterations,
                                            moveMethodMap[method]);

                                        totalTimeMs += duration.TotalMilliseconds;

                                        if (resultRoute != null && bestResultRoute.Cost > resultRoute.Cost)
                                        {
                                            bestResultRoute = resultRoute;
                                        }
                                    }

                                    double averageTimeMs = totalTimeMs / MultiStartCount;

                                    if (bestResultRoute != null)
                                    {
                                        allResults.Add(new ResultData
                                        {
                                            Method = method,
                                            InitialTemperature = temp,
                                            CoolingRate = alpha,
                                            SolutionsPerTemperature = solsPerT,
                                            MaxIterations = maxIterations,
                                            RouteCost = bestResultRoute.Cost,
                                            ExecutionTimeMs = averageTimeMs
                                        });
                                        Console.WriteLine($"\n-> Best Run for T0={temp}, a={alpha}, S/T={solsPerT}, M={method}: Cost={bestResultRoute.Cost:F2}, AvgTime={averageTimeMs:F2} ms");
                                    }
                                }
                            }
                        }
                    }
                    Console.WriteLine($"\n===== FINISHED SA TESTS FOR {datasetName.Replace('_', ' ')} DATASET =====");

                    var worksheet = workbook.Worksheets.Add($"SA_Results_{datasetName}");

                    worksheet.Cell(1, 1).InsertTable(allResults.Select(r => new
                    {
                        r.Method,
                        T0 = r.InitialTemperature,
                        Alpha = r.CoolingRate,
                        SolutionsPerTemperature = r.SolutionsPerTemperature,
                        MaxIterations = r.MaxIterations,
                        FinalRouteCost = r.RouteCost,
                        AvgExecutionTimeMs = r.ExecutionTimeMs
                    }));

                    var headerRange = worksheet.Range(1, 1, 1, 7);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                    worksheet.Columns().AdjustToContents();
                }

                workbook.SaveAs(fileName);
            }

            Console.WriteLine($"\nAll SA results have been successfully saved to '{fileName}'");
        }

        #endregion



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