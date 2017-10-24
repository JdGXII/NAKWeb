using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AKAWeb_v01.Models;
using AKAWeb_v01.Classes;
using System.Data.SqlClient;
using RazorEngine;
using System.Text;

namespace AKAWeb_v01.Controllers
{
    public class SubPagesController : Controller
    {

        public ActionResult Pages(string id)
        {
            if(id != null)
            {
                 var model = getPage(id);
                 if (model.isLive)
                 {
                     return View(model);
                 }
                 else
                 {
                     return View("Error");
                 }
                

            }
            else
            {
                return View("Error");
            }
            
        }

        private PageModel getPage(string title)
        {
            title = HttpUtility.UrlDecode(title);
            DBConnection testconn = new DBConnection();
            SqlDataReader datareader;
            //this query selects the page to load by title, if it exists i.e. title matches the functions parameters
            string query = "select id, title, subheader_image, content, section, isAlive from Pages where title ='" + title+"'";

            //assign data reader to first query
            datareader = testconn.ReadFromTest(query);
            //check if there are matching pages
            if (datareader.Read())
            {
                //this gets the section of the page and stores it in a string to be used multiple times
                string section = datareader.GetValue(4).ToString();
                //this query gets all page titles that belong to the same section as the requested page
                string query2 = "select title from Pages where section = " + section+" AND isAlive = 1";
                List<string> menu = new List<string>();
                //read from query2
                datareader = testconn.ReadFromTest(query2);
                while (datareader.Read())
                {
                    string menutitle = datareader.GetValue(0).ToString();
                    menu.Add(menutitle);

                }
                //this query gets the title of the section to which the page(s) belong
                string query3 = "select name from Sections where id = " + section;
                //use datareader to read from query3
                datareader = testconn.ReadFromTest(query3);
                datareader.Read();
                string leftMenuTitle = datareader.GetValue(0).ToString();
                //read from first query again
                datareader = testconn.ReadFromTest(query);
                datareader.Read();
                PageModel page = new PageModel();
                page.id = Int32.Parse(datareader.GetValue(0).ToString());
                page.title = datareader.GetValue(1).ToString();
                page.subheaderImage = datareader.GetValue(2).ToString();
                page.pageContent = datareader.GetValue(3).ToString();
                page.leftMenu = menu;
                page.leftMenuTitle = leftMenuTitle;
                page.isLive = (bool)datareader.GetValue(5);

                testconn.CloseDataReader();
                testconn.CloseConnection();
                return page;
            }
            else
            {
                PageModel page = new PageModel();
                              page.id = 1;
                page.title = "Test";
                page.subheaderImage = "";
                page.pageContent = "<div>hola</div>";
                page.leftMenu = null;
                return page;
            }


        }

        public ActionResult Institutions()
        {
            var institutions = getInstitutions("SELECT TOP 100 institution_id, department_id, state_id FROM institution_has_state_has_department");
            ViewData["state_filter"] = statesSelectList(getStates());
            ViewData["institution_filter"] = InstitutionsSelectList(institutions);
            ViewData["department_filter"] = departmentsSelectList(getDepartments());
            var model = institutions; ;
            if (TempData["model"] != null)                
            {
                 model = TempData["model"] as List<InstitutionModel>;
            }
        
            return View(model); 
        }

        private List<InstitutionModel> getInstitutions(string query)
        {
            DBConnection testconn = new DBConnection();
            List<InstitutionModel> institutions = new List<InstitutionModel>();
            
            //query = "SELECT TOP 100 institution_id, department_id, state_id FROM institution_has_state_has_department";
            SqlDataReader dataReader = testconn.ReadFromTest(query);
           if (dataReader.HasRows)
            {
                while (dataReader.Read())
                {
                    int inst_id = Int32.Parse(dataReader.GetValue(0).ToString());
                    int dept_id = Int32.Parse(dataReader.GetValue(1).ToString());
                    int state_id = Int32.Parse(dataReader.GetValue(2).ToString());

                    DepartmentModel department = getDepartment(dept_id);
                    AddressModel state = getState(state_id);
                    InstitutionModel institution = getInstitution(inst_id, department, state);



                    institutions.Add(institution);

                    
                    
                }
            }

            testconn.CloseDataReader();
            testconn.CloseConnection();
            return institutions;
        }

        private InstitutionModel getInstitution(int id, DepartmentModel department, AddressModel state)
        {
            DBConnection testconn = new DBConnection();
            InstitutionModel inst = new InstitutionModel();
            string institution = "SELECT institution, institution_website, associates, bachelors, masters, doctoral FROM Institutions WHERE id = " +id.ToString();
            SqlDataReader dataReader = testconn.ReadFromTest(institution);
            if (dataReader.HasRows)
            {
                while (dataReader.Read())
                {

                    string name = dataReader.GetValue(0).ToString();
                    string website = dataReader.GetValue(1).ToString();
                    bool associates = (bool)dataReader.GetValue(2);
                    bool bachelors = (bool)dataReader.GetValue(3);
                    bool masters = (bool)dataReader.GetValue(4);
                    bool phd = (bool)dataReader.GetValue(5);

                    inst = new InstitutionModel(id, name, website, department, state, bachelors, associates, masters, phd);
                    

                }
            }

            testconn.CloseDataReader();
            testconn.CloseConnection();

            return inst;

        }

        private DepartmentModel getDepartment(int id)
        {

            DBConnection testconn = new DBConnection();
            DepartmentModel dept = new DepartmentModel();
            string institution = "SELECT department, department_website FROM Departments WHERE id = " + id.ToString();
            SqlDataReader dataReader = testconn.ReadFromTest(institution);
            if (dataReader.HasRows)
            {
                while (dataReader.Read())
                {

                    string name = dataReader.GetValue(0).ToString();
                    string website = dataReader.GetValue(1).ToString();

                    dept.id = id;
                    dept.name = name;
                    dept.website = website;




                }
            }

            testconn.CloseDataReader();
            testconn.CloseConnection();

            return dept;

        }

