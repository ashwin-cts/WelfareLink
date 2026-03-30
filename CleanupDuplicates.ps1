# Run this script with PowerShell to clean up duplicate files
# Close Visual Studio first!

cd "C:\Users\2481305\OneDrive - Cognizant\Desktop\WelfareLinkGit"

Write-Host "Removing duplicate Views\Pages folder..." -ForegroundColor Yellow

if (Test-Path "WelfareLink\Views\Pages") {
    Remove-Item -Recurse -Force "WelfareLink\Views\Pages"
    Write-Host "Successfully removed Views\Pages folder!" -ForegroundColor Green
} else {
    Write-Host "Views\Pages folder not found." -ForegroundColor Red
}

Write-Host "`nNow open Visual Studio and build the project." -ForegroundColor Cyan
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
