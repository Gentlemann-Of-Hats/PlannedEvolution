@echo off
cd /d "%~dp0"

echo ==========================================
echo INITIALIZING PLANNEDVOLUTION REPO
echo ==========================================

:: 1. Turn this folder into a Git Repo
git init

:: 2. Name the main branch 'main'
git branch -M main

:: 3. Connect it to your GitHub link
git remote add origin https://github.com/Gentlemann-Of-Hats/PlannedEvolution.git

echo.
echo ==========================================
echo DONE!
echo Now you can run 'update-git.bat' whenever you want.
echo ==========================================
pause
