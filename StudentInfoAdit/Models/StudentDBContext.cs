using System.Data.Entity;

namespace StudentInfoAdit.Models
{
    public class StudentDBContext : DbContext
    {
        public StudentDBContext()
            : base("StudentDB")
        {
            Database.SetInitializer<StudentDBContext>(null);
        } 
        
        public DbSet<StudentModel> Students { get; set; }
        public DbSet<ParentModel> Parents { get; set; }
        public DbSet<StaffModel> Staff { get; set; }
        public DbSet<AttendanceModel> Attendances { get; set; }
        public DbSet<FeeStructure> FeeStructures { get; set; }
        public DbSet<FeePayment> FeePayments { get; set; }
        public DbSet<ExamModel> Exams { get; set; }
        public DbSet<ExamResultModel> ExamResults { get; set; }
        public DbSet<SubjectModel> Subjects { get; set; }
        public DbSet<BookModel> Books { get; set; }
        public DbSet<BookIssuedModel> BookIssueds { get; set; }
        public DbSet<NoticeModel> Notices { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<TimetableModel> Timetables { get; set; }
        public DbSet<AssignmentModel> Assignments { get; set; }
        public DbSet<AssignmentSubmissionModel> AssignmentSubmissions { get; set; }
        public DbSet<PasswordResetRequestModel> PasswordResetRequests { get; set; }
        public DbSet<SalaryStructureModel> SalaryStructures { get; set; }
        public DbSet<SalaryPaymentModel> SalaryPayments { get; set; }
        public DbSet<SalarySlipModel> SalarySlips { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<StudentModel>()
                .HasOptional(s => s.Parent)
                .WithRequired(p => p.Student);

            modelBuilder.Entity<AttendanceModel>()
                .HasRequired(a => a.Student)
                .WithMany()
                .HasForeignKey(a => a.StudentId);

            modelBuilder.Entity<BookIssuedModel>() 
                .HasRequired(b => b.Student)       
                .WithMany()                        
                .HasForeignKey(b => b.StudentId);  
          
            modelBuilder.Entity<BookIssuedModel>()
                .HasRequired(b => b.Book)
                .WithMany()
                .HasForeignKey(b => b.BookId);

            modelBuilder.Entity<FeePayment>()
                .HasRequired(p => p.FeeStructure)
                .WithMany(f => f.Payments)
                .HasForeignKey(p => p.FeeId);
                
            modelBuilder.Entity<ExamResultModel>()
                .HasRequired(r => r.Exam)
                .WithMany(e => e.Results)
                .HasForeignKey(r => r.ExamId);

            modelBuilder.Entity<ExamResultModel>()
                .HasRequired(r => r.Student)
                .WithMany()
                .HasForeignKey(r => r.StudentId);

            modelBuilder.Entity<User>()
                .Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(50);
            
            modelBuilder.Entity<User>()
                .Property(u => u.PasswordHash)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .IsRequired()
                .HasMaxLength(20);
        }
    }
}

// i am developing a school management system using asp.net mvc 5 c# where there are currently 7 modules(student details, parent details, staff details, attendance module, fee module, exam module, notice module) which have been almost implemented and i just added a login feature where there are 4 type of users which are admin, teacher, parent and accountant. earlier it had no user portal, there was a dashboard which had links to all the modules and it had the authorization to edit all the modules and reflect the updates on the actual database created using ms sql. Currently there is only one user which is an admin that can currently only view the dashboard. There should be an option of ERP at the upper right corner of the webpage which opens that webpage from where the admin gets the authority to modify all the modules then an option at the same place wen clicked gets back to the viewing page . Similarly I want for other type of users too but with different authorization to them. Firstly I want to develop that for admin. Shall I share the necessary codes so that you can help me out