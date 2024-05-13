using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace CURSE
{
    internal class DBClass
    {
        private SQLiteConnection SQLiteConn;
        private SQLiteDataAdapter adapter;
        public DBClass()
        {
            SQLiteConn = new SQLiteConnection();
        }
        public Image PictureFromDatabase()
        {
            try
            {
                string query = "SELECT Picture FROM " + "Picture";
                SQLiteCommand command = new SQLiteCommand(query, SQLiteConn);
                command.Parameters.AddWithValue("0", 1);

                byte[] imageData = (byte[])command.ExecuteScalar();

                if (imageData != null && imageData.Length > 0)
                {
                    using (MemoryStream ms = new MemoryStream(imageData))
                    {
                        return Image.FromStream(ms);    
                    }
                }
                else
                {
                    MessageBox.Show("Нет данных об изображении для выбранной записи.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке изображения из базы данных: " + ex.Message);
                return null;
            }
        }
        public void AdditionalDataFromDatabase(out double A, out double E, out int blocks)
        {
            A = 1;
            E = 1;
            blocks = 1;

                string query = "SELECT * FROM Picture";
                SQLiteCommand command = new SQLiteCommand(query, SQLiteConn);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        try { A = double.Parse(reader.GetString(reader.GetOrdinal("A"))); }
                        catch { A = reader.GetDouble(reader.GetOrdinal("A")); }

                        try { E = double.Parse(reader.GetString(reader.GetOrdinal("E"))); }
                        catch { E = reader.GetDouble(reader.GetOrdinal("E")); }
                         try { blocks = reader.GetInt32  (reader.GetOrdinal("blocks")); }
                            catch { blocks = reader.GetInt32(reader.GetOrdinal("blocks")); }

                }
                }
        }

        public DataTable LoadDataTableFromDatabase(string name)
        {

            try
            {
                string query = $"SELECT * FROM {name};";
                adapter = new SQLiteDataAdapter(query, SQLiteConn);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных из базы данных: " + ex.Message);
                return null;
            }
        }
        public void SaveImageToDatabase(Image image)
        {
            try
            {
                byte[] imageData;
                if (image != null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        image.Save(ms, image.RawFormat);
                        imageData = ms.ToArray();
                    }

                    string query = "UPDATE Picture SET Picture = @Picture";
                    SQLiteCommand command = new SQLiteCommand(query, SQLiteConn);
                    command.Parameters.AddWithValue("@Picture", imageData);
                    command.ExecuteNonQuery();
                }
                else
                {
                    MessageBox.Show("Выберите изображение перед сохранением.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении изображения в базе данных: " + ex.Message);
            }
        }
        public void SaveParametersToDatabase(double A, double E, int blocks)
        {
            try
            {
                string query = "UPDATE Picture SET A = @A, E = @E, blocks = @blocks";
                SQLiteCommand command = new SQLiteCommand(query, SQLiteConn);
                command.Parameters.AddWithValue("@A", A);
                command.Parameters.AddWithValue("@E", E);
                command.Parameters.AddWithValue("@blocks", blocks);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении параметров в базе данных: " + ex.Message);
            }
        }
        private void SaveTableToDatabase(DataTable dTable)
        {
            try
            {
                // указываем что адаптер должен использовать MissingSchemaAction для добавления ключей
                adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;

                // создаем экземпляр SQLiteCommandBuilder для автоматической генерации команды обновления
                SQLiteCommandBuilder commandBuilder = new SQLiteCommandBuilder(adapter);

                // получаем команду обновления из commandBuilder
                adapter.UpdateCommand = commandBuilder.GetUpdateCommand();

                // применяем все изменения из DataTable к базе данных
                adapter.Update(dTable);

                MessageBox.Show("Изменения сохранены в базе данных.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении изменений в базе данных: " + ex.Message);
            }
        }
        public void ConnectToDatabase(string dbFilePath)
        {
            try
            {
                string connectionString = $"Data Source={dbFilePath};Version=3;";
                SQLiteConn.ConnectionString = connectionString;
                SQLiteConn.Open();
                MessageBox.Show("Соединение с базой данных установлено успешно.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при подключении к базе данных: " + ex.Message);
            }
        }
        public void SaveChangesToDatabase(Image image, double A, double E, int blocks, DataTable DTable)
        {
            SaveImageToDatabase(image);
            SaveParametersToDatabase(A, E, blocks);
            SaveTableToDatabase(DTable);
        }
        public void DeleteRowFromDatabase(int epochValue)
        {
            string query = "DELETE FROM Данные WHERE Эпоха = @EpochValue";

            using (SQLiteCommand command = new SQLiteCommand(query, SQLiteConn))
            {
                command.Parameters.AddWithValue("@EpochValue", epochValue);
                command.ExecuteNonQuery();
            }
        }
        public void ReplaceCommasWithDotsInDatabase(string name)
        {
            try
            {
                string query = "UPDATE " + name +" SET ";
                for (int i = 1; i <= Form1.cycleAmount; i++)
                {
                    query += $"\"{i}\" = REPLACE(\"{i}\", ',', '.')";
                    if (i < 27)
                        query += ",";
                }

                using (SQLiteCommand command = new SQLiteCommand(query, SQLiteConn))
                {
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при выполнении замены знаков в бд: " + ex.Message);
            }
        }
        public void ReplaceSemicolonsWithDotsInDatabase(string name)
        {
            try
            {
                string query = "UPDATE " + name + " SET ";
                for (int i = 1; i <= Form1.cycleAmount; i++)
                {
                    query += $"\"{i}\" = REPLACE(\"{i}\", ';', '.')";
                    if (i < 27)
                        query += ",";
                }

                using (SQLiteCommand command = new SQLiteCommand(query, SQLiteConn))
                {
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при выполнении замены знаков в бд: " + ex.Message);
            }
        }
        public List<string> GetTableNames()
        {
            List<string> tableNames = new List<string>();
            string query = "SELECT name FROM sqlite_master WHERE type='table';";

            using (SQLiteCommand command = new SQLiteCommand(query, SQLiteConn))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tableNames.Add(reader.GetString(0));
                    }
                }
            }

            return tableNames;
        }
    }
}
