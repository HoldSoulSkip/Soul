﻿using CourseCenter.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CourseCenter.Models;
using System.Data.SqlClient;

using System.Data.Entity.Infrastructure;

namespace CourseCenter.Controllers
{
    public class StudentCourseController : Controller
    {

        DBEntities db = new DBEntities();
        //获取用户(学生)cookie，cookie存储是ID
        string studentId = TakeCookie.GetCookie("userId");
        //数据库帮助类
        ModelHelpers mHelp = new ModelHelpers();

        #region 查询学生所选的所有课程+CourseIndex
        /// <summary>
        /// 进入课程页面 --首页
        /// </summary>
        /// <returns></returns>
        public ActionResult CourseIndex()
        {
            List<Course> listCourse = db.Course.SqlQuery("select * from Courses cr join Stu_Course sc on cr.Id=sc.CourseId and sc.StudentId=@Id", new SqlParameter("@Id", studentId)).ToList();
            ViewBag.Course = listCourse;
            return View();
        }
        #endregion

        #region 学生查看 所有没有选的课程+CoursesAllExcSelected
        /// <summary>
        /// 显示所有的课程，但是除了选中的课程
        /// </summary>
        /// <returns></returns>
        public ActionResult CoursesAllExcSelected()
        {
            List<Course> listCouse = db.Course.SqlQuery("select * from Courses cr where cr.Id not in (select CourseId  from Stu_Course where StudentId=@Id ) ", new SqlParameter("@Id", studentId)).ToList();
            ViewBag.Course = listCouse;
            return View();
        }
        #endregion

        /// <summary>
        /// 进入课程的详细的页面，
        /// 查看课程的详细的内容
        /// </summary>
        /// <returns></returns>
        public ActionResult StudentCoursesDetail()
        {
            return View();
        }

        #region 学生选课操作+StudentCoursesSelect
        /// <summary>
        /// 选中 点击的课程，会显示已完成选中
        /// </summary>
        /// <returns></returns>
        public ActionResult StudentCoursesSelect(int id)
        {
            Stu_Course sc = new Stu_Course { CourseId = id, StudentId = new Guid(studentId) };
            mHelp.Add<Stu_Course>(sc);
            Course course = db.Course.Where(c => c.Id == id).FirstOrDefault();
            course.RealAttend++;
            db.SaveChanges();
            return RedirectToAction("CourseIndex");
        }
        #endregion

        #region 学生注册+Rgist
        /// <summary>
        /// 注册页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Rgist(StudentInfo studentInfo)
        {
            mHelp.Add<StudentInfo>(studentInfo);
            return View();
        }
        #endregion

        #region 学生查看和教师往来的私信+QusetionCenter
        /// <summary>
        /// 答疑中心
        /// </summary>
        /// <returns></returns>
        public ActionResult QusetionCenter()
        {
            List<Question> listAllQue = db.Question.Where(q => q.ToId == new Guid(studentId) || q.FromId == new Guid(studentId)).ToList();
            //查看自己收到的私信
            List<Question> ToMelistQue = listAllQue.Where(l => l.ToId == new Guid(studentId)).ToList();
            ViewBag.ToMelistQue = ToMelistQue;
            //查看自己发送的私信
            List<Question> FromMelistQue = listAllQue.Where(l => l.FromId == new Guid(studentId)).ToList();
            ViewBag.FromMelistQue = FromMelistQue;
            //学生发送私信的下拉框，选择发送课程老师
            var Course = db.Course.SqlQuery("select * from Courses cr join Stu_Course sc on cr.Id=sc.CourseId and sc.StudentId=@Id", new SqlParameter("@Id", studentId)).ToList();
            ViewBag.Course = Course;
            return View();
        }

        #endregion

        #region 学生新建私信+AddQusetion
        [ValidateInput(false)]
        public ActionResult AddQusetion(FormCollection formData)
        {
            //根据ID 获取学生的信息，因为后面有的信息要添加到私信中
            StudentInfo sInfo = db.StudentInfo.Where(s => s.Id == new Guid(studentId)).FirstOrDefault();
            Question que = new Question();
            string[] ToIdandToAccount = formData["teacher"].Split('&');

            que.ToId = new Guid(ToIdandToAccount[0]);
            que.ToAcconut = ToIdandToAccount[1];
            que.FromAccount = sInfo.UserName;
            que.FromId = new Guid(studentId);
            que.Flag = "未读";
            que.CreateTime = DateTime.Now.ToShortDateString();
            que.Content = formData["Content"];
            que.Title = formData["Title"];

            mHelp.Add<Question>(que);
            return RedirectToAction("QusetionCenter");
        }
        #endregion

        #region 学生读取私信的Ajax 调用
        //学生读取私信。
        public string QuestionDetail(int id)
        {
            Question que = new Question() { Id = id, Flag = "已读" };
            DbEntityEntry entry = db.Entry<Question>(que);
            entry.State = System.Data.EntityState.Unchanged;
            entry.Property("Flag").IsModified = true;
            db.SaveChanges();
            return "OK";
        }
        #endregion

        #region 删除私信+DelectQusetion
        public ActionResult DelectQusetion(int id)
        {
            Question que = new Question() { Id = id };
            mHelp.Del<Question>(que);
            return RedirectToAction("QusetionCenter");
        }
        #endregion

        #region 学生个人中心首页+StudentCenter
        /// <summary>
        /// 进入学生的个人中心
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult StudentCenter()
        {
            StudentInfo sInfo = db.StudentInfo.Where(s => s.Id == new Guid(studentId)).FirstOrDefault();
            return View(sInfo);
        }

        #endregion

