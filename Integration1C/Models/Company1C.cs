﻿using System;
using System.Collections.Generic;

namespace Integration1C
{
    public class Company1C
    {
#pragma warning disable IDE1006 // Naming Styles
        public Guid? company_id_1C { get; set; }
        public List<Amo_id> amo_ids { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string signee { get; set; }
        public string OGRN { get; set; }
        public string INN { get; set; }
        public string acc_no { get; set; }
        public string KPP { get; set; }
        public string BIK { get; set; }
        public string address { get; set; }
        public string post_address { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
}