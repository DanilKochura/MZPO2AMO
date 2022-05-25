﻿using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.LeadProcessors
{
    public class SendToAgrProcessor
    {
        private readonly Log _log;
        private readonly int _leadNumber;
        private readonly ProcessQueue _processQueue;
        private readonly CancellationToken _token;

        private readonly IAmoRepo<Lead> _leadRepo;
        private readonly IAmoRepo<Contact> _contRepo;
        private readonly IAmoRepo<Company> _compRepo;

        public SendToAgrProcessor(Amo amo, Log log, ProcessQueue processQueue, int leadNumber, CancellationToken token)
        {
            _log = log;
            _leadNumber = leadNumber;
            _processQueue = processQueue;
            _token = token;

            _leadRepo = amo.GetAccountById(19453687).GetRepo<Lead>();
            _contRepo = amo.GetAccountById(19453687).GetRepo<Contact>();
            _compRepo = amo.GetAccountById(19453687).GetRepo<Company>();
        }

        private static IEnumerable<Custom_fields_value> GetCFs(IEnumerable<Custom_fields_value> fields)
        {
            if (fields is null) yield break;
            
            foreach(var f in fields)
            {
                yield return new Custom_fields_value() 
                { 
                    field_id = f.field_id,
                    values = f.values.Select(x => new Custom_fields_value.Value() { value = x.value }).Take(1).ToArray()
                };
            }

            yield break;
        }

        private Lead.Embedded GetEmbedded(Lead.Embedded embedded)
        {
            Lead.Embedded result = new();

            result.contacts = embedded.contacts.Select(x => new Contact() { id = x.id }).Take(1).ToList();
            result.companies = embedded.companies.Select(x => new Company() { id = x.id }).ToList();
            result.tags = embedded.tags.Select(x => new Tag() { id = x.id }).ToList();

            return result;
        }

        public Task Send()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove($"corp2agr-{_leadNumber}");
                return Task.FromCanceled(_token);
            }
            try
            {
                //Получаем сделку
                Lead sourceLead = _leadRepo.GetById(_leadNumber);

                //Создаём новую сделку
                Lead lead = new()
                {
                    name = sourceLead.name,
                    price = sourceLead.price,
                    pipeline_id = 5238031,
                    status_id = 46783762,
                    responsible_user_id = 8119492, //Елена Тропина
                    custom_fields_values = GetCFs(sourceLead.custom_fields_values).ToList(),
                    _embedded = GetEmbedded(sourceLead._embedded)
                };

                lead._embedded.catalog_elements = new();

                //Устанавливаем номер сделки продажи
                lead.AddNewCF(759479, _leadNumber);

                //Сохраняем новую сделку
                int createdLeadNumber = _leadRepo.AddNew(lead).Select(x => x.id).First();

                var contacts = sourceLead._embedded.contacts.Select(x => new Contact() { id = x.id }).Skip(1).ToList();
                if (contacts.Count > 0)
                    _leadRepo.LinkEntity(createdLeadNumber,
                                         contacts.Select(x => new EntityLink()
                                         {
                                             to_entity_id = (int)x.id,
                                             to_entity_type = "contacts",
                                         }));

                //Добавляем в старую сделку инфу о новой
                sourceLead = new()
                {
                    id = _leadNumber
                };

                sourceLead.AddNewCF(759479, createdLeadNumber);

                //Сохраняем старую сделку
                _leadRepo.Save(sourceLead);

                //Получаем примечания к сделке
                var leadNotes = _leadRepo.GetEntityNotes(_leadNumber);
                List<Note> notes = new(leadNotes.Where(x => x.note_type == "common"));

                //Переносим примечания
                if (notes.Any())
                    foreach (var n in notes.Select(x => new Note()
                    {
                        entity_id = createdLeadNumber,
                        created_by = x.created_by,
                        updated_by = x.updated_by,
                        created_at = x.created_at,
                        updated_at = x.updated_at,
                        responsible_user_id = x.responsible_user_id,
                        group_id = x.group_id,
                        note_type = x.note_type,
                        parameters = x.parameters
                    }))
                        _leadRepo.AddNotes(n);

                _log.Add($"Создана новая сделка из продаж в дог. отделе {createdLeadNumber}");

                _processQueue.Remove($"corp2agr-{_leadNumber}");

                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                _processQueue.Remove($"corp2agr-{_leadNumber}");
                _log.Add($"Не получилось перенести сделку {_leadNumber} из продаж в дог. отдел: {e.Message}.");
                return Task.FromException(e);
            }
        }

        public Task Back()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove($"corp2agr-{_leadNumber}");
                return Task.FromCanceled(_token);
            }

            try
            {
                //Получаем сделку
                Lead lead = _leadRepo.GetById(_leadNumber);

                //Проверяем есть ли связь с оригинальной сделкой
                if (!lead.HasCF(759479))
                    throw new InvalidOperationException("В сделке дог. отдела отсутствует номер сделки из продаж.");

                //Получаем номер оригинальной сделки
                int sourceLeadNumber = lead.GetCFIntValue(759479);

                //Получаем оригинальную сделку
                Lead sourceLead = _leadRepo.GetById(sourceLeadNumber);

                //Проверяем что сделка на корректном этапе
                if (sourceLead.status_id != 47337076)
                    throw new InvalidOperationException("Сделка продаж находится на некорректном этапе.");

                //Перемещаем оригинальную сделку на новый этап
                sourceLead = new()
                {
                    id = sourceLeadNumber,
                    pipeline_id = 5312269,
                    status_id = 47317651
                };

                _leadRepo.Save(sourceLead);

                _log.Add($"Синхронизирована сделка продаж {sourceLeadNumber} и договорного отдела {_leadNumber}.");

                _processQueue.Remove($"corp2agr-{_leadNumber}");
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                _processQueue.Remove($"corp2agr-{_leadNumber}");
                _log.Add($"Не получилось синхронизировать сделку {_leadNumber} из дог. отдела в продажи: {e.Message}.");
                return Task.FromException(e);
            }
        }
    }
}