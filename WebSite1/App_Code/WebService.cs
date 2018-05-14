using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Services;
using System.Xml;
using System.Xml.Linq;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
// [System.Web.Script.Services.ScriptService]
public class WebService : System.Web.Services.WebService
{

    public WebService()
    {

        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
    }

    [WebMethod]
    public string HelloWorld()
    {
        return "Hello World";
    }

    [WebMethod]
    public int Add(int a, int b)
    {
        return a + b;
    }

    [WebMethod]
    public DataTable Get()
    {

        string connString = "SERVER=localhost" + ";" +
                "DATABASE=demo_connect;";

        SqlConnection cnMySQL = new SqlConnection(connString);
        using (SqlCommand cmd = new SqlCommand("SELECT * FROM student"))
        {
            using (SqlDataAdapter sda = new SqlDataAdapter())
            {
                cmd.Connection = cnMySQL;
                sda.SelectCommand = cmd;
                using (DataTable dt = new DataTable())
                {
                    dt.TableName = "student";
                    sda.Fill(dt);
                    return dt;
                    // return DataTableToJsonObj(dt);
                }
            }
        }
    }

    public string DataTableToJsonObj(DataTable dt)
    {
        DataSet ds = new DataSet();
        ds.Merge(dt);
        StringBuilder JsonString = new StringBuilder();
        if (ds != null && ds.Tables[0].Rows.Count > 0)
        {
            JsonString.Append("[");
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                JsonString.Append("{");
                for (int j = 0; j < ds.Tables[0].Columns.Count; j++)
                {
                    if (j < ds.Tables[0].Columns.Count - 1)
                    {
                        JsonString.Append("\"" + ds.Tables[0].Columns[j].ColumnName.ToString() + "\":" + "\"" + ds.Tables[0].Rows[i][j].ToString() + "\",");
                    }
                    else if (j == ds.Tables[0].Columns.Count - 1)
                    {
                        JsonString.Append("\"" + ds.Tables[0].Columns[j].ColumnName.ToString() + "\":" + "\"" + ds.Tables[0].Rows[i][j].ToString() + "\"");
                    }
                }
                if (i == ds.Tables[0].Rows.Count - 1)
                {
                    JsonString.Append("}");
                }
                else
                {
                    JsonString.Append("},");
                }
            }
            JsonString.Append("]");
            return JsonString.ToString();
        }
        else
        {
            return null;
        }
    }

    Dictionary<string, string[]> fields = new Dictionary<string, string[]> {
        { "timestamp_recv", new string[]{
            "ukn_dtl_1",
            "rec_timestamp",
            "ukn_dtl_2" } 
        },
        { "data", new string[]{
            "imei",
            "date_time",
            "longitude",
            "latitude",
            "reserve_0",
            "reserve_1",
            "reserve_2",
            "sos",
            "trunk",
            "engine",
            "status",
            "gps",
            "front_cam",
            "back_cam",
            "rfid_list",
            "reserve_3",
            "pos_status",
            "firmware",
            "cpu_time" }
        }
    };

    private Dictionary<string, string> parse_receive_timestamp(string timestamp_str)
    {
        string[] date_format_sida = {
            "MM/dd/yyyy hh:mm:ss tt",
            "M/dd/yyyy hh:mm:ss tt",
            "MM/d/yyyy hh:mm:ss tt",
            "M/d/yyyy hh:mm:ss tt",
            "MM/dd/yyyy hh:m:ss tt",
            "MM/dd/yyyy h:mm:ss tt",
            "M/dd/yyyy h:mm:ss tt",
            "MM/d/yyyy h:mm:ss tt",
            "M/d/yyyy h:mm:ss tt"
        };

        string[] timestamp_part = timestamp_str.Split('-');
        DateTime parsed_date = DateTime.Now;
        foreach(string date_format in date_format_sida)
            if (DateTime.TryParseExact(timestamp_part[1], date_format, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed_date))
                break;
        timestamp_part[1] = parsed_date.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
        Dictionary<string, string> timestamp_recv = new Dictionary<string, string> { };
        int i = 0;
        foreach (string field in this.fields["timestamp_recv"])
        {
            timestamp_recv[field] = timestamp_part[i];
            i += 1;
        }
        return timestamp_recv;
    }

    public string[] parse_rfid_list(string rfid_list_str)
    {
        List<string> rfid_list = new List<string>();
        Match m = Regex.Match(rfid_list_str, @"[a-fA-F\d]{1,24}");
        while (m.Success)
        {
            rfid_list.Add(m.Value);
            m = m.NextMatch();
        }
        return rfid_list.ToArray();
    }

    private Dictionary<string, string> extract_data(string raw_date, char delim = ',', char quotechar = '"')
    {
        Dictionary<string, string> extracted_dict = new Dictionary<string, string>();
        string[] splitted_data = raw_date.Split(delim);
        int i = 0;
        foreach (string field in this.fields["data"])
        {
            extracted_dict[field] = splitted_data[i].Trim(quotechar);
            i++;
        }
        return extracted_dict;
    }

    [WebMethod]
    public string parse_file() // ham nay tra ve json
    {
        List<Dictionary<string, Dictionary<string, string>>> file_data = new List<Dictionary<string, Dictionary<string, string>>>();
        var filestream = new System.IO.FileStream("D:\\260600.txt", System.IO.FileMode.Open);
        var file = new System.IO.StreamReader(filestream);
        string line_of_text;
        do // Trong nay can try catch hoac cach nao do de handle loi
        {
            Dictionary<string, Dictionary<string, string>> record = new Dictionary<string, Dictionary<string, string>>();
            line_of_text = file.ReadLine();
            if (line_of_text == null || line_of_text.Trim('\n', '\r').Length == 0)
                break;
            record["timestamp_recv"] = this.parse_receive_timestamp(line_of_text);
            line_of_text = file.ReadLine();
            if (line_of_text == null || line_of_text.Trim('\n', '\r').Length == 0)
                break;
            record["data"] = this.extract_data(line_of_text);
            file_data.Add(record);
        } while (true);
        filestream.Close();
        return (new JavaScriptSerializer()).Serialize(file_data);
    }

    // Lam cach nao do ma cai list nay tra ve k phair la string ma la link
    //[WebMethod]
    //public string tryGetListFile(string folder_path)
    //{
    //    var list_files = Directory.GetFiles("D:\\data_for_testing\\");
    //    return // tra ve o day nay phai la link hoac gi do bam duoc de khi bam vao thi trigger cai ham parse_date. T k lam web C# nen k biet.
    //}

}