        #region 学生个人中心首页+StudentCenter,Post方法 获取页面传递的值 操作数据库
        [HttpPost]
        public ActionResult StudentCenter(StudentInfo studentInfo)
        {

            try
            {

                studentInfo.Id = new Guid(studentId);
                studentInfo.Pwd = studentInfo.Pwd.Trim();
                if (string.IsNullOrEmpty(studentInfo.Pwd) || studentInfo.Pwd == "不修改就不需要输入")
                {
                    mHelp.Modify<StudentInfo>(studentInfo, new string[] { "Id", "Account", "UserName", "Sex" });
                    TempData["res"] = "修改成功";
                    return RedirectToAction("StudentCenter");
                }
                mHelp.Modify<StudentInfo>(studentInfo, new string[] { "Id", "Account", "UserName", "Pwd", "Sex" });
                TempData["res"] = "修改成功";
            }
            catch (Exception)
            {
                TempData["res"] = "<font color='red'>修改失败，请点解cancel或刷新 后 重新输入！<font/>";
            }

            return RedirectToAction("StudentCenter");
        }
        #endregion

        #region 学生进入课程+StudentEnterCourse
        /// <summary>
        /// 显示课程的各个模块，列表显示
        /// 传递的参数是课程的id
        /// </summary>
        /// <param name="CourseId"></param>
        /// <returns></returns>
        public ActionResult StudentEnterCourse(string[] id)
        {

            List<Module> listModule = null;
            if (id != null)
            {
                int courseId = Convert.ToInt32(id[0]); //从用户请求中取出课程的id
                listModule = db.Module.Where(m => m.CourseId == courseId).ToList();
                ViewBag.PageCourseId = id[0];//表示为新建或者编辑的标识符

            }
            if (id.Length == 2)
            {
                ViewBag.PageWorkId = id[1];
            }

            //完成课程的信息显示
            if (listModule != null)
            {
                foreach (var lm in listModule)
                {
                    if (lm.ModuleTag == 1)
                        ViewBag.lm1 = lm;
                    if (lm.ModuleTag == 2)
                        ViewBag.lm2 = lm;
                    if (lm.ModuleTag == 3)
                        ViewBag.lm3 = lm;
                    if (lm.ModuleTag == 4)
                        ViewBag.lm4 = lm;
                    if (lm.ModuleTag == 5)
                        ViewBag.lm5 = lm;
                }
            }





            return View();
        } 
        #endregion

        #region 学生进入某个课程后 显示的具体内容+ModuleView
        /// <summary>
        /// 显示各个模块的内容，传递的参数是模块的id和标志显示不同的页面，
        /// 5个页面
        /// </summary>
        /// <param name="ModuleId"></param>
        /// <param name="ModuleTag"></param>
        /// <returns></returns>
        public ActionResult ModuleView()
        {
            string CId = Request.QueryString["CId"]; ///属于哪一门课程
            if (CId != null && CId != "")
            {
                ViewBag.CId = CId; //保存
            }
            int CourseId = Convert.ToInt32(CId);



            string id = Request.QueryString["id"]; ///查看的是模块的id
            if (id != null)
            {
                int ModuleId = Convert.ToInt32(id);//获得模块的id
                Module module = db.Module.Where(m => m.Id == ModuleId).FirstOrDefault();
                ViewBag.Pagemodule = module;

            }
            string moduleTag = Request.QueryString["flag"];
            int tag = Convert.ToInt32(moduleTag);

            StudentWork work = db.StudentWork.Where(w => w.CourseId == CourseId && w.ModuleTag == tag).FirstOrDefault();
            if (work != null)
            {
                ViewBag.PageWork = work;
            }
            if (moduleTag.Equals("1"))
            {
                return View("ModuleView1");
            }
            else
                if (moduleTag.Equals("2"))
                {
                    return View("ModuleView2");
                }
                else
                    if (moduleTag.Equals("3"))
                    {
                        return View("ModuleView3");
                    }
                    else
                        if (moduleTag.Equals("4"))
                        {
                            return View("ModuleView4");
                        }
                        else
                        {
                            return View("ModuleView5");
                        }
        } 
        #endregion


        #region 学生提交学习work+FinishModule
        /// <summary>
        /// 完成学习，进入到不同的页面，完成学习
        /// 
        /// </summary>
        /// <returns></returns>
        [ValidateInput(false)]
        public ActionResult FinishModule(FormCollection form)
        {
            int moduleTag = Convert.ToInt32(Request.QueryString["moduleTag"]);
            string CId = form["CourseId"].Trim();

            HttpPostedFileBase hpfb = Request.Files["studentPath"];
            string filePath = "";
            if (hpfb.ContentLength > 0)
            {
                Common.UpLoad upLoad = new Common.UpLoad();
                filePath = upLoad.StudentSaveFile(hpfb);
            }

            int CourseId = 0;
            if (CId != null && CId != "")
            {
                CourseId = Convert.ToInt32(CId);
            }
            StudentWork work = null;
            StudentInfo sInfo = db.StudentInfo.Where(s => s.Id == new Guid(studentId)).FirstOrDefault();
            work = new StudentWork()
            {
                CourseId = Convert.ToInt32(form["CourseId"]),
                ModuleTag = Convert.ToInt32(Request.QueryString["moduleTag"]),
                WorkContent = form["WorkContent"],
                WorkTime = DateTime.Now.ToString(),
                StudentId = new Guid(studentId),
                StudentAccount = sInfo.Account,
                WorkFilePath = filePath

            };
            db.StudentWork.Add(work);
            db.SaveChanges();

            return RedirectToAction("StudentEnterCourse", new { id = CourseId });
        } 
        #endregion


    }
}
