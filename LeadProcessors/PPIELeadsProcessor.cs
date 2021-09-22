﻿using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.LeadProcessors
{
    public class PPIELeadsProcessor : ILeadProcessor
    {
        private readonly IAmoRepo<Lead> _leadRepo;
        private readonly ProcessQueue _processQueue;
        private readonly CancellationToken _token;
        private readonly int _leadNumber;
        private readonly Log _log;
        private Lead lead;

        public PPIELeadsProcessor(int leadNumber, AmoAccount acc, ProcessQueue processQueue, Log log, CancellationToken token)
        {
            _leadRepo = acc.GetRepo<Lead>();
            _processQueue = processQueue;
            _token = token;
            _leadNumber = leadNumber;
            _log = log;

            try
            {
                Thread.Sleep((int)TimeSpan.FromSeconds(3).TotalMilliseconds);
                lead = _leadRepo.GetById(leadNumber);
            }
            catch (Exception e)
            {
                _processQueue.Stop(leadNumber.ToString());
                _processQueue.Remove(leadNumber.ToString());
                _log.Add($"Error: Unable to create leadProcessor {leadNumber}: {e.Message}");
            }
        }

        int[] managers = new[]
        {
            7074307,
            7074316,
            7074319
        };

        public Task Run()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove($"initial_{_leadNumber}");
                return Task.FromCanceled(_token);
            }

            try
            {
                _leadRepo.Save(new Lead()
                {
                    id = lead.id,
                    name = "Новая сделка"
                });

                _processQueue.Remove($"initial_{_leadNumber}");
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                _log.Add($"Unable to process ppie lead {lead.id}: {e.Message}");

                _processQueue.Remove($"initial_{_leadNumber}");
                return Task.FromException(e);
            }
        }
    }
}