﻿using Google.Apis.Sheets.v4;
using MZPO.AmoRepo;
using MZPO.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.LeadProcessors
{
    public class SiteFormRetailProcessor : ILeadProcessor
    {
        private readonly Amo _amo;
        private readonly GSheets _gSheets;
        private readonly IAmoRepo<Lead> _leadRepo;
        private readonly IAmoRepo<Contact> _contRepo;
        private readonly Log _log;
        private readonly FormRequest _formRequest;
        private readonly TaskList _processQueue;
        private readonly CancellationToken _token;

        public SiteFormRetailProcessor(Amo amo, Log log, FormRequest formRequest, TaskList processQueue, CancellationToken token, GSheets gSheets)
        {
            _amo = amo;
            _leadRepo = amo.GetAccountById(28395871).GetRepo<Lead>();
            _contRepo = amo.GetAccountById(28395871).GetRepo<Contact>();
            _log = log;
            _formRequest = formRequest;
            _processQueue = processQueue;
            _token = token;
            _gSheets = gSheets;
        }

        private readonly Dictionary<string, int> fieldIds = new() {
            { "form_name_site", 639075 },
            { "site", 639081 },
            { "page_url", 639083 },
            { "page_title", 647653 },
            { "roistat_marker", 639085 },
            { "roistat_visit", 639073 },
            { "city_name", 639087 },
            { "course", 357005 },
            { "_ym_uid", 715049 },
            { "_ya_uid", 715049 },
            { "clid", 643439 },
            { "utm_source", 640697 },
            { "utm_medium", 640699 },
            { "utm_term", 640703 },
            { "utm_content", 643437 },
            { "utm_campaign", 640701 },
        };

        private class ContactsComparer : IEqualityComparer<Contact>
        {
            public bool Equals(Contact x, Contact y)
            {
                if (ReferenceEquals(x, y)) return true;

                if (x is null || y is null)
                    return false;

                return x.id == y.id;
            }

            public int GetHashCode(Contact c)
            {
                if (c is null) return 0;

                int hashProductCode = (int)c.id;

                return hashProductCode;
            }
        }

        private static void PopulateCFs(Lead lead, FormRequest formRequest, Dictionary<string, int> fieldIds)
        {
            foreach (var p in formRequest.GetType().GetProperties())
                if (fieldIds.ContainsKey(p.Name) &&
                    p.GetValue(formRequest) is not null &&
                    (string)p.GetValue(formRequest) != "undefined" &&
                    (string)p.GetValue(formRequest) != "")
                {
                    if (lead.custom_fields_values is null) lead.custom_fields_values = new();
                    lead.AddNewCF(fieldIds[p.Name], p.GetValue(formRequest));
                }
        }

        private static IEnumerable<int> AddNewLead(List<Contact> similarContacts, int price, bool webinar, bool events, FormRequest formRequest, Dictionary<string, int> fieldIds, IAmoRepo<Lead> leadRepo, Log log)
        {
            Lead lead = new()
            {
                name = "Новая сделка",
                price = price,
                responsible_user_id = 2576764,
                _embedded = new()
            };

            Contact contact = new()
            {
                name = formRequest.name,
                responsible_user_id = 2576764,
            };

            if (similarContacts.Any())
            {
                contact.id = similarContacts.First().id;
                contact.responsible_user_id = similarContacts.First().responsible_user_id;
                lead.responsible_user_id = similarContacts.First().responsible_user_id;
                log.Add($"Найден похожий контакт: {contact.id}.");
            }
            else
            {
                contact.custom_fields_values = new();

                if (formRequest.email is not null &&
                    formRequest.email != "undefined" &&
                    formRequest.email != "")
                    contact.AddNewCF(264913, formRequest.email);

                if (formRequest.phone is not null &&
                    formRequest.phone != "undefined" &&
                    formRequest.phone != "")
                    contact.AddNewCF(264911, formRequest.phone);
            }

            lead._embedded.contacts = new() { contact };

            #region Setting Pipelines
            int pipeline = 0;
            int status = 0;

            if (formRequest.pipeline is not null &&
                formRequest.pipeline != "undefined" &&
                formRequest.pipeline != "")
            {
                int.TryParse(formRequest.pipeline, out pipeline);

                if (formRequest.status is not null &&
                    formRequest.status != "undefined" &&
                    formRequest.status != "")
                    int.TryParse(formRequest.status, out status);
            }

            if (pipeline > 0)
            {
                lead.pipeline_id = pipeline;
                if (status > 0)
                    lead.status_id = status;
            }
            else
            {
                lead.pipeline_id = 3198184;
                lead.status_id = 32532880;
            }
            #endregion

            #region Add tags
            if (webinar)
            {
                if (lead._embedded.tags is null) lead._embedded.tags = new();
                lead._embedded.tags.Add(new() { id = 276829 });
            }

            if (events)
            {
                if (lead._embedded.tags is null) lead._embedded.tags = new();
                lead._embedded.tags.Add(new() { id = 276831 });
            } 
            #endregion

            PopulateCFs(lead, formRequest, fieldIds);

            IEnumerable<int> processedIds = leadRepo.AddNewComplex(lead);

            log.Add($"Создана новая сделка {processedIds.First()}");

            return processedIds;
        }

        private static IEnumerable<int> UpdateFoundLead(Lead oldLead, FormRequest formRequest, Dictionary<string, int> fieldIds, IAmoRepo<Lead> leadRepo, Log log)
        {
            Lead lead = new()
            {
                id = oldLead.id,
            };

            PopulateCFs(lead, formRequest, fieldIds);

            IEnumerable<int> processedIds = leadRepo.Save(lead).Select(x => x.id);

            log.Add($"Обновлена сделка {processedIds.First()}");

            return processedIds;
        }


        public Task Run()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove($"FormSiteRet");
                return Task.FromCanceled(_token);
            }

            try
            {
                #region Checking for contacts
                if ((_formRequest.email is null ||
                    _formRequest.email == "undefined" ||
                    _formRequest.email == "") &&
                    (_formRequest.phone is null ||
                    _formRequest.phone == "undefined" ||
                    _formRequest.phone == ""))
                {
                    _log.Add("Request without contacts");
                    _processQueue.Remove($"FormSiteRet");
                    return Task.CompletedTask;
                }

                if (_formRequest.phone is not null &&
                    _formRequest.phone != "undefined" &&
                    _formRequest.phone != "")
                    _formRequest.phone = _formRequest.phone.Trim().Replace("+", "").Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", "");
                #endregion

                #region Getting similar contact
                List<Contact> similarContacts = new();
                try
                {
                    if (_formRequest.phone is not null &&
                        _formRequest.phone != "undefined" &&
                        _formRequest.phone != "")
                        similarContacts.AddRange(_contRepo.GetByCriteria($"query={_formRequest.phone}&with=leads"));

                    if (_formRequest.email is not null &&
                        _formRequest.email != "undefined" &&
                        _formRequest.email != "")
                        similarContacts.AddRange(_contRepo.GetByCriteria($"query={_formRequest.email}&with=leads"));
                }
                catch (Exception e) { _log.Add($"Не удалось осуществить поиск похожих контактов: {e.Message}"); }
                #endregion

                #region Getting similar leads
                List<Lead> similarLeads = new();

                if (similarContacts.Any())
                {
                    List<int> leadIds = new();

                    foreach (var c in similarContacts)
                        if (c._embedded is not null &&
                            c._embedded.leads is not null)
                            leadIds.AddRange(c._embedded.leads.Select(x => x .id));

                    if (leadIds.Any())
                        similarLeads.AddRange(_leadRepo.BulkGetById(leadIds.Distinct()).Where(x => x.status_id != 142 && x.status_id != 143));
                }
                #endregion

                #region Parsing webinars and events
                bool.TryParse(_formRequest.webinar, out bool webinar);
                bool.TryParse(_formRequest.events, out bool events);
                int.TryParse(_formRequest.price, out int price);
                #endregion

                try
                {
                    IEnumerable<int> processedIds;

                    if (similarLeads.Any() &&
                        price == 0)
                        processedIds = UpdateFoundLead(similarLeads.First(), _formRequest, fieldIds, _leadRepo, _log);
                    else
                        processedIds = AddNewLead(similarContacts, price, webinar, events, _formRequest, fieldIds, _leadRepo, _log);

                    if (processedIds.Any() &&
                        _formRequest.comment is not null &&
                        _formRequest.comment != "undefined" &&
                        _formRequest.comment != "")
                    {
                        _leadRepo.AddNotes(new Note() { entity_id = processedIds.First(), note_type = "common", parameters = new Note.Params() { text = $"{_formRequest.comment}" } });
                        _log.Add($"Добавлены комментарии в сделку {processedIds.First()}");

                        if (webinar)
                        {
                            GSheetsProcessor leadProcessor = new(processedIds.First(), _amo, _gSheets, _processQueue, _log, _token);
                            leadProcessor.Webinar(_formRequest.date, _formRequest.comment, price, _formRequest.name, _formRequest.phone, _formRequest.email).Wait();
                        }
                        if (events)
                        {
                            GSheetsProcessor leadProcessor = new(processedIds.First(), _amo, _gSheets, _processQueue, _log, _token);
                            leadProcessor.Events(_formRequest.date, _formRequest.comment, price, _formRequest.name, _formRequest.phone, _formRequest.email).Wait();
                        }

                        _log.Add($"Добавлены данные о сделке {processedIds.First()} в таблицу.");
                    }
                }
                catch (Exception e)
                {
                    _log.Add($"Не получилось сохранить данные в амо: {e.Message}.");
                    _log.Add($"POST: {JsonConvert.SerializeObject(_formRequest, Formatting.Indented)}");
                }

                _processQueue.Remove($"FormSiteRet");
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                _log.Add($"Не получилось добавить заявку с сайта: {e.Message}.");
                _processQueue.Remove($"FormSiteRet");
                return Task.FromException(e);
            }
        }
    }
}