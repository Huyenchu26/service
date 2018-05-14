using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

/// <summary>
/// Summary description for Testing
/// </summary>
public class Testing
{
    Dictionary<string, string[]> fields = new Dictionary<string, string[]> {
        { "timestamp_recv", new string[]{"ukn_dtl_1", "rec_timestamp", "ukn_dtl_2" } },
        { "data", new string[]{"imei", "date_time", "longitude", "latitude", "reserve_0", "reserve_1", "reserve_2", "sos", "trunk", "engine", "status", "gps", "front_cam", "back_cam", "rfid_list", "reserve_3", "pos_status", "firmware", "cpu_time" }}
    };
    
    public Testing()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    private Dictionary<string, string> parse_receive_timestamp(string timestamp_str) {
        string[] date_format_sida = {
            "MM/dd/yyyy hh:mm:ss tt",
            "M/dd/yyyy hh:mm:ss tt",
            "MM/d/yyyy hh:mm:ss tt",
            "M/d/yyyy hh:mm:ss tt",
            "MM/dd/yyyy hh:m:ss tt",
            "MM/dd/yyyy h:mm:ss tt",
            "M/dd/yyyy h:mm:ss tt",
            "MM/d/yyyy h:mm:ss tt",
            "M/d/yyyy hh:mm:ss tt"
        };
        string[] timestamp_part = timestamp_str.Split('-');
        DateTime parsed_date = DateTime.Now;
        foreach (string date_format in date_format_sida)
            if (DateTime.TryParseExact(timestamp_part[1], date_format, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed_date))
                break;
        timestamp_part[1] = parsed_date.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
        Dictionary<string, string> timestamp_recv = new Dictionary<string, string>{ };
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
        foreach (string field in this.fields["data"]) {
            extracted_dict[field] = splitted_data[i].Trim(quotechar);
            i++;
        }
        return extracted_dict;
    }

    public string parse_file(string filepath) // ham nay tra ve json nay

    {
        List<Dictionary<string, Dictionary<string, string>>> file_data = new List<Dictionary<string, Dictionary<string, string>>>();
        var filestream = new System.IO.FileStream(filepath, System.IO.FileMode.Open);
        var file = new System.IO.StreamReader(filestream);
        string line_of_text;
        do // Trong nay can try catch hoac cach nao do de handle loi
        {
            Dictionary<string, Dictionary<string, string>> record = new Dictionary<string, Dictionary<string, string>>();
            line_of_text = file.ReadLine();
            record["timestamp_recv"] = this.parse_receive_timestamp(line_of_text);
            line_of_text = file.ReadLine();
            record["data"] = this.extract_data(line_of_text);
            file_data.Add(record);
        } while (line_of_text != null && line_of_text.Trim('\n', '\r').Length != 0);
        return (new JavaScriptSerializer()).Serialize(file_data);
    }

    public static void Main(string[] args)
    {
        System.IO.File.WriteAllText("D:\\20146203.json", new Testing().parse_file("D:\\260699.txt"));
    }
}