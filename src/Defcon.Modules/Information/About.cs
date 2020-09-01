using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Defcon.Core;
using Defcon.Entities.Discord.Embeds;
using Defcon.Library.Attributes;
using Defcon.Library.Extensions;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Serilog;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Defcon.Modules.Information
{
    [Category("Information")]
    [Group("About")]
    [Aliases("Kaida", "Bot")]
    [Description("Shows you details about the project.")]
    public class About : BaseCommandModule
    {
        private readonly ILogger logger;
        private readonly IRedisDatabase redis;

        public About(ILogger logger, IRedisDatabase redis)
        {
            this.logger = logger;
            this.redis = redis;
        }

        [GroupCommand]
        public async Task Info(CommandContext context)
        {
            var client = context.Client;
            var redisInfos = await redis.GetInfoAsync();

            var description = new StringBuilder().AppendLine($"App Version: {ApplicationInformation.Version}")
                                                 .AppendLine($"Gateway Version: {client.GatewayVersion}")
                                                 .AppendLine($"DSharpPlus Version: {client.VersionString}")
                                                 .AppendLine($"Redis Version: {redisInfos.GetValueOrDefault("redis_version")}")
                                                 .AppendLine($"mySQL Version: soon:tm:")
                                                 .AppendLine($"OS: {redisInfos.GetValueOrDefault("os")}")
                                                 .AppendLine($"Redis uptime: {TimeSpan.FromSeconds(int.Parse(redisInfos.GetValueOrDefault("uptime_in_seconds")))}")
                                                 .AppendLine($"Shard Id: {client.ShardId}")
                                                 .AppendLine($"Servers: soon:tm:")
                                                 .AppendLine($"Users: soon:tm:").ToString();

            var links = new StringBuilder().AppendLine(Formatter.MaskedUrl("Invite", new Uri(client.GenerateInviteLink())))
                                           .AppendLine(Formatter.MaskedUrl("GitHub", new Uri(ApplicationInformation.GitHub))).ToString();

            var fields = new List<EmbedField>
            {
                new EmbedField
                {
                    Inline = true,
                    Name = "Support",
                    Value = Formatter.MaskedUrl("Discord", new Uri(ApplicationInformation.DiscordServer))
                },
                new EmbedField
                {
                    Inline = true,
                    Name = "Links",
                    Value = links
                }
            };

            var embed = new Embed
            {
                Title = "About",
                Description = description,
                ThumbnailUrl = context.Client.CurrentUser.AvatarUrl,
                Fields = fields
            };

            await context.SendEmbedMessageAsync(embed);
        }

        [Command("Owners")]
        [Description("Shows a list of my masters!")]
        public async Task Owners(CommandContext context)
        {
            var owners = context.Client.CurrentApplication.Owners;
            var fields = new List<EmbedField>();
            var description = new StringBuilder();

            foreach (var owner in owners)
            {
                description.AppendLine($"{owner.Mention} {owner.GetUsertag()} {FormatterExtension.Spoiler($"{owner.Id}")}");
            }

            fields.Add(new EmbedField()
            {
                Inline = false,
                Name = "Owners",
                Value = description.ToString()
            });

            var embed = new Embed()
            {
                Fields = fields
            };

            await context.SendEmbedMessageAsync(embed);
        }
    }
}