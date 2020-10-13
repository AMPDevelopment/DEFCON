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
using DSharpPlus.CommandsNext.Exceptions;
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
        private readonly ILogService logService;
        private readonly IReactionService reactionService;
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
            this.client.Resumed += Resumed;
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
            this.client.GuildMembersChunked += GuildMemberChunked;
            this.client.GuildBanAdded += GuildBanAdded;
            this.client.GuildBanRemoved += GuildBanRemoved;
            this.client.InviteCreated += InviteCreated;
            this.client.InviteDeleted += InviteDeleted;
            this.client.DmChannelCreated += DmChannelCreated;
            this.client.DmChannelDeleted += DmChannelDeleted;
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
            this.client.WebhooksUpdated += WebhooksUpdated;
            this.client.ClientErrored += ClientErrored;
            this.client.UnknownEvent += UnknownEvent;
            this.logger.Information("Registered all events!");
        }

        private async Task Ready(DiscordClient c, ReadyEventArgs e)
        {
            Task.Run(async () =>
            {
                this.logger.Information("Client is ready!");
            
                timer = new Timer(async _ =>
                {
                    var guilds = c.Guilds.Values.ToList();
                    var guildsCount = guilds.Count;
                    var totalUsers = new List<DiscordUser>();

                    foreach (var guild in guilds)
                    {
                        var members = await guild.GetAllMembersAsync().ConfigureAwait(true);
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
                    await client.UpdateStatusAsync(activities.ElementAtOrDefault(activityIndex), UserStatus.Online, DateTimeOffset.UtcNow).ConfigureAwait(true);
                    activityIndex = activityIndex + 1 == activities.Count ? 0 : activityIndex + 1;
                }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(60));
            });
        }

        private async Task Resumed(DiscordClient c, ReadyEventArgs e)
        {
            this.logger.Information("The session has been resumed!");
        }

        private async Task GuildDownloadCompleted(DiscordClient c, GuildDownloadCompletedEventArgs e)
        {
            this.logger.Information("Client downloaded all guilds successfully.");
        }

        private async Task GuildAvailable(DiscordClient c, GuildCreateEventArgs e)
        {
            Task.Run(async () =>
            {
                await redis.InitGuild(e.Guild.Id).ConfigureAwait(true);
                this.logger.Information($"Guild '{e.Guild.Name}' ({e.Guild.Id}) became available.");
            });
        }

        private async Task GuildUnavailable(DiscordClient c, GuildDeleteEventArgs e)
        {
            this.logger.Information($"Guild '{e.Guild.Name}' ({e.Guild.Id}) became unavailable.");
        }

        private async Task GuildCreated(DiscordClient c, GuildCreateEventArgs e)
        {
            Task.Run(async () =>
            {
                await redis.InitGuild(e.Guild.Id).ConfigureAwait(true);
                this.logger.Information($"Joined the guild '{e.Guild.Name}' ({e.Guild.Id}).");
            });
        }

        private async Task GuildUpdated(DiscordClient c, GuildUpdateEventArgs e)
        {
            this.logger.Information($"Guild '{e.GuildAfter.Name}' ({e.GuildAfter.Id}) has been updated.");
        }

        private async Task GuildDeleted(DiscordClient c, GuildDeleteEventArgs e)
        {
            this.logger.Information($"Left the guild '{e.Guild.Name}' ({e.Guild.Id}).");
        }

        private async Task GuildEmojisUpdated(DiscordClient c, GuildEmojisUpdateEventArgs e)
        {
            this.logger.Information($"Guild '{e.Guild.Name}' ({e.Guild.Id}) has updated their emojis.");
        }

        private async Task GuildIntegrationsUpdated(DiscordClient c, GuildIntegrationsUpdateEventArgs e)
        {
            this.logger.Information($"Guild '{e.Guild.Name}' ({e.Guild.Id}) has updated their integrations.");
        }

        private async Task ChannelCreated(DiscordClient c, ChannelCreateEventArgs e)
        {
            Task.Run(async () =>
            {
                await logService.GuildLogger(c, e.Guild, e, LogType.Guild).ConfigureAwait(true);
                this.logger.Information($"Channel '{e.Channel.Name}' ({e.Channel.Id}) has been created on guild '{e.Guild.Name}' ({e.Guild.Id}).");
            });
        }

        private async Task ChannelUpdated(DiscordClient c, ChannelUpdateEventArgs e)
        {
            Task.Run(async () =>
            {
                if (!e.ChannelBefore.IsPrivate)
                {
                    await logService.GuildLogger(c, e.Guild, e, LogType.Guild).ConfigureAwait(true);
                    this.logger.Information($"Channel '{e.ChannelAfter.Name}' ({e.ChannelAfter.Id}) has been updated on guild '{e.Guild.Name}' ({e.Guild.Id}).");
                }
            });
        }

        private async Task ChannelDeleted(DiscordClient c, ChannelDeleteEventArgs e)
        {
            Task.Run(async () =>
            {
                if (!e.Channel.IsPrivate)
                {
                    await logService.GuildLogger(c, e.Guild, e, LogType.Guild).ConfigureAwait(true);
                    this.logger.Information($"Channel '{e.Channel.Name}' ({e.Channel.Id}) has been deleted on guild '{e.Guild.Name}' ({e.Guild.Id}).");
                }
            });
        }

        private async Task GuildRoleCreated(DiscordClient c, GuildRoleCreateEventArgs e)
        {
            Task.Run(async () =>
            {
                await logService.GuildLogger(c, e.Guild, e, LogType.Guild).ConfigureAwait(true);
                this.logger.Information($"Role '{e.Role.Name}' ({e.Role.Id}) has been created on guild '{e.Guild.Name}' ({e.Guild.Id}).");
            });
        }

        private async Task GuildRoleUpdated(DiscordClient c, GuildRoleUpdateEventArgs e)
        {
            Task.Run(async () =>
            {
                await logService.GuildLogger(c, e.Guild, e, LogType.Guild).ConfigureAwait(true);
                this.logger.Information($"Role '{e.RoleAfter.Name}' ({e.RoleAfter.Id}) has been updated on guild '{e.Guild.Name}' ({e.Guild.Id}).");
            });
        }

        private async Task GuildRoleDeleted(DiscordClient c, GuildRoleDeleteEventArgs e)
        {
            Task.Run(async () =>
            {
                await logService.GuildLogger(c, e.Guild, e, LogType.Guild).ConfigureAwait(true);
                this.logger.Information($"Role '{e.Role.Name}' ({e.Role.Id}) has been deleted on guild '{e.Guild.Name}' ({e.Guild.Id}).");
            });
        }

        private async Task GuildMemberAdded(DiscordClient c, GuildMemberAddEventArgs e)
        {
            Task.Run(async () =>
            {
                await redis.InitUser(e.Member.Id).ConfigureAwait(true);
                await logService.GuildLogger(c, e.Guild, e, LogType.JoinedLeft).ConfigureAwait(true);
                this.logger.Information($"Member '{e.Member.GetUsertag()}' ({e.Member.Id}) has joined the guild '{e.Guild.Name}' ({e.Guild.Id}).");
            });
        }

        private async Task GuildMemberUpdated(DiscordClient c, GuildMemberUpdateEventArgs e)
        {
            Task.Run(async () =>
            {
                var before = string.IsNullOrWhiteSpace(e.NicknameBefore) ? e.Member.Username : e.NicknameBefore;
                var after = string.IsNullOrWhiteSpace(e.NicknameAfter) ? e.Member.Username : e.NicknameAfter;

                if (before != after)
                {
                    await logService.GuildLogger(c, e.Guild, e, LogType.Nickname).ConfigureAwait(true);
                    this.logger.Information($"'{e.Member.GetUsertag()}' ({e.Member.Id}) has changed there nickname from '{before}' to '{after}' on '{e.Guild.Name}' ({e.Guild.Id}).");
                }
            });
        }

        private async Task GuildMemberChunked(DiscordClient c, GuildMembersChunkEventArgs e)
        {
            // Todo: logger
        }

        private async Task GuildMemberRemoved(DiscordClient c, GuildMemberRemoveEventArgs e)
        {
            Task.Run(async () =>
            {
                await logService.GuildLogger(c, e.Guild, e, LogType.JoinedLeft).ConfigureAwait(true);
                this.logger.Information($"Member '{e.Member.GetUsertag()}' ({e.Member.Id}) has left the guild '{e.Guild.Name}' ({e.Guild.Id}).");
            });
            
        }

        private async Task GuildBanAdded(DiscordClient c, GuildBanAddEventArgs e)
        {
            Task.Run(async () =>
            {
                await logService.GuildLogger(c, e.Guild, e, LogType.Ban).ConfigureAwait(true);
                this.logger.Information($"'{e.Member.GetUsertag()}' ({e.Member.Id}) has been banned from '{e.Guild.Name}' ({e.Guild.Id}).");
            });
        }

        private async Task GuildBanRemoved(DiscordClient c, GuildBanRemoveEventArgs e)
        {
            Task.Run(async () =>
            {
                await logService.GuildLogger(c, e.Guild, e, LogType.Ban).ConfigureAwait(true);
                this.logger.Information($"'{e.Member.GetUsertag()}' ({e.Member.Id}) has been unbanned from '{e.Guild.Name}' ({e.Guild.Id}).");
            });
        }

        private async Task InviteCreated(DiscordClient c, InviteCreateEventArgs e)
        {
            Task.Run(async () =>
            {
                await logService.GuildLogger(c, e.Guild, e, LogType.Invite).ConfigureAwait(true);
                this.logger.Information($"Invite code '{e.Invite.Code}' has been created by '{e.Invite.Inviter.GetUsertag()}' ({e.Invite.Inviter.Id}) for '{e.Guild.Name}' ({e.Guild.Id}).");
            });
        }

        private async Task InviteDeleted(DiscordClient c, InviteDeleteEventArgs e)
        {
            Task.Run(async () =>
            {
                await logService.GuildLogger(c, e.Guild, e, LogType.Invite).ConfigureAwait(true);
                this.logger.Information($"Invite code '{e.Invite.Code}' has been deleted by '{e.Invite.Inviter.GetUsertag()}' ({e.Invite.Inviter.Id}) on '{e.Guild.Name}' ({e.Guild.Id}).");
            });
        }

        private async Task DmChannelCreated(DiscordClient c, DmChannelCreateEventArgs e)
        {
            Task.Run(async () =>
            {
                var user = e.Channel.Recipients.First(x => !x.IsBot);
                this.logger.Information($"Direct message with '{user.GetUsertag()}' ({user.Id}) has been created.");
            });
        }

        private async Task DmChannelDeleted(DiscordClient c, DmChannelDeleteEventArgs e)
        {
            Task.Run(async () =>
            {
                var user = e.Channel.Recipients.First(x => !x.IsBot);
                this.logger.Information($"Direct message with '{user.GetUsertag()}' ({user.Id}) has been deleted.");
            });
        }

        private async Task MessageUpdated(DiscordClient c, MessageUpdateEventArgs e)
        {
            Task.Run(async () =>
            {
                if (e.Channel.IsPrivate)
                {
                    this.logger.Information($"The message ({e.Message.Id}) from '{e.Author.GetUsertag()}' ({e.Author.Id}) was updated in the direct message.");
                }
                else
                {
                    if (e.MessageBefore is { } && e.Author != null && !e.Author.IsBot && !e.MessageBefore.Content.Equals(e.Message.Content))
                    {
                        await logService.GuildLogger(c, e.Guild, e, LogType.Message).ConfigureAwait(true);
                        this.logger.Information($"The message ({e.Message.Id}) from '{e.Author.GetUsertag()}' ({e.Author.Id}) was updated in '{e.Channel.Name}' ({e.Channel.Id}) on '{e.Guild.Name}' ({e.Guild.Id}).");
                    }
                }
            });
        }

        private async Task MessageDeleted(DiscordClient c, MessageDeleteEventArgs e)
        {
            Task.Run(async () =>
            {
                if (e.Channel.IsPrivate)
                {
                    this.logger.Information(!string.IsNullOrWhiteSpace(e.Message.Content)
                        ? $"The message ({e.Message.Id}) from '{e.Message.Author.GetUsertag()}' ({e.Message.Author.Id}) was deleted in the direct message."
                        : $"The message ({e.Message.Id}) was deleted in the direct message.");
                }
                else
                {
                    var guildData = await redis.GetAsync<Guild>(RedisKeyNaming.Guild(e.Guild.Id)).ConfigureAwait(true);
                    if (!e.Message.Author.IsBot && !e.Message.Content.StartsWith(guildData.Prefix))
                    {
                        await logService.GuildLogger(c, e.Guild, e, LogType.Message).ConfigureAwait(true);
                    }
                }
            });
        }

        private async Task MessagesBulkDeleted(DiscordClient c, MessageBulkDeleteEventArgs e)
        {
            Task.Run(async () =>
            {
                if (!e.Channel.IsPrivate)
                {
                    await logService.GuildLogger(c, e.Guild, e, LogType.Message).ConfigureAwait(true);
                }
            });
        }

        private async Task MessageReactionAdded(DiscordClient c, MessageReactionAddEventArgs e)
        {
            Task.Run(async () =>
            {
                if (!e.User.IsBot)
            {
                var emojiName = e.Emoji.Name == "??" ? "an unknown reaction" : $"the reaction '{e.Emoji.Name}' ({e.Emoji.Id})";

                if (e.Channel.IsPrivate)
                {
                    this.logger.Information($"{e.User.GetUsertag()} ({e.User.Id}) has added {emojiName} to the message '{e.Message.Id}' in the direct message.");
                }
                else
                {
                    this.logger.Information($"'{e.User.GetUsertag()}' ({e.User.Id}) has added {emojiName} to the message '{e.Message.Id}' in the channel '{e.Channel.Name}' ({e.Channel.Id}) on the guild '{e.Guild.Name}' ({e.Guild.Id}).");

                    var member = e.Channel.Guild.GetMemberAsync(e.User.Id).Result;

                    if (reactionService.IsListener(e.Guild.Id, e.Message.Id, e.Emoji))
                    {
                        reactionService.ManageRole(e.Message, e.Channel, member, e.Emoji);
                    }
                }
            }
            });
        }

        private async Task MessageReactionRemoved(DiscordClient c, MessageReactionRemoveEventArgs e)
        {
            Task.Run(async () =>
            {
                if (!e.User.IsBot)
                {
                    var emojiName = e.Emoji.Name == "??" ? "an unknown reaction" : $"the reaction '{e.Emoji.Name}' ({e.Emoji.Id})";

                    if (e.Channel.IsPrivate)
                    {
                        this.logger.Information($"{e.User.GetUsertag()} ({e.User.Id}) has removed {emojiName} to the message '{e.Message.Id}' in the direct message.");
                        await Task.CompletedTask.ConfigureAwait(true);
                    }

                    this.logger.Information($"'{e.User.GetUsertag()}' ({e.User.Id}) has removed {emojiName} to the message '{e.Message.Id}' in the channel '{e.Channel.Name}' ({e.Channel.Id}) on the guild '{e.Guild.Name}' ({e.Guild.Id}).");
                }
            });
        }

        private async Task MessageReactionRemovedEmoji(DiscordClient c, MessageReactionRemoveEmojiEventArgs e)
        {
            this.logger.Information($"All reactions with the emoji '{e.Emoji.Name}' ({e.Emoji.Id}) of the message ({e.Message.Id}) has been removed in the channel '{e.Channel.Name}' ({e.Channel.Id}) on the guild '{e.Guild.Name}' ({e.Guild.Id}).");
        }

        private async Task MessageReactionsCleared(DiscordClient c, MessageReactionsClearEventArgs e)
        {
            this.logger.Information($"All reactions of the message ({e.Message.Id}) has been cleared in the channel '{e.Channel.Name}' ({e.Channel.Id}) on the guild '{e.Guild.Name}' ({e.Guild.Id}).");
        }

        private async Task VoiceServerUpdated(DiscordClient c, VoiceServerUpdateEventArgs e)
        {
            this.logger.Information($"Voice server has been updated to '{e.Endpoint}' on the guild '{e.Guild.Name}' ({e.Guild.Id}).");
        }

        private async Task VoiceStateUpdated(DiscordClient c, VoiceStateUpdateEventArgs e)
        {
            Task.Run(async () =>
            {
                await logService.GuildLogger(c, e.Guild, e, LogType.Voice).ConfigureAwait(true);
            });
        }

        private async Task SocketOpened(DiscordClient c, SocketEventArgs e)
        {
            this.logger.Information("Socket has been opened.");
        }

        private async Task SocketClosed(DiscordClient c, SocketCloseEventArgs e)
        {
            this.logger.Warning($"Socket has been closed. [{e.CloseCode}] ({e.CloseMessage})");
        }

        private async Task SocketErrored(DiscordClient c, SocketErrorEventArgs e)
        {
            this.logger.Error($"Socket has been errored! ({e.Exception.Message})");
        }

        private async Task WebhooksUpdated(DiscordClient c, WebhooksUpdateEventArgs e)
        {
            this.logger.Information($"A webhook has been updated in the channel '{e.Channel.Name}' ({e.Channel.Id}) on the guild '{e.Guild.Name}' ({e.Guild.Id}).");
        }

        private async Task ClientErrored(DiscordClient c, ClientErrorEventArgs e)
        {
            switch (e.Exception)
            {
                case BadRequestException badRequestException:
                    this.logger.Error($"Bad Request: {badRequestException.Message}");
                    break;
                case RateLimitException rateLimitException:
                    this.logger.Error($"Rate Limit: {rateLimitException.Message}");
                    break;
                case UnauthorizedException unauthorizedException:
                    this.logger.Error($"Unauthorized: {unauthorizedException.Message}");
                    break;
                case NotFoundException notFoundException:
                    this.logger.Error($"Not Found: {notFoundException.Message}");
                    break;
                case AggregateException aggregateException:
                    this.logger.Error($"Aggregate: {aggregateException.Message}");
                    break;
                case ServerErrorException serverErrorException:
                    this.logger.Error($"Server Error: {serverErrorException.Message}");
                    break;
                case CommandNotFoundException commandNotFoundException:
                    this.logger.Error($"Command not found: {commandNotFoundException.Message}");
                    break;
                case ChecksFailedException checksFailedException:
                    this.logger.Error($"Checks Failed: {checksFailedException.Message}");
                    break;
                case DuplicateCommandException duplicateCommandException:
                    this.logger.Error($"Duplicate Command: {duplicateCommandException.Message}");
                    break;
                case DuplicateOverloadException duplicateOverloadException:
                    this.logger.Error($"Duplicate overload: {duplicateOverloadException.Message}");
                    break;
                case InvalidOverloadException invalidOverloadException:
                    this.logger.Error($"Invalid overload: {invalidOverloadException.Message}");
                    break;
            }

            this.logger.Error(e.Exception, $"Client has occurred an error: {e.EventName}");
        }

        private async Task UnknownEvent(DiscordClient c, UnknownEventArgs e)
        {
            this.logger.Warning($"Unknown Event: {e.EventName}");
        }
    }
}