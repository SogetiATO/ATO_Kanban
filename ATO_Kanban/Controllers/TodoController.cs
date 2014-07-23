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
    public class TodoController : ApiController
    {
        private DataContext db = new DataContext();

        // GET api/Todo
        public IEnumerable<Todo> GetTodoes(string status = null)
        {
            var list = ((IObjectContextAdapter)db).ObjectContext.CreateObjectSet<Todo>();

            IQueryable<Todo> items = list.OrderByDescending(o => o.Priority.ID).ThenBy(o => o.CreateDate);
            items = items.Where(i => i.Status.Type == status);

            return items;
        }

        // GET api/Todo/5
        public Todo GetTodo(int id)
        {
            Todo todo = db.Todoes.Find(id);
            if (todo == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return todo;
        }

        // PUT api/Todo/5
        public HttpResponseMessage PutTodo(int id, Todo todo)
        {
            DataContext dataContext = new DataContext();

            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            if (id != todo.ID)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            var entry = db.Entry(todo);

            if (entry.State == EntityState.Detached)
            {
                var set = db.Set<Todo>();
                Todo attachedEntity = set.Find(todo.ID);  // You need to have access to key
                if (attachedEntity != null)
                {
                    attachedEntity.StatusID = todo.Status.ID;
                }
                else
                {
                    db.Todoes.Attach(todo);
                    entry.State = EntityState.Modified; // This should attach entity
                }
            }

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

        // POST api/Todo
        public HttpResponseMessage PostTodo(Todo todo)
        {
            if (ModelState.IsValid)
            {
                db.Todoes.Add(todo);
                db.SaveChanges();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, todo);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = todo.ID }));
                return response;
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
        }

        // DELETE api/Todo/5
        public HttpResponseMessage DeleteTodo(int id)
        {
            Todo todo = db.Todoes.Find(id);
            if (todo == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            db.Todoes.Remove(todo);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK, todo);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}