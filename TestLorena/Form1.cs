using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;

namespace TestLorena
{
    public partial class FormTest : Form
    {
        //массив салонов
        List<SalonModel> salons = new List<SalonModel>();

        //переменные для работы с БД
        private SQLiteConnection sql_con;
        private SQLiteCommand sql_cmd;
        private SQLiteDataAdapter DB;

        //функция соеденения с базой данных
        private void SetConnection()
        {
            sql_con = new SQLiteConnection("Data Source=TestLorena.db;Version=3;New=False;Compress=True;");
        }

        //функция выполнения SQL запросов (INSERT, UPDATE, DELETE)
        private void ExecuteQuery(string txtQuery)
        {
            SetConnection();
            sql_con.Open();
            sql_cmd = sql_con.CreateCommand();
            sql_cmd.CommandText = txtQuery;
            sql_cmd.ExecuteNonQuery();
            sql_con.Close();
        }

        //функция дефолтных значений
        private void DefaultValues()
        {
            salons.Add(new SalonModel { Id = 1, Name = "Миасс", Discount = 4, Addiction = false, Description = "", Parent = 0 });
            salons.Add(new SalonModel { Id = 2, Name = "Амелия", Discount = 5, Addiction = true, Description = "", Parent = 1 });
            salons.Add(new SalonModel { Id = 3, Name = "Тест1", Discount = 2, Addiction = true, Description = "", Parent = 2 });
            salons.Add(new SalonModel { Id = 4, Name = "Тест2", Discount = 0, Addiction = true, Description = "", Parent = 1 });
            salons.Add(new SalonModel { Id = 5, Name = "Курган", Discount = 11, Addiction = false, Description = "", Parent = 0 });
        }



        public FormTest()
        {
            InitializeComponent();

            //запрос данных списка салонов
            SetConnection();
            sql_con.Open();
            sql_cmd = sql_con.CreateCommand();
            string CommandText = "SELECT * FROM `SalonDiscount`";
            DB = new SQLiteDataAdapter(CommandText, sql_con);
            DataSet DS = new DataSet();
            DB.Fill(DS);
            DataTable DT = new DataTable();
            DT = DS.Tables[0];

            //Проверка отсутствия записей в списке салонов
            if (DT.Rows.Count == 0)
            {

                // формирование массива со списком салонов
                DefaultValues();

                //формирование и выполнение запроса на запись списка салонов
                string txtQuery = "INSERT INTO `SalonDiscount` (`Id`, `Name`, `Discount`, `Addiction`, `Description`, `Parent`) VALUES";
                foreach (SalonModel val in salons)
                {
                    txtQuery += " ('" + val.Id + "', '" + val.Name + "', '" + val.Discount + "', '" + val.Addiction + "', '" + val.Description + "', '" + val.Parent + "'),";
                }
                txtQuery = txtQuery.Remove(txtQuery.Length - 1);
                ExecuteQuery(txtQuery);

                //повторный запрос списка салонов
                sql_cmd = sql_con.CreateCommand();
                DB = new SQLiteDataAdapter(CommandText, sql_con);
                DataSet DS1 = new DataSet();
                DB.Fill(DS1);
                DataTable DT1 = new DataTable();
                DT1 = DS1.Tables[0];
                DT = DT1;
            }

            
            //добовление данных из запроса в ComboBox
            comboBoxSalon.ValueMember = "Id";
            comboBoxSalon.DisplayMember = "Name";
            comboBoxSalon.DataSource = DT;
            comboBoxSalon.DropDownStyle = ComboBoxStyle.DropDownList;
            sql_con.Close();
        }
    }
}
