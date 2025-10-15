using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;

namespace TravelingSalesmanProblem
{
    internal class ExcelDataReader
    {
        public static double[,] ReadDataToMatrix(string filePath)
        {
            try
            {
                using (var workbook = new XLWorkbook(filePath))
                {
                    var worksheet = workbook.Worksheet(1);
                    var range = worksheet.RangeUsed();

                    int startRow = 2;
                    int startCol = 2;
                    int endRow = range.LastRow().RowNumber();
                    int endCol = range.LastColumn().ColumnNumber();

                    int rows = endRow - startRow + 1;
                    int cols = endCol - startCol + 1;

                    // Sprawdzenie czy macierz jest kwadratowa
                    if (rows != cols)
                    {
                        Console.WriteLine($"Data must be squared ({rows}x{cols})");
                        return null;
                    }

                    double[,] matrix = new double[rows, cols]; // Zmieniono na double[,]

                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < cols; j++)
                        {
                            var cell = worksheet.Cell(startRow + i, startCol + j);

                            if (cell.Value.IsBlank)
                            {
                                Console.WriteLine($"Cell ({startRow + i}, {startCol + j}) is empty. Assert that is 0");
                                matrix[i, j] = 0.0; // Użycie 0.0 (double)
                            }
                            else if (cell.Value.IsNumber)
                            {
                                // Odczytanie wartości jako double
                                matrix[i, j] = cell.GetValue<double>();
                            }
                            // Próba parsowania na double, jeśli nie jest to typ liczbowy
                            else if (double.TryParse(cell.Value.ToString(), out double value))
                            {
                                matrix[i, j] = value;
                            }
                            else
                            {
                                Console.WriteLine($"Cannot convert value '{cell.Value}' in cell ({startRow + i}, {startCol + j}) to double. Assert that 0.");
                                matrix[i, j] = 0.0; // Użycie 0.0 (double)
                            }
                        }
                    }

                    Console.WriteLine($"Data successful read {rows}x{cols}");
                    return matrix;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failure while uploading file: {ex.Message}");
                return null;
            }
        }
    }
}