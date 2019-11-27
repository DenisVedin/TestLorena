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
        
        private SQLiteConnection sql_con;
        private SQLiteCommand sql_cmd;
        private SQLiteDataAdapter DB;

        //set connection
        private void SetConnection()
        {
            sql_con = new SQLiteConnection("Data Source=TestLorena.db;Version=3;New=False;Compress=True;");
        }

        //set execute query
        private void ExecuteQuery(string txtQuery)
        {
            SetConnection();
            sql_con.Open();
            sql_cmd = sql_con.CreateCommand();
            sql_cmd.CommandText = txtQuery;
            sql_cmd.ExecuteNonQuery();
            sql_con.Close();
        }

        public FormTest()
        {
            InitializeComponent();
        }
    }
}
