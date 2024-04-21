namespace websocket_forex_data_scrapper
{
    public class ForexNewsReader
    {
        public ForexNewsReader()
        {

        }
        public List<string> ReadNews()
        {
            string filePath = @".\path\to\your\file.csv";
            var memoryListForNews = new List<string>();

            using (StreamReader sr = new StreamReader(filePath))
            {
                string line;

                // Read and display lines from the file until the end of the file is reached.
                while ((line = sr.ReadLine()) != null)
                {
                    Console.WriteLine(line);
                    memoryListForNews.Add(line);

                }
            }

            return memoryListForNews;
        }
    }
}
