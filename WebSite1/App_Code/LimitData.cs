using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

/// <summary>
/// Summary description for LimitData
/// </summary>
public class LimitData
{
    public LimitData()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public static Dictionary<string, string> extract_data_begin(string timestamp_str)
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
        foreach (string date_format in date_format_list)
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


    public static Dictionary<string, string> extract_data(string raw_date, char delim = ',', char quotechar = '"')
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


    // convert to timestamp
    public static String DateToTimestamp(String date) {

        DateTime currentDate = DateTime.Now;
        Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(DateTime.Now)).TotalSeconds;

        return "";
    }

}