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
        private bool _discordTokenAvailable;
        private ILogger _logger;
        private IReactionListener _reactionListener;
        private IDatabase _redis;
        private IServiceProvider _serviceProvider;
        private IServiceCollection _services;
        private RedisValue _token;
        private int _totalShards;

        private static void Main(string[] args)
        {
            var logger = LoggerFactory.CreateLogger();
            logger.Information("[Kaida] Starting...");

            try
            {
                new Program().Start(args, logger)
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception e)
            {
                logger.Fatal(e, "[Kaida] Rest in peace!");
            }
        }

        private async Task Start(string[] args, ILogger logger)
        {
            _args = args;
            _logger = logger;

            _logger.Information("[Kaida] Initializing the setup...");
            await Setup();

            _logger.Information("[Kaida] Initializing the login...");
            await Login();

            await Task.Delay(-1);
        }

        private async Task Setup()
        {
            bool validInput;
            bool validRedisInstance;

            do
            {
                _logger.Information("[Kaida] Initializing the Redis database setup...");
                _logger.Warning("[Kaida] The application does not check if the redis database instance is already in use or not!");
                Console.Write("Please enter your Redis database instance [0-15]: ");
                var input = Console.ReadLine();
                validInput = int.TryParse(input, out _database);

                if (!validInput)
                {
                    _logger.Error("[Kaida] Input is not an integer.");
                }

                if (_database > 15 || _database < 0)
                {
                    _logger.Error("[Kaida] Not a valid database instance. Redis supports only zero (0) till fifteen (15).");
                    validRedisInstance = false;
                }
                else
                {
                    _logger.Information("[Kaida] Valid Redis database instance.");
                    validRedisInstance = true;
                }
            } while (validInput == false || validRedisInstance == false);

            try
            {
                _logger.Information("[Kaida] Connecting to Redis database '127.0.0.1:6379'...");
                var redisMultiplexer = ConnectionMultiplexer.Connect("127.0.0.1:6379");
                _redis = redisMultiplexer.GetDatabase(_database);
                _logger.Information($"[Kaida] Successfully connected to database '{_redis.Database}'.");
            }
            catch (Exception e)
            {
                _logger.Fatal(e, "[Kaida] Redis connection failed.");
                Environment.Exit(102);
            }

            _token = _redis.StringGet("discordToken");

            if (_token.IsNullOrEmpty)
            {
                _logger.Information("[Kaida] Initializing the bot token setup...");
                Console.Write("Your bot token: ");
                var newToken = Console.ReadLine();
                _redis.StringSet("discordToken", newToken);
                _token = newToken;
                _discordTokenAvailable = true;
                _logger.Information("[Kaida] Successfully set the bot token into the database.");
            }
            
            Console.Write("Amount of sharded clients: ");
            var totalShards = Console.ReadLine();
            _totalShards = int.Parse(totalShards);

            _logger.Information("[Kaida] Initializing the services setup...");

            _reactionListener = new ReactionListener(_redis);
            _services = new ServiceCollection()
                .AddSingleton(_redis)
                .AddSingleton(_logger)
                .AddSingleton(_reactionListener);
            _serviceProvider = _services.BuildServiceProvider();
            _logger.Information("[Kaida] Successfully setup the services.");

            OpenCL.IsEnabled = false;
            _logger.Information("[Kaida] Disabled GPU acceleration.");

            await Task.CompletedTask;
        }

        private async Task Login()
        {
            _logger.Information("[Kaida] Initializing the client setup...");
            var activity = new DiscordActivity("OnlyOneCookie", ActivityType.Watching);
            _client = new DiscordShardedClient(new DiscordConfiguration
            {
                Token = _token,
                TokenType = TokenType.Bot,
                ShardCount = _totalShards,
                DateTimeFormat = "dd-MM-yyyy HH:mm"
            });
            _logger.Information("[Kaida] Successfully setup the client.");

            _logger.Information("[Kaida] Setting up all configurations...");
            var ccfg = new CommandsNextConfiguration
            {
                DmHelp = true,
                Services = _serviceProvider,
                PrefixResolver = PrefixResolverAsync,
                EnableMentionPrefix = true,
                CaseSensitive = false,
                EnableDms = true,
                UseDefaultCommandHandler = true,
                IgnoreExtraArguments = false
            };
            _logger.Information("[Kaida] Commands configuration setup done.");

            var icfg = new InteractivityConfiguration
            {
                PaginationBehavior = TimeoutBehaviour.DeleteReactions,
                PaginationTimeout = TimeSpan.FromMinutes(5),
                Timeout = TimeSpan.FromMinutes(5)
            };
            _logger.Information("[Kaida] Interactivity configuration setup done.");
            
            _logger.Information("[Kaida] Connecting all shards...");
            await _client.StartAsync();
            await _client.UpdateStatusAsync(activity, UserStatus.Online);

            _logger.Information("[Kaida] Setting up client event handler...");
            _clientEventHandler = new ClientEventHandler(_client, _logger, _reactionListener);
            

            foreach (var shard in _client.ShardClients.Values)
            {
                _logger.Information($"[Kaida] Applying configs to shard {shard.ShardId}...");
                _cnext = shard.UseCommandsNext(ccfg);
                _cnext.RegisterCommands(Assembly.GetEntryAssembly());
                shard.UseInteractivity(icfg);
                _logger.Information($"[Kaida] Settings up command event handler for the shard {shard.ShardId}...");
                _commandEventHandler = new CommandEventHandler(_cnext, _logger);
                _logger.Information($"[Kaida] Setup for shard {shard.ShardId} done.");
            }

            foreach (var cnextRegisteredCommand in _cnext.RegisteredCommands)
            {
                _logger.Information($"[Kaida] {cnextRegisteredCommand.Value} is registered!");
            }
            
            await Task.Delay(Timeout.Infinite);
        }

        public Task<int> PrefixResolverAsync(DiscordMessage msg)
        {
            var prefix = _redis.StringGet($"{msg.Channel.Guild.Id}:CommandPrefix");
            if (!prefix.IsNullOrEmpty) return Task.FromResult(msg.GetStringPrefixLength(prefix));
            _redis.StringSet($"{msg.Channel.Guild.Id}:CommandPrefix", Configuration.Prefix);
            return Task.FromResult(msg.GetStringPrefixLength(Configuration.Prefix));
        }
    }
}
