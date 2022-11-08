ProductCatalogDaemon.Main main = new ProductCatalogDaemon.Main();

try
{
    main.RunAsync().GetAwaiter().GetResult();
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(ex.Message);
    Console.ResetColor();
}
