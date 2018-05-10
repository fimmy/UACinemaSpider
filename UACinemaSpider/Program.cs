using Common.Logging;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Mail;

namespace UACinemaSpider
{
    class Program
    {
        
        static void Main(string[] args)
        {
            SchedulerSart().Wait();
            Console.ReadLine();
            Console.ReadLine();
            Console.ReadLine();
            Console.ReadLine();
        }
        public static async System.Threading.Tasks.Task SchedulerSart()
        {

            StdSchedulerFactory factory = new StdSchedulerFactory();

            // get a scheduler
            IScheduler sched = await factory.GetScheduler();
            await sched.Start();

            // define the job and tie it to our HelloJob class
            IJobDetail job = JobBuilder.Create<SpiderJob>()
                .WithIdentity("myJob", "group1")
                .Build();

            // Trigger the job to run now, and then every 40 seconds
            ITrigger trigger = TriggerBuilder.Create()
              .WithIdentity("myTrigger", "group1")
              .UsingJobData("times",3)
              .StartNow()
              .WithSimpleSchedule(x => x
                  .WithIntervalInSeconds(300)
                  .RepeatForever())
              .Build();

            await sched.ScheduleJob(job, trigger);
        }
    }
    public class SpiderJob : IJob
    {
        private static void Send()
        {
            try
            {

                MailMessage myMail = new MailMessage();

                myMail.From = new MailAddress("");
                myMail.To.Add(new MailAddress(""));

                myMail.Subject = "购票提醒";
                myMail.SubjectEncoding = Encoding.UTF8;

                myMail.Body = "快去购票啦";
                myMail.BodyEncoding = Encoding.UTF8;
                myMail.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.qq.com";
                smtp.Port = 587;
                smtp.Credentials = new NetworkCredential("", "");
                smtp.EnableSsl = true;
                smtp.Send(myMail);

            }
            catch (Exception ex)
            {

                throw;
            }

        }

        private ILog _log;
        public SpiderJob()
        {
            _log = LogManager.GetLogger<SpiderJob>();
        }
        public System.Threading.Tasks.Task Execute(IJobExecutionContext context)
        {
            var times = context.Trigger.JobDataMap.GetInt("times");
            return System.Threading.Tasks.Task.Run(async () =>
            {

                var uri = "http://h5web.yuekeyun.com/route?a=ykse.schedule.getSchedules";
                var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.None, UseCookies = false };

                using (var httpclient = new HttpClient(handler))
                {
                    httpclient.BaseAddress = new Uri(uri);
                    var content = new FormUrlEncodedContent(new Dictionary<string, string>()
           {
               {"api", "ykse.schedule.getSchedules"},
               {"data", "{\"cinemaLinkId\":\"2649\"}"}
           });
                    httpclient.DefaultRequestHeaders.Accept.Clear();
                    httpclient.DefaultRequestHeaders.Add("Accept", "*/*");
                    httpclient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip,deflate");
                    httpclient.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.9");
                    httpclient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                    httpclient.DefaultRequestHeaders.Add("Cookie", "acw_tc=AQAAAIZkEBwGxAoAPnAUdNCHP6QYLJ8s;TKT_Appid=EEGUA_H5_PROD_2649_MPUSUB;EEGUA_H5_PROD_2649_MPUSUBTOcookie_cinemaLinkId=2649;EEGUA_H5_PROD_2649_MPUSUBTOcookie_location=440600;umid=15258565004745733257b-c11c-47d3-9cd7-7c3ead73c368;cna=t9hJEwYJSz8CAXQUcXSBN2N1;Hm_lvt_a183e0685503ac4e19b3ab73d12dea95=1525856504;Hm_lpvt_a183e0685503ac4e19b3ab73d12dea95=1525914115;isg=BGVlULbFU5dOv7dKbwLRx7zNdCFfChkq8c8GomdKYRyrfoXwL_IpBPMfDOII_jHs");
                    httpclient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.139 Safari/537.36");
                    httpclient.DefaultRequestHeaders.Add("Referer", "http://h5web.yuekeyun.com/app/EEGUA_H5_PROD_2649_MPUSUB/version408/cinema/film?wapid=EEGUA_H5_PROD_2649_MPUSUB&data=%7B%22cinemaLinkId%22%3A%222649%22%2C%22cinemaName%22%3A%22%E8%8B%B1%E7%9A%87UA%E4%BD%9B%E5%B1%B1%E5%B2%AD%E5%8D%97%E7%AB%99%E7%94%B5%E5%BD%B1%E5%9F%8E%22%2C%22filmId%22%3A%22051100922018%22%2C%22filmName%22%3A%22%E5%A4%8D%E4%BB%87%E8%80%85%E8%81%94%E7%9B%9F3%EF%BC%9A%E6%97%A0%E9%99%90%E6%88%98%E4%BA%89%22%2C%22showStatus%22%3A%22SOON_SHOW_TICKET%22%2C%22hot%22%3Atrue%7D");
                    var response = await httpclient.PostAsync(uri, content);

                    string responseString = await response.Content.ReadAsStringAsync();
                    dynamic obj = JObject.Parse(responseString);
                    var data = obj.data;
                    var bizValue = data.bizValue;
                    var films = bizValue.films;
                    List<Film> filmsModel = JsonConvert.DeserializeObject<List<Film>>(JsonConvert.SerializeObject(films));
                    //var film = films.Where(f => (string)f.filmId == "051100922018").ToList();
                    var film = filmsModel.Where(f => f.filmId == "051100922018").FirstOrDefault();
                    if (film is Film)
                    {
                        var date = film.dates.Where(d => d.date == "1526070600000").FirstOrDefault();
                        if (date is FilmDate)
                        {
                            if (date.schedules.Any(l => l.hallName.Contains("VIP厅")))
                                //if (date.schedules.Any(l => l.hallName.Contains("一号厅")))
                            {
                                if (times==0)
                                {
                                    context.Scheduler.DeleteJob(context.JobDetail.Key).Wait();
                                    _log.Info($"停止");
                                }
                                else
                                {
                                    Send();
                                    _log.Info($"有了-次数{times}");
                                    times--;
                                    var trigger = context.Trigger;
                                    trigger.JobDataMap.Put("times", times);
                                    context.Scheduler.RescheduleJob(context.Trigger.Key, trigger).Wait();
                                }
                            }
                            else
                            {
                                _log.Info($"还没有");
                            }
                        }
                    }
                }
            });
        }
    }
    public class Film
    {
        public string filmId { get; set; }
        public string filmLang { get; set; }
        public string rating { get; set; }
        public List<FilmDate> dates { get; set; }
    }
    public class FilmDate
    {
        public string date { get; set; }
        public string dateTag { get; set; }
        public List<Schedules> schedules { get; set; }
    }
    public class Schedules
    {
        public string hallName { get; set; }
    }
}
