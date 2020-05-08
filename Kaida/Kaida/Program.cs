using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using ImageMagick;
using Kaida.Handler;
using Kaida.Library.Formatters;
using Kaida.Library.Logger;
using Kaida.Library.Reaction;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using StackExchange.Redis;
using System.Diagnostics;

namespace Kaida
{
    internal class Program
    {
        private string[] args;
        private DiscordShardedClient client;
        private ClientEventHandler clientEventHandler;
        private CommandEventHandler commandEventHandler;
        private CommandsNextExtension commandsNext;
        private int database;
        private ILogger logger;
        private IReactionListener reactionListener;
        private IDatabase redis;
        private IServiceProvider serviceProvider;
        private IServiceCollection services;
        private RedisValue token;

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
            bool validInput;
            bool validRedisInstance;

            logger.Information("Initializing the Redis this.database setup...");
            logger.Warning("The application does not check if the redis database instance is already in use or not!");

            do
            {
                Console.Write("Please enter your Redis database instance [0-15]: ");
                var input = Console.ReadLine();
                validInput = int.TryParse(input, out database);

                if (!validInput)
                {
                    logger.Error("Input is not an integer.");
                }

                if (database > 15 || database < 0)
                {
                    logger.Error("Not a valid database instance. Redis supports only zero (0) till fifteen (15).");
                    validRedisInstance = false;
                }
                else
                {
                    logger.Information("Valid Redis this.database instance.");
                    validRedisInstance = true;
                }
            } while (validInput == false || validRedisInstance == false);

            try
            {
                logger.Information("Connecting to Redis this.database '127.0.0.1:6379'...");
                var redisMultiplexer = await ConnectionMultiplexer.ConnectAsync("127.0.0.1:6379");
                redis = redisMultiplexer.GetDatabase(database);
                logger.Information($"Successfully connected to database '{redis.Database}'.");
            }
            catch (Exception e)
            {
                logger.Fatal(e, "Redis connection failed.");
                Environment.Exit(102);
            }

            token = redis.StringGet("discordToken");

            if (token.IsNullOrEmpty)
            {
                logger.Information("Initializing the bot token setup...");
                Console.Write("Your bot token: ");
                var newToken = Console.ReadLine();
                redis.StringSet("discordToken", newToken);
                token = newToken;
                logger.Information("Successfully set the bot token into the database.");
            }
            else
            {
                logger.Information("Discord token is already set and will be used from the database.");
            }

            logger.Information("Initializing the services setup...");
            reactionListener = new ReactionListener(logger, redis);

            services = new ServiceCollection().AddSingleton(redis)
                                              .AddSingleton(logger)
                                              .AddSingleton(reactionListener);

            serviceProvider = services.BuildServiceProvider();
            logger.Information("Successfully setup the services.");
            OpenCL.IsEnabled = false;
            logger.Information("Disabled GPU acceleration.");
            await Task.CompletedTask.ConfigureAwait(true);
        }

        private async Task Login()
        {
            logger.Information("Initializing the client setup...");
            var activity = new DiscordActivity("ur mom is pretty gae", ActivityType.Custom);

            client = new DiscordShardedClient(new DiscordConfiguration
            {
                Token = token,
                TokenType = TokenType.Bot,
                DateTimeFormat = "dd-MM-yyyy HH:mm",
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
                EnableMentionPrefix = false,
                EnableDms = true,
                DmHelp = true,
                EnableDefaultHelp = true
            };

            logger.Information("Commands configuration setup done.");

            var icfg = new InteractivityConfiguration {PollBehaviour = PollBehaviour.KeepEmojis, Timeout = TimeSpan.FromMinutes(2)};

            logger.Information("Interactivity configuration setup done.");
            logger.Information("Connecting all shards...");
            await client.StartAsync()
                        .ConfigureAwait(true);
            await client.UpdateStatusAsync(activity, UserStatus.Online)
                        .ConfigureAwait(true);
            logger.Information("Setting up client event handler...");
            clientEventHandler = new ClientEventHandler(client, logger, redis, reactionListener);

            foreach (var shard in client.ShardClients.Values)
            {
                logger.Information($"Applying configs to shard {shard.ShardId}...");
                commandsNext = shard.UseCommandsNext(ccfg);
                commandsNext.RegisterCommands(Assembly.GetEntryAssembly());
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
            if (msg.Channel.IsPrivate) return Task.FromResult(msg.GetStringPrefixLength("$"));

            var prefix = redis.StringGet($"{msg.Channel.Guild.Id}:CommandPrefix");

            if (!string.IsNullOrWhiteSpace(prefix))
            {
                return Task.FromResult(msg.GetStringPrefixLength(prefix));
            }

            redis.StringSet($"{msg.Channel.Guild.Id}:CommandPrefix", "$");

            return Task.FromResult(msg.GetStringPrefixLength("$"));
        }
    }
}