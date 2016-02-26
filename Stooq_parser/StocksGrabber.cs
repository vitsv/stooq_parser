using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Timers;
using Newtonsoft.Json;

namespace Stooq_parser
{
    class StocksGrabber
    {
        public string DataUrl { get; set; }
        public IStocksParser Parser { get; set; }
        public IStocksPersister Persister { get; set; }

        public StocksGrabber(string url, IStocksParser parser, IStocksPersister persister)
        {
            DataUrl = url;
            Parser = parser;
            Persister = persister;
        }

        public void Update(object sender, ElapsedEventArgs e)
        {
            WebRequest request = WebRequest.Create(DataUrl);
            WebResponse response = request.GetResponse();

            StreamReader reader = new StreamReader(response.GetResponseStream());
            try
            {
                var result = Parser.Parse(reader.ReadToEnd());
                Persister.Persist(result);
            }
            catch (Exception ex)
            {
                //TODO log erros
                throw ex;
            }
            finally
            {
                reader.Close();
                response.Close();
            }
        }
    }

    public class ConsolePersister : IStocksPersister
    {
        public void Persist(Dictionary<string, string> data)
        {
            Console.WriteLine(string.Format("Actual stocks rates. {0}:", DateTime.Now.ToShortTimeString()));

            foreach (var stock in data)
            {
                Console.WriteLine(string.Format("{0} : {1}", stock.Key, stock.Value));
            }
        }
    }

    public class FilePersister : IStocksPersister
    {
        public void Persist(Dictionary<string, string> data)
        {
            Dictionary<string, string> initData = null;
            var changes = false;

            //Read previous data from file
            if (File.Exists("Data.txt"))
            {
                try
                {
                    using (StreamReader sr = new StreamReader("Data.txt"))
                    {
                        String line = sr.ReadToEnd();
                        initData = JsonConvert.DeserializeObject<Dictionary<string, string>>(line);
                        //If there old data then check if something change
                        if (JsonConvert.SerializeObject(data) != line)
                        {
                            changes = true;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("The file could not be read:");
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                changes = true;
            }

            //Save data if smth change
            if (changes)
            {
                try
                {
                    using (StreamWriter sr = new StreamWriter("Data.txt"))
                    {
                        sr.Write(JsonConvert.SerializeObject(data));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("The file could not be read:");
                    Console.WriteLine(e.Message);
                }
            }
        }
    }

    public class DbPersister : IStocksPersister
    {
        public void Persist(Dictionary<string, string> data)
        {
            throw new NotImplementedException();
        }
    }

    public class StooqGpwParser : IStocksParser
    {
        public Dictionary<string, string> Parse(string s)
        {
            var next = true;
            var result = new Dictionary<string, string>();
            while (next)
            {

                Match m = Regex.Match(s, "onclick=pp_m_\\(this\\)>(.*?)</a></td><td id=pp_v>(.*?)</td>", RegexOptions.IgnoreCase);
                next = m.Success;
                if (m.Success)
                {
                    result.Add(m.Groups[1].Value, m.Groups[2].Value);
                    s = new Regex("onclick=pp_m_\\(this\\)>(.*?)</a></td><td id=pp_v>(.*?)</td>").Replace(s,
                        string.Empty, 1);
                }
            }

            return result;
        }
    }

    public interface IStocksPersister
    {
        void Persist(Dictionary<string, string> data);
    }

    public interface IStocksParser
    {
        Dictionary<string, string> Parse(string s);
    }
}
