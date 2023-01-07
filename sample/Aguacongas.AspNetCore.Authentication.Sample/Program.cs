// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2021 @Olivier Lefebvre
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// DI setup
builder.AddSample();

var app = builder.Build();

// Midleware pipeline setup
app.UseSample(builder.Environment);

app.Run();