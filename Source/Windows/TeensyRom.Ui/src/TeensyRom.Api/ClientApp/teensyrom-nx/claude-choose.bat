@echo off
setlocal enableextensions enabledelayedexpansion
title Claude Code Config Switcher

REM Jump to the script's folder (assumes repo root)
cd /d "%~dp0"

set "CONFIG_DIR=.claude"
set "TARGET=%CONFIG_DIR%\settings.local.json"
set "GLM_SRC=%CONFIG_DIR%\settings.local.GLM.json"
set "CLAUDE_SRC=%CONFIG_DIR%\settings.local.CLAUDE.json"

if not exist "%CONFIG_DIR%" (
  echo [ERROR] %CONFIG_DIR% directory not found. Make sure you're running this at your project root.
  exit /b 1
)

:menu
echo(
echo ================================
echo   Claude Code Config Switcher
echo ================================
echo   [1] GLM   (default)
echo   [2] Claude
echo   [Q] Quit
echo(
set "choice="
set /p "choice=Select option [1]: "

if "%choice%"=="" set "choice=1"

if /i "%choice%"=="1" (
  set "SRC=%GLM_SRC%"
  set "NAME=GLM"
  goto apply
) else if /i "%choice%"=="2" (
  set "SRC=%CLAUDE_SRC%"
  set "NAME=CLAUDE"
  goto apply
) else if /i "%choice%"=="q" (
  echo Bye!
  exit /b 0
) else (
  echo [WARN] Invalid selection: "%choice%". Try again.
  echo(
  goto menu
)

:apply
if not exist "%SRC%" (
  echo [ERROR] Source config not found: "%SRC%"
  echo         Make sure your template exists:
  echo           - %GLM_SRC%
  echo           - %CLAUDE_SRC%
  exit /b 1
)

REM Remove existing target (ignore if missing)
del /f /q "%TARGET%" >nul 2>&1

REM Copy selected config
copy /y "%SRC%" "%TARGET%" >nul
if errorlevel 1 (
  echo [ERROR] Failed to copy "%SRC%" to "%TARGET%".
  exit /b 1
)

echo [OK] Applied %NAME% config: "%SRC%"
echo.

REM Launch Claude Code CLI (must be in your PATH)
claude

endlocal
