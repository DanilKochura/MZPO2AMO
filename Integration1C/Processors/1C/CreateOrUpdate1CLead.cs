﻿using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Linq;

namespace Integration1C
{
    public class CreateOrUpdate1CLead
    {
        private readonly Amo _amo;
        private readonly Log _log;
        private readonly int _leadId;
        private readonly int _amo_acc;

        public CreateOrUpdate1CLead(Amo amo, Log log, int leadId, int amo_acc)
        {
            _amo = amo;
            _log = log;
            _leadId = leadId;
            _amo_acc = amo_acc;
        }

        private static void PopulateCFs(Lead lead, int amo_acc, Lead1C lead1C)
        {
            if (lead.custom_fields_values is not null)
                foreach (var p in lead1C.GetType().GetProperties())
                    if (FieldLists.Leads[amo_acc].ContainsKey(p.Name) &&
                        lead.custom_fields_values.Any(x => x.field_id == FieldLists.Leads[amo_acc][p.Name]))
                        p.SetValue(lead1C, lead.custom_fields_values.First(x => x.field_id == FieldLists.Leads[amo_acc][p.Name]).values[0].value);
        }

        private static void GetConnectedEntities(Amo amo, Log log, Lead lead, int amo_acc, Lead1C lead1C)
        {
            if (lead._embedded is null ||
                lead._embedded.contacts is null ||
                !lead._embedded.contacts.Any() ||
                lead._embedded.catalog_elements is null ||
                !lead._embedded.catalog_elements.Any())
                throw new Exception($"No contacts or catalog elements in lead {lead.id}");

            #region Client
            var clientId = new CreateOrUpdate1CClient(amo, log, lead.id, amo_acc).Run();

            if (clientId == default) throw new Exception($"Unable to get clientId for contact from the lead {lead.id}");

            lead1C.client_id_1C = clientId;
            #endregion

            #region Course
            var course = amo.GetAccountById(amo_acc).GetRepo<Lead>().GetCEById(lead._embedded.catalog_elements.First().id);

            if (course is not null &&
                course.custom_fields is not null &&
                !course.custom_fields.Any(x => x.id == FieldLists.Courses[amo_acc]["product_id_1C"]) &&
                Guid.TryParse(course.custom_fields.First(x => x.id == FieldLists.Courses[amo_acc]["product_id_1C"]).values[0].value, out Guid product_id_1C))
                lead1C.product_id_1C = product_id_1C;
            else
                throw new Exception($"Unable to add course {course.id} from lead {lead.id}");
            #endregion

            #region Company
            if (lead1C.is_corporate &&
                lead._embedded.companies is not null &&
                lead._embedded.companies.Any())
            {
                var companyId = new CreateOrUpdate1CCompany(amo, log, lead.id).Run();

                if (companyId == default) throw new Exception($"Unable to get companyId for company from the lead {lead.id}");

                lead1C.company_id_1C = companyId;
            }

            if (lead1C.is_corporate &&
                lead1C.company_id_1C is null)
                throw new Exception($"Unable to get company in lead {lead.id}");
            #endregion
        }

        private static void UpdateLeadIn1C(Amo amo, Log log, Lead lead, Guid lead_id_1C, int amo_acc)
        {
            Lead1C lead1C = new() {
                lead_id_1C = lead_id_1C,
                price = (int)lead.price,
                amo_ids = new() { new() {
                        account_id = amo_acc,
                        entity_id = lead.id
            } } };

            PopulateCFs(lead, amo_acc, lead1C);

            if (amo_acc == 19453687)
                lead1C.is_corporate = true;

            GetConnectedEntities(amo, log, lead, amo_acc, lead1C);

            new LeadRepository().UpdateLead(lead1C);
        }

        private static Guid CreateLeadIn1C(Amo amo, Log log, Lead lead, int amo_acc)
        {
            Lead1C lead1C = new() {
                price = (int)lead.price,
                amo_ids = new() { new() {
                        account_id = amo_acc,
                        entity_id = lead.id
            } } };

            PopulateCFs(lead, amo_acc, lead1C);

            if (amo_acc == 19453687)
                lead1C.is_corporate = true;

            GetConnectedEntities(amo, log, lead, amo_acc, lead1C);

            return new LeadRepository().UpdateLead(lead1C);
        }

        private static void UpdateLeadInAmoWithUID(IAmoRepo<Lead> leadRepo, int amo_acc, int leadId, Guid uid)
        {
            Lead lead = new() {
                id = leadId,
                custom_fields_values = new() { new Lead.Custom_fields_value() {
                        field_id = FieldLists.Leads[amo_acc]["lead_id_1C"],
                        values = new Lead.Custom_fields_value.Values[] { new Lead.Custom_fields_value.Values() { value = uid.ToString("D") } }
            } } };

            try
            {
                leadRepo.Save(lead);
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to update lead {leadId} in amo {amo_acc}: {e}");
            }
        }


        public Guid Run()
        {
            try
            {
                var leadRepo = _amo.GetAccountById(_amo_acc).GetRepo<Lead>();

                var lead = leadRepo.GetById(_leadId);

                if (lead is null ||
                    lead == default)
                    throw new Exception("No lead returned from amo.");

                if (lead.custom_fields_values is not null &&
                    lead.custom_fields_values.Any(x => x.field_id == FieldLists.Leads[_amo_acc]["lead_id_1C"]) &&
                    Guid.TryParse((string)lead.custom_fields_values.First(x => x.field_id == FieldLists.Leads[_amo_acc]["lead_id_1C"]).values[0].value, out Guid lead_id_1C))
                {
                    UpdateLeadIn1C(_amo, _log, lead, lead_id_1C, _amo_acc);
                    return lead_id_1C;
                }

                var uid = CreateLeadIn1C(_amo, _log, lead, _amo_acc);

                UpdateLeadInAmoWithUID(leadRepo, _amo_acc, _leadId, uid);

                return uid;
            }
            catch (Exception e)
            {
                _log.Add($"Unable to create or upadate lead {_leadId} in 1C: {e}");
                return default;
            }
        }
    }
}