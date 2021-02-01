﻿using MZPO.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace MZPO.AmoRepo
{
    public class BaseRepository<T> : AbstractRepository, IBaseRepo<T> where T : IModel, new()
    {
        #region Definition
        private readonly AuthProvider _auth;
        private readonly string _entityLink;
        private readonly string _apiAddress;

        public BaseRepository(AmoAccount acc)
        {
            _auth = acc.auth;
            _entityLink = (string)typeof(T).GetProperty("entityLink").GetValue(null, null);   //IModel.entityLink;
            _apiAddress = $"https://{acc.subdomain}.amocrm.ru/api/v4/";
        }
        #endregion

        #region Supplementary methods
        public IEnumerable<T> GetEmbedded(EntityList entity)
        {
            if ((entity is { }) && (entity._embedded is { }))
                return (List<T>)entity.GetType().GetNestedType("Embedded").GetField(_entityLink).GetValue(entity._embedded);
            else return null;
        }

        private O GetResult<O>(AmoRequest request, O o)
        {
            try 
            {
                var response = request.GetResponse();
                if (response == "") return default(O);
                JsonConvert.PopulateObject(WebUtility.UrlDecode(response), o); 
            }
            catch (Exception e) { throw new Exception("Unable to process response : " + e.Message); }
            return o;
        }

        private EntityList GetList(string uri)
        {
            EntityList entityList = new EntityList() { _links = new Dictionary<string, EntityList.Links>() };
            entityList._links.Add("next", new EntityList.Links() { href = uri });

            while (entityList._links.ContainsKey("next"))
            {
                AmoRequest request = new AmoRequest("GET", entityList._links["next"].href, _auth);
                string response = "";

                var next = entityList._links["next"].href;
                try { response = request.GetResponse(); }
                catch(Exception e) { Log.Add($"Bad response: {e}"); }
                
                if (response == "") break;
                
                try { JsonConvert.PopulateObject(WebUtility.UrlDecode(response), entityList); }
                catch (Exception e) { entityList._links.Remove("next"); Log.Add($"Unexpected end of List in GetList():{e}"); }
                
                if (entityList._links.ContainsKey("next") && (next == entityList._links["next"].href)) entityList._links.Remove("next");
            }

            return entityList;
        }
        #endregion

        #region Realization
        public IEnumerable<T> AddNew(IEnumerable<T> payload)
        {
            var uri = $"{_apiAddress}{_entityLink}";

            var content = JsonConvert.SerializeObject(payload, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            AmoRequest request = new AmoRequest("POST", uri, content, _auth);
            EntityList result = new EntityList();
            return GetEmbedded(GetResult<EntityList>(request, result));
        }

        public IEnumerable<T> GetByCriteria(string criteria)
        {
            var uri = $"{_apiAddress}{_entityLink}?{criteria}";

            return GetEmbedded(GetList(uri));
        }

        public IEnumerable<T> BulkGetById(IEnumerable<int> ids)
        {
            int i = 0;
            StringBuilder criteria = new StringBuilder("");
            List<T> result = new List<T>();

            if (ids.Any())
            {
                foreach (var id in ids.Distinct())
                {
                    criteria.Append($"filter[id][{i++}]={id}&");
                    if (i % 10 == 0)
                    {
                        criteria.Append($"with=companies,contacts,leads");

                        result.AddRange(GetByCriteria(criteria.ToString()));

                        criteria = new StringBuilder("");
                    }
                }

                if (criteria.ToString() != "")
                {
                    criteria.Append($"with=companies,contacts,leads");

                    result.AddRange(GetByCriteria(criteria.ToString()));
                }
            }
            return result;
        }

        public T GetById(int id)
        {
            var uri = $"{_apiAddress}{_entityLink}/{id}?with=leads,contacts,companies";                                                               //?with = contacts,leads,catalog_elements,customers

            AmoRequest request = new AmoRequest("GET", uri, _auth);
            return GetResult<T>(request, new T());
        }

        public IEnumerable<T> Save(IEnumerable<T> payload)
        {
            var uri = $"{_apiAddress}{_entityLink}";

            var content = JsonConvert.SerializeObject(payload, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            AmoRequest request = new AmoRequest("PATCH", uri, content, _auth);
            EntityList result = new EntityList();
            return GetEmbedded(GetResult<EntityList>(request, result));
        }
        public IEnumerable<T> Save(T payload) => Save(new List<T>() { payload });

        public IEnumerable<Event> GetEvents(int id)
        {
            var uri = $"{_apiAddress}events?filter[entity]={_entityLink[0..^1]}&filter[entity_id][]={id}";

            var result = GetList(uri);

            if (result._embedded is { } && result._embedded.events is { })
                return result._embedded.events.ToList();
            else return null;
        }

        public IEnumerable<Event> GetEventsByCriteria(string criteria)
        {
            var uri = $"{_apiAddress}events?{criteria}";

            var result = GetList(uri);

            if (result._embedded is { } && result._embedded.events is { })
                return result._embedded.events.ToList();
            else return null;
        }

        public IEnumerable<Note> GetNotes(int id)
        {
            var uri = $"{_apiAddress}{_entityLink}/{id}/notes";

            var result = GetList(uri);

            if (result._embedded is { } && result._embedded.notes is { })
                return result._embedded.notes.ToList();
            else return null;
        }
        public IEnumerable<Note> GetNotesByCriteria(string criteria)
        {
            var uri = $"{_apiAddress}{_entityLink}/notes?{criteria}";

            var result = GetList(uri);

            if (result._embedded is { } && result._embedded.notes is { })
                return result._embedded.notes.ToList();
            else return null;
        }
        public Note GetNoteById(int id)
        {
            var uri = $"{_apiAddress}{_entityLink}/notes/{id}";

            AmoRequest request = new AmoRequest("GET", uri, _auth);
            return GetResult<Note>(request, new Note());
        }
        public IEnumerable<Note> BulkGetNotesById(IEnumerable<int> ids)
        {
            int i = 0;
            StringBuilder criteria = new StringBuilder("");
            List<Note> result = new List<Note>();

            if (ids.Any())
            {
                foreach (var id in ids.Distinct())
                {
                    criteria.Append($"filter[id][{i++}]={id}&");
                    if (i % 10 == 0)
                    {
                        result.AddRange(GetNotesByCriteria(criteria.ToString()));
                        i = 0;
                        criteria = new StringBuilder("");
                    }
                }

                if (criteria.ToString() != "")
                    result.AddRange(GetNotesByCriteria(criteria.ToString()));
            }
            return result;
        }

        public IEnumerable<Note> AddNotes(IEnumerable<Note> payload)
        {
            var uri = $"{_apiAddress}{_entityLink}/notes";

            var content = JsonConvert.SerializeObject(payload, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            AmoRequest request = new AmoRequest("POST", uri, content, _auth);
            EntityList result = new EntityList();
            return GetResult<EntityList>(request, result)._embedded.notes.ToList();
        }
        public IEnumerable<Note> AddNotes(Note note) => AddNotes(new List<Note>() { note });
        public IEnumerable<Note> AddNotes(int id, string comment) => AddNotes(new Note() { entity_id = id, note_type = "service_message", parameters = new Note.Params() { service = "mzpo2amo", text = comment } });

        public IEnumerable<Tag> GetTags()
        {
            var uri = $"{_apiAddress}{_entityLink}/tags";

            var result = GetList(uri);

            if (result._embedded is { } && result._embedded.tags is { })
                return result._embedded.tags.ToList();
            else return null;
        }


        public IEnumerable<Tag> AddTag(IEnumerable<Tag> payload)
        {
            var uri = $"{_apiAddress}{_entityLink}/tags";

            var content = JsonConvert.SerializeObject(payload, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            AmoRequest request = new AmoRequest("POST", uri, content, _auth);
            EntityList result = new EntityList();
            return GetResult<EntityList>(request, result)._embedded.tags.ToList();
        }
        public IEnumerable<Tag> AddTag(Tag newTag) => AddTag(new List<Tag>() { newTag });
        public IEnumerable<Tag> AddTag(string tagName) => AddTag(new Tag() { name = tagName });

        public IEnumerable<CustomField> GetFields()
        {
            var uri = $"{_apiAddress}{_entityLink}/custom_fields";

            var result = GetList(uri);

            if (result._embedded is { } && result._embedded.custom_fields is { })
                return result._embedded.custom_fields.ToList();
            else return null;
        }

        public IEnumerable<CustomField> AddField(IEnumerable<CustomField> payload)
        {
            var uri = $"{_apiAddress}{_entityLink}/custom_fields";

            var content = JsonConvert.SerializeObject(payload, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            AmoRequest request = new AmoRequest("POST", uri, content, _auth);
            EntityList result = new EntityList();
            return GetResult<EntityList>(request, result)._embedded.custom_fields.ToList();
        }
        public IEnumerable<CustomField> AddField(CustomField customField) => AddField(new List<CustomField>() { customField });
        public IEnumerable<CustomField> AddField(string fieldName) => AddField(new CustomField() { name = fieldName, type = "text" });
        
        public void AcceptUnsorted(string uid)
        {
            var uri = $"{_apiAddress}leads/unsorted/{uid}/accept";

            AmoRequest request = new AmoRequest("POST", uri, _auth);

            try { request.GetResponse(); }
            catch (Exception e) { throw new Exception($"Unable to accept unsorted: {e.Message}"); }
        }
        #endregion
    }
}