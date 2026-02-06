@echo off
cd /d "%~dp0"

echo ==========================================
echo FIRST-TIME SYNC WITH GITHUB REPO
echo ==========================================
echo This script handles the GUI-created repo with LICENSE/README

:: 1. Pull down the LICENSE and README that GitHub created
echo Pulling existing files from GitHub...
git pull origin main --allow-unrelated-histories --no-edit

:: 2. Stage your local code
echo Adding your local files...
git add .

:: 3. Commit everything together
echo Committing merged content...
git commit -m "Initial commit: merged with GitHub repo files"

:: 4. Push everything up
echo Pushing to GitHub...
git push origin main

echo.
echo ==========================================
echo SUCCESS! Your repo is now synced.
echo Check: https://github.com/Gentlemann-Of-Hats/PlannedEvolution
echo ==========================================
pause
