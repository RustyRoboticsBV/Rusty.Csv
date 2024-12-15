#if GODOT
using Godot;
#elif UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

using System;
using System.Collections.Generic;
using System.IO;

namespace Rusty.Csv
{
    /// <summary>
    /// A CSV table.
    /// </summary>
    [Serializable]
    public class CsvTable
    {
        /* Fields. */
#if GODOT
        [Export] string name;
        [Export] string[] cells;
        [Export] int width;
#elif UNITY_5_3_OR_NEWER
        [SerializeField] string name;
        [SerializeField] string[] cells;
        [SerializeField] int width;
#else
        private string name;
        private string[] cells;
        private int width;
#endif

        /* Public properties. */
        /// <summary>
        /// The title of the CSV table.
        /// </summary>
        public string Name
        {
            get => name;
            private set => name = value;
        }
        /// <summary>
        /// The cells of the CSV table, stored row-wise in an 1D array.
        /// </summary>
        public string[] Cells
        {
            get => cells;
            private set => cells = value;
        }
        /// <summary>
        /// The width of the CSV table.
        /// </summary>
        public int Width
        {
            get => width;
            private set => width = value;
        }
        /// <summary>
        /// The height of the CSV table.
        /// </summary>
        public int Height => (int)Mathf.Ceil(Cells.Length / (float)Width);

        /* Private properties. */
        private Dictionary<string, int> RowLookup { get; set; } = new Dictionary<string, int>();
        private Dictionary<string, int> ColumnLookup { get; set; } = new Dictionary<string, int>();

        /* Constructors. */
        /// <summary>
        /// Create a new CSV table from an array of cells and a table width.
        /// </summary>
        public CsvTable(string name, string[] cells, int width)
        {
            Name = name;
            SetContents(cells, width);
        }

        /// <summary>
        /// Create a new CSV table from a string of text.
        /// </summary>
        public CsvTable(string name, string fileText)
        {
            Name = name;
            try
            {
                Parse(fileText, out string[] cells, out int width);
                SetContents(cells, width);
            }
            catch (Exception ex)
            {
                throw new Exception($"CSV: Could not parse file '{name}' due an exception: {ex}.");
            }
        }

        /// <summary>
        /// Create a new CSV table object by loading it from a file.
        /// </summary>
        public CsvTable(string filePath) : this(Path.GetFileNameWithoutExtension(filePath), File.ReadAllText(filePath)) { }

