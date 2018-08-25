﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChoresAndFulfillment.Data;
using ChoresAndFulfillment.Data.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChoresAndFulfillment.Controllers
{
    public class ListMyJobsController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _applicationDbContext;
        public ListMyJobsController(UserManager<User> userManager, ApplicationDbContext applicationDbContext)
        {
            this._userManager = userManager;
            this._applicationDbContext = applicationDbContext;
        }
        [HttpGet]
        [Authorize]
        public IActionResult Index()
        {
            if (!IsWorker())
            {
                return Redirect("/");
            }
            User user = this._userManager.GetUserAsync(HttpContext.User).Result;
            WorkerAccount workerAccount =
                _applicationDbContext.
                WorkerAccounts.Where(a => a.Id == user.WorkerAccountId).
                Include(a => a.Jobs).First();
            if (!workerAccount.Jobs.Any(a=>a.JobState==JobState.Finished))
            {
                ViewData["Jobs"] = "<h2>You have no finished jobs!</h2>";
                return View();
            }
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var job in workerAccount.Jobs.Where(a => a.JobState == JobState.Active))
            {
                stringBuilder.AppendLine("<li>");
                stringBuilder.AppendLine("<a href=\"/WorkerFinishedJob/Index/" + job.Id + "\">" + job.Name + "</a>");
                stringBuilder.AppendLine("</li>");
            }
            ViewData["Jobs"] = stringBuilder.ToString();

            return View();
        }
        private bool IsWorker()
        {
            var currentUser = _userManager.GetUserAsync(HttpContext.User).Result;
            if (currentUser.AccountType == "EmployerAccount")
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}