using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using System.IO;
using System.Globalization;
using CsvHelper;
using System.Text;

namespace scraper
{
    class Program
    {
        static ScrapingBrowser _scrapingbrowser = new ScrapingBrowser();
        static void Main(string[] args)
        {
            var url = "https://www.bilietai.lt/lit/renginiai/teatras/";
            int pageAmount = GetPageAmount(url);
            var eventLinks = new List<string>();
            string website = "Bilietai.lt";
            string eventType = "Teatras";
            for(int i = 1; i <= pageAmount; i++)
            {
                eventLinks = GetEventLinks("https://www.bilietai.lt/lit/renginiai/teatras/page:" + i, eventLinks);
            }
            var listEventDetails = GetEvenDetailsAlternative(eventLinks);
            exportEventsToCsv(listEventDetails, website, eventType);

            url = "https://www.bilietai.lt/lit/renginiai/koncertai/";
            pageAmount = GetPageAmount(url);
            eventLinks = new List<string>();
            eventType = "Koncertai";
            for(int i = 1; i <= pageAmount; i++)
            {
                eventLinks = GetEventLinks("https://www.bilietai.lt/lit/renginiai/koncertai/page:" + i, eventLinks);
            }
            listEventDetails = new List<EventDetails>();
            listEventDetails = GetEvenDetailsAlternative(eventLinks);
            exportEventsToCsv(listEventDetails, website, eventType);
        }
        static List<string> GetEventLinks(string url, List<string> eventLinks)
        {
            var html = GetHtml(url);
            var links = html.CssSelect("a.event_short");

            foreach(var link in links)
            {
                eventLinks.Add(link.Attributes["href"].Value);
            }
            return eventLinks;
        }
        static List<EventDetails> GetEvenDetailsBilietaiLt(List<string> urls){
            var listEventDetails = new List<EventDetails>();

            foreach(var url in urls)
            {
                var htmlNode = GetHtml(url);
                var EventDetails = new EventDetails();

                EventDetails.EventImagePath = htmlNode.OwnerDocument.DocumentNode.SelectSingleNode("//img[contains(@class, 'gallery_single')]").Attributes["src"].Value;
                EventDetails.Url = url;
                EventDetails.EventName = htmlNode.OwnerDocument.DocumentNode.SelectSingleNode("//h1").InnerText;
                if(htmlNode.OwnerDocument.DocumentNode.SelectSingleNode("//html/body/div[3]/div/div/div[1]/div/div[1]/div[1]/div[1]/div[1]/div[2]/div[2]/div[1]/div[3]/div[2]/span") == null)
                {
                    EventDetails.Price = "Bilietai neparduodami";
                }
                else
                {
                    EventDetails.Price = htmlNode.OwnerDocument.DocumentNode.SelectSingleNode("//html/body/div[3]/div/div/div[1]/div/div[1]/div[1]/div[1]/div[1]/div[2]/div[2]/div[1]/div[3]/div[2]/span").InnerText;
                    
                }
                if(htmlNode.OwnerDocument.DocumentNode.SelectSingleNode("//html/body/div[3]/div/div/div[1]/div/div[1]/div[1]/div[1]/div[1]/div[2]/div[2]/div[1]/div[2]/div[2]") == null)
                {
                    EventDetails.EventLocation = "Renginys vyksta daugiau negu vienoje vietoje";
                }
                else
                {
                    EventDetails.EventLocation = htmlNode.OwnerDocument.DocumentNode.SelectSingleNode("//html/body/div[3]/div/div/div[1]/div/div[1]/div[1]/div[1]/div[1]/div[2]/div[2]/div[1]/div[2]/div[2]").InnerText;
                }
                EventDetails.EventDate = htmlNode.OwnerDocument.DocumentNode.SelectSingleNode("//div[contains(@class, 'concert_details_spec_content')]").InnerText;

                if(htmlNode.OwnerDocument.DocumentNode.SelectSingleNode("//html/body/div[3]/div/div/div[1]/div/div[1]/div[1]/div[1]/div[1]/div[2]/div[3]/button") == null)
                {
                    EventDetails.TicketLink = "Bilieto nusipirkti negalima";
                }
                else{
                    EventDetails.TicketLink = htmlNode.OwnerDocument.DocumentNode.SelectSingleNode("//html/body/div[3]/div/div/div[1]/div/div[1]/div[1]/div[1]/div[1]/div[2]/div[3]/button").Attributes["data-shopurl"].Value;
                }
                
                listEventDetails.Add(EventDetails);
            }
            return listEventDetails;
        }
        static List<EventDetails> GetEventDetailsKakava(List<string> urls)
        {
            var listEventDetails = new List<EventDetails>();

            foreach(var url in urls)
            {
                var htmlNode = GetHtml(url);
                var EventDetails = new EventDetails();

            }
            return listEventDetails;
        }
        static int GetPageAmount(string url)
        {
            var pages = new List<int>();
            var htmlNode = GetHtml(url);
            int maxPage = int.MinValue;
            int pageNumber = 0;

            var nodes = htmlNode.OwnerDocument.DocumentNode.SelectNodes("//a[@class = 'pager_page']");

            foreach(var node in nodes)
            {
                var page = node.InnerText;
                pageNumber = Int32.Parse(page);
                if(pageNumber > maxPage)
                {
                    maxPage = pageNumber;
                }
            }
            return maxPage;
        }
        static void exportEventsToCsv(List<EventDetails> listEventDetails, string website, string eventType)
        {
            using (var writer = new StreamWriter($@"/Users/p4t0s/Desktop/scraper/CSVs/{website}_{eventType}_{DateTime.Now.ToFileTime()}.csv"))
            using(var csv = new CsvWriter(writer, CultureInfo.InvariantCulture)){
               csv.WriteRecords(listEventDetails);
           }
        }
        static ScrapingBrowser browser = new ScrapingBrowser();
        static HtmlNode GetHtml(string url)
        {
            browser.Encoding = Encoding.UTF8;
            
            WebPage webPage = browser.NavigateToPage(new Uri(url));
            return webPage.Html;
        }
    }
    public class EventDetails
    {
        public string Url {get;set;}
        public string EventName {get;set;}
        public string EventLocation {get;set;}
        public string EventDate {get;set;}
        public string EventImagePath {get;set;}
        public string Price {get;set;}
        public string TicketLink {get;set;}
    }
}