using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for VehicleModel
/// </summary>
public class VehicleModel
{
    public VehicleModel()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public static Dictionary<string, string[]> Vehicle = new Dictionary<string, string[]> {
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
}