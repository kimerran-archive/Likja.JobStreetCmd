using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CsQuery;
using Flurl.Http;
using TableParser;

namespace Likja.JobStreetCmd
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WindowWidth = 180;
            Console.WindowHeight = 32;

            Task t = TrySearch(args);
            t.Wait();
        }

        static async Task TrySearch(string[] args)
        {
            var query = string.Join("+", args);

            // var url = @"http://job-search.jobstreet.com.ph/philippines/job-opening.php?key=" + query + "&location=61100&specialization=191&position=&ss=1&by=search&src=11";
            var url = @"http://192.225.223.51/philippines/job-opening.php?key=" + query + "&distil_RID=" + Guid.NewGuid();
            url += "&rnd=" + Guid.NewGuid();

            Console.WriteLine("Querying URL:{0}", url);
            var client = url
                .WithHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8")
                //.WithHeader("User-Agent",
                //    "Mozilla/5.0 (iPad; CPU OS 7_0 like Mac OS X) AppleWebKit/537.51.1 (KHTML, like Gecko) Version/7.0 Mobile/11A465 Safari/9537.53")
                .WithHeader("Accept-Language", "en-US,en;q=0.8")
                .WithHeader("Host", "job-search.jobstreet.com.ph")
                .WithHeader("Connection", "keep-alive");

            var results = await client.GetStringAsync();

            var dom = CQ.Create(results);

            var resultsRow = dom.Select(".rRow");

            if (!resultsRow.Any())
            {
                Console.WriteLine(results);
                Console.WriteLine("Request timed-out. Please try again later.");
                Console.WriteLine(("Url requested is : " + url));
            }
            else
            {
                int searchIndex = 1;
                var resultsTable = new List<JobDetail>();
                foreach (var row in resultsRow)
                {
                    var title = CQ.Create(row.InnerHTML).Select(".rRowTitle a")[0].InnerHTML;
                    var company = CQ.Create(row.InnerHTML).Select(".rRowCompanyClick")[0].InnerHTML;
                    var date = CQ.Create(row.InnerHTML).Select(".rRowDate")[0].InnerHTML.Split('<')[0];
                    var jobDetail = new JobDetail
                    {
                        ReferenceId = searchIndex,
                        Title = title,
                        Company = company,
                        Date = date
                    };
                    resultsTable.Add(jobDetail);
                    searchIndex++;
                }


                var outTable = resultsTable.ToStringTable(
                    x => x.ReferenceId,
                    x => x.Title,
                    x => x.Company,
                    x => x.Date
                );

                Console.WriteLine(outTable);
            }
        }
    }
}