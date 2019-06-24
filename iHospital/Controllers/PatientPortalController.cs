using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using iHospital.Database;
using iHospital.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace iHospital.Controllers
{
    [Authorize(Roles = "Admin, Patient")]
    public class PatientPortalController : Controller
    {
        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get { return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
            private set { _userManager = value; }
        }


        #region Self-Profile
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();

            var context = new iHospitalDataContext();

            var user = context.AspNetUsers.FirstOrDefault(u => u.Id == userId);

            var userVM = new UserViewModel()
            {
                Username = user.UserName,
                Email = user.Email,
                FName = user.FirstName,
                LName = user.LastName,
                Phone = user.PhoneNumber
            };
            return View(userVM);
        }


        [HttpPost]
        public ActionResult ChangeProfile(UserViewModel model)
        {
            try
            {
                var userId = model.Id;

                var context = new iHospitalDataContext();

                var user = context.AspNetUsers.FirstOrDefault(u => u.Id == userId);

                user.FirstName = model.FName;
                user.LastName = model.LName;
                user.PhoneNumber = model.Phone;

                context.SubmitChanges();

                return Json(new { success = true });
                //return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return Json(new
                {
                    success = false,
                    error = e.Message
                });
            }
        }


        public ActionResult ChangePwd()
        {
            return View();
        }


        [HttpPost]
        public ActionResult ChangePwd(UserViewModel model)
        {
            try
            {
                var userId = User.Identity.GetUserId();

                var result = UserManager.ChangePasswordAsync(userId, model.OldPwd, model.NewPwd);

                return Json(new { success = true });
                //return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return Json(new
                {
                    success = false,
                    error = e.Message
                });
            }
        }
        #endregion


        #region Appointment
        public ActionResult AppointmentHistory()
        {
            var listAppoitmentVms = new List<AppointmentViewModel>();

            var context = new iHospitalDataContext();
            var listAppointments = context.Appointments.ToList();

            if (!User.IsInRole("Admin"))
            {
                var userId = User.Identity.GetUserId();
                listAppointments = listAppointments.Where(a => a.PatientId == userId).ToList();
            }

            foreach (var apm in listAppointments)
            {
                var apmVM = new AppointmentViewModel()
                {
                    Id = apm.Id,

                    PatientFName = apm.Patient.FirstName,
                    PatientLName = apm.Patient.LastName,
                    PatientPhone = apm.Patient.Phone,

                    DoctorFName = apm.Staff.FirstName,
                    DoctorLName = apm.Staff.LastName,
                    DoctorPhone = apm.Staff.Phone,

                    TimeStart = apm.TimeStart.HasValue ? apm.TimeStart.Value.ToString("mm/dd/yyyy hh:mm") : "",
                    TimeEnd = apm.TimeEnd.HasValue ? apm.TimeEnd.Value.ToString("mm/dd/yyyy hh:mm") : "",

                    MedicalProblem = apm.MedicalProblem,

                    DateModified = apm.DateModified.ToString("mm/dd/yyyy hh:mm")
                };

                listAppoitmentVms.Add(apmVM);
            }

            return View(listAppoitmentVms);
        }


        public ActionResult SetAppointment()
        {
            var context = new iHospitalDataContext();

            var listDoctors = context.AspNetUsers.ToList().Where(u =>
            {
                var aspNetUserRole = u.AspNetUserRoles.FirstOrDefault();
                return aspNetUserRole != null && aspNetUserRole.AspNetRole.Name == "MedicalStaff";
            }).ToList();

            return View(listDoctors);
        }


        [HttpPost]
        public ActionResult SetAppointment(string doctor, string date, string time, string problem)
        {
            try
            {
                var userId = User.Identity.GetUserId();

                var datetime = Convert.ToDateTime(date + " " + time);

                var apm = new Appointment()
                {
                    Id = Guid.NewGuid().ToString(),
                    PatientId = userId,
                    DoctorId = doctor,
                    SpecialtyId = "test",
                    TimeStart = datetime,
                    TimeEnd = datetime.AddMinutes(30),
                    MedicalProblem = problem,
                    DateCreated = DateTime.Now,
                    DateModified = DateTime.Now,
                };

                var context = new iHospitalDataContext();
                context.Appointments.InsertOnSubmit(apm);
                context.SubmitChanges();

                return Json(new { success = true });
                //return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return Json(new
                {
                    success = false,
                    error = e.Message
                });
            }
        }
        #endregion
    }
}