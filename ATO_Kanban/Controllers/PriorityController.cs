using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using ATO_Kanban.Models;

namespace ATO_Kanban.Controllers
{
    public class PriorityController : ApiController
    {
        private DataContext db = new DataContext();

        // GET api/Priority
        public IEnumerable<Priority> GetPriorities()
        {
            return db.Priorities.AsEnumerable();
        }

        // GET api/Priority/5
        public Priority GetPriority(int id)
        {
            Priority priority = db.Priorities.Find(id);
            if (priority == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return priority;
        }

        // PUT api/Priority/5
        public HttpResponseMessage PutPriority(int id, Priority priority)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            if (id != priority.ID)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            db.Entry(priority).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // POST api/Priority
        public HttpResponseMessage PostPriority(Priority priority)
        {
            if (ModelState.IsValid)
            {
                db.Priorities.Add(priority);
                db.SaveChanges();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, priority);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = priority.ID }));
                return response;
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
        }

        // DELETE api/Priority/5
        public HttpResponseMessage DeletePriority(int id)
        {
            Priority priority = db.Priorities.Find(id);
            if (priority == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            db.Priorities.Remove(priority);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK, priority);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}