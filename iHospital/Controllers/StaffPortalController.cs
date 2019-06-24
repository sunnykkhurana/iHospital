using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using iHospital.Database;
using iHospital.Models;
using Microsoft.AspNet.Identity;
using System.Web;
using Microsoft.AspNet.Identity.Owin;

namespace iHospital.Controllers
{
    [Authorize(Roles = "Admin, AdministrativeStaff, MedicalStaff")]
    public class StaffPortalController : Controller
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
        [Authorize(Roles = "Admin, MedicalStaff")]
        public ActionResult ManageAppointment()
        {
            var listAppoitmentVms = new List<AppointmentViewModel>();

            var context = new iHospitalDataContext();
            var listAppointments = context.Appointments.ToList();

            if (!User.IsInRole("Admin"))
            {
                var userId = User.Identity.GetUserId();
                listAppointments = listAppointments.Where(a => a.DoctorId == userId).ToList();
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
        #endregion


        #region Staff
        [Authorize(Roles = "Admin, AdministrativeStaff")]
        public ActionResult ManageStaff()
        {
            var listUserVms = new List<UserViewModel>();

            var context = new iHospitalDataContext();
            var listUsers = context.AspNetUsers.ToList();

            foreach (var user in listUsers)
            {
                var role = user.AspNetUserRoles.FirstOrDefault();

                var userVM = new UserViewModel()
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Role = role != null ? role.AspNetRole.Name : "",
                    FName = user.FirstName,
                    LName = user.LastName,
                    Email = user.Email,
                    Phone = user.PhoneNumber
                };

                listUserVms.Add(userVM);
            }

            return View(listUserVms);
        }


        [Authorize(Roles = "Admin, AdministrativeStaff")]
        public ActionResult AddStaff()
        {
            return View();
        }


        [Authorize(Roles = "Admin, AdministrativeStaff")]
        [HttpPost]
        public async System.Threading.Tasks.Task<ActionResult> AddStaffAsync(UserViewModel model)
        {
            try
            {
                var userApp = new ApplicationUser { UserName = model.Email, Email = model.Email, LockoutEnabled = false };
                var result = await UserManager.CreateAsync(userApp, model.NewPwd);

                if (result.Succeeded)
                {
                    await UserManager.AddToRoleAsync(userApp.Id, model.Role);

                    var context = new iHospitalDataContext();

                    var user = context.AspNetUsers.FirstOrDefault(u => u.Id == userApp.Id);
                    user.FirstName = model.FName;
                    user.LastName = model.LName;
                    user.PhoneNumber = model.Phone;

                    var s = new Staff()
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Phone = user.PhoneNumber,
                        DepartmentId = "test",
                        Email = user.Email,
                        DateModified = DateTime.Now,
                        DateCreated = DateTime.Now
                    };
                    context.Staffs.InsertOnSubmit(s);
                    context.SubmitChanges();

                    return Json(new { success = true });
                }

                return Json(new { success = false, error = result.Errors.FirstOrDefault() });
            }
            catch (Exception e)
            {
                return Json(new { success = false, error = e.Message });
            }
        }


        [Authorize(Roles = "Admin, AdministrativeStaff")]
        public ActionResult EditStaff(string Id)
        {
            try
            {
                var context = new iHospitalDataContext();

                var user = context.AspNetUsers.FirstOrDefault(u => u.Id == Id);

                var userVM = new UserViewModel()
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    FName = user.FirstName,
                    LName = user.LastName,
                    Phone = user.PhoneNumber
                };

                return View(userVM);
            }
            catch (Exception e)
            {
                return View();
            }
        }
        #endregion
    }
}