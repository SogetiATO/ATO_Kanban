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

namespace ATO_Kanban.Models
{
    public class UserController : ApiController
    {
        private DataContext db = new DataContext();

        // GET api/User
        public IEnumerable<User> GetUsers()
        {
            List<User> userList = db.Users.ToList();

            userList.ForEach(u => u.Password = "");

            //foreach (User user in userList)
            //{
            //    user.Password = "";
            //}
            
            return userList;
        }

        // GET api/User/5
        public User GetUser(int id)
        {
            User user = db.Users.Find(id);
            if (user == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            user.Password = "";
            return user;
        }

        public User GetUser(string userName, string password)
        {
            User user = db.Users.Where(u => u.Username == userName).FirstOrDefault();
            if (user == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            if (password == user.Password)
            {
                user.Password = "";
                return user;
            }
            else
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Forbidden));
            }
        }

        // PUT api/User/5
        public HttpResponseMessage PutUser(int id, User user)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            if (id != user.ID)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            var entry = db.Entry(user);

            if (entry.State == EntityState.Detached)
            {
                var set = db.Set<User>();
                User attachedEntity = set.Find(user.ID);  // You need to have access to key
                if (attachedEntity != null)
                {
                    // Update User info
                    attachedEntity.Name = user.Name;
                    attachedEntity.Grade = user.Grade;
                    attachedEntity.Username = user.Username;
                }
                else
                {
                    db.Users.Attach(user);
                    db.Entry(user).State = EntityState.Modified;
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

        // POST api/User
        public HttpResponseMessage PostUser(User user)
        {
            if (ModelState.IsValid)
            {
                User existingUser = db.Users.Where(u => u.Username == user.Username).FirstOrDefault();
                HttpResponseMessage response  = null;
                if (existingUser == null)
                {
                    db.Users.Add(user);
                    db.SaveChanges();
                    response = Request.CreateResponse(HttpStatusCode.Created, user);
                    response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = user.ID }));
                }
                else
                {
                    response = Request.CreateResponse(HttpStatusCode.Conflict, user);
                    response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = existingUser.ID }));
                }
                return response;
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
        }

        // DELETE api/User/5
        public HttpResponseMessage DeleteUser(int id)
        {
            User user = db.Users.Find(id);
            if (user == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            db.Users.Remove(user);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK, user);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}