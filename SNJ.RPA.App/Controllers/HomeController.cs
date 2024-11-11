using Microsoft.AspNetCore.Mvc;
using SNJ.RPA.App.Models;
using System.Diagnostics;
using System.Data;
using NETCore.MailKit.Core;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace SNJ.RPA.App.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public JsonResult CallServices(string data_request, string email)
        {
            //try
            //{
            int _timeOut = 80; int _ct = 0;
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("--lang=en");
            //chromeOptions.AddArgument("--headless");
            //chromeOptions.AddArgument("--window-size=1920,1080");
            //if (!Debugger.IsAttached)
            //{
            //    chromeOptions.AddArgument("--headless");
            //    chromeOptions.AddArgument("--window-size=1920,1080");
            //}

            using (IWebDriver driver = new ChromeDriver(chromeOptions))
            {
                driver.Navigate().GoToUrl("https://pertento.fda.moph.go.th/FDA_SEARCH_CENTER/PRODUCT/FRM_SEARCH_CMT.aspx");
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(500);

                var _serach = driver.FindElement(By.Id("ContentPlaceHolder1_txt_trade"));
                WebDriverWait waitForElement = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                Thread.Sleep(3000);

                _serach.SendKeys(data_request);
                _serach.SendKeys(Keys.Enter);

                Thread.Sleep(300);

                String _num = "";

            CHECKSPAN:
                _num = driver.FindElement(By.Id("ContentPlaceHolder1_lb_num")).Text.Trim();
                if (_num == "0")
                {
                    _ct++; Thread.Sleep(100);
                    if (_ct < _timeOut)
                        goto CHECKSPAN;
                    else
                        return Json(new { status = "error", mesaage = "Data Not Found!" });
                }
                Thread.Sleep(500);

                List<TableData> fees = new List<TableData>();
                var rows = driver.FindElements(By.CssSelector("#ContentPlaceHolder1_RadGrid1_ctl00 > tbody > tr"));
                var _data = new List<TableData>();
                foreach (var row in rows)
                {
                    List<IWebElement> lstTdElem = new List<IWebElement>(row.FindElements(By.TagName("td")));
                    if (lstTdElem.Count > 0)
                    {
                        // IList<IWebElement> links = lstTdElem[19].FindElements(By.TagName("a"));
                        // links.First().Click();
                        _data.Add(new TableData()
                        {
                            doc_no = lstTdElem[2].Text.Trim(),
                            company = lstTdElem[3].Text.Trim(),
                            trade = lstTdElem[5].Text.Trim(),
                            product = lstTdElem[6].Text.Trim(),
                            status = lstTdElem[17].Text.Trim()
                        });
                    }
                }

                //more detail
                foreach (var item in _data)
                {
                    driver.Navigate().GoToUrl("https://pertento.fda.moph.go.th/FDA_SEARCH_CENTER/PRODUCT/export_cmt_detail.aspx?regnos=" + item.doc_no.Replace("-",""));
                    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(200); Thread.Sleep(1000);

                    item.date_register = driver.FindElement(By.Id("ContentPlaceHolder1_lb_appdate")).Text.Trim();
                    item.date_expire = driver.FindElement(By.Id("ContentPlaceHolder1_lb_expdate")).Text.Trim();
                    item.address = driver.FindElement(By.Id("ContentPlaceHolder1_lb_locat_pop")).Text.Trim();

                }


                // //Email
                string[] _mailTo = email.Split('\u002C');
                string _body = "As Your Request Data:<br>";
                //Column
                _body += $"<table style='border-collapse: collapse;width: 100%;'><thead><tr style='background-color: #E35CFF;'>" +
                    $"<th style='border: 1px solid #ddd;padding: 8px;'>Doc No</th>" +
                    $"<th style='border: 1px solid #ddd;padding: 8px;'>Company</th>" +
                    $"<th style='border: 1px solid #ddd;padding: 8px;'>Trade Name</th>" +
                    $"<th style='border: 1px solid #ddd;padding: 8px;'>Product</th>" +
                     $"<th style='border: 1px solid #ddd;padding: 8px;'>Date Registerd</th>" +
                      $"<th style='border: 1px solid #ddd;padding: 8px;'>Date Expired</th>" +
                       $"<th style='border: 1px solid #ddd;padding: 8px;'>Address</th>" +
                    $"<th style='border: 1px solid #ddd;padding: 8px;'>Status</th></tr></thead>";
                foreach (var _d in _data)
                {
                    _body += $"<tr>" +
                                            $"<td style='border: 1px solid #ddd;padding: 8px;'>{_d.doc_no}</td>" +
                                            $"<td style='border: 1px solid #ddd;padding: 8px;'>{_d.company}</td>" +
                                              $"<td style='border: 1px solid #ddd;padding: 8px;'>{_d.trade}</td>" +
                                                $"<td style='border: 1px solid #ddd;padding: 8px;'>{_d.product}</td>" +
                                                  $"<td style='border: 1px solid #ddd;padding: 8px;'>{_d.date_register}</td>" +
                                                     $"<td style='border: 1px solid #ddd;padding: 8px;'>{_d.date_expire}</td>" +
                                                        $"<td style='border: 1px solid #ddd;padding: 8px;'>{_d.address}</td>" +
                                                           $"<td style='border: 1px solid #ddd;padding: 8px;'>{_d.status}</td>" +
                                             $"</tr>";
                }
                var message = new Message(_mailTo, "SNJ-RPA", _body);
                IEmailSender _emailSender = new EmailSender();
                _emailSender.SendEmailAsync(message, "SNJ-RPA");
            }
            return Json(new { status = "ok", mesaage = "" });
            //}
            //catch (Exception ex)
            //{
            //    return Json(new { status = "error", mesaage = ex.Message + ":::XXX::" + ex.InnerException.Message } );
            //}

        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        static DataTable ConvertListToDataTable(List<TableData> data)
        {
            DataTable dataTable = new DataTable();

            // Add columns
            dataTable.Columns.Add("เลขจดแจ้ง", typeof(string));
            dataTable.Columns.Add("ชื่อผู้ประกอบการ", typeof(string));
            dataTable.Columns.Add("ชื่อการค้า", typeof(string));
            dataTable.Columns.Add("ชื่อผลิตภัณฑ์", typeof(string));
            dataTable.Columns.Add("สถานะ", typeof(string));

            // Add rows
            foreach (var item in data)
            {
                DataRow row = dataTable.NewRow();
                row["เลขจดแจ้ง"] = item.doc_no;
                row["ชื่อผู้ประกอบการ"] = item.company;
                row["ชื่อการค้า"] = item.trade;
                row["ชื่อผลิตภัณฑ์"] = item.product;
                row["สถานะ"] = item.status;
                dataTable.Rows.Add(row);
            }

            return dataTable;
        }
    }

    public class TableData
    {
        public string doc_no { get; set; }
        public string company { get; set; }
        public string trade { get; set; }
        public string product { get; set; }
        public string status { get; set; }
        public string? date_register { get; set; }
        public string? date_expire { get; set; }
        public string? address { get; set; }
    }

}