        /* Public methods. */
        /// <summary>
        /// Convert this table to an easy-to-read textual format. This is NOT the same as the file format, use the Serialize
        /// method for that instead!
        /// </summary>
        public override string ToString()
        {
            string str = "";
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    string cell = GetCell(x, y);
                    if (cell.Contains(","))
                        str += $"\"{cell}\"";
                    else
                        str += cell;

                    if (x < Width - 1)
                        str += ", ";
                }
                if (y < Height - 1)
                    str += '\n';
            }
            return str;
        }

        /// <summary>
        /// Get the contents of a cell, using its row and column index.
        /// </summary>
        public string GetCell(int column, int row)
        {
            // Check bounds.
            if (column < 0 || row < 0 || column >= Width || row >= Height)
                throw new ArgumentOutOfRangeException($"The cell ({column}, {row}) is out of bounds!");

            // Get cell index.
            int index = column + row * Width;

            // Get cell.
            return Cells[index];
        }

        /// <summary>
        /// Get the contents of a cell, using its row name and column index.
        /// </summary>
        public string GetCell(int column, string row)
        {
            return GetCell(column, FindRow(row));
        }

        /// <summary>
        /// Get the contents of a cell, using its row index and column name.
        /// </summary>
        public string GetCell(string column, int row)
        {
            return GetCell(FindColumn(column), row);
        }

        /// <summary>
        /// Get the contents of a cell, using its row and column name.
        /// </summary>
        public string GetCell(string column, string row)
        {
            return GetCell(FindColumn(column), FindRow(row));
        }

        /// <summary>
        /// Get a row of cells, using its index.
        /// </summary>
        public string[] GetRow(int row)
        {
            string[] cells = new string[Width];
            for (int i = 0; i < cells.Length; i++)
            {
                cells[i] = GetCell(i, row);
            }
            return cells;
        }

        /// <summary>
        /// Get a row of cells, using its name.
        /// </summary>
        public string[] GetRow(string row)
        {
            return GetRow(FindRow(row));
        }

        /// <summary>
        /// Get a column of cells, using its index.
        /// </summary>
        public string[] GetColumn(int column)
        {
            string[] cells = new string[Height];
            for (int i = 0; i < cells.Length; i++)
            {
                cells[i] = GetCell(column, i);
            }
            return cells;
        }

        /// <summary>
        /// Get a column of cells, using its name.
        /// </summary>
        public string[] GetColumn(string column)
        {
            return GetColumn(FindColumn(column));
        }

        /// <summary>
        /// Convert this table to a textual representation that can be safely written to a .csv file.
        /// </summary>
        public string Serialize()
        {
            string text = "";
            for (int y = 0; y < Height; y++)
            {
                if (y > 0)
                    text += '\n';
                for (int x = 0; x < Width; x++)
                {
                    string cell = cells[y * width + x];
                    bool hasComma = cell.Contains(',');
                    bool hasDoubleQuote = cell.Contains('\"');
                    if (hasDoubleQuote)
                        cell = cell.Replace("\"", "\"\"");
                    if (hasComma || hasDoubleQuote)
                        cell = $"\"{cell}\"";
                    text += cell + ',';
                }
            }
            return text;
        }

        /// <summary>
        /// Save this table to a file.
        /// </summary>
        public void Save(string filePath)
        {
            File.WriteAllText(filePath, Serialize());
        }

        /* Private methods. */
        /// <summary>
        /// Set the contents of this table.
        /// </summary>
        private void SetContents(string[] cells, int width)
        {
            // Figure out dimensions.
            Width = width;
            int height = (int)Mathf.Ceil(cells.Length / (float)width);

            // Copy cells.
            Cells = new string[width * height];
            for (int i = 0; i < cells.Length; i++)
            {
                Cells[i] = cells[i];
            }

            // Add row & column lookup.
            for (int i = 0; i < Width; i++)
            {
                if (!ColumnLookup.ContainsKey(Cells[i]))
                    ColumnLookup.Add(Cells[i], i);
            }
            for (int i = 0; i < Height; i++)
            {
                if (!RowLookup.ContainsKey(Cells[i * Width]))
                    RowLookup.Add(Cells[i * Width], i);
            }
        }

        /// <summary>
        /// Find the index of a column.
        /// </summary>
        private int FindColumn(string name)
        {
            try
            {
                return ColumnLookup[name];
            }
            catch
            {
                throw new ArgumentOutOfRangeException($"CSV: could not find column '{name}'!");
            }
        }

        /// <summary>
        /// Find the index of a row.
        /// </summary>
        private int FindRow(string name)
        {
            try
            {
                return RowLookup[name];
            }
            catch
            {
                throw new ArgumentOutOfRangeException($"CSV: could not find row '{name}'!");
            }
        }

        /// <summary>
        /// Parse the contents of a file, and return a 1D list of cells, as well as the width of the table.
        /// </summary>
        private static void Parse(string fileText, out string[] tableCells, out int tableWidth)
        {
            // Convert line-endings to UNIX-style.
            fileText = fileText.Replace("\r\n", "\n");      // Windows to UNIX.
            fileText = fileText.Replace("\r", "\n");        // Mac to UNIX.

            // Convert tabs to spaces.
            fileText = fileText.Replace("\t", " ");

            // Split into rows.
            List<string> rows = new List<string>(fileText.Split('\n'));

            // Remove empty rows and commentary rows. Rows that contain only commas and/or spaces are considered empty. Rows that
            // start with two slashes are considered to be commentary.
            for (int i = rows.Count - 1; i >= 0; i--)
            {
                if (rows[i].Replace(",", "").Replace(" ", "").Length == 0 || rows[i].StartsWith("//"))
                    rows.RemoveAt(i);
            }

            // Split rows into cells.
            List<List<string>> cells = new List<List<string>>();
            int widestRow = -1;
            for (int i = 0; i < rows.Count; i++)
            {
                // Skip empty rows.
                if (rows[i].Replace(",", "").Replace(" ", "").Length == 0)
                    continue;

                // Also skip rows that start with two slashes. These are interpreted as commentary.
                if (rows[i].StartsWith("//"))
                    continue;

                // Add row.
                cells.Add(new List<string>());

                // Split row string into cells. Commas mark the end of a cell, unless they appear between double quotes ("").
                // A double quote preceded by a backslash (\") is interpreted as a double quote character instead.
                // The last cell in the row doesn't have to be ended with a comma.
                // As we parse each row, we write its characters to the buffer character-by-character.
                string buffer = "";
                bool doubleQuotes = false;
                while (rows[i].Length > 0)
                {
                    // If we are between double quotes...
                    if (doubleQuotes)
                    {
                        // If we encounter two double quotes in a row, add a double quote to the buffer.
                        if (rows[i].Length >= 2 && rows[i][0] == '\"' && rows[i][1] == '\"')
                        {
                            buffer += '\"';
                            rows[i] = rows[i].Remove(0, 2);
                        }

                        // If we encounter a single double quote, exit double quote mode and write nothing to the buffer.
                        else if (rows[i][0] == '\"')
                        {
                            doubleQuotes = false;
                            rows[i] = rows[i].Remove(0, 1);
                        }

                        // Else, write character to the buffer.
                        else
                        {
                            buffer += rows[i][0];
                            rows[i] = rows[i].Remove(0, 1);
                        }
                    }

                    // If we are NOT between double quotes...
                    else
                    {
                        // If we encounter a double quote, enter double quote mode and write nothing to the buffer.
                        if (rows[i][0] == '\"')
                        {
                            doubleQuotes = true;
                            rows[i] = rows[i].Remove(0, 1);
                        }

                        // If we encounter a comma, add the contents of the buffer as a new cell and empty the buffer.
                        else if (rows[i][0] == ',')
                        {
                            cells[i].Add(buffer);
                            buffer = "";
                            rows[i] = rows[i].Remove(0, 1);
                        }

                        // Else, write character to the buffer.
                        else
                        {
                            buffer += rows[i][0];
                            rows[i] = rows[i].Remove(0, 1);
                        }
                    }
                }

                // The last cell on a line doesn't necessarily have to end with a comma. As a result, the buffer string may still
                // contain text. If so, add the current contents as an additional cell.
                if (buffer != "")
                    cells[i].Add(buffer);

                // Check if this row is wider than the currently widest row. If so, update the widest row index.
                if (widestRow == -1 || cells[i].Count > cells[widestRow].Count)
                    widestRow = i;
            }

            // Rows don't necessarily have the same width. If a row is less wide than the widest row, add empty cells to it until it
            // matches the width of the widest row.
            int width = cells[widestRow].Count;
            for (int i = 0; i < cells.Count; i++)
            {
                while (cells[i].Count < width)
                {
                    cells[i].Add("");
                }
            }

            // Convert to 1D array.
            string[] results = new string[cells.Count * width];
            for (int i = 0; i < cells.Count; i++)
            {
                for (int j = 0; j < cells[i].Count; j++)
                {
                    results[j + i * width] = cells[i][j];
                }
            }

            // Write to output.
            tableCells = results;
            tableWidth = width;
        }
    }
}