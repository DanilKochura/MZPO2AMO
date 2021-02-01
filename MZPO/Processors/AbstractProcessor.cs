﻿using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MZPO.Processors
{
    public abstract class AbstractProcessor                                                                             //Абстрактный процессор сделки, включает в себя определение и вспомогательный методы
    {
        #region Definition
        protected readonly IBaseRepo<Lead> _leadRepo;
        protected readonly AmoAccount _acc;
        protected readonly TaskList _processQueue;
        protected readonly CancellationToken _token;
        protected readonly int _leadNumber;
        //protected readonly Dictionary<string, int> _customFields;
        protected Lead lead;
        protected List<Tag> tags;
        protected List<Lead.Custom_fields_value> custom_fields_values;

        public AbstractProcessor(int leadNumber, AmoAccount acc, TaskList processQueue, CancellationToken token)
        {
            _leadRepo = acc.GetRepo<Lead>();
            _processQueue = processQueue;
            _token = token;
            _acc = acc;
            _leadNumber = leadNumber;
            //_customFields = _acc.GetCFList(); 
            custom_fields_values = new List<Lead.Custom_fields_value>();
            tags = new List<Tag>();

            try
            {
                Thread.Sleep((int)TimeSpan.FromSeconds(5).TotalMilliseconds);
                lead = _leadRepo.GetById(leadNumber);
                if (lead is { } && lead._embedded is { } && lead._embedded.tags is { })
                    tags = lead._embedded.tags;
            }
            catch (Exception e)
            {
                _processQueue.Stop(leadNumber.ToString());
                _processQueue.Remove(leadNumber.ToString());
                Log.Add($"Error: Unable to create leadProcessor {leadNumber}: {e.Message}");
            }
        }
        #endregion

        #region Supplementary methods
        protected string GetFieldValue(int fieldId)
        {
            if (custom_fields_values.Any(x => x.field_id == fieldId))
                return (string)custom_fields_values.Where(x => x.field_id == fieldId).FirstOrDefault().values[0].value;
            else if ((lead.custom_fields_values is { }) &&
                    lead.custom_fields_values.Any(x => x.field_id == fieldId))
                return (string)lead.custom_fields_values.Where(x => x.field_id == fieldId).FirstOrDefault().values[0].value;
            else return null;
        }
        //protected string GetFieldValue(string fieldName) => GetFieldValue(_customFields[fieldName]);


        protected string SetFieldValue(int fieldId, string fieldValue)
        {
            if (custom_fields_values.Any(x => x.field_id == fieldId))
                custom_fields_values.Where(x => x.field_id == fieldId).FirstOrDefault().values[0].value = fieldValue;
            else
            {
                if ((lead.custom_fields_values is { }) &&
                    lead.custom_fields_values.Any(x => x.field_id == fieldId))
                    lead.custom_fields_values.Where(x => x.field_id == fieldId).FirstOrDefault().values[0].value = fieldValue;

                custom_fields_values.Add(new Lead.Custom_fields_value()
                {
                    field_id = fieldId,
                    values = new Lead.Custom_fields_value.Values[] { new Lead.Custom_fields_value.Values() { value = fieldValue } }
                });
            }
            return fieldValue;
        }
        //protected string SetFieldValue(string fieldName, string fieldValue) => SetFieldValue(_customFields[fieldName], fieldValue);

        protected string SetTag(string tagValue)
        {
            if (!tags.Any(x => x.name == tagValue))
                tags.Add(new Tag() { name = tagValue });
            return tagValue;
        }
        #endregion

        public abstract void Run();
    }
}