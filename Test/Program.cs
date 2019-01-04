using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace Test
{
   static class Program
    {
        private static void Main()
        {
//            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-es");

            Console.OutputEncoding = Encoding.Unicode;
            var sw = new Stopwatch();

            sw.Start();
            var beginStringData = $@"variables=%7B%22pageID%22%3A%22";
           var endStringData = $@"%22%7D&doc_id=2473596855989497";
           var pageID = "453101831380213";
           var stringData = $@"{beginStringData}{pageID}{endStringData}";
            //    var cubano = "variables=%7B%22pageID%22%3A%221723473481303362%22%7D&doc_id=2473596855989497";
            //   stodola = cubano;
             string url = "https://www.facebook.com/api/graphql/";
             string languageCode = "pl-pl";

          var fullResponse = GetFullResponse(url, stringData, languageCode);
       
          var listOfEvents = GetListOfEvents(fullResponse);
          sw.Stop();
          Console.WriteLine(sw.ElapsedMilliseconds);
            foreach (var events in listOfEvents)
          {
            Console.WriteLine(events.TimeRange);
            Console.WriteLine(events.ShortTimeLabel);
            Console.WriteLine(events.ShortDateLabel);
            Console.WriteLine(events.LocalLocation);
            Console.WriteLine(events.Guests);
            Console.WriteLine(events.Title);
            Console.WriteLine(events.GlobalLocation);
            Console.WriteLine(events.BuyTicketUrl);
          }
        }

        public static string GetFullResponse(string url, string stringData, string languageCode)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);

            var data = Encoding.ASCII.GetBytes(stringData); // or UTF8
            /*            request.Headers.Add("Accept-Language", "es-es;");
                              request.Headers.Add("Accept-Encoding:", "gzip,deflate,br");
                              request.Headers.Add("Cookie",""); to set authorization
                              request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                        request.Headers.Add("Origin", "https://es-es.facebook.com");*/
            request.Method = "POST";
            request.Host = $"{languageCode}.facebook.com";
            request.Accept = "*/*";
            request.Headers.Add("TransferEncoding", "gzip,deflate,br");
            request.UserAgent =
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36";
            request.ContentType = "application/x-www-form-urlencoded"; //place MIME type here
            request.ContentLength = data.Length;
            var newStream = request.GetRequestStream();
            newStream.Write(data, 0, data.Length);
            var resStream = request.GetResponse().GetResponseStream();
            var readStream = new StreamReader(resStream ?? throw new Exception("emptyStrem"), Encoding.GetEncoding("ISO-8859-1")).ReadToEnd();
            newStream.Close();
            return readStream;
        }
        private static List<EventsData> GetListOfEvents(string fullResponse)
        {
   
        
            var encoded = EncodeNonAsciiCharacters(fullResponse);
            var decoded = DecodeEncodedNonAsciiCharacters(encoded);
            var strings = decoded.Split(new[] {"cursor"}, StringSplitOptions.None).ToList();
            strings.RemoveRange(strings.Count - 2, 2);
            var listOfEvents = new List<EventsData>();
            foreach (var club in strings)
            {
                var eventID = GetBetween(club, "\"eventID\": \"", "\""); //needed to join event
                var guestsText = GetBetween(club, "\"text\": \"", "\"");
                var guests = new string(guestsText.Where(char.IsDigit).ToArray());
                var timeRange = GetBetween(club, "start\": \"", "\"");
                var shortTimeLabel = GetBetween(club, "shortTimeLabel\": \"", "\"");
                var shortDateLabel = GetBetween(club, "shortDateLabel\": \"", "\"");
                var localLocation = GetBetween(club,
                    "\"__typename\": \"Page\",\n                        \"contextual_name\": \"", "\"");
                var globalLocation = GetBetween(club, "cityContextualName\": \"", "\"");
                //  var timeZoneLocation = getBetween(club, "timezone\": \"", "\"");
                var buyTicketUrl = GetBetween(club, "event_buy_ticket_url\": \"", "\"");
                var name = GetBetween(club, "preassigned_discount_note\": null,\n                     \"name\": \"", "\"");
                listOfEvents.Add(new EventsData
                {
                    EventId = eventID,
                    Title = name,
                    Guests = int.Parse(guests),
                    TimeRange = timeRange,
                    ShortTimeLabel = shortTimeLabel,
                    ShortDateLabel = shortDateLabel,
                    LocalLocation = localLocation,
                    GlobalLocation = globalLocation,
                    BuyTicketUrl = buyTicketUrl,
                  
                });
            }

            foreach (var singleItem in listOfEvents)
            {
                JsonSerializer serializer = new JsonSerializer();
                using (StreamWriter sw = new StreamWriter(@"C:\Users\JANEK\Desktop\Strona\json.txt"))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, singleItem);
                    // {"ExpiryDate":new Date(1230375600000),"Price":0}
                }
            }

            return listOfEvents;
        }
    
        public static string GetBetween(string strSource, string strStart, string strEnd)
        {
            if (!strSource.Contains(strStart) || !strSource.Contains(strEnd)) return "";
            var start = strSource.IndexOf(strStart, 0, StringComparison.Ordinal) + strStart.Length;
            var end = strSource.IndexOf(strEnd, start, StringComparison.Ordinal);
            return strSource.Substring(start, end - start);

        }

        private static string DecodeEncodedNonAsciiCharacters(string value)
        {
            return Regex.Replace(
                value,
                @"\\u(?<Value>[a-zA-Z0-9]{4})",
                m => ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString());
        }

        private static string EncodeNonAsciiCharacters(string value)
        {
            var sb = new StringBuilder();
            foreach (var c in value)
            {
                if (c > 127)
                {
                    // This character is too big for ASCII
                    var encodedValue = "\\u" + ((int)c).ToString("x4");
                    sb.Append(encodedValue);
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

      
    }
}
