﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using CourseCenter.Models;
using CourseCenter.Common;
using System.Data.SqlClient;


namespace CourseCenter.Controllers
{
    public class LearnerManageController : Controller
    {
        DBEntities db = new DBEntities();
        string teacherId = TakeCookie.GetCookie("userId");
        ModelHelpers mHelp = new ModelHelpers();

        #region 查询出该教师所有的课程 放在下拉框中，列表中默认显示第一门课程的学生情况+Learners
        /// <summary>
        /// 显示所有的信息，这个是初始化的，首先显示教师的正在上课
        /// 的一门的学生
        /// 获得第一个~~~top 1，where 开课状态为正在上课，课程的学生的信息
        /// </summary>
        /// <returns></returns>
        public ActionResult Learners()
        {
            //查询出该老师所有的课程，根据开始时间排序
            List<Course> listCourse = db.Course.Where(c => c.TeacherId == new Guid(teacherId)).OrderBy(c => c.BeginTime).ToList();
            ViewBag.listCourse = listCourse;
            if (listCourse.Count > 0)
            {
                //页面默认显示第一门课程
                Course firsrCourse = listCourse[0] as Course;
                List<StudentInfo> listStudentInfoOfIndex = db.StudentInfo.SqlQuery("select * from StudentInfoes where Id in (select StudentId from Stu_Course where CourseId=@Id )", new SqlParameter("@Id", firsrCourse.Id)).ToList();
                ViewBag.listStudentInfoOfIndex = listStudentInfoOfIndex;
                ViewBag.CourseId = firsrCourse.Id;
            }
            return View();
        }

        #endregion

        #region 根据点击下拉框的值 获取对应课程的学生情况+GetCourse
        public ActionResult GetCourse(int id)
        {
            List<StudentInfo> listStudentInfoOfIndex = db.StudentInfo.SqlQuery("select * from StudentInfoes where Id in (select StudentId from Stu_Course where Stu_Course.CourseId=@id )", new SqlParameter("@Id", id)).ToList();
            ViewBag.listStudentInfoOfIndex = listStudentInfoOfIndex;
            //查询出该老师所有的课程，根据开始时间排序
            List<Course> listCourse = db.Course.Where(c => c.TeacherId == new Guid(teacherId)).OrderBy(c => c.BeginTime).ToList();
            ViewBag.listCourse = listCourse;
            ViewBag.CourseId = id;
            return View("Learners");
        }
        #endregion

        #region 教师查看学生提交的作业情况+GetStudentWork
        /// <summary>
        /// 获得学生的上传作业的情况
        /// </summary>
        /// <returns></returns>
        public ActionResult GetStudentWork()
        {
            Guid studentId = new Guid(Request.QueryString["SId"]);
            int courseId = Convert.ToInt32(Request.QueryString["CId"]);
            List<StudentWork> listStudentWork = db.StudentWork.Where(sw => sw.CourseId == courseId && sw.StudentId == studentId).ToList();
            ViewBag.listStudentWork = listStudentWork;


            return View();
        }

        #endregion

        #region 教师设置学生成绩+SetScore
        public string SetScore(FormCollection form)
        {
            int courseId = Convert.ToInt32(form["CourseId"]);
            Guid studentId = new Guid(form["StudentId"]);
            int moduleTag = Convert.ToInt32(form["ModuleTag"]);

            CouScore cScore = db.CouScore.Where(c => c.CourseId == courseId && c.StudentId == studentId && c.ModuleTag == moduleTag).FirstOrDefault();
            if (cScore != null)
            {
                cScore.ModuleScore = form["score"];
            }
            else
            {
                cScore = new CouScore()
                {
                    CourseId = courseId,
                    StudentId = studentId,
                    ModuleTag = moduleTag,
                    ModuleScore = form["score"]
                };
                db.CouScore.Add(cScore);

            }
            db.SaveChanges();
            return "OK";
        }
        #endregion


        #region 教师查看学生成绩+GetStudentScore
        public ActionResult GetStudentScore()
        {
            Guid studentId = new Guid(Request.QueryString["SId"]);
            int courseId = Convert.ToInt32(Request.QueryString["CId"]);
            List<CouScore> listCs = db.CouScore.Where(cs => cs.CourseId == courseId && cs.StudentId == studentId).OrderBy(cs => cs.ModuleTag).ToList();
            ViewBag.listCs = listCs;
            string courseName = db.Course.Where(c => c.Id == courseId).FirstOrDefault().CourseName;
            ViewBag.courseName = courseName;
            return View();
        } 
        #endregion

    }
}
