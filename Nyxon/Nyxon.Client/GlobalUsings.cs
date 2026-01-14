// Nyxon.Core
global using Nyxon.Core.DTOs;
global using Nyxon.Core.Interfaces;
global using Nyxon.Core.Version;
global using Nyxon.Core.Models.Vaults;
global using Nyxon.Core.Models;
global using RatchetType = Nyxon.Core.DTOs.CreateSnapshotDto.RatchetType;

//frontend
global using Microsoft.AspNetCore.Components;
global using Microsoft.AspNetCore.SignalR.Client;
global using Microsoft.JSInterop;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Components.Authorization;
global using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
global using MudBlazor;

//client
global using Nyxon.Client.Services;
global using Nyxon.Client.Services.Hub;
global using Nyxon.Client.Services.Crypto;
global using Nyxon.Client.Services.Messaging;
global using Nyxon.Client.Models;
global using Nyxon.Client.ViewModels;
global using Nyxon.Client.Interfaces;
global using Nyxon.Client.Interfaces.Crypto;
global using Nyxon.Client.Components;
global using Nyxon.Client.Services.State;