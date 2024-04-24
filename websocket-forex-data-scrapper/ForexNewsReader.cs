using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace websocket_forex_data_scrapper
{
    public class ForexNewsReader
    {
        private PythonRunner _pythonRunner = new PythonRunner();
        public ForexNewsReader()
        {

        }
        public List<ForexNews> ReadNews(string pair = "EURUSD", bool latestNews = false)
        {
            string filePath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\..\..\..\ffc_news_events.csv"; ;

            if (latestNews)
            {
                var currentDate = DateTime.Now.ToString("yyyyMMdd");
                var fileName = "ffc_news_events_current_date";
                filePath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @$"\..\..\..\{fileName}.csv";

                _pythonRunner.RunPythonScript($"{fileName} {currentDate} {currentDate}");
            }
            var memoryListForNews = new List<ForexNews>();

            using (StreamReader sr = new StreamReader(filePath))
            {
                string line;

                // Read and display lines from the file until the end of the file is reached.
                while ((line = sr.ReadLine()) != null)
                {
                    Console.WriteLine(line);

                    var eventNews = new ForexNews();
                    var data = line.Split(',');
                    data = TrimData(data);
                    //triming data

                    var eventDateStr = data[1];
                    if(eventDateStr != "" && eventDateStr != null)
                    {
                        DateTime eventDate = DateTime.ParseExact(eventDateStr, "yyyy.MM.dd H:mm", System.Globalization.CultureInfo.InvariantCulture);

                        eventNews.EventTime = eventDate;
                    }

                    eventNews.Currency = data[3];
                    eventNews.Impact = data[4];
                    eventNews.News = data[5];

                    //only consider high impact news
                    if (eventNews.Impact == "red" && pair.Contains(eventNews.Currency))
                    {
                        memoryListForNews.Add(eventNews);
                    }

                }
            }

            var hashSet = new HashSet<ForexNews>(memoryListForNews, new NewsComparer());

            return hashSet.ToList();
        }

        private string[] TrimData(string[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = data[i].Trim();
            }

            return data;
        }


    }
}
