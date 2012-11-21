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
		readonly string DatabaseName = "UserData.db3";//TODO why read only?
		string documents = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			SetContentView(Resource.Layout.Main);
			
			tv = FindViewById<TextView>(Resource.Id.Results);
			Button DataBaseButton = FindViewById<Button>(Resource.Id.DataBaseButton);
			Button CreateDataBaseButton = FindViewById<Button>(Resource.Id.CreateLocalDataBase);
			Button PullDataButton = FindViewById<Button>(Resource.Id.GetLocalDataBaseData);

			DataBaseButton.Click += DataBaseButton_Click;
			CreateDataBaseButton.Click += CreateDataBaseButton_Click;
			PullDataButton.Click += PullDataButton_Click;
		}

		string dbPath ()
		{
			return Path.Combine(documents, DatabaseName); 
		}

		void PullDataButton_Click(object sender, EventArgs e)
		{
			var conn = new SqliteConnection("Data Source=" + dbPath ()); //object om db te lezen
			var strSql = "select Name from Customer where STATEID=@STATEID";
			var cmd = new SqliteCommand(strSql, conn);
			cmd.CommandType = CommandType.Text;
			cmd.Parameters.Add(new SqliteParameter("@STATEID", 1));

			try
			{
				conn.Open();

				var sdr = cmd.ExecuteReader ();//lezer

				while (sdr.Read())
				{
					// TODO RunOnUiThread
					 /* Found out how - not sure why though?
					 */ 
					string name = Convert.ToString(sdr["Name"]);
					RunOnUiThread(() => tv.Text = name);
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
			bool exists = File.Exists(dbPath ());  
			if (!exists)  
			{
				SqliteConnection.CreateFile(dbPath ());// db maken indien't niet bestaat
			}
			var conn = new SqliteConnection("Data Source=" + dbPath ());   //object dat communiceert met database

			//TODO where do these commands come from?
			/* other language with specific syntax?
			*/ 
			var commands = new[] { 
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
				// TODO indentation (like this? or...??)
				foreach (var cmd in commands)
					using (var sqlitecmd = conn.CreateCommand())
				{
					sqlitecmd.CommandText = cmd;
					sqlitecmd.CommandType = CommandType.Text;
					conn.Open();
					sqlitecmd.ExecuteNonQuery();
					conn.Close();
				}

				SqliteCommand sqlc = new SqliteCommand();
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
		/* neither am I comes out of the example in 'Professional Android Programming with Mono for Android and .NET#3aC#'
		 * I don't really understand this bit.
		 */

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






