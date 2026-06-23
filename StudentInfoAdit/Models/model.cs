using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Web.Mvc;

namespace StudentInfoAdit.Models
{
    [Table("Student")]
    public class StudentModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string PresentAddress { get; set; }
        public string StudentClass { get; set; }
        public string MobileNumber { get; set; }
        public string PhotoPath { get; set; }
        public virtual ParentModel Parent { get; set; }
    }

    [Table("Parent")]
    public class ParentModel
    {
        [Key]
        [ForeignKey("Student")]
        public int StudentId { get; set; }
        public string FatherName { get; set; }
        public string FOccupation { get; set; }
        public string FMobileno { get; set; }
        public string MotherName { get; set; }
        public string MOccupation { get; set; }
        public string MMobileno { get; set; }
        public virtual StudentModel Student { get; set; }
    }

    [Table("Staff")]
    public class StaffModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StaffId { get; set; }

        [Required]
        public string StaffName { get; set; }

        [Required]
        public string Role { get; set; }

        public string Subject { get; set; }

        [Required]
        public DateTime? DateJoined { get; set; }
    }

    public class StudentViewModel
    {
        public StudentModel Student { get; set; }
        public ParentModel Parent { get; set; }
        public List<StudentModel> StudentList { get; set; }
        public List<SelectListItem> ClassList { get; set; }
    }

    [Table("Attendance")]
    public class AttendanceModel
    {
        [Key]
        public int Id { get; set; }

        public int StudentId { get; set; }

        public DateTime AttendanceDate { get; set; }

        public TimeSpan? InTime { get; set; }

        public TimeSpan? OutTime { get; set; }

        public bool IsPresent { get; set; }

        [ForeignKey("StudentId")]
        public virtual StudentModel Student { get; set; }
    }

    public class AttendanceReportViewModel
    {
        public StudentModel Student { get; set; }

        public List<AttendanceModel> AttendanceRecords { get; set; }

        public int TotalDays { get; set; }

        public int PresentDays { get; set; }

        public double AttendancePercentage { get; set; }
    }

    [Table("FeeStructure")]
    public class FeeStructure
    {
        [Key]
        public int FeeId { get; set; }

        public int StudentId { get; set; }

        public string AcademicYear { get; set; }

        public decimal TotalFee { get; set; }

        public virtual StudentModel Student { get; set; }

        public virtual ICollection<FeePayment> Payments { get; set; }
    }


    [Table("FeePayment")]
    public class FeePayment
    {
        [Key]
        public int PaymentId { get; set; }

        [ForeignKey("FeeStructure")]
        public int FeeId { get; set; }

        public decimal AmountPaid { get; set; }

        public DateTime? PaymentDate { get; set; }

        public string PaymentMode { get; set; }
  
        public virtual FeeStructure FeeStructure { get; set; }
    }

    public class FeeViewModel
    {
        public FeeViewModel()
        {
            Payments = new List<FeePayment>();
        }

        public StudentModel Student { get; set; }

        public FeeStructure Fee { get; set; }

        public decimal TotalPaid { get; set; }

        public decimal Balance { get; set; }

        public List<FeePayment> Payments { get; set; }
    }

    [Table("Exam")]
    public class ExamModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ExamId { get; set; }

        public string ExamName { get; set; }

        public DateTime ExamDate { get; set; }

        public string StudentClass { get; set; }

        public virtual ICollection<ExamResultModel> Results { get; set; }
    }

    [Table("ExamResult")]
    public class ExamResultModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ResultId { get; set; }

        public int ExamId { get; set; }

        public int StudentId { get; set; }

        public string Subject { get; set; }

        public int MarksObtained { get; set; }

        public int MaxMarks { get; set; }

        [ForeignKey("ExamId")]
        public virtual ExamModel Exam { get; set; }

        [ForeignKey("StudentId")]
        public virtual StudentModel Student { get; set; }
    }

    [Table("Subject")]
    public class SubjectModel
    {
        [Key]
        public int SubjectId { get; set; }

        public string SubjectName { get; set; }

        public string StudentClass { get; set; }
    }

    public class ExamViewModel
    {
        public StudentModel Student { get; set; }
        public ExamModel Exam { get; set; } 
        public List<ExamResultModel> Results { get; set; }
        public decimal Percentage { get; set; }
        public string Grade { get; set; }
    }

    [Table("Book")]
    public class BookModel
    {
        [Key]
        public int BookId { get; set; }
        
        [Required]
        public string BookName { get; set; }
        public string Author { get; set; }
        public string Subject { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
    }

    [Table("BookIssued")]
    public class BookIssuedModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IssueId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int BookId { get; set; }

        public DateTime IssueDate { get; set; }

        public DateTime DueDate { get; set; }

        public bool Returned { get; set; }

        [ForeignKey("StudentId")]
        public virtual StudentModel Student { get; set; }

        [ForeignKey("BookId")]
        public virtual BookModel Book { get; set; }
    }

    public class IssueBookViewModel
    {
        public int StudentId { get; set; }
        public int BookId { get; set; }
        public DateTime DueDate { get; set; }
        public List<SelectListItem> StudentList { get; set; }
        public List<SelectListItem> BookList { get; set; }
    }

    [Table("Notice")]
    public class NoticeModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NoticeId { get; set; }
        
        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public DateTime PublishDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public string CreatedBy { get; set; }

        public bool IsActive { get; set; }
    }

    [Table("Users")]
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [StringLength(500)]
        public string PasswordHash { get; set; }

        [Required]
        [StringLength(200)]
        public string PasswordSalt { get; set; }

        [Required]
        [StringLength(50)]
        public string Role { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? LastLogin { get; set; }

        public int FailedLoginAttempts { get; set; }

        public bool IsLocked { get; set; }

        public int? StudentId { get; set; }

        [ForeignKey("StudentId")]
        public virtual StudentModel Student { get; set; }
    }

    [Table("Timetable")]
    public class TimetableModel
    {
        [Key]
        public int TimetableId { get; set; }

        [Required]
        public string Class { get; set; }

        [Required]
        public string DayOfWeek { get; set; }

        [Required]
        public int PeriodNo { get; set; }

        [Required]
        public string Subject { get; set; }

        public string TeacherName { get; set; }

        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
    }

    public class TimetableGridViewModel
    {
        public List<string> Days { get; set; }
        public List<int> Periods { get; set; }
        public Dictionary<string, TimetableModel> GridData { get; set; }
        public string SelectedClass { get; set; }
    }

    public class CharacterViewModel
    {
        public string Name { get; set; }
        public string House { get; set; }
        public string Image { get; set; }
    }

    [Table("Assignment")]
    public class AssignmentModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AssignmentId { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public string Subject { get; set; }
        public string StudentClass { get; set; }

        public DateTime DueDate { get; set; }

        public string AttachmentPath { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    [Table("AssignmentSubmission")]
    public class AssignmentSubmissionModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SubmissionId { get; set; }

        public int AssignmentId { get; set; }

        public string SubmittedBy { get; set; }  

        public string FilePath { get; set; }

        public DateTime SubmissionDate { get; set; }

        [ForeignKey("AssignmentId")]
        public virtual AssignmentModel Assignment { get; set; }
    }

    [Table("PasswordResetRequest")]
    public class PasswordResetRequestModel
    {
        [Key]
        public int RequestId { get; set; }

        public string Username { get; set; }

        public DateTime RequestDate { get; set; }

        public bool IsProcessed { get; set; }
    }
}