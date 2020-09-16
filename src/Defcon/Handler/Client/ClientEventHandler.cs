using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Defcon.Core.Entities.Enums;
using Defcon.Data.Guilds;
using Defcon.Library.Extensions;
using Defcon.Library.Redis;
using Defcon.Library.Services.Logs;
using Defcon.Library.Services.Reactions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using MoreLinq.Extensions;
using Serilog;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Defcon.Handler.Client
{
    public class ClientEventHandler
    {
        private readonly DiscordShardedClient client;
        private readonly ILogger logger;
        private readonly IReactionService reactionService;
        private readonly ILogService logService;
        private readonly IRedisDatabase redis;
        
        private int activityIndex;
        private Timer timer;

        public ClientEventHandler(DiscordShardedClient client, ILogger logger, IRedisDatabase redis, IReactionService reactionService, ILogService logService)
        {
            this.client = client;
            this.logger = logger;
            this.reactionService = reactionService;
            this.logService = logService;
            this.redis = redis;

            this.logger.Information("Register events...");
            this.client.Ready += Ready;
            this.client.GuildDownloadCompleted += GuildDownloadCompleted;
            this.client.GuildAvailable += GuildAvailable;
            this.client.GuildUnavailable += GuildUnavailable;
            this.client.GuildCreated += GuildCreated;
            this.client.GuildUpdated += GuildUpdated;
            this.client.GuildDeleted += GuildDeleted;
            this.client.GuildEmojisUpdated += GuildEmojisUpdated;
            this.client.GuildIntegrationsUpdated += GuildIntegrationsUpdated;
            this.client.ChannelCreated += ChannelCreated;
            this.client.ChannelDeleted += ChannelDeleted;
            this.client.ChannelUpdated += ChannelUpdated;
            this.client.GuildRoleCreated += GuildRoleCreated;
            this.client.GuildRoleUpdated += GuildRoleUpdated;
            this.client.GuildRoleDeleted += GuildRoleDeleted;
            this.client.GuildMemberAdded += GuildMemberAdded;
            this.client.GuildMemberRemoved += GuildMemberRemoved;
            this.client.GuildMemberUpdated += GuildMemberUpdated;
            this.client.GuildBanAdded += GuildBanAdded;
            this.client.GuildBanRemoved += GuildBanRemoved;
            this.client.DmChannelCreated += DmChannelCreated;
            this.client.DmChannelDeleted += DmChannelDeleted;
            this.client.MessageCreated += MessageCreated;
            this.client.MessageUpdated += MessageUpdated;
            this.client.MessageDeleted += MessageDeleted;
            this.client.MessagesBulkDeleted += MessagesBulkDeleted;
            this.client.MessageReactionAdded += MessageReactionAdded;
            this.client.MessageReactionRemoved += MessageReactionRemoved;
            this.client.MessageReactionRemovedEmoji += MessageReactionRemovedEmoji;
            this.client.MessageReactionsCleared += MessageReactionsCleared;
            this.client.VoiceServerUpdated += VoiceServerUpdated;
            this.client.VoiceStateUpdated += VoiceStateUpdated;
            this.client.SocketOpened += SocketOpened;
            this.client.SocketClosed += SocketClosed;
            this.client.SocketErrored += SocketErrored;
            this.client.ClientErrored += ClientErrored;
            this.client.UnknownEvent += UnknownEvent;
            this.logger.Information("Registered all events!");
        }

        private async Task Ready(ReadyEventArgs e)
        {
            logger.Information("Client is ready!");

            timer = new Timer(async _ =>
            {
                var guilds = e.Client.Guilds.Values.ToList();
                var guildsCount = guilds.Count;
                var totalUsers = new List<DiscordUser>();

                foreach (var guild in guilds)
                {
                    var members = await guild.GetAllMembersAsync();
                    totalUsers.AddRange(members.Where(x => x.IsBot == false));
                }

                var uniqueUsers = totalUsers.DistinctBy(x => x.Id)
                                            .ToList()
                                            .Count;

                var activities = new List<DiscordActivity>
                {
                    new DiscordActivity {ActivityType = ActivityType.Watching, Name = $"{guildsCount} servers"},
                    new DiscordActivity {ActivityType = ActivityType.ListeningTo, Name = $"{uniqueUsers} unique users"}
                };
                await client.UpdateStatusAsync(activities.ElementAtOrDefault(activityIndex), UserStatus.Online, DateTimeOffset.UtcNow);
                activityIndex = activityIndex + 1 == activities.Count ? 0 : activityIndex + 1;
            }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(60));
        }

        private async Task GuildDownloadCompleted(GuildDownloadCompletedEventArgs e)
        {
            logger.Information("Client downloaded all guilds successfully.");
        }

        private async Task GuildAvailable(GuildCreateEventArgs e)
        {
            await redis.InitGuild(e.Guild.Id);
            logger.Information($"Guild '{e.Guild.Name}' ({e.Guild.Id}) became available.");
        }

        private async Task GuildUnavailable(GuildDeleteEventArgs e)
        {
            logger.Information($"Guild '{e.Guild.Name}' ({e.Guild.Id}) became unavailable.");
        }

        private async Task GuildCreated(GuildCreateEventArgs e)
        {
            await redis.InitGuild(e.Guild.Id);
            logger.Information($"Joined the guild '{e.Guild.Name}' ({e.Guild.Id}).");
        }

        private async Task GuildUpdated(GuildUpdateEventArgs e)
        {
            logger.Information($"Guild '{e.GuildAfter.Name}' ({e.GuildAfter.Id}) has been updated.");
        }

        private async Task GuildDeleted(GuildDeleteEventArgs e)
        {
            logger.Information($"Left the guild '{e.Guild.Name}' ({e.Guild.Id}).");
        }

        private async Task GuildEmojisUpdated(GuildEmojisUpdateEventArgs e)
        {
            logger.Information($"Guild '{e.Guild.Name}' ({e.Guild.Id}) has updated their emojis.");
        }

        private async Task GuildIntegrationsUpdated(GuildIntegrationsUpdateEventArgs e)
        {
            logger.Information($"Guild '{e.Guild.Name}' ({e.Guild.Id}) has updated their integrations.");
        }

        private async Task ChannelCreated(ChannelCreateEventArgs e)
        {
            await logService.GuildLogger(e.Guild, e, LogType.Guild);

            logger.Information($"Channel '{e.Channel.Name}' ({e.Channel.Id}) has been created on guild '{e.Guild.Name}' ({e.Guild.Id}).");
        }

        private async Task ChannelUpdated(ChannelUpdateEventArgs e)
        {
            await logService.GuildLogger(e.Guild, e, LogType.Guild);

            if (!e.ChannelBefore.IsPrivate)
            {
                logger.Information($"Channel '{e.ChannelAfter.Name}' ({e.ChannelAfter.Id}) has been updated on guild '{e.Guild.Name}' ({e.Guild.Id}).");
            }
        }

        private async Task ChannelDeleted(ChannelDeleteEventArgs e)
        {
            await logService.GuildLogger(e.Guild, e, LogType.Guild);

            if (!e.Channel.IsPrivate)
            {
                logger.Information($"Channel '{e.Channel.Name}' ({e.Channel.Id}) has been deleted on guild '{e.Guild.Name}' ({e.Guild.Id}).");
            }
        }

        private async Task GuildRoleCreated(GuildRoleCreateEventArgs e)
        {
            await logService.GuildLogger(e.Guild, e, LogType.Guild);

            logger.Information($"Role '{e.Role.Name}' ({e.Role.Id}) has been created on guild '{e.Guild.Name}' ({e.Guild.Id}).");
        }

        private async Task GuildRoleUpdated(GuildRoleUpdateEventArgs e)
        {
            await logService.GuildLogger(e.Guild, e, LogType.Guild);

            logger.Information($"Role '{e.RoleAfter.Name}' ({e.RoleAfter.Id}) has been updated on guild '{e.Guild.Name}' ({e.Guild.Id}).");
        }

        private async Task GuildRoleDeleted(GuildRoleDeleteEventArgs e)
        {
            await logService.GuildLogger(e.Guild, e, LogType.Guild);

            logger.Information($"Role '{e.Role.Name}' ({e.Role.Id}) has been deleted on guild '{e.Guild.Name}' ({e.Guild.Id}).");
        }

        private async Task GuildMemberAdded(GuildMemberAddEventArgs e)
        {
            await redis.InitUser(e.Member.Id);
            await logService.GuildLogger(e.Guild, e, LogType.JoinedLeft);

            logger.Information($"Member '{e.Member.GetUsertag()}' ({e.Member.Id}) has joined the guild '{e.Guild.Name}' ({e.Guild.Id}).");
        }

        private async Task GuildMemberUpdated(GuildMemberUpdateEventArgs e)
        {
            var before = string.IsNullOrWhiteSpace(e.NicknameBefore) ? e.Member.Username : e.NicknameBefore;
            var after = string.IsNullOrWhiteSpace(e.NicknameAfter) ? e.Member.Username : e.NicknameAfter;

            if (before != after)
            {
                await logService.GuildLogger(e.Guild, e, LogType.Nickname);
                logger.Information($"'{e.Member.GetUsertag()}' ({e.Member.Id}) has changed there nickname from '{before}' to '{after}' on '{e.Guild.Name}' ({e.Guild.Id}).");
            }
        }

        private async Task GuildMemberRemoved(GuildMemberRemoveEventArgs e)
        {
            await logService.GuildLogger(e.Guild, e, LogType.JoinedLeft);

            logger.Information($"Member '{e.Member.GetUsertag()}' ({e.Member.Id}) has left the guild '{e.Guild.Name}' ({e.Guild.Id}).");
        }

        private async Task GuildBanAdded(GuildBanAddEventArgs e)
        {
            logger.Information($"'{e.Member.GetUsertag()}' ({e.Member.Id}) has been banned from '{e.Guild.Name}' ({e.Guild.Id}).");
        }

        private async Task GuildBanRemoved(GuildBanRemoveEventArgs e)
        {
            logger.Information($"'{e.Member.GetUsertag()}' ({e.Member.Id}) has been unbanned from '{e.Guild.Name}' ({e.Guild.Id}).");
        }

        private async Task DmChannelCreated(DmChannelCreateEventArgs e)
        {
            var user = e.Channel.Recipients.First(x => !x.IsBot);
            logger.Information($"Direct message with '{user.GetUsertag()}' ({user.Id}) has been created.");
        }

        private async Task DmChannelDeleted(DmChannelDeleteEventArgs e)
        {
            var user = e.Channel.Recipients.First(x => !x.IsBot);
            logger.Information($"Direct message with '{user.GetUsertag()}' ({user.Id}) has been deleted.");
        }

        private async Task MessageCreated(MessageCreateEventArgs e)
        {
            /* This would kill my bot */
        }

        private async Task MessageUpdated(MessageUpdateEventArgs e)
        {
            if (e.Channel.IsPrivate)
            {
                logger.Information($"The message ({e.Message.Id}) from '{e.Author.GetUsertag()}' ({e.Author.Id}) was updated in the direct message.");
            }
            else
            {
                if (e.Author != null && !e.Author.IsBot && !e.MessageBefore.Content.Equals(e.Message.Content))
                {
                    await logService.GuildLogger(e.Guild, e, LogType.Message);
                    logger.Information($"The message ({e.Message.Id}) from '{e.Author.GetUsertag()}' ({e.Author.Id}) was updated in '{e.Channel.Name}' ({e.Channel.Id}) on '{e.Guild.Name}' ({e.Guild.Id}).");
                }
            }
        }

        private async Task MessageDeleted(MessageDeleteEventArgs e)
        {
            var guildData = await redis.GetAsync<Guild>(RedisKeyNaming.Guild(e.Guild.Id));

            if (e.Channel.IsPrivate)
            {
                logger.Information(!string.IsNullOrWhiteSpace(e.Message.Content)
                                       ? $"The message ({e.Message.Id}) from '{e.Message.Author.GetUsertag()}' ({e.Message.Author.Id}) was deleted in the direct message."
                                       : $"The message ({e.Message.Id}) was deleted in the direct message.");
            }
            else
            {
                if (!e.Message.Author.IsBot && !e.Message.Content.StartsWith(guildData.Prefix))
                {
                    await logService.GuildLogger(e.Guild, e, LogType.Message);
                }
            }
        }

        private async Task MessagesBulkDeleted(MessageBulkDeleteEventArgs e)
        {
            if (!e.Channel.IsPrivate)
            {
                await logService.GuildLogger(e.Guild, e, LogType.Message);
            }
        }

        private async Task MessageReactionAdded(MessageReactionAddEventArgs e)
        {
            if (!e.User.IsBot)
            {
                var emojiName = e.Emoji.Name == "??" ? "an unknown reaction" : $"the reaction '{e.Emoji.Name}' ({e.Emoji.Id})";

                if (e.Channel.IsPrivate)
                {
                    logger.Information($"{e.User.GetUsertag()} ({e.User.Id}) has added {emojiName} to the message '{e.Message.Id}' in the direct message.");
                }
                else
                {
                    logger.Information($"'{e.User.GetUsertag()}' ({e.User.Id}) has added {emojiName} to the message '{e.Message.Id}' in the channel '{e.Channel.Name}' ({e.Channel.Id}) on the guild '{e.Guild.Name}' ({e.Guild.Id}).");

                    var member = e.Channel.Guild.GetMemberAsync(e.User.Id).Result;

                    if (reactionService.IsListener(e.Guild.Id, e.Message.Id, e.Emoji))
                    {
                        reactionService.ManageRole(e.Message, e.Channel, member, e.Emoji);
                    }

                    var guild = await redis.GetAsync<Guild>(RedisKeyNaming.Guild(e.Guild.Id));
                    if (guild.RulesAgreement.MessageId == e.Message.Id && e.Emoji.Id == EmojiLibrary.Accepted)
                    {
                        var role = e.Guild.GetRole(guild.RulesAgreement.RoleId);

                        if (role == null)
                        {
                            // Contact member and server owner!
                        }
                        else
                        {
                            await member.GrantRoleAsync(role);
                        }
                    }
                    else if (guild.RulesAgreement.MessageId == e.Message.Id && e.Emoji.Id == EmojiLibrary.Denied)
                    {
                        await e.Message.DeleteReactionAsync(e.Emoji, member);
                        await member.RemoveAsync("Did not accept the server rules.");
                    }
                }
            }
        }

        private async Task MessageReactionRemoved(MessageReactionRemoveEventArgs e)
        {
            if (!e.User.IsBot)
            {
                var emojiName = e.Emoji.Name == "??" ? "an unknown reaction" : $"the reaction '{e.Emoji.Name}' ({e.Emoji.Id})";

                if (e.Channel.IsPrivate)
                {
                    logger.Information($"{e.User.GetUsertag()} ({e.User.Id}) has removed {emojiName} to the message '{e.Message.Id}' in the direct message.");

                    await Task.CompletedTask;
                }

                var guild = redis.GetAsync<Guild>(RedisKeyNaming.Guild(e.Guild.Id))
                                 .GetAwaiter()
                                 .GetResult();

                if (guild.RulesAgreement.MessageId == e.Message.Id && e.Emoji.Id == EmojiLibrary.Accepted)
                {
                    var role = e.Guild.GetRole(guild.RulesAgreement.RoleId);
                    var member = e.Channel.Guild.GetMemberAsync(e.User.Id).Result;

                    member.RevokeRoleAsync(role);
                }

                logger.Information($"'{e.User.GetUsertag()}' ({e.User.Id}) has removed {emojiName} to the message '{e.Message.Id}' in the channel '{e.Channel.Name}' ({e.Channel.Id}) on the guild '{e.Guild.Name}' ({e.Guild.Id}).");
            }
        }

        private async Task MessageReactionRemovedEmoji(MessageReactionRemoveEmojiEventArgs e)
        {
            logger.Information($"All reactions with the emoji '{e.Emoji.Name}' ({e.Emoji.Id}) of the message ({e.Message.Id}) has been removed in the channel '{e.Channel.Name}' ({e.Channel.Id}) on the guild '{e.Guild.Name}' ({e.Guild.Id}).");
        }

        private async Task MessageReactionsCleared(MessageReactionsClearEventArgs e)
        {
            logger.Information($"All reactions of the message ({e.Message.Id}) has been cleared in the channel '{e.Channel.Name}' ({e.Channel.Id}) on the guild '{e.Guild.Name}' ({e.Guild.Id}).");
        }

        private async Task VoiceServerUpdated(VoiceServerUpdateEventArgs e)
        {
            logger.Information($"Voice server has been updated to '{e.Endpoint}' on the guild '{e.Guild.Name}' ({e.Guild.Id}).");
        }

        private async Task VoiceStateUpdated(VoiceStateUpdateEventArgs e)
        {
            await logService.GuildLogger(e.Guild, e, LogType.Voice);
        }

        private async Task SocketOpened()
        {
            logger.Information("Socket has been opened.");
        }

        private async Task SocketClosed(SocketCloseEventArgs e)
        {
            logger.Warning($"Socket has been closed. [{e.CloseCode}] ({e.CloseMessage})");
        }

        private async Task SocketErrored(SocketErrorEventArgs e)
        {
            logger.Error($"Socket has been errored! ({e.Exception.Message})");
        }

        private async Task ClientErrored(ClientErrorEventArgs e)
        {
            switch (e.Exception)
            {
                case BadRequestException badRequestException:
                    logger.Error($"[{e.EventName}] Bad Request: {badRequestException.Message}");

                    break;
                case RateLimitException rateLimitException:
                    logger.Error($"[{e.EventName}] Rate Limit: {rateLimitException.Message}");

                    break;
                case UnauthorizedException unauthorizedException:
                    logger.Error($"[{e.EventName}] Unauthorized: {unauthorizedException.Message}");

                    break;
                case NotFoundException notFoundException:
                    logger.Error($"[{e.EventName}] Not Found: {notFoundException.WebResponse}");

                    break;
                case AggregateException aggregateException:
                    logger.Error($"[{e.EventName}] Aggregate: {aggregateException.Message}");

                    break;
            }

            logger.Error(e.Exception, $"Client has occurred an error: {e.EventName}");
        }

        private async Task UnknownEvent(UnknownEventArgs e)
        {
            logger.Warning($"Unknown Event: {e.EventName}");
        }
    }
}