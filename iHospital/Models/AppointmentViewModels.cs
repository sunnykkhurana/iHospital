using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace iHospital.Models
{
    public class AppointmentViewModel
    {
        public string Id { get; set; }

        public string PatientFName { get; set; }
        public string PatientLName { get; set; }
        public string PatientPhone { get; set; }

        public string DoctorFName { get; set; }
        public string DoctorLName { get; set; }
        public string DoctorPhone { get; set; }

        public string TimeStart { get; set; }
        public string TimeEnd { get; set; }

        public string MedicalProblem { get; set; }

        public string DateModified { get; set; }
    }
}