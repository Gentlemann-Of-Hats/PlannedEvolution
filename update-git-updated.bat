@echo off
cd /d "%~dp0"

echo ==========================================
echo UPDATING PLANNEDVOLUTION REPO
echo ==========================================

:: 1. Stage all changes
echo Adding files...
git add .

:: 2. Commit
set /p CommitMsg="Enter commit message: "
git commit -m "%CommitMsg%"

:: 3. Push to GitHub
echo Pushing to GitHub...
git push -u origin main

echo.
echo ==========================================
echo DONE! Check: https://github.com/Gentlemann-Of-Hats/PlannedEvolution
echo ==========================================
pause
