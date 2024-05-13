using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CURSE
{
    internal class Calculations
    {
        private DataStorage _storage;

        public Calculations(DataStorage _storage)
        {
            this._storage = _storage;
        }

        public DataTable CalculateMPlus(DataTable dTable)
        {
            DataTable result = new DataTable();
            for (int columnIndex = 0; columnIndex < dTable.Columns.Count; columnIndex++)
            {
                result.Columns.Add(dTable.Columns[columnIndex].ColumnName, typeof(double));
            }
            for (int i = 0; i < dTable.Rows.Count; i++)
            {
                DataRow newRow = result.Rows.Add();
                for (int j = 0; j < dTable.Columns.Count; j++)
                {
                    double value = Convert.ToDouble(dTable.Rows[i][j]) + _storage.E;
                    newRow[j] = value;
                }
            }
            return result;
        }

        public DataTable RemoveEpochFromDT(DataTable sourceTable)
        {
            DataTable destinationTable = sourceTable.Clone();
            destinationTable.Columns.RemoveAt(0);
            foreach (DataRow row in sourceTable.Rows)
            {
                DataRow newRow = destinationTable.NewRow();
                for (int i = 1; i < sourceTable.Columns.Count; i++)
                {
                    newRow[i - 1] = row[i];
                }
                destinationTable.Rows.Add(newRow);
            }
            return destinationTable;
        }

        public DataTable CalculateMMinus(DataTable dTable)
        {
            DataTable result = new DataTable();
            for (int columnIndex = 0; columnIndex < dTable.Columns.Count; columnIndex++)
            {
                result.Columns.Add(dTable.Columns[columnIndex].ColumnName, typeof(double));
            }
            for (int i = 0; i < dTable.Rows.Count; i++)
            {
                DataRow newRow = result.Rows.Add();
                for (int j = 0; j < dTable.Columns.Count; j++)
                {
                    double value = Convert.ToDouble(dTable.Rows[i][j]) - _storage.E;
                    newRow[j] = value;
                }
            }
            return result;
        }

        public double MultiSumOfTwoRows(DataRow row1, DataRow row2)
        {
            double sum = 0;
            for (int i = 0; i < row1.ItemArray.Length; i++)
            {
                double value1 = Convert.ToDouble(row1[i]);
                double value2 = Convert.ToDouble(row2[i]);
                sum += value1 * value2;
            }
            return sum;
        }

        public double CalculateAlpha(DataRow row1, DataRow row2, double value1, double value2)
        {
            double sumProd = MultiSumOfTwoRows(row1, row2);
            double denominator = value1 * value2;
            double radians = Math.Acos(sumProd / denominator);
            double degrees = radians * (180 / Math.PI);
            return degrees;
        }

        public List<double> CalculateAlphas(DataTable dTable, List<double> Ms)
        {
            List<double> Alphas = new List<double>();
            for (int i = 1; i < dTable.Rows.Count; i++)
            {
                Alphas.Add(CalculateAlpha(dTable.Rows[0], dTable.Rows[i], Ms[0], Ms[i]));
            }
            return Alphas;
        }

        public List<double> CalculateAverageM(DataTable dTable)
        {
            List<double> Ms = new List<double>();
            foreach (DataRow row in dTable.Rows)
            {
                double sqrtSumOfSquares = SqrtSumOfSquares(row);
                Ms.Add(sqrtSumOfSquares);
            }
            return Ms;
        }

        public double SqrtSumOfSquares(DataRow row)
        {
            double sumOfSquares = 0;
            foreach (var item in row.ItemArray)
            {
                double value = Convert.ToDouble(item);
                sumOfSquares += value * value;
            }
            return Math.Sqrt(sumOfSquares);
        }

        public double CalculateDmax()
        {
            List<double> Ns = new List<double>();
            List<double> nDiff = new List<double>();
            foreach (DataRow dRow in _storage.DTable.Rows)
            {
                Ns.Add(CalculateMediumN(dRow));
            }
            for (int i = 1; i < _storage.DTable.Rows.Count - 1; i++)
            {
                nDiff.Add(Math.Abs(Ns[i] - Ns[i + 1]));
            }
            return nDiff.Max() / 10;
        }

        public double CalculateMediumN(DataRow row)
        {
            return row.ItemArray.Select(item => Convert.ToDouble(item)).Average();
        }

        public List<double> CalculateNextRow(double Dmax, DataRow previousRow)
        {
            Random rand = new Random();
            var values = previousRow.ItemArray.Select(x => Convert.ToDouble(x)).ToList();
            List<double> nextRow = new List<double>();
            foreach (var value in values)
            {
                double Svi = (rand.NextDouble() * Dmax) - Dmax / 2;
                nextRow.Add(value + Svi);
            }
            return nextRow; 
        }

        public List<double> CalculatePredict(double A, List<double> array)
        {
            double Medium = array.Average();
            List<double> predicts = new List<double>();

            foreach (double elem in array)
            {
                predicts.Add((A * elem) + ((1 - A) * Medium));
            }
            double lastPredict = CalculateLastPredict(A, predicts);
            predicts.Add(lastPredict);
            return predicts;
        }

        public double CalculateLastPredict(double A, List<double> predicts)
        {
            List<double> copyPredicts = new List<double>(predicts);
            double Medium = copyPredicts.Average();
            return A * Medium + (1 - A) * predicts.Last();
        }

        public List<double> CalculateD(List<double> MPlus, List<double> MMinus)
        {
            List<double> D = new List<double>();
            for (int i = 0; i < MPlus.Count; i++)
            {
                double minus = MPlus[i] - MMinus[i];
                double delit = minus / 2;
                D.Add(delit);
            }
            return D;
        }
        public List<double> CalculateL(List<double> Ms) 
        {
            List<double> L = new List<double>();
            for (int i = 0; i < Ms.Count; i++)
            {
                L.Add(Math.Abs(Ms[0] - Ms[i]));
            }
            return L;
        }
    }
}