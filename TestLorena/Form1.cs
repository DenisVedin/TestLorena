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
        //Массив салонов
        List<SalonModel> salons = new List<SalonModel>();

        //Переменные для работы с БД
        private SQLiteConnection sql_con;
        private SQLiteCommand sql_cmd;
        private SQLiteDataAdapter DB;

        //Функция соеденения с базой данных
        private void SetConnection()
        {
            sql_con = new SQLiteConnection("Data Source=TestLorena.db;Version=3;New=False;Compress=True;");
        }

        //Функция выполнения SQL запросов (INSERT, UPDATE, DELETE)
        private void ExecuteQuery(string txtQuery)
        {
            SetConnection();
            sql_con.Open();
            sql_cmd = sql_con.CreateCommand();
            sql_cmd.CommandText = txtQuery;
            sql_cmd.ExecuteNonQuery();
            sql_con.Close();
        }

        //Функция дефолтных значений
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

            //Запрос данных списка салонов
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

                //Формирование массива со списком салонов
                DefaultValues();

                //Формирование и выполнение запроса на запись списка салонов
                string txtQuery = "INSERT INTO `SalonDiscount` (`Id`, `Name`, `Discount`, `Addiction`, `Description`, `Parent`) VALUES";
                foreach (SalonModel val in salons)
                {
                    txtQuery += " ('" + val.Id + "', '" + val.Name + "', '" + val.Discount + "', '" + val.Addiction + "', '" + val.Description + "', '" + val.Parent + "'),";
                }
                txtQuery = txtQuery.Remove(txtQuery.Length - 1);
                ExecuteQuery(txtQuery);

                //Повторный запрос списка салонов
                sql_cmd = sql_con.CreateCommand();
                DB = new SQLiteDataAdapter(CommandText, sql_con);
                DataSet DS1 = new DataSet();
                DB.Fill(DS1);
                DataTable DT1 = new DataTable();
                DT1 = DS1.Tables[0];
                DT = DT1;
            }

            
            //Добовление данных из запроса в ComboBox
            comboBoxSalon.ValueMember = "Id";
            comboBoxSalon.DisplayMember = "Name";
            comboBoxSalon.DataSource = DT;
            comboBoxSalon.DropDownStyle = ComboBoxStyle.DropDownList;
            sql_con.Close();
        }

        //Блокировка ввода любых символов кроме цифр в поле "Стоимость"
        private void textBoxPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }
            
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        //Функция получения скидки предка с рекурсивным прохождением до корня дерева
        private static int GetParentDiscount(int Id, bool Addiction, int DiscountParent, SQLiteConnection sql_con)
        {
            if (Addiction)
            {
                SQLiteCommand sql_cmd1 = sql_con.CreateCommand();
                string CommandText = "SELECT * FROM `SalonDiscount` WHERE `Id` = '" + Id + "'";
                SQLiteDataAdapter DB1 = new SQLiteDataAdapter(CommandText, sql_con);
                DataSet DS3 = new DataSet();
                DB1.Fill(DS3);
                DataTable DT3 = new DataTable();
                DT3 = DS3.Tables[0];
                foreach (DataRow DR in DT3.Rows)
                {
                    DiscountParent += Convert.ToInt32(DR["Discount"]);
                    DiscountParent = GetParentDiscount(Convert.ToInt32(DR["Parent"]), Convert.ToBoolean(DR["Addiction"]), DiscountParent, sql_con);
                }
            }

            return DiscountParent;
        }

        //Клик на кнопку "Расчитать"
        private void buttonCalculate_Click(object sender, EventArgs e)
        {
            //Проверка поля "Стоимость" на пустоту
            if (textBoxPrice.Text.ToString() == "")
            {
                //Если поле "Стоимость" пусто, то выводить сообщение об ошибке
                MessageBox.Show("Ошибка! Заполните поле 'Стоимость'");
            }
            else
            {
                //Если поле "Стоимость" заполнено, то выполняется запрос данных о конкретном салоне выбранного из списка
                //расчёт стоимости и запись в таблицы
                sql_con.Open();
                sql_cmd = sql_con.CreateCommand();
                string CommandText = "SELECT * FROM `SalonDiscount` WHERE `Id` = '" + comboBoxSalon.SelectedValue.ToString() + "'";
                DB = new SQLiteDataAdapter(CommandText, sql_con);
                DataSet DS2 = new DataSet();
                DB.Fill(DS2);
                DataTable DT2 = new DataTable();
                DT2 = DS2.Tables[0];
                int Discount = 0;
                int DiscountParent = 0;
                int Price = 0;
                double S = 0.0;
                string txtQuery = "";
                foreach (DataRow DR in DT2.Rows)
                {
                    //Скидка
                    Discount = Convert.ToInt32(DR["Discount"]);
                    //Цена
                    Price = Convert.ToInt32(textBoxPrice.Text.ToString());

                    //Запрос скидки предков
                    DiscountParent = GetParentDiscount(Convert.ToInt32(DR["Parent"]), Convert.ToBoolean(DR["Addiction"]), DiscountParent, sql_con);
                    //Расчёт по формуле
                    S = (double)Price - ((double)Price * (((double)Discount + (double)DiscountParent) / 100));
                    //Округление результата до сотых
                    S = Math.Round(S, 2);

                    //Запись в таблицу для восставновления расчётов по формуле
                    txtQuery = "INSERT INTO `InfoFormula` (`Id`, `Price`, `Discount`, `DiscountParent`, `Sum`) VALUES " +
                        "(NULL, '" + Price.ToString().Replace(",", ".") + "', '" + Discount.ToString().Replace(",", ".") + "', '" + DiscountParent.ToString().Replace(",", ".") + "', '" + S.ToString().Replace(",", ".") + "')";
                    ExecuteQuery(txtQuery);

                    //Запись в таблицу результатов для вывода пользователю
                    txtQuery = "INSERT INTO `ResultSelected` (`Id`, `Selected`, `Price`, `Sum`) VALUES " +
                        "(NULL, '" + DR["Name"].ToString() + "', '" + Price.ToString().Replace(",", ".") + "', '" + S.ToString().Replace(",", ".") + "')";
                    ExecuteQuery(txtQuery);
                }
                sql_con.Close();

            }
        }
    }
}
