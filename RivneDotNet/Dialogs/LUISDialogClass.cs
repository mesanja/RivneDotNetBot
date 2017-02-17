using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using RivneDotNet.Users;

namespace RivneDotNet.Dialogs
{
    [LuisModel("", "")]
    [Serializable]
    public class LuisDialogClass : LuisDialog<object>
    {
        private const string UserStatistcIntentName = "UserStatistic";

        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            var msg = context.MakeMessage();
            msg.TextFormat = "markdown";
            msg.Text = "*:(*";
            await context.PostAsync(msg);
        }

        [LuisIntent(UserStatistcIntentName)]
        public async Task HighScores(IDialogContext context, LuisResult result)
        {
            // See if the intent has a > .99 match
            bool boolIntentMatch = false;
            foreach (var objIntent in result.Intents)
            {
                // If the UserStatistic Intent is detected and it's score is greater than or = to .99 
                if ((objIntent.Intent == UserStatistcIntentName) && (objIntent.Score >= .99f))
                {
                    boolIntentMatch = true;
                }
            }
            if (boolIntentMatch)
            {
                int days = 0;
                var dayIntentResult = GetDaysCountFromDaysIntent(result);
                days = dayIntentResult.Item2;
                bool isFound = false;
                if (!dayIntentResult.Item1)
                {
                    var timeValueIntent = GetDaysCountFromTimeValueIntent(result);
                    if (timeValueIntent.Item1)
                    {
                        days = timeValueIntent.Item2;
                        isFound = true;
                        // check whether user is drunk
                        if (timeValueIntent.Item3)
                        {
                            await CreateDrunkAttachment(context);
                        }
                    }
                }
                else
                {
                    isFound = true;
                }

                if (isFound)
                {
                    await context.PostAsync($"I see you need my statistic for {days} days. Processing it...");
                    UsersRepository repository = new UsersRepository();
                    DateTime currentDate = DateTime.Now.AddDays(-days);
                    var count = repository.GetAlll().Count(x => x.Date > currentDate);
                    await context.PostAsync($"I have {count} user who registered {days} days ago (since {currentDate.ToShortDateString()})");
                }
            }
            else
            {
                context.Done<object>(null);
            }
        }

        private static async Task CreateDrunkAttachment(IDialogContext context)
        {
            var msg = context.MakeMessage();
            msg.Attachments = new List<Attachment>
            {
                new Attachment
                {
                    ContentUrl = "https://cdn.meme.am/cache/instances/folder873/500x/44796873.jpg",
                    ContentType = "image/jpg",
                    Name = "someone is drunk - jpg"
                }
            };
            await context.PostAsync(msg, CancellationToken.None);
        }

        private static Tuple<bool,int,bool> GetDaysCountFromTimeValueIntent(LuisResult luisResult)
        {
            EntityRecommendation timeValueEntity;
            int daysCount = 0;
            bool probablyDrunk = false;
            bool result = false;
            if (luisResult.TryFindEntity("TimeValue", out timeValueEntity))
            {
                string numberPart = Regex.Replace(timeValueEntity.Entity, @"[^0-9]+", "");
                if (int.TryParse(numberPart, out daysCount))
                {
                    probablyDrunk = true;
                    result = true;
                }
                else
                {
                    switch (timeValueEntity.Entity)
                    {
                        case "week":
                            {
                                daysCount = 7;
                            }
                            break;
                        case "month":
                            {
                                daysCount = 30;
                            }
                            break;
                        case "yesterday":
                            daysCount = 1;
                            break;
                    }
                    result = daysCount > 0;
                }
            }
            return new Tuple<bool, int, bool>(result, daysCount, probablyDrunk);
        }

        private Tuple<bool, int> GetDaysCountFromDaysIntent(LuisResult luisResult)
        {
            bool result = false;
            int daysCount = 0;
            EntityRecommendation daysEntity;
            if (luisResult.TryFindEntity("Days", out daysEntity))
            {
                if (int.TryParse(daysEntity.Entity, out daysCount))
                {
                    result = true;
                }
            }
            return new Tuple<bool, int>(result, daysCount);
        }
    }
}