using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CourseCenter.Models;

namespace CourseCenter.Controllers
{
    public class ModuleManageController : Controller
    {

        CourseCenter.Models.DBEntities db = new Models.DBEntities();
        Guid teacherId = new Guid(Common.TakeCookie.GetCookie("userid"));

        #region 显示莫个课程的 每一个模块+ModuleView
        /// <summary>
        /// 
        /// 代码要求为
        /// 通过Flag的不同，获取到不同的页面
        /// 同时获取到Module，显示在不同页面中，修改的时候，显示内容
        /// </summary>
        /// <returns></returns>
        public ActionResult ModuleView()
        {
            string CId = Request.QueryString["CId"];
            if (CId != null && CId != "")
            {
                ViewBag.CId = CId;
                int CourseId = Convert.ToInt32(CId);
                Course course = db.Course.Where(c => c.Id == CourseId).FirstOrDefault();
                ViewBag.Pagecourse = course;
            }
            string id = Request.QueryString["id"];
            if (id != null)
            {
                int ModuleId = Convert.ToInt32(id);//获得模块的id
                Module module = db.Module.Where(m => m.Id == ModuleId).FirstOrDefault();
                ViewBag.Pagemodule = module;
                ViewBag.ModuleId = id;
            }
            string moduleTag = Request.QueryString["flag"];
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

        #region 教师添加课程模块内容+AddModule
        /// <summary>
        /// 增加模块的方法的复用，显示成功以后 加一个标识符显示为OK
        /// 这里面有错误，
        /// 
        /// </summary>
        /// <returns></returns>
        [ValidateInput(false)]
        public ActionResult AddModule(FormCollection form)
        {  ////// 新增的提示
            ///应该把课程简介的方法和模块添加的方法，分离


            int moduleTag = Convert.ToInt32(Request.QueryString["moduleTag"]);


            HttpPostedFileBase hpfb = Request.Files["teacherUpLoad"];
            string filePath = "";
            if (hpfb.ContentLength > 0)
            {
                Common.UpLoad upLoad = new Common.UpLoad();
                filePath = upLoad.TeacherSaveFile(hpfb);
            }
            string CId = form["CourseId"].Trim();
            string ModuleId = form["ModuleId"].Trim(); ///完成修改

            string CourseAttend = string.IsNullOrEmpty(form["CourseAttend"]) ? "0" : form["CourseAttend"];


            int CourseId = 0;
            if (CId != null && CId != "")
            {
                CourseId = Convert.ToInt32(CId);
            }

            Course course = null;
            try
            {
                if (moduleTag == 1)
                {
                    course = db.Course.Where(c => c.Id == CourseId).FirstOrDefault();
                    if (course == null)
                    {
                        course = new Course()
                        {
                            BeginTime = form["BeginTime"],
                            EndTime = form["EndTime"],
                            TeacherId = teacherId,
                            CourseName = form["CourseName"],
                            CourseStatus = 1,
                            CourseAttend = Convert.ToInt32(CourseAttend),
                            KeyValue = form["KeyValue"],

                        };
                        db.Course.Add(course);

                    }
                    else
                    {
                        course.CourseAttend = Convert.ToInt32(CourseAttend);
                        course.KeyValue = form["KeyValue"];
                        course.CourseName = form["CourseName"];
                        course.BeginTime = form["BeginTime"];
                        course.EndTime = form["EndTime"];

                    }
                    db.SaveChanges();
                }
                else
                {
                    course = db.Course.Where(c => c.Id == CourseId).FirstOrDefault();
                }



                Module module = null;

                if (ModuleId != null && ModuleId != "")
                {
                    int id = Convert.ToInt32(ModuleId);
                    module = db.Module.Where(m => m.Id == id).FirstOrDefault();
                }
                if (module == null)
                {

                    module = new Module()
                  {
                      ModuleTag = moduleTag,
                      DeadlineTime = form["DeadlineTime"],
                      ModuleContent = form["ModuleContent"],
                      ModuleTitle = form["ModuleTitle"],
                      CourseId = course.Id,
                      ModuleFilePath = filePath

                  };
                    db.Module.Add(module);
                }
                else
                {
                    module.DeadlineTime = form["DeadlineTime"];
                    module.ModuleContent = form["ModuleContent"];
                    module.ModuleTitle = form["ModuleTitle"];
                    if (!string.IsNullOrEmpty(filePath))
                    { module.ModuleFilePath = filePath; }

                }

                db.SaveChanges();

            }
            catch (Exception)
            {
                ViewBag.TagMSG = "Error";
                throw;
            }

            ViewBag.TagMSG = "OK";


            return RedirectToAction("CoursesDetail", "CourseManage", new { id = course.Id });
        } 
        #endregion

    }
}
