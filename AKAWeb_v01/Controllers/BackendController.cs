﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AKAWeb_v01.Controllers
{
    public class BackendController : Controller
    {
        private string username = "admin";
        private string password = "admin";

        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            try
            {
                if((username == this.username) && (password == this.password))
                {
                    System.Web.HttpContext.Current.Session["userpermission"] = "3";
                    
                    ViewData["sessionString"] = System.Web.HttpContext.Current.Session["userpermission"];
                    
                    return RedirectToAction("EditCarousel");

                }
                else
                {
                    ViewBag.Message = "Wrong Credentials";
                    return RedirectToAction("Index");
                }
                
            }
            catch
            {
                ViewBag.Message = "Something went wrong while validating";
                return View("~/Views/Backend/Index.cshtml");
            }
            
        }
        public ActionResult EditCarousel()
        {
            String userpermission = ""; 
            if (System.Web.HttpContext.Current.Session["userpermission"] != null)
            {
                userpermission = System.Web.HttpContext.Current.Session["userpermission"] as String;
            }
            if (userpermission.Equals("3"))
            {
           
                return View();
            }
            else
                return RedirectToAction("Index");
        }
        // GET: Backend
        public ActionResult Index()
        {
            return View();
        }

        // GET: Backend/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Backend/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Backend/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Backend/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Backend/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Backend/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Backend/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
