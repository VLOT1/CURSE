using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CURSE
{
    internal class DataStorage
    {
        private DBClass DBClass;    

        public Image image;
        public double A;
        public double E;
        public DataTable DTable;
        public DataTable DTableCalc;
        public int blocks;
        public List<bool> isStable;
        public DataStorage(DBClass DBClass)
        {
            this.DBClass = DBClass;
            DTable = new DataTable();
            GetDataFromDatabase();
        }
        private void GetDataFromDatabase()
        {
            image = DBClass.PictureFromDatabase();
            DBClass.AdditionalDataFromDatabase(out A, out E, out blocks);
            DTable = DBClass.LoadDataTableFromDatabase("Данные");
            isStable = new List<bool>();
        }
    }
}
