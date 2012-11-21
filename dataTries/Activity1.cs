using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Data;
using System.IO;
using Mono.Data.Sqlite;
using Mono.Data.Tds;

namespace dataTries
{
	[Activity(Label = "Data Activity", MainLauncher = true, Icon="@drawable/icon")]
	public class Activity1 : Activity
	{
		TextView tv;
		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			SetContentView(Resource.Layout.Main);
			
			tv = FindViewById<TextView>(Resource.Id.Results);
			Button DataBaseButton = FindViewById<Button>(Resource.Id.DataBaseButton);
			Button CreateDataBaseButton = FindViewById<Button>(Resource.Id.CreateLocalDataBase);
			Button PullDataButton = FindViewById<Button>(Resource.Id.GetLocalDataBaseData);

			// TODO less code is usually  beter :)
			/*
			 * You can add EventHandlers with a bit less code:
			 * DataBaseButton.Click += DataBaseButton_Click;
			 */ 

			DataBaseButton.Click += new EventHandler(DataBaseButton_Click);
			CreateDataBaseButton.Click += new EventHandler(CreateDataBaseButton_Click);
			PullDataButton.Click += new EventHandler(PullDataButton_Click);
		}
		
		void PullDataButton_Click(object sender, EventArgs e)
		{

			// TODO Code reuse
			/*
			 * 
			 * The code that is used to communicate with the database is reused (partially) in the CreateDataBaseButton_Click method.
			 * Try to extract a method that handles situations, so that, for instance, if you change you database location, you need to adjust in only one place.
			 */ 

			// TODO code improvement readonly vars
			/*
			 * you could make your DatabaseName a class member, perhaps readonly 
			 * (note: prefer readonly to const!)
			 * 
			 */ 
			string DatabaseName = "UserData.db3";
			string documents = System.Environment.GetFolderPath(
				System.Environment.SpecialFolder.Personal);
			string db = Path.Combine(documents, DatabaseName);//verwijzing naar database
			var conn = new SqliteConnection("Data Source=" + db); //object om db te lezen
			var strSql = "select Name from Customer where STATEID=@STATEID";
			var cmd = new SqliteCommand(strSql, conn);
			cmd.CommandType = CommandType.Text;
			cmd.Parameters.Add(new SqliteParameter("@STATEID", 2));

			try
			{
				conn.Open();

				// TODO code style improvement
				/*
				 * 
				 * Try using var sdr = cmd.ExecuteReader ();
				 * 1. left hand side use var for brevity
				 * 2. C# syntax is written with a space between the method and the parentheses.
				 */ 
				SqliteDataReader sdr = cmd.ExecuteReader();//lezer

				// TODO Your query does not yield any results because there is no customer with stateId 2 
				while (sdr.Read())
				{
					// TODO RunOnUiThread
					 /* tv.Text is not updated because the operation is not running in the UI Thread.
					 * Use RunOnUiThread to solve the problem. :)
					 * Let me know if you don't find out how...
					 */ 
					tv.Text = Convert.ToString(sdr["Name"]);//lezen en weergeven
				}
			}
			catch (System.Exception sysExc)
			{
				tv.Text = sysExc.Message;
			}
			finally
			{
				if (conn.State != ConnectionState.Closed)
				{
					conn.Close();
				}
				conn.Dispose();
			}
		}
		
