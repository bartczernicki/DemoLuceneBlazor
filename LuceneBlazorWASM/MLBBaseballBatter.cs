using System;

namespace LuceneBlazorWASM
{
    public class MLBBaseballBatter
    {
        public bool InductedToHallOfFame { get; set; }

        public bool OnHallOfFameBallot { get; set; }

        public string FullPlayerName { get; set; }
        public float YearsPlayed { get; set; }

        public float AB { get; set; }

        public float R { get; set; }

        public float H { get; set; }

        public float Doubles { get; set; }

        public float Triples { get; set; }

        public float HR { get; set; }

        public float RBI { get; set; }

        public float SB { get; set; }

        public float BattingAverage { get; set; }

        public float SluggingPct { get; set; }

        public float AllStarAppearances { get; set; }

        public float MVPs { get; set; }

        public float TripleCrowns { get; set; }

        public float GoldGloves { get; set; }

        public float MajorLeaguePlayerOfTheYearAwards { get; set; }

        public float TB { get; set; }

        public float TotalPlayerAwards { get; set; }

        public float LastYearPlayed { get; set; }

        public string ID { get; set; }

        public override string ToString()
        {
            var mlbBatterString = $@"FullPlayerName:{this.FullPlayerName}, YearPlayed:{this.YearsPlayed}, AB:{this.AB}
            R:{this.R}, H:{this.H}, Doubles:{this.Doubles}, Triples:{this.Triples}, HR:{this.HR}, RBI:{this.RBI}, SB:{this.SB},
            BattingAvg:{this.BattingAverage}, Slg:{this.SluggingPct}, AllStar:{this.AllStarAppearances}, MVPs:{this.MVPs},
            TripleCrowns:{this.TripleCrowns}, GoldGloves:{this.GoldGloves}, PlayerOfYear:{this.MajorLeaguePlayerOfTheYearAwards}, TB:{this.TB}, LastYearPlayed:{this.LastYearPlayed}, ID:{this.ID}";

            return mlbBatterString;
        }

        public MLBBaseballBatter CalculateStatisticsProratedBySeason(int numberOfSeasons)
        {
            var batter = new MLBBaseballBatter
            {
                FullPlayerName = this.FullPlayerName,
                ID = this.ID,
                InductedToHallOfFame = false,
                LastYearPlayed = this.LastYearPlayed,
                OnHallOfFameBallot = false,
                YearsPlayed = numberOfSeasons * 1f,
                AB = (this.AB / this.YearsPlayed) * numberOfSeasons,
                R = (this.R / this.YearsPlayed) * numberOfSeasons,
                H = (this.H / this.YearsPlayed) * numberOfSeasons,
                Doubles = (this.Doubles / this.YearsPlayed) * numberOfSeasons,
                Triples = (this.Triples / this.YearsPlayed) * numberOfSeasons,
                HR = (float)Math.Round(
                    ((this.HR / this.YearsPlayed) * numberOfSeasons), 0,
                    MidpointRounding.AwayFromZero),
                RBI = (this.RBI / this.YearsPlayed) * numberOfSeasons,
                SB = (this.SB / this.YearsPlayed) * numberOfSeasons,
                BattingAverage =
                    (float)
                    (
                    ((this.H / this.YearsPlayed) * numberOfSeasons) /
                    ((this.AB / this.YearsPlayed) * numberOfSeasons)
                    ),
                AllStarAppearances = (float)Math.Round(
                    (Decimal)(this.AllStarAppearances / this.YearsPlayed) * numberOfSeasons,
                    0,
                    MidpointRounding.AwayFromZero),
                MVPs = (this.MVPs / this.YearsPlayed) * numberOfSeasons,
                TripleCrowns = (this.TripleCrowns / this.YearsPlayed) * numberOfSeasons,
                GoldGloves = (float)Math.Round(
                    ((this.GoldGloves / this.YearsPlayed) * numberOfSeasons), 0, MidpointRounding.AwayFromZero),
                MajorLeaguePlayerOfTheYearAwards = (this.MajorLeaguePlayerOfTheYearAwards / this.YearsPlayed) * numberOfSeasons,
                TB = (float)Math.Round(
                ((this.TB / this.YearsPlayed) * numberOfSeasons), 0, MidpointRounding.AwayFromZero),
                TotalPlayerAwards = (float)Math.Round(
                (this.TotalPlayerAwards / this.YearsPlayed) * numberOfSeasons, 0, MidpointRounding.AwayFromZero),
                SluggingPct = TB / AB
            };

            return batter;
        }

        public static string GetDefaultBatter()
        {
            return "Mike Trout";
        }
    }
}
