using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.Linq;
using StudentInfoAdit.Models;
namespace StudentInfoAdit.Controllers
{
    [Authorize]
    public class AttendanceController : Controller
    {
        private StudentDBContext db = new StudentDBContext();

        public ActionResult AttendanceDetails(string searchName)
        {
            ViewBag.StudentList = db.Students.ToList();
            string cs = ConfigurationManager.ConnectionStrings["StudentDB"].ConnectionString;
            List<AttendanceModel> attendanceList =
                new List<AttendanceModel>();
            using (SqlConnection con =
                new SqlConnection(cs))
            {
                string query = @"
                SELECT
                    a.StudentId,
                    s.StudentName,
                    a.AttendanceDate,
                    a.InTime,
                    a.OutTime,
                    a.IsPresent
                FROM Attendance as a
                INNER JOIN Student s
                    ON a.StudentId = s.StudentId
                WHERE
                    @SearchName IS NULL
                    OR s.StudentName LIKE '%' + @SearchName + '%'
                ORDER BY
                    a.AttendanceDate DESC";
                SqlCommand cmd =
                    new SqlCommand(query, con);
                if (string.IsNullOrWhiteSpace(searchName))
                {
                    cmd.Parameters.AddWithValue(
                        "@SearchName",
                        DBNull.Value);
                }
                else
                { 
                    cmd.Parameters.AddWithValue(
                        "@SearchName", 
                        searchName);
                }
                con.Open();
                SqlDataReader dr =
                    cmd.ExecuteReader();

                while (dr.Read())
                {
                    attendanceList.Add(
                        new AttendanceModel
                        {
                            StudentId =
                                Convert.ToInt32(
                                    dr["StudentId"]),

                            StudentName =
                                dr["StudentName"]
                                .ToString(),

                            AttendanceDate =
                                Convert.ToDateTime(
                                    dr["AttendanceDate"]),

                            InTime =
                                dr["InTime"] ==
                                DBNull.Value
                                ? (TimeSpan?)null
                                : (TimeSpan)dr["InTime"],
                                                                            
                            OutTime =
                                dr["OutTime"] ==
                                DBNull.Value
                                ? (TimeSpan?)null
                                : (TimeSpan)dr["OutTime"],

                            IsPresent =
                                Convert.ToBoolean(
                                    dr["IsPresent"])
                        });
                }
            }
            ViewBag.AttendanceList = attendanceList;

            return View(new AttendanceModel{AttendanceDate = DateTime.Today});
        }

        [HttpPost]
        public ActionResult SaveAttendance(AttendanceModel model)
        {
            string cs = ConfigurationManager.ConnectionStrings["StudentDB"].ConnectionString;
            using (SqlConnection con =
                new SqlConnection(cs))
            {
                con.Open();
                string checkQuery = @"
                SELECT COUNT(*)
                FROM Attendance
                WHERE StudentId=@StudentId
                AND AttendanceDate=@AttendanceDate";
                
                SqlCommand checkCmd =
                    new SqlCommand(
                        checkQuery,
                        con);
 
                checkCmd.Parameters.AddWithValue(
                    "@StudentId",
                    model.StudentId);
               
                checkCmd.Parameters.AddWithValue(
                    "@AttendanceDate",
                    model.AttendanceDate.Date);
                int count = (int)checkCmd.ExecuteScalar();
                
                if (count > 0)
                {
                    TempData["Error"] =
                        "Attendance already exists for this date.";

                    return RedirectToAction(
                        "AttendanceDetails");
                }

                string insertQuery = @"
                INSERT INTO Attendance
                (
                    StudentId,
                    AttendanceDate,
                    InTime,
                    OutTime,
                    IsPresent 
                )
                VALUES
                ( 
                    @StudentId,
                    @AttendanceDate,
                    @InTime,
                    @OutTime,
                    @IsPresent
                )";
                                                                                                                                                                                                                                 
                SqlCommand cmd =
                    new SqlCommand(
                        insertQuery,
                        con);  

                cmd.Parameters.AddWithValue(
                    "@StudentId",
                    model.StudentId);

                cmd.Parameters.AddWithValue(
                    "@AttendanceDate",
                    model.AttendanceDate.Date);

                cmd.Parameters.AddWithValue(
                    "@InTime",
                    (object)model.InTime ??
                    DBNull.Value);

                cmd.Parameters.AddWithValue(
                    "@OutTime",
                    (object)model.OutTime ??
                    DBNull.Value);
                
                cmd.Parameters.AddWithValue(
                    "@IsPresent",
                    model.IsPresent);
                cmd.ExecuteNonQuery();
            }
            
            return RedirectToAction(
                "AttendanceDetails");
        }
    }
}