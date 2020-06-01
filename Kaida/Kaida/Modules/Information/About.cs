﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Kaida.Entities.Discord.Embeds;
using Kaida.Library.Attributes;
using Kaida.Library.Extensions;
using Serilog;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Kaida.Modules.Information
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
            var description = new StringBuilder().AppendLine($"App Version: {ApplicationInformation.Version}")
                                                 .AppendLine($"Gateway Version: {client.GatewayVersion}")
                                                 .AppendLine($"DSharpPlus Version: {client.VersionString}")
                                                 .AppendLine($"Redis Version: soon:tm:")
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
        public async Task Owners(CommandContext context)
        {
            var owners = context.Client.CurrentApplication.Owners;
            var fields = new List<EmbedField>();
            var description = new StringBuilder();

            foreach (var owner in owners)
            {
                description.AppendLine($"{owner.GetUsertag()} | {owner.Id}");
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