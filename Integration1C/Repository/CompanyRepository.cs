﻿using Newtonsoft.Json;
using System;
using System.Net;

namespace Integration1C
{
    internal class CompanyRepository
    {
        internal Company1C GetCompany(Company1C company) => GetCompany(company.company_id_1C);

        internal Company1C GetCompany(Guid company_id)
        {
            string uri = $"http://94.230.11.182:50080/uuc/hs/courses/EditPartner?id={company_id.ToString("D")}";
            Request1C request = new("GET", uri);
            
            Company1C result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }

        internal Company1C UpdateCompany(Company1C company)
        {
            string uri = "http://94.230.11.182:50080/uuc/hs/courses/EditPartner";
            string content = JsonConvert.SerializeObject(company, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }); ;
            Request1C request = new("PATCH", uri, content);

            Company1C result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }

        internal Company1C AddCompany(Company1C company)
        {
            string uri = "http://94.230.11.182:50080/uuc/hs/courses/EditPartner";
            string content = JsonConvert.SerializeObject(company, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }); ;
            Request1C request = new("POST", uri, content);
            
            Company1C result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }
    }
}