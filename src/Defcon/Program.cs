using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Defcon.Core;
using Defcon.Data.Configuration;
using Defcon.Data.Guilds;
using Defcon.Handler.Client;
using Defcon.Handler.Command;
using Defcon.Library.Extensions;
using Defcon.Library.Formatters;
using Defcon.Library.Logger;
using Defcon.Library.Redis;
using Defcon.Library.Services.Infractions;
using Defcon.Library.Services.Logs;
using Defcon.Library.Services.Reactions;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using ImageMagick;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Newtonsoft;
using SteamCSharp;

namespace Defcon
{
    internal class Program
    {
        private string[] args;
        private DiscordShardedClient client;
        private ClientEventHandler clientEventHandler;
        private CommandEventHandler commandEventHandler;
        private CommandsNextExtension commandsNext;
        private ILogger logger;
        private IReactionService reactionService;
        private IInfractionService infractionService;
        private ILogService logService;
        private IRedisDatabase redis;
        private IServiceProvider serviceProvider;
        private IServiceCollection services;
        private Config config;
        private Steam steam;

        public static void Main(string[] args)
        {
            var logger = LoggerFactory.CreateLogger();
            logger.Information("Starting...");

            try
            {
                new Program().Start(args, logger)
                             .ConfigureAwait(false)
                             .GetAwaiter()
                             .GetResult();
            }
            catch (Exception e)
            {
                logger.Fatal(e, "Rest in peace!");
                Process.Start(AppDomain.CurrentDomain.FriendlyName);
                Environment.Exit(1);
            }
        }

        private async Task Start(string[] args, ILogger logger)
        {
            this.args = args;
            this.logger = logger;
            this.logger.Information("Initializing the setup...");
            await Setup()
               .ConfigureAwait(true);
            this.logger.Information("Initializing the login...");
            await Login()
               .ConfigureAwait(true);
            await Task.Delay(Timeout.Infinite)
                      .ConfigureAwait(true);
        }

        private async Task Setup()
        {
            var redisSetup = new Setup(logger);

            logger.Information("Initializing the services setup...");
            steam = new Steam("");
            services = new ServiceCollection().AddSingleton(logger)
                                              .AddSingleton(steam)
                                              .AddStackExchangeRedisExtensions<NewtonsoftSerializer>(redisSetup.RedisConfiguration);

            serviceProvider = services.BuildServiceProvider();
            redis = serviceProvider.GetService<IRedisDatabase>();

            reactionService = new ReactionService(logger, redis);
            infractionService = new InfractionService(logger, redis);
            logService = new LogService(logger, redis);
            services.AddSingleton(reactionService)
                    .AddSingleton(infractionService)
                    .AddSingleton(logService);
            serviceProvider = services.BuildServiceProvider();
            logger.Information("Successfully setup the services.");

            config = await redis.GetAsync<Config>(RedisKeyNaming.Config);

            if (config == null || string.IsNullOrWhiteSpace(config.Token))
            {
                logger.Information("Initializing the bot token setup...");
                Console.Write("Your bot token: ");
                var token = Console.ReadLine();

                if (config == null)
                {
                    config = new Config()
                    {
                        Token = token
                    };
                    await redis.AddAsync<Config>(RedisKeyNaming.Config, config);
                }
                else if (string.IsNullOrWhiteSpace(config.Token))
                {
                    config.Token = token;
                    await redis.ReplaceAsync<Config>(RedisKeyNaming.Config, config);
                }

                logger.Information("Successfully set the bot token into the database.");
            }
            else
            {
                logger.Information("Discord token is already set and will be used from the database.");
            }

            OpenCL.IsEnabled = false;
            logger.Information("Disabled GPU acceleration.");
            await Task.CompletedTask.ConfigureAwait(true);
        }

        private async Task Login()
        {
            logger.Information("Initializing the client setup...");

            client = new DiscordShardedClient(new DiscordConfiguration
            {
                Token = config.Token,
                TokenType = TokenType.Bot,
                LogTimestampFormat = "dd-MM-yyyy HH:mm",
                AutoReconnect = true,
                MessageCacheSize = 4096,
                ReconnectIndefinitely = true,
                HttpTimeout = Timeout.InfiniteTimeSpan,
                GatewayCompressionLevel = GatewayCompressionLevel.Payload
            });

            logger.Information("Successfully setup the client.");
            logger.Information("Setting up all configurations...");

            var ccfg = new CommandsNextConfiguration
            {
                Services = serviceProvider,
                PrefixResolver = PrefixResolverAsync,
                EnableMentionPrefix = true,
                EnableDms = true,
                DmHelp = false,
                EnableDefaultHelp = true,
                UseDefaultCommandHandler = true
            };

            logger.Information("Commands configuration setup done.");

            var icfg = new InteractivityConfiguration { PollBehaviour = PollBehaviour.KeepEmojis, Timeout = TimeSpan.FromMinutes(2) };

            logger.Information("Interactivity configuration setup done.");
            logger.Information("Connecting all shards...");
            await client.StartAsync()
                        .ConfigureAwait(true);
            logger.Information("Setting up client event handler...");
            clientEventHandler = new ClientEventHandler(client, logger, redis, reactionService, logService);

            foreach (var shard in client.ShardClients.Values)
            {
                logger.Information($"Applying configs to shard {shard.ShardId}...");
                commandsNext = shard.UseCommandsNext(ccfg);
                commandsNext.RegisterCommands(Assembly.GetExecutingAssembly());
                commandsNext.SetHelpFormatter<HelpFormatter>();
                shard.UseInteractivity(icfg);
                logger.Information($"Settings up command event handler for the shard {shard.ShardId}...");
                commandEventHandler = new CommandEventHandler(commandsNext, logger);
                logger.Information($"Setup for shard {shard.ShardId} done.");
                await shard.InitializeAsync();
            }

            foreach (var cNextRegisteredCommand in commandsNext.RegisteredCommands)
            {
                logger.Information($"{cNextRegisteredCommand.Value} is registered!");
            }
        }

        public Task<int> PrefixResolverAsync(DiscordMessage msg)
        {
            if (msg.Channel.IsPrivate) return Task.FromResult(msg.GetStringPrefixLength(ApplicationInformation.DefaultPrefix));

            var guild = redis.GetAsync<Guild>(RedisKeyNaming.Guild(msg.Channel.GuildId))
                             .GetAwaiter()
                             .GetResult();

            var prefix = guild.Prefix;

            if (!string.IsNullOrWhiteSpace(prefix))
            {
                return Task.FromResult(msg.GetStringPrefixLength(prefix));
            }

            guild.Prefix = ApplicationInformation.DefaultPrefix;
            redis.ReplaceAsync<Guild>(RedisKeyNaming.Guild(msg.Channel.GuildId), guild);

            return Task.FromResult(msg.GetStringPrefixLength(ApplicationInformation.DefaultPrefix));
        }
    }
}