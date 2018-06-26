using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaliensBot
{
    public class GetPlayerInfo
    {
        public Response response;

        public class Response
        {
            public int active_planet;
            public int active_zone_game;
            public int active_zone_position;
            public int score;
            public int level;
            public int next_level_score;
        }
    }

    public class GetPlanets
    {
        public Response response;

        public class Response
        {
            public List<Planets> planets;

            public class Planets
            {
                public int id;
                public State state;

                public class State
                {
                    public string name;
                    public string difficulty;
                    public bool active;
                    public bool captured;
                    public float capture_progress;
                }
            }
        }
    }

    public class GetPlanet
    {
        public Response response;

        public class Response
        {
            public List<Planets> planets;

            public class Planets
            {
                public int id;
                public State state;
                public List<Zones> zones;

                public class State
                {
                    public string name;
                    public string difficulty;
                    public bool active;
                    public bool captured;
                    public float capture_progress;
                }

                public class Zones
                {
                    public int zone_position;
                    public int gameid;
                    public int difficulty;
                    public bool captured;
                    public float capture_progress;
                }
            }
        }
    }

    public class ReportScore
    {
        public Response response;

        public class Response
        {
            public int old_score;
            public int old_level;
            public int new_score;
            public int new_level;
            public int next_level_score;
        }
    }
}
