using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using ImageMagick;
using Kaida.Handler;
using Kaida.Library.Formatter;
using Kaida.Library.Logger;
using Kaida.Library.Reaction;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using StackExchange.Redis;

namespace Kaida
{
    internal class Program
    {
        private string[] _args;
        private DiscordShardedClient _client;
        private ClientEventHandler _clientEventHandler;
        private CommandsNextExtension _cnext;
        private CommandEventHandler _commandEventHandler;
        private int _database;
        private ILogger _logger;
        private IReactionListener _reactionListener;
        private IDatabase _redis;
        private IServiceProvider _serviceProvider;
        private IServiceCollection _services;
        private RedisValue _token;

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
                System.Diagnostics.Process.Start(System.AppDomain.CurrentDomain.FriendlyName);
                System.Environment.Exit(1);
            }
        }

        private async Task Start(string[] args, ILogger logger)
        {
            _args = args;
            _logger = logger;
            _logger.Information("Initializing the setup...");
            await Setup().ConfigureAwait(true);
            _logger.Information("Initializing the login...");
            await Login().ConfigureAwait(true);
            await Task.Delay(Timeout.Infinite).ConfigureAwait(true);
        }

        private async Task Setup()
        {
            bool validInput;
            bool validRedisInstance;

            _logger.Information("Initializing the Redis database setup...");
            _logger.Warning("The application does not check if the redis database instance is already in use or not!");

            do
            {
                Console.Write("Please enter your Redis database instance [0-15]: ");
                var input = Console.ReadLine();
                validInput = int.TryParse(input, out _database);

                if (!validInput)
                {
                    _logger.Error("Input is not an integer.");
                }

                if (_database > 15 || _database < 0)
                {
                    _logger.Error("Not a valid database instance. Redis supports only zero (0) till fifteen (15).");
                    validRedisInstance = false;
                }
                else
                {
                    _logger.Information("Valid Redis database instance.");
                    validRedisInstance = true;
                }
            } while (validInput == false || validRedisInstance == false);

            try
            {
                _logger.Information("Connecting to Redis database '127.0.0.1:6379'...");
                var redisMultiplexer = ConnectionMultiplexer.Connect("127.0.0.1:6379");
                _redis = redisMultiplexer.GetDatabase(_database);
                _logger.Information($"Successfully connected to database '{_redis.Database}'.");
            }
            catch (Exception e)
            {
                _logger.Fatal(e, "Redis connection failed.");
                Environment.Exit(102);
            }

            _token = _redis.StringGet("discordToken");

            if (_token.IsNullOrEmpty)
            {
                _logger.Information("Initializing the bot token setup...");
                Console.Write("Your bot token: ");
                var newToken = Console.ReadLine();
                _redis.StringSet("discordToken", newToken);
                _token = newToken;
                _logger.Information("Successfully set the bot token into the database.");
            }
            else
            {
                _logger.Information("Discord token is already set and will be used from the database.");
            }

            _logger.Information("Initializing the services setup...");
            _reactionListener = new ReactionListener(_logger, _redis);

            _services = new ServiceCollection()
                .AddSingleton(_redis)
                .AddSingleton(_logger)
                .AddSingleton(_reactionListener);

            _serviceProvider = _services.BuildServiceProvider();
            _logger.Information("Successfully setup the services.");
            OpenCL.IsEnabled = false;
            _logger.Information("Disabled GPU acceleration.");
            await Task.CompletedTask.ConfigureAwait(true);
        }

        private async Task Login()
        {
            _logger.Information("Initializing the client setup...");
            var activity = new DiscordActivity("ur mom is pretty gae", ActivityType.Custom);

            _client = new DiscordShardedClient(new DiscordConfiguration
            {
                Token = _token,
                TokenType = TokenType.Bot,
                DateTimeFormat = "dd-MM-yyyy HH:mm",
                AutoReconnect = true,
                MessageCacheSize = 4086,
                ReconnectIndefinitely = true,
                HttpTimeout = Timeout.InfiniteTimeSpan,
                GatewayCompressionLevel = GatewayCompressionLevel.Payload
            });

            _logger.Information("Successfully setup the client.");
            _logger.Information("Setting up all configurations...");

            var ccfg = new CommandsNextConfiguration
            {
                Services = _serviceProvider,
                PrefixResolver = PrefixResolverAsync,
                EnableMentionPrefix = false,
                EnableDms = true,
                DmHelp = true,
                EnableDefaultHelp = true
            };

            _logger.Information("Commands configuration setup done.");

            var icfg = new InteractivityConfiguration
            {
                PollBehaviour = DSharpPlus.Interactivity.Enums.PollBehaviour.KeepEmojis,
                Timeout = TimeSpan.FromMinutes(2)
            };

            _logger.Information("Interactivity configuration setup done.");
            _logger.Information("Connecting all shards...");
            await _client.StartAsync().ConfigureAwait(true);
            await _client.UpdateStatusAsync(activity, UserStatus.Online).ConfigureAwait(true);
            _logger.Information("Setting up client event handler...");
            _clientEventHandler = new ClientEventHandler(_client, _logger, _redis, _reactionListener);

            foreach (var shard in _client.ShardClients.Values)
            {
                _logger.Information($"Applying configs to shard {shard.ShardId}...");
                _cnext = shard.UseCommandsNext(ccfg);
                _cnext.RegisterCommands(Assembly.GetEntryAssembly());
                shard.UseInteractivity(icfg);
                _logger.Information($"Settings up command event handler for the shard {shard.ShardId}...");
                _commandEventHandler = new CommandEventHandler(_cnext, _logger);
                _logger.Information($"Setup for shard {shard.ShardId} done.");
                await shard.InitializeAsync();
            }

            foreach (var cnextRegisteredCommand in _cnext.RegisteredCommands)
            {
                _logger.Information($"{cnextRegisteredCommand.Value} is registered!");
            }
        }

        public Task<int> PrefixResolverAsync(DiscordMessage msg)
        {
            if (!msg.Channel.IsPrivate)
            {
                var prefix = _redis.StringGet($"{msg.Channel.Guild.Id}:CommandPrefix");
                if (!string.IsNullOrWhiteSpace(prefix))
                {
                    return Task.FromResult(msg.GetStringPrefixLength(prefix));
                }

                _redis.StringSet($"{msg.Channel.Guild.Id}:CommandPrefix", "$");
            }

            return Task.FromResult(msg.GetStringPrefixLength("$"));
        }
    }
}
