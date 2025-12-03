// common
global using System.ComponentModel.DataAnnotations;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using System.Text.Json;
global using System.Text;

// backend
global using Backend.Data;
global using Backend.Models;
global using Backend.Services;
global using Backend.Services.Auth;
global using Backend.Extensions;
global using Backend.Services.Crypto;
global using Backend.Services.Cache;
global using Backend.Services.Vault;
global using Backend.Interfaces;

// shared
global using Shared.DTOs;
global using Shared.Interfaces;
global using Shared.Version;
global using Shared.Models.Valkey;