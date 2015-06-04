using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace SSRSUpdateReports
{
    class Program
    {
        static void Main(string[] args)
        {
            // Connection Setup
            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = "Data Source=[DataSourceHere]; Initial Catalog=ReportServer; Trusted_Connection=True;";

                conn.Open();

                if (conn.State != ConnectionState.Open)
                    Console.WriteLine("Connection to database failed!");
                else
                    Console.WriteLine("Connection to Database Success.");


                Console.WriteLine("");
                Console.WriteLine("Continue? (Y/N)");
                string continueAnswer = Console.ReadLine();

                if (continueAnswer == "N")
                {
                    // Quit the application
                    Environment.Exit(0);
                }


                //SSMS Query: SELECT ItemID,Name,Content,Type FROM Catalog WHERE Type = 2
                SqlCommand getReportCatalogContentCommand = new SqlCommand("SELECT ItemID,Name,Content,Type,Path FROM Catalog WHERE Type = 2", conn);

                getReportCatalogContentCommand.Parameters.Add(new SqlParameter("0", 1));

                using (SqlDataReader reader = getReportCatalogContentCommand.ExecuteReader())
                {
                    Console.WriteLine("SSRS Tool : Extract Report XML for all reports, edit and insert.");

                    while (reader.Read())
                    {
                        // Load your reports into a datatable
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        // For each report item write the UniqueId and Content
                        foreach (DataRow dr in dt.Rows)
                        {
                            Console.WriteLine("Item: " + dr[0] + "\t" + dr[1]);

                            // Pass that report item into conversion method
                            CreateFileConvertData(dr);

                            // Lets close our connection till we want to update our xml data
                            conn.Close();
                        }

                        Console.WriteLine("Your Reports were sucessfully decoded and stored in C:\\MyTmpXMLReports\\");
                        Console.WriteLine("--------------");
                        Console.WriteLine("You can now edit the XML in each report text file");
                        Console.WriteLine("--------------");
                        Console.WriteLine("Ready to insert your changes back to the Report Server? (Y/N)");
                        string answer = Console.ReadLine();

                        if (answer == "Y")
                        {
                            InsertXMLChangesBackIntoDB();
                        }
                        else
                            Console.WriteLine("Changes were not inserted into the ReportServer DB");

                        // Quit the application
                        Environment.Exit(0);

                        Console.ReadLine();
                        Console.Clear();

                        return;
                    }
                }
            }
        }

        // After XML changes have been made we save and insert those changes back into their respective positions in the report catalog
        private static void InsertXMLChangesBackIntoDB()
        {
            string path = @"C:\MyTmpXMLReports\";

            string[] reportName = Directory.GetFiles(path);

            // Go through each report item 
            foreach (string fileName in Directory.GetFiles(path).Select(Path.GetFileNameWithoutExtension))
            {
                Console.WriteLine(fileName);
                string uniqueIdentifier = fileName;

                string contents = System.IO.File.ReadAllText(path + fileName + ".txt");

                byte[] _currentXML = ConvertStringToByte(contents);

                using (SqlConnection conn = new SqlConnection())
                {
                    conn.ConnectionString = "Data Source=[DataSourceHere]; Initial Catalog=ReportServer; Trusted_Connection=True;";

                    using (SqlCommand command = conn.CreateCommand())
                    {
                        conn.Open();

                        command.Parameters.AddWithValue("@Content", _currentXML);
                        command.Parameters.AddWithValue("@ItemID", fileName);
                        command.CommandText = "UPDATE Catalog SET Content = @Content WHERE ItemID = @ItemID";

                        command.ExecuteNonQuery();

                    }

                    Console.WriteLine("Status: Update Query Ran Sucessfully.");
                    Console.WriteLine("");

                    conn.Close();
                }

                Array.Clear(_currentXML, 0, _currentXML.Length);
            }

            Console.WriteLine("SSRS Reports Update Successful!");

        }

        private static byte[] ConvertStringToByte(string Input)
        {
            return System.Text.Encoding.UTF8.GetBytes(Input);
        }

        public static void CreateFileConvertData(DataRow dr)
        {
            var _reportItemId = dr[0];
            var _reportName = dr[1];
            var _currentContent = dr[2];
            var _reportServerPath = dr[4];

            // Convert byte[] to string
            Byte[] _currentByteArrary = (byte[])dr[2];

            string output = ConvertByteArrayToString(_currentByteArrary);

            Console.WriteLine("");

            string path = @"C:\MyTmpXMLReports\";


            File.Create(path + _reportItemId + ".txt").Close();

            StreamWriter file = new StreamWriter(path + _reportItemId + ".txt");
            file.WriteLine(output);
            file.Close();

        }

        // Pass in the Byte Array and perform the conversion
        private static string ConvertByteArrayToString(Byte[] _currentByteArrary)
        {
            string StringOutput = System.Text.Encoding.UTF8.GetString(_currentByteArrary);
            return StringOutput;
        }

    }
}
