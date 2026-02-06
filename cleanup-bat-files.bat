@echo off
cd /d "%~dp0"

echo ==========================================
echo CLEANING BAT FILES FROM REPO
echo ==========================================

:: 1. Remove .bat files from Git tracking (keeps them on your PC)
echo Removing batch files from Git...
git rm --cached *.bat

:: 2. Remove any .zip files too
echo Removing zip files from Git...
git rm --cached *.zip 2>nul

:: 3. Stage the removal
echo Staging changes...
git add .

:: 4. Commit the cleanup
echo Committing cleanup...
git commit -m "Remove batch scripts and zip files from repo"

:: 5. Push the changes
echo Pushing to GitHub...
git push origin main

echo.
echo ==========================================
echo SUCCESS! Bat files removed from repo.
echo They still exist on your PC, just not in Git.
echo ==========================================
pause
