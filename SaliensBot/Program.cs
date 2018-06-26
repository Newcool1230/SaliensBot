using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using System.Threading;

namespace SaliensBot
{
    class Program
    {
        static string token;

        static void Main(string[] args)
        {
            if (args.Length >= 1)
                token = args[0];
            else
                Environment.Exit(0);

            Console.WriteLine($"登入：{token}");

            while (true)
            {
                int planetId = 0;
                float minProg = 1.0f;
                string planetName = "";

                // 取得全部星球資料
                var Planets = GetPlanets().response;
                while (Planets.planets == null)
                {
                    Planets = GetPlanets().response;
                    Thread.Sleep(1000);
                }

                foreach (var i in Planets.planets)
                {
                    if (i.state.active && !i.state.captured)
                    {
                        if (i.state.capture_progress < minProg)
                        {
                            planetId = i.id;
                            planetName = i.state.name;
                            minProg = i.state.capture_progress;
                        }
                    }
                }

                // 取得玩家訊息
                var PlayerInfo = GetPlayerInfo().response;
                while (PlayerInfo.next_level_score == 0)
                {
                    PlayerInfo = GetPlayerInfo().response;
                    Thread.Sleep(1000);
                }

                if (PlayerInfo.active_zone_game != 0)
                {
                    Console.WriteLine($"正在離開未完成的區域...");
                    LeaveGame(PlayerInfo.active_zone_game);
                }
                if (PlayerInfo.active_planet != planetId)
                {
                    LeaveGame(PlayerInfo.active_planet);
                    Console.WriteLine($"正在進入「{planetName}」星球...");
                    JoinPlanet(planetId);
                }
                else
                    Console.WriteLine($"繼續待在「{planetName}」星球");

                for (int counter = 0; counter < 20; counter++)
                {
                    // 取得目前星球資料
                    var Planet = GetPlanet(planetId).response;
                    while (Planet.planets == null)
                    {
                        Planet = GetPlanet(planetId).response;
                        Thread.Sleep(1000);
                    }

                    int minDiff = 0;
                    int zone_position = 0;
                    foreach (var i in Planet.planets[0].zones)
                    {
                        if (!i.captured && i.difficulty >= minDiff)
                        {
                            if (i.capture_progress > 0.95f)
                                continue;
                            minDiff = i.difficulty;
                            zone_position = i.zone_position;
                            if (minDiff == 3)
                                break;
                        }
                    }
                    Console.WriteLine($"正在進入區域：{zone_position}，難度：{minDiff}");
                    JoinZone(zone_position);
                    for (int i = 128; i >= 0; i--)
                    {
                        Console.Write($"\r請等待 {i} 秒後結算...");
                        Thread.Sleep(1000);
                    }
                    var Score = ReportScore(minDiff).response;
                    Console.WriteLine($"\r經驗值：{Score.new_score}(+{Score.new_score - Score.old_score}) / {Score.next_level_score}，等級：{Score.new_level}\n");
                }
            }
        }
        
        static GetPlayerInfo GetPlayerInfo()
        {
            JavaScriptSerializer Json = new JavaScriptSerializer();
            string QueryUrl = "https://community.steam-api.com/ITerritoryControlMinigameService/GetPlayerInfo/v0001/";
            return Json.Deserialize<GetPlayerInfo>(UrlQuery(QueryUrl, $"access_token={token}"));
        }

        static GetPlanets GetPlanets()
        {
            JavaScriptSerializer Json = new JavaScriptSerializer();
            string QueryUrl = "https://community.steam-api.com/ITerritoryControlMinigameService/GetPlanets/v0001/?active_only=1&language=tchinese";
            return Json.Deserialize<GetPlanets>(UrlQuery(QueryUrl));
        }

        static void JoinPlanet(int id)
        {
            string QueryUrl = "https://community.steam-api.com/ITerritoryControlMinigameService/JoinPlanet/v0001/";
            UrlQuery(QueryUrl, $"id={id}&access_token={token}");
        }

        static GetPlanet GetPlanet(int id)
        {
            JavaScriptSerializer Json = new JavaScriptSerializer();
            string QueryUrl = $"https://community.steam-api.com/ITerritoryControlMinigameService/GetPlanet/v0001/?id={id}&language=tchinese";
            return Json.Deserialize<GetPlanet>(UrlQuery(QueryUrl));
        }

        static void JoinZone(int zone_position)
        {
            string QueryUrl = "https://community.steam-api.com/ITerritoryControlMinigameService/JoinZone/v0001/";
            UrlQuery(QueryUrl, $"zone_position={zone_position}&access_token={token}");
        }

        static ReportScore ReportScore(int difficulty)
        {
            int score = 595;
            if (difficulty == 2)
                score = 1190;
            if (difficulty == 3)
                score = 2380;
            JavaScriptSerializer Json = new JavaScriptSerializer();
            string QueryUrl = "https://community.steam-api.com/ITerritoryControlMinigameService/ReportScore/v0001/";
            return Json.Deserialize<ReportScore>(UrlQuery(QueryUrl, $"access_token={token}&score={score}&language=tchinese"));
        }

        static void LeaveGame(int gameid)
        {
            string QueryUrl = "https://community.steam-api.com/IMiniGameService/LeaveGame/v0001/";
            UrlQuery(QueryUrl, $"access_token={token}&gameid={gameid}");
        }

        static string UrlQuery(string Url, string Params = null)
        {
            try
            {
                WebRequest R = HttpWebRequest.Create(Url);
                R.Timeout = 30000;
                if (Params != null)
                {
                    R.Method = "POST";
                    R.ContentType = "application/x-www-form-urlencoded";
                    using (var sR = new StreamWriter(R.GetRequestStream()))
                    {
                        sR.Write(Params);
                        sR.Flush();
                    }
                }
                string rS;
                WebResponse wR = R.GetResponse();
                using (var sR = new StreamReader(wR.GetResponseStream()))
                    rS = sR.ReadToEnd();
                wR.Close();
                return rS;
            }
            catch (Exception) { return "{\"response\":{}}"; };
        }
    }
}
