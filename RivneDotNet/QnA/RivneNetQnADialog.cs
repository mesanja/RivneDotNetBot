using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using RivneDotNet.Users;

namespace RivneDotNet.QnA
{
    [Serializable]
    [QnAMakerService("", "")]
    public class RivneNetQnADialog : QnAMakerDialog<object>
    {
        protected override async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            var message = await item;

            if (message.Text.Contains("menu") || message.Text.Contains("help"))
            {
                MakeChoice(context);
            }
            else
            {
                await base.MessageReceived(context, item);
            }
        }

        public override async Task NoMatchHandler(IDialogContext context, string originalQueryText)
        {
            await context.PostAsync($"Sorry, I couldn't find an answer for '{originalQueryText}'.");
            MakeChoice(context);
        }

        private void MakeChoice(IDialogContext context)
        {
            PromptDialog.Choice(context, this.OnOptionSelected,
                new List<string>() {"register", "hi", "where", "when", "why", "speakers", "honeycombsoft", "help"},
                "You can ask Rivne .Net community bot about:");
        }

        [QnAMakerResponseHandler(50)]
        public async Task LowScoreHandler(IDialogContext context, string originalQueryText, QnAMakerResult result)
        {
            await context.PostAsync($"I found an answer that might help...{result.Answer}.");
            context.Wait(MessageReceived);
        }

        private async Task OnOptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            var question = await result;
            var answer = QnAService.GetAnswer(question);
            await context.PostAsync(answer.Answer);
        }
    }
}