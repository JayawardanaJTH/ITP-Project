﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Project.Models;
using System.IO;
using System.Net;
using System.Data.Entity;

namespace Project.Controllers
{
    public class UploadFileController : Controller
    {
        
        // GET: UploadFile
        public ActionResult Index()
        {
            ViewBag.FileMessage = "Select file you want to upload";
            ViewBag.teachersList = new SelectList(GetteachersList(), "teacher_id", "teacher_name");
            return View();
        }

        public List<teacher> GetteachersList()
        {
            dbModels db = new dbModels();
            List<teacher> teachers = db.teachers.ToList();
            return teachers;
        }

        public ActionResult GetSubjectList(int teacher_id)
        {
            dbModels db = new dbModels();
            List<teacher_subject> subID = db.teacher_subject.Where(x => x.teacher_id == teacher_id).ToList();
            
            foreach(var item in subID)
            {
                int subjectId = item.subject_id;
                
                List<subject> subjects = db.subjects.Where(x => x.subject_id == subjectId).ToList();
                ViewBag.subList = new SelectList(subjects, "subject_id", "subject1");
            }
           
            
            return PartialView("DisplaySubjects");
        }

        public ActionResult GetGradeList(int teacher_id)
        {
            dbModels db = new dbModels();
            List<teacher_grade> grades = db.teacher_grade.Where(x => x.teacher_id == teacher_id).ToList();
    
            ViewBag.gradeList = new SelectList(grades, "grade_id", "grade");

            return PartialView("DisplayGrades");
        }

        [HttpPost]
        public ActionResult Index(IEnumerable<HttpPostedFileBase> files,TeacherRel model, String message) 
        {
            dbModels db = new dbModels();
            upload_file log = new upload_file();
            upload_file_teacher log2 = new upload_file_teacher();
            

            int count = 0;
            if (!ModelState.IsValid)
            {
                return View();
            }
            else
            {
                if (files != null)
                {
                    foreach (var file in files)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            var fileName = file.FileName;
                            var path = Path.Combine(Server.MapPath("~/UploadedFiles"), fileName);
                            file.SaveAs(path);

                            log.file_name = fileName;
                            log.file_path = path;
                            log.upload_date = DateTime.Now;

                            int gradeid = model.grade_id;
                            int subjectid = model.subject_id;


                            var grades = db.grades.Where(u => u.grade_id == gradeid)
                                                             .Select(u => new
                                                             {
                                                                 grade = u.grade1
                                                             }).Single();

                            var subjects = db.subjects.Where(u => u.subject_id == subjectid)
                                                            .Select(u => new
                                                            {
                                                                subject = u.subject1
                                                            }).Single();

                            log.grade = grades.grade;
                            log.subject = subjects.subject;

                            db.upload_file.Add(log);
                            db.SaveChanges();

                            int teacherid = model.teacher_id;

                            log2.teacher_id = teacherid;

                            db.upload_file_teacher.Add(log2);
                            db.SaveChanges();

                            count++;
                        }
                    }
                }
                return RedirectToAction("ViewList");
            }

            
        }

        public ActionResult ViewList()
        {
            dbModels db = new dbModels();

            List<upload_file> files = db.upload_file.ToList();

            return View(files);

        }
       

        public FileResult DownloadFile(string fileName)
        {
            var filePath = "~/UploadedFiles/" + fileName;
            return File(filePath, "application/force- download", Path.GetFileName(filePath));
        }

        public List<string> GetFileList()
        {
            var directory = new DirectoryInfo(Server.MapPath("~/UploadedFiles"));
            FileInfo[] fileNames = directory.GetFiles("*.*");

            List<String> files = new List<string>();

            foreach (var item in fileNames)
            {
                files.Add(item.Name);
            }
            return files;
        }

        public ActionResult Delete(int? id)
        {
            if(id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            dbModels db = new dbModels();
            upload_file file = db.upload_file.Find(id);

            if(file == null)
            {
                return HttpNotFound();
            }
            return View(file);
            
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteSucces(int id)
        {
            dbModels db = new dbModels();
            upload_file file = db.upload_file.Find(id);
            db.upload_file.Remove(file);
            db.SaveChanges();

            return RedirectToAction("ViewList");
        }

        public ActionResult Edit(int? id)
        {
            dbModels db = new dbModels();
            upload_file file = db.upload_file.Find(id);

            ViewBag.teachersList = new SelectList(GetteachersList(), "teacher_id", "teacher_name");

            return View(file);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Edit")]
        public ActionResult EditSucces(int id,upload_file model)
        {
            dbModels db = new dbModels();

            upload_file file = db.upload_file.Find(id);

            file.file_name = model.file_name;
            file.grade = model.grade;
            file.subject = model.subject;

            db.Entry(file).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("ViewList");
        }

    }
}