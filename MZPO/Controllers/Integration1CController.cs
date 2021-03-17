﻿using Microsoft.AspNetCore.Mvc;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Integration1C;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace MZPO.Controllers
{
    [Route("integration/1c/{action}")]
    public class Integration1CController : Controller
    {
        private readonly TaskList _processQueue;
        private readonly Amo _amo;
        private readonly Log _log;

        public Integration1CController(Amo amo, TaskList processQueue, Log log)
        {
            _amo = amo;
            _processQueue = processQueue;
            _log = log;
        }
        
        // POST: integration/1c/saveclient
        [HttpPost]
        [ActionName("SaveClient")]
        public IActionResult SaveClient1C()
        {
            var col = Request.Form;

            if (!Int32.TryParse(col["account[id]"], out int accNumber)) return BadRequest("Incorrect account number.");

            if (!col.ContainsKey("leads[status][0][id]")) return BadRequest("Unexpected request.");
            if (!Int32.TryParse(col["leads[add][0][id]"], out int leadNumber)) return BadRequest("Incorrect lead number.");

            var task = Task.Run(() => new CreateOrUpdate1CClient(_amo, _log, leadNumber, accNumber).Run());

            return Ok();
        }

        // POST: integration/1c/updateclient
        [HttpPost]
        [ActionName("UpdateClient")]
        public IActionResult UpdateClient1C()
        {
            var col = Request.Form;

            AmoAccount acc;

            if (!Int32.TryParse(col["account[id]"], out int accNumber)) return BadRequest("Incorrect account number.");

            try { acc = _amo.GetAccountById(accNumber); }
            catch (Exception e) { _log.Add(e.Message); return Ok(); }

            if (!col.ContainsKey("contacts[update][0][id]")) return BadRequest("Unexpected request.");
            if (!Int32.TryParse(col["contacts[update][0][id]"], out int contactNumber)) return BadRequest("Incorrect contact number.");

            var task = Task.Run(() => new Update1CClient(_amo, _log, contactNumber, accNumber).Run());

            return Ok();
        }

        // POST: integration/1c/client
        [HttpPost]
        [ActionName("Client")]
        public IActionResult CreateClientAmo()
        {
            using StreamReader sr = new(Request.Body);
            var request = sr.ReadToEndAsync().Result;

            Client1C client1C = new();

            try
            {
                JsonConvert.PopulateObject(WebUtility.UrlDecode(request), client1C);
            }
            catch (Exception e)
            {
                _log.Add($"Unable to parse JSON to client1C: {e}");
                return BadRequest("Incorrect JSON");
            }

            List<Amo_id> result = new();

            if (client1C.amo_ids is null ||
                !client1C.amo_ids.Any())
                result.AddRange(new CreateOrUpdateAmoContact(client1C, _amo, _log).Run());
            else
                result.AddRange(new UpdateAmoContact(client1C, _amo, _log).Run());

            return Ok(JsonConvert.SerializeObject(result, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
        }

        // POST: integration/1c/savecompany
        [HttpPost]
        [ActionName("SaveCompany")]
        public IActionResult SaveCompany1C()
        {
            var col = Request.Form;

            AmoAccount acc;

            if (!Int32.TryParse(col["account[id]"], out int accNumber)) return BadRequest("Incorrect account number.");

            try { acc = _amo.GetAccountById(accNumber); }
            catch (Exception e) { _log.Add(e.Message); return Ok(); }

            if (!col.ContainsKey("leads[status][0][id]")) return BadRequest("Unexpected request.");
            if (!Int32.TryParse(col["leads[add][0][id]"], out int leadNumber)) return BadRequest("Incorrect lead number.");

            var task = Task.Run(() => new CreateOrUpdate1CCompany(_amo, _log, leadNumber).Run());

            return Ok();
        }

        // POST: integration/1c/updatecompany
        [HttpPost]
        [ActionName("UpdateCompany")]
        public IActionResult UpdateCompany1C()
        {
            var col = Request.Form;

            AmoAccount acc;

            if (!Int32.TryParse(col["account[id]"], out int accNumber)) return BadRequest("Incorrect account number.");

            try { acc = _amo.GetAccountById(accNumber); }
            catch (Exception e) { _log.Add(e.Message); return Ok(); }

            if (!col.ContainsKey("contacts[update][0][id]")) return BadRequest("Unexpected request.");
            if (!Int32.TryParse(col["contacts[update][0][id]"], out int companyNumber)) return BadRequest("Incorrect lead number.");

            var task = Task.Run(() => new Update1CCompany(_amo, _log, companyNumber).Run());

            return Ok();
        }

        // POST: integration/1c/company
        [HttpPost]
        [ActionName("Company")]
        public IActionResult CreateCompanyAmo()
        {
            using StreamReader sr = new(Request.Body);
            var request = sr.ReadToEndAsync().Result;

            Company1C company1C = new();

            try
            {
                JsonConvert.PopulateObject(WebUtility.UrlDecode(request), company1C);
            }
            catch (Exception e)
            {
                _log.Add($"Unable to parse JSON to company1C: {e}");
                return BadRequest("Incorrect JSON");
            }

            List<Amo_id> result = new();

            if (company1C.amo_ids is null ||
                !company1C.amo_ids.Any())
                result.AddRange(new CreateOrUpdateAmoCompany(company1C, _amo, _log).Run());
            else
                result.AddRange(new UpdateAmoCompany(company1C, _amo, _log).Run());

            return Ok(JsonConvert.SerializeObject(result, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
        }

        // POST: integration/1c/savelead
        [HttpPost]
        [ActionName("SaveLead")]
        public IActionResult SaveLead1C()
        {
            var col = Request.Form;

            AmoAccount acc;

            if (!Int32.TryParse(col["account[id]"], out int accNumber)) return BadRequest("Incorrect account number.");

            try { acc = _amo.GetAccountById(accNumber); }
            catch (Exception e) { _log.Add(e.Message); return Ok(); }

            if (!col.ContainsKey("leads[status][0][id]")) return BadRequest("Unexpected request.");
            if (!Int32.TryParse(col["leads[add][0][id]"], out int leadNumber)) return BadRequest("Incorrect lead number.");

            var task = Task.Run(() => new CreateOrUpdate1CLead(_amo, _log, leadNumber, accNumber).Run());

            return Ok();
        }

        // POST: integration/1c/updatelead
        [HttpPost]
        [ActionName("UpdateLead")]
        public IActionResult UpdateLead1C()
        {
            var col = Request.Form;

            AmoAccount acc;

            if (!Int32.TryParse(col["account[id]"], out int accNumber)) return BadRequest("Incorrect account number.");

            try { acc = _amo.GetAccountById(accNumber); }
            catch (Exception e) { _log.Add(e.Message); return Ok(); }

            if (!col.ContainsKey("leads[status][0][id]")) return BadRequest("Unexpected request.");
            if (!Int32.TryParse(col["leads[add][0][id]"], out int leadNumber)) return BadRequest("Incorrect lead number.");

            var task = Task.Run(() => new Update1CLead(_amo, _log, leadNumber, accNumber).Run());

            return Ok();
        }

        // POST: integration/1c/lead
        [HttpPost]
        [ActionName("Lead")]
        public IActionResult CreateLeadAmo()
        {
            using StreamReader sr = new(Request.Body);
            var request = sr.ReadToEndAsync().Result;

            Lead1C lead1C = new();

            try
            {
                JsonConvert.PopulateObject(WebUtility.UrlDecode(request), lead1C);
            }
            catch (Exception e)
            {
                _log.Add($"Unable to parse JSON to lead1C: {e}");
                return BadRequest("Incorrect JSON");
            }

            List<Amo_id> result = new();

            if (lead1C.amo_ids is null ||
                !lead1C.amo_ids.Any())
                result.AddRange(new CreateOrUpdateAmoLead(lead1C, _amo, _log).Run());
            else
                result.AddRange(new UpdateAmoLead(lead1C, _amo, _log).Run());

            return Ok(JsonConvert.SerializeObject(result, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
        }

        // POST: integration/1c/course
        [HttpPost]
        [ActionName("Course")]
        public IActionResult CreateCourseAmo()
        {
            using StreamReader sr = new(Request.Body);
            var request = sr.ReadToEndAsync().Result;

            Course1C course1C = new();

            try
            {
                JsonConvert.PopulateObject(WebUtility.UrlDecode(request), course1C);
            }
            catch (Exception e)
            {
                _log.Add($"Unable to parse JSON to course1C: {e}");
                return BadRequest("Incorrect JSON");
            }

            List<Amo_id> result = new();

            if (course1C.amo_ids is null ||
                !course1C.amo_ids.Any())
                result.AddRange(new CreateOrUpdateAmoCourse(course1C, _amo, _log).Run());
            else
                result.AddRange(new UpdateAmoCourse(course1C, _amo, _log).Run());

            return Ok(JsonConvert.SerializeObject(result, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
        }

        // POST: integration/1c/paymentreceived
        [HttpPost]
        [ActionName("PaymentReceived")]
        public IActionResult ProcessPayment()
        {
            using StreamReader sr = new StreamReader(Request.Body);
            var hook = sr.ReadToEndAsync().Result;

            using StreamWriter sw = new StreamWriter("hook.txt", true, System.Text.Encoding.Default);
            sw.WriteLine("POST: integration/1c/paymentreceived");
            sw.WriteLine(WebUtility.UrlDecode(hook));
            sw.WriteLine("--**--**--");

            return Ok();
        }

        // POST: integration/1c/courseended
        [HttpPost]
        [ActionName("CourseEnded")]
        public IActionResult FinishCourse()
        {
            using StreamReader sr = new StreamReader(Request.Body);
            var hook = sr.ReadToEndAsync().Result;

            using StreamWriter sw = new StreamWriter("hook.txt", true, System.Text.Encoding.Default);
            sw.WriteLine("POST: integration/1c/courseended");
            sw.WriteLine(WebUtility.UrlDecode(hook));
            sw.WriteLine("--**--**--");

            return Ok();
        }
    }
}