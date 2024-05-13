using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CURSE
{
    internal class FDL
    {
        private DataStorage storage;
        private Calculations calculations;
        public int rowCount;

        public List<double> ms;
        public List<double> msPlus;
        public List<double> msMinus;

        public List<double> msCPr;
        public List<double> msCPrPlus;
        public List<double> msCPrMinus;

        public List<double> alphas;
        public List<double> alphasPlus;
        public List<double> alphasMinus;

        public List<double> alphasC;
        public List<double> alphasPlusC;
        public List<double> alphasMinusC;

        public List<double> alphasCPr;
        public List<double> alphasPlusCPr;
        public List<double> alphasMinusCPr;

        public List<List<double>> MsPredicts;
        public List<List<double>> MsPlusPredicts;
        public List<List<double>> MsMinusPredicts;

        public List<List<double>> AlphasPredicts;
        public List<List<double>> AlphasPlusPredicts;
        public List<List<double>> AlphasMinusPredicts;


        public List<double> mPredicts;
        public List<double> mPlusPredicts;
        public List<double> mMinusPredicts;

        List<double> AlphaPredicts;
        List<double> AlphaPredictsPlus;
        List<double> AlphaPredictsMinus;

        List<double> Ls;
        List<double> Ds;

        public FDL(Calculations calculations, DataStorage storage) { 
            this.storage = storage;
            this.calculations = calculations;
        }
        public void calculateFDL(DataTable DTableCalc, double A)
        {
            rowCount = DTableCalc.Rows.Count;
            DataTable MPlusTable = calculations.CalculateMPlus(DTableCalc);
            DataTable MMinusTable = calculations.CalculateMMinus(DTableCalc);
            ms = calculations.CalculateAverageM(DTableCalc);
            msPlus = calculations.CalculateAverageM(MPlusTable);
            msMinus = calculations.CalculateAverageM(MMinusTable);

            alphas = calculations.CalculateAlphas(DTableCalc, ms);
            alphasPlus = calculations.CalculateAlphas(MPlusTable, msPlus);
            alphasMinus = calculations.CalculateAlphas(MMinusTable, msMinus);
            if (double.IsNaN(alphasMinus.Last()))
            {
                alphasMinus[alphasMinus.Count - 1] = 0;
            }

            mPredicts = calculations.CalculatePredict(A, ms);
            mPlusPredicts = calculations.CalculatePredict(A, msPlus);
            mMinusPredicts = calculations.CalculatePredict(A, msMinus);

            AlphaPredicts = calculations.CalculatePredict(A, alphas);
            AlphaPredictsPlus = calculations.CalculatePredict(A, alphasPlus);
            AlphaPredictsMinus = calculations.CalculatePredict(A, alphasMinus);

            AlphasToCharts();
            predictsToCharts();
            GetLD(out Ls, out Ds);
            AllAPredicts();
        }
        public void AllAPredicts()
        {
            MsPredicts = new List<List<double>>();
            MsPlusPredicts = new List<List<double>>();
            MsMinusPredicts = new List<List<double>>();
            AlphasPredicts = new List<List<double>>();
            AlphasPlusPredicts = new List<List<double>>();
            AlphasMinusPredicts = new List<List<double>>();

            var msCopy = new List<double>(ms);
            var msPlusCopy = new List<double>(msPlus);
            var msMinusCopy = new List<double>(msMinus);
            var alphasCopy = new List<double>(alphas);
            var alphasPlusCopy = new List<double>(alphasPlus);
            var alphasMinusCopy = new List<double>(alphasMinus);

            MsPredicts.Add(calculations.CalculatePredict(0.1, msCopy));
            MsPredicts.Add(calculations.CalculatePredict(0.4, msCopy));
            MsPredicts.Add(calculations.CalculatePredict(0.7, msCopy));
            MsPredicts.Add(calculations.CalculatePredict(0.9, msCopy));

            MsPlusPredicts.Add(calculations.CalculatePredict(0.1, msPlusCopy));
            MsPlusPredicts.Add(calculations.CalculatePredict(0.4, msPlusCopy));
            MsPlusPredicts.Add(calculations.CalculatePredict(0.7, msPlusCopy));
            MsPlusPredicts.Add(calculations.CalculatePredict(0.9, msPlusCopy));

            MsMinusPredicts.Add(calculations.CalculatePredict(0.1, msMinusCopy));
            MsMinusPredicts.Add(calculations.CalculatePredict(0.4, msMinusCopy));
            MsMinusPredicts.Add(calculations.CalculatePredict(0.7, msMinusCopy));
            MsMinusPredicts.Add(calculations.CalculatePredict(0.9, msMinusCopy));

            AlphasPredicts.Add(calculations.CalculatePredict(0.1, alphasCopy));
            AlphasPredicts.Add(calculations.CalculatePredict(0.4, alphasCopy));
            AlphasPredicts.Add(calculations.CalculatePredict(0.7, alphasCopy));
            AlphasPredicts.Add(calculations.CalculatePredict(0.9, alphasCopy));

            AlphasPlusPredicts.Add(calculations.CalculatePredict(0.1, alphasPlusCopy));
            AlphasPlusPredicts.Add(calculations.CalculatePredict(0.4, alphasPlusCopy));
            AlphasPlusPredicts.Add(calculations.CalculatePredict(0.7, alphasPlusCopy));
            AlphasPlusPredicts.Add(calculations.CalculatePredict(0.9, alphasPlusCopy));

            AlphasMinusPredicts.Add(calculations.CalculatePredict(0.1, alphasMinusCopy));
            AlphasMinusPredicts.Add(calculations.CalculatePredict(0.4, alphasMinusCopy));
            AlphasMinusPredicts.Add(calculations.CalculatePredict(0.7, alphasMinusCopy));
            AlphasMinusPredicts.Add(calculations.CalculatePredict(0.9, alphasMinusCopy));
        }

        public void predictsToCharts()
        {
            msCPr = new List<double>(ms);
            msCPrPlus = new List<double>(msPlus);
            msCPrMinus = new List<double>(msMinus);

            msCPr.Add(mPredicts.Last());
            msCPrPlus.Add(mPlusPredicts.Last());
            msCPrMinus.Add(mMinusPredicts.Last());

            alphasCPr = new List<double>(alphas);
            alphasPlusCPr = new List<double>(alphasPlus);
            alphasMinusCPr = new List<double>(alphasMinus);

            alphasCPr.Add(AlphaPredicts.Last());
            alphasPlusCPr.Add(AlphaPredictsPlus.Last());
            alphasMinusCPr.Add(AlphaPredictsMinus.Last());
        }
        public List<double> AddZeroToStart(List<double> doubles)
        {
            doubles.Insert(0, 0);
            return doubles;
        }
        public void AlphasToCharts()
        {
            alphasC = AddZeroToStart(alphas);
            alphasPlusC = AddZeroToStart(alphasPlus);
            alphasMinusC = AddZeroToStart(alphasMinus);
        }
        public DataTable GetFDLMainTable()
        {
            DataTable FDLMainTable = new DataTable();
            FDLMainTable.Columns.Add("Цикл наблюдений");

            FDLMainTable.Columns.Add("Ms");
            FDLMainTable.Columns.Add("MPredicts");
            FDLMainTable.Columns.Add("Alphas");
            FDLMainTable.Columns.Add("AlphaPredicts");

            FDLMainTable.Columns.Add("MsPlus");
            FDLMainTable.Columns.Add("MPlusPredicts");
            FDLMainTable.Columns.Add("AlphasPlus");
            FDLMainTable.Columns.Add("AlphaPredictsPlus");

            FDLMainTable.Columns.Add("MsMinus");
            FDLMainTable.Columns.Add("MMinusPredicts");
            FDLMainTable.Columns.Add("AlphasMinus");
            FDLMainTable.Columns.Add("AlphaPredictsMinus");

            int maxCount = Math.Max(ms.Count, Math.Max(alphas.Count, mPredicts.Count));

            for (int i = 0; i < maxCount; i++)
            {
                object[] row = new object[13];

                if (i < storage.DTable.Rows.Count)
                    row[0] = storage.DTable.Rows[i][0];
                if (i < ms.Count)
                    row[1] = Math.Round(ms[i], 6);
                if (i < mPredicts.Count)
                    row[2] = Math.Round(mPredicts[i], 6);
                if (i < alphas.Count)
                    row[3] = Math.Round(alphas[i], 6);
                if (i < AlphaPredicts.Count)
                    row[4] = Math.Round(AlphaPredicts[i], 6);

                if (i < msPlus.Count)
                    row[5] = Math.Round(msPlus[i], 6);
                if (i < mPlusPredicts.Count)
                    row[6] = Math.Round(mPlusPredicts[i], 6);
                if (i < alphasPlus.Count)
                    row[7] = Math.Round(alphasPlus[i], 6);
                if (i < AlphaPredictsPlus.Count)
                    row[8] = Math.Round(AlphaPredictsPlus[i], 6);

                if (i < ms.Count)
                    row[9] = Math.Round(msMinus[i], 6);
                if (i < mMinusPredicts.Count)
                    row[10] = Math.Round(mMinusPredicts[i], 6);
                if (i < alphasMinus.Count)
                    row[11] = Math.Round(alphasMinus[i], 6);
                if (i < AlphaPredictsMinus.Count)
                    row[12] = Math.Round(AlphaPredictsMinus[i], 6);

                FDLMainTable.Rows.Add(row);
            }
            return FDLMainTable;
        }
        public DataTable GetFDLStateTable()
        {
            DataTable FDLStateTable = new DataTable();

            FDLStateTable.Columns.Add("Цикл наблюдений");

            FDLStateTable.Columns.Add("R");
            FDLStateTable.Columns.Add("L");
            FDLStateTable.Columns.Add("Состояние");

            for (int i = 0; i < rowCount+1; i++)
            {
                object[] row = new object[4];
                if (i < storage.DTable.Rows.Count)
                    row[0] = storage.DTable.Rows[i][0];
                if (i < Ls.Count)
                    row[1] = Ls[i];
                if (i < Ds.Count)
                    row[2] = Ds[i];
                if (Ls[i] < Ds[i])
                {
                    row[3] = "Неизменяемое";
                    storage.isStable.Add(true);
                }
                else
                {
                    row[3] = "Изменяемое";
                    storage.isStable.Add(false);
                }

                FDLStateTable.Rows.Add(row);
            }
            return FDLStateTable;
        }
        public void GetLD(out List<double> Ls, out List<double> Ds)
        {
            Ls = calculations.CalculateL(msCPr);
            Ds = calculations.CalculateD(msCPrPlus, msCPrMinus);
        }
    }
}
