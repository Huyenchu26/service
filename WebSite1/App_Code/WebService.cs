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


    // return lastest record
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
                record["timestamp_recv"] = LimitData.extract_data_begin(line_of_text);
                line_of_text = file.ReadLine();
                if (line_of_text == null || line_of_text.Trim('\n', '\r').Length == 0)
                    break;
                record["data"] = LimitData.extract_data(line_of_text);
                file_data.Clear();
                file_data.Add(record);
            } while (true);
            filestream.Close();
            
            listJson.Add(JsonConvert.SerializeObject(file_data));

            //listJson.Add((new JavaScriptSerializer()).Serialize(file_data));
            //return (new JavaScriptSerializer()).Serialize(file_data);
        }

        return listJson;
    }


    // input khoảng thời gian
    [WebMethod]
    public String getHistory(String imei, String startDate, String endDate)
    {
        List<Dictionary<string, Dictionary<string, string>>> file_data = new List<Dictionary<string, Dictionary<string, string>>>();
        //List<String> listJson = new List<string>();
        String[] imeiArray = ProcessDirectory(Config.PathFile);
        for (int i = 0; i < imeiArray.Length; i++)
        {
            if (imeiArray[i].Split('.')[0].Equals(imei))
            {

                //List<Dictionary<string, Dictionary<string, string>>> file_data = new List<Dictionary<string, Dictionary<string, string>>>();
                var filestream = new System.IO.FileStream(Config.PathFile + imeiArray[i], System.IO.FileMode.Open);
                var file = new System.IO.StreamReader(filestream);
                string line_of_text;
                do
                // chua handle error
                {
                    Dictionary<string, Dictionary<string, string>> record = new Dictionary<string, Dictionary<string, string>>();
                    line_of_text = file.ReadLine();
                    if (line_of_text == null || line_of_text.Trim('\n', '\r').Length == 0)
                        break;
                    record["timestamp_recv"] = LimitData.extract_data_begin(line_of_text);
                    line_of_text = file.ReadLine();
                    if (line_of_text == null || line_of_text.Trim('\n', '\r').Length == 0)
                        break;
                    record["data"] = LimitData.extract_data(line_of_text);

                    DateTime curDate = stringToDate(line_of_text.Split(',')[1]);

                    if (DateToTimestamp(curDate, stringToDate(startDate)) >= 0
                        && DateToTimestamp(stringToDate(endDate), curDate) >= 0)
                    {
                        file_data.Add(record);
                    }


                } while (true);
                filestream.Close();

                //listJson.Add(JsonConvert.SerializeObject(file_data));
            }
            else System.Diagnostics.Debug.WriteLine("Huyenchu imeiArray: " + imeiArray[0].ToString() + " " + imeiArray[1].ToString());
        }
        return JsonConvert.SerializeObject(file_data);
    }


    // Count seconds between two date
    public double DateToTimestamp(DateTime startTime, DateTime endTime)
    {
        var diffInSeconds = (startTime - endTime).TotalSeconds;
        return diffInSeconds;
    }

    // convert string to datetime

    public DateTime stringToDate(String str)
    {
        return DateTime.ParseExact(str, "yyyy/MM/dd HH:mm:ss",
                                       System.Globalization.CultureInfo.InvariantCulture);
    }

}

