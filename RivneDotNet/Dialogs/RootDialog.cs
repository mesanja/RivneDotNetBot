using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using RivneDotNet.Users;

namespace RivneDotNet.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private const string RegisterUser = "Sing up for free!";

        private const string UsersStatistic = "Statistic";

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            if (message.Text.ToLower().Contains("register"))
            {
                await context.Forward(new UserDialog(), this.ResumeAfterUserDialog, message, CancellationToken.None);
            }
            else if(message.Text.ToLower().Contains("statistic"))
            {
                context.Call(new LuisDialogClass(), this.ResumeAfterStatisticDialog);
            }
            else
            {
                await context.Forward(new HelpDialog(), this.ResumeAfterHelpDialog, message, CancellationToken.None);
            }
        }

        public static async Task<T> RequestAsync<T>(string input)
        {
            var strEscaped = Uri.EscapeDataString(input);
            var url = $"https://api.projectoxford.ai/luis/v1/application?id={}&subscription-key={}&q={strEscaped}";

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(content);
                }
            }

            return default(T);
        }

        private void ShowOptions(IDialogContext context)
        {
            PromptDialog.Choice(context, this.OnOptionSelected, new List<string>() { RegisterUser, UsersStatistic }, "Wana play with my options?", "Not a valid option", 3);
        }

        private async Task OnOptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;

                switch (optionSelected)
                {
                    case RegisterUser:
                        context.Call(new UserDialog(), this.ResumeAfterUserDialog);
                        break;

                    case UsersStatistic:
                        break;
                }
            }
            catch (TooManyAttemptsException ex)
            {
                await context.PostAsync($"Ooops! Too many attemps :(. But don't worry, I'm handling that exception and you can try again!");

                context.Wait(this.MessageReceivedAsync);
            }
        }

        private async Task ResumeAfterHelpDialog(IDialogContext context, IAwaitable<HelpAnswer> result)
        {
            var message = await result;

            if (message.Type == HelpDialogResultTypes.RootDialog)
            {
                await context.PostAsync($"Thanks for using help. I can register you or show some statistic.");
                context.Wait(this.MessageReceivedAsync);
            }
            else
            {
                // just post answer we received from Help dialog
                await context.PostAsync(message.QnAnswer);
                context.Wait(this.MessageReceivedAsync);
            }
        }
        private async Task ResumeAfterStatisticDialog(IDialogContext context, IAwaitable<object> result)
        {
            try
            {
                var message = await result;

                if (message != null)
                {
                }
            }
            catch (Exception ex)
            {
                await context.PostAsync($"Failed with message: {ex.Message}");
            }
            finally
            {
                context.Wait(this.MessageReceivedAsync);
            }
        }

        private async Task ResumeAfterUserDialog(IDialogContext context, IAwaitable<UserForm> result)
        {
            try
            {
                var message = await result;

                if (message != null)
                {
                    UsersRepository repository = new UsersRepository();
                    repository.AddUser(message);
                }
            }
            catch (Exception ex)
            {
                await context.PostAsync($"Failed with message: {ex.Message}");
            }
            finally
            {
                context.Wait(this.MessageReceivedAsync);
            }
        }
    }
}