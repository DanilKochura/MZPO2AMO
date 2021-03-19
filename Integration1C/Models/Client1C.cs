﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace Integration1C
{
    public class Client1C
    {
#pragma warning disable IDE1006 // Naming Styles
        public Guid? client_id_1C { get; set; }
        public List<Amo_id> amo_ids { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string name { get; set; }
        [JsonConverter(typeof(DateFormatConverter))]
        public DateTime? dob { get; set; }
        public string pass_serie { get; set; }
        public string pass_number { get; set; }
        public string pass_issued_by { get; set; }
        public string pass_issued_at { get; set; }
        public string pass_dpt_code { get; set; }
#pragma warning restore IDE1006 // Naming Styles    

        public class DateFormatConverter : IsoDateTimeConverter
        {
            public DateFormatConverter()
            {
                DateTimeFormat = "dd.MM.yyyy H:mm:ss";
            }
        }
    }
}