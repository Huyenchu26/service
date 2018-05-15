using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Services;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using Newtonsoft.Json;

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

    private Dictionary<string, string> parse_receive_timestamp(string timestamp_str)
    {
        string[] date_format_list = {
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
        foreach(string date_format in date_format_list)
            if (DateTime.TryParseExact(timestamp_part[1], date_format, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed_date))
                break;
        timestamp_part[1] = parsed_date.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
        Dictionary<string, string> timestamp_recv = new Dictionary<string, string> { };
        int i = 0;
        foreach (string field in VehicleModel.Vehicle["timestamp_recv"])
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
        foreach (string field in VehicleModel.Vehicle["data"])
        {
            extracted_dict[field] = splitted_data[i].Trim(quotechar);
            i++;
        }
        return extracted_dict;
    }

    //get list file from pathfile
    public string[] ProcessDirectory(string pathfile)
    {
        // get list file in folder
        string[] fileList = Directory.GetFiles(pathfile);
        string[] ListFileName = new string[fileList.Length];

        for (int i = 0; i < fileList.Length; i++)
        {
            ListFileName[i] = Path.GetFileName(fileList[i]).Trim();
        }
        return ListFileName;
    }


    // return json object
    [WebMethod]
    public List<String> parse_file() 
    {
        List<String> listJson = new List<string>();
        string[] listFile = ProcessDirectory(Config.PathFile);
        for (int i = 0; i < listFile.Length; i++)
        {
            List<Dictionary<string, Dictionary<string, string>>> file_data = new List<Dictionary<string, Dictionary<string, string>>>();
            var filestream = new System.IO.FileStream(Config.PathFile + listFile[i], System.IO.FileMode.Open);
            var file = new System.IO.StreamReader(filestream);
            string line_of_text;
            do 
                // chua handle error
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

            listJson.Add(JsonConvert.SerializeObject(file_data));

            //listJson.Add((new JavaScriptSerializer()).Serialize(file_data));
            //return (new JavaScriptSerializer()).Serialize(file_data);
        }
        return listJson;
    }
}

