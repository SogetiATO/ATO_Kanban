﻿using System;
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
using System.Net.Mail;

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
            
            //SmtpClient smtpClient = new SmtpClient("smtp.gmail.com");
            //smtpClient.EnableSsl = true;
            //smtpClient.Credentials = new System.Net.NetworkCredential("SogetiATO@gmail.com", "password");
            //smtpClient.Port = 587;
            //smtpClient.Send(message);


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
                    // Update status
                    attachedEntity.StatusID = todo.Status.ID;

                    // Update reason for revision if one exists
                    if (!String.IsNullOrEmpty(todo.ReasonForRevision))
                    {
                        attachedEntity.ReasonForRevision = todo.ReasonForRevision;
                    }
                    
                    // Update ClaimedByID when claimed
                    if (todo.ClaimedByID != null)
                    {
                        attachedEntity.ClaimedByID = todo.ClaimedByID;
                    }

                    if (todo.Status.ID == 2)
                    {
                        var test = "test";
                    }
                    else if (todo.Status.ID == 4)
                    {
                        // Update FinishDate if Status == 4
                        attachedEntity.FinishDate = DateTime.Now;
                    }
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
            Todo newTodo = new Todo();
            newTodo.AssigneeID = todo.AssigneeID;
            newTodo.CreateDate = DateTime.Now;
            newTodo.Description = todo.Description;
            newTodo.Title = todo.Title;
            newTodo.StatusID = 1;
            newTodo.PriorityID = todo.PriorityID;

            if (todo.EmailATO)
            {
                var smtpClient = new SmtpClient();
                MailMessage message = new MailMessage()
                {
                    From = new MailAddress("SogetiATO@gmail.com"),
                    Subject = newTodo.Title,
                    Body = newTodo.Description
                };
                message.To.Add(new MailAddress("Evan.Lepolt@gmail.com"));
                smtpClient.Send(message);
            }

            
            // If userGrade does not equal C, then all the following options will be forced to false;
            string userGrade = db.Users.Find(todo.AssigneeID).Grade;
            if (userGrade == "C")
            {
                newTodo.EmailATO = todo.EmailATO;
                newTodo.IsPublic = todo.IsPublic;
                newTodo.Optional = todo.Optional;
                newTodo.ReasonForRevision = todo.ReasonForRevision;
                newTodo.RequiresApproval = todo.RequiresApproval;
            }

            try
            {
                db.Todoes.Add(newTodo);
                db.SaveChanges();
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, newTodo);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = newTodo.ID }));
                return response;
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, e.InnerException.ToString());
            }

            //if (ModelState.IsValid)
            //{
            //    db.Todoes.Add(todo);
            //    db.SaveChanges();

            //    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, todo);
            //    response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = todo.ID }));
            //    return response;
            //}
            //else
            //{
            //    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            //}
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