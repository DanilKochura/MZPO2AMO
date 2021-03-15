﻿using MZPO.AmoRepo;
using MZPO.Services;
using System;

namespace Integration1C
{
    public class UpdateAmoContact
    {
        private readonly Amo _amo;
        private readonly Log _log;
        private readonly Client1C _client1C;

        public UpdateAmoContact(Client1C client1C, Amo amo, Log log)
        {
            _amo = amo;
            _log = log;
            _client1C = client1C;
        }

        private static void UpdateContactInAmo(Client1C client1C, IAmoRepo<Contact> contRepo, int contact_id, int acc_id)
        {
            Contact contact = new()
            {
                id = contact_id,
                name = client1C.name,
                custom_fields_values = new(),
            };

            contact.custom_fields_values.Add(new Contact.Custom_fields_value()
            {
                field_id = FieldLists.Contacts[acc_id]["company_id_1C"],
                values = new Contact.Custom_fields_value.Values[] { new Contact.Custom_fields_value.Values() { value = client1C.client_id_1C.ToString("D") } }
            });

            foreach (var p in client1C.GetType().GetProperties())
                if (FieldLists.Contacts[acc_id].ContainsKey(p.Name) &&
                    p.GetValue(client1C) is not null &&
                    (string)p.GetValue(client1C) != "") //В зависимости от политики передачи пустых полей
                {
                    if (contact.custom_fields_values is null) contact.custom_fields_values = new();
                    contact.custom_fields_values.Add(new Contact.Custom_fields_value()
                    {
                        field_id = FieldLists.Contacts[acc_id][p.Name],
                        values = new Contact.Custom_fields_value.Values[] { new Contact.Custom_fields_value.Values() { value = (string)p.GetValue(client1C) } }
                    });
                }
            try
            {
                contRepo.Save(contact);
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to update contact {contact_id} in amo: {e}");
            }
        }

        public void Run()
        {
            try
            {
                if (_client1C.amo_ids is not null)
                {
                    foreach (var c in _client1C.amo_ids)
                        UpdateContactInAmo(_client1C, _amo.GetAccountById(c.account_id).GetRepo<Contact>(), c.entity_id, c.account_id);
                    return;
                }
            }
            catch (Exception e)
            {
                _log.Add($"Unable to update company in amo from 1C: {e}");
            }
        }
    }
}