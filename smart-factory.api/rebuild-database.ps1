# Rebuild Database Script
# Stop any running processes first!

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Smart Factory - Rebuild Database Script  " -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Check if API is running
$apiProcess = Get-Process -Name "SmartFactory.Api" -ErrorAction SilentlyContinue
if ($apiProcess) {
    Write-Host "WARNING: SmartFactory.Api is running!" -ForegroundColor Red
    Write-Host "Please stop the API server first (Ctrl+C in terminal)" -ForegroundColor Yellow
    Write-Host ""
    $response = Read-Host "Do you want to force kill the process? (yes/no)"
    if ($response -eq "yes") {
        Stop-Process -Name "SmartFactory.Api" -Force
        Write-Host "API process killed." -ForegroundColor Green
        Start-Sleep -Seconds 2
    } else {
        Write-Host "Script cancelled." -ForegroundColor Yellow
        exit 1
    }
}

# Navigate to API directory
Set-Location -Path "$PSScriptRoot\SmartFactory.Api"

Write-Host "Step 1: Dropping database..." -ForegroundColor Yellow
dotnet ef database drop --force --project ../SmartFactory.Application

if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to drop database. Check connection string." -ForegroundColor Red
    exit 1
}

Write-Host "✓ Database dropped successfully" -ForegroundColor Green
Write-Host ""

Write-Host "Step 2: Applying all migrations..." -ForegroundColor Yellow
dotnet ef database update --project ../SmartFactory.Application

if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to apply migrations." -ForegroundColor Red
    exit 1
}

Write-Host "✓ Migrations applied successfully" -ForegroundColor Green
Write-Host ""

Write-Host "Step 3: Seeding initial data..." -ForegroundColor Yellow
Write-Host "(You may need to start the API once to trigger DbSeeder)" -ForegroundColor Cyan
Write-Host ""

Write-Host "============================================" -ForegroundColor Green
Write-Host "  Database rebuilt successfully!           " -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Start the API: dotnet run" -ForegroundColor White
Write-Host "2. Check logs for seed data confirmation" -ForegroundColor White
Write-Host ""






