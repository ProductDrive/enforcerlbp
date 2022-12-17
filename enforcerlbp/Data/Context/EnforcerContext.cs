using Entities;
using Entities.CompanySetup;
using Entities.Consultation;
using Entities.Documents;
using Entities.Users;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Context
{
    public class EnforcerContext: IdentityDbContext<EnforcerUser>
    {
        public EnforcerContext(DbContextOptions<EnforcerContext> options) : base(options)
        {

        }
        public DbSet<Patient>Patients  { get; set; }
        public DbSet<Physiotherapist>Physiotherapists  { get; set; }
        public DbSet<VerificationDocument> VerificationDocuments { get; set; }
        public DbSet<ConsultationService> ConsultationServices{ get; set; }
        public DbSet<PhysioSession> PhysioSessions{ get; set; }
        public DbSet<ProcessSetting>ProcessSettings { get; set; }

        //Exercise
        public DbSet<Exercise> Exercises{ get; set; }
        public DbSet<ExercisePrescription> ExercisePrescriptions { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<FeedbackReply> FeedbackReplies{ get; set; }






    }
}
