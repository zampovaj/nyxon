// common
global using System.ComponentModel.DataAnnotations;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using System.Text.Json;
global using System.Text;
global using System.Security.Claims;
global using System.Threading.Tasks;
global using Microsoft.AspNetCore.Authorization;

// Nyxon.Server
global using Nyxon.Server.Data;
global using Nyxon.Server.Models;
global using Nyxon.Server.Services.Auth;
global using Nyxon.Server.Extensions;
global using Nyxon.Server.Services.Cache;
global using Nyxon.Server.Services.Vault;
global using Nyxon.Server.Interfaces;
global using Nyxon.Server.Services.Messaging;
global using Nyxon.Server.Models.Valkey;
global using Nyxon.Server.Hubs;
global using Nyxon.Server.Services.Users;

// Nyxon.Core
global using Nyxon.Core.DTOs;
global using Nyxon.Core.Interfaces;
global using Nyxon.Core.Version;
global using Nyxon.Core.Models;
global using Nyxon.Core.Services.Hash;// Nyxon.Core
global using Nyxon.Core.DTOs;
global using Nyxon.Core.Interfaces;
global using Nyxon.Core.Version;
global using Nyxon.Core.Interfaces.Crypto;
global using Nyxon.Core.Crypto;
global using Nyxon.Core.Models;
global using Nyxon.Core.Services;
global using Nyxon.Core.Extensions;
global using Nyxon.Core.Services.Keys;
global using Nyxon.Core.Services.Hash;
global using Nyxon.Core.Models.Vaults;
global using RatchetType = Nyxon.Core.DTOs.CreateSnapshotDto.RatchetType;
global using Nyxon.Core.Constants;