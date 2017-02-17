using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;
using RivneDotNet.Users;

namespace RivneDotNet.Dialogs
{
    [Serializable]
    public class UserDialog : IDialog<UserForm>
    {
        public UserDialog()
        {
        }

        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
            return Task.CompletedTask;
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            context.Call(UserForm.BuildFormDialog(FormOptions.PromptInStart), FormComplete);
        }

        private async Task FormComplete(IDialogContext context, IAwaitable<UserForm> result)
        {
            UserForm form = null;
            try
            {
                form = await result;
                if (form != null)
                {
                    await context.PostAsync("Thanks for completing the form!");
                }
                else
                {
                    await context.PostAsync("Form returned empty response! Type anything to restart it.");
                }
            }
            catch (OperationCanceledException)
            {
                await context.PostAsync("You canceled the form! Type anything to restart it.");
            }

            context.Done(form);
        }
    }
}