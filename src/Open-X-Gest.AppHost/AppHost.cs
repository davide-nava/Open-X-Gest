var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.OpenX_Gest_Api>("openx-gest-api");

builder.Build().Run();
