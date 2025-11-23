using CoinFlipGame.ImageUploader.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CoinFlipGame.ImageUploader;

class Program
{
    static async Task<int> Main(string[] args)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        // Setup DI container
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<ImageUploadService>();
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        var serviceProvider = services.BuildServiceProvider();

        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        var uploadService = serviceProvider.GetRequiredService<ImageUploadService>();

        try
        {
            Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║         Coin Flip Game - Image Uploader                   ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            // Parse command line arguments
            if (args.Length > 0)
            {
                var command = args[0].ToLowerInvariant();

                switch (command)
                {
                    case "upload":
                        await UploadImagesAsync(configuration, uploadService, logger);
                        break;

                    case "list":
                        await ListBlobsAsync(uploadService, logger);
                        break;

                    case "clear":
                        await ClearContainerAsync(uploadService, logger);
                        break;

                    case "help":
                    default:
                        ShowHelp();
                        break;
                }
            }
            else
            {
                // No arguments - run interactive mode
                await InteractiveModeAsync(configuration, uploadService, logger);
            }

            return 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred");
            return 1;
        }
    }

    static async Task InteractiveModeAsync(IConfiguration configuration, ImageUploadService uploadService, ILogger<Program> logger)
    {
        while (true)
        {
            Console.WriteLine("\nWhat would you like to do?");
            Console.WriteLine("  1. Upload and compress coin images");
            Console.WriteLine("  2. List uploaded blobs");
            Console.WriteLine("  3. Clear all blobs (CAUTION!)");
            Console.WriteLine("  4. Exit");
            Console.Write("\nYour choice: ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await UploadImagesAsync(configuration, uploadService, logger);
                    break;

                case "2":
                    await ListBlobsAsync(uploadService, logger);
                    break;

                case "3":
                    await ClearContainerAsync(uploadService, logger);
                    break;

                case "4":
                    Console.WriteLine("Goodbye!");
                    return;

                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }
    }

    static async Task UploadImagesAsync(IConfiguration configuration, ImageUploadService uploadService, ILogger<Program> logger)
    {
        var sourceDir = configuration["SourcePaths:CoinImagesDirectory"];
        
        if (string.IsNullOrEmpty(sourceDir))
        {
            logger.LogError("Source directory not configured in appsettings.json");
            return;
        }

        // Make path absolute if it's relative
        if (!Path.IsPathRooted(sourceDir))
        {
            sourceDir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), sourceDir));
        }

        Console.WriteLine($"\nSource directory: {sourceDir}");
        Console.WriteLine("\nThis will:");
        Console.WriteLine("  - Resize images to 400x400 (if larger)");
        Console.WriteLine("  - Compress images");
        Console.WriteLine("  - Upload to Azure Blob Storage");
        Console.WriteLine("  - Maintain directory structure");
        Console.WriteLine("\nContinue? (y/n): ");
        
        var confirm = Console.ReadLine()?.ToLowerInvariant();
        if (confirm != "y" && confirm != "yes")
        {
            Console.WriteLine("Upload cancelled.");
            return;
        }

        Console.WriteLine("\nStarting upload process...\n");

        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
            logger.LogWarning("Cancellation requested...");
        };

        var (success, failed) = await uploadService.ProcessDirectoryAsync(sourceDir, cts.Token);

        Console.WriteLine("\n" + new string('═', 60));
        Console.WriteLine($"Upload complete!");
        Console.WriteLine($"  ✓ Successful: {success}");
        Console.WriteLine($"  ✗ Failed: {failed}");
        Console.WriteLine(new string('═', 60));
    }

    static async Task ListBlobsAsync(ImageUploadService uploadService, ILogger<Program> logger)
    {
        Console.WriteLine("\nFetching blob list...\n");

        var blobs = await uploadService.ListAllBlobsAsync();

        if (blobs.Count == 0)
        {
            Console.WriteLine("No blobs found in container.");
            return;
        }

        Console.WriteLine($"Found {blobs.Count} blobs:\n");

        // Group by directory for better readability
        var grouped = blobs.GroupBy(b => Path.GetDirectoryName(b) ?? "root")
            .OrderBy(g => g.Key);

        foreach (var group in grouped)
        {
            Console.WriteLine($"\n📁 {group.Key}/");
            foreach (var blob in group.OrderBy(b => b))
            {
                Console.WriteLine($"   └─ {Path.GetFileName(blob)}");
            }
        }
    }

    static async Task ClearContainerAsync(ImageUploadService uploadService, ILogger<Program> logger)
    {
        Console.WriteLine("\n⚠️  WARNING: This will delete ALL blobs in the container!");
        Console.Write("Type 'DELETE' to confirm: ");
        
        var confirm = Console.ReadLine();
        if (confirm != "DELETE")
        {
            Console.WriteLine("Clear cancelled.");
            return;
        }

        Console.WriteLine("\nDeleting all blobs...\n");

        var deletedCount = await uploadService.ClearContainerAsync();

        Console.WriteLine($"\n✓ Deleted {deletedCount} blobs.");
    }

    static void ShowHelp()
    {
        Console.WriteLine("Usage: CoinFlipGame.ImageUploader [command]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  upload    Upload and compress coin images from the configured directory");
        Console.WriteLine("  list      List all blobs in the container");
        Console.WriteLine("  clear     Delete all blobs from the container (use with caution!)");
        Console.WriteLine("  help      Show this help message");
        Console.WriteLine();
        Console.WriteLine("If no command is provided, the tool will run in interactive mode.");
    }
}
