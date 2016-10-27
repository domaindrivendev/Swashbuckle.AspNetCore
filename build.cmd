@echo off
cd %~dp0
@powershell -NoProfile -ExecutionPolicy unrestricted ./build.ps1

