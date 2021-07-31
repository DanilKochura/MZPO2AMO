﻿using MZPO.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

namespace Integration1C
{
    internal class CourseRepository
    {
        private readonly Cred1C _cred1C;

        public CourseRepository(Cred1C cred1C)
        {
            _cred1C = cred1C;
        }

        public class Result
        {
            public Guid product_id_1C { get; set; }
        }

        internal Course1C GetCourse(Course1C course) => GetCourse((Guid)course.product_id_1C);

        internal Course1C GetCourse(Guid course_id)
        {
            string method = $"EditCourse?uid={course_id:D}";
            Request1C request = new("GET", method, _cred1C);
            Course1C result = new();
            var response = request.GetResponse();
            try { JsonConvert.PopulateObject(WebUtility.UrlDecode(response), result); }
            catch (Exception e) { throw new Exception($"Unable to process response from 1C: {e.Message}, Response: {response}"); }
            return result;
        }

        internal Guid UpdateCourse(Course1C course)
        {
            if (course.product_id_1C is null ||
                course.product_id_1C == default)
                throw new Exception("Unable to update 1C client, no UID.");

            string method = "EditCourse";
            string content = JsonConvert.SerializeObject(course, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include });
            Request1C request = new("POST", method, content, _cred1C);

            Result result = new();
            var response = request.GetResponse();
            try { JsonConvert.PopulateObject(WebUtility.UrlDecode(response), result); }
            catch (Exception e) { throw new Exception($"Unable to process response from 1C: {e.Message}, Response: {response}"); }
            return result.product_id_1C;
        }

        internal IEnumerable<Course1C> GetAllCourses()
        {
            throw new NotImplementedException();

            string method = "";
            Request1C request = new("GET", method, _cred1C);
            List<Course1C> result = new();
            var response = request.GetResponse();
            try { JsonConvert.PopulateObject(WebUtility.UrlDecode(response), result); }
            catch (Exception e) { throw new Exception($"Unable to process response from 1C: {e.Message}, Response: {response}"); }
            return result;
        }
    }
}