using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Net;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace ScrapyCrawl
{
    class Program
    {
        static void Main(string[] args)
        {
            const string MANGA_URL = "http://mangakatana.com/manga/one-piece.49/";
            const string chapNo = "c336";
            const string nameNo = chapNo;
            const string URL = MANGA_URL + chapNo;
            List<string> listChapterlink = new List<string>();
            List<string> listChaptername = new List<string>();
            //measure time 
            TimeSpan waitForEle = TimeSpan.FromSeconds(10);
            using (var driver = Driver())
            {


                CrawlChapter(driver,waitForEle, URL, nameNo);

                //var tuples = CrawlManga(driver, MANGA_URL);
                ////item1 = link
                //listChapterlink = tuples.Item1;
                ////item2 = name
                //listChaptername = tuples.Item2;

                //for (int i = 0; i < listChapterlink.Count; i++)
                //{
                //    Console.WriteLine("Crawling Chapter: "+listChaptername[i]);
                //    CrawlChapter(driver, listChapterlink[i], listChaptername[i]);
                //    Console.WriteLine("Crawling Chapter: " + listChaptername[i] + " Successfully!");
                //}
            }
            Console.ReadKey();
        }

        //Define web driver 
        static ChromeDriver Driver()
        {
            //Without opening real browser
            ChromeOptions opt = new ChromeOptions();
            opt.AddArgument("--headless");
            opt.SetLoggingPreference(LogType.Browser, LogLevel.All);

            // Initialize the Chrome Driver
            return new ChromeDriver(opt);

        }

        //Wait ajax load
        static void WaitAJAX(ChromeDriver webDriver, TimeSpan waitForEle)
        {
            WebDriverWait wait = new WebDriverWait(webDriver, waitForEle);
            wait.Until(driver => (bool)((IJavaScriptExecutor)driver).ExecuteScript("return jQuery.active == 0"));
        }



        //Return list of chapter link in the manga
        static (List<string>, List<string>) CrawlManga(ChromeDriver driver, string url,TimeSpan ts)
        {
            List<string> listChapterlink = new List<string>();
            List<string> listChaptername = new List<string>();

            //go to home page
            driver.Navigate().GoToUrl(url);

            //Wait for loading
            //Thread.Sleep(1000);
            WaitAJAX(driver,ts);

            //Get link node
            var chapterNode = driver.FindElementsByXPath("//div[@class='chapter']/a");

            //Get the link from chapter node
            foreach (var link in chapterNode)
            {
                string chapterNumberInString = Regex.Match(link.Text, @"\d+").Value;
                bool isChapterNumber = int.TryParse(chapterNumberInString, out int chapterNumber);
                if (chapterNumber > 331 && isChapterNumber)
                {
                    listChapterlink.Add(link.GetAttribute("href"));
                    listChaptername.Add(chapterNumberInString);
                }
            }
            return (listChapterlink, listChaptername);
        }

        //Crawl the manga
        static void CrawlChapter(ChromeDriver driver,TimeSpan ts, string URL, string chapterNo)
        {
            List<string> linkImg = new List<string>();
            //PATH
            const string REAL_PATH = @"D:\Project\ScrapyCrawl\one_piece_crawl\";
            //Create folder store images
            var dir = Directory.CreateDirectory(REAL_PATH + chapterNo);

            //const string URL = "http://mangakatana.com/manga/one-piece.49/c331";
            // Go to the chapter page
            driver.Navigate().GoToUrl(URL);

            //Wait for page to load
            Thread.Sleep(20000);
            //WaitAJAX(driver, ts);

            // Get the image elements
            var parentImgNode = driver.FindElementsByXPath("//*[@class='wrap_img uk-width-1-1']/img");

            //get image src linkImg
            int length = parentImgNode.Count();
            for (int i = 0; i < length; i++)
            {
                linkImg.Add(parentImgNode[i].GetAttribute("src"));
            }

            //Download image
            using (WebClient client = new WebClient())
            {
                for (int i = 0; i < linkImg.Count; i++)
                {
                    Console.WriteLine("Downloading image...");
                    Console.WriteLine("The link is: " + linkImg[i]);
                    client.DownloadFile(new Uri(linkImg[i]), dir.FullName + @"\image" + i + ".jpg");
                    Thread.Sleep(5000);
                }

            }

            //Close the browser
            //driver.Close();
        }
    }
}