		void CreateDataBaseButton_Click(object sender, EventArgs e)
		{
			//Database aanmaken
			string DatabaseName = "UserData.db3";
			string documents = System.Environment.GetFolderPath(
				System.Environment.SpecialFolder.Personal);
			string db = Path.Combine(documents, DatabaseName);  
			bool exists = File.Exists(db);  
			if (!exists)  
			{
				SqliteConnection.CreateFile(db);// db maken indien't niet bestaat
			}
			var conn = new SqliteConnection("Data Source=" + db);   //object dat communiceert met database

			 // TODO Yes, the array is a list commands that will be executed below.
			var commands = new[] { //lijst met commando's????
				"DROP TABLE IF EXISTS TWITTERDATA",
				"DROP TRIGGER IF EXISTS TWITTERDATA_INSERT",
				"CREATE TABLE IF NOT EXISTS STATE (STATEID INT PRIMARY KEY, STATENAME VARCHAR(50))",
				"CREATE TABLE IF NOT EXISTS CUSTOMER(CUSTOMERID BIGINT PRIMARY KEY, " +
				"NAME VARCHAR(100), CONTACTNAME VARCHAR(100), DATEJOINED DATETIME, " +
				"PHONE VARCHAR(25), ADDRESS VARCHAR(100), CITY VARCHAR(50), " +
				"STATEID INT, ZIPCODE VARCHAR(25), DATEENTERED DATETIME, " +
				"DATEUPDATED DATETIME, FOREIGN KEY(STATEID) REFERENCES STATE(STATEID))",
				"CREATE TRIGGER IF NOT EXISTS CUSTOMER_INSERT INSERT ON CUSTOMER " +
				"BEGIN UPDATE CUSTOMER SET DATEENTERED=DATE('now') " +
				"WHERE CUSTOMERID=NEW.CUSTOMERID; END;",
				"CREATE INDEX IF NOT EXISTS IDX_CUSTOMERNAME ON CUSTOMER (NAME)",
				"CREATE INDEX IF NOT EXISTS IDX_STATEID ON CUSTOMER (STATEID)",
				"CREATE INDEX IF NOT EXISTS IDX_DATEENTERED ON CUSTOMER (DATEENTERED)",
				"INSERT INTO STATE (STATENAME) VALUES ('TENNESSEE');",
				"INSERT INTO STATE (STATENAME) VALUES ('GEORGIA');"};
			try
			{

				// TODO indentation
				foreach (var cmd in commands)
					using (var sqlitecmd = conn.CreateCommand())
				{//commando's uitvoeren???
					sqlitecmd.CommandText = cmd;
					sqlitecmd.CommandType = CommandType.Text;
					conn.Open();
					sqlitecmd.ExecuteNonQuery();
					conn.Close();
				}
				SqliteCommand sqlc = new SqliteCommand(); //object om commando's door te geven???
				sqlc.Connection = conn;
				conn.Open();
				string strSql = "INSERT INTO CUSTOMER (NAME, " +
					"CONTACTNAME, STATEID) VALUES " +
						"(@NAME, @CONTACTNAME, @STATEID)";
				sqlc.CommandText = strSql;
				sqlc.CommandType = CommandType.Text;
				sqlc.Parameters.Add(new SqliteParameter("@NAME", "The Coca-Cola Company"));
				sqlc.Parameters.Add(new SqliteParameter("@CONTACTNAME", "John Johns"));
				sqlc.Parameters.Add(new SqliteParameter("@STATEID", 1));//parameters voor commando toevoegen
				sqlc.ExecuteNonQuery();
				if (conn.State != ConnectionState.Closed)
				{
					conn.Close();
				}
				conn.Dispose();
				tv.Text = "Commands completed.";
			}
			catch (System.Exception sysExc)
			{
				tv.Text = "Exception: " + sysExc.Message;
			}
		}

		//TODO not sure where you are trying to connect on this one. :)

		void DataBaseButton_Click(object sender, EventArgs e)
		{
			System.Data.SqlClient.SqlConnection sqlCn = new System.Data.SqlClient.SqlConnection();
			System.Data.SqlClient.SqlCommand sqlCm = new System.Data.SqlClient.SqlCommand();
			System.Data.SqlClient.SqlDataAdapter sqlDa = new System.Data.SqlClient.SqlDataAdapter();
			DataTable dt = new DataTable();
			string strSql = "select * from Session";
			string strCn = "Server=mobiledev.scalabledevelopment.com;Database=AnDevConTest;User ID=AnDevCon;Password=AnDevConPWD;Network Library=DBMSSOCN";
			sqlCn.ConnectionString = strCn;
			sqlCm.CommandText = strSql;
			sqlCm.CommandType = CommandType.Text;
			sqlCm.Connection = sqlCn;
			sqlDa.SelectCommand = sqlCm;
			try
			{
				sqlDa.Fill(dt);
				tv.Text = "Records returned: " + dt.Rows.Count.ToString();
			}
			catch (System.Exception sysExc)
			{
				Console.WriteLine("Exc: " + sysExc.Message);
				tv.Text = "Exc: " + sysExc.Message;
			}
			finally
			{
				if (sqlCn.State != ConnectionState.Closed)
				{
					sqlCn.Close();
				}
				sqlCn.Dispose();
				sqlCm.Dispose();
				sqlDa.Dispose();
				sqlCn = null;
				sqlCm = null;
				sqlDa = null;
			}
		}
	}
}






