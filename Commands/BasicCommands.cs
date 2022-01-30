using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;

namespace Ding_Dong_Discord_Bot.Commands
{
    public class BasicCommands : BaseCommandModule
    {
        [Command("echo")]
        [Description("Replay the message the user sent")]
        public async Task RelayMessage(CommandContext ctn_, [Description("Message")] string msg_)
        {
            await ctn_.Channel.SendMessageAsync(msg_).ConfigureAwait(false);
        }
    }
}