        private AddressModel getState(int id)
        {

            DBConnection testconn = new DBConnection();
            AddressModel state = new AddressModel();
            string institution = "SELECT state_acronym, state_name FROM States WHERE id = " + id.ToString();
            SqlDataReader dataReader = testconn.ReadFromTest(institution);
            if (dataReader.HasRows)
            {
                while (dataReader.Read())
                {

                    string acro = dataReader.GetValue(0).ToString();
                    string name = dataReader.GetValue(1).ToString();
                    state.state = acro;




                }
            }

            testconn.CloseDataReader();
            testconn.CloseConnection();

            return state;

        }

        private List<AddressModel> getStates()
        {

            DBConnection testconn = new DBConnection();
            List<AddressModel> states = new List<AddressModel>();
            string institution = "SELECT state_acronym, state_name, id FROM States";
            SqlDataReader dataReader = testconn.ReadFromTest(institution);
            if (dataReader.HasRows)
            {
                while (dataReader.Read())
                {

                    string acro = dataReader.GetValue(0).ToString();
                    string name = dataReader.GetValue(1).ToString();
                    int id = Int32.Parse(dataReader.GetValue(2).ToString());

                    AddressModel state = new AddressModel();
                    state.user_id = id;
                    state.state = acro;

                    states.Add(state);



                }
            }

            testconn.CloseDataReader();
            testconn.CloseConnection();

            return states;

        }

        private List<DepartmentModel> getDepartments()
        {

            DBConnection testconn = new DBConnection();
            List<DepartmentModel> departments = new List<DepartmentModel>();
            string institution = "SELECT department, department_website, id FROM Departments";
            SqlDataReader dataReader = testconn.ReadFromTest(institution);
            if (dataReader.HasRows)
            {
                while (dataReader.Read())
                {

                    string name = dataReader.GetValue(0).ToString();
                    string website = dataReader.GetValue(1).ToString();
                    int id = Int32.Parse(dataReader.GetValue(2).ToString());

                    DepartmentModel dept = new DepartmentModel(id, name, website);
                    departments.Add(dept);
                }
            }

            testconn.CloseDataReader();
            testconn.CloseConnection();

            return departments;

        }

        private List<SelectListItem> departmentsSelectList(List<DepartmentModel> departments)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            SelectListItem item = new SelectListItem();
            item.Value = "ALL";
            item.Text = "ALL";
            list.Add(item);

            foreach(DepartmentModel department in departments)
            {
                item = new SelectListItem();
                item.Value = department.id.ToString();
                item.Text = department.name;

                list.Add(item);

            }

            return list;
        }

        private List<SelectListItem> statesSelectList(List<AddressModel> addresses)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            SelectListItem item = new SelectListItem();
            item.Value = "ALL";
            item.Text = "ALL";
            list.Add(item);

            foreach (AddressModel address in addresses)
            {
                item = new SelectListItem();
                item.Value = address.user_id.ToString();
                item.Text = address.state;

                list.Add(item);

            }

            return list;
        }

        private List<SelectListItem> InstitutionsSelectList(List<InstitutionModel> institutions)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            SelectListItem item = new SelectListItem();
            item.Value = "ALL";
            item.Text = "ALL";
            list.Add(item);

            foreach (InstitutionModel institution in institutions)
            {
                item = new SelectListItem();
                item.Value = institution.id.ToString();
                item.Text = institution.name;

                list.Add(item);

            }

            return list;
        }

        [HttpPost]
        public ActionResult InstitutionFilter(string state_filter, string institution_filter, string department_filter)
        {
            StringBuilder query = new StringBuilder("SELECT TOP 100 institution_id, department_id, state_id FROM institution_has_state_has_department WHERE ");
            
            if (state_filter == "ALL" && institution_filter == "ALL" && department_filter == "ALL")
            {
                query.Replace(" WHERE ", "");
            }
            else
            {
                if(state_filter != "ALL" && institution_filter == "ALL" && department_filter == "ALL")
                {
                    query.Append("state_id = ");
                    query.Append(state_filter);
                }
                if(state_filter != "ALL" && institution_filter != "ALL" && department_filter == "ALL")
                {
                    query.Append("state_id = ");
                    query.Append(state_filter);
                    query.Append(" AND institution_id = ");
                    query.Append(institution_filter);
                }
                if(state_filter != "ALL" && institution_filter != "ALL" && department_filter != "ALL")
                {
                    query.Append("state_id = ");
                    query.Append(state_filter);
                    query.Append(" AND institution_id = ");
                    query.Append(institution_filter);
                    query.Append(" AND department_id = ");
                    query.Append(department_filter);
                }
                if (state_filter == "ALL" && institution_filter != "ALL" && department_filter == "ALL")
                {
                    query.Append("institution_id = ");
                    query.Append(institution_filter);
                }
                if (state_filter == "ALL" && institution_filter != "ALL" && department_filter != "ALL")
                {
                    query.Append("institution_id = ");
                    query.Append(institution_filter);
                    query.Append(" AND department_id = ");
                    query.Append(department_filter);
                }
                if (state_filter == "ALL" && institution_filter == "ALL" && department_filter != "ALL")
                {
                    query.Append("department_id = ");
                    query.Append(department_filter);
                }


            }

            string final_query = query.ToString();
            var model = getInstitutions(final_query);
            TempData["model"] = model;
            return RedirectToAction("Institutions");
        }




    }
}