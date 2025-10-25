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
            // Zakładam, że masz klasę ExcelDataReader
            dataCities48 = ExcelDataReader.ReadDataToMatrix(_cities48);
            dataCities76 = ExcelDataReader.ReadDataToMatrix(_cities76);
            dataCities127 = ExcelDataReader.ReadDataToMatrix(_cities127);
        }

        public static void Main(string[] args)
        {
            // TestNearestNeighbour();

            // TestHillCLimbing(); 

            // TestSimulatedAnnealing();

           //  TestTabuSearch();


            //Odpala to Kazik: 
            // Po skończeniu szukajcie pliku: TabuSearchKazik_Results.xlsx (wyślij go mi na messanger)
            // TestTabuSearchKazik();

            //Odpala to Rafał:
            // Po skończeniu szukajcie pliku: TabuSearchRafal_Results.xlsx (wyślij go mi na messanger)
            //TestTabuSearchRafal();

            TestAntColonyOptimization();

            Console.WriteLine("\nPress any key to exit.");
            Console.ReadKey();
        }


        #region NearestNeighbourAlgorithm

        public static (List<int> bestPath, double bestCost, TimeSpan duration) RunNearestNeighbourAlgorithm(double[,] dataCities)
        {
            Stopwatch stopwatch = new Stopwatch();
            int citiesCount = dataCities.GetLength(0);

            Console.WriteLine($"\n--- Running Nearest Neighbour ({citiesCount} cities) ---");

            stopwatch.Start();

            // Wywołujemy zmodyfikowaną metodę statyczną z klasy NearestNeighbour
            var (bestPath, bestCost) = NearestNeighbour.MakeNearestNeighbourAlgorithm(dataCities, citiesCount);

            stopwatch.Stop();

            // Wypisujemy ścieżkę i koszt
            Console.WriteLine("Best path found (NN):");
            Console.WriteLine(string.Join(" -> ", bestPath));
            Console.WriteLine($"Final Best Route (NN): {bestCost:F2}");
            Console.WriteLine($"Execution Time: {stopwatch.Elapsed.TotalMilliseconds:F2} ms");

            return (bestPath, bestCost, stopwatch.Elapsed);
        }
        public static void TestNearestNeighbour()
        {
            const int MultiStartCount = 10;

            var datasets = new Dictionary<string, double[,]>
            {
                { "48_Cities", dataCities48 },
                { "76_Cities", dataCities76 },
                { "127_Cities", dataCities127 }
            };

            string fileName = "NearestNeighbour_Results.xlsx";
            using (var workbook = new XLWorkbook())
            {
                foreach (var datasetEntry in datasets)
                {
                    string datasetName = datasetEntry.Key;
                    double[,] cityData = datasetEntry.Value;

                    // Lista będzie zawierać tylko 1 wpis dla każdego zestawu danych
                    var allResults = new List<ResultData>();

                    Console.WriteLine($"\n===== STARTING NN TESTS FOR {datasetName.Replace('_', ' ')} DATASET =====");

                    double bestResultCost = double.MaxValue;
                    double totalTimeMs = 0;

                    for (int i = 0; i < MultiStartCount; i++)
                    {
                        var (path, cost, duration) = RunNearestNeighbourAlgorithm(cityData);
                        totalTimeMs += duration.TotalMilliseconds;

                        // Koszt będzie zawsze ten sam, ale dla pewności przypisujemy go raz
                        if (cost < bestResultCost)
                        {
                            bestResultCost = cost;
                        }
                    }

                    double averageTimeMs = totalTimeMs / MultiStartCount;

                    // Dodajemy pojedynczy zagregowany wynik
                    allResults.Add(new ResultData
                    {
                        Method = "NearestNeighbour",
                        RouteCost = bestResultCost,
                        ExecutionTimeMs = averageTimeMs
                    });

                    Console.WriteLine($"\n===== FINISHED NN TESTS FOR {datasetName.Replace('_', ' ')} DATASET =====");
                    Console.WriteLine($"-> Best Cost: {bestResultCost:F2}, Avg Time: {averageTimeMs:F2} ms\n");

                    // Tworzenie arkusza Excel
                    var worksheet = workbook.Worksheets.Add($"NN_Results_{datasetName}");

                    // Wstawiamy dane do tabeli (tylko 3 kolumny)
                    worksheet.Cell(1, 1).InsertTable(allResults.Select(r => new
                    {
                        Algorithm = r.Method,
                        FinalRouteCost = r.RouteCost,
                        AvgExecutionTimeMs = r.ExecutionTimeMs
                    }));

                    var headerRange = worksheet.Range(1, 1, 1, 3); // Dopasowujemy do 3 kolumn
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                    worksheet.Columns().AdjustToContents();
                }

                workbook.SaveAs(fileName);
            }

            Console.WriteLine($"\nAll NN results have been successfully saved to '{fileName}'");
        }

        #endregion


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
            const int MultiStartCount = 10;

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

      

        #region TabuSearchAlgorithm
        public static (TSP bestRoute, TimeSpan duration) RunTabuSearchAlgorithm(double[,] dataCities, MoveTypeEnum moveType, int tabuListLength,
            int maxIterationsWithoutImprovement, int maxIterations)
        {
            Stopwatch stopwatch = new Stopwatch();

            Console.WriteLine($"\n--- Running Tabu Search: Move={moveType}, TabuLen={tabuListLength}, NoImprove={maxIterationsWithoutImprovement}, TotalIter={maxIterations} ---");

            stopwatch.Start();

            // Wywołanie głównej metody algorytmu Tabu Search
            TSP bestRoute = TabuSearch.SolveTSP(dataCities, moveType, tabuListLength, maxIterationsWithoutImprovement, maxIterations);

            stopwatch.Stop();

            Console.WriteLine($"Final Best Route (TS): {bestRoute.Cost:F2}");
            Console.WriteLine($"Execution Time: {stopwatch.Elapsed.TotalMilliseconds:F2} ms");
            Console.WriteLine(new string('-', 50));

            return (bestRoute, stopwatch.Elapsed);
        }

       
        public static void TestTabuSearch()
        {
            // Parametryzacja algorytmu Tabu Search
            int[] tabuListLengths = { 10, 20, 50 };
            int[] maxIterationsWithoutImprovement = { 500, 1000, 2000 };
            int maxIterations = 10000;
            const int MultiStartCount = 10;

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
            };

            string fileName = "TabuSearch_Results.xlsx";
            using (var workbook = new XLWorkbook())
            {
                foreach (var datasetEntry in datasets)
                {
                    string datasetName = datasetEntry.Key;
                    double[,] cityData = datasetEntry.Value;
                    var allResults = new List<ResultData>();
                    List<int> mockList = Enumerable.Range(0, cityData.GetLength(0)).ToList();

                    Console.WriteLine($"\n===== STARTING TS TESTS FOR {datasetName.Replace('_', ' ')} DATASET =====");

                    foreach (var method in methods)
                    {
                        foreach (var tabuLength in tabuListLengths)
                        {
                            foreach (var noImprovementLimit in maxIterationsWithoutImprovement)
                            {
                                TSP bestResultRoute = new TSP(mockList, double.MaxValue);
                                double totalTimeMs = 0;

                                Console.WriteLine($"\n--- Testing: {method}, Tabu={tabuLength}, NoImprove={noImprovementLimit} ---");

                                for (int i = 0; i < MultiStartCount; i++) // Wielokrotny start
                                {
                                    var (resultRoute, duration) = RunTabuSearchAlgorithm(
                                        cityData,
                                        moveMethodMap[method],
                                        tabuLength,
                                        noImprovementLimit,
                                        maxIterations);

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
                                        TabuListLength = tabuLength,
                                        MaxStagnation = noImprovementLimit,
                                        MaxIterations = maxIterations,
                                        RouteCost = bestResultRoute.Cost,
                                        ExecutionTimeMs = averageTimeMs
                                    });
                                    Console.WriteLine($"\n-> Best Run for M={method}, L={tabuLength}, I={noImprovementLimit}: Cost={bestResultRoute.Cost:F2}, AvgTime={averageTimeMs:F2} ms");
                                }
                            }
                        }
                    }
                    Console.WriteLine($"\n===== FINISHED TS TESTS FOR {datasetName.Replace('_', ' ')} DATASET =====");

                    // Zapis wyników do arkusza Excel
                    var worksheet = workbook.Worksheets.Add($"TS_Results_{datasetName}");

                    worksheet.Cell(1, 1).InsertTable(allResults.Select(r => new
                    {
                        r.Method,
                        TabuListLength = r.TabuListLength,
                        MaxStagnation = r.MaxStagnation,
                        MaxIterations = r.MaxIterations,
                        FinalRouteCost = r.RouteCost,
                        AvgExecutionTimeMs = r.ExecutionTimeMs
                    }));

                    var headerRange = worksheet.Range(1, 1, 1, 6);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                    worksheet.Columns().AdjustToContents();
                }

                workbook.SaveAs(fileName);
            }

            Console.WriteLine($"\nAll TS results have been successfully saved to '{fileName}'");
        }

        public static void TestTabuSearchKazik()
        {
            // Parametryzacja algorytmu Tabu Search
            int[] tabuListLengths = { 10, 20, 50 };
            int[] maxIterationsWithoutImprovement = { 500  };
            int maxIterations = 10000;
            const int MultiStartCount = 10;

            var moveMethodMap = new Dictionary<string, MoveTypeEnum>
            {
                { "SWAP", MoveTypeEnum.Swap },
                { "INSERT", MoveTypeEnum.Insert },
                { "REVERSE", MoveTypeEnum.Revert }
            };
            string[] methods = moveMethodMap.Keys.ToArray();

            var datasets = new Dictionary<string, double[,]>
            {
                { "127_Cities", dataCities127 },
            };

            string fileName = "TabuSearchKazik_Results.xlsx";
            using (var workbook = new XLWorkbook())
            {
                foreach (var datasetEntry in datasets)
                {
                    string datasetName = datasetEntry.Key;
                    double[,] cityData = datasetEntry.Value;
                    var allResults = new List<ResultData>();
                    List<int> mockList = Enumerable.Range(0, cityData.GetLength(0)).ToList();

                    Console.WriteLine($"\n===== STARTING TS TESTS FOR {datasetName.Replace('_', ' ')} DATASET =====");

                    foreach (var method in methods)
                    {
                        foreach (var tabuLength in tabuListLengths)
                        {
                            foreach (var noImprovementLimit in maxIterationsWithoutImprovement)
                            {
                                TSP bestResultRoute = new TSP(mockList, double.MaxValue);
                                double totalTimeMs = 0;

                                Console.WriteLine($"\n--- Testing: {method}, Tabu={tabuLength}, NoImprove={noImprovementLimit} ---");

                                for (int i = 0; i < MultiStartCount; i++) // Wielokrotny start
                                {
                                    var (resultRoute, duration) = RunTabuSearchAlgorithm(
                                        cityData,
                                        moveMethodMap[method],
                                        tabuLength,
                                        noImprovementLimit,
                                        maxIterations);

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
                                        TabuListLength = tabuLength,
                                        MaxStagnation = noImprovementLimit,
                                        MaxIterations = maxIterations,
                                        RouteCost = bestResultRoute.Cost,
                                        ExecutionTimeMs = averageTimeMs
                                    });
                                    Console.WriteLine($"\n-> Best Run for M={method}, L={tabuLength}, I={noImprovementLimit}: Cost={bestResultRoute.Cost:F2}, AvgTime={averageTimeMs:F2} ms");
                                }
                            }
                        }
                    }
                    Console.WriteLine($"\n===== FINISHED TS TESTS FOR {datasetName.Replace('_', ' ')} DATASET =====");

                    // Zapis wyników do arkusza Excel
                    var worksheet = workbook.Worksheets.Add($"TS_Results_{datasetName}");

                    worksheet.Cell(1, 1).InsertTable(allResults.Select(r => new
                    {
                        r.Method,
                        TabuListLength = r.TabuListLength,
                        MaxStagnation = r.MaxStagnation,
                        MaxIterations = r.MaxIterations,
                        FinalRouteCost = r.RouteCost,
                        AvgExecutionTimeMs = r.ExecutionTimeMs
                    }));

                    var headerRange = worksheet.Range(1, 1, 1, 6);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                    worksheet.Columns().AdjustToContents();
                }

                workbook.SaveAs(fileName);
            }

            Console.WriteLine($"\nAll TS results have been successfully saved to '{fileName}'");
        }

        public static void TestTabuSearchRafal()
        {
            // Parametryzacja algorytmu Tabu Search
            int[] tabuListLengths = { 10 };
            int[] maxIterationsWithoutImprovement = { 500, 1000, 2000 };
            int maxIterations = 10000;
            const int MultiStartCount = 10;

            var moveMethodMap = new Dictionary<string, MoveTypeEnum>
            {
                { "SWAP", MoveTypeEnum.Swap },
                { "INSERT", MoveTypeEnum.Insert },
                { "REVERSE", MoveTypeEnum.Revert }
            };
            string[] methods = moveMethodMap.Keys.ToArray();

            var datasets = new Dictionary<string, double[,]>
            {
                { "127_Cities", dataCities127 }
            };

            string fileName = "TabuSearchRafal_Results.xlsx";
            using (var workbook = new XLWorkbook())
            {
                foreach (var datasetEntry in datasets)
                {
                    string datasetName = datasetEntry.Key;
                    double[,] cityData = datasetEntry.Value;
                    var allResults = new List<ResultData>();
                    List<int> mockList = Enumerable.Range(0, cityData.GetLength(0)).ToList();

                    Console.WriteLine($"\n===== STARTING TS TESTS FOR {datasetName.Replace('_', ' ')} DATASET =====");

                    foreach (var method in methods)
                    {
                        foreach (var tabuLength in tabuListLengths)
                        {
                            foreach (var noImprovementLimit in maxIterationsWithoutImprovement)
                            {
                                TSP bestResultRoute = new TSP(mockList, double.MaxValue);
                                double totalTimeMs = 0;

                                Console.WriteLine($"\n--- Testing: {method}, Tabu={tabuLength}, NoImprove={noImprovementLimit} ---");

                                for (int i = 0; i < MultiStartCount; i++) // Wielokrotny start
                                {
                                    var (resultRoute, duration) = RunTabuSearchAlgorithm(
                                        cityData,
                                        moveMethodMap[method],
                                        tabuLength,
                                        noImprovementLimit,
                                        maxIterations);

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
                                        TabuListLength = tabuLength,
                                        MaxStagnation = noImprovementLimit,
                                        MaxIterations = maxIterations,
                                        RouteCost = bestResultRoute.Cost,
                                        ExecutionTimeMs = averageTimeMs
                                    });
                                    Console.WriteLine($"\n-> Best Run for M={method}, L={tabuLength}, I={noImprovementLimit}: Cost={bestResultRoute.Cost:F2}, AvgTime={averageTimeMs:F2} ms");
                                }
                            }
                        }
                    }
                    Console.WriteLine($"\n===== FINISHED TS TESTS FOR {datasetName.Replace('_', ' ')} DATASET =====");

                    // Zapis wyników do arkusza Excel
                    var worksheet = workbook.Worksheets.Add($"TS_Results_{datasetName}");

                    worksheet.Cell(1, 1).InsertTable(allResults.Select(r => new
                    {
                        r.Method,
                        TabuListLength = r.TabuListLength,
                        MaxStagnation = r.MaxStagnation,
                        MaxIterations = r.MaxIterations,
                        FinalRouteCost = r.RouteCost,
                        AvgExecutionTimeMs = r.ExecutionTimeMs
                    }));

                    var headerRange = worksheet.Range(1, 1, 1, 6);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                    worksheet.Columns().AdjustToContents();
                }

                workbook.SaveAs(fileName);
            }

            Console.WriteLine($"\nAll TS results have been successfully saved to '{fileName}'");
        }
        #endregion

        #region AntColonyOptimizationAlgorithm
        public static (TSP bestRoute, TimeSpan duration) RunAntColonyOptimization(double[,] dataCities, int numAnts, double alpha, double beta, double rho, int maxIterations)
        {
            Stopwatch stopwatch = new Stopwatch();

            Console.WriteLine($"\n--- Running Ant Colony Optimization: Ants={numAnts}, Alpha={alpha:F1}, Beta={beta:F1}, Rho={rho:F2}, Iter={maxIterations} ---");

            // Inicjalizacja algorytmu ACO
            AntColonyOptimization acoSolver = new AntColonyOptimization(dataCities, numAnts, alpha, beta, rho);

            stopwatch.Start();

            // Uruchomienie głównej metody algorytmu
            TSP bestRoute = acoSolver.Solve(maxIterations);

            stopwatch.Stop();

            Console.WriteLine($"Final Best Route (ACO): {bestRoute.Cost:F2}");
            Console.WriteLine($"Execution Time: {stopwatch.Elapsed.TotalMilliseconds:F2} ms");
            Console.WriteLine(new string('-', 50));

            return (bestRoute, stopwatch.Elapsed);
        }

        public static void TestAntColonyOptimization()
        {
            // Parametryzacja algorytmu Kolonii Mrówek (ACO)
            int[] numAnts = { 10, 20, 50 };                      // Liczba mrówek
            double[] alphas = { 1.0, 2.0, 5.0 };                   // Waga feromonu (alpha)
            double[] betas = { 2.0, 5.0, 7.0 };                    // Waga heurystyki (beta)
            double[] rhos = { 0.1, 0.5, 0.8 };                     // Współczynnik parowania (rho)
            int maxIterations = 700;                         // Liczba iteracji
            const int MultiStartCount = 10;                  // Wielokrotne uruchomienie

            var datasets = new Dictionary<string, double[,]>
            {
                { "48_Cities", dataCities48 },
                { "76_Cities", dataCities76 },
                { "127_Cities", dataCities127 }
            };

            string fileName = "AntColonyOptimization_Results.xlsx";
            using (var workbook = new XLWorkbook())
            {
                foreach (var datasetEntry in datasets)
                {
                    string datasetName = datasetEntry.Key;
                    double[,] cityData = datasetEntry.Value;
                    var allResults = new List<ResultData>();
                    List<int> mockList = Enumerable.Range(0, cityData.GetLength(0)).ToList();

                    Console.WriteLine($"\n===== STARTING ACO TESTS FOR {datasetName.Replace('_', ' ')} DATASET =====");

                    foreach (var m in numAnts)
                    {
                        foreach (var alpha in alphas)
                        {
                            foreach (var beta in betas)
                            {
                                foreach (var rho in rhos)
                                {
                                    TSP bestResultRoute = new TSP(mockList, double.MaxValue);
                                    double totalTimeMs = 0;

                                    Console.WriteLine($"\n--- Testing: Ants={m}, Alpha={alpha}, Beta={beta}, Rho={rho} ---");

                                    // Wielokrotny start (Multi-start)
                                    for (int i = 0; i < MultiStartCount; i++)
                                    {
                                        var (resultRoute, duration) = RunAntColonyOptimization(
                                            cityData, m, alpha, beta, rho, maxIterations);

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
                                            Method = "AntColonyOptimization",
                                            NumAnts = m,
                                            Alpha = alpha,
                                            Beta = beta,
                                            Rho = rho,
                                            MaxIterations = maxIterations,
                                            RouteCost = bestResultRoute.Cost,
                                            ExecutionTimeMs = averageTimeMs
                                        });
                                        Console.WriteLine($"\n-> Best Run for M={m}, a={alpha}, b={beta}, r={rho}: Cost={bestResultRoute.Cost:F2}, AvgTime={averageTimeMs:F2} ms");
                                    }
                                }
                            }
                        }
                    }

                    Console.WriteLine($"\n===== FINISHED ACO TESTS FOR {datasetName.Replace('_', ' ')} DATASET =====");

                    // Zapis wyników do arkusza Excel
                    var worksheet = workbook.Worksheets.Add($"ACO_Results_{datasetName}");

                    worksheet.Cell(1, 1).InsertTable(allResults.Select(r => new
                    {
                        Algorithm = r.Method,
                        NumAnts = r.NumAnts,
                        Alpha = r.Alpha,
                        Beta = r.Beta,
                        Rho = r.Rho,
                        MaxIterations = r.MaxIterations,
                        FinalRouteCost = r.RouteCost,
                        AvgExecutionTimeMs = r.ExecutionTimeMs
                    }));

                    // Formatowanie nagłówka
                    var headerRange = worksheet.Range(1, 1, 1, 8);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                    worksheet.Columns().AdjustToContents();
                }

                workbook.SaveAs(fileName);
            }

            Console.WriteLine($"\nAll ACO results have been successfully saved to '{fileName}'");
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