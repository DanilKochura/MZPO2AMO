﻿using MZPO.Services;
using Newtonsoft.Json;
using System;
using System.Net;

namespace Integration1C
{
    internal class CompanyRepository
    {
        private readonly Cred1C _cred1C;

        public CompanyRepository(Cred1C cred1C)
        {
            _cred1C = cred1C;
        }

        internal Company1C GetCompany(Company1C company) => GetCompany((Guid)company.company_id_1C);

        internal Company1C GetCompany(Guid company_id)
        {
            string method = $"http://94.230.11.182:50080/uuc/hs/courses/EditPartner?id={company_id:D}";
            Request1C request = new("GET", method, _cred1C);
            
            Company1C result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }

        internal Guid UpdateCompany(Company1C company)
        {
            if (company.company_id_1C is null ||
                company.company_id_1C == default)
                throw new Exception("Unable to update 1C client, no UID.");

            string method = "http://94.230.11.182:50080/uuc/hs/courses/EditPartner";
            string content = JsonConvert.SerializeObject(company, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }); ;
            Request1C request = new("PATCH", method, content, _cred1C);

            Guid result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }

        internal Guid AddCompany(Company1C company)
        {
            if (string.IsNullOrEmpty(company.email) &&
                string.IsNullOrEmpty(company.phone))
                throw new Exception("Unable to add company to 1C: no phone or email.");

            company.company_id_1C = null;

            string method = "http://94.230.11.182:50080/uuc/hs/courses/EditPartner";
            string content = JsonConvert.SerializeObject(company, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }); ;
            Request1C request = new("POST", method, content, _cred1C);

            Guid result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }
    }
}