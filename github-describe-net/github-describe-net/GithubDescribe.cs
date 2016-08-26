using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GithubDescribe
{
    class GithubDescribe
    {
        private const string GithubApiUri = "https://api.github.com/";
        private static readonly WebClient Wc = new WebClient();

        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Usage:\ngithubDescribe.exe ProjectOwner Project CommitSHA");
                Environment.Exit(4);
            }

            string user = args[0];
            string project = args[1];
            string commitSha = args[2];

            try
            {
                Console.WriteLine(GetGitDescribeString(user.Trim(), project.Trim(), commitSha.Trim()));
            }
            catch (InvalidDataException e)
            {
                Console.WriteLine(e);
                Environment.Exit(1);
            }
            catch (WebException e)
            {
                Console.WriteLine(e);
                Environment.Exit(2);
            }
            catch (JsonException e)
            {
                Console.WriteLine(e);
                Environment.Exit(3);
            }
        }


        private static string GetGitDescribeString(string user, string project, string commitSha)
        {
            //Get all tags of the project
            var tags = GetResponseArray($"{GithubApiUri}repos/{user}/{project}/git/refs/tags");

            var comparisons = new List<CommitComparisonResult>();

            Parallel.ForEach(tags, tag =>
            {
                if (tag["object"]["type"].ToString() != "tag")
                {
                    //Filter out lightweight tags
                    return;
                }

                //Get tag info
                var tagInfo = GetResponseObject($"{GithubApiUri}repos/{user}/{project}/git/tags/{tag["object"]["sha"]}");

                //Compare the commit of the tag with the current tag
                var compare = GetResponseObject($"{GithubApiUri}repos/{user}/{project}/compare/{tagInfo["object"]["sha"].ToString()}...{commitSha}");
                comparisons.Add(new CommitComparisonResult()
                {
                    Tag = tagInfo["tag"].ToString(),
                    Distance = compare["status"].ToString() == "ahead" ? Convert.ToInt32(compare["ahead_by"].ToString()) : -1
                });
            });

            //Find nearest tag which is behind the current commit 
            CommitComparisonResult latestTag = comparisons.Where(e => e.Distance >= 0).OrderBy(e => e.Distance).FirstOrDefault();

            if (latestTag == null)
            {
                //No suitable tag found for this commit
                throw new InvalidDataException("Commit not describable");
            }

            return $"{latestTag.Tag}-{latestTag.Distance}-g{commitSha.Substring(0, 7)}";
        }

        private class CommitComparisonResult
        {
            public string Tag { get; set; }
            public int Distance { get; set; }
        }

        private static JArray GetResponseArray(string uri)
        {
            //Need to set a user-agent otherwise github denies our request
            Wc.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            var json = Wc.DownloadString(uri);
            return JArray.Parse(json);
        }

        private static JObject GetResponseObject(string uri)
        {
            //Need to set a user-agent otherwise github denies our request
            Wc.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            var json = Wc.DownloadString(uri);
            return JObject.Parse(json);
        }
    }
}
